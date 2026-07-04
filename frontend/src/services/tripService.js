import http from '../api/http'
import { Plan } from '../models/trip'

const tripService = {
  async getAll() {
    const { data } = await http.get('/api/trips')
    return data.map((p) => new Plan(p))
  },

  async getById(id) {
    const { data } = await http.get(`/api/trips/${id}`)
    return new Plan(data)
  },

  async create(dto) {
    const { data } = await http.post('/api/trips', dto)
    return new Plan(data)
  },

  async update(id, dto) {
    const { data } = await http.put(`/api/trips/${id}`, dto)
    return new Plan(data)
  },

  async remove(id) {
    await http.delete(`/api/trips/${id}`)
  },
}

export default tripService
