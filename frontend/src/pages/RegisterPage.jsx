import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'
import Input from '../components/ui/Input.jsx'
import Button from '../components/ui/Button.jsx'
import Alert from '../components/ui/Alert.jsx'

export default function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [ime, setIme] = useState('')
  const [email, setEmail] = useState('')
  const [lozinka, setLozinka] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')

    if (!ime || !email || !lozinka) {
      setError('Ime, email i lozinka su obavezni.')
      return
    }
    if (!/^\S+@\S+\.\S+$/.test(email)) {
      setError('Unesi ispravan email.')
      return
    }
    if (lozinka.length < 6) {
      setError('Lozinka mora imati bar 6 karaktera.')
      return
    }

    setLoading(true)
    try {
      await register({ ime, email, lozinka })
      navigate('/')
    } catch (err) {
      setError(err.response?.data?.poruka || 'Registracija nije uspela.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-sm">
        <h1 className="mb-6 text-2xl font-semibold text-slate-900">Registracija</h1>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input label="Ime" value={ime} onChange={(e) => setIme(e.target.value)} />
          <Input
            label="Email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <Input
            label="Lozinka"
            type="password"
            value={lozinka}
            onChange={(e) => setLozinka(e.target.value)}
          />
          {error && <Alert type="error">{error}</Alert>}
          <Button type="submit" disabled={loading} className="w-full">
            {loading ? 'Kreiranje naloga...' : 'Registruj se'}
          </Button>
        </form>
        <p className="mt-4 text-center text-sm text-slate-500">
          Već imaš nalog?{' '}
          <Link to="/login" className="text-teal-600 hover:underline">
            Prijavi se
          </Link>
        </p>
      </div>
    </div>
  )
}
