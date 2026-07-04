import http from '../api/http'
import { Aktivnost } from '../models/trip'

const activityService = {
  async getAll(tripId, date) {
    const { data } = await http.get(`/api/trips/${tripId}/activities`, {
      params: date ? { date } : undefined,
    })
    return data.map((a) => new Aktivnost(a))
  },

  async create(tripId, dto) {
    const { data } = await http.post(`/api/trips/${tripId}/activities`, dto)
    return new Aktivnost(data)
  },

  async update(tripId, id, dto) {
    const { data } = await http.put(`/api/trips/${tripId}/activities/${id}`, dto)
    return new Aktivnost(data)
  },

  async remove(tripId, id) {
    await http.delete(`/api/trips/${tripId}/activities/${id}`)
  },
}

export default activityService
