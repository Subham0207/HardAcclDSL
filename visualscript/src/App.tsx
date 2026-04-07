import { useRef, useState } from 'react'
import '@xyflow/react/dist/style.css'
import './App.css'
import { GraphCanvas, type GraphCanvasHandle } from './features/graph/GraphCanvas'
import { starterNodeTemplates } from './features/graph/initialGraph'

function App() {
  const graphRef = useRef<GraphCanvasHandle | null>(null)
  const viewportRef = useRef<HTMLDivElement | null>(null)
  const [graphPreview, setGraphPreview] = useState<string>('')
  const [sendStatus, setSendStatus] = useState<string>('')

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
          <button
            type="button"
            className="legend-node-btn legend-node-btn-primary"
            onClick={async () => {
              const snapshot = graphRef.current?.exportGraphSnapshot()
              if (!snapshot) {
                return
              }

              console.log('Visual script graph snapshot', snapshot)
              setGraphPreview(JSON.stringify(snapshot, null, 2))

              try {
                const response = await fetch('/api/lua/graph-to-ast', {
                  method: 'POST',
                  headers: {
                    'Content-Type': 'application/json',
                  },
                  body: JSON.stringify(snapshot),
                })

                const responseText = await response.text()
                if (!response.ok) {
                  setSendStatus(`Send failed (${response.status}): ${responseText}`)
                  return
                }

                setSendStatus(`Sent to backend`)
              } catch (error) {
                const message = error instanceof Error ? error.message : 'Unknown error'
                setSendStatus(`Send failed: ${message}`)
              }
            }}
          >
            Send Graph Snapshot
          </button>
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
          {sendStatus ? <p className="legend-send-status">{sendStatus}</p> : null}
          {graphPreview ? <pre className="legend-ast-preview">{graphPreview}</pre> : null}
        </div>

        <div className="flow-host" ref={viewportRef}>
          <GraphCanvas ref={graphRef} viewportRef={viewportRef} />
        </div>
      </div>
    </div>
  )
}

export default App
