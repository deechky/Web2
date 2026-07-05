import http from '../api/http'
import { Podsetnik } from '../models/trip'

const reminderService = {
  async getAll(tripId) {
    const { data } = await http.get(`/api/trips/${tripId}/reminders`)
    return data.map((r) => new Podsetnik(r))
  },

  async create(tripId, dto) {
    const { data } = await http.post(`/api/trips/${tripId}/reminders`, dto)
    return new Podsetnik(data)
  },

  async update(tripId, id, dto) {
    const { data } = await http.put(`/api/trips/${tripId}/reminders/${id}`, dto)
    return new Podsetnik(data)
  },

  async remove(tripId, id) {
    await http.delete(`/api/trips/${tripId}/reminders/${id}`)
  },
}

export default reminderService
