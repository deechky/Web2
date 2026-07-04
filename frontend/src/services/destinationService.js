import http from '../api/http'
import { Destinacija } from '../models/trip'

const destinationService = {
  async getAll(tripId) {
    const { data } = await http.get(`/api/trips/${tripId}/destinations`)
    return data.map((d) => new Destinacija(d))
  },

  async create(tripId, dto) {
    const { data } = await http.post(`/api/trips/${tripId}/destinations`, dto)
    return new Destinacija(data)
  },

  async update(tripId, id, dto) {
    const { data } = await http.put(`/api/trips/${tripId}/destinations/${id}`, dto)
    return new Destinacija(data)
  },

  async remove(tripId, id) {
    await http.delete(`/api/trips/${tripId}/destinations/${id}`)
  },
}

export default destinationService
