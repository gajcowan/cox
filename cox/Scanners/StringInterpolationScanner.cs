using System;
using System.Collections.Generic;
using System.Text;

using lox.Tokens;

namespace lox
{
    public class StringInterpolationScanner : Lexer
    {
        public StringInterpolationScanner( String source) : base (source)
        {
        }

        public List<Token> Scan()
        {
            Tokens.Add(new Token(TokenType.INTERPOLATION_START, null, null, 0));
            Int32 Pos = 0;
            while (!IsAtEnd())
            {
                Char c = Advance();
                if ( c == '{')
                {
                    if (Match('{'))
                    {
                        // This is the end
                    }
                    else
                    {
                        
                        // We have a string literal
                        Tokens.Add( new Token(TokenType.STRING, Source.Substring(Pos, Current - Pos-1), Source.Substring(Pos, Current - Pos - 1), -1)) ;
                        Pos = Current;
                        while ((c = Advance()) != '}')
                        {
                        }
                        Tokens.AddRange(new Scanner(Source.Substring(Pos - 1, Current - Pos + 1)).Scan(false));
                        Pos = Current;
                    }
                }
            }
            if( Pos < Current)
                Tokens.Add(new Token(TokenType.STRING, Source.Substring(Pos, Current - Pos), Source.Substring(Pos, Current - Pos), -1));

            Tokens.Add(new Token(TokenType.INTERPOLATION_END, null, null, 0));
            return Tokens;
        }

        protected override void ScanToken()
        {
            
        }
    }
}
