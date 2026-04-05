import type { NodeTypes } from '@xyflow/react'
import { AddNode } from './AddNode'
import { AssignmentNode } from './AssignmentNode'
import { DivideNode } from './DivideNode'
import { FunctionCallNode } from './FunctionCallNode'
import { IdentifierNode } from './IdentifierNode'
import { LocalDeclarationNode } from './LocalDeclarationNode'
import { ModuloNode } from './ModuloNode'
import { MultiplyNode } from './MultiplyNode'
import { NumberLiteralNode } from './NumberLiteralNode'
import { ReturnNode } from './ReturnNode'
import { SubtractNode } from './SubtractNode'

export const scriptNodeTypes: NodeTypes = {
  localDecl: LocalDeclarationNode,
  assignment: AssignmentNode,
  return: ReturnNode,
  functionCall: FunctionCallNode,
  add: AddNode,
  subtract: SubtractNode,
  multiply: MultiplyNode,
  divide: DivideNode,
  modulo: ModuloNode,
  identifier: IdentifierNode,
  numberLiteral: NumberLiteralNode,
}
