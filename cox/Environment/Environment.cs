using lox.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace lox
{
    public class Environment
    {
        public Environment Enclosing;

        private Dictionary<String, Object> values = new Dictionary<String, Object>();
        
        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment Enclosing)
        {
            this.Enclosing = Enclosing;
        }

        public Object Get(Token name)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                return values[name.Lexeme];
            }

            if (Enclosing != null)
                return Enclosing.Get(name);

            throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
        }

        public void Assign(Token name, Object value)
        {
            if(values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return;
            }

            if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
        }

        public void Define(String name, Object value)
        {
            values.Add(name, value);
        }

        public Object GetAt(int distance, String name)
        {
            Environment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.Enclosing;
            }

            return environment.values[name];
        }

        void AssignAt(int distance, Token name, Object value)
        {
            Environment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.Enclosing;
            }

            environment.values.Add(name.Lexeme, value);
        }

        public override String ToString()
        {
            String result = values.ToString();
            //if (Enclosing != null)
            //{
            //    result += " -> " + Enclosing.ToString();
            //}

            return result;
        }
    }
}
