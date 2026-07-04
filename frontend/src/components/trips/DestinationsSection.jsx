import { useEffect, useState } from 'react'
import destinationService from '../../services/destinationService'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Alert from '../ui/Alert.jsx'

const EMPTY_FORM = { naziv: '', lokacija: '', datumDolaska: '', datumOdlaska: '' }

export default function DestinationsSection({ tripId }) {
  const [items, setItems] = useState([])
  const [error, setError] = useState('')
  const [form, setForm] = useState(EMPTY_FORM)
  const [editingId, setEditingId] = useState(null)

  useEffect(() => {
    load()
  }, [tripId])

  async function load() {
    try {
      setItems(await destinationService.getAll(tripId))
    } catch (err) {
      setError(err.response?.data?.poruka || 'Neuspešno učitavanje destinacija.')
    }
  }

  function validate() {
    if (!form.naziv || !form.lokacija || !form.datumDolaska || !form.datumOdlaska) {
      return 'Sva polja su obavezna.'
    }
    if (new Date(form.datumDolaska) > new Date(form.datumOdlaska)) {
      return 'Datum dolaska ne može biti posle datuma odlaska.'
    }
    return ''
  }

  async function handleSubmit(e) {
    e.preventDefault()
    const validationError = validate()
    setError(validationError)
    if (validationError) return

    try {
      if (editingId) {
        const updated = await destinationService.update(tripId, editingId, form)
        setItems((prev) => prev.map((d) => (d.id === editingId ? updated : d)))
      } else {
        const created = await destinationService.create(tripId, form)
        setItems((prev) => [...prev, created])
      }
      setForm(EMPTY_FORM)
      setEditingId(null)
    } catch (err) {
      setError(err.response?.data?.poruka || 'Čuvanje destinacije nije uspelo.')
    }
  }

  function startEdit(d) {
    setEditingId(d.id)
    setForm({
      naziv: d.naziv,
      lokacija: d.lokacija,
      datumDolaska: d.datumDolaska?.slice(0, 10) || '',
      datumOdlaska: d.datumOdlaska?.slice(0, 10) || '',
    })
    setError('')
  }

  function cancelEdit() {
    setEditingId(null)
    setForm(EMPTY_FORM)
    setError('')
  }

  async function handleRemove(id) {
    await destinationService.remove(tripId, id)
    setItems((prev) => prev.filter((d) => d.id !== id))
    if (editingId === id) cancelEdit()
  }

  return (
    <section className="rounded-2xl bg-white p-5 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-slate-900">Destinacije</h2>
      <ul className="mb-4 space-y-2">
        {items.map((d) => (
          <li key={d.id} className="flex items-center justify-between rounded-lg bg-slate-50 px-3 py-2">
            <span>
              {d.naziv} — {d.lokacija} ({d.datumDolaska} - {d.datumOdlaska})
            </span>
            <span className="flex gap-2">
              <Button variant="secondary" onClick={() => startEdit(d)}>
                Uredi
              </Button>
              <Button variant="danger" onClick={() => handleRemove(d.id)}>
                Obriši
              </Button>
            </span>
          </li>
        ))}
        {items.length === 0 && <li className="text-sm text-slate-500">Nema unesenih destinacija.</li>}
      </ul>
      <form onSubmit={handleSubmit} className="grid grid-cols-2 gap-3">
        <Input
          label="Naziv"
          value={form.naziv}
          onChange={(e) => setForm((f) => ({ ...f, naziv: e.target.value }))}
        />
        <Input
          label="Lokacija"
          value={form.lokacija}
          onChange={(e) => setForm((f) => ({ ...f, lokacija: e.target.value }))}
        />
        <Input
          label="Datum dolaska"
          type="date"
          value={form.datumDolaska}
          onChange={(e) => setForm((f) => ({ ...f, datumDolaska: e.target.value }))}
        />
        <Input
          label="Datum odlaska"
          type="date"
          value={form.datumOdlaska}
          onChange={(e) => setForm((f) => ({ ...f, datumOdlaska: e.target.value }))}
        />
        {error && (
          <div className="col-span-2">
            <Alert type="error">{error}</Alert>
          </div>
        )}
        <div className="col-span-2 flex gap-3">
          <Button type="submit">{editingId ? 'Sačuvaj izmene' : 'Dodaj destinaciju'}</Button>
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
