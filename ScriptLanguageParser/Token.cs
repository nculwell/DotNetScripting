using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguageParser
{
    class Token
    {
        public TokenType Type { get; }
        public string Text { get; }
        public Token(TokenType type) : this(type, null) { }
        public Token(TokenType type, string text)
        {
            Type = type;
            Text = text;
        }
        public decimal AsNumber()
        {
            return decimal.Parse(Text);
        }
    }
}
