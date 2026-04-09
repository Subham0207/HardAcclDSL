# HardAcclDSL: Current Direction and Progress

## Product Goal
Use Lua as a text syntax for authoring logic, and use a custom AST as the canonical model for a future visual scripting UI.

Core workflow target:
1. Lua text -> Parse -> AST
2. Visual scripting UI edits the same AST
3. AST -> Lua code generation

IR is no longer the immediate focus. It can be revisited later if optimization or backend compilation needs arise.

## What We Have Implemented

### 0) Repository and Tooling Notes
- Frontend visual scripting app exists at `visualscript/` (Vite + React + TypeScript).
- Frontend package manager is Yarn.
- Use Yarn commands in `visualscript/` for install/run/build (`yarn`, `yarn dev`, `yarn build`).

### 1) ANTLR-based Parsing (replaces hand-written lexer complexity)
- Added ANTLR runtime/build integration in the API project.
- Added grammar file for a Lua subset.
- Parser now produces:
	- syntax diagnostics
	- token list with line/column
	- parse tree string (debug)
	- structured parse tree object (JSON-friendly tree)
	- mapped AST root for valid parses

### 2) Lua Parsing Services
- Added parser service that runs generated ANTLR lexer/parser and collects diagnostics.
- Added conversion service that validates syntax and returns placeholder conversion output plus parse details.

### 3) API Endpoints
- POST /api/lua/convert
	- Returns IR placeholder string, parse tree (debug string), tokens, and AST.
- POST /api/lua/parse-tree
	- Returns parse tree as a structured tree object (node + children), not only a string.
- POST /api/lua/ast
	- Returns AST only.
- POST /api/lua/graph-snapshot
	- Receives typed VisualScript graph snapshot JSON and returns an acknowledgement (node/edge counts).
- POST /api/lua/graph-to-ast
	- Receives request body with required fields: `user`, `scriptName`, `graphSnapshot`.
	- Accepts optional runtime globals object: `globals` (`{ [name: string]: number }`).
	- Maps graph snapshot to AST, renders Lua, executes Lua, and saves generated Lua script before returning.
	- Saved Lua now includes graph position metadata comments per node in this format:
		- `-- @vs-node <nodeId> <nodeType> <x> <y>`
	- API response `luaCode` remains clean executable Lua (position comments are for persisted roundtrip state).
	- Also returns generated Lua code and Lua execution result.
	- Execution result now includes:
		- `success`
		- `error`
		- `returnValues`
		- `printedLines` (captured `print(...)` output)
	- Runtime global names must match Lua identifier format (`[A-Za-z_][A-Za-z0-9_]*`) and values must be finite numbers.
- POST /api/lua/lua-to-visualscript
	- Supports two request modes:
		- `luaCode` mode: receives Lua code and returns parsed AST plus mapped VisualScript graph snapshot.
		- storage lookup mode: receives `user` + `scriptName`, loads Lua from storage, then returns parsed AST plus mapped VisualScript graph snapshot.
	- If `user` and `scriptName` are provided, both are required together.
	- Returns `404` when script is not found for the provided user.
	- When Lua includes `-- @vs-node ...` comments, mapper restores node ids/types/positions into AST metadata and reuses them in graph snapshot output.
	- If no position comments are present, visual graph mapping falls back to default layout behavior.
- GET /api/lua/lua-to-visualscript/default
	- Uses hardcoded bootstrap Lua (`local result = 10`) and returns mapped AST + graph snapshot for first-load UI hydration.
- Added new Lua script storage controller: `LuaScriptStorageController` under `api/lua-scripts`.
	- `POST /api/lua-scripts/save`
		- request body: `user`, `scriptName`, `luaCode`
		- stores Lua code in S3 as `uuid.lua`
		- stores DynamoDB metadata row with keys `user` (PK), `scriptname` (SK), and `s3Link`
		- duplicate `(user, scriptName)` now updates existing script (upsert behavior)
		- existing script update reuses the same S3 object key and overwrites content (no new S3 file)
		- response returns only `user` and `scriptName` (`s3Link` is internal)
	- `GET /api/lua-scripts/{user}/{scriptName}`
		- reads metadata from DynamoDB and Lua content from S3
		- response returns `user`, `scriptName`, and `luaCode` (`s3Link` is internal)
	- `GET /api/lua-scripts/{user}`
		- lists script metadata rows for user
		- response script metadata omits `s3Link`
	- `DELETE /api/lua-scripts/{user}/{scriptName}`
		- deletes both S3 object and DynamoDB metadata row
	- `POST /api/lua-scripts/execute`
		- request body: `user`, `scriptName`, optional `globals` (`{ [name: string]: number }`)
		- loads stored Lua script for `(user, scriptName)` from storage and executes it directly
		- uses same execution payload contract as graph-to-ast (`success`, `error`, `returnValues`, `printedLines`)
		- does not include Lua code in response

### 3.1) Visual Script UI (React Flow Prototype)
- Installed React Flow library (`@xyflow/react`) in `visualscript/`.
- Replaced Vite starter page with a basic graph canvas prototype.
- Refactored UI into reusable modules:
	- thin `App.tsx` shell
	- `GraphCanvas` for graph state/behavior
	- per-node components under `features/nodes/`
	- centralized node registry (`nodeTypes`)
	- separate graph config (`initialGraph`, `defaultEdgeOptions`)
- Added starter nodes for:
	- LocalDeclaration
	- Assignment
	- Return
	- Print
	- Global Node
	- Add
	- Subtract
	- Multiply
	- Divide
	- Modulo
	- Identifier
	- NumberLiteral
- Each node now owns its own pin/handle definitions directly using React Flow `Handle` (no wrapper abstraction).
- Node frame layout is now a 3-column flex structure:
	- left rail for input handles
	- center body for node content
	- right rail for output handles
- Initial graph uses explicit `sourceHandle` and `targetHandle` ids for clearer field-level wiring.
- Added React Flow interactions:
	- drag nodes
	- connect edges via visible input/output pins (handles)
	- select nodes and edges
	- delete selected node/edge with `Delete` or `Backspace`
	- minimap
	- controls
	- background grid
- Added visual selection feedback:
	- selected node gets stronger border/glow
	- selected edge changes to a high-contrast highlight color
- Added execution-flow prototype support (first pass):
	- LocalDeclaration and Print include `exec-in` and `exec-out` handles
	- execution handles use distinct styling from data handles
	- execution edges (exec -> exec) use a distinct dashed green style and marker
	- connection validation now enforces handle compatibility:
		- `exec-*` can connect only to `exec-*`
		- data handles can connect only to data handles
		- mixed execution/data links are blocked in the editor
- UI node metadata simplification:
	- no Statement/Expression/AstNode role text shown in node cards
	- operator nodes no longer show an extra operator-symbol row
- Editable node fields now persist into graph state (instead of staying as initial default values).
- New nodes now use UUID ids (`crypto.randomUUID`) in frontend graph creation.
- Frontend now captures and sends graph snapshot JSON plus required `user` and `scriptName` to backend mapping route (`/api/lua/graph-to-ast`).
- Frontend validates `user` and `scriptName` before sending graph snapshots.
- Frontend now supports runtime globals via multiline JSON input (object map of number values).
- Frontend now validates runtime globals JSON shape and values before sending execution requests.
- Frontend includes explicit action to add a `Global Node` to the graph and separate runtime value input for execution.
- Frontend now asks for username at startup and loads that user's scripts from `GET /api/lua-scripts/{user}`.
- Frontend now shows saved scripts in a dropdown and, when selected, calls `POST /api/lua/lua-to-visualscript` with `user` + `scriptName` to load and render that script graph.
- Frontend now includes a dedicated Lua Console panel on the right side of the graph canvas.
- Lua Console displays:
	- generated Lua code
	- printed execution output lines
	- runtime execution errors
- Frontend graph-to-AST conversion was intentionally removed; backend will own graph-to-AST mapping.
- Frontend now bootstraps graph state on first load by calling `/api/lua/lua-to-visualscript/default` and loading returned snapshot into the canvas.
- Backend now uses explicit `VisualScriptGraph...` naming for clarity:
	- `VisualScriptGraphSnapshotDto`
	- `VisualScriptGraphIndex`
	- `VisualScriptGraphSnapshotAckResponse`
- Backend helper index (`VisualScriptGraphIndex`) exists and can answer:
	- what is connected to a pin
	- what node owns a pin
	- what node executes next via `exec-out -> exec-in`
- Backend mapper service now exists: `VisualScriptGraphToAstMapper`.
- Added reverse mapper service: `AstToVisualScriptGraphMapper`.
	- Maps AST statements/expressions into typed VisualScript nodes and edges.
	- Emits both execution flow edges and data flow edges.
- Added storage service: `LuaScriptStorageService` implementing `ILuaScriptStorageService`.
	- Uses AWS SDK S3 + DynamoDB clients.
	- Uses upsert save semantics keyed by `(user, scriptname)`.
	- Existing rows keep their original `s3Link`; only object content is overwritten on save.
	- Stores only `s3Link` in DynamoDB (no Lua content duplication).
	- `s3Link` is internal-only and is not exposed by read/list/save API response contracts.
	- Persisted Lua content now carries graph position comments for roundtrip reconstruction.
- Current mapper coverage:
	- statement nodes: `localDecl`, `assignment`, `return`, `print`
	- expression nodes: `identifier`, `global`, `numberLiteral`, `add/subtract/multiply/divide/modulo`
	- execution ordering from `exec-out -> exec-in`
	- diagnostics collection for cycles/unsupported nodes/invalid assignment target
	- `localDecl` used as expression source maps to identifier by variable name
- Added `AstToLuaScribanRenderer` for AST -> Lua generation using Scriban templates.
- Lua generation now adds parentheses for nested arithmetic based on AST tree shape to preserve operation order.
- Added `LuaExecutionService` using Lua.NET (Lua 5.4 bindings) for server-side execution of generated Lua.
- Graph-to-AST flow is now end-to-end: VisualScript -> AST -> Lua -> Execute -> Response payload.
- Lua `print(...)` output is captured inside the Lua runtime and returned to clients as `printedLines`.
- Lua execution now supports injecting number globals before script execution.
- Added direct stored-script execution flow via `/api/lua-scripts/execute` using runtime number globals.
- Added Lua position comment codec for graph roundtrip metadata:
	- encodes `nodeId` + `nodeType` + `(x, y)` into Lua comments on persisted scripts
	- decodes comments during Lua->AST mapping and applies positions by expected node type order
	- avoids comment drift for non-graph AST nodes (for example local declaration inline literals)
- Fixed false-positive expression cycle detection for shared expression DAG nodes:
	- mapper now treats `visiting` as recursion stack (add/remove per traversal path)
	- repeated references to the same source node no longer incorrectly emit `expression_cycle`
	- real cycle fallback for `localDecl` / `identifier` now prefers variable name over generic `cycle`

AST JSON contract (latest):
- `kind` is the only node discriminator in API responses.
- `$type` metadata is intentionally not emitted.
- Node-specific fields are emitted based on node kind (e.g., `name`, `value`, `operator`, `arguments`).
- Implemented via a custom AST JSON converter for serialization.
- AST deserialization from JSON is not implemented yet.

### 4) Tests
- Tests are split by concern/file:
	- LuaToIR integration tests
	- ANTLR parser result tests (Lua -> parser output)
	- ANTLR to AST mapping tests
- VisualScript graph-to-AST mapper tests
- Lua execution service tests (return values, runtime error, and print capture)
- AST-to-VisualScript mapper tests
- Lua-to-VisualScript controller endpoint tests
- Added route-level regression test for `/api/lua/graph-to-ast` with a shared-`result` expression graph.
- Large route request/response payloads are now stored as fixtures in:
	- `tests/HardAcclDslApi.UnitTests/TestData/graph-to-ast-request.json`
	- `tests/HardAcclDslApi.UnitTests/TestData/graph-to-ast-response.json`
- Current count: 35 passing tests.
- AST mapping tests verify whole-tree structural equality while ignoring NodeId.

### 5) AST Model (Updated)
- AST now uses a single base node type: AstNode.
- ProgramNode stores `Statements` as `List<AstNode>`.
- Statement and expression families are not split into separate base classes.
- `FunctionCallNode` is used directly for call statements.
- `GlobalReferenceExpressionNode` exists for explicit global-variable references in the AST model.
- `FunctionDeclarationNode` exists in the AST model as a planned node type (grammar/mapping for function declarations is not implemented yet).
- Visual editor currently models function invocation as explicit `Print` node semantics rather than generic function-call semantics.

### 6) Deployment and Infrastructure (Implemented)
- Added backend deployment workflow: `.github/workflows/deploy-backend-lambda.yml`.
	- Triggered on `master` pushes and manual dispatch.
	- Builds and pushes Lambda container image to ECR.
	- Deploys infrastructure/app via CloudFormation template.
- Added frontend deployment workflow: `.github/workflows/deploy-frontend-pages.yml`.
	- Triggered on `master` pushes and manual dispatch.
	- Builds Vite app and deploys to GitHub Pages via GitHub Actions.
	- Uses Node 24 in CI to satisfy current frontend dependency engine requirements.
- Added Lambda container Dockerfile: `src/HardAcclDslApi/Dockerfile.lambda`.
	- Uses AWS Lambda Web Adapter extension to run existing ASP.NET API on Lambda without controller rewrites.
- Added infrastructure template: `infra/cloudformation.yml`.
	- Creates IAM role for Lambda execution.
	- Creates container-based Lambda function.
	- Creates API Gateway HTTP API with Lambda proxy integration and routes (`ANY /`, `ANY /{proxy+}`).
	- Configures API stage and invoke permission.
	- Exposes output key `HardAcclDSLApiUrl`.
	- Creates S3 bucket `hardaccldsl` for Lua script files.
	- Creates DynamoDB table `HardAcclDSL` with key schema:
		- PK: `user`
		- SK: `scriptname`
	- Adds Lambda IAM permissions for DynamoDB (`GetItem/PutItem/DeleteItem/Query`) and S3 (`GetObject/PutObject/DeleteObject/ListBucket`).
	- Injects runtime env vars into Lambda:
		- `SCRIPT_BUCKET_NAME`
		- `SCRIPT_TABLE_NAME`

Deployment conventions now in use:
- Region fixed to `us-east-1` in backend workflow.
- ECR repository name fixed to lowercase `hardaccldsl` (required by ECR naming constraints).
- Backend CloudFormation names fixed:
	- stack: `HardAcclDSL`
	- lambda: `HardAcclDSLLambda`
	- http api: `HardAcclDSLApi`
- Frontend API base variable name standardized as `HARDACCLDSL_API_URL`.
	- Frontend workflow injects this into build as `VITE_API_BASE_URL`.
	- Frontend app uses `VITE_API_BASE_URL` when present and falls back to local `/api` proxy in dev.

Operational notes:
- Automatic GitHub variable write-back from backend workflow was removed after GitHub integration permission errors (`403 Resource not accessible by integration`).
- Current model: set repository variable `HARDACCLDSL_API_URL` manually to the API Gateway URL.
- Added ASP.NET Core CORS middleware in backend (`Program.cs`) and wired `CORS_ALLOWED_ORIGIN` through CloudFormation Lambda environment.
	- This resolved deployed browser preflight failure (`OPTIONS 405`) for frontend -> API Gateway requests.
- Script storage conventions:
	- S3 object keys use UUID filenames (`uuid.lua`) to make script rename operations metadata-only.
	- No versioning implemented.
	- User identity is provided from request body for now (no auth flow yet).

## Current Grammar Scope (Lua Subset)
Supported now:
- local assignment: local a = expression
- assignment: a = expression
- return expression
- function calls: f(x, y)
- print call usage (via existing call-expression grammar path)
- expressions with +, -, *, /
- number and string literals
- line comments with --

Out of scope for now:
- tables
- function declarations/bodies
- control flow blocks
- full Lua operator set and precedence
- varargs, metatables, goto, etc.

## Parse Tree vs AST
Important distinction:
- Parse tree (ANTLR output) is concrete syntax structure.
- AST (our domain model) is simplified semantic structure used by UI and code generation.

Decision:
- Parse tree is an internal ingestion artifact.
- AST will be the source of truth for visual scripting and Lua emission.

## Next Steps (AST-First Plan)
1. Add server-side snapshot validation diagnostics on top of `VisualScriptGraphIndex`.
2. Expand backend graph -> AST mapper coverage and edge-case handling.
3. Add richer diagnostic payloads (pin/edge-level context for easier UI debugging).
4. Add AST validation diagnostics (semantic and placement checks).
5. Implement AST -> Lua code generator (templated printer).
6. Add round-trip tests:
   - Lua -> AST -> Lua (semantic equivalence)
   - AST -> Lua -> AST (shape stability for supported subset)
7. Add function declaration grammar + AST mapping (`FunctionDeclarationNode`).
8. Decide NodeId lifecycle strategy for visual editor persistence and patch operations.
9. Connect snapshot->AST pipeline with existing `/api/lua/convert` and generation flows.
10. Extend execution diagnostics (for example source node mapping for runtime errors and safer sandbox restrictions).

## Execution Flow Rules (UI Graph)
Current intent for execution-enabled nodes (first pass):
- LocalDeclaration and Print are execution statements and expose:
	- `exec-in` (target)
	- `exec-out` (source)
- Execution flow determines statement order in generated Lua.
- Data flow provides expression values and does not by itself define statement order.

Allowed connection examples:
- `LocalDeclaration.exec-out` -> `Print.exec-in`
- `LocalDeclaration.out` -> `Add.left`
- `NumberLiteral.out` -> `Add.right`
- `Add.out` -> `Print.value`

Forbidden connection examples:
- `exec-out` -> data handle (`left`, `right`, `value`, `out`)
- data handle (`out`) -> `exec-in`
- `exec-in` -> `exec-in`
- `exec-out` -> `exec-out`
- multiple incoming execution edges to one `exec-in` (unless branching is introduced later)

Current enforcement status:
- Implemented in graph editor connection validation (`isValidConnection`) and connect-time guard (`onConnect`).
- Invalid mixed links are rejected both during drag-preview and when attempting to create the edge.

Ordering semantics for code generation:
- Follow execution chain from entry statement(s) using execution edges.
- For each execution statement, resolve its required data inputs from data edges.
- Example graph:
	- LocalDeclaration sets `a = 10`
	- Add computes `a + 20`
	- Print prints Add result
- Expected Lua:
	- `local a = 10`
	- `print(a + 20)`

## Architectural Notes
- Keep parser internals (ANTLR rule/token names) isolated from frontend contracts.
- Expose AST DTOs to the visual scripting layer, not parse tree DTOs.
- Preserve source spans on AST nodes where feasible for diagnostics and editor mapping.
- NodeId is currently non-semantic metadata and is intentionally ignored in AST equality tests.
- AST API payloads should remain stable around `kind` to keep frontend integrations predictable.
- Graph-to-Lua generation direction: template-based emitters per node type (for example FunctionDeclaration template).

## Why This Direction
This direction directly supports product requirements:
- text-based authoring in Lua
- visual authoring on a shared AST
- predictable code generation back to Lua

It keeps implementation complexity manageable while preserving a path to add IR later if needed.
