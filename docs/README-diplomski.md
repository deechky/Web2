# Diplomski rad — dokumentacija

`diplomski-rad.docx` je generisan direktno iz zvaničnog fakultetskog šablona
(`template_diplomski.docx`, Завршни рад, ментор др Себастијан Стоја) preko
`build_thesis.py` — skripta otvara šablon (python-docx), zadržava sve njegove stilove/fontove/
naslovne formate bez izmene, i ubacuje stvaran sadržaj poglavlja 1-7 + Literatura + Biografija,
zasnovan isključivo na onome što je stvarno implementirano, testirano i izmereno u ovoj sesiji
(README.md, docs/observability-scenarios.md, docs/testing.md) — ništa izmišljeno.

## Šta MORAŠ sama popuniti pre predaje

Ovo su isključivo lični/administrativni podaci koje ja nisam mogla znati niti smela pretpostaviti:

- **Ime i prezime** — na naslovnoj strani, u obe bibliografske kartice (srpska/engleska) i u tabeli
  zadatka za završni rad. Traži `[ПОПУНИ: Име и презиме]` (Find & Replace u Word-u je najbrži put).
- **Broj indeksa** — u tabeli zadatka za završni rad (`[ПОПУНИ: број индекса]`).
- **Broj strana / broj citata** — u obe bibliografske kartice (`[ПОПУНИ: број страна]`,
  `[ПОПУНИ: број цитата]`) — vidljivo tek kad dokument ima konačnu paginaciju u Word-u.
- **Biografija** (poslednje poglavlje) — godina/mesto rođenja, srednja škola, godina upisa na
  fakultet — ostavljeno u originalnom XXXX/XXX obliku iz šablona, potpuno lični podaci.
- **Datumi** (prihvatanje teme, odbrana) i **potpisi** — u tabelama, popunjava fakultet/mentor.
- Naslov rada je predložen kao **„Имплементација опсервабилности микросервисне архитектуре”** — ako
  mentor traži drugačiju formulaciju, izmeni na sva tri mesta gde se pojavljuje (naslovna strana,
  obe bibliografske kartice, tabela zadatka) ili ponovo pokreni skriptu sa izmenjenom `TITLE`
  promenljivom.

## Šta MORAŠ uraditi u Word-u posle otvaranja

1. **Ažurirati sadržaj (TOC)** — desni klik na polje gde je sadržaj → *Update Field* → *Update
   entire table* (isto uputstvo kao u `sablon za pisanje dokumentacije.pdf`, sekcija o
   formatiranju sadržaja). Bez ovoga brojevi strana u sadržaju su i dalje iz praznog šablona.
2. Proveriti prelome strana / da li slike upadaju lepo u stranicu (Word-ov automatski layout se
   može razlikovati od onoga što python-docx vidi).

## Ako treba nešto izmeniti u sadržaju poglavlja

Najlakše je izmeniti tekst direktno u `build_thesis.py` (svaki pasus je čitljiv Python string) i
ponovo pokrenuti:

```bash
python3 docs/build_thesis.py
```

Skripta uvek kreće iznova od `template_diplomski.docx` (ne od prethodnog `diplomski-rad.docx`),
pa je bezbedno pokretati je više puta — nikad ne akumulira izmene niti duplira sadržaj.

## Šta pokriva svako poglavlje

| Poglavlje | Izvor sadržaja |
|---|---|
| 1. Uvod | Kontekst projekta, motivacija, struktura rada |
| 2. Opis problema | AS-IS problemi (odsustvo logovanja/metrika/tracinga/health checks) |
| 3. Opis korišćenih tehnologija i alata | Postojeći stack + observability stack, sa obrazloženjem izbora |
| 4. Opis početnog rešenja | AS-IS arhitektura (dijagram `dijagrami/arhitektura-original.png`) |
| 5. Opis rešenja problema | TO-BE arhitektura, health checks, OTel instrumentacija, Grafana, 4 demonstraciona scenarija (docs/observability-scenarios.md), AS-IS/TO-BE tabela |
| 6. Testiranje i rezultati | docs/testing.md — 21/21 testova, tabela rezultata |
| 7. Zaključak | Ograničenja (stvarno pronađena tokom rada, ne izmišljena) i pravci daljeg razvoja |
| Literatura | 10 izvora, IEEE stil, sve zvanična dokumentacija korišćenih alata |
