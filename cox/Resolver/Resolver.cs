using System;
using System.Collections.Generic;
using System.Text;

using cox.AbstractSyntaxTree;
using cox.Tokens;

namespace cox
{
    public class Resolver : IVisitorExpr<object>, IVisitorStmt<object>
    {
        private enum FunctionType
        {
            NONE,
            FUNCTION,
            METHOD,
            INITIALIZER
        }

        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }

        private Stack<Dictionary<String, Boolean>> scopes = new Stack<Dictionary<String, Boolean>>();

        private Dictionary<Expr, Int32> locals = new Dictionary<Expr, Int32>();

        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        public object VisitAssignExpr(Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitBinaryExpr(Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        public object VisitBlockStmt(Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        public object VisitClassStmt(Class stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;
            //> Inheritance not-yet

            if (stmt.Superclass != null)
            {
                currentClass = ClassType.SUBCLASS;
                Resolve(stmt.Superclass);
                BeginScope();
                scopes.Peek().Add("super", true);
            }
            //< Inheritance not-yet

            foreach (Function method in stmt.Methods)
            {
                // Push the implicit scope that binds "this" and "class".
                BeginScope();
                scopes.Peek().Add("this", true);

                FunctionType declaration = FunctionType.METHOD;
                if (method.Name.Lexeme.Equals("init"))
                {
                    declaration = FunctionType.INITIALIZER;
                }

                ResolveFunction(method, declaration);
                EndScope();
            }
            if (currentClass == ClassType.SUBCLASS)
                EndScope();

            currentClass = enclosingClass;
            return null;
        }

        public object VisitExpressionStmt(Expression stmt)
        {
            Resolve(stmt.Expr);
            return null;
        }

        public object VisitFunctionStmt(Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object VisitGetExpr(Get expr)
        {
            Resolve(expr.Obj);
            return null;
        }

        public object VisitGroupingExpr(Grouping expr)
        {
            Resolve(expr.Expr);
            return null;
        }

        public object VisitCallExpr(Call expr)
        {
            Resolve(expr.Callee);

            foreach (Expr argument in expr.Arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public object VisitIfStmt(If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Thenbranch);

            if (stmt.Elsebranch != null)
                Resolve(stmt.Elsebranch);

            return null;
        }

        public object VisitLiteralExpr(Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitPrintStmt(Print stmt)
        {
            Resolve(stmt.Expr);
            return null;
        }

        public object VisitReturnStmt(Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Cox.Error(stmt.Keyword, "Cannot return from top-level code.");
            }

            if (stmt.Value != null)
            {
                if (currentFunction == FunctionType.INITIALIZER)
                {
                    Cox.Error(stmt.Keyword, "Cannot return a value from an initializer.");
                }

                Resolve(stmt.Value); 
            }

            return null;
        }

        public object VisitSetExpr(Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Obj);
            return null;
        }

        public object VisitSuperExpr(Super expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Cox.Error(expr.Keyword, "Cannot use 'super' outside of a class.");
            }
            else if (currentClass != ClassType.SUBCLASS)
            {
                Cox.Error(expr.Keyword, "Cannot use 'super' in a class with no superclass.");
            }
            else
            {
                ResolveLocal(expr, expr.Keyword);
            }
            return null;
        }

        public object VisitThisExpr(This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Cox.Error(expr.Keyword, "Cannot use 'this' outside of a class.");
            }
            else
            {
                ResolveLocal(expr, expr.Keyword);
            }
            return null;
        }

        public object VisitUnaryExpr(Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object VisitVariableExpr(Variable expr)
        {
            if (scopes.Count > 0)
            {
                if (scopes.Peek().ContainsKey(expr.Name.Lexeme) && scopes.Peek()[expr.Name.Lexeme] == false)
                {
                    Cox.Error(expr.Name, "Cannot read local variable in its own initializer.");
                }
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitVarStmt(Var stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }
            Define(stmt.Name);
            return null;
        }

        public object VisitWhileStmt(While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null;
        }

        public  Dictionary<Expr, Int32>Resolve(List<Stmt> statements)
        {
            foreach(Stmt statement in statements)
            {
                Resolve(statement);
            }

           return locals;
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<String, Boolean>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            // Don't need to track top level variables.
            if (scopes.Count == 0)
                return;

            Dictionary<String, Boolean> scope = scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
            {
                Cox.Error(name, "Variable with this name already Declared in this scope.");
            }

            scope.Add(name.Lexeme, false);
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0)
                return;

            scopes.Peek()[name.Lexeme] = true;
        }

        private void ResolveFunction(Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach (Token param in function.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);
            EndScope();

            currentFunction = enclosingFunction;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            var arr = scopes.ToArray();
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if ( arr[i].ContainsKey(name.Lexeme))
                {

                    locals.Add(expr, arr.Length - 1 - i);
                    return;
                }
            }

            // Not found. Assume it is global.
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            if (expr is null)
                return;
            expr.Accept(this);
        }

        public object VisitConditionalExpr(Conditional expr)
        {
            Resolve(expr.Expr);
            Resolve(expr.Trueexpr);
            Resolve(expr.Falseexpr);
            return null;
        }

        public object VisitBreakStmt(Break stmt)
        {
            return null;             
        }

        public object VisitInterpolationExpr(Interpolation expr)
        {
            foreach (Expr item in expr.Exprs)
            {
                Resolve(item);
            }

            return null;
        }

        public object VisitStringFormatExpr(StringFormat expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Alignment);
            Resolve(expr.Format);

            return null;
        }
    }
}
