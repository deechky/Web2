import http from '../api/http'

const userService = {
  async getAll() {
    const { data } = await http.get('/api/users')
    return data
  },

  async remove(id) {
    await http.delete(`/api/users/${id}`)
  },
}

export default userService
