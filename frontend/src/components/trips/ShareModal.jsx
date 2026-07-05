import { useEffect, useState } from 'react'
import { QRCodeSVG } from 'qrcode.react'
import shareService from '../../services/shareService'
import useFeedback from '../../hooks/useFeedback'
import Modal from '../ui/Modal.jsx'
import Button from '../ui/Button.jsx'
import Input from '../ui/Input.jsx'
import Select from '../ui/Select.jsx'
import Alert from '../ui/Alert.jsx'

export default function ShareModal({ tripId, open, onClose }) {
  const [shares, setShares] = useState([])
  const [tip, setTip] = useState('View')
  const [editEmails, setEditEmails] = useState('')
  const { error, success, showSuccess, showError } = useFeedback()

  useEffect(() => {
    if (open) {
      load()
    }
  }, [open])

  async function load() {
    try {
      setShares(await shareService.list(tripId))
    } catch (err) {
      showError(err.response?.data?.poruka || 'Neuspešno učitavanje deljenja.')
    }
  }

  async function handleCreate() {
    let emails
    if (tip === 'Edit') {
      emails = editEmails
        .split(',')
        .map((e) => e.trim())
        .filter(Boolean)

      if (emails.length === 0) {
        showError('Za EDIT link unesi bar jedan email naloga sa pravom izmene.')
        return
      }
    }

    try {
      const created = await shareService.create(tripId, tip, emails)
      setShares((prev) => [...prev, created])
      setEditEmails('')
      showSuccess('Kod za deljenje je generisan.')
    } catch (err) {
      showError(err.response?.data?.poruka || 'Kreiranje deljenja nije uspelo.')
    }
  }

  async function handleRevoke(id) {
    await shareService.revoke(tripId, id)
    setShares((prev) => prev.filter((s) => s.id !== id))
    showSuccess('Deljenje je opozvano.')
  }

  return (
    <Modal open={open} onClose={onClose} title="Podeli plan">
      <div className="space-y-4">
        <Select label="Tip pristupa" value={tip} onChange={(e) => setTip(e.target.value)}>
          <option value="View">Pregled (View)</option>
          <option value="Edit">Uređivanje (Edit)</option>
        </Select>

        {tip === 'Edit' && (
          <Input
            label="Email-ovi naloga sa pravom izmene (odvojeni zarezom)"
            placeholder="npr. pera@primer.com, mika@primer.com"
            value={editEmails}
            onChange={(e) => setEditEmails(e.target.value)}
          />
        )}

        <Button onClick={handleCreate}>Generiši kod</Button>

        {error && <Alert type="error">{error}</Alert>}
        {success && <Alert type="success">{success}</Alert>}

        <ul className="space-y-3">
          {shares.map((share) => {
            const link = `${window.location.origin}/share/${share.kod}`
            return (
              <li key={share.id} className="rounded-lg border border-slate-200 p-3">
                <p className="text-sm font-medium text-slate-700">
                  {share.tip} — kod: <span className="font-mono">{share.kod}</span>
                </p>
                {share.tip === 'Edit' && (
                  <p className="mt-1 text-xs text-slate-500">
                    Dozvoljeni nalozi: {share.dozvoljeniEmails?.join(', ') || '—'}
                  </p>
                )}
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
