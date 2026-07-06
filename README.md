# Travel Planner

Projekat iz predmeta **Primena veb programiranja u infrastrukturnim sistemima** — web aplikacija za
planiranje putovanja: planovi, destinacije, dnevne aktivnosti (sa prikazom kroz kalendar), troškovi i
budžet, beleške, podsetnici, checklist/packing lista, korisničke uloge i deljenje plana putem koda ili
QR koda (VIEW / EDIT pristup).

Dijagrami se nalaze u [`dijagrami/`](dijagrami/): `arhitektura.svg`, `use-case.svg`, `er-model.svg`
(izvor svakog je istoimeni `.mmd` fajl — Mermaid dijagram renderovan u SVG preko `mermaid-cli`).

## 1. Arhitektura sistema

Mikroservisna arhitektura na **Microsoft Service Fabric** (lokalni klaster), perzistencija u
**Microsoft SQL Server** — jedna baza po servisu (database-per-service), bez deljenja tabela.

![Arhitektura sistema](dijagrami/arhitektura.svg)

```mermaid
flowchart TB
    FE["Frontend — React SPA (Vite)"]
    GW["Gateway — stateless<br/>YARP reverse proxy · :8141"]
    AUTH["AuthService — stateless<br/>registracija, login, JWT, uloge · :8772"]
    TRIP["TripService — stateless<br/>planovi, destinacije, aktivnosti,<br/>troškovi, checklist, beleške, podsetnici · :8726"]
    SHARE["SharingService — stateful<br/>deljenje plana (kod/QR) · :8420"]
    UDB[("UsersDB")]
    TDB[("TripsDB")]
    SDB[("SharingDB")]
    RC[("Reliable Collections<br/>(IReliableDictionary — keš)")]

    FE -- "HTTP/REST + JWT" --> GW
    GW --> AUTH
    GW --> TRIP
    GW --> SHARE
    AUTH --> UDB
    TRIP --> TDB
    SHARE --> SDB
    SHARE --> RC
    AUTH -. "X-Internal-Key\nbriši planove obrisanog korisnika" .-> TRIP
    SHARE -. "X-Internal-Key\npovuci plan radi VIEW deljenja" .-> TRIP
```

- **Gateway** (stateless) — jedina ulazna tačka za frontend; YARP reverse proxy rutira zahteve ka
  ostalim servisima po putanji (vidi tabelu ruta ispod). Ne sadrži sopstvenu poslovnu logiku ni bazu.
- **AuthService** (stateless) — registracija, prijava, izdavanje i potpisivanje JWT tokena, uloge
  (Korisnik/Admin), administracija korisničkih naloga. Baza `UsersDB`.
- **TripService** (stateless) — centralni servis sa celokupnim sadržajem plana: destinacije,
  aktivnosti, troškovi/budžet, checklist, beleške, podsetnici. Baza `TripsDB`.
- **SharingService** (**stateful**) — deljenje plana putem koda/QR-a (VIEW/EDIT). `SharingDB` je izvor
  istine, dok se validni kodovi dodatno keširaju u **Reliable Collections** (`IReliableDictionary`) radi
  brže validacije bez stalnog odlaska u SQL — ovo je razlog zašto je baš ovaj servis stateful.

Svaki servis nezavisno validira potpis i istek JWT tokena (`ClockSkew = 0`). Server-server pozivi
(AuthService → TripService, SharingService → TripService) idu direktno (ne kroz Gateway) i osigurani su
deljenim tajnim ključem u `X-Internal-Key` header-u — ne JWT tokenom, jer ne postoji korisnik u tom
kontekstu.

### Rutiranje kroz Gateway (YARP)

| Putanja (prefiks) | Cilja servis | Port |
|---|---|---|
| `/api/auth/**` | AuthService | 8772 |
| `/api/users/**` | AuthService | 8772 |
| `/api/trips/{tripId}/shares**` | SharingService | 8420 |
| `/api/shares/**` | SharingService | 8420 |
| `/api/trips/**` (sve ostalo — planovi i sav sadržaj) | TripService | 8726 |

Frontend u radu gađa **isključivo Gateway** (port 8141, konfigurisano kroz `VITE_API_URL` u `.env`).

## 2. Use Case dijagram

![Use Case dijagram](dijagrami/use-case.svg)

```mermaid
flowchart LR
    Gost(("Gost"))
    Korisnik(("Korisnik"))
    Admin(("Admin"))

    Gost --> UC1["Registracija"]
    Gost --> UC2["Prijava"]
    Gost --> UC3["Pregled deljenog plana\n(kod / QR, VIEW)"]

    Korisnik --> UC2
    Korisnik --> UC4["Kreiranje / izmena / brisanje plana"]
    Korisnik --> UC5["Upravljanje destinacijama"]
    Korisnik --> UC6["Upravljanje aktivnostima\n(lista i kalendar prikaz)"]
    Korisnik --> UC7["Evidencija troškova i budžeta"]
    Korisnik --> UC8["Checklist / packing lista"]
    Korisnik --> UC9["Beleške"]
    Korisnik --> UC10["Podsetnici"]
    Korisnik --> UC11["Deljenje plana\n(generisanje VIEW/EDIT koda i QR)"]
    Korisnik --> UC12["Izvoz plana u PDF"]
    Korisnik --> UC13["Pristup tuđem planu\nkroz EDIT deljenje"]

    Admin --> UC2
    Admin --> UC4
    Admin --> UC14["Pregled svih korisnika"]
    Admin --> UC15["Brisanje korisničkog naloga"]
    Admin --> UC16["Pregled i administracija\nsvih planova u sistemu"]
```

- **Gost** — neautentikovan posetilac; može da se registruje, prijavi, ili otvori tuđi VIEW link/QR bez naloga.
- **Korisnik** — puno upravljanje sopstvenim planovima i svim njihovim sadržajem, plus pristup tuđem
  planu ako poseduje validan EDIT kod (dodatno ograničen na mejlove sa dozvoljene liste tog koda).
- **Admin** — sve što i Korisnik, plus pregled svih korisnika, brisanje naloga (kaskadno briše i sve
  njegove planove) i uvid u planove svih korisnika.

## 3. ER model (logička šema, database-per-service)

![ER model](dijagrami/er-model.svg)

```mermaid
erDiagram
    KORISNIK ||--o{ PLAN : "kreira (KorisnikId, cross-service)"
    PLAN ||--o{ DESTINACIJA : ima
    PLAN ||--o{ AKTIVNOST : ima
    PLAN ||--o{ TROSAK : ima
    PLAN ||--o{ CHECKLIST_STAVKA : ima
    PLAN ||--o{ BELESKA : ima
    PLAN ||--o{ PODSETNIK : ima
    PLAN ||--o{ SHARE : "deljen kroz (PlanId, cross-service)"

    KORISNIK {
        guid Id
        string Ime
        string Email
        string LozinkaHash
        string Uloga
        datetime DatumKreiranja
    }
    PLAN {
        guid Id
        guid KorisnikId
        string Naziv
        string Opis
        date PocetniDatum
        date KrajnjiDatum
        decimal PlaniraniBudzet
        string Napomene
    }
    DESTINACIJA {
        guid Id
        guid PlanId
        string Naziv
        string Lokacija
        date DatumDolaska
        date DatumOdlaska
    }
    AKTIVNOST {
        guid Id
        guid PlanId
        string Naziv
        date Datum
        string Vreme
        string Lokacija
        decimal ProcenjeniTrosak
        string Status
    }
    TROSAK {
        guid Id
        guid PlanId
        string Naziv
        string Kategorija
        decimal Iznos
        date Datum
    }
    CHECKLIST_STAVKA {
        guid Id
        guid PlanId
        string Naziv
        bool Zavrseno
    }
    BELESKA {
        guid Id
        guid PlanId
        string Naslov
        string Sadrzaj
        datetime DatumKreiranja
    }
    PODSETNIK {
        guid Id
        guid PlanId
        string Naziv
        date Datum
        string Opis
        bool Zavrseno
    }
    SHARE {
        guid Id
        string Kod
        guid PlanId
        guid KreatorId
        string Tip
        string DozvoljeniEmails
        datetime IstekDatum
        bool Opozvan
    }
```

`KORISNIK` (UsersDB), `PLAN`+ostalo (TripsDB) i `SHARE` (SharingDB) žive u **odvojenim bazama** —
`KorisnikId` i `PlanId` su logičke, ne fizičke strane veze (nema FK preko granice servisa); unutar
iste baze (TripsDB) sve veze od `Plan` ka ostalim tabelama su prave FK sa `ON DELETE CASCADE`.

## 4. Tehnologije

- **Frontend:** React 19 (Vite), Tailwind CSS v4, React Router v7, axios, Context API, AG Grid
  (community), qrcode.react, jsPDF.
- **Backend:** C# / .NET 8, Microsoft Service Fabric (stateless + stateful servisi), Entity Framework
  Core 8, YARP (Gateway reverse proxy), BCrypt.Net (heširanje lozinki), JWT Bearer autentikacija.
- **Baza podataka:** Microsoft SQL Server — tri odvojene baze (`UsersDB`, `TripsDB`, `SharingDB`).

## 5. Preduslovi

- Visual Studio 2022 sa instaliranim **Azure Service Fabric** alatima
- **Service Fabric SDK i Runtime**, podešen lokalni klaster (System tray → Service Fabric Local Cluster
  Manager → Setup/Start Local Cluster, 1 node)
- **.NET 8 SDK**
- **SQL Server** dostupan na `localhost` (podrazumevana instanca)
- **Node.js** 18+ i npm
- (opciono) `dotnet-ef` alat: `dotnet tool install --global dotnet-ef`

## 6. Pokretanje — Backend

1. Proveriti da lokalni Service Fabric klaster radi.

2. **Baze i migracije.** Connection string-ovi su u `appsettings.json` svakog servisa
   (`Server=localhost`). Za svaki servis primeniti migracije iz foldera servisa:
   ```bash
   cd backend/TravelPlanner/AuthService    && dotnet ef database update
   cd backend/TravelPlanner/TripService     && dotnet ef database update
   cd backend/TravelPlanner/SharingService  && dotnet ef database update
   ```

3. **Pristup baze za servisni nalog (obavezno).** Servisi na klasteru rade pod nalogom
   `NT AUTHORITY\NETWORK SERVICE` i konektuju se Windows autentikacijom. Kako baze kreira nalog kojim se
   pokreće migracija, servisnom nalogu treba dodeliti pristup — **za svaku bazu** (`UsersDB`, `TripsDB`,
   `SharingDB`):
   ```sql
   USE UsersDB;   -- pa TripsDB, pa SharingDB
   IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'NT AUTHORITY\NETWORK SERVICE')
       CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE];
   ALTER ROLE db_owner ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
   ```
   Bez ovoga prvi poziv ka bazi vraća `Login failed for user 'NT AUTHORITY\NETWORK SERVICE'`.

4. Otvoriti `backend/TravelPlanner/TravelPlanner.sln` u Visual Studio i pokrenuti **F5** — deployuje sva
   četiri servisa na lokalni klaster.

### Portovi servisa (lokalno)

| Servis | Port | Swagger |
|---|---|---|
| Gateway (ulazna tačka) | 8141 | — (samo proxy) |
| AuthService | 8772 | http://localhost:8772/swagger |
| TripService | 8726 | http://localhost:8726/swagger |
| SharingService | 8420 | http://localhost:8420/swagger |

## 7. Pokretanje — Frontend

```bash
cd frontend
cp .env.example .env      # VITE_API_URL -> Gateway (podrazumevano http://localhost:8141)
npm install
npm run dev
```

Aplikacija je dostupna na `http://localhost:5173`. Svi HTTP pozivi idu isključivo kroz injektovane
servise u `src/services/`, nikad direktno iz komponenti; adresa backend-a se čita iz `.env`.

## 8. Podrazumevani nalozi (seed podaci)

Kreirani kroz EF Core migracije prilikom prve primene na bazu:

| Uloga | Email | Lozinka |
|---|---|---|
| Admin | `admin@travelplanner.com` | `admin123` |
| Korisnik | `marko@primer.com` | `marko123` |
| Korisnik | `ana@primer.com` | `ana123` |

Marko i Ana već imaju po nekoliko unapred unetih planova sa destinacijama, aktivnostima, troškovima i
checklist stavkama, radi lakšeg testiranja bez ručnog unosa svih podataka.

## 9. Deljenje plana (VIEW / EDIT)

- **VIEW** — link/QR kod otvara plan u režimu pregleda, i bez prijave (`/share/{kod}`).
- **EDIT** — zahteva prijavljen nalog **čiji je mejl na listi dozvoljenih mejlova** definisanoj prilikom
  kreiranja deljenja; provera ide preko mejla iz JWT-a prijavljenog korisnika (nikad preko podatka koji
  bi klijent mogao sam da pošalje), tako da se pravo izmene ne može lažno preuzeti tuđim nalogom.
- Opoziv deljenja odmah onemogućava dalji pristup kroz taj kod.

## 10. Struktura projekta

```
backend/TravelPlanner/       # Service Fabric solution (TravelPlanner.sln)
  ├─ Gateway/                # YARP reverse proxy (ulazna tačka)
  ├─ AuthService/            # korisnici, JWT, uloge (UsersDB)
  ├─ TripService/            # planovi i sav sadržaj (TripsDB)
  └─ SharingService/         # deljenje, stateful (SharingDB)
frontend/                    # React + Vite aplikacija
```

## 11. Kriterijumi kvaliteta (kratak pregled ispunjenosti)

- SQL migracije: postoje za sva tri servisa sa bazom (Auth/Trip/Sharing).
- Frontend podeljen po komponentama (`components/`, `pages/`), sa sopstvenim modelima (`models/`).
- DTO i modeli baze su odvojeni, sa eksplicitnim mapiranjem (`Mapping/` po servisu).
- REST konvencija za nazivanje resursa (množina, ugnežđeni resursi po `tripId`).
- Lozinke heširane (BCrypt), potpis i istek JWT tokena validirani na svakom servisu.
- Validacija: krajnji datum ne može biti pre početnog, budžet/iznosi ne mogu biti negativni,
  datumi destinacija/aktivnosti moraju biti u okviru trajanja plana.
- Kaskadno brisanje: brisanjem plana brišu se sve povezane stavke (destinacije, aktivnosti, troškovi,
  checklist, beleške, podsetnici, deljenja); brisanjem korisnika brišu se i svi njegovi planovi.
- URL-ovi eksternih servisa (Gateway adresa) čitaju se iz `.env` na frontend strani.
- HTTP pozivi sa frontenda idu isključivo kroz injektovane servise (`src/services/`).
