import { forwardRef, useCallback, useImperativeHandle, useRef } from 'react'
import {
  Background,
  ConnectionMode,
  Controls,
  MarkerType,
  MiniMap,
  ReactFlow,
  type ReactFlowInstance,
  type Edge,
  addEdge,
  useEdgesState,
  useNodesState,
  type Connection,
} from '@xyflow/react'
import { defaultEdgeOptions } from './defaultEdgeOptions'
import { createStarterNode, initialEdges, initialNodes, type StarterNodeType } from './initialGraph'
import { scriptNodeTypes } from '../nodes/nodeTypes'
import type { ScriptFlowNode } from '../nodes/types'
import { buildGraphSnapshot, type GraphSnapshot } from './graphSnapshot'

export type GraphCanvasHandle = {
  addNodeAtViewportCenter: (type: StarterNodeType) => void
  exportGraphSnapshot: () => GraphSnapshot
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

  const isExecHandle = useCallback((handleId: string | null | undefined) => handleId?.startsWith('exec-') ?? false, [])

  const isConnectionAllowed = useCallback(
    (connection: Connection | Edge) => {
      if (!connection.sourceHandle || !connection.targetHandle) {
        return false
      }

      return isExecHandle(connection.sourceHandle) === isExecHandle(connection.targetHandle)
    },
    [isExecHandle],
  )

  const onConnect = useCallback(
    (connection: Connection) => {
      if (!isConnectionAllowed(connection)) {
        return
      }

      const isExecutionEdge =
        connection.sourceHandle?.startsWith('exec-') && connection.targetHandle?.startsWith('exec-')

      if (isExecutionEdge && connection.source && connection.target) {
        const executionEdge: Edge = {
          id: `exec-${connection.source}-${connection.sourceHandle ?? 'none'}-${connection.target}-${connection.targetHandle ?? 'none'}-${Date.now()}`,
          source: connection.source,
          target: connection.target,
          sourceHandle: connection.sourceHandle,
          targetHandle: connection.targetHandle,
          type: 'smoothstep',
          animated: true,
          style: { stroke: '#2b8a3e', strokeWidth: 3, strokeDasharray: '6 4' },
          markerEnd: {
            type: MarkerType.ArrowClosed,
            color: '#2b8a3e',
          },
        }

        setEdges((current) => addEdge(executionEdge, current))
        return
      }

      setEdges((current) => addEdge({ ...connection, animated: true }, current))
    },
    [isConnectionAllowed, setEdges],
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
      exportGraphSnapshot: () => buildGraphSnapshot(nodes, edges),
    }),
    [edges, nodes, setNodes, viewportRef],
  )

  return (
    <ReactFlow
      nodes={nodes}
      edges={edges}
      onNodesChange={onNodesChange}
      onEdgesChange={onEdgesChange}
      onConnect={onConnect}
      isValidConnection={isConnectionAllowed}
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
