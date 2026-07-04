import { Link, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'
import Button from './ui/Button.jsx'

export default function Layout() {
  const { user, logout } = useAuth()

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
          <Link to="/" className="text-lg font-semibold text-teal-700">
            Travel Planner
          </Link>
          <nav className="flex items-center gap-4">
            {user?.isAdmin && (
              <Link to="/admin/users" className="text-sm text-slate-600 hover:text-teal-700">
                Admin
              </Link>
            )}
            <span className="text-sm text-slate-500">{user?.ime}</span>
            <Button variant="secondary" onClick={logout}>
              Odjavi se
            </Button>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-5xl px-4 py-6">
        <Outlet />
      </main>
    </div>
  )
}
