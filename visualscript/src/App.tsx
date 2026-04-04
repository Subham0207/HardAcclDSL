import '@xyflow/react/dist/style.css'
import './App.css'
import { GraphCanvas } from './features/graph/GraphCanvas'

function App() {
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
          <GraphCanvas />
        </div>
      </div>
    </div>
  )
}

export default App
