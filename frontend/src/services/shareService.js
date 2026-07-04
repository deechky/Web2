import http from '../api/http'

const shareService = {
  async create(tripId, tip) {
    const { data } = await http.post(`/api/trips/${tripId}/shares`, { tip })
    return data
  },

  async list(tripId) {
    const { data } = await http.get(`/api/trips/${tripId}/shares`)
    return data
  },

  async revoke(tripId, id) {
    await http.delete(`/api/trips/${tripId}/shares/${id}`)
  },

  async resolve(code) {
    const { data } = await http.get(`/api/shares/${code}`)
    return data
  },
}

export default shareService
