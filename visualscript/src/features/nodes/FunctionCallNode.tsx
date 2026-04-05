import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { FunctionCallFlowNode } from './types'

export function FunctionCallNode({ data }: NodeProps<FunctionCallFlowNode>) {
  return (
    <NodeFrame
      label={data.label}
      detail={data.detail}
      leftRail={
        <>
          <div className="script-port script-port-in">
            <Handle id="arg0" type="target" position={Position.Left} className="script-handle" isConnectable />
            <span className="script-handle-label">arg0</span>
          </div>
          <div className="script-port script-port-in">
            <Handle id="arg1" type="target" position={Position.Left} className="script-handle" isConnectable />
            <span className="script-handle-label">arg1</span>
          </div>
        </>
      }
      rightRail={
        <div className="script-port script-port-out">
          <Handle id="out" type="source" position={Position.Right} className="script-handle" isConnectable />
          <span className="script-handle-label">out</span>
        </div>
      }
    />
  )
}
