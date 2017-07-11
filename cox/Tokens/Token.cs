using System;
using System.Collections.Generic;
using System.Text;

namespace cox.Tokens
{
    public class Token
    {
        public TokenType Type;
        public String Lexeme;
        public Object Literal;
        public Int32 Line;

        public Token(TokenType type, String lexeme, Object literal, Int32 line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override String ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}
