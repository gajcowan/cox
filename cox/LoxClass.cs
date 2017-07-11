using System;
using System.Collections.Generic;
using System.Text;

namespace lox
{
    public class LoxClass
    {
        public String Name;
        private LoxClass Superclass;
        private Dictionary<String, LoxFunction> Methods;

        public LoxClass(String name, LoxClass superclass, Dictionary<String, LoxFunction> methods)
        {
            Name = name;
            Superclass = superclass;
            Methods = methods;
        }

        public LoxFunction FindMethod(LoxInstance instance, String name)
        {
            LoxClass klass = this;
            while (klass != null)
            {
                if (klass.Methods.ContainsKey(name))
                {
                    return klass.Methods[name].Bind(instance);
                }

                klass = klass.Superclass;
            }

            return null;
        }

        override public String ToString()
        {
            return Name;
        }

        public int RequiredArguments()
        {
            LoxFunction initializer = Methods["init"];
            if (initializer == null)
                return 0;

            return initializer.RequiredArguments();
        }

        public Object Call(Interpreter interpreter, List<Object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);

            LoxFunction initializer = Methods["init"];
            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }
    }
}


