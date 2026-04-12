import basicSsl from '@vitejs/plugin-basic-ssl'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), basicSsl()],
  server: {
    port: parseInt(process.env.VITE_DEV_PORT || '5173'),
    proxy: {
      '/api': {
        target: process.env.API_PROXY_TARGET || 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
      },
      '/health': {
        target: process.env.API_PROXY_TARGET || 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
      },
      '/metrics': {
        target: process.env.API_PROXY_TARGET || 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
      },
      '/swagger': {
        target: process.env.API_PROXY_TARGET || 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
