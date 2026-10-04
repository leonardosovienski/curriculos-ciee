import { useState } from 'react'
import { Link } from 'react-router'
import { createCandidate } from '../api.js'
import { EMPTY_CANDIDATE, LIMITS, normalizeCandidate, validateCandidate } from '../validation.js'
import ResumeUpload from './ResumeUpload.jsx'

const PARSED_FIELDS = { fullName: 'nome', email: 'e-mail', phone: 'telefone' }

// Formulário único de cadastro, usado tanto no fluxo manual quanto no fluxo com PDF:
// a importação apenas preenche estes mesmos campos, e as mesmas validações valem no salvamento.
export default function CandidateForm({ onSaved }) {
  const [values, setValues] = useState(EMPTY_CANDIDATE)
  const [errors, setErrors] = useState({})
  const [status, setStatus] = useState(null)
  const [saving, setSaving] = useState(false)
  const [uploadKey, setUploadKey] = useState(0)

  function handleChange(event) {
    const { name, value } = event.target
    setValues((current) => ({ ...current, [name]: value }))
    if (errors[name]) {
      setErrors(({ [name]: _removed, ...rest }) => rest)
    }
  }

  function handleClear() {
    setValues(EMPTY_CANDIDATE)
    setErrors({})
    setStatus(null)
    setUploadKey((key) => key + 1) // limpa também a mensagem da importação
  }

  // Preenche apenas campos vazios: a importação nunca apaga o que a pessoa já digitou.
  function applyParsedResume(data) {
    setStatus(null)
    const found = Object.keys(PARSED_FIELDS).filter((field) => data[field])
    const filled = found.filter((field) => !values[field].trim())

    if (filled.length > 0) {
      setValues((current) => {
        const next = { ...current }
        for (const field of filled) next[field] = data[field]
        return next
      })
      setErrors((current) => {
        const next = { ...current }
        for (const field of filled) delete next[field]
        return next
      })
    }

    if (found.length === 0) {
      return { type: 'warning', text: 'Nenhum dado identificado no PDF. Preencha o formulário manualmente.' }
    }
    if (filled.length === 0) {
      return { type: 'info', text: 'Os dados encontrados no PDF não foram aplicados porque esses campos já estavam preenchidos.' }
    }
    const names = filled.map((field) => PARSED_FIELDS[field]).join(', ')
    return { type: 'info', text: `Preenchemos: ${names}. Revise e complete os dados antes de salvar.` }
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setStatus(null)

    const candidate = normalizeCandidate(values)
    const clientErrors = validateCandidate(candidate)
    if (Object.keys(clientErrors).length > 0) {
      setErrors(clientErrors)
      focusField(Object.keys(clientErrors)[0])
      return
    }

    setSaving(true)
    try {
      const saved = await createCandidate(candidate)
      setValues(EMPTY_CANDIDATE)
      setErrors({})
      setStatus({ type: 'success', message: 'Candidato cadastrado com sucesso.', candidateId: saved.id })
      onSaved?.(saved)
    } catch (error) {
      setErrors(error.fieldErrors ?? {})
      setStatus({ type: 'error', message: error.message })
      const firstField = Object.keys(error.fieldErrors ?? {})[0]
      if (firstField) focusField(firstField)
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className="candidate-form" onSubmit={handleSubmit} noValidate>
      <ResumeUpload key={uploadKey} onParsed={applyParsedResume} disabled={saving} />

      <Field name="fullName" label="Nome completo" required error={errors.fullName}>
        <input
          id="field-fullName" name="fullName" type="text" autoComplete="name"
          maxLength={LIMITS.fullName} value={values.fullName} onChange={handleChange}
          {...invalidProps('fullName', errors)}
        />
      </Field>

      <Field name="email" label="E-mail" required error={errors.email}>
        <input
          id="field-email" name="email" type="email" autoComplete="email" inputMode="email"
          maxLength={LIMITS.email} value={values.email} onChange={handleChange}
          {...invalidProps('email', errors)}
        />
      </Field>

      <div className="field-row">
        <Field name="phone" label="Telefone" error={errors.phone}>
          <input
            id="field-phone" name="phone" type="tel" autoComplete="tel"
            maxLength={LIMITS.phone} value={values.phone} onChange={handleChange}
            {...invalidProps('phone', errors)}
          />
        </Field>

        <Field name="desiredPosition" label="Área ou cargo de interesse" error={errors.desiredPosition}>
          <input
            id="field-desiredPosition" name="desiredPosition" type="text"
            maxLength={LIMITS.desiredPosition} value={values.desiredPosition} onChange={handleChange}
            {...invalidProps('desiredPosition', errors)}
          />
        </Field>
      </div>

      <Field
        name="professionalSummary" label="Resumo profissional" error={errors.professionalSummary}
        hint={`${values.professionalSummary.length}/${LIMITS.professionalSummary}`}
      >
        <textarea
          id="field-professionalSummary" name="professionalSummary" rows={5}
          maxLength={LIMITS.professionalSummary} value={values.professionalSummary} onChange={handleChange}
          {...invalidProps('professionalSummary', errors)}
        />
      </Field>

      {status && (
        <div className={`message message-${status.type}`} role={status.type === 'error' ? 'alert' : 'status'}>
          {status.message}{' '}
          {status.candidateId && <Link to={`/candidates/${status.candidateId}`}>Ver detalhes</Link>}
        </div>
      )}

      <div className="form-actions">
        <button type="submit" className="button-primary" disabled={saving}>
          {saving ? 'Salvando…' : 'Salvar candidato'}
        </button>
        <button type="button" className="button-secondary" onClick={handleClear} disabled={saving}>
          Limpar formulário
        </button>
      </div>
    </form>
  )
}

function Field({ name, label, required, hint, error, children }) {
  return (
    <div className="field">
      <div className="field-label">
        <label htmlFor={`field-${name}`}>
          {label} {required ? <span className="required">(obrigatório)</span> : <span className="optional">(opcional)</span>}
        </label>
        {hint && <span className="field-hint">{hint}</span>}
      </div>
      {children}
      {error && <p className="field-error" id={`error-${name}`}>{error}</p>}
    </div>
  )
}

function invalidProps(name, errors) {
  return errors[name] ? { 'aria-invalid': true, 'aria-describedby': `error-${name}` } : {}
}

function focusField(name) {
  document.getElementById(`field-${name}`)?.focus()
}
