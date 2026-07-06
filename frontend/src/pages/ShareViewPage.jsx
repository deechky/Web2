import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import shareService from '../services/shareService'
import { useAuth } from '../context/AuthContext.jsx'
import Alert from '../components/ui/Alert.jsx'
import Button from '../components/ui/Button.jsx'

export default function ShareViewPage() {
  const { code } = useParams()
  const { token, user } = useAuth()
  const navigate = useNavigate()
  const [data, setData] = useState(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [editAllowed, setEditAllowed] = useState(null)

  const load = useCallback(async () => {
    try {
      const result = await shareService.resolve(code)
      setData(result)
      setError('')
    } catch (err) {
      const status = err.response?.status
      if (status === 404 || status === 410) {
        setData(null)
        setError(status === 410 ? 'Ovaj link je istekao.' : 'Link za deljenje nije validan ili je opozvan.')
      }
    } finally {
      setLoading(false)
    }
  }, [code])

  useEffect(() => {
    load()
    const intervalId = setInterval(load, 15000)
    window.addEventListener('focus', load)
    return () => {
      clearInterval(intervalId)
      window.removeEventListener('focus', load)
    }
  }, [load])

  useEffect(() => {
    if (data?.tip === 'Edit' && token && user?.email) {
      shareService
        .checkAccess(code, user.email)
        .then((result) => setEditAllowed(result.valid))
        .catch(() => setEditAllowed(false))
    }
  }, [data, token, user, code])

  function goToEdit() {
    sessionStorage.setItem(`shareCode:${data.planId}`, code)
    navigate(`/trips/${data.planId}`)
  }

  if (loading) {
    return <p className="p-6 text-slate-500">Učitavanje...</p>
  }

  if (error || !data) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
        <Alert type="error">{error || 'Plan nije dostupan.'}</Alert>
      </div>
    )
  }

  if (data.tip === 'Edit') {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
        <div className="w-full max-w-sm rounded-2xl bg-white p-8 text-center shadow-sm">
          <p className="mb-4 text-slate-700">Ovaj link omogućava uređivanje plana.</p>
          {!token && (
            <>
              <p className="mb-4 text-sm text-slate-500">Prijavi se da bi mogao/mogla da uređuješ.</p>
              <Link to="/login">
                <Button>Prijavi se</Button>
              </Link>
            </>
          )}
          {token && editAllowed === null && <p className="text-sm text-slate-500">Proveravam pristup...</p>}
          {token && editAllowed === true && <Button onClick={goToEdit}>Otvori plan za uređivanje</Button>}
          {token && editAllowed === false && (
            <Alert type="error">
              Tvoj nalog ({user?.email}) nema pravo izmene ovog plana. Obrati se vlasniku da te doda
              na listu.
            </Alert>
          )}
        </div>
      </div>
    )
  }

  const plan = data.plan?.plan
  const destinacije = data.plan?.destinacije || []
  const aktivnosti = data.plan?.aktivnosti || []
  const troskovi = data.plan?.troskovi || []
  const checklistStavke = data.plan?.checklistStavke || []
  const beleske = data.plan?.beleske || []
  const podsetnici = data.plan?.podsetnici || []
  const budzet = data.plan?.budzet

  return (
    <div className="min-h-screen bg-slate-50 px-4 py-8">
      <div className="mx-auto max-w-2xl space-y-6">
        <div className="rounded-2xl bg-white p-5 shadow-sm">
          <p className="mb-2 inline-block rounded-full bg-teal-50 px-3 py-1 text-xs font-medium text-teal-700">
            Samo za pregled
          </p>
          <h1 className="text-2xl font-semibold text-slate-900">{plan?.naziv}</h1>
          {plan?.opis && <p className="mt-1 text-slate-600">{plan.opis}</p>}
          <p className="mt-2 text-sm text-slate-500">
            {plan?.pocetniDatum} — {plan?.krajnjiDatum}
          </p>
        </div>

        {budzet && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <div className="rounded-2xl bg-white p-4 text-center shadow-sm">
              <p className="text-xs uppercase text-slate-400">Planirano</p>
              <p className="text-xl font-semibold text-slate-900">{budzet.planirano}</p>
            </div>
            <div className="rounded-2xl bg-white p-4 text-center shadow-sm">
              <p className="text-xs uppercase text-slate-400">Potrošeno</p>
              <p className="text-xl font-semibold text-slate-900">{budzet.potroseno}</p>
            </div>
            <div className="rounded-2xl bg-white p-4 text-center shadow-sm">
              <p className="text-xs uppercase text-slate-400">Preostalo</p>
              <p
                className={`text-xl font-semibold ${budzet.preostalo < 0 ? 'text-red-600' : 'text-emerald-600'}`}
              >
                {budzet.preostalo}
              </p>
            </div>
          </div>
        )}

        <div className="rounded-2xl bg-white p-5 shadow-sm">
          <h2 className="mb-2 text-lg font-semibold text-slate-900">Destinacije</h2>
          <ul className="space-y-1 text-sm text-slate-700">
            {destinacije.map((d) => (
              <li key={d.id}>
                {d.naziv} — {d.lokacija}
              </li>
            ))}
            {destinacije.length === 0 && <li className="text-slate-500">Nema destinacija.</li>}
          </ul>
        </div>

        <div className="rounded-2xl bg-white p-5 shadow-sm">
          <h2 className="mb-2 text-lg font-semibold text-slate-900">Aktivnosti</h2>
          <ul className="space-y-1 text-sm text-slate-700">
            {aktivnosti.map((a) => (
              <li key={a.id}>
                {a.datum} — {a.naziv} ({a.status})
              </li>
            ))}
            {aktivnosti.length === 0 && <li className="text-slate-500">Nema aktivnosti.</li>}
          </ul>
        </div>

        <div className="rounded-2xl bg-white p-5 shadow-sm">
          <h2 className="mb-2 text-lg font-semibold text-slate-900">Troškovi</h2>
          <ul className="space-y-1 text-sm text-slate-700">
            {troskovi.map((t) => (
              <li key={t.id}>
                {t.naziv} — {t.iznos} ({t.kategorija})
              </li>
            ))}
            {troskovi.length === 0 && <li className="text-slate-500">Nema troškova.</li>}
          </ul>
        </div>

        <div className="rounded-2xl bg-white p-5 shadow-sm">
          <h2 className="mb-2 text-lg font-semibold text-slate-900">Checklist</h2>
          <ul className="space-y-1 text-sm text-slate-700">
            {checklistStavke.map((c) => (
              <li key={c.id}>
                {c.zavrseno ? '✓' : '○'} {c.naziv}
              </li>
            ))}
            {checklistStavke.length === 0 && <li className="text-slate-500">Nema stavki.</li>}
          </ul>
        </div>

        <div className="rounded-2xl bg-white p-5 shadow-sm">
          <h2 className="mb-2 text-lg font-semibold text-slate-900">Beleške</h2>
          <ul className="space-y-2 text-sm text-slate-700">
            {beleske.map((b) => (
              <li key={b.id}>
                <p className="font-medium text-slate-800">{b.naslov}</p>
                <p className="whitespace-pre-wrap break-words text-slate-600">{b.sadrzaj}</p>
              </li>
            ))}
            {beleske.length === 0 && <li className="text-slate-500">Nema beleški.</li>}
          </ul>
        </div>

        <div className="rounded-2xl bg-white p-5 shadow-sm">
          <h2 className="mb-2 text-lg font-semibold text-slate-900">Podsetnici</h2>
          <ul className="space-y-1 text-sm text-slate-700">
            {podsetnici.map((r) => (
              <li key={r.id}>
                <span className={r.zavrseno ? 'text-slate-400 line-through' : ''}>
                  {r.datum} — {r.naziv}
                </span>
              </li>
            ))}
            {podsetnici.length === 0 && <li className="text-slate-500">Nema podsetnika.</li>}
          </ul>
        </div>
      </div>
    </div>
  )
}
