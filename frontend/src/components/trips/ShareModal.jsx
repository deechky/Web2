import { useEffect, useState } from 'react'
import { QRCodeSVG } from 'qrcode.react'
import shareService from '../../services/shareService'
import Modal from '../ui/Modal.jsx'
import Button from '../ui/Button.jsx'
import Select from '../ui/Select.jsx'
import Alert from '../ui/Alert.jsx'

export default function ShareModal({ tripId, open, onClose }) {
  const [shares, setShares] = useState([])
  const [tip, setTip] = useState('View')
  const [error, setError] = useState('')

  useEffect(() => {
    if (open) {
      load()
    }
  }, [open])

  async function load() {
    try {
      setShares(await shareService.list(tripId))
    } catch (err) {
      setError(err.response?.data?.poruka || 'Neuspešno učitavanje deljenja.')
    }
  }

  async function handleCreate() {
    setError('')
    try {
      const created = await shareService.create(tripId, tip)
      setShares((prev) => [...prev, created])
    } catch (err) {
      setError(err.response?.data?.poruka || 'Kreiranje deljenja nije uspelo.')
    }
  }

  async function handleRevoke(id) {
    await shareService.revoke(tripId, id)
    setShares((prev) => prev.filter((s) => s.id !== id))
  }

  return (
    <Modal open={open} onClose={onClose} title="Podeli plan">
      <div className="space-y-4">
        <div className="flex items-end gap-3">
          <Select label="Tip pristupa" value={tip} onChange={(e) => setTip(e.target.value)}>
            <option value="View">Pregled (View)</option>
            <option value="Edit">Uređivanje (Edit)</option>
          </Select>
          <Button onClick={handleCreate}>Generiši kod</Button>
        </div>
        {error && <Alert type="error">{error}</Alert>}

        <ul className="space-y-3">
          {shares.map((share) => {
            const link = `${window.location.origin}/share/${share.kod}`
            return (
              <li key={share.id} className="rounded-lg border border-slate-200 p-3">
                <p className="text-sm font-medium text-slate-700">
                  {share.tip} — kod: <span className="font-mono">{share.kod}</span>
                </p>
                <p className="mt-1 break-all text-xs text-slate-500">{link}</p>
                <div className="mt-2 flex items-center justify-between">
                  <QRCodeSVG value={link} size={80} />
                  <Button variant="danger" onClick={() => handleRevoke(share.id)}>
                    Opozovi
                  </Button>
                </div>
              </li>
            )
          })}
          {shares.length === 0 && <li className="text-sm text-slate-500">Nema aktivnih deljenja.</li>}
        </ul>
      </div>
    </Modal>
  )
}
