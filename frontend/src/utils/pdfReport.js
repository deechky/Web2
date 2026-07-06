import jsPDF from 'jspdf'

// jsPDF-ov ugrađeni font (Helvetica/WinAnsi) nema č/ć/đ (potvrđeno testom: ta slova se potpuno
// gube iz teksta, npr. "Grčkoj" -> "Grkoj") — š/ž rade, ali radi doslednosti transliterišemo sve.
const DIACRITIC_MAP = {
  č: 'c', ć: 'c', š: 's', ž: 'z', đ: 'dj',
  Č: 'C', Ć: 'C', Š: 'S', Ž: 'Z', Đ: 'Dj',
}

function transliterate(text) {
  return String(text).replace(/[čćšžđČĆŠŽĐ]/g, (ch) => DIACRITIC_MAP[ch])
}

export function generateTripPdf({ plan, destinacije, aktivnosti, troskovi, checklistStavke, beleske = [], podsetnici = [], budget }) {
  const doc = new jsPDF()
  let y = 15

  function line(text, size = 11, gap = 7) {
    if (y > 270) {
      doc.addPage()
      y = 15
    }
    doc.setFontSize(size)
    doc.text(transliterate(text), 14, y)
    y += gap
  }

  function section(title) {
    y += 3
    line(title, 13, 7)
  }

  line(plan.naziv, 18, 10)
  if (plan.opis) line(plan.opis)
  line(`${plan.pocetniDatum} — ${plan.krajnjiDatum}`)
  if (plan.napomene) line(`Napomene: ${plan.napomene}`)

  if (budget) {
    section('Budžet')
    line(`Planirano: ${budget.planirano}`)
    line(`Potrošeno: ${budget.potroseno}`)
    line(`Preostalo: ${budget.preostalo}`)
  }

  section('Destinacije')
  if (destinacije.length === 0) {
    line('Nema unesenih destinacija.')
  } else {
    destinacije.forEach((d) => line(`${d.naziv} — ${d.lokacija} (${d.datumDolaska} - ${d.datumOdlaska})`))
  }

  section('Aktivnosti')
  if (aktivnosti.length === 0) {
    line('Nema unesenih aktivnosti.')
  } else {
    aktivnosti.forEach((a) => line(`${a.datum} ${a.vreme ? a.vreme + ' ' : ''}— ${a.naziv} (${a.status})`))
  }

  section('Troškovi')
  if (troskovi.length === 0) {
    line('Nema unesenih troškova.')
  } else {
    troskovi.forEach((t) => line(`${t.naziv} — ${t.iznos} (${t.kategorija}, ${t.datum})`))
  }

  section('Checklist')
  if (checklistStavke.length === 0) {
    line('Nema stavki.')
  } else {
    checklistStavke.forEach((c) => line(`${c.zavrseno ? '[x]' : '[ ]'} ${c.naziv}`))
  }

  section('Podsetnici')
  if (podsetnici.length === 0) {
    line('Nema podsetnika.')
  } else {
    podsetnici.forEach((r) => line(`${r.zavrseno ? '[x]' : '[ ]'} ${r.datum} — ${r.naziv}`))
  }

  section('Beleške')
  if (beleske.length === 0) {
    line('Nema beleški.')
  } else {
    beleske.forEach((b) => {
      line(b.naslov, 11, 6)
      doc.setFontSize(10)
      const wrapped = doc.splitTextToSize(transliterate(b.sadrzaj), 180)
      wrapped.forEach((row) => line(row, 10, 5))
    })
  }

  doc.save(`${plan.naziv || 'plan'}.pdf`)
}
