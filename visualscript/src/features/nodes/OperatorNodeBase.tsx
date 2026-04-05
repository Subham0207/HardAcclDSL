import { Handle, Position, type Node, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { OperatorNodeData } from './types'

type OperatorNodeBaseProps = NodeProps<Node<OperatorNodeData>>

export function OperatorNodeBase({ data }: OperatorNodeBaseProps) {
  return (
    <NodeFrame
      label={data.label}
      role={data.role}
      detail={data.detail}
      leftRail={
        <>
          <div className="script-port script-port-in">
            <Handle id="left" type="target" position={Position.Left} className="script-handle" isConnectable />
            <span className="script-handle-label">left</span>
          </div>
          <div className="script-port script-port-in">
            <Handle id="right" type="target" position={Position.Left} className="script-handle" isConnectable />
            <span className="script-handle-label">right</span>
          </div>
        </>
      }
      rightRail={
        <div className="script-port script-port-out">
          <Handle id="out" type="source" position={Position.Right} className="script-handle" isConnectable />
          <span className="script-handle-label">out</span>
        </div>
      }
      body={<div className="script-node-operator">{data.operatorSymbol}</div>}
    />
  )
}
