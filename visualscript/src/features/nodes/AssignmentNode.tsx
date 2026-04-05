import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { AssignmentFlowNode } from './types'

export function AssignmentNode({ data }: NodeProps<AssignmentFlowNode>) {
  return (
    <NodeFrame label={data.label} role={data.role} detail={data.detail}>
      <Handle id="value" type="target" position={Position.Left} className="script-handle script-handle-in" isConnectable />
      <span className="script-handle-label script-handle-label-in">value</span>
      <Handle id="out" type="source" position={Position.Right} className="script-handle script-handle-out" isConnectable />
      <span className="script-handle-label script-handle-label-out">out</span>
    </NodeFrame>
  )
}
