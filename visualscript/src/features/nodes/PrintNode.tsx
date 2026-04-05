import { Handle, Position, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { PrintFlowNode } from './types'

export function PrintNode({ data }: NodeProps<PrintFlowNode>) {
  return (
    <NodeFrame
      label={data.label}
      detail={data.detail}
      leftRail={
        <>
          <div className="script-port script-port-in script-port-exec">
            <Handle id="exec-in" type="target" position={Position.Left} className="script-handle script-handle-exec" isConnectable />
            <span className="script-handle-label script-handle-label-exec">exec</span>
          </div>
          <div className="script-port script-port-in">
            <Handle id="value" type="target" position={Position.Left} className="script-handle" isConnectable />
            <span className="script-handle-label">value</span>
          </div>
        </>
      }
      rightRail={
        <div className="script-port script-port-out script-port-exec">
          <Handle id="exec-out" type="source" position={Position.Right} className="script-handle script-handle-exec" isConnectable />
          <span className="script-handle-label script-handle-label-exec">exec</span>
        </div>
      }
    />
  )
}
