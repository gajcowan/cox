using System;
using System.Collections.Generic;
using System.IO;

namespace GenerateAST
{
    class Program
    {
        static Int32 Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: generate_ast <output directory>");
                Console.ReadKey();
                return 1;
            }
            String outputDir = args[0];

            DefineAst(outputDir, "Expr", new List<String>
            { 
                "Assign         : Token name, Expr value",
                "Binary         : Expr left, Token op, Expr right",
                "Call           : Expr callee, Token paren, List<Expr> arguments",
                "Conditional    : Expr expr, Expr trueExpr, Expr falseExpr",
                "Get            : Expr obj, Token name",
                "Grouping       : Expr expr",
                "Literal        : Object value",
                "Logical        : Expr left, Token op, Expr right",
                "Set            : Expr obj, Token name, Expr value",
                "Super          : Token keyword, Token method",
                "This           : Token keyword",
                "Unary          : Token op, Expr right",
                "Variable       : Token name",
                "Interpolation  : List<Expr> exprs",
                "StringFormat   : Expr value, Expr alignment, Expr format", 
            });

            DefineAst(outputDir, "Stmt", new List<String>
            {
                "Block      : List<Stmt> statements",
                "Break      : ", 
                "Class      : Token name, Expr superclass, List<Function> methods",
                "Expression : Expr expr",
                "Function   : Token name, List<Token> parameters, List<Stmt> body",
                "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "Print      : Expr expr",
                "Return     : Token keyword, Expr value",
                "Var        : Token name, Expr initializer",
                "While      : Expr condition, Stmt body"
            }); 
            return 0;
        }

        private static void DefineAst(String outputDir, String baseName, List<String> types)
        { 
            String path = outputDir + "\\" + baseName + ".cs";
            var outputFile = System.IO.File.CreateText(path);
            outputFile.WriteLine($"using System;");
            outputFile.WriteLine($"using System.Collections.Generic;");
            outputFile.WriteLine($"");
            outputFile.WriteLine($"using lox.Tokens;");
            outputFile.WriteLine($"");
            outputFile.WriteLine($"namespace lox.AbstractSyntaxTree");
            outputFile.WriteLine($"{{");
            //outputFile.WriteLine($"{{");


            DefineVisitor(outputFile, baseName, types);

            outputFile.WriteLine($"\tpublic abstract class {baseName}");
            outputFile.WriteLine($"\t{{");
            outputFile.WriteLine($"\t\tpublic abstract T Accept<T>(IVisitor{baseName}<T> visitor);");
            outputFile.WriteLine($"\t}}");
            outputFile.WriteLine($"");

            // The AST classes.
            foreach (String type in types)
            {
                String className = type.Split(':')[0].Trim();
                String fields = type.Split(':')[1].Trim();
                DefineType(outputFile, baseName, className, fields);
            }
            outputFile.WriteLine($"}}");

            outputFile.Flush();
            outputFile.Dispose();
        }

        private static void DefineType(StreamWriter outputFile, String baseName, String className, String fieldList)
        {
            outputFile.WriteLine($"\tpublic class {className} : {baseName}");
            outputFile.WriteLine($"\t{{");
            outputFile.WriteLine();

            // Store parameters in fields.
            String[] fields;
            if (fieldList.Length == 0)
            {
                fields = new String[0];
            }
            else
            {
                fields = fieldList.Split(',');
            }

            foreach (String field in fields)
            {
                var f = field.Trim();
                String type = f.Split(' ')[0]; 
                String name = f.Split(' ')[1];
                outputFile.WriteLine($"\t\tpublic {type} {Char.ToUpper(name[0]) + name.Substring(1).ToLower()};");
            }

            outputFile.WriteLine();
            // Constructor.
            outputFile.WriteLine($"\t\tpublic {className} ({fieldList})");
            outputFile.WriteLine($"\t\t{{");

            foreach (String field in fields)
            {
                var f = field.Trim();
                String name = f.Split(' ')[1];
                outputFile.WriteLine($"\t\t\t{Char.ToUpper(name[0]) + name.Substring(1).ToLower()} = {name};");
            }

            outputFile.WriteLine($"\t\t}}");
            outputFile.WriteLine();
            outputFile.WriteLine($"\t\toverride public T Accept<T>(IVisitor{baseName}<T> visitor)");
            outputFile.WriteLine($"\t\t{{");
            outputFile.WriteLine($"\t\t\treturn visitor.Visit{className}{baseName}( this );");
            outputFile.WriteLine($"\t\t}}");


            outputFile.WriteLine($"\t}}");
            outputFile.WriteLine($"");
        }

        static void DefineVisitor(StreamWriter outputFile, String baseName, List<String> types)
        {
            outputFile.WriteLine($"\tpublic interface IVisitor{baseName}<T>");
            outputFile.WriteLine($"\t{{");

            foreach(String type in types)
            {
                String typeName = type.Split(':')[0].Trim();
                outputFile.WriteLine($"\t\tT Visit{typeName}{baseName}( {typeName} {baseName.ToLower()});");
            }
            outputFile.WriteLine($"\t}}");
            outputFile.WriteLine();

        }
    }
}
