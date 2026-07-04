const variants = {
  primary: 'bg-teal-600 text-white hover:bg-teal-700',
  secondary: 'bg-white text-teal-700 border border-teal-600 hover:bg-teal-50',
  danger: 'bg-red-600 text-white hover:bg-red-700',
}

export default function Button({ children, variant = 'primary', className = '', ...props }) {
  return (
    <button
      className={`rounded-lg px-4 py-2 font-medium transition disabled:cursor-not-allowed disabled:opacity-50 ${variants[variant]} ${className}`}
      {...props}
    >
      {children}
    </button>
  )
}
