# Testiranje — health checks i observability pipeline

Faza 6 diplomskog rada (tema 9). Ovaj dokument beleži stvaran plan i stvarne rezultate formalnog
testiranja, urađenog **posle** implementacije (Milestone 1-5) i demonstracionih scenarija
([`observability-scenarios.md`](observability-scenarios.md)), kao poslednji korak provere pre
pisanja same dokumentacije rada.

## Obim (namerno ograničen)

Ova faza testira **health checks** i **observability pipeline** — tačno ono što je diplomski
dodao. Ne dodaje testove za postojeću poslovnu logiku (planovi, destinacije, troškovi...), koja
nije imala testove pre ovog rada i nije predmet teme; širenje obima tamo bi bilo van onoga što je
zatraženo i nepotrebno bi uvećalo posao bez veze sa observabilnošću.

## Preduslov: refaktoring radi testabilnosti

Pre pisanja testova, DI registracija i middleware pipeline svakog servisa su izdvojeni iz anonimne
lambde unutar `CreateServiceInstanceListeners()` (Service Fabric listener factory) u javne statičke
metode `ConfigureServices(WebApplicationBuilder)` i `ConfigurePipeline(WebApplication)`:

| Servis | Nova klasa |
|---|---|
| Gateway | `GatewayApp` |
| AuthService | `AuthServiceApp` |
| TripService | `TripServiceApp` |
| SharingService | `SharingServiceApp` |

Service Fabric i dalje kreira `WebApplicationBuilder`/konfiguriše `WebHost` (treba mu `listener`/`url`
iz SF konteksta), ali samu registraciju servisa i middleware pipeline sada poziva iz ove zajedničke
metode — identično bez obzira da li servis hostuje SF ili test. **Nema divergencije** između "kako
radi u produkciji" i "kako se testira": testovi pozivaju iste dve metode, samo bez SF-a.

Refaktoring je čisto izdvajanje (bez promene ponašanja) — potvrđeno sa:
1. build sva 4 servisa posle refaktoringa (0 grešaka),
2. redeploy na SF klaster i provera da su sva 4 servisa i dalje `Healthy` na `/health/live` i
   `/health/ready` (identično kao pre refaktoringa).

## Test projekti i strategija

Četiri xUnit projekta (`*.Tests`), po jedan za svaki servis, referenciraju svoj servis direktno
(project reference) — bez `WebApplicationFactory` (SF hosting model nema klasičan `Program`
entry point za to), umesto toga svaki test podiže **pravi Kestrel host** na efemernom portu preko
`XxxApp.ConfigureServices/ConfigurePipeline`, sa in-memory konfiguracijom.

### 1. Integracioni testovi za health checks (`HealthEndpointsTests`)

Pravi HTTP pozivi ka pravom pokrenutom hostu. Za AuthService/TripService/SharingService koriste
**pravu lokalnu SQL Server bazu** (istu koju koristi i razvoj/demo, `UsersDB`/`TripsDB`/`SharingDB`)
— ne mock, ne in-memory provider, jer je upravo *stvarna* dostupnost baze ono što se testira.
Gateway nema bazu, pa njegovi testovi koriste `FakeDownstreamServer` (minimalan pravi HTTP server
pokrenut u test procesu) da simuliraju dostupan/nedostupan downstream servis.

Pokriveno za sva 4 servisa:
- `/health/live` vraća `Healthy` sa praznim `checks` (ne dodiruje zavisnosti).
- `/health/ready` vraća `Healthy` + 200 kada je zavisnost (baza/downstream) dostupna.
- `/health/ready` vraća `Unhealthy` + **503** kada zavisnost nije dostupna, sa čitljivom porukom
  greške u JSON odgovoru.

Gateway dodatno testira da pad **jednog** downstream servisa ne obara status ostala dva
(`auth-service`/`sharing-service` ostaju `Healthy` dok je `trip-service` `Unhealthy`).

### 2. Observability pipeline testovi (`ObservabilityPipelineTests`)

Testira da NAŠ kod ispravno registruje i aktivira OTel instrumentaciju — koristi ugrađeni
`System.Diagnostics.ActivityListener` (bez OTel-specifičnih test paketa, bez Docker zavisnosti):
zahtev ka `/health/live` (i `/health/ready` za TripService, gde se očekuje i dodatni SQL child
span) mora proizvesti `Activity` čiji `DisplayName` (nakon `ActivityStopped`, kad OTel obogati ime
rutom) sadrži putanju zahteva.

**Namerno razdvojeno** od stvarne isporuke do Collector-a/Tempo-a/Loki-ja/Prometheus-a — to je već
rigorozno provereno na pravom deployovanom sistemu u
[`observability-scenarios.md`](observability-scenarios.md) (stvarni trace ID-jevi, log redovi,
izmerene vrednosti). Dupliranje te provere ovde bi test suite učinilo zavisnim od toga da Docker
stack bude pokrenut da bi `dotnet test` prošao, što nije poželjno.

**Usputan, stvaran nalaz zabeležen u testu:** `Activity.DisplayName` u trenutku `ActivityStarted` JE
sirovo ime izvora (npr. `Microsoft.AspNetCore.Hosting.HttpRequestIn`), NE obogaćeno ime rute
(`GET /health/live`) koje se vidi u Tempo-u — obogaćivanje se dešava tek pri `ActivityStopped`, kad
je ruta poznata. Prvi pokušaj testa (hook na `ActivityStarted`) je zato pao; ispravka (hook na
`ActivityStopped`) je i ispravnija i tačnije odražava šta se stvarno vidi u tracing backend-u.

### 3. Unit test za popravljen "tihi" catch blok (`TripClientTests`, AuthService.Tests)

Regresioni test za AS-IS nalaz iz analize: `TripClient.DeleteUserTripsAsync` je ranije gutao greške
bez ijednog loga. Sa lažnim `HttpMessageHandler`-om (uspeh / 500 / baca `HttpRequestException`) i
lažnim `ILogger<TripClient>` koji hvata pozive, tri testa dokazuju:
- uspešan poziv → `true`, nijedan log,
- neuspešan HTTP status → `false` + `LogWarning`,
- mrežni kvar (izuzetak) → `false` + `LogError`.

`TripClient` je `public class`, pa je testiran direktno bez potrebe za `InternalsVisibleTo`.
`HealthCheckResponseWriter` i `DownstreamHealthCheck` su namerno ostali `internal` i testirani su
**indirektno** kroz integracione testove (stvaran HTTP odgovor kroz ceo pipeline) umesto direktnim
unit testom uz `InternalsVisibleTo` — smisleniji test (proverava stvarno ponašanje, ne interni
detalj implementacije) uz manje dodatne mehanike.

## Rezultati (stvarno izvršeno, `dotnet test`)

| Projekat | Passed | Failed | Total | Trajanje |
|---|---|---|---|---|
| TripService.Tests | 6 | 0 | 6 | ~8s |
| AuthService.Tests | 7 | 0 | 7 | ~8s |
| SharingService.Tests | 4 | 0 | 4 | ~5s |
| Gateway.Tests | 4 | 0 | 4 | ~5s |
| **Ukupno** | **21** | **0** | **21** | — |

Svi testovi prolaze. Test projekti su dodati u `TravelPlanner.sln` (`dotnet sln add`), pa se vide i
grade zajedno sa ostatkom rešenja u Visual Studio-u.

## Šta ova faza namerno NE pokriva (i zašto)

- **Poslovna logika** (planovi/destinacije/troškovi/deljenje) — nije menjana ovim radom, izvan teme.
- **Docker/Collector/Tempo/Loki/Prometheus end-to-end** kao automatizovan test — namerno
  demonstrirano kroz stvaran sistem u `observability-scenarios.md`, ne dupliran kao `dotnet test`
  koji bi zahtevao da Docker bude pokrenut da bi prošao (loša praksa za test suite koji treba da
  bude pouzdano ponovljiv).
- **k6 load test** kao automatizovan `dotnet test` — ostaje poseban alat/skripta
  (`observability/load-tests/scenario3-load-test.js`), pokreće se ručno/po potrebi, ne kao deo
  redovnog test suite-a (drugačija svrha - opterećenje, ne korektnost).
