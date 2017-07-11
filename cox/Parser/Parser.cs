using System;
using System.Collections.Generic;
using System.Text;

using cox.AbstractSyntaxTree;
using cox.Tokens;

namespace cox
{

    //  expression  → assignment 
    //  assignment  → identifier "=" assignment 
    //              | comma
    //  comma       → conditional( "," conditional )*
    //  conditional → ( "?" expression ":" conditional )?
    //              | logic_or
    //  logic_or    → logic_and( "||" logic_and )*
    //  logic_and   → equality( "&&" equality )*
    //  equality    → comparison(( "!=" | "==" ) comparison )*
    //  comparison  → bitwise(( ">" | ">=" | "<" | "<=" ) bitwise )*
    //  bitwise     → term (( "&" | "|" | "~")) term )*
    //  term        → factor(( "-" | "+" ) factor )*
    //  factor      → unary(( "/" | "*" ) unary )*
    //  unary       → ( "~" | "!" | "-" | "--" | "++" ) unary
    //              | postfix
    //  postfix     → primary( "--" | ++" )*
    //  primary     → NUMBER | STRING | "true" | "false" | "nil"
    //              | "(" expression ")" 
    //              | "$"


    //  statement   → expression
    //              | ifStmt
    //              | printStmt
    //              | whileStmt
    //              | block 
    //  whileStmt   → "while" "(" expression ")" statement 
    //  forStmt     → "for" "(" 
    //                      ( varDecl | exprStmt | ";" )
    //                      expression? ";"
    //                      expression? ")" statement 
    //  printStmt   → "print" "(" expression) ")"


    public class Parser
    {
        private class ParseError : Exception { };
        private List<Token> Tokens;
        private int Current = 0;
        private int loopDepth = 0;

        public Parser(List<Token> tokens)
        {
            Tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if(Match(TokenType.CLASS))
                    return ClassDeclaration();

                if(Match(TokenType.FUNC))
                    return Function("function");

                if(Match(TokenType.VAR))
                    return VarDeclaration();

                return Statement();
                
            }
            catch (ParseError ex)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt ClassDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect class name.");

            Expr superclass = null;
            if(Match(TokenType.LESS))
            {
                Consume(TokenType.IDENTIFIER, "Expect superclass name.");
                superclass = new Variable(Previous());
            }

            List<Function> methods = new List<Function>();
            Consume(TokenType.LBRACE, "Expect '{' before class body.");

            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                methods.Add(Function("method"));
            }

            Consume(TokenType.RBRACE, "Expect '}' after class body.");

            return new Class(name, superclass, methods);
        }

        private Stmt Statement()
        {
            if (Match(TokenType.BREAK))
                return BreakStatement();

            if (Match(TokenType.FOR))
                return ForStatement();

            if(Match(TokenType.IF))
                return IfStatement();

            if(Match(TokenType.PRINT))
                return PrintStatement();

            if(Match(TokenType.RETURN))
                return ReturnStatement();

            if(Match(TokenType.WHILE))
                return WhileStatement();

            if (Check(TokenType.LBRACE))
                return new Block(Block());

            return ExpressionStatement();
        }


        private Stmt ForStatement()
        {
            // Parse it.
            Consume(TokenType.LPAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Stmt increment = null;
            if (!Check(TokenType.RPAREN))
            {
                increment = new Expression(Expression());
            }
            Consume(TokenType.RPAREN, "Expect ')' after for clauses.");

            try
            {
                loopDepth++;

                Stmt body = Statement();

                // Desugar to a while loop.
                if (increment != null)
                {
                    body = new Block(new List<Stmt>() { body, increment });
                }

                if (condition == null)
                    condition = new Literal(true);

                body = new While(condition, body);

                if (initializer != null)
                {
                    body = new Block(new List<Stmt>() { initializer, body });
                }

                return body;
            }
            finally
            {
                loopDepth--;
            }
        }

        private Stmt BreakStatement()
        {
            if (loopDepth == 0)
            {
                Error(Previous(), "Must be inside a loop to use 'break'.");
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after 'break'.");
            return new Break();
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LPAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RPAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;

            if(Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Print(value);
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if(!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
            return new Return(keyword, value);
        }

        private Stmt WhileStatement()
        {
            Consume(TokenType.LPAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RPAREN, "Expect ')' after condition.");

            try
            {
                loopDepth++;
                Stmt body = Statement();

                return new While(condition, body);
            }
            finally
            {
                loopDepth--;
            }
        }

        private List<Stmt> Block()
        {
            Consume(TokenType.LBRACE, "Expect '{' before block.");
            List<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RBRACE, "Expect '}' after block.");

            return statements;
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if(Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Var(name, initializer);
        }

        private Function Function(String kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");
            Consume(TokenType.LPAREN, "Expect '(' after " + kind + " name.");
            List<Token> parameters = new List<Token>();
            if(!Check(TokenType.RPAREN))
            {
                do
                {
                    if(parameters.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 parameters.");
                    }

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RPAREN, "Expect ')' after parameters.");

            List<Stmt> body = Block();
            return new Function(name, parameters, body);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Expression(expr);
        }

        private Expr Expression()
        {
            return Comma();
        }

        private Expr InterpolationExpression()
        {
            return Assignment();
        }

        private Expr Comma()
        {
            Expr expr = Assignment();

            while (Match(TokenType.COMMA))
            {
                Token op = Previous();
                Expr right = Assignment();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Assignment()
        {
            Expr expr = Conditional();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Variable)
                {
                    Token name = ((Variable)expr).Name;
                    return new Assign(name, value);
                }
                else
                {
                    if (expr is Get)
                    {
                        Get get = (Get)expr;
                        return new Set(get.Obj, get.Name, value);
                    }
                }

                Error(equals, "Invalid assignment target.");
            }

            if (Match(TokenType.PLUS_EQUALS, TokenType.MINUS_EQUALS, TokenType.MUL_EQUALS, TokenType.DIV_EQUALS))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Variable)
                {
                    Token name = ((Variable)expr).Name;
                    return new Assign(name, new Binary( expr, equals, value));
                }
                else
                {
                    if (expr is Get)
                    {
                        Get get = (Get)expr;
                        return new Set(get.Obj, get.Name, value);
                    }
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Conditional()
        {
            Expr expr = Or();

            if( Match(TokenType.QUESTIONMARK))
            {
                Expr thenBranch = Expression();
                Consume(TokenType.COLON, "Expect ':' after then branch of conditional expression.");
                Expr elseBranch = Conditional();
                expr = new Conditional(expr, thenBranch, elseBranch);
            }
            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(TokenType.OR))
            {
                Token op = Previous();
                Expr right = And();
                expr = new Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(TokenType.AND))
            {
                Token op = Previous();
                Expr right = Equality();
                expr = new Logical(expr, op, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Bitwise();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Bitwise();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Bitwise()
        {
            Expr expr = Term();
            while (Match(TokenType.BINARY_AND, TokenType.BINARY_OR, TokenType.BINARY_XOR, TokenType.BINARY_LEFTSHIFT, TokenType.BINARY_RIGHTSHIFT))
            {
                Token op = Previous();
                Expr right = Term();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token op = Previous();
                Expr right = Factor();

                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.DIV, TokenType.MUL))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BINARY_NOT, TokenType.BANG, TokenType.MINUS, TokenType.PLUS_PLUS, TokenType.MINUS_MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Unary(op, right);
            }
            if (Match(TokenType.PLUS))
            {
                throw Error(Previous(), "Unary '+' not supported.");
            }

            return Postfix();
        }

        private Expr Postfix()
        {
            Expr expr = Call();

            if (Match(TokenType.PLUS_PLUS, TokenType.MINUS_MINUS))
            {
                Token op = Previous();
                expr = new Binary(expr, op, null);

            }
            return expr;
        }


        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if(Match(TokenType.LPAREN))
                {
                    expr = FinishCall(expr);
                    //> Classes not-yet
                }
                else if(Match(TokenType.DOT))
                {
                    Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                    expr = new Get(expr, name);
                    //< Classes not-yet
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if(!Check(TokenType.RPAREN))
            {
                do
                {
                    if(arguments.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 arguments.");
                    }

                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RPAREN, "Expect ')' after arguments.");

            return new Call(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE))
                return new Literal(false);

            if(Match(TokenType.TRUE))
                return new Literal(true);

            if(Match(TokenType.NULL))
                return new Literal(null);

            if(Match(TokenType.NUMBER, TokenType.STRING))
                return new Literal(Previous().Literal);

            if(Match(TokenType.SUPER))
            {
                Token keyword = Previous();
                Consume(TokenType.DOT, "Expect '.' after 'super'.");
                Token method = Consume(TokenType.IDENTIFIER, "Expect superclass method name.");
                return new Super(keyword, method);
            }

            if(Match(TokenType.THIS))
                return new This(Previous());

            if(Match(TokenType.IDENTIFIER))
                return new Variable(Previous());

            if (Match(TokenType.DOLLAR))
            {
                Consume(TokenType.INTERPOLATION_START, "INTERPOLATION_START");
                List<Expr> exprs = new List<Expr>();
                while(Peek().Type != TokenType.INTERPOLATION_END)
                {
                    if(Match(TokenType.LBRACE))
                    {
                        Expr value = InterpolationExpression();
                        Expr alignment = null ;
                        Expr format = null ;

                        if (Peek().Type == TokenType.COMMA)
                        {
                            Advance();
                            alignment = Expression();
                        }
                        if(Peek().Type == TokenType.COLON)
                        {
                            Advance();
                            format = Expression();
                        }
                        exprs.Add(new StringFormat(value, alignment, format));
                        Consume(TokenType.RBRACE, "'}' Expected.");
                    }
                    else
                        exprs.Add(Expression());

                }
                Expr expr = new Interpolation(exprs);

                Consume(TokenType.INTERPOLATION_END, "INTERPOLATION_END");
                return expr;
            }

            if (Match(TokenType.LPAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RPAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private Token Consume(TokenType type, String message)
        {
            if(Check(type))
                return Advance();

            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, String message)
        {
            Cox.Error(token, message);
            return new ParseError();
        }

        private Boolean Match(params object[] types)
        {
            foreach (TokenType type in types)
            {
                if(Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Boolean MatchNext(params object[] types)
        {
            foreach (TokenType type in types)
            {
                if (CheckNext(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Boolean Check(TokenType tokenType)
        {
            if(IsAtEnd())
                return false;

            return Peek().Type == tokenType;
        }

        private Boolean CheckNext(TokenType tokenType)
        {
            if (IsAtEnd())
                return false;
            return PeekNext().Type == tokenType;
        }

        private Token Advance()
        {
            if(!IsAtEnd())
                Current++;
            return Previous();
        }

        private Boolean IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return Tokens[Current];
        }

        private Token PeekNext()
        {
            return Tokens[Current + 1];
        }

        private Token Previous()
        {
            return Tokens[Current - 1];
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if(Previous().Type == TokenType.SEMICOLON) return;

                if(Keywords.GetKeyWordStatementTokenType(Peek().Lexeme) != null)
                { 
                  return;
                }
                Advance();
            }
        }
    }
}
