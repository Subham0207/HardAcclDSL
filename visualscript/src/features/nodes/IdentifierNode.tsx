import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { IdentifierFlowNode } from './types'

export function IdentifierNode({ data }: NodeProps<IdentifierFlowNode>) {
  return (
    <NodeFrame
      label={data.label}
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
            <span>name</span>
            <input className="script-node-input" defaultValue={data.variableName} placeholder="myVar" />
          </label>
        </div>
      }
    />
  )
}
