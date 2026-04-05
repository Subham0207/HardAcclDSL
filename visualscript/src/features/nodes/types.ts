import type { Node } from '@xyflow/react'

export type ScriptNodeRole = 'Statement' | 'Expression' | 'AstNode'

export type BaseScriptNodeData = {
  label: string
  role: ScriptNodeRole
  detail?: string
}

export type LocalDeclarationNodeData = BaseScriptNodeData & {
  variableName: string
  initialValue: string
}

export type AssignmentNodeData = BaseScriptNodeData
export type ReturnNodeData = BaseScriptNodeData
export type FunctionCallNodeData = BaseScriptNodeData
export type OperatorNodeData = BaseScriptNodeData & {
  operatorSymbol: string
}

export type IdentifierNodeData = BaseScriptNodeData & {
  variableName: string
}

export type NumberLiteralNodeData = BaseScriptNodeData & {
  value: string
}

export type LocalDeclarationFlowNode = Node<LocalDeclarationNodeData, 'localDecl'>
export type AssignmentFlowNode = Node<AssignmentNodeData, 'assignment'>
export type ReturnFlowNode = Node<ReturnNodeData, 'return'>
export type FunctionCallFlowNode = Node<FunctionCallNodeData, 'functionCall'>
export type AddFlowNode = Node<OperatorNodeData, 'add'>
export type SubtractFlowNode = Node<OperatorNodeData, 'subtract'>
export type MultiplyFlowNode = Node<OperatorNodeData, 'multiply'>
export type DivideFlowNode = Node<OperatorNodeData, 'divide'>
export type ModuloFlowNode = Node<OperatorNodeData, 'modulo'>
export type IdentifierFlowNode = Node<IdentifierNodeData, 'identifier'>
export type NumberLiteralFlowNode = Node<NumberLiteralNodeData, 'numberLiteral'>

export type ScriptFlowNode =
  | LocalDeclarationFlowNode
  | AssignmentFlowNode
  | ReturnFlowNode
  | FunctionCallFlowNode
  | AddFlowNode
  | SubtractFlowNode
  | MultiplyFlowNode
  | DivideFlowNode
  | ModuloFlowNode
  | IdentifierFlowNode
  | NumberLiteralFlowNode
