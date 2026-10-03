import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router'
import { getCandidate } from '../api.js'
import { formatDateTime } from '../format.js'

export default function CandidateDetailsPage() {
  const { id } = useParams()
  const [state, setState] = useState({ status: 'loading' })

  useEffect(() => {
    let ignore = false

    if (!/^\d+$/.test(id)) {
      setState({ status: 'not-found' })
      return
    }

    setState({ status: 'loading' })
    getCandidate(id)
      .then((candidate) => !ignore && setState({ status: 'loaded', candidate }))
      .catch((error) => {
        if (ignore) return
        setState(error.status === 404 ? { status: 'not-found' } : { status: 'error', message: error.message })
      })

    return () => {
      ignore = true
    }
  }, [id])

  return (
    <section className="panel details">
      <p><Link to="/">Voltar para o cadastro</Link></p>

      {state.status === 'loading' && <p className="muted">Carregando candidato…</p>}

      {state.status === 'not-found' && (
        <>
          <h1>Candidato não encontrado</h1>
          <p className="muted">O candidato solicitado não existe ou o endereço está incorreto.</p>
        </>
      )}

      {state.status === 'error' && <div className="message message-error" role="alert">{state.message}</div>}

      {state.status === 'loaded' && <CandidateDetails candidate={state.candidate} />}
    </section>
  )
}

function CandidateDetails({ candidate }) {
  return (
    <>
      <h1>{candidate.fullName}</h1>
      <p className="muted">Cadastrado em {formatDateTime(candidate.createdAt)}</p>

      <dl className="details-list">
        <Item label="E-mail" value={candidate.email} />
        <Item label="Telefone" value={candidate.phone} />
        <Item label="Área ou cargo de interesse" value={candidate.desiredPosition} />
        <Item label="Resumo profissional" value={candidate.professionalSummary} multiline />
      </dl>
    </>
  )
}

function Item({ label, value, multiline }) {
  return (
    <div className="details-item">
      <dt>{label}</dt>
      <dd className={multiline ? 'multiline' : undefined}>
        {value || <span className="muted">Não informado</span>}
      </dd>
    </div>
  )
}
