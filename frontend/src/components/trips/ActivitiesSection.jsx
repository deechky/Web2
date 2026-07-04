import { useEffect, useState } from 'react'
import activityService from '../../services/activityService'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Select from '../ui/Select.jsx'
import Alert from '../ui/Alert.jsx'
import CalendarView from './CalendarView.jsx'

const STATUSI = ['Planirano', 'Rezervisano', 'Zavrseno', 'Otkazano']

export default function ActivitiesSection({ tripId, onChanged }) {
  const [items, setItems] = useState([])
  const [error, setError] = useState('')
  const [naziv, setNaziv] = useState('')
  const [datum, setDatum] = useState('')
  const [vreme, setVreme] = useState('')
  const [procenjeniTrosak, setProcenjeniTrosak] = useState('')
  const [status, setStatus] = useState('Planirano')

  useEffect(() => {
    load()
  }, [tripId])

  async function load() {
    try {
      setItems(await activityService.getAll(tripId))
    } catch (err) {
      setError(err.response?.data?.poruka || 'Neuspešno učitavanje aktivnosti.')
    }
  }

  async function handleAdd(e) {
    e.preventDefault()
    setError('')

    if (!naziv || !datum) {
      setError('Naziv i datum su obavezni.')
      return
    }
    if (Number(procenjeniTrosak) < 0) {
      setError('Procenjeni trošak ne može biti negativan.')
      return
    }

    try {
      const created = await activityService.create(tripId, {
        naziv,
        datum,
        vreme: vreme || null,
        procenjeniTrosak: Number(procenjeniTrosak) || 0,
        status,
      })
      setItems((prev) => [...prev, created])
      setNaziv('')
      setDatum('')
      setVreme('')
      setProcenjeniTrosak('')
      setStatus('Planirano')
      onChanged?.()
    } catch (err) {
      setError(err.response?.data?.poruka || 'Dodavanje aktivnosti nije uspelo.')
    }
  }

  return (
    <section className="rounded-2xl bg-white p-5 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-slate-900">Aktivnosti (po danima)</h2>
      <div className="mb-4">
        <CalendarView activities={items} />
      </div>
      <form onSubmit={handleAdd} className="grid grid-cols-2 gap-3">
        <Input label="Naziv" value={naziv} onChange={(e) => setNaziv(e.target.value)} />
        <Input label="Datum" type="date" value={datum} onChange={(e) => setDatum(e.target.value)} />
        <Input
          label="Vreme (opciono)"
          type="time"
          value={vreme}
          onChange={(e) => setVreme(e.target.value)}
        />
        <Input
          label="Procenjeni trošak"
          type="number"
          min="0"
          value={procenjeniTrosak}
          onChange={(e) => setProcenjeniTrosak(e.target.value)}
        />
        <Select label="Status" value={status} onChange={(e) => setStatus(e.target.value)}>
          {STATUSI.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </Select>
        {error && (
          <div className="col-span-2">
            <Alert type="error">{error}</Alert>
          </div>
        )}
        <Button type="submit" className="col-span-2">
          Dodaj aktivnost
        </Button>
      </form>
    </section>
  )
}
