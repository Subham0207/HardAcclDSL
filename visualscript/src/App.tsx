import { useEffect, useRef, useState } from 'react'
import '@xyflow/react/dist/style.css'
import './App.css'
import { GraphCanvas, type GraphCanvasHandle } from './features/graph/GraphCanvas'
import { starterNodeTemplates } from './features/graph/initialGraph'
import { ExecutionConsole } from './features/console/ExecutionConsole'
import type { GraphSnapshot } from './features/graph/graphSnapshot'

type GraphToAstResponse = {
  luaCode: string
  execution: {
    success: boolean
    error: string
    returnValues: string[]
    printedLines: string[]
  }
}

type LuaToVisualScriptResponse = {
  graphSnapshot: GraphSnapshot
}

type GraphToAstRequest = {
  user: string
  scriptName: string
  graphSnapshot: GraphSnapshot
}

function App() {
  const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? ''
  const graphRef = useRef<GraphCanvasHandle | null>(null)
  const viewportRef = useRef<HTMLDivElement | null>(null)
  const [graphPreview, setGraphPreview] = useState<string>('')
  const [sendStatus, setSendStatus] = useState<string>('')
  const [printedLines, setPrintedLines] = useState<string[]>([])
  const [executionError, setExecutionError] = useState<string>('')
  const [luaCode, setLuaCode] = useState<string>('')
  const [userName, setUserName] = useState<string>('')
  const [scriptName, setScriptName] = useState<string>('')

  useEffect(() => {
    const loadDefaultGraph = async () => {
      try {
        const endpoint = apiBaseUrl
          ? `${apiBaseUrl}/api/lua/lua-to-visualscript/default`
          : '/api/lua/lua-to-visualscript/default'

        const response = await fetch(endpoint)
        const responseText = await response.text()
        if (!response.ok) {
          setSendStatus(`Bootstrap failed (${response.status}): ${responseText}`)
          return
        }

        const payload = JSON.parse(responseText) as LuaToVisualScriptResponse
        if (payload.graphSnapshot) {
          graphRef.current?.loadGraphSnapshot(payload.graphSnapshot)
        }
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Unknown error'
        setSendStatus(`Bootstrap failed: ${message}`)
      }
    }

    loadDefaultGraph()
  }, [apiBaseUrl])

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
                const trimmedUser = userName.trim()
                const trimmedScriptName = scriptName.trim()
                if (!trimmedUser || !trimmedScriptName) {
                  const message = 'Username and script name are required before sending.'
                  setSendStatus(message)
                  window.alert(message)
                  return
                }

                const endpoint = apiBaseUrl ? `${apiBaseUrl}/api/lua/graph-to-ast` : '/api/lua/graph-to-ast'
                const requestPayload: GraphToAstRequest = {
                  user: trimmedUser,
                  scriptName: trimmedScriptName,
                  graphSnapshot: snapshot,
                }

                const response = await fetch(endpoint, {
                  method: 'POST',
                  headers: {
                    'Content-Type': 'application/json',
                  },
                  body: JSON.stringify(requestPayload),
                })

                const responseText = await response.text()
                if (!response.ok) {
                  setPrintedLines([])
                  setExecutionError('')
                  setLuaCode('')
                  setSendStatus(`Send failed (${response.status}): ${responseText}`)
                  return
                }

                const payload = JSON.parse(responseText) as GraphToAstResponse
                setLuaCode(payload.luaCode ?? '')
                setPrintedLines(payload.execution?.printedLines ?? [])
                setExecutionError(payload.execution?.error ?? '')
                setSendStatus('Sent to backend')
              } catch (error) {
                const message = error instanceof Error ? error.message : 'Unknown error'
                setPrintedLines([])
                setExecutionError('')
                setLuaCode('')
                setSendStatus(`Send failed: ${message}`)
              }
            }}
          >
            Send Graph Snapshot
          </button>
          <div className="legend-buttons">
            <label className="legend-field">
              <span>Username</span>
              <input
                className="legend-input"
                type="text"
                value={userName}
                onChange={(event) => setUserName(event.target.value)}
                placeholder="Enter username"
              />
            </label>
            <label className="legend-field">
              <span>Script Name</span>
              <input
                className="legend-input"
                type="text"
                value={scriptName}
                onChange={(event) => setScriptName(event.target.value)}
                placeholder="Enter script name"
              />
            </label>
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

        <div className="console-panel">
          <ExecutionConsole printedLines={printedLines} error={executionError} luaCode={luaCode} />
        </div>
      </div>
    </div>
  )
}

export default App
