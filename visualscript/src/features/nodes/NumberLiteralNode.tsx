import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { NumberLiteralFlowNode } from './types'

export function NumberLiteralNode({ data }: NodeProps<NumberLiteralFlowNode>) {
  return (
    <NodeFrame label={data.label} role={data.role} detail={data.detail}>
      <Handle id="out" type="source" position={Position.Right} className="script-handle script-handle-out" isConnectable />
      <span className="script-handle-label script-handle-label-out">out</span>
    </NodeFrame>
  )
}
