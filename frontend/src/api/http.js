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

// Istekla/nevažeća sesija -> očisti nalog i vodi na /login. Ne diramo /login ni /share
// (javna stranica deljenja) da ne bismo napravili petlju preusmeravanja.
http.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error?.response?.status === 401) {
      const path = window.location.pathname
      if (!path.startsWith('/login') && !path.startsWith('/share')) {
        localStorage.removeItem('token')
        localStorage.removeItem('user')
        window.location.assign('/login')
      }
    }
    return Promise.reject(error)
  },
)

export default http
