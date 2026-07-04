import http from '../api/http'
import { Trosak } from '../models/trip'

const expenseService = {
  async getAll(tripId) {
    const { data } = await http.get(`/api/trips/${tripId}/expenses`)
    return data.map((t) => new Trosak(t))
  },

  async create(tripId, dto) {
    const { data } = await http.post(`/api/trips/${tripId}/expenses`, dto)
    return new Trosak(data)
  },

  async update(tripId, id, dto) {
    const { data } = await http.put(`/api/trips/${tripId}/expenses/${id}`, dto)
    return new Trosak(data)
  },

  async remove(tripId, id) {
    await http.delete(`/api/trips/${tripId}/expenses/${id}`)
  },
}

export default expenseService
