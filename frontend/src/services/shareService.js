import http from '../api/http'

const shareService = {
  async create(tripId, tip, emails) {
    const { data } = await http.post(`/api/trips/${tripId}/shares`, { tip, emails })
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

  async checkAccess(code, email) {
    const { data } = await http.get(`/api/shares/${code}/validate`, { params: { email } })
    return data
  },
}

export default shareService
