import { useCallback, useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import tripService from '../services/tripService'
import budgetService from '../services/budgetService'
import useFeedback from '../hooks/useFeedback'
import Alert from '../components/ui/Alert.jsx'
import Button from '../components/ui/Button.jsx'
import Input from '../components/ui/Input.jsx'
import DestinationsSection from '../components/trips/DestinationsSection.jsx'
import ActivitiesSection from '../components/trips/ActivitiesSection.jsx'
import ExpensesSection from '../components/trips/ExpensesSection.jsx'
import ChecklistSection from '../components/trips/ChecklistSection.jsx'
import ShareModal from '../components/trips/ShareModal.jsx'

export default function TripDetailPage() {
  const { id } = useParams()
  const [plan, setPlan] = useState(null)
  const [budget, setBudget] = useState(null)
  const [error, setError] = useState('')
  const [shareOpen, setShareOpen] = useState(false)
  const [editing, setEditing] = useState(false)
  const [form, setForm] = useState(null)
  const { error: formError, success, showSuccess, showError: showFormError } = useFeedback()

  const loadPlan = useCallback(async () => {
    try {
      setPlan(await tripService.getById(id))
    } catch (err) {
      setError(err.response?.data?.poruka || 'Neuspešno učitavanje plana.')
    }
  }, [id])

  const loadBudget = useCallback(async () => {
    try {
      setBudget(await budgetService.get(id))
    } catch {
      setBudget(null)
    }
  }, [id])

  useEffect(() => {
    loadPlan()
    loadBudget()
  }, [loadPlan, loadBudget])

  function startEdit() {
    setForm({
      naziv: plan.naziv,
      opis: plan.opis || '',
      pocetniDatum: plan.pocetniDatum?.slice(0, 10) || '',
      krajnjiDatum: plan.krajnjiDatum?.slice(0, 10) || '',
      planiraniBudzet: plan.planiraniBudzet,
      napomene: plan.napomene || '',
    })
    setEditing(true)
  }

  async function handleSave(e) {
    e.preventDefault()

    if (!form.naziv || !form.pocetniDatum || !form.krajnjiDatum) {
      showFormError('Naziv i datumi su obavezni.')
      return
    }
    if (new Date(form.krajnjiDatum) < new Date(form.pocetniDatum)) {
      showFormError('Krajnji datum ne može biti pre početnog.')
      return
    }
    if (Number(form.planiraniBudzet) < 0) {
      showFormError('Budžet ne može biti negativan.')
      return
    }

    try {
      const updated = await tripService.update(id, {
        ...form,
        planiraniBudzet: Number(form.planiraniBudzet) || 0,
      })
      setPlan(updated)
      setEditing(false)
      loadBudget()
      showSuccess('Plan je uspešno izmenjen.')
    } catch (err) {
      showFormError(err.response?.data?.poruka || 'Čuvanje izmena nije uspelo.')
    }
  }

  if (error) {
    return <Alert type="error">{error}</Alert>
  }

  if (!plan) {
    return <p className="text-slate-500">Učitavanje...</p>
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <Link to="/" className="text-sm text-teal-600 hover:underline">
          ← Nazad na planove
        </Link>
        <Button variant="secondary" onClick={() => setShareOpen(true)}>
          Podeli
        </Button>
      </div>

      <div className="rounded-2xl bg-white p-4 shadow-sm sm:p-5">
        {editing ? (
          <form onSubmit={handleSave} className="space-y-3">
            <Input
              label="Naziv"
              value={form.naziv}
              onChange={(e) => setForm((f) => ({ ...f, naziv: e.target.value }))}
            />
            <Input
              label="Opis"
              value={form.opis}
              onChange={(e) => setForm((f) => ({ ...f, opis: e.target.value }))}
            />
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <Input
                label="Početni datum"
                type="date"
                value={form.pocetniDatum}
                onChange={(e) => setForm((f) => ({ ...f, pocetniDatum: e.target.value }))}
              />
              <Input
                label="Krajnji datum"
                type="date"
                value={form.krajnjiDatum}
                onChange={(e) => setForm((f) => ({ ...f, krajnjiDatum: e.target.value }))}
              />
            </div>
            <Input
              label="Planirani budžet"
              type="number"
              min="0"
              value={form.planiraniBudzet}
              onChange={(e) => setForm((f) => ({ ...f, planiraniBudzet: e.target.value }))}
            />
            <Input
              label="Napomene"
              value={form.napomene}
              onChange={(e) => setForm((f) => ({ ...f, napomene: e.target.value }))}
            />
            {formError && <Alert type="error">{formError}</Alert>}
            <div className="flex flex-wrap gap-3">
              <Button type="submit">Sačuvaj izmene</Button>
              <Button type="button" variant="secondary" onClick={() => setEditing(false)}>
                Otkaži
              </Button>
            </div>
          </form>
        ) : (
          <>
            <div className="flex flex-wrap items-start justify-between gap-2">
              <h1 className="text-2xl font-semibold text-slate-900">{plan.naziv}</h1>
              <Button variant="secondary" onClick={startEdit}>
                Uredi plan
              </Button>
            </div>
            {plan.opis && <p className="mt-1 text-slate-600">{plan.opis}</p>}
            <p className="mt-2 text-sm text-slate-500">
              {plan.pocetniDatum} — {plan.krajnjiDatum}
            </p>
            {plan.napomene && <p className="mt-2 text-sm text-slate-500">Napomene: {plan.napomene}</p>}
            {success && (
              <div className="mt-3">
                <Alert type="success">{success}</Alert>
              </div>
            )}
          </>
        )}
      </div>

      <ShareModal tripId={id} open={shareOpen} onClose={() => setShareOpen(false)} />

      {budget && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <div className="rounded-2xl bg-white p-4 text-center shadow-sm">
            <p className="text-xs uppercase text-slate-400">Planirano</p>
            <p className="text-xl font-semibold text-slate-900">{budget.planirano}</p>
          </div>
          <div className="rounded-2xl bg-white p-4 text-center shadow-sm">
            <p className="text-xs uppercase text-slate-400">Potrošeno</p>
            <p className="text-xl font-semibold text-slate-900">{budget.potroseno}</p>
          </div>
          <div className="rounded-2xl bg-white p-4 text-center shadow-sm">
            <p className="text-xs uppercase text-slate-400">Preostalo</p>
            <p className={`text-xl font-semibold ${budget.preostalo < 0 ? 'text-red-600' : 'text-emerald-600'}`}>
              {budget.preostalo}
            </p>
          </div>
        </div>
      )}

      <DestinationsSection tripId={id} />
      <ActivitiesSection tripId={id} onChanged={loadBudget} />
      <ExpensesSection tripId={id} onChanged={loadBudget} />
      <ChecklistSection tripId={id} />
    </div>
  )
}
