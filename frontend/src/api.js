// Cliente HTTP mínimo sobre fetch.
// Erros da API seguem ProblemDetails / ValidationProblemDetails (RFC 9457).

export class ApiError extends Error {
  constructor(status, message, fieldErrors = {}) {
    super(message)
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

const NETWORK_ERROR = 'Não foi possível conectar ao servidor. Verifique se a API está em execução.'
const UNEXPECTED_ERROR = 'Erro inesperado. Tente novamente.'

async function request(path, options = {}) {
  let response
  try {
    response = await fetch(path, {
      ...options,
      headers: { Accept: 'application/json', ...options.headers },
    })
  } catch {
    throw new ApiError(0, NETWORK_ERROR)
  }

  const body = await readJson(response)

  if (!response.ok) {
    throw toApiError(response.status, body)
  }

  return body
}

async function readJson(response) {
  const text = await response.text()
  if (!text) return null
  try {
    return JSON.parse(text)
  } catch {
    return null
  }
}

function toApiError(status, problem) {
  // O ASP.NET devolve as chaves de "errors" com o nome da propriedade C# (ex.: "FullName").
  // Normalizamos para o nome usado no formulário (ex.: "fullName").
  const fieldErrors = {}
  for (const [key, messages] of Object.entries(problem?.errors ?? {})) {
    if (!/^[A-Za-z]/.test(key)) continue
    const field = key.charAt(0).toLowerCase() + key.slice(1)
    fieldErrors[field] = Array.isArray(messages) ? messages[0] : String(messages)
  }

  let message
  if (status === 400 && Object.keys(fieldErrors).length > 0) {
    message = 'Corrija os campos destacados.'
  } else if (problem?.detail) {
    message = problem.detail
  } else if (status === 413) {
    // Arquivo muito acima do limite pode ser barrado pelo servidor antes da API, sem corpo JSON.
    message = 'O PDF deve possuir no máximo 5 MB.'
  } else {
    message = UNEXPECTED_ERROR
  }

  return new ApiError(status, message, fieldErrors)
}

export function listCandidates() {
  return request('/api/candidates')
}

export function getCandidate(id) {
  return request(`/api/candidates/${encodeURIComponent(id)}`)
}

export function createCandidate(candidate) {
  return request('/api/candidates', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(candidate),
  })
}

export function parseResume(file) {
  const form = new FormData()
  form.append('file', file)
  // Sem Content-Type manual: o navegador define multipart/form-data com o boundary correto.
  return request('/api/resumes/parse', { method: 'POST', body: form })
}
