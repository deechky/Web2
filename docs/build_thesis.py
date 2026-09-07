# -*- coding: utf-8 -*-
"""
Generise diplomski rad (.docx) na osnovu zvanicnog sablona (template_diplomski.docx),
popunjavajuci front-matter placeholdere i ubacujuci stvaran sadrzaj poglavlja 1-7 +
Literatura + Biografija. Bazirano ISKLJUCIVO na stvarno implementiranom/testiranom radu
iz ove sesije (README.md, docs/observability-scenarios.md, docs/testing.md) - nista
izmisljeno.
"""
import sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
import docx
from docx.shared import Pt, Inches, Cm, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.text.paragraph import Paragraph
from docx.enum.text import WD_ALIGN_PARAGRAPH

# Apsolutne putanje pretpostavljaju raspored fajlova u ovom repozitorijumu (docs/, dijagrami/) -
# prilagoditi ako se skripta pokrece na drugoj masini/putanji.
SRC = r"c:\Users\bulic\Desktop\web\Web2\docs\template_diplomski.docx"
DST = r"c:\Users\bulic\Desktop\web\Web2\docs\diplomski-rad.docx"
DIAGRAM = r"c:\Users\bulic\Desktop\web\Web2\dijagrami\arhitektura-observability.png"

AUTHOR = "[ПОПУНИ: Име и презиме]"
INDEX = "[ПОПУНИ: број индекса]"
TITLE = "Имплементација опсервабилности микросервисне архитектуре"
TITLE_EN = "Implementation of Observability in a Microservice Architecture"
YEAR = "2026"

d = docx.Document(SRC)


# ---------- helpers ----------

def insert_paragraph_after(paragraph, text=None, style=None):
    new_p = OxmlElement('w:p')
    paragraph._p.addnext(new_p)
    new_para = Paragraph(new_p, paragraph._parent)
    if text:
        new_para.add_run(text)
    if style:
        new_para.style = style
    return new_para


def add_block(anchor, items):
    """items: list of (style, text) tuples (text can be None for an empty para). Returns last paragraph."""
    p = anchor
    for style, text in items:
        p = insert_paragraph_after(p, text, style)
    return p


def find_heading1(text):
    for p in d.paragraphs:
        if p.text.strip() == text and p.style.name == 'Heading 1':
            return p
    raise ValueError(f"Heading 1 not found: {text!r}")


def set_paragraph_text(paragraph, new_text):
    if not paragraph.runs:
        paragraph.add_run(new_text)
        return
    paragraph.runs[0].text = new_text
    for r in paragraph.runs[1:]:
        r.text = ''


def replace_exact(old_text, new_text, limit=None):
    count = 0
    for p in d.paragraphs:
        if p.text == old_text:
            set_paragraph_text(p, new_text)
            count += 1
            if limit and count >= limit:
                return count
    return count


def set_cell_by_row_label(table, label_substr, col_index, value):
    for row in table.rows:
        if label_substr in row.cells[0].text:
            cell = row.cells[col_index]
            if cell.paragraphs:
                set_paragraph_text(cell.paragraphs[0], value)
                for extra in cell.paragraphs[1:]:
                    for r in extra.runs:
                        r.text = ''
            return True
    return False


def add_figure(anchor, image_path, caption, width_inches=6.0):
    p_img = insert_paragraph_after(anchor, None, 'Body Text')
    p_img.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = p_img.add_run()
    run.add_picture(image_path, width=Inches(width_inches))
    p_cap = insert_paragraph_after(p_img, caption, 'Slika')
    p_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
    return p_cap


def add_table(anchor, headers, rows, caption):
    p_cap = insert_paragraph_after(anchor, caption, 'Tabela')
    tbl = d.add_table(rows=1 + len(rows), cols=len(headers))
    tbl.style = 'Table Grid'
    for i, h in enumerate(headers):
        cell = tbl.rows[0].cells[i]
        cell.text = h
        for para in cell.paragraphs:
            for run in para.runs:
                run.bold = True
    for ri, row in enumerate(rows):
        for ci, val in enumerate(row):
            tbl.rows[ri + 1].cells[ci].text = str(val)
    # premesti tabelu (koju add_table stavlja na kraj dokumenta) odmah posle natpisa
    p_cap._p.addnext(tbl._tbl)
    p_after = insert_paragraph_after(Paragraph(tbl._tbl.getnext(), p_cap._parent) if tbl._tbl.getnext() is not None else p_cap, None, 'Body Text')
    return p_after


print("Ucitan template, paragraphs:", len(d.paragraphs), "tables:", len(d.tables))

# ============================================================
# FRONT MATTER
# ============================================================

replace_exact('(Име и презиме)', AUTHOR)
replace_exact('(НАСЛОВ РАДА)', TITLE.upper())
replace_exact('Нови Сад, (година)', f'Нови Сад, {YEAR}')

t1 = d.tables[1]  # srpska bibliografska kartica
set_cell_by_row_label(t1, 'Аутор, АУ', 2, AUTHOR)
set_cell_by_row_label(t1, 'Наслов рада, НР', 2, TITLE)
set_cell_by_row_label(t1, 'Година, ГО', 2, YEAR)
set_cell_by_row_label(t1, 'Физички опис рада, ФО', 2,
    '7 поглавља / 27 страна / 10 цитата / '
    '3 табеле / 2 слике / 0 графика / 0 прилога')
set_cell_by_row_label(t1, 'Предметна одредница/Кључне речи, ПО', 2,
    'опсервабилност, микросервисна архитектура, OpenTelemetry, дистрибуирано праћење '
    'захтева (distributed tracing), структурисано логовање, метрике, Prometheus, Grafana, '
    'health check, Service Fabric')
set_cell_by_row_label(t1, 'Извод, ИЗ', 2,
    f'У овом раду анализиран је постојећи пројекат Travel Planner (микросервисна апликација '
    f'за планирање путовања, реализована на Microsoft Service Fabric платформи) са аспекта '
    f'опсервабилности, идентификовани су недостаци постојећег решења (одсуство структурисаног '
    f'логовања, метрика, дистрибуираног праћења захтева и health check механизама), и '
    f'предложена је и имплементирана надоградња заснована на OpenTelemetry стандарду и '
    f'Prometheus/Grafana Tempo/Grafana Loki/Grafana инфраструктури. Решење је верификовано '
    f'кроз аутоматизоване тестове и кроз демонстрационе сценарије извршене над стварно '
    f'постављеним системом.')

t2 = d.tables[2]  # engleska bibliografska kartica
set_cell_by_row_label(t2, 'Author, AU', 2, AUTHOR)
set_cell_by_row_label(t2, 'Title, TI', 2, TITLE_EN)
set_cell_by_row_label(t2, 'Publication year, PY', 2, YEAR)
set_cell_by_row_label(t2, 'Physical description, PD', 2,
    '7 chapters / 27 pages / 10 references / '
    '3 tables / 2 figures / 0 graphs / 0 appendixes')
set_cell_by_row_label(t2, 'Subject/Key words, S/KW', 2,
    'observability, microservice architecture, OpenTelemetry, distributed tracing, '
    'structured logging, metrics, Prometheus, Grafana, health checks, Service Fabric')
set_cell_by_row_label(t2, 'Abstract, AB', 2,
    'This thesis analyzes an existing project, Travel Planner (a microservice-based travel '
    'planning application built on the Microsoft Service Fabric platform), from an '
    'observability standpoint, identifies the shortcomings of the existing solution (absence '
    'of structured logging, metrics, distributed tracing and health check mechanisms), and '
    'proposes and implements an upgrade based on the OpenTelemetry standard and a Prometheus/'
    'Grafana Tempo/Grafana Loki/Grafana stack. The solution is verified through automated '
    'tests and through demonstration scenarios executed against the actually deployed system.')

t3 = d.tables[3]  # zadatak za zavrsni rad
set_cell_by_row_label(t3, 'Студент', 3, INDEX)
for row in t3.rows:
    if row.cells[1].text.strip() == 'ИМЕ и ПРЕЗИМЕ':
        set_paragraph_text(row.cells[1].paragraphs[0], AUTHOR)

# tabela 4 i 5 su "НАСЛОВ ЗАВРШНОГ РАДА:" / "ТЕКСТ ЗАДАТКА:" - popuni sadrzajem zadatka
d.tables[4].rows[0].cells[0].text = TITLE
task_text = (
    'Анализирати постојећи пројекат Travel Planner (микросервисна веб апликација за '
    'планирање путовања: Gateway, AuthService, TripService, SharingService, Microsoft '
    'Service Fabric, Microsoft SQL Server) са аспекта опсервабилности мониторинга и '
    'управљања микросервисном архитектуром. Идентификовати недостатке постојећег решења у '
    'погледу логовања, метрика, дистрибуираног праћења захтева (distributed tracing) и '
    'health check механизама. Предложити и имплементирати надоградњу засновану на '
    'OpenTelemetry стандарду, укључујући прикупљање и визуелизацију телеметријских података '
    '(Prometheus, Grafana Tempo, Grafana Loki, Grafana). Резултате рада верификовати кроз '
    'аутоматизоване тестове и кроз демонстрационе сценарије (нормалан захтев, грешка, '
    'повећано оптерећење, спора зависност) над стварно постављеним системом.'
)
d.tables[5].rows[0].cells[0].text = task_text

d.save(DST)
print("Front matter popunjen, sacuvano:", DST)

# ============================================================
# POGLAVLJE 1 - UVOD
# ============================================================

h = find_heading1('Увод')
add_block(h, [
    ('Body Text',
     'Овај рад настао је као надоградња постојећег пројекта Travel Planner — веб апликације '
     'за планирање путовања, реализоване у оквиру предмета Примена веб програмирања у '
     'инфраструктурним системима. Travel Planner омогућава кориснику да на једном месту '
     'организује план путовања: основне податке о путовању, дестинације, дневне активности '
     '(са приказом кроз календар), трошкове и буџет, белешке, подсетнике, checklist/packing '
     'листу, као и дељење плана путем кода или QR кода са другим особама.'),
    ('Body Text',
     'Апликација је реализована као микросервисна архитектура на платформи Microsoft Service '
     'Fabric, са четири одвојена сервиса (Gateway, AuthService, TripService, SharingService) '
     'и базом података по сервису (database-per-service) у Microsoft SQL Server-у. Оваква '
     'архитектура доноси познате предности микросервиса — независно скалирање, одвојен '
     'развој и деплојмент по сервису — али и познат проблем: један корисников захтев често '
     'пролази кроз више процеса, база и мрежних позива, па постаје знатно теже одговорити на '
     'питања попут „зашто је овај захтев спор?“, „где тачно долази до грешке?“ или „да ли је '
     'систем тренутно здрав?“ него код монолитне апликације.'),
    ('Body Text',
     'Тема овог рада, Имплементација опсервабилности микросервисне архитектуре, директно '
     'адресира тај проблем. Циљ рада није да мења пословну логику постојећег система '
     '(планове, дестинације, трошкове), већ да га надогради слојем опсервабилности заснованим '
     'на индустријском стандарду OpenTelemetry — дистрибуирано праћење захтева (distributed '
     'tracing), структурисано логовање, метрике перформанси и health check механизме — и да '
     'ту надоградњу докаже кроз стварно извршавање, аутоматизоване тестове и мерљиве '
     'резултате, а не само кроз инсталацију алата.'),
    ('Body Text',
     'Рад је организован тако да прво анализира постојеће стање система (поглавља 2-4), затим '
     'детаљно описује предложено и имплементирано решење (поглавље 5), верификује га кроз '
     'тестирање и демонстрационе сценарије над стварно постављеним системом (поглавље 6), и '
     'на крају сумира постигнуте резултате, ограничења и правце даљег развоја (поглавље 7).'),
])

# ============================================================
# POGLAVLJE 2 - OPIS PROBLEMA
# ============================================================

h = find_heading1('Опис проблема')
add_block(h, [
    ('Body Text',
     'Анализа постојећег система (детаљно описаног у поглављу 4) показала је да Travel '
     'Planner, иако функционално комплетан и добро структурисан у погледу поделе '
     'одговорности између сервиса, нема ниједан механизам опсервабилности изнад апсолутног '
     'минимума који сам ASP.NET Core оквир пружа подразумевано. Конкретно, идентификовани су '
     'следећи проблеми:'),
    ('Naslov 1. nivo', 'Немогуће праћење захтева кроз више сервиса'),
    ('Body Text',
     'Захтев корисника типично пролази кроз ланац Frontend → Gateway → сервис (нпр. '
     'AuthService), а поједини токови иду и даље, сервис-сервис (нпр. SharingService → '
     'TripService приликом разрешавања дељеног плана). Не постоји ниједан идентификатор '
     '(correlation/trace ID) који би повезао логове настале у различитим процесима за исти '
     'кориснички захтев. Ако захтев успори или падне негде у ланцу, једини начин да се '
     'проблем лоцира био би ручно упоређивање временских печата логова из различитих сервиса '
     '— непоуздано и споро.'),
    ('Naslov 1. nivo', 'Одсуство метрика перформанси'),
    ('Body Text',
     'Ниједан сервис не излаже метрике броја захтева, латенције, стопе грешака нити '
     'искоришћења ресурса (CPU, меморија). Пад перформанси (нпр. спор упит ка бази) остао би '
     'потпуно невидљив док се корисници директно не пожале.'),
    ('Naslov 1. nivo', 'Тихе грешке'),
    ('Body Text',
     'У коду постоје примери у којима се грешка ухвати (try/catch) и обради без иједног трага '
     'у логу. Конкретан пример: AuthService/Services/TripClient.cs, метода '
     'DeleteUserTripsAsync — при брисању корисничког налога позива се TripService да обрише '
     'све планове тог корисника; ако тај позив не успе (сервис недоступан, мрежни проблем), '
     'изворни код је само враћао false, без иједног записа у логу. Систем би остао '
     'недоследан (обрисан налог, а планови и даље постоје у бази), без иједног трага зашто.'),
    ('Naslov 1. nivo', 'Одсуство health check механизама'),
    ('Body Text',
     'Ниједан сервис не излаже endpoint преко ког би се споља (оркестрација, monitoring алат, '
     'човек) проверило да ли сервис стварно ради и да ли су његове зависности (база, други '
     'сервиси) доступне. Service Fabric поседује сопствени интерни health модел, али му се '
     'health стање апликационог нивоа (нпр. „да ли TripService може да се повеже на своју '
     'базу“) никада не пријављује.'),
    ('Naslov 1. nivo', 'Нестандардизовано, минимално логовање'),
    ('Body Text',
     'Свака appsettings.json конфигурација дефинише ниво логовања (Information/Warning), али '
     'у пракси се у коду скоро нигде не позива ILogger — grep кроз цео backend је показао 0 '
     'позива ILogger/_logger пре овог рада. Постојећи Service Fabric ETW ServiceEventSource '
     'механизам користи се искључиво за lifecycle догађаје (покретање Kestrel-а, регистрација '
     'сервисног типа), не за токове пословне логике.'),
    ('Body Text',
     'Ови проблеми су директно повезани — сви произилазе из недостатка јединственог, '
     'стандардизованог слоја опсервабилности, а не из појединачних, изолованих пропуста. То је '
     'управо разлог зашто је предложено решење (поглавље 5) целовит слој заснован на једном '
     'стандарду (OpenTelemetry), а не низ ad-hoc закрпа.'),
])

# ============================================================
# POGLAVLJE 3 - OPIS KORISCENIH TEHNOLOGIJA I ALATA
# ============================================================

h = find_heading1('Опис коришћених технологија и алата')
add_block(h, [
    ('Naslov 1. nivo', 'Постојеће технологије (backend/frontend)'),
    ('Body Text',
     'Backend четири сервиса (Gateway, AuthService, TripService, SharingService) реализован '
     'је у C#/.NET 8, хостован на Microsoft Service Fabric платформи (stateless сервиси за '
     'Gateway/AuthService/TripService, stateful за SharingService), уз Entity Framework Core '
     '8 за приступ подацима, YARP (Yet Another Reverse Proxy) за Gateway рутирање, BCrypt.Net '
     'за хеширање лозинки и JWT Bearer аутентикацију. Подаци се чувају у Microsoft SQL Server '
     'бази, по принципу database-per-service (UsersDB, TripsDB, SharingDB). Frontend је React '
     '19 (Vite) апликација која комуницира искључиво преко Gateway-а.'),
    ('Naslov 1. nivo', 'OpenTelemetry'),
    ('Body Text',
     'OpenTelemetry (OTel) изабран је као централни стандард надоградње јер обједињује сва '
     'три облика телеметрије (traces, metrics, logs) под једним, vendor-неутралним API-јем и '
     'SDK-ом, са великим бројем готових ("auto") инструментација управо за технологије које '
     'пројекат већ користи — ASP.NET Core, HttpClient, SqlClient — тако да се велик део '
     'вредности добија без писања сопственог инструментационог кода. Верзија OpenTelemetry '
     '.NET SDK-а коришћена у раду је 1.18.0, конзистентна кроз сва четири сервиса.'),
    ('Naslov 1. nivo', 'OpenTelemetry Collector'),
    ('Body Text',
     'Сервиси не шаљу телеметрију директно ка Prometheus/Tempo/Loki, већ ка једном '
     'централном OpenTelemetry Collector-у (OTLP протокол, gRPC на порту 4317). Ово раздваја '
     '„како апликација емитује телеметрију” од „где та телеметрија завршава” — каснија измена '
     'backend-а за складиштење (нпр. замена Loki-ја другим решењем) не би захтевала измену '
     'ниједног сервиса, само Collector-ове конфигурације.'),
    ('Naslov 1. nivo', 'Prometheus, Grafana Tempo, Grafana Loki'),
    ('Body Text',
     'За складиштење три врсте телеметрије изабран је такозвани „Grafana stack”: Prometheus '
     'за метрике (Collector излаже Prometheus-компатибилан /metrics endpoint који Prometheus '
     'периодично scrape-ује), Grafana Tempo за дистрибуирано праћење захтева (trace-ове), и '
     'Grafana Loki за централизоване логове. Предност ове комбинације над, на пример, '
     'самостојећим Jaeger-ом за tracing јесте што све три компоненте имају нативну, '
     'provisioned интеграцију са Grafanom — укључујући унакрсну навигацију (клик са log '
     'линије на њен trace, клик са trace-а на метрику) која је директно демонстрирана у '
     'поглављу 5.'),
    ('Naslov 1. nivo', 'Grafana'),
    ('Body Text',
     'Grafana је изабрана као јединствен визуелни слој за све три врсте телеметрије, уместо '
     'по једног специјализованог алата за сваку (нпр. Jaeger UI за trace-ове, посебан алат за '
     'метрике). Ово директно подржава сценарио „развијач види комплетну слику једног захтева '
     'на једном месту”, који је централна теза рада.'),
    ('Naslov 1. nivo', 'Health checks'),
    ('Body Text',
     'Коришћен је уграђен ASP.NET Core механизам (Microsoft.Extensions.Diagnostics.'
     'HealthChecks), допуњен пакетом AspNetCore.HealthChecks.SqlServer (верзија 9.0.0, '
     'компатибилна са .NET 8) за проверу доступности SQL Server базе по сервису.'),
    ('Naslov 1. nivo', 'k6'),
    ('Body Text',
     'За демонстрацију понашања система под оптерећењем (сценарио 3, поглавље 5) коришћен '
     'је k6 (Grafana k6), алат за load testing чији се скриптовани сценарији пишу у '
     'JavaScript-у, што га чини лакшим за одржавање и читање у односу на алтернативе попут '
     'JMeter-а.'),
    ('Naslov 1. nivo', 'xUnit'),
    ('Body Text',
     'За аутоматизовано тестирање (поглавље 6) коришћен је xUnit, стандардни test framework '
     'за .NET, јер пројекат пре овог рада није имао дефинисан test framework нити иједан тест.'),
    ('Naslov 1. nivo', 'Docker / Docker Compose'),
    ('Body Text',
     'Цела observability инфраструктура (Collector, Prometheus, Tempo, Loki, Grafana) '
     'покреће се кроз Docker Compose, независно од постојећег Service Fabric кластера и SQL '
     'Server инстанце — додатак, не замена постојећег начина рада система.'),
])

d.save(DST)
print("Poglavlja 1-3 dodata, sacuvano:", DST)

# ============================================================
# POGLAVLJE 4 - OPIS POCETNOG RESENJA (AS-IS)
# ============================================================

h = find_heading1('Опис почетног решења')
p = add_block(h, [
    ('Body Text',
     'У овом поглављу описано је стање система Travel Planner пре надоградње описане у '
     'овом раду — архитектура, технологије и, посебно, постојећи (или тачније, непостојећи) '
     'механизми опсервабилности, установљени детаљном анализом изворног кода.'),
    ('Naslov 1. nivo', 'Архитектура система'),
    ('Body Text',
     'Систем чине четири одвојена сервиса на Microsoft Service Fabric платформи:'),
    ('1. nabrajanje',
     'Gateway (stateless) — YARP reverse proxy, једина улазна тачка за frontend; рутира '
     'захтеве ка осталим сервисима по путањи, без сопствене пословне логике или базе.'),
    ('1. nabrajanje',
     'AuthService (stateless) — регистрација, пријава, издавање и потписивање JWT токена, '
     'улоге (Корисник/Admin), администрација корисничких налога. База UsersDB.'),
    ('1. nabrajanje',
     'TripService (stateless) — централни сервис са целокупним садржајем плана: '
     'дестинације, активности, трошкови/буџет, checklist, белешке, подсетници. База TripsDB.'),
    ('1. nabrajanje',
     'SharingService (stateful) — дељење плана путем кода/QR-а (VIEW/EDIT). SharingDB је '
     'извор истине, а важећи кодови се додатно кеширају у Service Fabric Reliable '
     'Collections ради брже валидације — то је разлог зашто је баш овај сервис stateful.'),
])
p = add_figure(p, r"c:\Users\bulic\Desktop\web\Web2\dijagrami\arhitektura-original.png",
    'Слика 4.1. Архитектура система пре надоградње', width_inches=6.3)
p = add_block(p, [
    ('Body Text',
     'Комуникација frontend-а са системом иде искључиво преко Gateway-а (HTTP/REST + JWT). '
     'Позиви сервис-сервис (AuthService → TripService приликом брисања налога, SharingService '
     '→ TripService приликом VIEW приступа) иду директно, мимо Gateway-а, и осигурани су '
     'дељеним тајним кључем у X-Internal-Key заглављу — не JWT токеном, јер у том контексту '
     'не постоји прављени корисник.'),
    ('Naslov 1. nivo', 'Технологије'),
    ('Body Text',
     'Backend: C#/.NET 8, Microsoft Service Fabric, Entity Framework Core 8, YARP, '
     'BCrypt.Net, JWT Bearer аутентикација. База података: Microsoft SQL Server, три '
     'одвојене базе (database-per-service). Frontend: React 19 (Vite), Tailwind CSS, React '
     'Router, axios.'),
    ('Naslov 1. nivo', 'Постојећи механизми опсервабилности'),
    ('Body Text',
     'Детаљна анализа изворног кода (претрага на ILogger/_logger позиве, health check '
     'регистрације, tracing/metrics пакете) показала је следеће затечено стање:'),
    ('1. nabrajanje',
     'Логовање: искључиво подразумевани ASP.NET Core Console logger (ниво Information/'
     'Warning дефинисан у appsettings.json). Претрага кроз цео backend показала је нула '
     'позива ILogger/_logger — контролери, сервиси и HTTP клијенти не логују ништа.'),
    ('1. nabrajanje',
     'Service Fabric ETW ServiceEventSource постоји по сервису, али се користи искључиво за '
     'lifecycle догађаје (покретање Kestrel-а, регистрација сервисног типа) — методе '
     'ServiceRequestStart/Stop су дефинисане, али се нигде не позивају.'),
    ('1. nabrajanje', 'Метрике: не постоје — нема Prometheus exporter-а нити коришћења .NET Metrics API-ја.'),
    ('1. nabrajanje',
     'Дистрибуирано праћење захтева: не постоји. Нема trace/correlation ID-а нигде у '
     'систему — ни на frontend-у, ни на Gateway-у, ни у сервис-сервис позивима.'),
    ('1. nabrajanje',
     'Health checks: AddHealthChecks() се нигде не позива — ниједан сервис нема /health '
     'endpoint. Service Fabric-у се health стање апликационог нивоа никада не пријављује.'),
    ('1. nabrajanje',
     'Руковање грешкама: нема глобалног exception middleware-а; неухваћени изузеци падају '
     'на ASP.NET Core подразумевано понашање, без структурисаног лога.'),
    ('1. nabrajanje', 'Тестови: не постоји ниједан тест пројекат, ни backend ни frontend.'),
    ('Body Text',
     'Ово затечено стање и конкретни проблеми који из њега произилазе детаљно су описани у '
     'поглављу 2 и представљају директну полазну тачку за решење описано у поглављу 5.'),
])

d.save(DST)
print("Poglavlje 4 dodato, sacuvano:", DST)

# ============================================================
# POGLAVLJE 5 - OPIS RESENJA PROBLEMA (TO-BE + implementacija)
# ============================================================

h = find_heading1('Опис решења проблема')
p = add_block(h, [
    ('Body Text',
     'Решење је реализовано као додатни слој опсервабилности изнад постојећег система, без '
     'измене пословне логике. Имплементација је спроведена кроз шест логичких целина: '
     'health checks, инфраструктура за прикупљање телеметрије, инструментација сервиса '
     '(traces/metrics/logs), Grafana dashboard, тестирање (описано одвојено у поглављу 6) и '
     'демонстрациони сценарији над стварно постављеним системом.'),
    ('Naslov 1. nivo', 'TO-BE архитектура'),
    ('Body Text',
     'Свих четири сервиса шаљу телеметрију преко OTLP протокола ка једном OpenTelemetry '
     'Collector-у, који је даље усмерава: trace-ове ка Tempo-у, метрике ка Prometheus-у '
     '(преко сопственог /metrics endpoint-а који Prometheus периодично scrape-uje), логове '
     'ка Loki-ју. Grafana приказује сва три извора обједињено, укључујући унакрсну '
     'навигацију (log линија → trace, trace → метрика).'),
])
p = add_figure(p, r"c:\Users\bulic\Desktop\web\Web2\dijagrami\arhitektura-observability.png",
    'Слика 5.1. TO-BE архитектура — додат observability слој', width_inches=6.3)
p = add_block(p, [
    ('Body Text',
     'Пропагација trace контекста кроз сервис-сервис позиве (нпр. SharingService → '
     'TripService) дешава се аутоматски, без иједне линије custom кода: пошто и позивалац и '
     'прималац користе исту OpenTelemetry HttpClient/ASP.NET Core инструментацију, W3C '
     'traceparent заглавље се чита из долазног захтева и пише на одлазни у оквиру истог '
     'ambient Activity контекста. X-Internal-Key заглавље остаје непромењено (механизам '
     'аутентикације), traceparent је одвојено, стандардно заглавље.'),

    ('Naslov 1. nivo', 'Health checks'),
    ('Body Text',
     'Сваки сервис излаже два endpoint-а: /health/live (процес је жив, не додирује '
     'зависности — намењен одлукама о рестарту) и /health/ready (процес и критичне '
     'зависности спремни да опслуже саобраћај). За AuthService/TripService/SharingService, '
     '/health/ready проверава доступност сопствене SQL Server базе. Gateway нема сопствену '
     'базу — његов /health/ready проверава мрежну доступност три downstream сервиса, преко '
     'истих адреса које YARP већ користи за рутирање (без дуплирања конфигурације). Одговор '
     'је структурисан JSON са статусом сваке појединачне провере и, у случају грешке, '
     'читљивом поруком.'),

    ('Naslov 1. nivo', 'Инструментација сервиса'),
    ('Body Text',
     'Заједничка OpenTelemetry конфигурација (traces + metrics + logs) дефинисана је у по '
     'једној ObservabilityExtensions класи по сервису (исти образац поновљен по сервису, јер '
     'пројекат нема дељену библиотеку између сервиса), и укључује:'),
    ('1. nabrajanje',
     'Traces — ASP.NET Core инструментацију (server span по захтеву), HttpClient '
     'инструментацију (пропагација контекста кроз сервис-сервис позиве, описано изнад) и '
     'SqlClient инструментацију (child span по SQL упиту, на три сервиса са базом). Намерно '
     'без снимања сировог SQL текста — у коришћеној верзији пакета то захтева експлицитно '
     'укључивање преко environment променљиве управо због ризика цурења података кроз SQL '
     'параметре; уместо тога користи се RecordException.'),
    ('1. nabrajanje',
     'Metrics — RED метрике (request rate, error rate, latency) из ASP.NET Core/HttpClient '
     'инструментације, плус метрике извршног окружења (CPU, меморија, garbage collector, '
     'thread pool) из OpenTelemetry.Instrumentation.Runtime пакета — све без иједне линије '
     'custom кода.'),
    ('1. nabrajanje',
     'Logs — ILogger и даље пише на конзолу (непромењено, ради локалног развоја), а сада '
     'додатно и преко OpenTelemetry logging провајдера ка Loki-ју, са аутоматски додатим '
     'trace_id/span_id и ASP.NET Core контекстом (путања захтева, connection ID) у сваком '
     'запису.'),
    ('Body Text',
     'Као директна последица овог рада, попуњен је и конкретан „тих” catch блок пронађен '
     'приликом анализе (AuthService/Services/TripClient.cs, метода DeleteUserTripsAsync) — '
     'неуспешан HTTP статус сада производи LogWarning, мрежни изузетак производи LogError, '
     'уместо да се грешка потпуно прогута.'),

    ('Naslov 1. nivo', 'Grafana dashboard'),
    ('Body Text',
     'Provisioned dashboard „Travel Planner - Overview” приказује RED метрике (request '
     'rate, error rate, p95 latency), ресурсе (меморија/CPU/GC) и ток логова, све '
     'филтрирано по сервису преко template варијабле. Datasource-и (Prometheus/Tempo/Loki) '
     'су такође provisioned, укључујући derived field везу која омогућава да се из log '
     'линије у Loki-ју једним кликом отвори одговарајући trace у Tempo-у.'),
])

p = add_block(p, [
    ('Naslov 1. nivo', 'Демонстрациони сценарији'),
    ('Body Text',
     'Сва четири сценарија извршена су над стварно постављеним системом (Service Fabric '
     'локални кластер, праве базе), не симулирана — детаљна документација са тачним trace '
     'ID-јевима, log редовима и измереним временима налази се у пратећем фајлу '
     'docs/observability-scenarios.md; овде су сумирани кључни резултати.'),

    ('Naslov 2. nivo', 'Сценарио 1 — нормалан захтев кроз више сервиса'),
    ('Body Text',
     'Пријава корисника, креирање VIEW дељења плана, па разрешавање тог кода као анониман '
     'посетилац — последњи корак намерно прелази три процеса: Gateway → SharingService → '
     'TripService. Добијен trace показује сервер span на Gateway-у, дете client span ка '
     'SharingService-у, унутар њега сервер span и даље дете client span ка TripService-у, а '
     'унутар TripService-овог сервер span-а девет SQL child span-ова (по један за сваку '
     'табелу коју InternalController.GetFullPlan чита). Сваки SQL упит добио је сопствени '
     'child span, исправно угнежђен под HTTP сервер span-ом који га је изазвао — аутоматски, '
     'без иједне линије custom кода за пропагацију контекста. Логови сва три сервиса '
     'корелисани су преко истог trace_id, укључујући и EF Core-ове сопствене „Executed '
     'DbCommand” записе са тачним трајањем по упиту.'),

    ('Naslov 2. nivo', 'Сценарио 2 — грешка (пад зависности)'),
    ('Body Text',
     'TripService је рестартован кроз Service Fabric-ов сопствени механизам '
     '(Restart-ServiceFabricDeployedCodePackage) — покушај да се процес угаси директно са '
     'нивоа оперативног система је одбијен, јер сервис ради под другим Windows налогом него '
     'интерактивна сесија. Gateway-ов /health/ready је истог тренутка пријавио Unhealthy '
     'статус за trip-service проверу, систем се сам опоравио за приближно седам секунди, а '
     'health check инфраструктура је аутоматски забележила Error-ниво лог са тачном поруком '
     'узрока квара — без иједне линије custom кода написане за ову конкретну ситуацију.'),

    ('Naslov 2. nivo', 'Сценарио 3 — повећано оптерећење'),
    ('Body Text',
     'k6 load test (скрипта: observability/load-tests/scenario3-load-test.js) са расподелом '
     'до 30 паралелних виртуелних корисника, реалистичан ток (пријава па преглед планова). '
     'Резултат: 780 захтева, 0% неуспешних, p95 латенција измерена на клијенту (k6) '
     'износила је 2.12 секунде, а независно измерена на серверу (Prometheus хистограм из '
     'OTel метрике) 2.14 секунде — готово идентично, што потврђује да метрике стварно '
     'одражавају понашање система под оптерећењем, не само да „нешто броје”.'),

    ('Naslov 2. nivo', 'Сценарио 4 — спора зависност (уско грло)'),
    ('Body Text',
     'Директном SQL трансакцијом закључан је тачно један ред у табели Plans (X lock, без '
     'commit-а), а истовремено је упућен прави HTTP захтев (PUT /api/trips/{id}) који мења '
     'исти ред. Trace показује да је укупан захтев трајао 23.9 секунди, при чему SELECT упит '
     'у истом захтеву траје свега 14.5 милисекунди — дистрибуирано праћење захтева тачно '
     'показује да је узрок UPDATE наредба, не захтев уопштено. Успут је откривен и стваран '
     'налаз: TripsDB има укључен READ_COMMITTED_SNAPSHOT, па читања не чекају на закључан '
     'ред (потврђено директним увидом у sys.dm_tran_locks); тек упис са стварно измењеном '
     'вредношћу (writer-vs-writer сукоб, на који snapshot изолација не утиче) изазива право '
     'чекање.'),
])

p = add_table(p,
    headers=['Област', 'Пре надоградње (AS-IS)', 'После надоградње (TO-BE)'],
    rows=[
        ['Logging', 'Нула позива ILogger; подразумевани Console logger, ниво Information '
                     'непопуњен садржајем.',
         'Структурисани логови (Microsoft.Extensions.Logging + OTel provider) са аутоматским '
         'trace_id/span_id, слати ка Loki-ју преко OTLP.'],
        ['Metrics', 'Не постоје ниједне метрике перформанси нити ресурса.',
         'RED метрике + ресурси извршног окружења (CPU/меморија/GC), аутоматски из ASP.NET '
         'Core/HttpClient/Runtime инструментације.'],
        ['Tracing', 'Не постоји; нема correlation/trace ID-а нигде у систему.',
         'Пун W3C дистрибуирано праћење захтева кроз све сервисе, укључујући сервис-сервис '
         'позиве, аутоматска пропагација без custom кода.'],
        ['Monitoring', 'Нема визуелизације; једини увид је ручно читање конзолног излаза.',
         'Grafana dashboard (RED метрике, ресурси, log stream) провисиониран и филтриран по '
         'сервису.'],
        ['Health checks', 'Не постоје; Service Fabric нема увид у апликациони health.',
         '/health/live и /health/ready на сва четири сервиса, укључујући проверу база и '
         'downstream зависности.'],
        ['Error detection', 'Тихе грешке (нпр. TripClient catch блок без иједног лога).',
         'Грешке произвoде log записе одговарајућег нивоа (Warning/Error), видљиве и '
         'корелисане са trace-ом.'],
        ['Debugging', 'Ручно упоређивање временских печата логова из различитих процеса.',
         'Један trace_id повезује логове, метрике и trace кроз сва три сервиса укључена у '
         'захтев.'],
    ],
    caption='Табела 5.1. Поређење система пре и после надоградње')

d.save(DST)
print("Poglavlje 5 (kompletno) dodato, sacuvano:", DST)

# ============================================================
# POGLAVLJE 6 - TESTIRANJE I REZULTATI
# ============================================================

h = find_heading1('Тестирање и резултати')
p = add_block(h, [
    ('Body Text',
     'Формално тестирање намерно је ограничено на оно што је овај рад заиста додао — '
     'health checks и observability pipeline. Постојећа пословна логика (планови, '
     'дестинације, трошкови) није имала тестове пре овог рада и није предмет теме; '
     'проширивање обима тамо било би ван онога што је затражено.'),
    ('Naslov 1. nivo', 'Рефакторинг ради тестабилности'),
    ('Body Text',
     'Пре писања тестова, DI регистрација и middleware pipeline сваког сервиса издвојени '
     'су из анонимне lambda функције унутар Service Fabric listener factory-ја у јавне '
     'статичке методе ConfigureServices(WebApplicationBuilder) и '
     'ConfigurePipeline(WebApplication) (по једна класа по сервису: GatewayApp, '
     'AuthServiceApp, TripServiceApp, SharingServiceApp). Service Fabric и даље креира '
     'WebApplicationBuilder и конфигурише WebHost (потребан му је listener/url из SF '
     'контекста), али саму регистрацију сервиса и middleware pipeline сада позива из ове '
     'заједничке методе — идентично без обзира да ли позивалац сервиса хостује SF или тест. '
     'Рефакторинг је чисто издвајање, без промене понашања, што је потврђено поновним '
     'build-овањем сва четири сервиса и redeploy-ем на Service Fabric кластер, уз проверу да '
     'су сви /health/live и /health/ready endpoint-и остали идентично здрави пре и после.'),
    ('Naslov 1. nivo', 'Тест пројекти и стратегија'),
    ('Body Text',
     'Четири xUnit пројекта (по један за сваки сервис) не користе WebApplicationFactory '
     '(Service Fabric hosting модел нема класичну Program улазну тачку потребну за то) — '
     'сваки тест подиже прав Kestrel host на ефемерном порту преко издвојених '
     'ConfigureServices/ConfigurePipeline метода, без mock-а базе података.'),
    ('1. nabrajanje',
     'HealthEndpointsTests (сва четири сервиса) — интеграциони тестови, прав HTTP позив ка '
     'правом покренутом host-у. За AuthService/TripService/SharingService користи се права '
     'локална SQL Server база. Gateway користи FakeDownstreamServer (минималан прав HTTP '
     'сервер покренут у тест процесу) да симулира доступан/недоступан downstream сервис. '
     'Покривено за сва четири сервиса: /health/live враћа Healthy без обзира на доступност '
     'зависности; /health/ready враћа Healthy + 200 када је зависност доступна, а Unhealthy '
     '+ 503 са читљивом поруком грешке када није.'),
    ('1. nabrajanje',
     'ObservabilityPipelineTests (сва четири сервиса) — тестира да наш код исправно '
     'региструје и активира OTel инструментацију, коришћењем уграђеног System.Diagnostics.'
     'ActivityListener (без OTel-специфичних тест пакета, без Docker зависности). Прва '
     'верзија теста (hook на ActivityStarted) није прошла — открила је стваран детаљ: '
     'Activity.DisplayName у тренутку ActivityStarted је сирово име извора (нпр. '
     '„Microsoft.AspNetCore.Hosting.HttpRequestIn”), не обогаћено име руте које се види у '
     'Tempo-у; обогаћивање се дешава тек при ActivityStopped, када је позната рута. Исправка '
     '(hook на ActivityStopped) је и исправнија и тачније одражава оно што се стварно види у '
     'tracing backend-у.'),
    ('1. nabrajanje',
     'TripClientTests (AuthService.Tests) — регресиони тест за поменути „тих” catch блок. '
     'Са лажним HttpMessageHandler-ом (успех / HTTP 500 / баца HttpRequestException) и '
     'лажним ILogger<TripClient> који хвата позиве, три теста доказују да успешан позив не '
     'производи ниједан лог, неуспешан HTTP статус производи LogWarning, а мрежни квар '
     'производи LogError.'),
    ('Naslov 1. nivo', 'Резултати'),
])
p = add_table(p,
    headers=['Пројекат', 'Passed', 'Failed', 'Total'],
    rows=[
        ['TripService.Tests', '6', '0', '6'],
        ['AuthService.Tests', '7', '0', '7'],
        ['SharingService.Tests', '4', '0', '4'],
        ['Gateway.Tests', '4', '0', '4'],
        ['Укупно', '21', '0', '21'],
    ],
    caption='Табела 6.1. Резултати аутоматизованог тестирања (dotnet test)')
p = add_block(p, [
    ('Body Text',
     'Сви тестови пролазе. Тест пројекти су додати у TravelPlanner.sln, па се граде и '
     'приказују заједно са остатком решења у Visual Studio-у. Детаљан план и образложење '
     'обима налазе се у пратећем фајлу docs/testing.md.'),
    ('Body Text',
     'Осим формалних тестова, решење је верификовано и кроз четири демонстрациона '
     'сценарија изведена над стварно постављеним системом (поглавље 5), што заједно чини '
     'двослојну верификацију: аутоматизовани тестови доказују да наш код исправно ради у '
     'изолацији, а демонстрациони сценарији доказују да цео систем — укључујући Docker '
     'инфраструктуру, стварну базу података и стварну мрежну комуникацију између процеса — '
     'ради исправно у целини.'),
])

d.save(DST)
print("Poglavlje 6 dodato, sacuvano:", DST)

# ============================================================
# POGLAVLJE 7 - ZAKLJUCAK
# ============================================================

h = find_heading1('Закључак')
add_block(h, [
    ('Body Text',
     'У овом раду анализиран је постојећи систем Travel Planner са аспекта опсервабилности, '
     'идентификовани су конкретни недостаци (одсуство структурисаног логовања, метрика, '
     'дистрибуираног праћења захтева и health check механизама, као и тихе грешке у постојећем '
     'коду), и имплементирана је надоградња заснована на OpenTelemetry стандарду и Prometheus/'
     'Grafana Tempo/Grafana Loki/Grafana инфраструктури, без измене постојеће пословне логике.'),
    ('Body Text',
     'Решење је верификовано на два независна начина: аутоматизованим тестовима (21 тест, '
     'сви пролазе, поглавље 6) и кроз четири демонстрациона сценарија изведена над стварно '
     'постављеним системом (нормалан захтев кроз три сервиса, намерна грешка са аутоматским '
     'опоравком, k6 load test са потврђеним поклапањем клијентске и серверске латенције, и '
     'намерно успорена SQL зависност чији је tracing тачно локализовао — не само да је '
     'захтев спор, већ и која конкретна SQL наредба је узрок).'),
    ('Naslov 1. nivo', 'Ограничења'),
    ('Body Text',
     'Health check позиви (/health/live, /health/ready) пролазе кроз исту ASP.NET Core '
     'инструментацију као и сав остали саобраћај, па се њихови одговори мешају у исте '
     'метрике захтева као и стварни кориснички захтеви — за производни dashboard било би '
     'потребно експлицитно искључити /health/* путање из панела за стопу грешака.'),
    ('Body Text',
     'Observability инфраструктура (Docker Compose стек) ради независно од постојећег '
     'Service Fabric кластера; интеграција са Service Fabric-овим сопственим Health Manager '
     'механизмом (тако да апликациони health утиче и на SF-ове одлуке о опоравку) није '
     'реализована у овом раду.'),
    ('Body Text',
     'Distributed tracing тренутно почиње на Gateway-у; frontend (browser) није '
     'инструментисан OpenTelemetry Web SDK-ом, па је почетни, мрежни део захтева (од '
     'корисниковог browser-a до Gateway-a) ван обухвата trace-а.'),
    ('Naslov 1. nivo', 'Правци даљег развоја'),
    ('1. nabrajanje',
     'Формално alerting решење (нпр. Prometheus Alertmanager) изнад постојећих Grafana '
     'dashboard-а, за проактивно обавештавање уместо ручног прегледа.'),
    ('1. nabrajanje',
     'Инструментација frontend-а (OpenTelemetry Web SDK) ради потпуног end-to-end trace-а, '
     'од browser-а до базе података.'),
    ('1. nabrajanje',
     'Аутоматизација k6 load test-а као дела CI/CD pipeline-а, ради континуираног праћења '
     'деградације перформанси између верзија.'),
    ('1. nabrajanje',
     'Интеграција апликационог health статуса са Service Fabric Health Manager-ом, ради '
     'дубље сарадње са платформом за опоравак од квара.'),
])

# ============================================================
# LITERATURA
# ============================================================

h = find_heading1('Литература')
p = h
for i, text in enumerate([
    'IEEE, "IEEE Reference Guide." ieeeauthorcenter.ieee.org. '
    'https://ieeeauthorcenter.ieee.org/wp-content/uploads/IEEE-Reference-Guide.pdf '
    '(приступљено: септембар 2026).',

    'OpenTelemetry Authors, "OpenTelemetry .NET Documentation." opentelemetry.io. '
    'https://opentelemetry.io/docs/languages/net/ (приступљено: септембар 2026).',

    'Microsoft, "Health checks in ASP.NET Core." learn.microsoft.com. '
    'https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks '
    '(приступљено: септембар 2026).',

    'Microsoft, "Service Fabric documentation." learn.microsoft.com. '
    'https://learn.microsoft.com/en-us/azure/service-fabric/ (приступљено: септембар 2026).',

    'YARP contributors, "YARP: Yet Another Reverse Proxy - Documentation." '
    'microsoft.github.io. https://microsoft.github.io/reverse-proxy/ '
    '(приступљено: септембар 2026).',

    'Grafana Labs, "Grafana Tempo documentation." grafana.com. '
    'https://grafana.com/docs/tempo/latest/ (приступљено: септембар 2026).',

    'Grafana Labs, "Grafana Loki documentation." grafana.com. '
    'https://grafana.com/docs/loki/latest/ (приступљено: септембар 2026).',

    'Grafana Labs, "Grafana k6 documentation." grafana.com. '
    'https://grafana.com/docs/k6/latest/ (приступљено: септембар 2026).',

    'Prometheus Authors, "Prometheus documentation." prometheus.io. '
    'https://prometheus.io/docs/introduction/overview/ (приступљено: септембар 2026).',

    'The OpenTelemetry Collector Authors, "OpenTelemetry Collector documentation." '
    'opentelemetry.io. https://opentelemetry.io/docs/collector/ (приступљено: септембар 2026).',
], start=1):
    p = insert_paragraph_after(p, f'[{i}] {text}', 'literatura')

# ukloni preostale "ПОПУНИТИ" placeholdere odmah posle nasih stvarnih izvora
for para in list(d.paragraphs):
    if para.style.name == 'Normal (Web)' and para.text.strip() == 'ПОПУНИТИ':
        el = para._element
        el.getparent().remove(el)

# ============================================================
# CLEANUP - ukloni preostale placeholder paragrafe iz sablona
# ============================================================

removed = 0
for para in list(d.paragraphs):
    if para.text.strip() in ('ТЕКСТ.', 'ТЕКСТ..'):
        el = para._element
        el.getparent().remove(el)
        removed += 1
print(f"Uklonjeno {removed} placeholder paragrafa (TEKST.)")

d.save(DST)
print("KOMPLETNO sacuvano:", DST)
print("Ukupno paragraphs:", len(d.paragraphs), "tables:", len(d.tables))
