import { useEffect, useState } from 'react'
import checklistService from '../../services/checklistService'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Alert from '../ui/Alert.jsx'

export default function ChecklistSection({ tripId }) {
  const [items, setItems] = useState([])
  const [error, setError] = useState('')
  const [naziv, setNaziv] = useState('')

  useEffect(() => {
    load()
  }, [tripId])

  async function load() {
    try {
      setItems(await checklistService.getAll(tripId))
    } catch (err) {
      setError(err.response?.data?.poruka || 'Neuspešno učitavanje checklist-e.')
    }
  }

  async function handleAdd(e) {
    e.preventDefault()
    setError('')

    if (!naziv) {
      setError('Naziv stavke je obavezan.')
      return
    }

    try {
      const created = await checklistService.create(tripId, { naziv })
      setItems((prev) => [...prev, created])
      setNaziv('')
    } catch (err) {
      setError(err.response?.data?.poruka || 'Dodavanje stavke nije uspelo.')
    }
  }

  async function handleToggle(item) {
    const updated = await checklistService.update(tripId, item.id, {
      naziv: item.naziv,
      zavrseno: !item.zavrseno,
    })
    setItems((prev) => prev.map((i) => (i.id === item.id ? updated : i)))
  }

  async function handleRemove(id) {
    await checklistService.remove(tripId, id)
    setItems((prev) => prev.filter((i) => i.id !== id))
  }

  return (
    <section className="rounded-2xl bg-white p-5 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-slate-900">Checklist / Packing lista</h2>
      <ul className="mb-4 space-y-2">
        {items.map((item) => (
          <li key={item.id} className="flex items-center justify-between rounded-lg bg-slate-50 px-3 py-2">
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={item.zavrseno} onChange={() => handleToggle(item)} />
              <span className={item.zavrseno ? 'text-slate-400 line-through' : 'text-slate-700'}>
                {item.naziv}
              </span>
            </label>
            <Button variant="danger" onClick={() => handleRemove(item.id)}>
              Obriši
            </Button>
          </li>
        ))}
        {items.length === 0 && <li className="text-sm text-slate-500">Nema stavki.</li>}
      </ul>
      <form onSubmit={handleAdd} className="flex gap-3">
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
    </section>
  )
}
