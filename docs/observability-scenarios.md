# Demonstracioni scenariji observabilnosti

Ovaj dokument beleži rezultate stvarnog izvršavanja demonstracionih scenarija nad realno
deployovanim sistemom (Service Fabric lokalni klaster, sva 4 servisa, prava `TripsDB`/`UsersDB`/
`SharingDB` baza, observability stack iz `docker-compose.observability.yml`). Nijedan nalaz ovde
nije simuliran niti izmišljen — svaki trace ID, log red i vremenska oznaka su iz stvarnog izvršavanja
zabeleženog tokom rada na diplomskom (tema 9: Implementacija observabilnosti mikroservisne
arhitekture).

## Scenario 1 — Normalan zahtev kroz više mikroservisa

**Tok:** Prijava korisnika `marko@primer.com`, zatim kreiranje VIEW deljenja za plan
`Letovanje u Grčkoj`, pa razrešavanje tog koda kao anoniman posetilac (javni `/api/shares/{code}`
endpoint). Poslednji korak namerno prelazi tri procesa: Gateway → SharingService → TripService.

**Trace:** `95fa368e7049230dde2205c190443eb` (Tempo), `durationMs≈2395`

```
gateway          GET /api/shares/{**catch-all}    (server, root)
  gateway        GET                               (client, proxy ka SharingService)
    sharing-service  GET api/shares/{code}          (server)
      sharing-service  GET                           (client, ka TripService internal API)
        trip-service   GET api/internal/trips/{id}/full  (server)
          trip-service   SELECT [Plans]              (9 SQL child span-ova ukupno:
          trip-service   SELECT [Troskovi]            Plans, Troskovi x2, Aktivnosti x2,
          trip-service   SELECT [Aktivnosti]          Destinacije, ChecklistStavke,
          trip-service   SELECT [Destinacije]         Beleske, Podsetnici)
          trip-service   SELECT [ChecklistStavke]
          trip-service   SELECT [Beleske]
          trip-service   SELECT [Podsetnici]
```

Svaki SQL upit u `InternalController.GetFullPlan` dobio je svoj child span, ispravno ugnježden pod
HTTP server span-om koji ga je izazvao - **automatski**, bez ijedne linije custom koda za
propagaciju konteksta (isti mehanizam koji povezuje i sve ostale servis-servis pozive u sistemu).

**Logovi (Loki), isti trace, sve tri usluge korelisane preko `trace_id`:**

```
gateway          | Proxying to http://localhost:8420/api/shares/tOW5zKydJF
sharing-service  | Start processing HTTP request GET .../api/internal/trips/.../full
trip-service     | Executed DbCommand (65ms)   SELECT [Podsetnici] ...
trip-service     | Executed DbCommand (1207ms) SELECT [Beleske] ...
trip-service     | Executed DbCommand (162ms)  SELECT COALESCE(SUM(...)) FROM [Aktivnosti]
trip-service     | Executed DbCommand (254ms)  SELECT COALESCE(SUM(...)) FROM [Troskovi]
gateway          | Received HTTP/1.1 response 200.
```

EF Core-ove sopstvene "Executed DbCommand" log poruke (sa tačnim trajanjem po upitu) prolaze kroz
OTel logging provider bez ikakve dodatne konfiguracije - odmah korisno za identifikaciju sporih
upita (vidi Scenario 4).

**Metrike (Prometheus), stvarni brojevi zahteva po servisu u trenutku testa:**
`trip-service=7, gateway=7, sharing-service=6, auth-service=5` (`http_server_request_duration_seconds_count`).

**Zaključak:** logovi, trace i metrike se mogu međusobno korelisati kroz jedan `trace_id` bez ijednog
reda custom koda za correlation ID - OpenTelemetry SDK to radi automatski na osnovu W3C trace
konteksta koji se prenosi kroz `HttpClient`/`ASP.NET Core` instrumentaciju.

## Scenario 2 — Greška (pad zavisnosti)

**Kako je izazvana:** `Restart-ServiceFabricDeployedCodePackage` nad TripService-om (zvaničan Service
Fabric mehanizam za restart koda, ne nasilno gašenje procesa sa OS nivoa - ono je pokušano prvo, ali
odbijeno jer proces radi pod drugim Windows nalogom nego interaktivna sesija).

**Vremenska linija (stvarno merena, ne procenjena):**

| Vreme | `trip-service /health/live` | `gateway /health/ready` |
|---|---|---|
| 23:28:36 | `000` (konekcija odbijena) | greška (Unhealthy) |
| 23:28:43 | `200` | `Healthy` |

Sistem se sam oporavio za **~7 sekundi** bez ručne intervencije (Service Fabric je automatski
ponovo pokrenuo instancu).

**Log (Loki, `Error` nivo, automatski iz ugrađenog health check frameworka, bez custom koda):**

```
Error | Health check trip-service with status Unhealthy completed after 2008.4346ms
        with message 'http://localhost:8726 nije dostupan.'
```

Poruka `'{url} nije dostupan.'` je tačno string iz `Gateway/Health/DownstreamHealthCheck.cs` -
dokaz da je health check infrastruktura iz Milestone-a 1 stvarno ono što je detektovalo i
zabeležilo pad.

**Zapažen nedostatak (za poglavlje "Ograničenja" u dokumentaciji):** `/health/live` i
`/health/ready` pozivi prolaze kroz isto ASP.NET Core instrumentation kao i sav ostali saobraćaj,
pa se njihovi 503 odgovori mešaju u isti `http_server_request_duration_seconds` metrika kao i
stvarni korisnički zahtevi. Za produkcioni dashboard bi trebalo eksplicitno isključiti `/health/*`
rute iz panela za error rate (npr. `http_route!~"/health.*"` u PromQL) - nije urađeno u ovoj fazi
jer nije uticalo na demonstraciju, ali je zabeleženo kao poznato ograničenje.

## Scenario 3 — Povećano opterećenje (k6)

**Test:** `k6` (v0.55.1), realan tok po virtuelnom korisniku (login pa `GET /api/trips`) kroz Gateway,
naizmenično kao Marko i Ana (postojeći seed nalozi). Ramp 1→5→30 virtuelnih korisnika kroz 60s.

**Rezultat (k6, klijentska strana):**

| Metrika | Vrednost |
|---|---|
| Ukupno zahteva | 780 (390 iteracija × 2 poziva) |
| Uspešnost | 100.00% (0 od 780 neuspešnih) |
| Throughput | 12.86 req/s (na vrhuncu 30 VU) |
| Latency avg / p90 / p95 / max | 787ms / 1.66s / **2.12s** / 5.55s |

**Isto opterećenje, izmereno nezavisno kroz OTel/Prometheus (server strana, ne k6):**

| Metrika | Vrednost |
|---|---|
| `exported_job=gateway` request count, pre → posle testa | 58 → 434 |
| p95 latency (`histogram_quantile(0.95, ...)`, Gateway) | **2.14s** |
| Request rate (`rate(...)[1m]`) | 6.61 req/s (mereno posle ramp-down faze) |

**Zaključak:** p95 izmeren nezavisno na klijentu (k6, 2.12s) i na serveru (Prometheus histogram iz
OTel metrike, 2.14s) se poklapaju u okviru merne greške - potvrda da OTel metrike tačno odražavaju
stvarno ponašanje sistema pod opterećenjem, ne samo da "nešto broje". Sistem je ostao stabilan
(0% grešaka) pri 30 paralelnih korisnika na lokalnom 1-node klasteru; p95 od ~2.1s pri toj
konkurentnosti je merljiva osnova (baseline) za buduće poređenje posle eventualne optimizacije
(npr. connection pooling, keširanje, skaliranje instanci).

## Scenario 4 — Spora zavisnost (usko grlo)

**Kako je izazvana:** direktna SQL transakcija je otvorila X (exclusive) lock nad tačno jednim
redom u `Plans` (`UPDATE Plans SET Napomene = Napomene WHERE Id = ...`, bez commit-a, držano 25s),
dok je pravi HTTP zahtev (`PUT /api/trips/{id}` kroz Gateway) istovremeno pokušao da izmeni isti red.

**Usputan, stvaran nalaz:** `TripsDB` ima uključen `READ_COMMITTED_SNAPSHOT` (RCSI). Prva dva
pokušaja (obično `GET` čitanje, pa `PUT` sa identičnim vrednostima) **nisu** blokirana - prvo zato
što RCSI čitanje ne zahteva lock (čita poslednju commit-ovanu verziju reda), drugo zato što je
Entity Framework Core change tracker detektovao da poslati podaci nisu različiti od postojećih i
**nije uopšte poslao UPDATE komandu ka bazi**. Provereno direktno preko
`sys.dm_tran_locks` da je lock zaista dodeljen (KEY, mode=X, GRANT) pre nego što je zaključeno da je
uzrok negde drugde. Tek `PUT` sa stvarno izmenjenom vrednošću (writer-vs-writer, na koji RCSI ne
utiče) je izazvao pravo čekanje.

**Izmereno:** `PUT` zahtev trajao **23.9s** (curl: `23.905735s`), tačno onoliko koliko je lock bio
držan (25s hold minus ~1s koliko je trebalo da se zahtev pošalje posle signala da je lock preuzet).

**Trace** `48b4f1cae70f5b7ec0cdc797f1f2a5d3`, `durationMs=23897`:

```
gateway          PUT /api/trips/{**catch-all}     23903.4ms   <<<< usko grlo vidljivo na ivici
  gateway        PUT                               23902.2ms
    trip-service PUT api/trips/{id}                23897.2ms   <<<< usko grlo vidljivo u servisu
      trip-service SELECT [Plans]                     14.5ms   (brzo - RCSI, bez čekanja)
      trip-service UPDATE                          23871.1ms   <<<< TAČAN uzrok, identifikovan
```

Distribuirano praćenje ovde daje nešto što logovi sami po sebi ne bi dali lako: **tačnu SQL
komandu** i **tačno trajanje** unutar dužeg zahteva, razlikujući "brz SELECT" od "spor UPDATE" u
istom zahtevu - ne samo da je zahtev bio spor, već i gde tačno.

---

*Napomena: seed podaci (`Napomene` polje testiranog plana) su posle testa vraćeni na originalnu
vrednost; manja kozmetička razlika u dijakritiku (č → c) je ostala usled enkodiranja kroz
Bash/curl u ovoj razvojnoj sesiji, bez uticaja na funkcionalnost.*
