# Lua to IR: Initial Design Notes

## Goal
Build a staged pipeline that converts a supported subset of Lua into an internal representation (IR) for analysis and later code generation.

## Pipeline
1. Lexical analysis (Lexer): source text -> tokens
2. Parsing (Parser): tokens -> AST
3. Lowering: AST -> IR

Tokens are parser input, not IR. Keep IR separate so it is easier to validate, optimize, and evolve.

## Token Design
Each token should carry:
- Kind: broad category used by parser decisions
- Lexeme text: exact source text for identifier/literal/operator spelling
- Start/end offset: absolute character range in source (use end as exclusive)
- Line/column: human-readable position for diagnostics

### Recommended token kinds
- Keyword
- Identifier
- Number
- String
- Operator
- Punctuator
- EOF
- Invalid

## Lua-Specific Clarification
Lua is not indentation-sensitive. Do not model indentation as a syntactic token category.

## Vocabulary Strategy (Subset First)
Use sub-kinds so unsupported syntax can be rejected cleanly with precise errors:
- KeywordKind: local, function, end, if, then, else, while, do, return
- OperatorKind: =, +, -, *, /, ==, ~=, <, <=, >, >=, and, or, not, .., #
- PunctuatorKind: (, ), ,, ., :, ;, {, }, [, ]

## Minimal AST for v1
Statements:
- LocalDecl
- Assign
- Return
- ExprStmt

Expressions:
- IdentifierExpr
- NumberLiteralExpr
- StringLiteralExpr
- BinaryExpr
- CallExpr

## Minimal IR for v1
- LoadConst
- LoadLocal
- StoreLocal
- Binary
- Call
- Return

Add control-flow IR later (Jump/Branch) when introducing if/while.

## Example
Lua source:

```lua
local a = 2 + 3
```

Conceptual IR:
1. t1 = const 2
2. t2 = const 3
3. t3 = add t1, t2
4. store_local a, t3

## Scope Policy
Start strict and documented:
- Supported now: local declarations, numeric/string literals, arithmetic, simple calls, return
- Deferred: tables, closures/upvalues, varargs, metamethods, goto, full Lua surface area

This keeps the implementation stable while the DSL vocabulary grows incrementally.
