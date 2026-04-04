import type { Node } from '@xyflow/react'

export type ScriptNodeRole = 'Statement' | 'Expression' | 'AstNode'

export type BaseScriptNodeData = {
  label: string
  role: ScriptNodeRole
  detail: string
}

export type LocalDeclarationNodeData = BaseScriptNodeData
export type AssignmentNodeData = BaseScriptNodeData
export type ReturnNodeData = BaseScriptNodeData
export type FunctionCallNodeData = BaseScriptNodeData
export type BinaryNodeData = BaseScriptNodeData
export type IdentifierNodeData = BaseScriptNodeData
export type NumberLiteralNodeData = BaseScriptNodeData

export type LocalDeclarationFlowNode = Node<LocalDeclarationNodeData, 'localDecl'>
export type AssignmentFlowNode = Node<AssignmentNodeData, 'assignment'>
export type ReturnFlowNode = Node<ReturnNodeData, 'return'>
export type FunctionCallFlowNode = Node<FunctionCallNodeData, 'functionCall'>
export type BinaryFlowNode = Node<BinaryNodeData, 'binary'>
export type IdentifierFlowNode = Node<IdentifierNodeData, 'identifier'>
export type NumberLiteralFlowNode = Node<NumberLiteralNodeData, 'numberLiteral'>

export type ScriptFlowNode =
  | LocalDeclarationFlowNode
  | AssignmentFlowNode
  | ReturnFlowNode
  | FunctionCallFlowNode
  | BinaryFlowNode
  | IdentifierFlowNode
  | NumberLiteralFlowNode
