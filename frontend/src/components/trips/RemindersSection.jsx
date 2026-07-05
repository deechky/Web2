import { useEffect, useState } from 'react'
import reminderService from '../../services/reminderService'
import useFeedback from '../../hooks/useFeedback'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Alert from '../ui/Alert.jsx'

const EMPTY_FORM = { naziv: '', datum: '', opis: '' }

export default function RemindersSection({ tripId }) {
  const [items, setItems] = useState([])
  const { error, success, showSuccess, showError } = useFeedback()
  const [form, setForm] = useState(EMPTY_FORM)
  const [editingId, setEditingId] = useState(null)

  useEffect(() => {
    load()
  }, [tripId])

  async function load() {
    try {
      setItems(await reminderService.getAll(tripId))
    } catch (err) {
      showError(err.response?.data?.poruka || 'Neuspešno učitavanje podsetnika.')
    }
  }

  function validate() {
    if (!form.naziv || !form.datum) {
      return 'Naziv i datum su obavezni.'
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

    const dto = { naziv: form.naziv, datum: form.datum, opis: form.opis, zavrseno: false }

    try {
      if (editingId) {
        const existing = items.find((r) => r.id === editingId)
        const updated = await reminderService.update(tripId, editingId, {
          ...dto,
          zavrseno: existing?.zavrseno || false,
        })
        setItems((prev) => prev.map((r) => (r.id === editingId ? updated : r)))
        showSuccess('Podsetnik je izmenjen.')
      } else {
        const created = await reminderService.create(tripId, dto)
        setItems((prev) => [...prev, created])
        showSuccess('Podsetnik je dodat.')
      }
      setForm(EMPTY_FORM)
      setEditingId(null)
    } catch (err) {
      showError(err.response?.data?.poruka || 'Čuvanje podsetnika nije uspelo.')
    }
  }

  async function handleToggle(item) {
    const updated = await reminderService.update(tripId, item.id, {
      naziv: item.naziv,
      datum: item.datum,
      opis: item.opis,
      zavrseno: !item.zavrseno,
    })
    setItems((prev) => prev.map((r) => (r.id === item.id ? updated : r)))
  }

  function startEdit(item) {
    setEditingId(item.id)
    setForm({ naziv: item.naziv, datum: item.datum?.slice(0, 10) || '', opis: item.opis || '' })
  }

  function cancelEdit() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  async function handleRemove(id) {
    await reminderService.remove(tripId, id)
    setItems((prev) => prev.filter((r) => r.id !== id))
    if (editingId === id) cancelEdit()
    showSuccess('Podsetnik je obrisan.')
  }

  return (
    <section className="rounded-2xl bg-white p-5 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-slate-900">Podsetnici</h2>
      <ul className="mb-4 space-y-2">
        {items.map((item) => (
          <li
            key={item.id}
            className="flex flex-wrap items-center justify-between gap-2 rounded-lg bg-slate-50 px-3 py-2"
          >
            <label className="flex flex-1 items-center gap-2">
              <input type="checkbox" checked={item.zavrseno} onChange={() => handleToggle(item)} />
              <span>
                <span className={item.zavrseno ? 'text-slate-400 line-through' : 'text-slate-700'}>
                  {item.naziv}
                </span>
                <span className="ml-2 text-xs text-slate-400">{item.datum?.slice(0, 10)}</span>
                {item.opis && <span className="block text-xs text-slate-500">{item.opis}</span>}
              </span>
            </label>
            <span className="flex gap-2">
              <Button variant="secondary" onClick={() => startEdit(item)}>
                Uredi
              </Button>
              <Button variant="danger" onClick={() => handleRemove(item.id)}>
                Obriši
              </Button>
            </span>
          </li>
        ))}
        {items.length === 0 && <li className="text-sm text-slate-500">Nema podsetnika.</li>}
      </ul>
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
        <div className="sm:col-span-2">
          <Input
            label="Opis (opciono)"
            value={form.opis}
            onChange={(e) => setForm((f) => ({ ...f, opis: e.target.value }))}
          />
        </div>
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
          <Button type="submit">{editingId ? 'Sačuvaj izmene' : 'Dodaj podsetnik'}</Button>
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
