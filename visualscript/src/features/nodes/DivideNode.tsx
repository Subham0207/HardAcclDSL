import type { NodeProps } from '@xyflow/react'
import { OperatorNodeBase } from './OperatorNodeBase'
import type { DivideFlowNode } from './types'

export function DivideNode(props: NodeProps<DivideFlowNode>) {
  return <OperatorNodeBase {...props} />
}
