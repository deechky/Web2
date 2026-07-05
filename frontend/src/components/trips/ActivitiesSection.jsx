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
      <h2 className="mb-3 text-lg font-semibold text-slate-900">Aktivnosti (po danima)</h2>
      <div className="mb-4">
        <CalendarView activities={items} onEdit={startEdit} onRemove={handleRemove} />
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
