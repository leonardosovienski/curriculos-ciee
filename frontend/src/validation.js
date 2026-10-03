// Validação de UX. O backend é a fonte de verdade e repete estas regras
// (Curriculos.Api/Models/CandidateRules.cs) com os mesmos limites e a mesma expressão de e-mail.

export const LIMITS = {
  fullName: 150,
  email: 254,
  phone: 30,
  desiredPosition: 150,
  professionalSummary: 2000,
}

export const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export const EMPTY_CANDIDATE = {
  fullName: '',
  email: '',
  phone: '',
  desiredPosition: '',
  professionalSummary: '',
}

export function normalizeCandidate(values) {
  const trimmed = {}
  for (const [key, value] of Object.entries(values)) {
    trimmed[key] = typeof value === 'string' ? value.trim() : value
  }
  return trimmed
}

export function validateCandidate(values) {
  const errors = {}

  if (!values.fullName) errors.fullName = 'Informe o nome completo.'
  if (!values.email) errors.email = 'Informe o e-mail.'
  else if (!EMAIL_PATTERN.test(values.email)) errors.email = 'Informe um e-mail válido.'

  for (const [field, max] of Object.entries(LIMITS)) {
    if (!errors[field] && values[field] && values[field].length > max) {
      errors[field] = `Use no máximo ${max} caracteres.`
    }
  }

  return errors
}
