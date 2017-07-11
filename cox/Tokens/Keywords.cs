using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lox.Tokens
{
    public class Keywords
    {
        enum Category
        {
            NonStatement,
            Statement,
        };

        class Word
        {
            public Category Category;
            public String Value;

            public Word(Category category, String value)
            {
                Category = category;
                Value = value;
            }
        }

        private static Dictionary<Word, TokenType> Words = null;

        public static TokenType? GetKeyWordTokenType(String word)
        {
            if(Words == null)
                BuildWords();

            var item = Words.Where(c => c.Key.Value == word);
            if(item.Count() == 0)
                return null;
            else
                return item.FirstOrDefault().Value;

        }

        public static TokenType? GetKeyWordStatementTokenType(String word)
        {
            if(Words == null)
                BuildWords();

            var item = Words.Where(c => c.Key.Category == Category.Statement && c.Key.Value == word);
            if(item.Count() == 0)
                return null;
            else
                return item.FirstOrDefault().Value;
        }

        private static void BuildWords()
        {
            Words = new Dictionary<Word, TokenType>()
            {
                { new Word(Category.NonStatement, "else"), TokenType.ELSE },
                { new Word(Category.NonStatement, "false"), TokenType.FALSE },
                { new Word(Category.NonStatement, "true"), TokenType.TRUE },
                { new Word(Category.NonStatement, "null"), TokenType.NULL },
                { new Word(Category.Statement, "class"), TokenType.CLASS },
                { new Word(Category.Statement, "for"), TokenType.FOR },
                { new Word(Category.Statement, "func"), TokenType.FUNC },
                { new Word(Category.Statement, "if"), TokenType.IF },
                { new Word(Category.Statement, "print"), TokenType.PRINT },
                { new Word(Category.Statement, "return"), TokenType.RETURN },
                { new Word(Category.Statement, "super"), TokenType.SUPER },
                { new Word(Category.Statement, "this"), TokenType.THIS },
                { new Word(Category.Statement, "var"), TokenType.VAR },
                { new Word(Category.Statement, "while"), TokenType.WHILE },
                { new Word(Category.Statement, "break"), TokenType.BREAK },
            };
        }
    }
}
