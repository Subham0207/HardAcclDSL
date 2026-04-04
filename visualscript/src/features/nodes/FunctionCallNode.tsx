import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { FunctionCallFlowNode } from './types'

export function FunctionCallNode({ data }: NodeProps<FunctionCallFlowNode>) {
  return (
    <NodeFrame label={data.label} role={data.role} detail={data.detail}>
      <Handle
        id="arg0"
        type="target"
        position={Position.Left}
        className="script-handle script-handle-in"
        style={{ top: '35%' }}
        isConnectable
      />
      <Handle
        id="arg1"
        type="target"
        position={Position.Left}
        className="script-handle script-handle-in"
        style={{ top: '70%' }}
        isConnectable
      />
      <Handle id="out" type="source" position={Position.Right} className="script-handle script-handle-out" isConnectable />
    </NodeFrame>
  )
}
