import { useCallback, useMemo } from 'react'
import {
  Background,
  Controls,
  MiniMap,
  ReactFlow,
  addEdge,
  useEdgesState,
  useNodesState,
  type Connection,
  type Edge,
  type Node,
  type NodeProps,
} from '@xyflow/react'
import '@xyflow/react/dist/style.css'
import './App.css'

type ScriptNodeData = {
  label: string
  role: string
  detail: string
}

function ScriptNode({ data }: NodeProps<Node<ScriptNodeData>>) {
  return (
    <div className="script-node">
      <div className="script-node-label">{data.label}</div>
      <div className="script-node-role">{data.role}</div>
      <div className="script-node-detail">{data.detail}</div>
    </div>
  )
}

const initialNodes: Array<Node<ScriptNodeData>> = [
  {
    id: 'localDecl-1',
    type: 'scriptNode',
    position: { x: 80, y: 40 },
    data: {
      label: 'LocalDeclaration',
      role: 'Statement',
      detail: 'local result = value',
    },
  },
  {
    id: 'binary-1',
    type: 'scriptNode',
    position: { x: 430, y: 40 },
    data: {
      label: 'Binary',
      role: 'Expression',
      detail: 'left + right',
    },
  },
  {
    id: 'number-1',
    type: 'scriptNode',
    position: { x: 760, y: 10 },
    data: {
      label: 'NumberLiteral',
      role: 'Expression',
      detail: '2',
    },
  },
  {
    id: 'identifier-1',
    type: 'scriptNode',
    position: { x: 760, y: 140 },
    data: {
      label: 'Identifier',
      role: 'Expression',
      detail: 'inputA',
    },
  },
  {
    id: 'assign-1',
    type: 'scriptNode',
    position: { x: 80, y: 240 },
    data: {
      label: 'Assignment',
      role: 'Statement',
      detail: 'result = value',
    },
  },
  {
    id: 'call-1',
    type: 'scriptNode',
    position: { x: 430, y: 240 },
    data: {
      label: 'FunctionCall',
      role: 'AstNode',
      detail: 'print(result)',
    },
  },
  {
    id: 'return-1',
    type: 'scriptNode',
    position: { x: 80, y: 430 },
    data: {
      label: 'Return',
      role: 'Statement',
      detail: 'return result',
    },
  },
]

const initialEdges: Edge[] = [
  { id: 'e1', source: 'localDecl-1', target: 'binary-1', label: 'value' },
  { id: 'e2', source: 'binary-1', target: 'number-1', label: 'left' },
  { id: 'e3', source: 'binary-1', target: 'identifier-1', label: 'right' },
  { id: 'e4', source: 'assign-1', target: 'call-1', label: 'value' },
  { id: 'e5', source: 'return-1', target: 'identifier-1', label: 'value' },
]

function App() {
  const [nodes, , onNodesChange] = useNodesState(initialNodes)
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges)

  const onConnect = useCallback(
    (connection: Connection) => {
      setEdges((current) => addEdge({ ...connection, animated: true }, current))
    },
    [setEdges],
  )

  const nodeTypes = useMemo(() => ({ scriptNode: ScriptNode }), [])

  return (
    <div className="app-shell">
      <header className="app-header">
        <h1>Visual Script Prototype</h1>
        <p>Starter graph for AST-oriented Lua authoring. Drag and connect nodes to experiment.</p>
      </header>

      <div className="canvas-shell">
        <div className="legend">
          <h2>Starter Nodes</h2>
          <ul>
            <li>LocalDeclaration</li>
            <li>Assignment</li>
            <li>Return</li>
            <li>FunctionCall</li>
            <li>Binary</li>
            <li>Identifier</li>
            <li>NumberLiteral</li>
          </ul>
        </div>

        <div className="flow-host">
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            nodeTypes={nodeTypes}
            fitView
          >
            <Background gap={16} size={1} />
            <Controls />
            <MiniMap pannable zoomable />
          </ReactFlow>
        </div>
      </div>
    </div>
  )
}

export default App
