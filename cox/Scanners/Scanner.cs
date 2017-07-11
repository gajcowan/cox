using System;
using System.Collections.Generic;

using lox.Tokens;


namespace lox
{
    public class Scanner : Lexer
    {

        public Scanner(String source) : base( source)
        {
        }

        protected override void ScanToken()
        {
            Char c = Advance();
            switch (c)
            {
                case '[':
                    AddToken(TokenType.LSQRBRACE); // TODO
                    break;
                case ']':
                    AddToken(TokenType.RSQRBRACE); // TODO
                    break;
                case '$':
                    AddToken(TokenType.DOLLAR);
                    if (Match('"'))
                    {
                        var value = String().Substring(1); // Strip of the leading quote
                        var scanner = new StringInterpolationScanner(value);
                        var t = scanner.Scan();
                        Tokens.AddRange(t);
                    }
                    else
                    {
                        Lox.Error(Line, $"Expected '\"'");
                    }
                    break;
                case '(':
                    AddToken(TokenType.LPAREN);
                    break;
                case ')':
                    AddToken(TokenType.RPAREN);
                    break;
                case '{':
                    AddToken(TokenType.LBRACE);
                    break;
                case '}':
                    AddToken(TokenType.RBRACE);
                    break;
                case ',':
                    AddToken(TokenType.COMMA);
                    break;
                case '.':
                    AddToken(TokenType.DOT);
                    break;
                case '-':
                    if (Match('-'))
                        AddToken(TokenType.MINUS_MINUS);
                    else
                    {
                        if (Match('='))
                            AddToken(TokenType.MINUS_EQUALS);
                        else
                            AddToken(TokenType.MINUS);
                    }
                    break;
                case '+':
                    if (Match('+'))
                        AddToken(TokenType.PLUS_PLUS);
                    else
                    {
                        if (Match('='))
                            AddToken(TokenType.PLUS_EQUALS);
                        else
                            AddToken(TokenType.PLUS);
                    }
                    break;
                case '*':
                    if (Match('='))
                        AddToken(TokenType.MUL_EQUALS);
                    else
                        AddToken(TokenType.MUL);
                    break;
                case '/':
                    if (Match('='))
                        AddToken(TokenType.DIV_EQUALS);
                    else
                    {
                        if (Match('/'))
                        {
                            // A comment goes until the end of the line.
                            while (Peek() != '\n' && !IsAtEnd())
                                Advance();
                        }
                        else
                        {
                            if (Match('*'))
                            {
                                // Multi line comment, we need to look for a */
                                while (!(Peek() == '*' && PeekNext() == '/') && !IsAtEnd())
                                {
                                    if (Peek() == '\n')
                                        Line++;

                                    Advance();
                                }
                                if(IsAtEnd())
                                {
                                    Lox.Error(Line, $"End-of-File found, '*/' expected");
                                }
                                else
                                {
                                    Advance();
                                    Advance();
                                }
                            }
                            else
                            {
                                AddToken(TokenType.DIV);
                            }
                        }
                    }
                    break;
                case ';':
                    AddToken(TokenType.SEMICOLON);
                    break;
                case ':':
                    AddToken(TokenType.COLON);
                    break;
                case '?':
                    AddToken(TokenType.QUESTIONMARK);
                    break;
                case '&':
                    if (Match('&'))
                        AddToken(TokenType.AND);
                    else
                        AddToken(TokenType.BINARY_AND);
                    break;
                case '|':
                    if (Match('|'))
                        AddToken(TokenType.OR);
                    else
                        AddToken(TokenType.BINARY_OR);
                    break;
                case '^':
                    AddToken(TokenType.BINARY_XOR);
                    break;
                case '~':
                    AddToken(TokenType.BINARY_NOT);
                    break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    if (Match('<'))
                        AddToken(TokenType.BINARY_LEFTSHIFT);
                    else
                        if (Match('='))
                            AddToken(TokenType.LESS_EQUAL);
                        else
                            AddToken(TokenType.LESS);
                    break;
                case '>':
                    if (Match('>'))
                       AddToken(TokenType.BINARY_RIGHTSHIFT);
                    else
                        if (Match('='))
                            AddToken(TokenType.GREATER_EQUAL);
                        else
                            AddToken(TokenType.GREATER);
                    break;
                case '"':
                    {
                        String value = String();
                        if (value != null)
                            AddToken(TokenType.STRING, value);
                    }
                    break;
                case ' ':
                case '\t':
                case '\r':
                    break;
                case '\n':
                    Line++;
                    break;
                default:
                    if(Char.IsDigit(c))
                        Number();
                    else
                        if(Char.IsLetter(c) || c == '_')
                            Identifier();
                        else
                            Lox.Error(Line, $"Unexpected Character {c}");
                    break;
            }
        }
        
        public String String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if(Peek() == '\\')
                {
                    Advance();
                }

                if(Peek() == '\n')
                    Line++;

                Advance();
            }

            // Unterminated string.
            if(IsAtEnd())
            {
                Lox.Error(Line, "Unterminated string.");
                return null;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            return Source.Substring(Start + 1
                , Current-Start-2);
        }

        private void CharString()
        {
            while (Peek() != '\'' && !IsAtEnd())
            {
                if(Peek() == '\\')
                {
                    Advance();
                }
                if(Peek() == '\n')
                    Line++;

                Advance();
            }

            // Unterminated string.
            if(IsAtEnd())
            {
                Lox.Error(Line, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            String value = Source.Substring(Start + 1, Current-Start- 1);
            AddToken(TokenType.STRING, value);
        }

    }
}
