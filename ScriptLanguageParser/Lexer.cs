using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScriptLanguageParser
{
    class Lexer
    {
        private readonly string _sourceCode;
        private int _offset;
        private int _lineStartOffset;
        private readonly Regex _identifier = new Regex(@"^\p{alpha}\p{alnum}*");
        private readonly Regex _string = new Regex("^\"([^\"]|\"\")*\"");
        private readonly Regex _number = new Regex(@"^(\p{digit}+(\.\p{digit}*)?|\.\p{digit}+)");

        /// <summary>
        /// The line in the source code that the current token is in.
        /// </summary>
        public int Line { get; private set; }
        /// <summary>
        /// The column in the source code that the current token begins in.
        /// </summary>
        public int Column { get { return _offset - _lineStartOffset; } }
        /// <summary>
        /// The token that the lexer is currently "pointing" to.
        /// </summary>
        public Token CurrentToken { get; private set; }
        /// <summary>
        /// The token after CurrentToken. (Used for lookahead; commonly called "peek".)
        /// </summary>
        public Token FollowingToken { get; private set; }

        public Lexer(string sourceCode)
        {
            _sourceCode = sourceCode;
            _offset = 0;
            _lineStartOffset = 0;
            Line = 1;
            CurrentToken = null;
            FollowingToken = ScanNextToken();
        }
        public void Advance()
        {
            CurrentToken = FollowingToken;
            FollowingToken = ScanNextToken();
        }
        private Token ScanNextToken()
        {
            // Skip whitespace.
            while (_offset < _sourceCode.Length && char.IsWhiteSpace(_sourceCode[_offset]))
            {
                if (_sourceCode[_offset] == '\n')
                {
                    _lineStartOffset = _offset + 1;
                    Line++;
                }
                _offset = _offset + 1;
            }
            // EOF.
            if (_offset == _sourceCode.Length)
            {
                return new Token(TokenType.EOF);
            }
            // Punctuation tokens.
            switch (_sourceCode[_offset])
            {
                case ',': return new Token(TokenType.COMMA);
                case '.': return new Token(TokenType.DOT);
                case '(': return new Token(TokenType.LPAREN);
                case ')': return new Token(TokenType.RPAREN);
                case '{': return new Token(TokenType.LBRACE);
                case '}': return new Token(TokenType.RBRACE);
                case '+': return new Token(TokenType.ADD);
                case '-': return new Token(TokenType.SUB);
                case '*': return new Token(TokenType.MUL);
                case '/': return new Token(TokenType.DIV);
                case '=': return new Token(TokenType.EQU);
                case '<': return new Token(TokenType.EQU);
                case '>': return new Token(TokenType.EQU);
            }
            // Identifier or keyword.
            Match m = _identifier.Match(_sourceCode, _offset);
            if (m.Success)
            {
                var tokenText = m.Groups[0].Value;
                _offset += tokenText.Length;
                switch (tokenText)
                {
                    case "const": return new Token(TokenType.KWD_CONST);
                    case "var": return new Token(TokenType.KWD_VAR);
                    case "set": return new Token(TokenType.KWD_SET);
                    case "func": return new Token(TokenType.KWD_FUNC);
                    case "and": return new Token(TokenType.KWD_AND);
                    case "or": return new Token(TokenType.KWD_OR);
                    case "not": return new Token(TokenType.KWD_NOT);
                    case "if": return new Token(TokenType.KWD_IF);
                    case "then": return new Token(TokenType.KWD_THEN);
                    case "else": return new Token(TokenType.KWD_ELSE);
                    case "for": return new Token(TokenType.KWD_FOR);
                    case "end": return new Token(TokenType.KWD_END);
                    default: return new Token(TokenType.IDENTIFIER, tokenText);
                }
            }
            m = _string.Match(_sourceCode, _offset);
            if (m.Success)
            {
                var tokenText = m.Groups[0].Value;
                _offset += tokenText.Length;
                return new Token(TokenType.STRING, tokenText.Substring(1, tokenText.Length - 2));
            }
            m = _number.Match(_sourceCode, _offset);
            if (m.Success)
            {
                var tokenText = m.Groups[0].Value;
                _offset += tokenText.Length;
                return new Token(TokenType.NUMBER, tokenText);
            }
            throw new SyntaxException(this, "Unexpected character scanned.");
        }
    }
}
