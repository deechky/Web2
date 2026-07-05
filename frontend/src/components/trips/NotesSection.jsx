import { useEffect, useState } from 'react'
import noteService from '../../services/noteService'
import useFeedback from '../../hooks/useFeedback'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Textarea from '../ui/Textarea.jsx'
import Alert from '../ui/Alert.jsx'

const EMPTY_FORM = { naslov: '', sadrzaj: '' }

export default function NotesSection({ tripId }) {
  const [items, setItems] = useState([])
  const { error, success, showSuccess, showError } = useFeedback()
  const [form, setForm] = useState(EMPTY_FORM)
  const [editingId, setEditingId] = useState(null)

  useEffect(() => {
    load()
  }, [tripId])

  async function load() {
    try {
      setItems(await noteService.getAll(tripId))
    } catch (err) {
      showError(err.response?.data?.poruka || 'Neuspešno učitavanje beleški.')
    }
  }

  function validate() {
    if (!form.naslov || !form.sadrzaj) {
      return 'Naslov i sadržaj su obavezni.'
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

    try {
      if (editingId) {
        const updated = await noteService.update(tripId, editingId, form)
        setItems((prev) => prev.map((b) => (b.id === editingId ? updated : b)))
        showSuccess('Beleška je izmenjena.')
      } else {
        const created = await noteService.create(tripId, form)
        setItems((prev) => [created, ...prev])
        showSuccess('Beleška je dodata.')
      }
      setForm(EMPTY_FORM)
      setEditingId(null)
    } catch (err) {
      showError(err.response?.data?.poruka || 'Čuvanje beleške nije uspelo.')
    }
  }

  function startEdit(beleska) {
    setEditingId(beleska.id)
    setForm({ naslov: beleska.naslov, sadrzaj: beleska.sadrzaj })
  }

  function cancelEdit() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  async function handleRemove(id) {
    await noteService.remove(tripId, id)
    setItems((prev) => prev.filter((b) => b.id !== id))
    if (editingId === id) cancelEdit()
    showSuccess('Beleška je obrisana.')
  }

  return (
    <section className="rounded-2xl bg-white p-5 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-slate-900">Beleške</h2>
      <ul className="mb-4 space-y-2">
        {items.map((beleska) => (
          <li key={beleska.id} className="rounded-lg bg-slate-50 px-3 py-2">
            <div className="flex flex-wrap items-start justify-between gap-2">
              <h3 className="font-medium text-slate-800">{beleska.naslov}</h3>
              <span className="flex gap-2">
                <Button variant="secondary" onClick={() => startEdit(beleska)}>
                  Uredi
                </Button>
                <Button variant="danger" onClick={() => handleRemove(beleska.id)}>
                  Obriši
                </Button>
              </span>
            </div>
            <p className="mt-1 whitespace-pre-wrap text-sm text-slate-600">{beleska.sadrzaj}</p>
          </li>
        ))}
        {items.length === 0 && <li className="text-sm text-slate-500">Nema beleški.</li>}
      </ul>
      <form onSubmit={handleSubmit} className="space-y-3">
        <Input
          label="Naslov"
          value={form.naslov}
          onChange={(e) => setForm((f) => ({ ...f, naslov: e.target.value }))}
        />
        <Textarea
          label="Sadržaj"
          rows={3}
          value={form.sadrzaj}
          onChange={(e) => setForm((f) => ({ ...f, sadrzaj: e.target.value }))}
        />
        {error && <Alert type="error">{error}</Alert>}
        {success && <Alert type="success">{success}</Alert>}
        <div className="flex flex-wrap gap-3">
          <Button type="submit">{editingId ? 'Sačuvaj izmene' : 'Dodaj belešku'}</Button>
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
