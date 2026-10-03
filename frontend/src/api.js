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

  const message =
    status === 400 && Object.keys(fieldErrors).length > 0
      ? 'Corrija os campos destacados.'
      : problem?.detail || (status === 0 || status >= 500 ? UNEXPECTED_ERROR : problem?.title) || UNEXPECTED_ERROR

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
