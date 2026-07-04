import jsPDF from 'jspdf'

export function generateTripPdf({ plan, destinacije, aktivnosti, troskovi, checklistStavke, budget }) {
  const doc = new jsPDF()
  let y = 15

  function line(text, size = 11, gap = 7) {
    if (y > 270) {
      doc.addPage()
      y = 15
    }
    doc.setFontSize(size)
    doc.text(String(text), 14, y)
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

  doc.save(`${plan.naziv || 'plan'}.pdf`)
}
