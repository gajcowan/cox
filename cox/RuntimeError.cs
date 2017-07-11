using System;
using System.Collections.Generic;
using System.Text;

using cox;
using cox.Tokens;

namespace cox
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
