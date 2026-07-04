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
const EMPTY_FORM = { naziv: '', kategorija: 'Ostalo', iznos: '', datum: '' }

function ActionsCellRenderer(params) {
  return (
    <div className="flex gap-2 py-1">
      <Button variant="secondary" onClick={() => params.context.onEdit(params.data)}>
        Uredi
      </Button>
      <Button variant="danger" onClick={() => params.context.onRemove(params.data.id)}>
        Obriši
      </Button>
    </div>
  )
}

const COLUMN_DEFS = [
  { field: 'naziv', headerName: 'Naziv', flex: 1 },
  { field: 'kategorija', headerName: 'Kategorija', flex: 1 },
  { field: 'iznos', headerName: 'Iznos', flex: 1 },
  { field: 'datum', headerName: 'Datum', flex: 1 },
  { headerName: 'Akcije', cellRenderer: ActionsCellRenderer, flex: 1 },
]

export default function ExpensesSection({ tripId, onChanged }) {
  const [items, setItems] = useState([])
  const [error, setError] = useState('')
  const [form, setForm] = useState(EMPTY_FORM)
  const [editingId, setEditingId] = useState(null)

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

  function validate() {
    if (!form.naziv || !form.datum) {
      return 'Naziv i datum su obavezni.'
    }
    if (Number(form.iznos) < 0) {
      return 'Iznos ne može biti negativan.'
    }
    return ''
  }

  async function handleSubmit(e) {
    e.preventDefault()
    const validationError = validate()
    setError(validationError)
    if (validationError) return

    const dto = {
      naziv: form.naziv,
      kategorija: form.kategorija,
      iznos: Number(form.iznos) || 0,
      datum: form.datum,
    }

    try {
      if (editingId) {
        const updated = await expenseService.update(tripId, editingId, dto)
        setItems((prev) => prev.map((t) => (t.id === editingId ? updated : t)))
      } else {
        const created = await expenseService.create(tripId, dto)
        setItems((prev) => [...prev, created])
      }
      setForm(EMPTY_FORM)
      setEditingId(null)
      onChanged?.()
    } catch (err) {
      setError(err.response?.data?.poruka || 'Čuvanje troška nije uspelo.')
    }
  }

  function startEdit(trosak) {
    setEditingId(trosak.id)
    setForm({
      naziv: trosak.naziv,
      kategorija: trosak.kategorija,
      iznos: trosak.iznos,
      datum: trosak.datum?.slice(0, 10) || '',
    })
    setError('')
  }

  function cancelEdit() {
    setEditingId(null)
    setForm(EMPTY_FORM)
    setError('')
  }

  async function handleRemove(id) {
    await expenseService.remove(tripId, id)
    setItems((prev) => prev.filter((t) => t.id !== id))
    if (editingId === id) cancelEdit()
    onChanged?.()
  }

  return (
    <section className="rounded-2xl bg-white p-5 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-slate-900">Troškovi</h2>
      <div className="mb-4 h-64">
        <AgGridReact
          rowData={items}
          columnDefs={COLUMN_DEFS}
          context={{ onEdit: startEdit, onRemove: handleRemove }}
        />
      </div>
      <form onSubmit={handleSubmit} className="grid grid-cols-2 gap-3">
        <Input
          label="Naziv"
          value={form.naziv}
          onChange={(e) => setForm((f) => ({ ...f, naziv: e.target.value }))}
        />
        <Select
          label="Kategorija"
          value={form.kategorija}
          onChange={(e) => setForm((f) => ({ ...f, kategorija: e.target.value }))}
        >
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
          value={form.iznos}
          onChange={(e) => setForm((f) => ({ ...f, iznos: e.target.value }))}
        />
        <Input
          label="Datum"
          type="date"
          value={form.datum}
          onChange={(e) => setForm((f) => ({ ...f, datum: e.target.value }))}
        />
        {error && (
          <div className="col-span-2">
            <Alert type="error">{error}</Alert>
          </div>
        )}
        <div className="col-span-2 flex gap-3">
          <Button type="submit">{editingId ? 'Sačuvaj izmene' : 'Dodaj trošak'}</Button>
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
