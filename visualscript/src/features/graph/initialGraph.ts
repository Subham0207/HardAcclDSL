import type { Edge, XYPosition } from '@xyflow/react'
import type { ScriptFlowNode, ScriptNodeRole } from '../nodes/types'

export type StarterNodeType = ScriptFlowNode['type']

export type StarterNodeTemplate = {
  type: StarterNodeType
  label: string
  role: ScriptNodeRole
  detail: string
}

export const starterNodeTemplates: StarterNodeTemplate[] = [
  { type: 'localDecl', label: 'LocalDeclaration', role: 'Statement', detail: 'local result = value' },
  { type: 'assignment', label: 'Assignment', role: 'Statement', detail: 'result = value' },
  { type: 'return', label: 'Return', role: 'Statement', detail: 'return result' },
  { type: 'functionCall', label: 'FunctionCall', role: 'AstNode', detail: 'print(result)' },
  { type: 'binary', label: 'Binary', role: 'Expression', detail: 'left + right' },
  { type: 'identifier', label: 'Identifier', role: 'Expression', detail: 'inputA' },
  { type: 'numberLiteral', label: 'NumberLiteral', role: 'Expression', detail: '2' },
]

const templatesByType = new Map(starterNodeTemplates.map((t) => [t.type, t]))

export const initialNodes: ScriptFlowNode[] = []
export const initialEdges: Edge[] = []

export function createStarterNode(type: StarterNodeType, position: XYPosition, id: string): ScriptFlowNode {
  const template = templatesByType.get(type)
  if (!template) {
    throw new Error(`Unknown starter node type: ${type}`)
  }

  return {
    id,
    type,
    position,
    data: {
      label: template.label,
      role: template.role,
      detail: template.detail,
    },
  } as ScriptFlowNode
}
