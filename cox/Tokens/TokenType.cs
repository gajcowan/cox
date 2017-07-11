using System;
using System.Collections.Generic;
using System.Text;

namespace cox.Tokens
{
    public enum TokenType
    {

        // Single-character tokens
        PLUS,               // +
        MINUS,              // - 
        MUL,                // *
        DIV,                // /
        LPAREN,             // (
        RPAREN,             // )
        LBRACE,             // {
        RBRACE,             // }
        LSQRBRACE,          // [
        RSQRBRACE,          // ]
        DOT,                // .
        COMMA,              // ,
        SEMICOLON,          // ;
        COLON,              // :
        QUESTIONMARK,       // ?
        DOLLAR,             // $

        // One or two character tokens
        BANG,               // !
        BANG_EQUAL,         // !=
        EQUAL,              // =
        EQUAL_EQUAL,        // ==
        GREATER,            // >
        GREATER_EQUAL,      // >=
        LESS,               // <
        LESS_EQUAL,         // <=
        PLUS_PLUS,          // ++
        MINUS_MINUS,        // --
        PLUS_EQUALS,         // +=
        MINUS_EQUALS,       // -=
        MUL_EQUALS,         // *=
        DIV_EQUALS,         // /=
        OR,                 // ||
        AND,                // &&


        // Bitwise functions
        BINARY_AND,         // &
        BINARY_OR,          // |
        BINARY_XOR,         // ^
        BINARY_NOT,         // ~
        BINARY_LEFTSHIFT,   // <<
        BINARY_RIGHTSHIFT,  // >>

        // Literals
        IDENTIFIER,
        STRING,
        NUMBER,

        INTERPOLATION_START,
        INTERPOLATION_END,

            // Keywords
        TRUE,
        FALSE,
        IF,
        ELSE,
        FOR,
        WHILE,
        RETURN,
        CLASS,
        SUPER,
        VAR,
        THIS,
        NULL,
        PRINT,
        FUNC,
        BREAK,
       
        //
        EOF
    }
}
