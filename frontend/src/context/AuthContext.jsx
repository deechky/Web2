import { createContext, useContext, useEffect, useState } from 'react'
import authService from '../services/authService'
import { User } from '../models/User'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [token, setToken] = useState(null)

  useEffect(() => { 
    const storedToken = localStorage.getItem('token')
    const storedUser = localStorage.getItem('user')
    if (storedToken && storedUser) {
      setToken(storedToken)
      setUser(new User(JSON.parse(storedUser)))
    }
  }, [])

  function persist(authResponse) {
    localStorage.setItem('token', authResponse.token)
    localStorage.setItem('user', JSON.stringify(authResponse.user))
    setToken(authResponse.token)
    setUser(authResponse.user)
  }

  async function login(dto) {
    const authResponse = await authService.login(dto)
    persist(authResponse)
    return authResponse.user
  }

  async function register(dto) {
    const authResponse = await authService.register(dto)
    persist(authResponse)
    return authResponse.user
  }

  function logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setToken(null)
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, token, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}
