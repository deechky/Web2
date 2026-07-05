import { useEffect, useState } from 'react'
import activityService from '../../services/activityService'
import useFeedback from '../../hooks/useFeedback'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Select from '../ui/Select.jsx'
import Alert from '../ui/Alert.jsx'
import CalendarView from './CalendarView.jsx'

const STATUSI = ['Planirano', 'Rezervisano', 'Zavrseno', 'Otkazano']
const EMPTY_FORM = { naziv: '', datum: '', vreme: '', procenjeniTrosak: '', status: 'Planirano' }

export default function ActivitiesSection({ tripId, onChanged }) {
  const [items, setItems] = useState([])
  const { error, success, showSuccess, showError } = useFeedback()
  const [form, setForm] = useState(EMPTY_FORM)
  const [editingId, setEditingId] = useState(null)
  const [view, setView] = useState('list')

  useEffect(() => {
    load()
  }, [tripId])

  async function load() {
    try {
      setItems(await activityService.getAll(tripId))
    } catch (err) {
      showError(err.response?.data?.poruka || 'Neuspešno učitavanje aktivnosti.')
    }
  }

  function validate() {
    if (!form.naziv || !form.datum) {
      return 'Naziv i datum su obavezni.'
    }
    if (Number(form.procenjeniTrosak) < 0) {
      return 'Procenjeni trošak ne može biti negativan.'
    }
    return ''
  }

  async function handleSubmit(e) {
    e.preventDefault()
    const validationError = validate()
    if (validationError) {
      showError(validationError)
      return
    }

    const dto = {
      naziv: form.naziv,
      datum: form.datum,
      vreme: form.vreme || null,
      procenjeniTrosak: Number(form.procenjeniTrosak) || 0,
      status: form.status,
    }

    try {
      if (editingId) {
        const updated = await activityService.update(tripId, editingId, dto)
        setItems((prev) => prev.map((a) => (a.id === editingId ? updated : a)))
        showSuccess('Aktivnost je uspešno izmenjena.')
      } else {
        const created = await activityService.create(tripId, dto)
        setItems((prev) => [...prev, created])
        showSuccess('Aktivnost je uspešno dodata.')
      }
      setForm(EMPTY_FORM)
      setEditingId(null)
      onChanged?.()
    } catch (err) {
      showError(err.response?.data?.poruka || 'Čuvanje aktivnosti nije uspelo.')
    }
  }

  function startEdit(activity) {
    setEditingId(activity.id)
    setForm({
      naziv: activity.naziv,
      datum: activity.datum?.slice(0, 10) || '',
      vreme: activity.vreme || '',
      procenjeniTrosak: activity.procenjeniTrosak ?? '',
      status: activity.status,
    })
  }

  function cancelEdit() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  async function handleRemove(id) {
    await activityService.remove(tripId, id)
    setItems((prev) => prev.filter((a) => a.id !== id))
    if (editingId === id) cancelEdit()
    showSuccess('Aktivnost je obrisana.')
    onChanged?.()
  }

  return (
    <section className="rounded-2xl bg-white p-5 shadow-sm">
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <h2 className="text-lg font-semibold text-slate-900">Aktivnosti (po danima)</h2>
        <div className="flex overflow-hidden rounded-lg border border-slate-300 text-sm">
          <button
            type="button"
            onClick={() => setView('list')}
            className={`px-3 py-1 ${view === 'list' ? 'bg-teal-600 text-white' : 'bg-white text-slate-600'}`}
          >
            Lista
          </button>
          <button
            type="button"
            onClick={() => setView('calendar')}
            className={`px-3 py-1 ${view === 'calendar' ? 'bg-teal-600 text-white' : 'bg-white text-slate-600'}`}
          >
            Kalendar
          </button>
        </div>
      </div>
      <div className="mb-4">
        {view === 'calendar' ? (
          <CalendarView activities={items} initialDate={items[0]?.datum} />
        ) : (
          <ActivitiesAgenda activities={items} onEdit={startEdit} onRemove={handleRemove} />
        )}
      </div>
      <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <Input
          label="Naziv"
          value={form.naziv}
          onChange={(e) => setForm((f) => ({ ...f, naziv: e.target.value }))}
        />
        <Input
          label="Datum"
          type="date"
          value={form.datum}
          onChange={(e) => setForm((f) => ({ ...f, datum: e.target.value }))}
        />
        <Input
          label="Vreme (opciono)"
          type="time"
          value={form.vreme}
          onChange={(e) => setForm((f) => ({ ...f, vreme: e.target.value }))}
        />
        <Input
          label="Procenjeni trošak"
          type="number"
          min="0"
          value={form.procenjeniTrosak}
          onChange={(e) => setForm((f) => ({ ...f, procenjeniTrosak: e.target.value }))}
        />
        <Select
          label="Status"
          value={form.status}
          onChange={(e) => setForm((f) => ({ ...f, status: e.target.value }))}
        >
          {STATUSI.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </Select>
        {error && (
          <div className="sm:col-span-2">
            <Alert type="error">{error}</Alert>
          </div>
        )}
        {success && (
          <div className="sm:col-span-2">
            <Alert type="success">{success}</Alert>
          </div>
        )}
        <div className="flex flex-wrap gap-3 sm:col-span-2">
          <Button type="submit">{editingId ? 'Sačuvaj izmene' : 'Dodaj aktivnost'}</Button>
          {editingId && (
            <Button type="button" variant="secondary" onClick={cancelEdit}>
              Otkaži
            </Button>
          )}
        </div>
      </form>
    </section>
  )
}

function ActivitiesAgenda({ activities, onEdit, onRemove }) {
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
              <li
                key={activity.id}
                className="flex flex-wrap items-center justify-between gap-2 text-sm text-slate-700"
              >
                <span>
                  {activity.vreme ? `${activity.vreme} — ` : ''}
                  {activity.naziv} ({activity.status})
                </span>
                <span className="flex gap-2">
                  <Button variant="secondary" onClick={() => onEdit(activity)}>
                    Uredi
                  </Button>
                  <Button variant="danger" onClick={() => onRemove(activity.id)}>
                    Obriši
                  </Button>
                </span>
              </li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  )
}
