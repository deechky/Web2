import http from '../api/http'
import { Budget } from '../models/trip'

const budgetService = {
  async get(tripId) {
    const { data } = await http.get(`/api/trips/${tripId}/budget`)
    return new Budget(data)
  },
}

export default budgetService
