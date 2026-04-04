import { useCallback } from 'react'
import {
  Background,
  ConnectionMode,
  Controls,
  MiniMap,
  ReactFlow,
  addEdge,
  useEdgesState,
  useNodesState,
  type Connection,
} from '@xyflow/react'
import { defaultEdgeOptions } from './defaultEdgeOptions'
import { initialEdges, initialNodes } from './initialGraph'
import { scriptNodeTypes } from '../nodes/nodeTypes'

export function GraphCanvas() {
  const [nodes, , onNodesChange] = useNodesState(initialNodes)
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges)

  const onConnect = useCallback(
    (connection: Connection) => {
      setEdges((current) => addEdge({ ...connection, animated: true }, current))
    },
    [setEdges],
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
      fitView
    >
      <Background gap={16} size={1} />
      <Controls />
      <MiniMap pannable zoomable />
    </ReactFlow>
  )
}
