import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Em desenvolvimento, o Vite encaminha /api para a API ASP.NET Core.
// Assim o frontend usa URLs relativas e o backend não precisa de CORS.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
})
