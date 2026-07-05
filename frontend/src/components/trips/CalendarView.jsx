import { useMemo, useState } from 'react'
import Button from '../ui/Button.jsx'

const DANI = ['Pon', 'Uto', 'Sre', 'Čet', 'Pet', 'Sub', 'Ned']
const MESECI = [
  'Januar', 'Februar', 'Mart', 'April', 'Maj', 'Jun',
  'Jul', 'Avgust', 'Septembar', 'Oktobar', 'Novembar', 'Decembar',
]

export default function CalendarView({ activities, initialDate }) {
  const start = initialDate ? new Date(initialDate) : new Date()
  const [cursor, setCursor] = useState(new Date(start.getFullYear(), start.getMonth(), 1))

  const byDay = useMemo(() => {
    const map = {}
    for (const a of activities) {
      const key = String(a.datum).slice(0, 10)
      if (!map[key]) map[key] = []
      map[key].push(a)
    }
    return map
  }, [activities])

  const year = cursor.getFullYear()
  const month = cursor.getMonth()
  const firstDay = new Date(year, month, 1)
  const offset = (firstDay.getDay() + 6) % 7
  const daysInMonth = new Date(year, month + 1, 0).getDate()

  const cells = []
  for (let i = 0; i < offset; i++) cells.push(null)
  for (let d = 1; d <= daysInMonth; d++) cells.push(d)

  const dayKey = (d) => `${year}-${String(month + 1).padStart(2, '0')}-${String(d).padStart(2, '0')}`

  return (
    <div className="rounded-xl border border-slate-200 p-3">
      <div className="mb-3 flex items-center justify-between">
        <Button variant="secondary" onClick={() => setCursor(new Date(year, month - 1, 1))}>
          ‹
        </Button>
        <span className="font-semibold text-slate-800">
          {MESECI[month]} {year}
        </span>
        <Button variant="secondary" onClick={() => setCursor(new Date(year, month + 1, 1))}>
          ›
        </Button>
      </div>

      <div className="grid grid-cols-7 gap-1 text-center text-xs font-medium text-slate-500">
        {DANI.map((d) => (
          <div key={d} className="py-1">
            {d}
          </div>
        ))}
      </div>

      <div className="grid grid-cols-7 gap-1">
        {cells.map((d, i) => {
          if (d === null) return <div key={`prazno-${i}`} />
          const dnevne = byDay[dayKey(d)] ?? []
          return (
            <div key={d} className="min-h-[64px] rounded-lg border border-slate-100 p-1 text-left align-top">
              <div className="text-xs text-slate-400">{d}</div>
              <div className="space-y-0.5">
                {dnevne.map((a) => (
                  <div
                    key={a.id}
                    title={a.naziv}
                    className="truncate rounded bg-teal-100 px-1 text-[11px] text-teal-800"
                  >
                    {a.naziv}
                  </div>
                ))}
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
