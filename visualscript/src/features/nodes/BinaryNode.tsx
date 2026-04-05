import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { BinaryFlowNode } from './types'

export function BinaryNode({ data }: NodeProps<BinaryFlowNode>) {
  return (
    <NodeFrame label={data.label} role={data.role} detail={data.detail}>
      <Handle
        id="left"
        type="target"
        position={Position.Left}
        className="script-handle script-handle-in"
        style={{ top: '35%' }}
        isConnectable
      />
      <span className="script-handle-label script-handle-label-in" style={{ top: '35%' }}>
        left
      </span>
      <Handle
        id="right"
        type="target"
        position={Position.Left}
        className="script-handle script-handle-in"
        style={{ top: '70%' }}
        isConnectable
      />
      <span className="script-handle-label script-handle-label-in" style={{ top: '70%' }}>
        right
      </span>
      <Handle id="out" type="source" position={Position.Right} className="script-handle script-handle-out" isConnectable />
      <span className="script-handle-label script-handle-label-out">out</span>
    </NodeFrame>
  )
}
