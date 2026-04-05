import type { NodeProps } from '@xyflow/react'
import { OperatorNodeBase } from './OperatorNodeBase'
import type { MultiplyFlowNode } from './types'

export function MultiplyNode(props: NodeProps<MultiplyFlowNode>) {
  return <OperatorNodeBase {...props} />
}
