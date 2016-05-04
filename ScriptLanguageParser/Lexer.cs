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
        private readonly Regex _identifier = new Regex(@"^[A-Za-z][A-Za-z0-9]*");
        private readonly Regex _string = new Regex("^\"([^\"]|\"\")*\"");
        private readonly Regex _number = new Regex(@"^([0-9]+(\.[0-9]*)?|\.[0-9]+)");

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

            // Save the start of the token and advance the offset to the next character.
            var tokenStartOffset = _offset;
            var firstChar = _sourceCode[_offset];
            _offset += 1;

            // Punctuation tokens.
            switch (firstChar)
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
                case '<': return new Token(TokenType.GT);
                case '>': return new Token(TokenType.LT);
            }

            // Identifier or keyword.
            if (char.IsLetter(firstChar))
            {
                while (_offset < _sourceCode.Length && char.IsLetterOrDigit(_sourceCode[_offset]))
                {
                    _offset += 1;
                }
                var tokenText = _sourceCode.Substring(tokenStartOffset, _offset - tokenStartOffset);
                // See if this token is a keyword. If not, it's an identifier.
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

            // Number beginning with a digit.
            if (char.IsDigit(firstChar))
            {
                // Read a string of digits.
                while (_offset < _sourceCode.Length && char.IsDigit(_sourceCode[_offset]))
                {
                    _offset += 1;
                }
                // If the next character is a decimal point, include it, and read more digits following.
                if (_offset < _sourceCode.Length && _sourceCode[_offset] == '.')
                {
                    do _offset += 1;
                    while (_offset < _sourceCode.Length && char.IsDigit(_sourceCode[_offset]));
                }
                var tokenText = _sourceCode.Substring(tokenStartOffset, _offset - tokenStartOffset);
                return new Token(TokenType.NUMBER, tokenText);
            }

            // Number beginning with a decimal point.
            if (firstChar == '.')
            {
                // Read a string of digits.
                while (_offset < _sourceCode.Length && char.IsDigit(_sourceCode[_offset]))
                {
                    _offset += 1;
                }
                var tokenText = _sourceCode.Substring(tokenStartOffset, _offset - tokenStartOffset);
                return new Token(TokenType.NUMBER, tokenText);
            }

            // Quoted string.
            if (firstChar == '"')
            {
                // Read until matching quote is found. Repeat as long as the character
                // following the matching quote is another quote. This has the effect
                // of escaping a quote character by doubling it.
                do
                {
                    do
                    {
                        _offset += 1;
                    } while (_offset < _sourceCode.Length && _sourceCode[_offset] != '"');
                    _offset += 1;
                } while (_offset < _sourceCode.Length && _sourceCode[_offset] == '"');
                if (_offset == _sourceCode.Length)
                {
                    throw new SyntaxException(this, "Unterminated string literal.");
                }
                // Text of string token strips out the beginning and ending quotes
                // and reduces quote pairs to single quote characters.
                var tokenText = _sourceCode.Substring(tokenStartOffset + 1, _offset - tokenStartOffset - 2);
                tokenText = tokenText.Replace("\"\"", "\"");
                return new Token(TokenType.STRING, tokenText);
            }

            throw new SyntaxException(this, "Unexpected character scanned ('"+ firstChar + "').");
        }
    }
}
