import { useCallback, useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import tripService from '../services/tripService'
import budgetService from '../services/budgetService'
import Alert from '../components/ui/Alert.jsx'
import DestinationsSection from '../components/trips/DestinationsSection.jsx'
import ActivitiesSection from '../components/trips/ActivitiesSection.jsx'
import ExpensesSection from '../components/trips/ExpensesSection.jsx'
import ChecklistSection from '../components/trips/ChecklistSection.jsx'

export default function TripDetailPage() {
  const { id } = useParams()
  const [plan, setPlan] = useState(null)
  const [budget, setBudget] = useState(null)
  const [error, setError] = useState('')

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

  if (error) {
    return <Alert type="error">{error}</Alert>
  }

  if (!plan) {
    return <p className="text-slate-500">Učitavanje...</p>
  }

  return (
    <div className="space-y-6">
      <Link to="/" className="text-sm text-teal-600 hover:underline">
        ← Nazad na planove
      </Link>

      <div className="rounded-2xl bg-white p-5 shadow-sm">
        <h1 className="text-2xl font-semibold text-slate-900">{plan.naziv}</h1>
        {plan.opis && <p className="mt-1 text-slate-600">{plan.opis}</p>}
        <p className="mt-2 text-sm text-slate-500">
          {plan.pocetniDatum} — {plan.krajnjiDatum}
        </p>
        {plan.napomene && <p className="mt-2 text-sm text-slate-500">Napomene: {plan.napomene}</p>}
      </div>

      {budget && (
        <div className="grid grid-cols-3 gap-4">
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
