import axios from 'axios'

const api = axios.create({
  baseURL: process.env.VITE_API_BASE_URL || 'https://localhost:7243/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request/response interceptors will be added later by auth layer

export default api
