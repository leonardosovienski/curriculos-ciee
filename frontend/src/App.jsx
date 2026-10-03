import { Link, Route, Routes } from 'react-router'
import CandidateDetailsPage from './pages/CandidateDetailsPage.jsx'
import HomePage from './pages/HomePage.jsx'

export default function App() {
  return (
    <>
      <header className="app-header">
        <div className="container">
          <Link to="/" className="app-title">Banco de currículos</Link>
        </div>
      </header>

      <main className="container">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/candidates/:id" element={<CandidateDetailsPage />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
    </>
  )
}

function NotFound() {
  return (
    <section className="panel">
      <h1>Página não encontrada</h1>
      <p><Link to="/">Voltar para o cadastro</Link></p>
    </section>
  )
}
