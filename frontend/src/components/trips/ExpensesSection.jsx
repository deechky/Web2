import { useEffect, useState } from 'react'
import { AgGridReact } from 'ag-grid-react'
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community'
import expenseService from '../../services/expenseService'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Select from '../ui/Select.jsx'
import Alert from '../ui/Alert.jsx'

ModuleRegistry.registerModules([AllCommunityModule])

const KATEGORIJE = ['Prevoz', 'Smestaj', 'Hrana', 'Ulaznice', 'Kupovina', 'Ostalo']

const COLUMN_DEFS = [
  { field: 'naziv', headerName: 'Naziv', flex: 1 },
  { field: 'kategorija', headerName: 'Kategorija', flex: 1 },
  { field: 'iznos', headerName: 'Iznos', flex: 1 },
  { field: 'datum', headerName: 'Datum', flex: 1 },
]

export default function ExpensesSection({ tripId, onChanged }) {
  const [items, setItems] = useState([])
  const [error, setError] = useState('')
  const [naziv, setNaziv] = useState('')
  const [kategorija, setKategorija] = useState('Ostalo')
  const [iznos, setIznos] = useState('')
  const [datum, setDatum] = useState('')

  useEffect(() => {
    load()
  }, [tripId])

  async function load() {
    try {
      setItems(await expenseService.getAll(tripId))
    } catch (err) {
      setError(err.response?.data?.poruka || 'Neuspešno učitavanje troškova.')
    }
  }

  async function handleAdd(e) {
    e.preventDefault()
    setError('')

    if (!naziv || !datum) {
      setError('Naziv i datum su obavezni.')
      return
    }
    if (Number(iznos) < 0) {
      setError('Iznos ne može biti negativan.')
      return
    }

    try {
      const created = await expenseService.create(tripId, {
        naziv,
        kategorija,
        iznos: Number(iznos) || 0,
        datum,
      })
      setItems((prev) => [...prev, created])
      setNaziv('')
      setIznos('')
      setDatum('')
      onChanged?.()
    } catch (err) {
      setError(err.response?.data?.poruka || 'Dodavanje troška nije uspelo.')
    }
  }

  return (
    <section className="rounded-2xl bg-white p-5 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-slate-900">Troškovi</h2>
      <div className="mb-4 h-64">
        <AgGridReact rowData={items} columnDefs={COLUMN_DEFS} />
      </div>
      <form onSubmit={handleAdd} className="grid grid-cols-2 gap-3">
        <Input label="Naziv" value={naziv} onChange={(e) => setNaziv(e.target.value)} />
        <Select label="Kategorija" value={kategorija} onChange={(e) => setKategorija(e.target.value)}>
          {KATEGORIJE.map((k) => (
            <option key={k} value={k}>
              {k}
            </option>
          ))}
        </Select>
        <Input
          label="Iznos"
          type="number"
          min="0"
          value={iznos}
          onChange={(e) => setIznos(e.target.value)}
        />
        <Input label="Datum" type="date" value={datum} onChange={(e) => setDatum(e.target.value)} />
        {error && (
          <div className="col-span-2">
            <Alert type="error">{error}</Alert>
          </div>
        )}
        <Button type="submit" className="col-span-2">
          Dodaj trošak
        </Button>
      </form>
    </section>
  )
}
