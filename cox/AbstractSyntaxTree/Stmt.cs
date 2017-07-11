using System;
using System.Collections.Generic;

using lox.Tokens;

namespace lox.AbstractSyntaxTree
{
	public interface IVisitorStmt<T>
	{
		T VisitBlockStmt( Block stmt);
		T VisitBreakStmt( Break stmt);
		T VisitClassStmt( Class stmt);
		T VisitExpressionStmt( Expression stmt);
		T VisitFunctionStmt( Function stmt);
		T VisitIfStmt( If stmt);
		T VisitPrintStmt( Print stmt);
		T VisitReturnStmt( Return stmt);
		T VisitVarStmt( Var stmt);
		T VisitWhileStmt( While stmt);
	}

	public abstract class Stmt
	{
		public abstract T Accept<T>(IVisitorStmt<T> visitor);
	}

	public class Block : Stmt
	{

		public List<Stmt> Statements;

		public Block (List<Stmt> statements)
		{
			Statements = statements;
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitBlockStmt( this );
		}
	}

	public class Break : Stmt
	{


		public Break ()
		{
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitBreakStmt( this );
		}
	}

	public class Class : Stmt
	{

		public Token Name;
		public Expr Superclass;
		public List<Function> Methods;

		public Class (Token name, Expr superclass, List<Function> methods)
		{
			Name = name;
			Superclass = superclass;
			Methods = methods;
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitClassStmt( this );
		}
	}

	public class Expression : Stmt
	{

		public Expr Expr;

		public Expression (Expr expr)
		{
			Expr = expr;
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitExpressionStmt( this );
		}
	}

	public class Function : Stmt
	{

		public Token Name;
		public List<Token> Parameters;
		public List<Stmt> Body;

		public Function (Token name, List<Token> parameters, List<Stmt> body)
		{
			Name = name;
			Parameters = parameters;
			Body = body;
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitFunctionStmt( this );
		}
	}

	public class If : Stmt
	{

		public Expr Condition;
		public Stmt Thenbranch;
		public Stmt Elsebranch;

		public If (Expr condition, Stmt thenBranch, Stmt elseBranch)
		{
			Condition = condition;
			Thenbranch = thenBranch;
			Elsebranch = elseBranch;
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitIfStmt( this );
		}
	}

	public class Print : Stmt
	{

		public Expr Expr;

		public Print (Expr expr)
		{
			Expr = expr;
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitPrintStmt( this );
		}
	}

	public class Return : Stmt
	{

		public Token Keyword;
		public Expr Value;

		public Return (Token keyword, Expr value)
		{
			Keyword = keyword;
			Value = value;
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitReturnStmt( this );
		}
	}

	public class Var : Stmt
	{

		public Token Name;
		public Expr Initializer;

		public Var (Token name, Expr initializer)
		{
			Name = name;
			Initializer = initializer;
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitVarStmt( this );
		}
	}

	public class While : Stmt
	{

		public Expr Condition;
		public Stmt Body;

		public While (Expr condition, Stmt body)
		{
			Condition = condition;
			Body = body;
		}

		override public T Accept<T>(IVisitorStmt<T> visitor)
		{
			return visitor.VisitWhileStmt( this );
		}
	}

}
