﻿using System;
using System.Collections.Generic;
using System.Text;

using cox.AbstractSyntaxTree;

namespace cox
{
    public class ASTPrinter : IVisitorExpr<String>, IVisitorStmt<String>
    {
        public String Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitAssignExpr(Assign expr)
        {
            throw new NotImplementedException();
        }

        public String VisitBinaryExpr(Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string VisitBlockStmt(Block stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitBreakStmt(Break stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitCallExpr(Call expr)
        {
            throw new NotImplementedException();
        }

        public string VisitClassStmt(Class stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitConditionalExpr(Conditional expr)
        {
            throw new NotImplementedException();
        }

        public string VisitExpressionStmt(Expression stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitFunctionStmt(Function stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitGetExpr(Get expr)
        {
            throw new NotImplementedException();
        }

        public String VisitGroupingExpr(Grouping expr)
        {
            return Parenthesize("group", expr.Expr);
        }

        public string VisitIfStmt(If stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitInterpolationExpr(Interpolation expr)
        {
            throw new NotImplementedException();
        }

        public String VisitLiteralExpr(Literal expr)
        {
            return expr.Value.ToString();
        }

        public string VisitLogicalExpr(Logical expr)
        {
            throw new NotImplementedException();
        }

        public string VisitPrintStmt(Print stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitReturnStmt(Return stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitSetExpr(Set expr)
        {
            throw new NotImplementedException();
        }

        public string VisitStringFormatExpr(StringFormat expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSuperExpr(Super expr)
        {
            throw new NotImplementedException();
        }

        public string VisitThisExpr(This expr)
        {
            throw new NotImplementedException();
        }

        public String VisitUnaryExpr(Unary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Right);
        }

        public string VisitVariableExpr(Variable expr)
        {
            throw new NotImplementedException();
        }

        public string VisitVarStmt(Var stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitWhileStmt(While stmt)
        {
            throw new NotImplementedException();
        }

        private String Parenthesize(String name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in  exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}

