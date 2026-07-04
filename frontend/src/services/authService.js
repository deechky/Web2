import http from '../api/http'
import { User } from '../models/User'

function mapAuthResponse(data) {
  return {
    token: data.token,
    user: new User({ ime: data.ime, email: data.email, uloga: data.uloga }),
  }
}

const authService = {
  async register({ ime, email, lozinka }) {
    const response = await http.post('/api/auth/register', { ime, email, lozinka })
    return mapAuthResponse(response.data)
  },

  async login({ email, lozinka }) {
    const response = await http.post('/api/auth/login', { email, lozinka })
    return mapAuthResponse(response.data)
  },
}

export default authService
