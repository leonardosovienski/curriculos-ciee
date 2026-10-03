const dateTimeFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
const dateFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short' })

export function formatDateTime(value) {
  return value ? dateTimeFormatter.format(new Date(value)) : ''
}

export function formatDate(value) {
  return value ? dateFormatter.format(new Date(value)) : ''
}
