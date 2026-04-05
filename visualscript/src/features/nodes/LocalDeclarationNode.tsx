import { Handle, Position, useReactFlow, type NodeProps } from '@xyflow/react'
import { NodeFrame } from './components/NodeFrame'
import type { LocalDeclarationFlowNode, ScriptFlowNode } from './types'

export function LocalDeclarationNode({ id, data }: NodeProps<LocalDeclarationFlowNode>) {
  const { setNodes } = useReactFlow<ScriptFlowNode>()

  const updateData = (patch: Partial<LocalDeclarationFlowNode['data']>) => {
    setNodes((nodes) =>
      nodes.map((node) => (node.id === id ? ({ ...node, data: { ...node.data, ...patch } } as ScriptFlowNode) : node)),
    )
  }

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
        <>
          <div className="script-port script-port-out script-port-exec">
            <Handle id="exec-out" type="source" position={Position.Right} className="script-handle script-handle-exec" isConnectable />
            <span className="script-handle-label script-handle-label-exec">exec</span>
          </div>
          <div className="script-port script-port-out">
            <Handle id="out" type="source" position={Position.Right} className="script-handle" isConnectable />
            <span className="script-handle-label">out</span>
          </div>
        </>
      }
      body={
        <div className="script-node-fields">
          <label className="script-node-field">
            <span>name</span>
            <input
              className="script-node-input"
              value={data.variableName}
              onChange={(event) => updateData({ variableName: event.target.value })}
              placeholder="result"
            />
          </label>
          <label className="script-node-field">
            <span>value</span>
            <input
              className="script-node-input"
              value={data.initialValue}
              onChange={(event) => updateData({ initialValue: event.target.value })}
              placeholder="0"
            />
          </label>
        </div>
      }
    />
  )
}
