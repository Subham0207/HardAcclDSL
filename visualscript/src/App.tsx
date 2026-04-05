import { useRef } from 'react'
import '@xyflow/react/dist/style.css'
import './App.css'
import { GraphCanvas, type GraphCanvasHandle } from './features/graph/GraphCanvas'
import { starterNodeTemplates } from './features/graph/initialGraph'

function App() {
  const graphRef = useRef<GraphCanvasHandle | null>(null)
  const viewportRef = useRef<HTMLDivElement | null>(null)

  return (
    <div className="app-shell">
      <header className="app-header">
        <h1>Visual Script Prototype</h1>
        <p>Starter graph for AST-oriented Lua authoring. Drag and connect nodes to experiment.</p>
      </header>

      <div className="canvas-shell">
        <div className="legend">
          <h2>Starter Nodes</h2>
          <p className="legend-help">Click to add a node at the graph center.</p>
          <div className="legend-buttons">
            {starterNodeTemplates.map((template) => (
              <button
                key={template.type}
                type="button"
                className="legend-node-btn"
                onClick={() => graphRef.current?.addNodeAtViewportCenter(template.type)}
              >
                {template.label}
              </button>
            ))}
          </div>
        </div>

        <div className="flow-host" ref={viewportRef}>
          <GraphCanvas ref={graphRef} viewportRef={viewportRef} />
        </div>
      </div>
    </div>
  )
}

export default App
