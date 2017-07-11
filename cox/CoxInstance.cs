using cox.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace cox
{
    public class CoxInstance
    {
        private CoxClass Klass;
        public Dictionary<String, Object> Fields = new Dictionary<String, Object>();

        public CoxInstance(CoxClass klass)
        {
            Klass = klass;
        }

        public Object GetProperty(Token name)
        {
            if (Fields.ContainsKey(name.Lexeme))
            {
                return Fields[name.Lexeme];
            }

            CoxFunction method = Klass.FindMethod(this, name.Lexeme);
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
