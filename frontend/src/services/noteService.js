import http from '../api/http'
import { Beleska } from '../models/trip'

const noteService = {
  async getAll(tripId) {
    const { data } = await http.get(`/api/trips/${tripId}/notes`)
    return data.map((b) => new Beleska(b))
  },

  async create(tripId, dto) {
    const { data } = await http.post(`/api/trips/${tripId}/notes`, dto)
    return new Beleska(data)
  },

  async update(tripId, id, dto) {
    const { data } = await http.put(`/api/trips/${tripId}/notes/${id}`, dto)
    return new Beleska(data)
  },

  async remove(tripId, id) {
    await http.delete(`/api/trips/${tripId}/notes/${id}`)
  },
}

export default noteService
