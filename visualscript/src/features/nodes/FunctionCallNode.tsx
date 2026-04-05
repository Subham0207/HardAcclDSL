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
      <span className="script-handle-label script-handle-label-in" style={{ top: '35%' }}>
        arg0
      </span>
      <Handle
        id="arg1"
        type="target"
        position={Position.Left}
        className="script-handle script-handle-in"
        style={{ top: '70%' }}
        isConnectable
      />
      <span className="script-handle-label script-handle-label-in" style={{ top: '70%' }}>
        arg1
      </span>
      <Handle id="out" type="source" position={Position.Right} className="script-handle script-handle-out" isConnectable />
      <span className="script-handle-label script-handle-label-out">out</span>
    </NodeFrame>
  )
}
