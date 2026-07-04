import axios from 'axios'

const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
})

http.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  const tripMatch = config.url?.match(/\/api\/trips\/([0-9a-fA-F-]{36})/)
  if (tripMatch) {
    const shareCode = sessionStorage.getItem(`shareCode:${tripMatch[1]}`)
    if (shareCode) {
      config.headers['X-Share-Code'] = shareCode
    }
  }

  return config
})

export default http
