using lox.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace lox
{
    public class LoxInstance
    {
        private LoxClass Klass;
        public Dictionary<String, Object> Fields = new Dictionary<String, Object>();

        public LoxInstance(LoxClass klass)
        {
            Klass = klass;
        }

        public Object GetProperty(Token name)
        {
            if (Fields.ContainsKey(name.Lexeme))
            {
                return Fields[name.Lexeme];
            }

            LoxFunction method = Klass.FindMethod(this, name.Lexeme);
            if (method != null)
                return method;

            throw new RuntimeError(name, "Undefined property '" + name.Lexeme + "'.");
        }

        override public String ToString()
        {
            return Klass.Name + " instance";
        }
    }
}
