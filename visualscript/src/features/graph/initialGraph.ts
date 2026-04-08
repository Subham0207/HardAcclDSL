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
  { type: 'localDecl', label: 'LocalDeclaration', role: 'Statement', detail: 'local name = value' },
  { type: 'assignment', label: 'Assignment', role: 'Statement', detail: 'result = value' },
  { type: 'return', label: 'Return', role: 'Statement', detail: 'return result' },
  { type: 'print', label: 'Print', role: 'Statement', detail: 'print(value)' },
  { type: 'add', label: 'Add', role: 'Expression', detail: 'left + right' },
  { type: 'subtract', label: 'Subtract', role: 'Expression', detail: 'left - right' },
  { type: 'multiply', label: 'Multiply', role: 'Expression', detail: 'left * right' },
  { type: 'divide', label: 'Divide', role: 'Expression', detail: 'left / right' },
  { type: 'modulo', label: 'Modulo', role: 'Expression', detail: 'left % right' },
  { type: 'identifier', label: 'Identifier', role: 'Expression', detail: 'read variable' },
  { type: 'global', label: 'Global Node', role: 'Expression', detail: 'read global' },
  { type: 'numberLiteral', label: 'NumberLiteral', role: 'Expression', detail: 'numeric constant' },
]

const templatesByType = new Map(starterNodeTemplates.map((t) => [t.type, t]))

export const initialNodes: ScriptFlowNode[] = []
export const initialEdges: Edge[] = []

export function createStarterNode(type: StarterNodeType, position: XYPosition, id: string): ScriptFlowNode {
  const template = templatesByType.get(type)
  if (!template) {
    throw new Error(`Unknown starter node type: ${type}`)
  }

  const baseData = {
    label: template.label,
    role: template.role,
    detail: template.detail,
  }

  let data: Record<string, string> = baseData

  if (type === 'localDecl') {
    data = {
      ...baseData,
      variableName: 'result',
      initialValue: '0',
    }
  } else if (type === 'identifier') {
    data = {
      ...baseData,
      variableName: 'result',
    }
  } else if (type === 'global') {
    data = {
      ...baseData,
      variableName: 'multiplier',
    }
  } else if (type === 'numberLiteral') {
    data = {
      ...baseData,
      value: '0',
    }
  } else if (type === 'add') {
    data = {
      ...baseData,
      operatorSymbol: '+',
    }
  } else if (type === 'subtract') {
    data = {
      ...baseData,
      operatorSymbol: '-',
    }
  } else if (type === 'multiply') {
    data = {
      ...baseData,
      operatorSymbol: '*',
    }
  } else if (type === 'divide') {
    data = {
      ...baseData,
      operatorSymbol: '/',
    }
  } else if (type === 'modulo') {
    data = {
      ...baseData,
      operatorSymbol: '%',
    }
  }

  return {
    id,
    type,
    position,
    data,
  } as ScriptFlowNode
}
