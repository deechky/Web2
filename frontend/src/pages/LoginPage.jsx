import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'
import Input from '../components/ui/Input.jsx'
import Button from '../components/ui/Button.jsx'
import Alert from '../components/ui/Alert.jsx'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [lozinka, setLozinka] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')

    if (!email || !lozinka) {
      setError('Email i lozinka su obavezni.')
      return
    }

    setLoading(true)
    try {
      await login({ email, lozinka })
      navigate('/')
    } catch (err) {
      setError(err.response?.data?.poruka || 'Prijava nije uspela.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-sm">
        <h1 className="mb-6 text-2xl font-semibold text-slate-900">Prijava</h1>
        <form onSubmit={handleSubmit} className="space-y-4">
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
            {loading ? 'Prijavljivanje...' : 'Prijavi se'}
          </Button>
        </form>
        <p className="mt-4 text-center text-sm text-slate-500">
          Nemaš nalog?{' '}
          <Link to="/register" className="text-teal-600 hover:underline">
            Registruj se
          </Link>
        </p>
      </div>
    </div>
  )
}
