import { Link } from 'react-router'
import { formatDate } from '../format.js'

export default function CandidateList({ candidates, loading, error, onRetry }) {
  if (loading) {
    return <p className="muted">Carregando candidatos…</p>
  }

  if (error) {
    return (
      <div className="message message-error" role="alert">
        {error}{' '}
        <button type="button" className="link-button" onClick={onRetry}>Tentar novamente</button>
      </div>
    )
  }

  if (candidates.length === 0) {
    return <p className="empty-state">Nenhum candidato cadastrado ainda. Use o formulário para cadastrar o primeiro.</p>
  }

  return (
    <div className="table-wrapper">
      <table className="candidate-table">
        <thead>
          <tr>
            <th scope="col">Nome</th>
            <th scope="col">E-mail</th>
            <th scope="col">Área ou cargo</th>
            <th scope="col">Cadastrado em</th>
          </tr>
        </thead>
        <tbody>
          {candidates.map((candidate) => (
            <tr key={candidate.id}>
              <td><Link to={`/candidates/${candidate.id}`}>{candidate.fullName}</Link></td>
              <td>{candidate.email}</td>
              <td>{candidate.desiredPosition || <span className="muted">Não informado</span>}</td>
              <td className="nowrap">{formatDate(candidate.createdAt)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
