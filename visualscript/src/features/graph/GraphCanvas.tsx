import { forwardRef, useCallback, useImperativeHandle, useRef } from 'react'
import {
  Background,
  ConnectionMode,
  Controls,
  MiniMap,
  ReactFlow,
  type ReactFlowInstance,
  addEdge,
  useEdgesState,
  useNodesState,
  type Connection,
} from '@xyflow/react'
import { defaultEdgeOptions } from './defaultEdgeOptions'
import { createStarterNode, initialEdges, initialNodes, type StarterNodeType } from './initialGraph'
import { scriptNodeTypes } from '../nodes/nodeTypes'
import type { ScriptFlowNode } from '../nodes/types'

export type GraphCanvasHandle = {
  addNodeAtViewportCenter: (type: StarterNodeType) => void
}

type GraphCanvasProps = {
  viewportRef: React.RefObject<HTMLDivElement | null>
}

export const GraphCanvas = forwardRef<GraphCanvasHandle, GraphCanvasProps>(function GraphCanvas(
  { viewportRef },
  ref,
) {
  const [nodes, setNodes, onNodesChange] = useNodesState(initialNodes)
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges)
  const flowRef = useRef<ReactFlowInstance<ScriptFlowNode> | null>(null)

  const onConnect = useCallback(
    (connection: Connection) => {
      setEdges((current) => addEdge({ ...connection, animated: true }, current))
    },
    [setEdges],
  )

  useImperativeHandle(
    ref,
    () => ({
      addNodeAtViewportCenter: (type: StarterNodeType) => {
        const pane = viewportRef.current
        const flow = flowRef.current
        if (!pane || !flow) {
          return
        }

        const rect = pane.getBoundingClientRect()
        const center = flow.screenToFlowPosition({
          x: rect.left + rect.width / 2,
          y: rect.top + rect.height / 2,
        })

        const id = `${type}-${Date.now()}-${Math.floor(Math.random() * 1000)}`
        const node = createStarterNode(type, { x: center.x - 80, y: center.y - 38 }, id)
        setNodes((current) => [...current, node])
      },
    }),
    [setNodes, viewportRef],
  )

  return (
    <ReactFlow
      nodes={nodes}
      edges={edges}
      onNodesChange={onNodesChange}
      onEdgesChange={onEdgesChange}
      onConnect={onConnect}
      nodeTypes={scriptNodeTypes}
      connectionMode={ConnectionMode.Loose}
      nodesConnectable
      elementsSelectable
      edgesFocusable
      deleteKeyCode={['Backspace', 'Delete']}
      defaultEdgeOptions={defaultEdgeOptions}
      connectionLineStyle={{ stroke: '#1f5ea0', strokeWidth: 2.2 }}
      onInit={(instance) => {
        flowRef.current = instance
      }}
      fitView
    >
      <Background gap={16} size={1} />
      <Controls />
      <MiniMap pannable zoomable />
    </ReactFlow>
  )
})
