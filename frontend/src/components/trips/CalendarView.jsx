export default function CalendarView({ activities }) {
  const grouped = activities.reduce((acc, activity) => {
    const key = activity.datum
    if (!acc[key]) acc[key] = []
    acc[key].push(activity)
    return acc
  }, {})

  const dates = Object.keys(grouped).sort()

  if (dates.length === 0) {
    return <p className="text-sm text-slate-500">Nema unesenih aktivnosti.</p>
  }

  return (
    <div className="space-y-3">
      {dates.map((date) => (
        <div key={date} className="rounded-lg border border-slate-200 p-3">
          <p className="mb-2 text-sm font-semibold text-teal-700">{date}</p>
          <ul className="space-y-1">
            {grouped[date].map((activity) => (
              <li key={activity.id} className="text-sm text-slate-700">
                {activity.vreme ? `${activity.vreme} — ` : ''}
                {activity.naziv} ({activity.status})
              </li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  )
}
