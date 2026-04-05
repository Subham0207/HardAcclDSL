import type { NodeProps } from '@xyflow/react'
import { OperatorNodeBase } from './OperatorNodeBase'
import type { ModuloFlowNode } from './types'

export function ModuloNode(props: NodeProps<ModuloFlowNode>) {
  return <OperatorNodeBase {...props} />
}
