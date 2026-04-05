import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { AssignmentFlowNode } from './types'

export function AssignmentNode({ data }: NodeProps<AssignmentFlowNode>) {
  return (
    <NodeFrame
      label={data.label}
      role={data.role}
      detail={data.detail}
      leftRail={
        <>
          <div className="script-port script-port-in">
            <Handle id="target" type="target" position={Position.Left} className="script-handle" isConnectable />
            <span className="script-handle-label">target</span>
          </div>
          <div className="script-port script-port-in">
            <Handle id="value" type="target" position={Position.Left} className="script-handle" isConnectable />
            <span className="script-handle-label">value</span>
          </div>
        </>
      }
    />
  )
}
