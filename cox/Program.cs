using System;
using System.Collections.Generic;
using System.IO;

using lox.AbstractSyntaxTree;
using lox.Tokens;

namespace lox
{
    public class Lox
    {
        private static Interpreter Interpreter = new Interpreter();
        private static Boolean HadError = false;
        private static Boolean HadRuntimeError = false;


        static Int32 Main(string[] args)
        {
            //Int32 a = 0;
            //var x = $"Hello {{{a=a+1;a++}";
            
            if(args.Length > 1)
            {
                Console.WriteLine("Usag: lox [script]");
            }
            else
            {
                if(args.Length == 1)
                {
                    RunFile(args[0]);
                }
                else
                {
                    RunPrompt();
                }
            }


            Console.WriteLine("Press Any Key");
            Console.ReadKey();

            if (HadError)
                return 65;

            if(HadRuntimeError)
                return 70;

            return 0;
        }

        private static void RunFile( String path)
        {
            String input = File.ReadAllText(path);
            Run(input);
        }

        private static void RunPrompt()
        {
            while (true)
            {
                HadError = false;
                Console.Write(">");
                String input = Console.ReadLine();
                if(input == "")
                    break;
                else
                {
                    Run(input) ;
                }
            }
        }

        private static void Run(String source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.Scan();

            Parser Parser = new Parser(tokens);
            List<Stmt> statements = Parser.Parse();

            if( HadError)
                return;

            Resolver resolver = new Resolver();
            Dictionary<Expr, Int32> locals = resolver.Resolve(statements);

            if(HadError)
                return;
            // Are we a program or an expresion
            

            Interpreter.Interpret(statements, locals);
        }

        public static void Error( Int32 line, String message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, String message)
        {
            if(token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        static private void Report( Int32 line, String where, String message)
        {
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            HadError = true;
        }

        static public void RuntimeError(RuntimeError error)
        {
            Console.WriteLine( $"{error.Message}\n [line {error.Token.Line}]");
            HadRuntimeError = true;
        }
    }
}