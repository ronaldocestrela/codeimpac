import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 1597,
    allowedHosts: ['a449-2804-7d74-9c-6600-3919-65b0-be8e-c1df.ngrok-free.app']
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/setupTests.ts'
  }
})
