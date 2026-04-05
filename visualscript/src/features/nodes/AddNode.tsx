import type { NodeProps } from '@xyflow/react'
import { OperatorNodeBase } from './OperatorNodeBase'
import type { AddFlowNode } from './types'

export function AddNode(props: NodeProps<AddFlowNode>) {
  return <OperatorNodeBase {...props} />
}
