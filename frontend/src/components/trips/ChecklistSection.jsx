import { useEffect, useState } from 'react'
import checklistService from '../../services/checklistService'
import useFeedback from '../../hooks/useFeedback'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Alert from '../ui/Alert.jsx'

export default function ChecklistSection({ tripId }) {
  const [items, setItems] = useState([])
  const { error, success, showSuccess, showError } = useFeedback()
  const [naziv, setNaziv] = useState('')
  const [editingId, setEditingId] = useState(null)
  const [editNaziv, setEditNaziv] = useState('')

  useEffect(() => {
    load()
  }, [tripId])

  async function load() {
    try {
      setItems(await checklistService.getAll(tripId))
    } catch (err) {
      showError(err.response?.data?.poruka || 'Neuspešno učitavanje checklist-e.')
    }
  }

  async function handleAdd(e) {
    e.preventDefault()

    if (!naziv) {
      showError('Naziv stavke je obavezan.')
      return
    }

    try {
      const created = await checklistService.create(tripId, { naziv })
      setItems((prev) => [...prev, created])
      setNaziv('')
      showSuccess('Stavka je dodata.')
    } catch (err) {
      showError(err.response?.data?.poruka || 'Dodavanje stavke nije uspelo.')
    }
  }

  async function handleToggle(item) {
    const updated = await checklistService.update(tripId, item.id, {
      naziv: item.naziv,
      zavrseno: !item.zavrseno,
    })
    setItems((prev) => prev.map((i) => (i.id === item.id ? updated : i)))
  }

  function startEdit(item) {
    setEditingId(item.id)
    setEditNaziv(item.naziv)
  }

  function cancelEdit() {
    setEditingId(null)
    setEditNaziv('')
  }

  async function saveEdit(item) {
    if (!editNaziv) {
      showError('Naziv stavke je obavezan.')
      return
    }
    const updated = await checklistService.update(tripId, item.id, {
      naziv: editNaziv,
      zavrseno: item.zavrseno,
    })
    setItems((prev) => prev.map((i) => (i.id === item.id ? updated : i)))
    cancelEdit()
    showSuccess('Stavka je izmenjena.')
  }

  async function handleRemove(id) {
    await checklistService.remove(tripId, id)
    setItems((prev) => prev.filter((i) => i.id !== id))
    if (editingId === id) cancelEdit()
    showSuccess('Stavka je obrisana.')
  }

  return (
    <section className="rounded-2xl bg-white p-5 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-slate-900">Checklist / Packing lista</h2>
      <ul className="mb-4 space-y-2">
        {items.map((item) => (
          <li
            key={item.id}
            className="flex flex-wrap items-center justify-between gap-2 rounded-lg bg-slate-50 px-3 py-2"
          >
            {editingId === item.id ? (
              <div className="flex w-full flex-wrap items-center gap-2">
                <Input value={editNaziv} onChange={(e) => setEditNaziv(e.target.value)} className="flex-1" />
                <Button onClick={() => saveEdit(item)}>Sačuvaj</Button>
                <Button variant="secondary" onClick={cancelEdit}>
                  Otkaži
                </Button>
              </div>
            ) : (
              <>
                <label className="flex items-center gap-2">
                  <input type="checkbox" checked={item.zavrseno} onChange={() => handleToggle(item)} />
                  <span className={item.zavrseno ? 'text-slate-400 line-through' : 'text-slate-700'}>
                    {item.naziv}
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
              </>
            )}
          </li>
        ))}
        {items.length === 0 && <li className="text-sm text-slate-500">Nema stavki.</li>}
      </ul>
      <form onSubmit={handleAdd} className="flex flex-wrap gap-3">
        <Input
          label="Nova stavka"
          value={naziv}
          onChange={(e) => setNaziv(e.target.value)}
          className="flex-1"
        />
        <Button type="submit" className="self-end">
          Dodaj
        </Button>
      </form>
      {error && <Alert type="error">{error}</Alert>}
      {success && <Alert type="success">{success}</Alert>}
    </section>
  )
}
