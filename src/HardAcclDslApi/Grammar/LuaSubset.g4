grammar LuaSubset;

chunk
    : statement* EOF
    ;

statement
    : localAssignStmt
    | assignStmt
    | returnStmt
    | callStmt
    ;

localAssignStmt
    : LOCAL NAME ASSIGN expression
    ;

assignStmt
    : NAME ASSIGN expression
    ;

returnStmt
    : RETURN expression
    ;

callStmt
    : functionCall
    ;

functionCall
    : NAME LPAREN argumentList? RPAREN
    ;

argumentList
    : expression (COMMA expression)*
    ;

expression
    : additiveExpr
    ;

additiveExpr
    : multiplicativeExpr ((PLUS | MINUS) multiplicativeExpr)*
    ;

multiplicativeExpr
    : primaryExpr ((STAR | SLASH) primaryExpr)*
    ;

primaryExpr
    : NUMBER
    | STRING
    | NAME
    | functionCall
    | LPAREN expression RPAREN
    ;

LOCAL  : 'local';
RETURN : 'return';
ASSIGN : '=';
PLUS   : '+';
MINUS  : '-';
STAR   : '*';
SLASH  : '/';
COMMA  : ',';
LPAREN : '(';
RPAREN : ')';

NAME
    : [a-zA-Z_] [a-zA-Z0-9_]*
    ;

NUMBER
    : [0-9]+ ('.' [0-9]+)?
    ;

STRING
    : '"' ( '\\' . | ~["\\\r\n] )* '"'
    | '\'' ( '\\' . | ~['\\\r\n] )* '\''
    ;

LINE_COMMENT
    : '--' ~[\r\n]* -> skip
    ;

WS
    : [ \t\r\n]+ -> skip
    ;
