import http from '../api/http'
import { ChecklistStavka } from '../models/trip'

const checklistService = {
  async getAll(tripId) {
    const { data } = await http.get(`/api/trips/${tripId}/checklist-items`)
    return data.map((c) => new ChecklistStavka(c))
  },

  async create(tripId, dto) {
    const { data } = await http.post(`/api/trips/${tripId}/checklist-items`, dto)
    return new ChecklistStavka(data)
  },

  async update(tripId, id, dto) {
    const { data } = await http.put(`/api/trips/${tripId}/checklist-items/${id}`, dto)
    return new ChecklistStavka(data)
  },

  async remove(tripId, id) {
    await http.delete(`/api/trips/${tripId}/checklist-items/${id}`)
  },
}

export default checklistService
