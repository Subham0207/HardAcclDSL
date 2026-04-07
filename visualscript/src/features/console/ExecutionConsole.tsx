type ExecutionConsoleProps = {
  printedLines: string[]
  error: string
  luaCode: string
}

export function ExecutionConsole({ printedLines, error, luaCode }: ExecutionConsoleProps) {
  const hasOutput = printedLines.length > 0
  const hasError = error.trim().length > 0

  return (
    <section className="execution-console" aria-live="polite">
      <h2>Lua Console</h2>

      {luaCode ? (
        <details className="execution-lua" open>
          <summary>Generated Lua</summary>
          <pre>{luaCode}</pre>
        </details>
      ) : null}

      {hasError ? <p className="execution-error">{error}</p> : null}

      <div className="execution-output" role="log" aria-label="Lua output lines">
        {hasOutput ? (
          printedLines.map((line, index) => (
            <div key={`${index}-${line}`} className="execution-line">
              {line}
            </div>
          ))
        ) : (
          <div className="execution-placeholder">No output yet. Run Graph Snapshot to execute Lua.</div>
        )}
      </div>
    </section>
  )
}
