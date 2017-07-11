using System;
using System.Collections.Generic;
using System.Text;

using lox.AbstractSyntaxTree;
using lox.Tokens;

namespace lox
{
    public class ReturnEX : Exception
    {
        public Object Value;

        public ReturnEX( Object value)
        {
            Value = value;
        }
    }
    public class Interpreter : IVisitorExpr<Object>, IVisitorStmt<Object>
    {
        private Environment Globals;
        private Environment Environment;
        private Dictionary<Expr, Int32> Locals;

        public Interpreter()
        {
            Globals = new Environment();
            Environment = Globals;
        }

        public void Interpret(List<Stmt> statements, Dictionary<Expr, Int32> locals)
        {
            Locals = locals;

            try
            {
                foreach(Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = Environment;
            try
            {
                Environment = environment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                Environment = previous;
            }
        }

        private Object Evaluate(Expr expr)
        {
            if (expr is null)
                return null;

            return expr.Accept(this);
        }

        public object VisitAssignExpr(Assign expr)
        {
            Object value = Evaluate(expr.Value);

            Environment.Assign(expr.Name, value);
            return value; 
        }

        public object VisitBinaryExpr(Binary expr)
        {
            Object left = Evaluate(expr.Left);
            Object right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.BINARY_AND:
                    CheckNumberOperands(expr.Op, left, right);
                    return Convert.ToInt32(left) & Convert.ToInt32(right);

                case TokenType.BINARY_OR:
                    CheckNumberOperands(expr.Op, left, right);
                    return Convert.ToInt32(left) | Convert.ToInt32(right);

                case TokenType.BINARY_XOR:
                    CheckNumberOperands(expr.Op, left, right);
                    return Convert.ToInt32(left) ^ Convert.ToInt32(right);

                case TokenType.BINARY_LEFTSHIFT:
                    CheckNumberOperands(expr.Op, left, right);
                    return Convert.ToInt32(left) << Convert.ToInt32(right);

                case TokenType.BINARY_RIGHTSHIFT:
                    CheckNumberOperands(expr.Op, left, right);
                    return Convert.ToInt32(left) >> Convert.ToInt32(right);

                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);

                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);

                case TokenType.GREATER:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left > (double)right;

                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left >= (double)right;

                case TokenType.LESS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left < (double)right;

                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left <= (double)right;

                case TokenType.MINUS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left - (double)right;

                case TokenType.DIV:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left / (double)right;

                case TokenType.MUL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left * (double)right;

                case TokenType.PLUS_EQUALS:
                    {
                        CheckNumberOperands(expr.Op, left, right);
                        double value = (double)left;
                        value += (double)right;
                        if (left is Variable variable)
                            Environment.Assign(variable.Name, value);

                        return value;
                    }

                case TokenType.MINUS_EQUALS:
                    {
                        CheckNumberOperands(expr.Op, left, right);
                        double value = (double)left;
                        value -= (double)right;
                        if (left is Variable variable)
                            Environment.Assign(variable.Name, value);

                        return value;
                    }

                case TokenType.MUL_EQUALS:
                    {
                        CheckNumberOperands(expr.Op, left, right);
                        double value = (double)left;
                        value *= (double)right;
                        if (left is Variable variable)
                            Environment.Assign(variable.Name, value);

                        return value;
                    }

                case TokenType.DIV_EQUALS:
                    {
                        CheckNumberOperands(expr.Op, left, right);
                        double value = (double)left;
                        value /= (double)right;
                        if (left is Variable variable)
                            Environment.Assign(variable.Name, value);

                        return value;
                    }

                case TokenType.PLUS_PLUS:
                    {
                        CheckNumberOperand(expr.Op, left);
                        double value = (double)left;
                        if (left is Variable variable)
                            Environment.Assign(variable.Name, value + 1);
                        return value;
                    }

                case TokenType.MINUS_MINUS:
                    {
                        CheckNumberOperand(expr.Op, left);
                        double value = (double)left;
                        if (expr.Left is Variable variable)
                            Environment.Assign(variable.Name, value - 1);
                        return value;
                    }

                case TokenType.PLUS:
                    if (left is Double && right is Double)
                    {
                        return (double)left + (double)right;
                    }

                    if (left is String && right is String)
                    {
                        return (String)left + (String)right;
                    }
                    throw new RuntimeError(expr.Op, "Operands must be two numbers or two strings.");
            }

            // Unreachable.
            return null;
        }

        public object VisitCallExpr(Call expr)
        {
            Object callee = Evaluate(expr.Callee);

            List<Object> arguments = new List<Object>();
            foreach(Expr argument in  expr.Arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is ICallable)) {
                // TODO: Change error message to not mention classes explicitly
                // since this shows up before classes are implemented.
                throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
            }

            ICallable function = (ICallable)callee;
            if (arguments.Count < function.RequiredArguments())
            {
                throw new RuntimeError(expr.Paren, "Not enough arguments.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGetExpr(Get expr)
        {
            Object obj = Evaluate(expr.Obj);
            if (obj is LoxInstance)
            {
                return ((LoxInstance)obj).GetProperty(expr.Name);
            }

            throw new RuntimeError(expr.Name, "Only instances have properties.");
        }

        public object VisitGroupingExpr(Grouping expr)
        {
            return Evaluate(expr.Expr);
        }

        public object VisitLiteralExpr(Literal expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(Logical expr)
        {
            object left = Evaluate(expr.Left);
         
            if(expr.Op.Type == TokenType.OR)
            {
                if(IsTruthy(left))
                    return left;
            }
            else
            {
                if(!IsTruthy(left))
                    return left;
            }

            return Evaluate(expr.Right);
        }

        public object VisitSetExpr(Set expr)
        {
            Object value = Evaluate(expr.Value);
            Object obj = Evaluate(expr.Obj);

            if (obj is LoxInstance) {
                ((LoxInstance)obj).Fields.Add(expr.Name.Lexeme, value);
                return value;
            }

            throw new RuntimeError(expr.Name, "Only instances have fields.");
        }

        public object VisitSuperExpr(Super expr)
        {
            int distance = Locals[expr];
            LoxClass superclass = (LoxClass)Environment.GetAt(distance, "super");

            // "this" is always one level nearer than "super"'s environment.
            LoxInstance receiver = (LoxInstance)Environment.GetAt(distance - 1, "this");

            LoxFunction method = superclass.FindMethod(receiver, expr.Method.Lexeme);
            if (method == null)
            {
                throw new RuntimeError(expr.Method, $"Undefined property '{expr.Method.Lexeme}'.");
            }

            return method;
        }

        public object VisitThisExpr(This expr)
        {
            return LookUpVariable(expr.Keyword, expr);
        }

        public object VisitUnaryExpr(Unary expr)
        {
            Object right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.BINARY_NOT:
                    return ~Convert.ToInt32(right);

                case TokenType.BANG:
                    return !IsTrue(right);

                case TokenType.MINUS:
                    CheckNumberOperand(expr.Op, right);
                    return -(double)right;

                case TokenType.PLUS_PLUS:
                    {
                        CheckNumberOperand(expr.Op, right);
                        double value = (double)right + 1;
                        if (expr.Right is Variable variable)
                            Environment.Assign(variable.Name, value);
                        return value;
                    }

                case TokenType.MINUS_MINUS:
                    {
                        CheckNumberOperand(expr.Op, right);
                        double value = (double)right - 1;
                        if (expr.Right is Variable variable)
                            Environment.Assign(variable.Name, value);
                        return value;
                    }
            }

            // Unreachable.
            return null;
        }

        public object VisitVariableExpr(Variable expr)
        {
            return Environment.Get(expr.Name);
        }

        public object VisitBlockStmt(Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(Environment));
            return null;
        }

        public object VisitBreakStmt(Break stmt)
        {
            throw new BreakException();
        }

        public object VisitClassStmt(Class stmt)
        {
            Environment.Define(stmt.Name.Lexeme, null);

            Dictionary<String, LoxFunction> methods = new Dictionary<String, LoxFunction>();

            Object superclass = null;
            if (stmt.Superclass != null)
            {
                superclass = Evaluate(stmt.Superclass);
                if (!(superclass is LoxClass))
                {
                    throw new RuntimeError(stmt.Name, "Superclass must be a class.");
                }

                Environment = new Environment(Environment);
                Environment.Define("super", superclass);
            }
            foreach (Function method in stmt.Methods)
            {
                LoxFunction function = new LoxFunction(method, Environment, method.Name.Lexeme.Equals("init"));
                methods.Add(method.Name.Lexeme, function);
            }

            LoxClass klass = new LoxClass(stmt.Name.Lexeme, (LoxClass)superclass, methods);

            if (superclass != null)
            {
                Environment = Environment.Enclosing;
            }

            Environment.Assign(stmt.Name, klass);
            return null;
        }

        public object VisitConditionalExpr(Conditional expr)
        {
            Object check = Evaluate(expr.Expr);
            Object trueExpr = Evaluate(expr.Trueexpr);
            Object falseExpr = Evaluate(expr.Falseexpr);

            if (IsTruthy(check))
                return trueExpr;
            else
                return falseExpr;

        }

        public object VisitExpressionStmt(Expression stmt)
        {
            Evaluate(stmt.Expr);
            return null;
        }

        public object VisitFunctionStmt(Function stmt)
        {
            Environment.Define(stmt.Name.Lexeme, null);
            LoxFunction function = new LoxFunction(stmt, Environment, false);
            Environment.Assign(stmt.Name, function);
            return null;
        }

        public object VisitIfStmt(If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Thenbranch);
            }
            else if (stmt.Elsebranch != null)
            {
                Execute(stmt.Elsebranch);
            }
            return null;
        }

        public object VisitInterpolationExpr(Interpolation expr)
        {
            String result = "";

            foreach (Expr item in expr.Exprs)
            {
                result = result + Stringify(Evaluate(item));
            }
            return result;
        }

        public object VisitPrintStmt(Print stmt)
        {
            Object value = Evaluate(stmt.Expr);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Return stmt)
        {
            Object Value = null;
            if (stmt.Value != null)
                Value = Evaluate(stmt.Value);

            throw new ReturnEX(Value);
        }

        public object VisitVarStmt(Var stmt)
        {
            Object value = null;
            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            Environment.Define(stmt.Name.Lexeme, value);
            return null; throw new NotImplementedException();
        }

        public object VisitWhileStmt(While stmt)
        {
            try
            {
                while (IsTruthy(Evaluate(stmt.Condition)))
                {
                    Execute(stmt.Body);
                }
            }
            catch(BreakException ex)
            {
                // Do nothing.
            }
            return null;
        }

        private Boolean IsTruthy(Object obj)
        {
            if(obj == null)
                return false;

            if(obj is Boolean)
                return (Boolean)obj;

            return true;
        }

        private Boolean IsTrue(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is Boolean)
                return (Boolean)obj;

            return true;
        }

        private Boolean IsEqual(Object a, Object b)
        {
            // nil is only equal to nil.
            if (a == null && b == null)
                return true;

            if (a == null)
                return false;

            return a.Equals(b);
        }

        private Object LookUpVariable(Token name, Expr expr)
        {
            if (Locals.TryGetValue(expr, out Int32 distance))
            {
                return Environment.GetAt(distance, name.Lexeme);
            }
            else
            {
                return Globals.Get(name);
            }
        }

        private void CheckNumberOperand(Token op, Object operand)
        {
            if (operand is Double)
                return;

            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token op, Object left, Object right)
        {
            if (left is Double && right is Double)
                return;

            throw new RuntimeError(op, "Operands must be numbers.");
        }

        private String Stringify(Object obj)
        {
            if (obj == null)
                return "null";

            if (obj is Double)
            {
                String text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return obj.ToString();
        }

        public object VisitStringFormatExpr(StringFormat expr)
        {
            Object obj = Evaluate(expr.Value);
            Int32 pad = Convert.ToInt32(Evaluate(expr.Alignment));

            // http://blog.stevex.net/string-formatting-in-csharp/
            String x = Stringify(expr.Format);
            switch (obj)
            {
                case Double val:
                    switch (x)
                    {
                        case "c":
                            break;
                        case "d":
                            break;
                        case "e":
                            break;
                        case "f":
                            break;
                        case "g":
                            break;
                        case "n":
                            break;
                        case "r":
                            break;
                        case "x":
                            break;
                    }
                    break;
            }

            obj = Stringify(obj);
            if (pad <= 0)
                obj = ((String)obj).PadRight(Math.Abs(pad));
            else
                obj = ((String)obj).PadLeft(pad);

            return obj;
        }

    }
}
