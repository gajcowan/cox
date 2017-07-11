using System;
using System.Collections.Generic;
using System.Text;

using lox;
using lox.Tokens;

namespace lox
{

    public class RuntimeError : Exception
    {
        public Token Token;

        public RuntimeError(Token token, String message) : base(message)
        {
            Token = token;
        }
    }

    public class BreakException : Exception
    {
    }
}
