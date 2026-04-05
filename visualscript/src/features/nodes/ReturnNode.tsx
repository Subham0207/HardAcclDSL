import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { ReturnFlowNode } from './types'

export function ReturnNode({ data }: NodeProps<ReturnFlowNode>) {
  return (
    <NodeFrame
      label={data.label}
      detail={data.detail}
      leftRail={
        <div className="script-port script-port-in">
          <Handle id="value" type="target" position={Position.Left} className="script-handle" isConnectable />
          <span className="script-handle-label">value</span>
        </div>
      }
    />
  )
}
