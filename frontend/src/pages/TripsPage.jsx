import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useTrips } from '../context/TripContext.jsx'
import useFeedback from '../hooks/useFeedback'
import destinationService from '../services/destinationService'
import activityService from '../services/activityService'
import expenseService from '../services/expenseService'
import checklistService from '../services/checklistService'
import budgetService from '../services/budgetService'
import { generateTripPdf } from '../utils/pdfReport'
import Button from '../components/ui/Button.jsx'
import Input from '../components/ui/Input.jsx'
import Alert from '../components/ui/Alert.jsx'

export default function TripsPage() {
  const { trips, loading, error, createTrip, removeTrip } = useTrips()
  const { success, showSuccess, error: formError, showError: showFormError } = useFeedback()
  const [downloadingId, setDownloadingId] = useState(null)
  const [showForm, setShowForm] = useState(false)
  const [naziv, setNaziv] = useState('')
  const [pocetniDatum, setPocetniDatum] = useState('')
  const [krajnjiDatum, setKrajnjiDatum] = useState('')
  const [planiraniBudzet, setPlaniraniBudzet] = useState('')

  async function handleCreate(e) {
    e.preventDefault()

    if (!naziv || !pocetniDatum || !krajnjiDatum) {
      showFormError('Naziv i datumi su obavezni.')
      return
    }
    if (new Date(krajnjiDatum) < new Date(pocetniDatum)) {
      showFormError('Krajnji datum ne može biti pre početnog.')
      return
    }
    if (Number(planiraniBudzet) < 0) {
      showFormError('Budžet ne može biti negativan.')
      return
    }

    try {
      await createTrip({
        naziv,
        pocetniDatum,
        krajnjiDatum,
        planiraniBudzet: Number(planiraniBudzet) || 0,
      })
      setNaziv('')
      setPocetniDatum('')
      setKrajnjiDatum('')
      setPlaniraniBudzet('')
      setShowForm(false)
      showSuccess('Plan je uspešno kreiran.')
    } catch (err) {
      showFormError(err.response?.data?.poruka || 'Kreiranje plana nije uspelo.')
    }
  }

  async function handleRemove(id) {
    await removeTrip(id)
    showSuccess('Plan je obrisan.')
  }

  async function handleDownloadPdf(trip) {
    setDownloadingId(trip.id)
    try {
      const [destinacije, aktivnosti, troskovi, checklistStavke, budget] = await Promise.all([
        destinationService.getAll(trip.id),
        activityService.getAll(trip.id),
        expenseService.getAll(trip.id),
        checklistService.getAll(trip.id),
        budgetService.get(trip.id),
      ])
      generateTripPdf({ plan: trip, destinacije, aktivnosti, troskovi, checklistStavke, budget })
      showSuccess('PDF izveštaj je preuzet.')
    } catch (err) {
      showFormError(err.response?.data?.poruka || 'Preuzimanje PDF izveštaja nije uspelo.')
    } finally {
      setDownloadingId(null)
    }
  }

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-slate-900">Moji planovi putovanja</h1>
        <Button onClick={() => setShowForm((v) => !v)}>
          {showForm ? 'Otkaži' : 'Novi plan'}
        </Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 space-y-4 rounded-2xl bg-white p-6 shadow-sm">
          <Input label="Naziv" value={naziv} onChange={(e) => setNaziv(e.target.value)} />
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Početni datum"
              type="date"
              value={pocetniDatum}
              onChange={(e) => setPocetniDatum(e.target.value)}
            />
            <Input
              label="Krajnji datum"
              type="date"
              value={krajnjiDatum}
              onChange={(e) => setKrajnjiDatum(e.target.value)}
            />
          </div>
          <Input
            label="Planirani budžet"
            type="number"
            min="0"
            value={planiraniBudzet}
            onChange={(e) => setPlaniraniBudzet(e.target.value)}
          />
          {formError && <Alert type="error">{formError}</Alert>}
          <Button type="submit">Sačuvaj</Button>
        </form>
      )}

      {error && <Alert type="error">{error}</Alert>}
      {success && <Alert type="success">{success}</Alert>}
      {loading && <p className="text-slate-500">Učitavanje...</p>}

      <div className="grid gap-4 sm:grid-cols-2">
        {trips.map((trip) => (
          <div key={trip.id} className="rounded-2xl bg-white p-5 shadow-sm">
            <Link to={`/trips/${trip.id}`} className="text-lg font-medium text-teal-700 hover:underline">
              {trip.naziv}
            </Link>
            <p className="mt-1 text-sm text-slate-500">
              {trip.pocetniDatum} — {trip.krajnjiDatum}
            </p>
            <p className="mt-1 text-sm text-slate-500">Budžet: {trip.planiraniBudzet}</p>
            <div className="mt-3 flex gap-2">
              <Button
                variant="secondary"
                disabled={downloadingId === trip.id}
                onClick={() => handleDownloadPdf(trip)}
              >
                {downloadingId === trip.id ? 'Priprema...' : 'Preuzmi PDF'}
              </Button>
              <Button variant="danger" onClick={() => handleRemove(trip.id)}>
                Obriši
              </Button>
            </div>
          </div>
        ))}
        {!loading && trips.length === 0 && (
          <p className="text-slate-500">Nemaš još nijedan plan putovanja.</p>
        )}
      </div>
    </div>
  )
}
