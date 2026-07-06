import { useEffect, useState } from 'react'
import userService from '../services/userService'
import useFeedback from '../hooks/useFeedback'
import { useAuth } from '../context/AuthContext.jsx'
import Button from '../components/ui/Button.jsx'
import Alert from '../components/ui/Alert.jsx'

export default function AdminUsersPage() {
  const { user: currentUser } = useAuth()
  const [users, setUsers] = useState([])
  const { error, success, showSuccess, showError } = useFeedback()

  useEffect(() => {
    load()
  }, [])

  async function load() {
    try {
      setUsers(await userService.getAll())
    } catch (err) {
      showError(err.response?.data?.poruka || 'Neuspešno učitavanje korisnika.')
    }
  }

  async function handleRemove(id) {
    await userService.remove(id)
    setUsers((prev) => prev.filter((u) => u.id !== id))
    showSuccess('Korisnik je obrisan.')
  }

  return (
    <div>
      <h1 className="mb-6 text-2xl font-semibold text-slate-900">Administracija korisnika</h1>
      {error && <Alert type="error">{error}</Alert>}
      {success && <Alert type="success">{success}</Alert>}
      <div className="overflow-x-auto rounded-2xl bg-white shadow-sm">
        <table className="w-full min-w-[560px] text-left text-sm">
          <thead className="bg-slate-50 text-slate-500">
            <tr>
              <th className="px-4 py-3">Ime</th>
              <th className="px-4 py-3">Email</th>
              <th className="px-4 py-3">Uloga</th>
              <th className="px-4 py-3" />
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id} className="border-t border-slate-100">
                <td className="px-4 py-3">{user.ime}</td>
                <td className="px-4 py-3">{user.email}</td>
                <td className="px-4 py-3">{user.uloga}</td>
                <td className="px-4 py-3 text-right">
                  {user.id === currentUser?.id ? (
                    <span className="text-xs text-slate-400">(ti)</span>
                  ) : (
                    <Button variant="danger" onClick={() => handleRemove(user.id)}>
                      Obriši
                    </Button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {users.length === 0 && <p className="p-4 text-slate-500">Nema korisnika.</p>}
      </div>
    </div>
  )
}
