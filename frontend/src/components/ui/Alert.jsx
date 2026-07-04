const styles = {
  error: 'bg-red-50 text-red-700 border-red-200',
  success: 'bg-emerald-50 text-emerald-700 border-emerald-200',
}

export default function Alert({ type = 'error', children }) {
  return (
    <div className={`rounded-lg border px-3 py-2 text-sm ${styles[type]}`}>
      {children}
    </div>
  )
}
