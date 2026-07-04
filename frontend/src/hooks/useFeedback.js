import { useEffect, useState } from 'react'

export default function useFeedback() {
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    if (!success) return undefined
    const timer = setTimeout(() => setSuccess(''), 3000)
    return () => clearTimeout(timer)
  }, [success])

  function showSuccess(message) {
    setError('')
    setSuccess(message)
  }

  function showError(message) {
    setSuccess('')
    setError(message)
  }

  return { error, success, showSuccess, showError }
}
