using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguageParser
{
    internal class Token
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
        public override string ToString()
        {
            switch (Type)
            {
                case TokenType.IDENTIFIER: return "ID(" + Text + ")";
                case TokenType.STRING: return "String(\"" + Text.Replace("\"", "\"\"") + "\")";
                case TokenType.NUMBER: return "Number(" + AsNumber().ToString() + ")";
                default: return Type.ToString();
            }
        }
    }
}
