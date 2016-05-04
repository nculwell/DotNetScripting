using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptLanguageParser
{
    class Parser
    {
        Lexer _lexer;

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
        }

        public StatementBlock ParseScript()
        {
            List<Statement> statements = new List<Statement>();
            while (true)
            {
                if (_lexer.FollowingToken.Type == TokenType.EOF)
                    return new StatementBlock(statements);
                statements.Add(ParseStatement());
            }
        }

        private Statement ParseStatement()
        {
            switch (_lexer.FollowingToken.Type)
            {
                case TokenType.KWD_CONST: return ParseConst();
                case TokenType.KWD_VAR: return ParseVar();
                case TokenType.KWD_SET: return ParseSet();
                case TokenType.KWD_FUNC: return ParseFunc();
                default:
                    throw new SyntaxException(_lexer, "Not expected in statement position.");
            }
        }

        private Statement ParseFunc()
        {
            Expect(TokenType.KWD_FUNC, "(predicted)");
            //_lexer.Advance();
            //if (_lexer.CurrentToken.Type != TokenType.IDENTIFIER)
            //    throw new SyntaxException(_lexer, "Expected identifier to follow 'func'.");
            //var funcName = _lexer.CurrentToken.Text;
            var funcName = Expect(TokenType.IDENTIFIER, "func").Text;
            Expect(TokenType.LPAREN, "func name");
            //_lexer.Advance();
            //if (_lexer.CurrentToken.Type != TokenType.LPAREN)
            //    throw new SyntaxException(_lexer, "Expected '(' to follow func name.");
            List<string> funcArgs = new List<string>();
            _lexer.Advance();
            if (_lexer.CurrentToken.Type == TokenType.RPAREN)
                return new FuncStatement(funcName, funcArgs, ParseFuncBody());
            //_lexer.Advance();
            //if (_lexer.CurrentToken.Type != TokenType.IDENTIFIER)
            //    throw new SyntaxException(_lexer, "Expected func argument name to follow '('.");
            Expect(TokenType.IDENTIFIER, "(");
            while (true)
            {
                _lexer.Advance();
                if (_lexer.CurrentToken.Type == TokenType.RPAREN)
                    return new FuncStatement(funcName, funcArgs, ParseFuncBody());
                if (_lexer.CurrentToken.Type != TokenType.COMMA)
                    throw new SyntaxException(_lexer, "Expected ',' to follow func argument name.");
                _lexer.Advance();
                if (_lexer.CurrentToken.Type != TokenType.IDENTIFIER)
                    throw new SyntaxException(_lexer, "Expected func argument name to follow ','.");
            }
        }

        private StatementBlock ParseFuncBody()
        {
            List<Statement> statements = new List<Statement>();
            Expect(TokenType.LBRACE, "func arguments");
            while (true)
            {
                if (_lexer.FollowingToken.Type == TokenType.RBRACE)
                {
                    _lexer.Advance(); // consume the closing brace
                    break;
                }
                statements.Add(ParseStatement());
            }
            return new StatementBlock(statements);
        }

        private Token Expect(TokenType type, string precedingContext)
        {
            _lexer.Advance();
            if (_lexer.CurrentToken.Type != type)
                throw new SyntaxException(_lexer, "Expected identifier to follow '" + precedingContext + "'.");
            return _lexer.CurrentToken;
        }

        private ConstStatement ParseConst()
        {
            //_lexer.Advance();
            //if (_lexer.CurrentToken.Type != TokenType.IDENTIFIER)
            //    throw new SyntaxException(_lexer, "Expected identifier to follow 'const'.");
            //var constName = _lexer.CurrentToken.Text;
            Expect(TokenType.KWD_CONST, "(begin statement)");
            var constName = Expect(TokenType.IDENTIFIER, "const").Text;
            var constExpr = ParseAssignmentTail();
            return new ConstStatement(constName, constExpr);
        }

        private VarStatement ParseVar()
        {
            //_lexer.Advance();
            //if (_lexer.CurrentToken.Type != TokenType.IDENTIFIER)
            //    throw new SyntaxException(_lexer, "Expected identifier to follow 'var'.");
            //var varName = _lexer.CurrentToken.Text;
            Expect(TokenType.KWD_VAR, "(begin statement)");
            var varName = Expect(TokenType.IDENTIFIER, "var").Text;
            var initExpr = ParseAssignmentTail();
            return new VarStatement(varName, initExpr);
        }

        private SetStatement ParseSet()
        {
            _lexer.Advance();
            if (_lexer.CurrentToken.Type != TokenType.IDENTIFIER)
                throw new SyntaxException(_lexer, "Expected identifier to follow 'set'.");
            var varName = _lexer.CurrentToken.Text;
            var identifierRef = ParseIdentifierProps(varName);
            var initExpr = ParseAssignmentTail();
            return new SetStatement(identifierRef, initExpr);
        }

        private Expr ParseAssignmentTail()
        {
            Expect(TokenType.EQU, "(assignment head)");
            //_lexer.Advance();
            //if (_lexer.CurrentToken.Type != TokenType.EQU)
            //    throw new SyntaxException(_lexer, "Expected '='.");
            //_lexer.Advance();
            return ParseExpr();
        }

        private Expr ParseExpr()
        {
            return ParseBooleanExpr();
        }

        private Expr ParseBooleanExpr()
        {
            var lhs = ParseCompareExpr();
            switch (_lexer.FollowingToken.Type)
            {
                case TokenType.KWD_AND:
                    _lexer.Advance();
                    return new BinOpExpr(BinOp.AND, lhs, ParseBooleanExpr());
                case TokenType.KWD_OR:
                    _lexer.Advance();
                    return new BinOpExpr(BinOp.OR, lhs, ParseBooleanExpr());
                default:
                    return lhs;
            }
        }

        private Expr ParseCompareExpr()
        {
            var lhs = ParseMultiplicativeExpr();
            switch (_lexer.FollowingToken.Type)
            {
                case TokenType.EQU:
                    _lexer.Advance();
                    return new BinOpExpr(BinOp.EQU, lhs, ParseCompareExpr());
                default:
                    return lhs;
            }
        }

        private Expr ParseMultiplicativeExpr()
        {
            var lhs = ParseAdditiveExpr();
            switch (_lexer.FollowingToken.Type)
            {
                case TokenType.MUL:
                    _lexer.Advance();
                    return new BinOpExpr(BinOp.MUL, lhs, ParseMultiplicativeExpr());
                case TokenType.DIV:
                    _lexer.Advance();
                    return new BinOpExpr(BinOp.DIV, lhs, ParseMultiplicativeExpr());
                default:
                    return lhs;
            }
        }

        private Expr ParseAdditiveExpr()
        {
            var lhs = ParseUnaryExpr();
            switch (_lexer.FollowingToken.Type)
            {
                case TokenType.ADD:
                    _lexer.Advance();
                    return new BinOpExpr(BinOp.ADD, lhs, ParseAdditiveExpr());
                case TokenType.SUB:
                    _lexer.Advance();
                    return new BinOpExpr(BinOp.SUB, lhs, ParseAdditiveExpr());
                default:
                    return lhs;
            }
        }

        private Expr ParseUnaryExpr()
        {
            var lhs = ParseAtomicExpr();
            switch (_lexer.FollowingToken.Type)
            {
                case TokenType.ADD:
                    _lexer.Advance();
                    return new BinOpExpr(BinOp.ADD, lhs, ParseAdditiveExpr());
                default:
                    return lhs;
            }
        }

        private Expr ParseAtomicExpr()
        {
            switch (_lexer.FollowingToken.Type)
            {
                case TokenType.LPAREN:
                    {
                        _lexer.Advance();
                        var parenExpr = ParseExpr();
                        _lexer.Advance();
                        if (_lexer.CurrentToken.Type != TokenType.RPAREN)
                            throw new SyntaxException(_lexer, "Expected ')'.");
                        return parenExpr;
                    }
                case TokenType.IDENTIFIER:
                    {
                        _lexer.Advance();
                        var identifierName = _lexer.CurrentToken.Text;
                        if (_lexer.FollowingToken.Type != TokenType.DOT)
                            return ParseIdentifierProps(identifierName);
                        else if (_lexer.FollowingToken.Type != TokenType.LPAREN)
                            return ParseFuncCall(identifierName);
                        else
                            return new IdentifierExpr(identifierName, null);
                    }
                case TokenType.NUMBER:
                    {
                        _lexer.Advance();
                        var n = new NumberValue(_lexer.CurrentToken.AsNumber());
                        return new LiteralExpr(n);
                    }
                case TokenType.STRING:
                    {
                        _lexer.Advance();
                        var s = new StringValue(_lexer.CurrentToken.Text);
                        return new LiteralExpr(s);
                    }
                default:
                    throw new SyntaxException(_lexer, "Expected an expression.");
            }
        }

        private FuncCallExpr ParseFuncCall(string functionName)
        {
            List<Expr> funcArgs = new List<Expr>();
            _lexer.Advance();
            if (_lexer.CurrentToken.Type != TokenType.LPAREN)
                throw new SyntaxException(_lexer, "Expected '(' in function call.");
            if (_lexer.FollowingToken.Type != TokenType.RPAREN)
            {
                funcArgs.Add(ParseExpr());
                while (true)
                {
                    if (_lexer.FollowingToken.Type == TokenType.RPAREN)
                        break;
                    _lexer.Advance();
                    if (_lexer.CurrentToken.Type != TokenType.COMMA)
                        throw new SyntaxException(_lexer, "Expected ',' after function argument.");
                    funcArgs.Add(ParseExpr());
                }
            }
            _lexer.Advance(); // consume RPAREN
            return new FuncCallExpr(functionName, funcArgs);
        }

        private IdentifierExpr ParseIdentifierProps(string identifierName)
        {
            List<string> propertyNames = new List<string>();
            while (true)
            {
                if (_lexer.FollowingToken.Type != TokenType.DOT)
                    break;
                _lexer.Advance();
                _lexer.Advance();
                if (_lexer.CurrentToken.Type != TokenType.IDENTIFIER)
                    throw new SyntaxException(_lexer, "Expected identifier after '.'.");
                propertyNames.Add(_lexer.CurrentToken.Text);
            }
            return new IdentifierExpr(identifierName, propertyNames);
        }
    }
}
