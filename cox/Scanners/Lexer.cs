using System;
using System.Collections.Generic;

using lox.Tokens;

namespace lox
{
    public abstract class Lexer
    {
        protected String Source;
        protected List<Token> Tokens = new List<Token>();

        protected Int32 Start = 0;
        protected Int32 Current = 0;
        protected Int32 Line = 1;

        public Lexer(String source)
        {
            Source = source;
        }

        public List<Token> Scan(Boolean withEOF = true)
        {
            ScanTokens();
            if (withEOF)
                Tokens.Add(new Token(TokenType.EOF, "", null, Line));

            return Tokens;
        }

        private void ScanTokens()
        {
            while (!IsAtEnd())
            {
                Start = Current;
                ScanToken();
            }
        }

        protected Boolean IsAtEnd()
        {
            return Current >= Source.Length;
        }

        abstract protected void ScanToken();
        
        protected char Advance()
        {
            Current++;
            return Source[Current - 1];
        }

        protected Boolean Match(Char expected)
        {
            if(IsAtEnd())
                return false;

            if(Source[Current] != expected)
                return false;

            Current++;
            return true;
        }

        protected char Peek()
        {
            if(Current >= Source.Length)
                return '\0';

            return Source[Current];
        }

        protected char PeekNext()
        {
            if(Current + 1 >= Source.Length)
                return '\0';

            return Source[Current + 1];
        }

        protected void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        protected void AddToken(TokenType type, Object literal)
        {
            String text = Source.Substring(Start, Current-Start);
            Tokens.Add(new Token(type, text, literal, Line));
        }

        protected void Number()
        {
            while (Char.IsDigit(Peek()))
                Advance();

            // Look for a fractional part.
            if(Peek() == '.' && Char.IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (Char.IsDigit(Peek()))
                    Advance();
            }

            AddToken(TokenType.NUMBER, Double.Parse(Source.Substring(Start, Current-Start)));
        }

        protected void Identifier()
        {
            while (Char.IsLetterOrDigit(Peek()) || Peek() == '_' || Peek() == '.')
                Advance();

            // See if the identifier is a reserved word.
            String text = Source.Substring(Start, Current-Start);

            TokenType? type = Keywords.GetKeyWordTokenType(text);
            if(type == null)
                type = TokenType.IDENTIFIER;
            AddToken((TokenType)type);
        }

    }
}
