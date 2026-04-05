import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { ReturnFlowNode } from './types'

export function ReturnNode({ data }: NodeProps<ReturnFlowNode>) {
  return (
    <NodeFrame label={data.label} role={data.role} detail={data.detail}>
      <Handle id="value" type="target" position={Position.Left} className="script-handle script-handle-in" isConnectable />
      <span className="script-handle-label script-handle-label-in">value</span>
    </NodeFrame>
  )
}
