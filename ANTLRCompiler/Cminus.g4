grammar Cminus;

/*
 * Parser Rules
 */

 programa
	: declaracaolista
	;

 declaracaolista
	: declaracao
	| declaracaolista declaracao
	;

 declaracao
	: vardeclaracao
	| fundeclaracao
	;

 vardeclaracao
	: tipoespecificador ID ';'				#inteirodeclaracao
	| tipoespecificador ID '[' NUM ']' ';'	#vetordeclaracao
	;

 tipoespecificador
	: 'int'
	| 'void'
	;

 fundeclaracao
	: tipoespecificador ID '(' params ')' compostodecl
	;

 params
	: paramlista
	| 'void'
	;

 paramlista
	: param
	| paramlista ',' param
	;
 
 param
	: tipoespecificador ID '[' ']'	#vetorparam
	| tipoespecificador ID			#inteiroparam
	;

 compostodecl
	: '{' localdeclaracoes statementlista '}'
	| '{' localdeclaracoes '}'
	| '{' statementlista '}'
	| '{' '}'
	;

 localdeclaracoes
	: vardeclaracao
	| localdeclaracoes vardeclaracao
	;

 statementlista
	: statement
	| statementlista statement
	;

 statement
	: expressaodecl
	| compostodecl
	| selecaodecl
	| iteracaodecl
	| retornodecl
	;

 expressaodecl
	: expressao ';'
	| ';'
	;

 selecaodecl
	: 'if' '(' expressao ')' statement						#selecaoif
	| 'if' '(' expressao ')' statement 'else' statement		#selecaoifelse
	;

 iteracaodecl
	: 'while' '(' expressao ')' statement
	;

 retornodecl
	: 'return' ';'				#retornonull
	| 'return' expressaodecl	#retornovalor
	;

 expressao
	: var '=' expressao   #expressaoatribuicao
	| simplesexpressao    #expressaosimples
	;

 var
	: ID '[' expressao ']'	#varvetor
	| ID					#varnormal
	;

 simplesexpressao
	: somaexpressao relacional somaexpressao	#simplesexpressaorelacional
	| somaexpressao								#simplesexpressaosomaexpressao
	;

 relacional
	: '<='
	| '<'
	| '>'
	| '>='
	| '=='
	| '!='
	;


 somaexpressao
	: termo						#somaexpressaotermo
	| somaexpressao soma termo	#somaexpressaosomatermo
	;

 soma
	: '+'
	| '-'
	;

 termo
	: fator				#termofator
	| termo mult fator	#termomultfator
	;

 mult
	: '*'
	| '/'
	;

 fator
	: '(' expressao ')'	#fatorexpressao
	| var				#fatorvar
	| ativacao			#fatorativacao
	| NUM				#fatornum
	;

 ativacao
	: ID '(' arglista ')'	
	| ID '(' ')'
	;

 arglista
	: expressao					#unicoarg
	| arglista ',' expressao	#listaargs
	;

compileUnit
	:	EOF
	;



/*
 * Lexer Rules
 */

ID
	: [a-zA-Z]+
	;

NUM
	: [0-9]+
	;

WHITESPACE
	: ['\n''\r''\t'' ']
		-> skip
	;

COMMENT
	: '/*' .*? '*/'	-> skip
	;

WS
	:	' ' -> channel(HIDDEN)
	;
