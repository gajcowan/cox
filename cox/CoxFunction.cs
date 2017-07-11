using System;
using System.Collections.Generic;
using System.Text;

using cox.AbstractSyntaxTree;

namespace cox
{
    public class CoxFunction : ICallable
    {
        private Function Declaration;
        private Environment Closure;
        private Boolean IsInitializer;

        public CoxFunction(Function declaration, Environment closure, Boolean isInitializer)
        {
            Declaration = declaration;
            Closure = closure;
            IsInitializer = isInitializer;
        }

        public CoxFunction Bind(CoxInstance self)
        {
            Environment environment = new Environment(Closure);
            environment.Define("this", self);
            return new CoxFunction(Declaration, environment, IsInitializer);
        }

        override public String ToString()
        {
            return Declaration.Name.Lexeme;
        }

        public int RequiredArguments()
        {
            return Declaration.Parameters.Count;
        }

        public Object Call(Interpreter interpreter, List<Object> arguments)
        {
            Object result = null;

            try
            {
                Environment environment = new Environment(Closure);
                for (int i = 0; i < Declaration.Parameters.Count; i++)
                {
                    environment.Define(Declaration.Parameters[i].Lexeme, arguments[i]);
                }

                interpreter.ExecuteBlock(Declaration.Body, environment);
            }
            catch(ReturnEX returnValue)
            {
                result = returnValue.Value;
            }

            return IsInitializer ? Closure.GetAt(0, "this") : result;
        }
    }
}
