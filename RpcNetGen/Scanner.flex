namespace RpcNetGen
{
    using System;
    using System.IO;
    using System.Text;
    using TUVienna.CS_CUP.Runtime;

%%

%class Scanner
%unicode
%implements TUVienna.CS_CUP.Runtime.Scanner
%function next_token
%type TUVienna.CS_CUP.Runtime.Symbol
%eofval{
    return new Symbol(Symbols.EOF);
%eofval}
%eofclose
%line
%column

%{
    StringBuilder strng = new StringBuilder();

    private Symbol symbol(int type)
    {
        return new Symbol(type, yyline + 1, yycolumn + 1);
    }

    private Symbol symbol(int type, object value)
    {
        return new Symbol(type, yyline + 1, yycolumn + 1, value);
    }
%}

// Macros
LINE_TERMINATOR=\r|\n|\r\n
INPUT_CHARACTER=[^\r\n]

WHITE_SPACE={LINE_TERMINATOR}|[ \t\f]

CS_COMMENT={MULTILINE_COMMENT}|{EOL_COMMENT}
MULTILINE_COMMENT = "/*"{COMMENT_CONTENT}\*+"/"
EOL_COMMENT="//".*{LINE_TERMINATOR}
COMMENT_CONTENT=([^*]|\*+[^*/])*

IDENTIFIER=[a-zA-Z_][a-zA-Z0-9_]*
INTEGER_LITERAL = [1-9][0-9]*|"0x"[0-9A-Fa-f]+|0[0-7]+|0|-[1-9][0-9]*

%%

<YYINITIAL> {
// keywords
    "program"           { return symbol(Symbols.PROGRAM); }
    "version"           { return symbol(Symbols.VERSION); }
    "PROGRAM"           { return symbol(Symbols.PROGRAM); }
    "VERSION"           { return symbol(Symbols.VERSION); }

    "const"             { return symbol(Symbols.CONST); }
    "typedef"           { return symbol(Symbols.TYPEDEF); }

    "switch"            { return symbol(Symbols.SWITCH); }
    "case"              { return symbol(Symbols.CASE); }
    "default"           { return symbol(Symbols.DEFAULT); }

// data types
    "void"              { return symbol(Symbols.VOID); }
    "char"              { return symbol(Symbols.CHAR); }
    "short"             { return symbol(Symbols.SHORT); }
    "Int16"             { return symbol(Symbols.SHORT); }
    "int16_t"           { return symbol(Symbols.SHORT); }
    "u_short"           { return symbol(Symbols.USHORT); }
    "UInt16"            { return symbol(Symbols.USHORT); }
    "uint16_t"          { return symbol(Symbols.USHORT); }
    "int"               { return symbol(Symbols.INT); }
    "Int32"             { return symbol(Symbols.INT); }
    "int32_t"           { return symbol(Symbols.INT); }
    "uint"              { return symbol(Symbols.UINT); }
    "u_int"             { return symbol(Symbols.UINT); }
    "UInt32"            { return symbol(Symbols.UINT); }
    "uint32_t"          { return symbol(Symbols.UINT); }
    "long"              { return symbol(Symbols.LONG); }
    "u_long"            { return symbol(Symbols.ULONG); }
    "hyper"             { return symbol(Symbols.HYPER); }
    "Int64"             { return symbol(Symbols.HYPER); }
    "int64_t"           { return symbol(Symbols.HYPER); }
    "uhyper"            { return symbol(Symbols.UHYPER); }
    "UInt64"            { return symbol(Symbols.UHYPER); }
    "uint64_t"          { return symbol(Symbols.UHYPER); }
    "float"             { return symbol(Symbols.FLOAT); }
    "double"            { return symbol(Symbols.DOUBLE); }
    "quadruple"         { return symbol(Symbols.QUADRUPLE); }
    "bool"              { return symbol(Symbols.BOOL); }
    "boolean"           { return symbol(Symbols.BOOL); }
    "bool_t"            { return symbol(Symbols.BOOL); }
    "enum"              { return symbol(Symbols.ENUM); }
    "opaque"            { return symbol(Symbols.OPAQUE); }
    "string"            { return symbol(Symbols.STRING); }
    "struct"            { return symbol(Symbols.STRUCT); }
    "union"             { return symbol(Symbols.UNION); }

// modifiers
    "unsigned"          { return symbol(Symbols.UNSIGNED); }

// separators
    ";"                 { return symbol(Symbols.SEMICOLON); }
    ","                 { return symbol(Symbols.COMMA); }
    ":"                 { return symbol(Symbols.COLON); }
    "="                 { return symbol(Symbols.EQUAL); }
    "*"                 { return symbol(Symbols.STAR); }
    "("                 { return symbol(Symbols.LPAREN); }
    ")"                 { return symbol(Symbols.RPAREN); }
    "{"                 { return symbol(Symbols.LBRACE); }
    "}"                 { return symbol(Symbols.RBRACE); }
    "["                 { return symbol(Symbols.LBRACKET); }
    "]"                 { return symbol(Symbols.RBRACKET); }
    "<"                 { return symbol(Symbols.LANGLE); }
    ">"                 { return symbol(Symbols.RANGLE); }

// integer literals
    {INTEGER_LITERAL} {
        return symbol(Symbols.INTEGER_LITERAL, yytext());
    }

// identifiers: simple return the identifier as the value of the symbol
    {IDENTIFIER} {
        return symbol(Symbols.IDENTIFIER, yytext());
    }

// white space & comment handling
    {WHITE_SPACE}       { /* ignore */ }
    {CS_COMMENT}      { /* ignore */ }
}

. | \n                  { throw new InvalidOperationException("Illegal character \"" + yytext() + "\""); }

%%
}
