using System;
using System.Collections.Generic;
using System.Text;

namespace cox
{
    public class CoxClass
    {
        public String Name;
        private CoxClass Superclass;
        private Dictionary<String, CoxFunction> Methods;

        public CoxClass(String name, CoxClass superclass, Dictionary<String, CoxFunction> methods)
        {
            Name = name;
            Superclass = superclass;
            Methods = methods;
        }

        public CoxFunction FindMethod(CoxInstance instance, String name)
        {
            CoxClass klass = this;
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
            CoxFunction initializer = Methods["init"];
            if (initializer == null)
                return 0;

            return initializer.RequiredArguments();
        }

        public Object Call(Interpreter interpreter, List<Object> arguments)
        {
            CoxInstance instance = new CoxInstance(this);

            CoxFunction initializer = Methods["init"];
            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }
    }
}


