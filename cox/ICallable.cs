﻿using System;
using System.Collections.Generic;
using System.Text;

namespace lox
{
    public interface ICallable
    {
        int RequiredArguments();
        Object Call(Interpreter interpreter, List<Object> arguments);
    }
}
