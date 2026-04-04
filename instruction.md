# HardAcclDSL: Current Direction and Progress

## Product Goal
Use Lua as a text syntax for authoring logic, and use a custom AST as the canonical model for a future visual scripting UI.

Core workflow target:
1. Lua text -> Parse -> AST
2. Visual scripting UI edits the same AST
3. AST -> Lua code generation

IR is no longer the immediate focus. It can be revisited later if optimization or backend compilation needs arise.

## What We Have Implemented

### 1) ANTLR-based Parsing (replaces hand-written lexer complexity)
- Added ANTLR runtime/build integration in the API project.
- Added grammar file for a Lua subset.
- Parser now produces:
	- syntax diagnostics
	- token list with line/column
	- parse tree string (debug)
	- structured parse tree object (JSON-friendly tree)

### 2) Lua Parsing Services
- Added parser service that runs generated ANTLR lexer/parser and collects diagnostics.
- Added conversion service that validates syntax and returns placeholder conversion output.

### 3) API Endpoints
- POST /api/lua/convert
	- Returns IR placeholder string, parse tree (debug string), and tokens.
- POST /api/lua/parse-tree
	- Returns parse tree as a structured tree object (node + children), not only a string.

### 4) Tests
- Added integration tests for LuaToIR.Convert:
	- valid Lua subset path
	- invalid syntax path

## Current Grammar Scope (Lua Subset)
Supported now:
- local assignment: local a = expression
- assignment: a = expression
- return expression
- function calls: f(x, y)
- expressions with +, -, *, /
- number and string literals
- line comments with --

Out of scope for now:
- tables
- functions/closures bodies
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
1. Define AST model types with stable node IDs.
2. Build ParseTree -> AST mapper (visitor-based).
3. Add AST validation diagnostics.
4. Add AST -> Lua code generator (templated printer).
5. Add round-trip tests:
	 - Lua -> AST -> Lua (semantic equivalence)
	 - AST -> Lua -> AST (shape stability for supported subset)

## Architectural Notes
- Keep parser internals (ANTLR rule/token names) isolated from frontend contracts.
- Expose AST DTOs to the visual scripting layer, not parse tree DTOs.
- Preserve source spans on AST nodes where feasible for diagnostics and editor mapping.

## Why This Direction
This direction directly supports product requirements:
- text-based authoring in Lua
- visual authoring on a shared AST
- predictable code generation back to Lua

It keeps implementation complexity manageable while preserving a path to add IR later if needed.
