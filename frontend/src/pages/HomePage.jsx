import { useCallback, useEffect, useState } from 'react'
import { listCandidates } from '../api.js'
import CandidateForm from '../components/CandidateForm.jsx'
import CandidateList from '../components/CandidateList.jsx'

export default function HomePage() {
  const [candidates, setCandidates] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const loadCandidates = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setCandidates(await listCandidates())
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadCandidates()
  }, [loadCandidates])

  return (
    <div className="home-grid">
      <section className="panel" aria-labelledby="form-title">
        <h1 id="form-title">Cadastrar candidato</h1>
        <p className="muted">Preencha os dados abaixo. Nome e e-mail são obrigatórios.</p>
        <CandidateForm onSaved={loadCandidates} />
      </section>

      <section className="panel" aria-labelledby="list-title">
        <div className="panel-heading">
          <h2 id="list-title">Candidatos cadastrados</h2>
          {!loading && !error && <span className="count">{candidates.length}</span>}
        </div>
        <CandidateList candidates={candidates} loading={loading} error={error} onRetry={loadCandidates} />
      </section>
    </div>
  )
}
