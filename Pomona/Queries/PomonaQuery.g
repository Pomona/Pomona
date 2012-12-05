grammar PomonaQuery;

options {
    language=CSharp3;
    output=AST;
}


tokens {
   ROOT;
   AND_OP;
   OR_OP;
   LT_OP;
   GT_OP;
   LE_OP;
   GE_OP;
   EQ_OP;
   MUL_OP;
   DIV_OP;
   NE_OP;
   ADD_OP;
   SUB_OP;
   MOD_OP;
   DOT_OP;
   AS_OP;
   DATETIME_LITERAL;
   GUID_LITERAL;
   METHOD_CALL;
   INDEXER_ACCESS;
   LAMBDA_OP;
}    


@parser::namespace { Pomona.Queries }
@lexer::namespace { Pomona.Queries }


@lexer::members {const int HIDDEN = Hidden;}


PREFIXED_STRING
	:	('a'..'z'|'A'..'Z') ('a'..'z'|'A'..'Z'|'0'..'9')*  '\'' ( ~( '\\' | '\'' ) )* '\''
	;

ID  :	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*	
    ;

INT :	'0'..'9'+ ('.' ('0'..'9')+)? ('m'|'M'|'f'|'F')?
    ;



WS  :   ( ' '
        | '\t'
        | '\r'
        | '\n'
        ) {$channel=HIDDEN;}
    ;

/*
STRING
    :  '\'' ( ESC_SEQ | ~('\\'|'\'') )* '\''
    ;
*/
STRING
    :  ( '\'' ( ~( '\\' | '\'' ) )* '\'' )+
    ;
    
    
public parse 
	:	exp EOF -> ^(ROOT exp)
	;

exp
	:	as_expression
	;

as_expression
	: lambda_expression ( 'as' lambda_expression )? -> ^(AS_OP lambda_expression+)
	;

lambda_expression
	:	or_expression ( ':' or_expression )? -> ^(LAMBDA_OP or_expression+)
	;

or_expression
	:	and_expression ( 'or' and_expression )* -> ^(OR_OP and_expression+)
	;

and_operator
	:	'and';
	

and_expression 
	: relational_expr ( and_operator relational_expr )* -> ^(AND_OP relational_expr+)
	;



relational_operator 
	: 'gt' -> GT_OP
	| 'lt' -> LT_OP
	| 'eq' -> EQ_OP
	| 'ge' -> GE_OP
	| 'le' -> LE_OP
	| 'ne' -> NE_OP
	;
/*
relational_operator 
	: '>'
	| '<'
	| '=='
	| '>='
	| '<='
	;
*/
relational_expr 
	:	additive_expr (relational_operator^ additive_expr)?
	;

additive_operator
    : 'add' -> ADD_OP
    | 'sub' -> SUB_OP
    ;

additive_expr 
	:	multiplicative_expr ( additive_operator^ multiplicative_expr )*
	;

multiplicative_operator
    :   'mul' -> MUL_OP
    |   'div' -> DIV_OP
	|   'mod' -> MOD_OP
    ;

multiplicative_expr 
	:	unary_expr ( multiplicative_operator^ unary_expr )*
	;

unary_operator 
	:	'not'
	;

dot_operator
	:	'.' -> DOT_OP
	;


unary_expr 
	: unary_operator unary_expr
	| primary_expr
	;

primary_expr
	: postfix_expr ( dot_operator^ postfix_expr )*
	;

postfix_expr
	:	ID ( '(' arglist_expr ')' ) -> ^(METHOD_CALL ID arglist_expr)
	|	ID ( '(' ')' ) -> ^(METHOD_CALL ID)
	|	ID ( '[' arglist_expr ']' ) -> ^(INDEXER_ACCESS ID arglist_expr)
	|	ID ( '[' ']' ) -> ^(INDEXER_ACCESS ID)
	| ID
	| STRING
	| INT
	| '('! exp ')'!
	| PREFIXED_STRING
	;

arglist_expr 
	:	exp ( ','! exp )*
	;
	/*
constant_expr
	:	INT
	| STRING
	;
*/

fragment
HEX_DIGIT : ('0'..'9'|'a'..'f'|'A'..'F') ;

fragment
ESC_SEQ
    :   '\\' ('b'|'t'|'n'|'f'|'r'|'\"'|'\''|'\\')
    |   UNICODE_ESC
    |   OCTAL_ESC
    ;

fragment
OCTAL_ESC
    :   '\\' ('0'..'3') ('0'..'7') ('0'..'7')
    |   '\\' ('0'..'7') ('0'..'7')
    |   '\\' ('0'..'7')
    ;

fragment
UNICODE_ESC
    :   '\\' 'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
    ;
