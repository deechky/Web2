export default function Select({ label, className = '', children, ...props }) {
  return (
    <label className="block">
      {label && <span className="mb-1 block text-sm font-medium text-slate-700">{label}</span>}
      <select
        className={`w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 outline-none transition focus:ring-2 focus:ring-teal-500 ${className}`}
        {...props}
      >
        {children}
      </select>
    </label>
  )
}
