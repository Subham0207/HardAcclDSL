import { Handle, Position, useReactFlow, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { NumberLiteralFlowNode, ScriptFlowNode } from './types'

export function NumberLiteralNode({ id, data }: NodeProps<NumberLiteralFlowNode>) {
  const { setNodes } = useReactFlow<ScriptFlowNode>()

  const updateData = (patch: Partial<NumberLiteralFlowNode['data']>) => {
    setNodes((nodes) =>
      nodes.map((node) => (node.id === id ? ({ ...node, data: { ...node.data, ...patch } } as ScriptFlowNode) : node)),
    )
  }

  return (
    <NodeFrame
      label={data.label}
      detail={data.detail}
      rightRail={
        <div className="script-port script-port-out">
          <Handle id="out" type="source" position={Position.Right} className="script-handle" isConnectable />
          <span className="script-handle-label">out</span>
        </div>
      }
      body={
        <div className="script-node-fields">
          <label className="script-node-field">
            <span>value</span>
            <input
              className="script-node-input"
              value={data.value}
              onChange={(event) => updateData({ value: event.target.value })}
              placeholder="0"
            />
          </label>
        </div>
      }
    />
  )
}
