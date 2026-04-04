import type { Edge } from '@xyflow/react'
import type { ScriptFlowNode } from '../nodes/types'

export const initialNodes: ScriptFlowNode[] = [
  {
    id: 'localDecl-1',
    type: 'localDecl',
    position: { x: 80, y: 40 },
    data: {
      label: 'LocalDeclaration',
      role: 'Statement',
      detail: 'local result = value',
    },
  },
  {
    id: 'binary-1',
    type: 'binary',
    position: { x: 430, y: 40 },
    data: {
      label: 'Binary',
      role: 'Expression',
      detail: 'left + right',
    },
  },
  {
    id: 'number-1',
    type: 'numberLiteral',
    position: { x: 760, y: 10 },
    data: {
      label: 'NumberLiteral',
      role: 'Expression',
      detail: '2',
    },
  },
  {
    id: 'identifier-1',
    type: 'identifier',
    position: { x: 760, y: 140 },
    data: {
      label: 'Identifier',
      role: 'Expression',
      detail: 'inputA',
    },
  },
  {
    id: 'assign-1',
    type: 'assignment',
    position: { x: 80, y: 240 },
    data: {
      label: 'Assignment',
      role: 'Statement',
      detail: 'result = value',
    },
  },
  {
    id: 'call-1',
    type: 'functionCall',
    position: { x: 430, y: 240 },
    data: {
      label: 'FunctionCall',
      role: 'AstNode',
      detail: 'print(result)',
    },
  },
  {
    id: 'return-1',
    type: 'return',
    position: { x: 80, y: 430 },
    data: {
      label: 'Return',
      role: 'Statement',
      detail: 'return result',
    },
  },
]

export const initialEdges: Edge[] = [
  {
    id: 'e1',
    source: 'localDecl-1',
    sourceHandle: 'out',
    target: 'binary-1',
    targetHandle: 'left',
    label: 'value',
    animated: true,
  },
  {
    id: 'e2',
    source: 'number-1',
    sourceHandle: 'out',
    target: 'binary-1',
    targetHandle: 'left',
    label: 'left',
    animated: true,
  },
  {
    id: 'e3',
    source: 'identifier-1',
    sourceHandle: 'out',
    target: 'binary-1',
    targetHandle: 'right',
    label: 'right',
    animated: true,
  },
  {
    id: 'e4',
    source: 'binary-1',
    sourceHandle: 'out',
    target: 'assign-1',
    targetHandle: 'value',
    label: 'value',
    animated: true,
  },
  {
    id: 'e5',
    source: 'assign-1',
    sourceHandle: 'out',
    target: 'call-1',
    targetHandle: 'arg0',
    label: 'arg0',
    animated: true,
  },
  {
    id: 'e6',
    source: 'identifier-1',
    sourceHandle: 'out',
    target: 'call-1',
    targetHandle: 'arg1',
    label: 'arg1',
    animated: true,
  },
  {
    id: 'e7',
    source: 'assign-1',
    sourceHandle: 'out',
    target: 'return-1',
    targetHandle: 'value',
    label: 'value',
    animated: true,
  },
]
