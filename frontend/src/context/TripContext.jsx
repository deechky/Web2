import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import tripService from '../services/tripService'
import { useAuth } from './AuthContext.jsx'

const TripContext = createContext(null)

export function TripProvider({ children }) {
  const { token } = useAuth()
  const [trips, setTrips] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const refresh = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = await tripService.getAll()
      setTrips(data)
    } catch (err) {
      setError(err.response?.data?.poruka || 'Neuspešno učitavanje planova.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    if (token) {
      refresh()
    } else {
      setTrips([])
    }
  }, [token, refresh])

  async function createTrip(dto) {
    const trip = await tripService.create(dto)
    setTrips((prev) => [...prev, trip])
    return trip
  }

  async function updateTrip(id, dto) {
    const trip = await tripService.update(id, dto)
    setTrips((prev) => prev.map((t) => (t.id === id ? trip : t)))
    return trip
  }

  async function removeTrip(id) {
    await tripService.remove(id)
    setTrips((prev) => prev.filter((t) => t.id !== id))
  }

  return (
    <TripContext.Provider value={{ trips, loading, error, refresh, createTrip, updateTrip, removeTrip }}>
      {children}
    </TripContext.Provider>
  )
}

export function useTrips() {
  return useContext(TripContext)
}
