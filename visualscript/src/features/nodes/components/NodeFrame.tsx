import type { PropsWithChildren } from 'react'

type NodeFrameProps = PropsWithChildren<{
  label: string
  role: string
  detail: string
}>

export function NodeFrame({ label, role, detail, children }: NodeFrameProps) {
  return (
    <div className="script-node">
      {children}
      <div className="script-node-content">
        <div className="script-node-label">{label}</div>
        <div className="script-node-role">{role}</div>
        <div className="script-node-detail">{detail}</div>
      </div>
    </div>
  )
}
