import type { NodeProps } from '@xyflow/react'
import { OperatorNodeBase } from './OperatorNodeBase'
import type { SubtractFlowNode } from './types'

export function SubtractNode(props: NodeProps<SubtractFlowNode>) {
  return <OperatorNodeBase {...props} />
}
