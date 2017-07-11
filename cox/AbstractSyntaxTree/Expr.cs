using System;
using System.Collections.Generic;

using cox.Tokens;

namespace cox.AbstractSyntaxTree
{
	public interface IVisitorExpr<T>
	{
		T VisitAssignExpr( Assign expr);
		T VisitBinaryExpr( Binary expr);
		T VisitCallExpr( Call expr);
		T VisitConditionalExpr( Conditional expr);
		T VisitGetExpr( Get expr);
		T VisitGroupingExpr( Grouping expr);
		T VisitLiteralExpr( Literal expr);
		T VisitLogicalExpr( Logical expr);
		T VisitSetExpr( Set expr);
		T VisitSuperExpr( Super expr);
		T VisitThisExpr( This expr);
		T VisitUnaryExpr( Unary expr);
		T VisitVariableExpr( Variable expr);
		T VisitInterpolationExpr( Interpolation expr);
		T VisitStringFormatExpr( StringFormat expr);
	}

	public abstract class Expr
	{
		public abstract T Accept<T>(IVisitorExpr<T> visitor);
	}

	public class Assign : Expr
	{

		public Token Name;
		public Expr Value;

		public Assign (Token name, Expr value)
		{
			Name = name;
			Value = value;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitAssignExpr( this );
		}
	}

	public class Binary : Expr
	{

		public Expr Left;
		public Token Op;
		public Expr Right;

		public Binary (Expr left, Token op, Expr right)
		{
			Left = left;
			Op = op;
			Right = right;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitBinaryExpr( this );
		}
	}

	public class Call : Expr
	{

		public Expr Callee;
		public Token Paren;
		public List<Expr> Arguments;

		public Call (Expr callee, Token paren, List<Expr> arguments)
		{
			Callee = callee;
			Paren = paren;
			Arguments = arguments;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitCallExpr( this );
		}
	}

	public class Conditional : Expr
	{

		public Expr Expr;
		public Expr Trueexpr;
		public Expr Falseexpr;

		public Conditional (Expr expr, Expr trueExpr, Expr falseExpr)
		{
			Expr = expr;
			Trueexpr = trueExpr;
			Falseexpr = falseExpr;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitConditionalExpr( this );
		}
	}

	public class Get : Expr
	{

		public Expr Obj;
		public Token Name;

		public Get (Expr obj, Token name)
		{
			Obj = obj;
			Name = name;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitGetExpr( this );
		}
	}

	public class Grouping : Expr
	{

		public Expr Expr;

		public Grouping (Expr expr)
		{
			Expr = expr;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitGroupingExpr( this );
		}
	}

	public class Literal : Expr
	{

		public Object Value;

		public Literal (Object value)
		{
			Value = value;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitLiteralExpr( this );
		}
	}

	public class Logical : Expr
	{

		public Expr Left;
		public Token Op;
		public Expr Right;

		public Logical (Expr left, Token op, Expr right)
		{
			Left = left;
			Op = op;
			Right = right;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitLogicalExpr( this );
		}
	}

	public class Set : Expr
	{

		public Expr Obj;
		public Token Name;
		public Expr Value;

		public Set (Expr obj, Token name, Expr value)
		{
			Obj = obj;
			Name = name;
			Value = value;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitSetExpr( this );
		}
	}

	public class Super : Expr
	{

		public Token Keyword;
		public Token Method;

		public Super (Token keyword, Token method)
		{
			Keyword = keyword;
			Method = method;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitSuperExpr( this );
		}
	}

	public class This : Expr
	{

		public Token Keyword;

		public This (Token keyword)
		{
			Keyword = keyword;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitThisExpr( this );
		}
	}

	public class Unary : Expr
	{

		public Token Op;
		public Expr Right;

		public Unary (Token op, Expr right)
		{
			Op = op;
			Right = right;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitUnaryExpr( this );
		}
	}

	public class Variable : Expr
	{

		public Token Name;

		public Variable (Token name)
		{
			Name = name;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitVariableExpr( this );
		}
	}

	public class Interpolation : Expr
	{

		public List<Expr> Exprs;

		public Interpolation (List<Expr> exprs)
		{
			Exprs = exprs;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitInterpolationExpr( this );
		}
	}

	public class StringFormat : Expr
	{

		public Expr Value;
		public Expr Alignment;
		public Expr Format;

		public StringFormat (Expr value, Expr alignment, Expr format)
		{
			Value = value;
			Alignment = alignment;
			Format = format;
		}

		override public T Accept<T>(IVisitorExpr<T> visitor)
		{
			return visitor.VisitStringFormatExpr( this );
		}
	}

}
