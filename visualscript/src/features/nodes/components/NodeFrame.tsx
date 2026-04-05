import type { ReactNode } from 'react'

type NodeFrameProps = {
  label: string
  role: string
  detail?: string
  leftRail?: ReactNode
  rightRail?: ReactNode
  body?: ReactNode
}

export function NodeFrame({ label, role, detail, leftRail, rightRail, body }: NodeFrameProps) {
  return (
    <div className="script-node">
      <div className="script-node-layout">
        <div className="script-node-rail script-node-rail-left">{leftRail}</div>

        <div className="script-node-content">
          <div className="script-node-label">{label}</div>
          <div className="script-node-role">{role}</div>
          {detail ? <div className="script-node-detail">{detail}</div> : null}
          {body}
        </div>

        <div className="script-node-rail script-node-rail-right">{rightRail}</div>
      </div>
    </div>
  )
}
