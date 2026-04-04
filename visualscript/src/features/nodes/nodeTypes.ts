import type { NodeTypes } from '@xyflow/react'
import { AssignmentNode } from './AssignmentNode'
import { BinaryNode } from './BinaryNode'
import { FunctionCallNode } from './FunctionCallNode'
import { IdentifierNode } from './IdentifierNode'
import { LocalDeclarationNode } from './LocalDeclarationNode'
import { NumberLiteralNode } from './NumberLiteralNode'
import { ReturnNode } from './ReturnNode'

export const scriptNodeTypes: NodeTypes = {
  localDecl: LocalDeclarationNode,
  assignment: AssignmentNode,
  return: ReturnNode,
  functionCall: FunctionCallNode,
  binary: BinaryNode,
  identifier: IdentifierNode,
  numberLiteral: NumberLiteralNode,
}
