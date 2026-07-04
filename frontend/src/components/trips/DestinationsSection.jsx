import { useEffect, useState } from 'react'
import destinationService from '../../services/destinationService'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Alert from '../ui/Alert.jsx'

export default function DestinationsSection({ tripId }) {
  const [items, setItems] = useState([])
  const [error, setError] = useState('')
  const [naziv, setNaziv] = useState('')
  const [lokacija, setLokacija] = useState('')
  const [datumDolaska, setDatumDolaska] = useState('')
  const [datumOdlaska, setDatumOdlaska] = useState('')

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

  async function handleAdd(e) {
    e.preventDefault()
    setError('')

    if (!naziv || !lokacija || !datumDolaska || !datumOdlaska) {
      setError('Sva polja su obavezna.')
      return
    }
    if (new Date(datumDolaska) > new Date(datumOdlaska)) {
      setError('Datum dolaska ne može biti posle datuma odlaska.')
      return
    }

    try {
      const created = await destinationService.create(tripId, {
        naziv,
        lokacija,
        datumDolaska,
        datumOdlaska,
      })
      setItems((prev) => [...prev, created])
      setNaziv('')
      setLokacija('')
      setDatumDolaska('')
      setDatumOdlaska('')
    } catch (err) {
      setError(err.response?.data?.poruka || 'Dodavanje destinacije nije uspelo.')
    }
  }

  async function handleRemove(id) {
    await destinationService.remove(tripId, id)
    setItems((prev) => prev.filter((d) => d.id !== id))
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
            <Button variant="danger" onClick={() => handleRemove(d.id)}>
              Obriši
            </Button>
          </li>
        ))}
        {items.length === 0 && <li className="text-sm text-slate-500">Nema unesenih destinacija.</li>}
      </ul>
      <form onSubmit={handleAdd} className="grid grid-cols-2 gap-3">
        <Input label="Naziv" value={naziv} onChange={(e) => setNaziv(e.target.value)} />
        <Input label="Lokacija" value={lokacija} onChange={(e) => setLokacija(e.target.value)} />
        <Input
          label="Datum dolaska"
          type="date"
          value={datumDolaska}
          onChange={(e) => setDatumDolaska(e.target.value)}
        />
        <Input
          label="Datum odlaska"
          type="date"
          value={datumOdlaska}
          onChange={(e) => setDatumOdlaska(e.target.value)}
        />
        {error && (
          <div className="col-span-2">
            <Alert type="error">{error}</Alert>
          </div>
        )}
        <Button type="submit" className="col-span-2">
          Dodaj destinaciju
        </Button>
      </form>
    </section>
  )
}
