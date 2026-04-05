import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { NumberLiteralFlowNode } from './types'

export function NumberLiteralNode({ data }: NodeProps<NumberLiteralFlowNode>) {
  return (
    <NodeFrame
      label={data.label}
      role={data.role}
      detail={data.detail}
      rightRail={
        <div className="script-port script-port-out">
          <Handle id="out" type="source" position={Position.Right} className="script-handle" isConnectable />
          <span className="script-handle-label">out</span>
        </div>
      }
      body={
        <div className="script-node-fields">
          <label className="script-node-field">
            <span>value</span>
            <input className="script-node-input" defaultValue={data.value} placeholder="0" />
          </label>
        </div>
      }
    />
  )
}
