import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { LocalDeclarationFlowNode } from './types'

export function LocalDeclarationNode({ data }: NodeProps<LocalDeclarationFlowNode>) {
  return (
    <NodeFrame label={data.label} role={data.role} detail={data.detail}>
      <Handle id="value" type="target" position={Position.Left} className="script-handle script-handle-in" isConnectable />
      <Handle id="out" type="source" position={Position.Right} className="script-handle script-handle-out" isConnectable />
    </NodeFrame>
  )
}
