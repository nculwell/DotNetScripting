using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptLanguageParser
{
    public enum TokenType
    {
        /// <summary> End-of-file marker. </summary>
        EOF,

        /// <summary> Identifer (constant/variable/function name). </summary>
        IDENTIFIER,
        /// <summary> String literal. </summary>
        STRING,
        /// <summary> Number literal. </summary>
        NUMBER,

        /// <summary> Period (.). </summary>
        DOT,
        /// <summary> Left parenthesis [(]. </summary>
        LPAREN,
        /// <summary> Right parenthesis [)]. </summary>
        RPAREN,
        /// <summary> Plus (+). </summary>
        ADD,
        /// <summary> Minus (-). </summary>
        SUB,
        /// <summary> Asterisk (*). </summary>
        MUL,
        /// <summary> Slash (/). </summary>
        DIV,
        /// <summary> Equals (=). </summary>
        EQU,
        /// <summary> Comma (,). </summary>
        COMMA,
        /// <summary> Left curly brace/bracket ({). </summary>
        LBRACE,
        /// <summary> Right curly brace/bracket (}). </summary>
        RBRACE,

        // Keywords
        KWD_CONST,
        KWD_VAR,
        KWD_SET,
        KWD_FUNC,
        KWD_AND,
        KWD_OR,
        KWD_NOT,
        KWD_IF,
        KWD_THEN,
        KWD_ELSE,
        KWD_FOR,
        KWD_END,

    }
}
