import type { Edge } from '@xyflow/react'
import type { ScriptFlowNode } from '../nodes/types'

type NodeHandleLayout = {
  dataIn: string[]
  dataOut: string[]
  execIn: string[]
  execOut: string[]
}

export type GraphSnapshot = {
  nodes: Array<{
    id: string
    type: ScriptFlowNode['type']
    position: ScriptFlowNode['position']
    data: ScriptFlowNode['data']
    handles: NodeHandleLayout
    dataFlowEdgeIds: string[]
    execFlowEdgeIds: string[]
  }>
  edges: Array<{
    id: string
    source: string
    sourceHandle: string | null
    target: string
    targetHandle: string | null
    flow: 'data' | 'exec'
  }>
}

const handleLayoutByType: Record<ScriptFlowNode['type'], NodeHandleLayout> = {
  localDecl: { dataIn: ['value'], dataOut: ['out'], execIn: ['exec-in'], execOut: ['exec-out'] },
  assignment: { dataIn: ['target', 'value'], dataOut: [], execIn: [], execOut: [] },
  return: { dataIn: ['value'], dataOut: [], execIn: [], execOut: [] },
  print: { dataIn: ['value'], dataOut: [], execIn: ['exec-in'], execOut: ['exec-out'] },
  add: { dataIn: ['left', 'right'], dataOut: ['out'], execIn: [], execOut: [] },
  subtract: { dataIn: ['left', 'right'], dataOut: ['out'], execIn: [], execOut: [] },
  multiply: { dataIn: ['left', 'right'], dataOut: ['out'], execIn: [], execOut: [] },
  divide: { dataIn: ['left', 'right'], dataOut: ['out'], execIn: [], execOut: [] },
  modulo: { dataIn: ['left', 'right'], dataOut: ['out'], execIn: [], execOut: [] },
  identifier: { dataIn: [], dataOut: ['out'], execIn: [], execOut: [] },
  global: { dataIn: [], dataOut: ['out'], execIn: [], execOut: [] },
  numberLiteral: { dataIn: [], dataOut: ['out'], execIn: [], execOut: [] },
}

const isExecHandle = (handleId: string | null | undefined): boolean => handleId?.startsWith('exec-') ?? false

export const buildGraphSnapshot = (nodes: ScriptFlowNode[], edges: Edge[]): GraphSnapshot => {
  const normalizedEdges = edges.map((edge) => ({
    id: edge.id,
    source: edge.source,
    sourceHandle: edge.sourceHandle ?? null,
    target: edge.target,
    targetHandle: edge.targetHandle ?? null,
    flow: isExecHandle(edge.sourceHandle) && isExecHandle(edge.targetHandle) ? ('exec' as const) : ('data' as const),
  }))

  const nodesWithFlow = nodes.map((node) => {
    const connected = normalizedEdges.filter((edge) => edge.source === node.id || edge.target === node.id)

    return {
      id: node.id,
      type: node.type,
      position: node.position,
      data: node.data,
      handles: handleLayoutByType[node.type],
      dataFlowEdgeIds: connected.filter((edge) => edge.flow === 'data').map((edge) => edge.id),
      execFlowEdgeIds: connected.filter((edge) => edge.flow === 'exec').map((edge) => edge.id),
    }
  })

  return {
    nodes: nodesWithFlow,
    edges: normalizedEdges,
  }
}
