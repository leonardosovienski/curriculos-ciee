import { useRef, useState } from 'react'
import { parseResume } from '../api.js'

const MAX_SIZE_BYTES = 5 * 1024 * 1024

// Importação opcional de currículo em PDF. Não salva nada: apenas devolve os dados
// encontrados para o formulário de cadastro, que continua editável.
export default function ResumeUpload({ onParsed, disabled }) {
  const inputRef = useRef(null)
  const [reading, setReading] = useState(false)
  const [message, setMessage] = useState(null)

  async function handleFileChange(event) {
    const file = event.target.files?.[0]
    event.target.value = '' // permite escolher o mesmo arquivo de novo
    if (!file) return

    if (!file.name.toLowerCase().endsWith('.pdf')) {
      setMessage({ type: 'error', text: 'O arquivo enviado não é um PDF válido.' })
      return
    }
    if (file.size > MAX_SIZE_BYTES) {
      setMessage({ type: 'error', text: 'O PDF deve possuir no máximo 5 MB.' })
      return
    }

    setReading(true)
    setMessage(null)
    try {
      const data = await parseResume(file)
      setMessage(onParsed(data, file.name))
    } catch (error) {
      setMessage({ type: error.status === 422 ? 'warning' : 'error', text: error.message })
    } finally {
      setReading(false)
    }
  }

  return (
    <div className="resume-upload">
      <div className="resume-upload-row">
        <div>
          <p className="resume-upload-title">Importar currículo em PDF (opcional)</p>
          <p className="field-hint">Preenche nome, e-mail e telefone quando possível. Até 5 MB.</p>
        </div>
        <button
          type="button" className="button-secondary"
          onClick={() => inputRef.current?.click()} disabled={disabled || reading}
        >
          {reading ? 'Lendo currículo…' : 'Escolher PDF'}
        </button>
        <input
          ref={inputRef} type="file" accept=".pdf,application/pdf" hidden
          onChange={handleFileChange} aria-label="Arquivo PDF do currículo"
        />
      </div>

      {message && (
        <div className={`message message-${message.type}`} role={message.type === 'error' ? 'alert' : 'status'}>
          {message.text}
        </div>
      )}
    </div>
  )
}
