using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptLanguageParser
{
    public enum TokenType
    {
        EOF,
        IDENTIFIER,
        STRING,
        NUMBER,
        DOT,
        LPAREN,
        RPAREN,
        ADD,
        SUB,
        MUL,
        DIV,
        EQU,
        KWD_CONST,
        KWD_VAR,
        KWD_SET,
        KWD_FUNC,
        KWD_AND,
        KWD_OR,
        KWD_NOT,
        COMMA,
        LBRACKET,
        RBRACKET,
    }
}
