using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ScriptLanguageParser
{
    abstract class Statement
    {
        public abstract Value Interpret(Env env);
        public abstract void PrettyPrint(TextWriter printer, int indent);
        protected void Indent(TextWriter printer, int indent)
        {
            for (int i = 0; i < indent; i++)
                printer.Write("  ");
        }
        public override string ToString()
        {
            var sw = new StringWriter();
            PrettyPrint(sw, 0);
            return sw.ToString();
        }
    }

    abstract class Expr
    {
        public abstract Value Interpret(Env env);
        public abstract void PrettyPrint(TextWriter printer, int indent);
        public override string ToString()
        {
            var sw = new StringWriter();
            PrettyPrint(sw, 0);
            return sw.ToString();
        }
    }

    class StatementBlock : Statement
    {
        private readonly IList<Statement> _statements;
        public StatementBlock(IList<Statement> statements)
        {
            _statements = statements;
        }
        public override Value Interpret(Env env)
        {
            foreach(var stmt in _statements)
            {
                var returnValue = stmt.Interpret(env);
                if ((object)returnValue != InternalValue.NoReturn)
                    return returnValue;
            }
            return InternalValue.NoReturn;
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            //Indent(printer, indent);
            //printer.WriteLine("{");
            foreach (var stmt in _statements)
            {
                Indent(printer, indent);
                stmt.PrettyPrint(printer, indent);
                //Indent(printer, indent + 1);
                //stmt.PrettyPrint(printer, indent + 1);
                printer.WriteLine();
            }
            //Indent(printer, indent);
            //printer.Write("}");
        }
    }

    class ConstStatement : Statement
    {
        public string ConstName { get; }
        public Expr ConstExpr { get; }
        public ConstStatement(string name, Expr constExpr)
        {
            ConstName = name;
            ConstExpr = constExpr;
        }
        public override Value Interpret(Env env)
        {
            var constValue = ConstExpr.Interpret(env);
            env.BindValue(ConstName, constValue); // FIXME: Do something to make constants constant.
            return InternalValue.NoReturn;
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            printer.Write(string.Format("const {0} = ", ConstName));
            ConstExpr.PrettyPrint(printer, indent);
        }
    }

    class VarStatement : Statement
    {
        public string VarName { get; }
        public Expr InitExpr { get; }
        public VarStatement(string varName, Expr initExpr)
        {
            VarName = varName;
            InitExpr = initExpr;
        }
        public override Value Interpret(Env env)
        {
            var constValue = InitExpr.Interpret(env);
            env.BindValue(VarName, constValue);
            return InternalValue.NoReturn;
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            printer.Write(string.Format("var {0} = ", VarName));
            InitExpr.PrettyPrint(printer, indent);
        }
    }

    class SetStatement : Statement
    {
        public IdentifierExpr LvalueRef { get; } // optional, null if not present
        public Expr AssignmentExpr { get; }
        public SetStatement(IdentifierExpr lvalueRef, Expr assignmentExpr)
        {
            LvalueRef = lvalueRef;
            AssignmentExpr = assignmentExpr;
        }
        public override Value Interpret(Env env)
        {
            var assignmentValue = AssignmentExpr.Interpret(env);
            if (LvalueRef.PropNames.Count() == 0)
            {
                env.SetValue(LvalueRef.IdentifierName, assignmentValue);
            }
            else
            {
                var wrappedObject = env.Lookup(LvalueRef.IdentifierName);
                var nativeObjectValue = wrappedObject as NativeObjectValue;
                if (nativeObjectValue == null)
                    throw new OperatorTypeException(".", "native object");
                var obj = nativeObjectValue.NativeObject;
                foreach (var intermediatePropName
                         in LvalueRef.PropNames.Take(LvalueRef.PropNames.Count() - 1))
                {
                    var objProperty = obj.GetType().GetProperty(intermediatePropName);
                    if (objProperty == null)
                        throw new PropertyNotFoundException(
                            LvalueRef.IdentifierName, intermediatePropName);
                    obj = objProperty.GetValue(obj);
                }
                var finalPropName = LvalueRef.PropNames[LvalueRef.PropNames.Count() - 1];
                var finalProperty = obj.GetType().GetProperty(finalPropName);
                if (finalProperty == null)
                    throw new PropertyNotFoundException(
                        LvalueRef.IdentifierName, finalPropName);
                try
                {
                    finalProperty.SetValue(obj, assignmentValue);
                }
                catch (ArgumentException)
                {
                    throw new PropertyAssignmentException(LvalueRef, assignmentValue);
                }
            }
            return InternalValue.NoReturn;
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            printer.Write("set ");
            LvalueRef.PrettyPrint(printer, indent);
            printer.Write(" = ");
            AssignmentExpr.PrettyPrint(printer, indent);
        }
    }

    class FuncStatement : Statement
    {
        public string FuncName { get; }
        public List<string> FuncArgs { get; }
        public StatementBlock FuncBody { get; }
        public FuncStatement(string funcName, List<string> funcArgs, StatementBlock funcBody)
        {
            FuncName = funcName;
            FuncArgs = funcArgs;
            FuncBody = funcBody;
        }
        public override Value Interpret(Env env)
        {
            env.BindValue(FuncName, new FuncValue(FuncName, env, FuncArgs, FuncBody));
            return InternalValue.NoReturn;
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            printer.Write("func " + FuncName + "(");
            printer.Write(string.Join(", ", FuncArgs));
            //printer.WriteLine(")");
            printer.WriteLine(") {");
            //FuncBody.PrettyPrint(printer, indent);
            FuncBody.PrettyPrint(printer, indent + 1);
            Indent(printer, indent);
            printer.Write("}");
        }
    }

    class LiteralExpr : Expr
    {
        public Value LiteralValue { get; }
        public LiteralExpr(Value literalValue) { LiteralValue = literalValue; }
        public override Value Interpret(Env env)
        {
            return LiteralValue;
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            printer.Write(LiteralValue.ToString());
        }
    }

    class FuncCallExpr : Expr
    {
        public string FuncName { get; }
        public IList<Expr> FuncArgs { get; }
        public FuncCallExpr(string funcName, List<Expr> funcArgs)
        {
            FuncName = funcName;
            FuncArgs = funcArgs != null
                ? funcArgs.AsReadOnly()
                : new List<Expr>().AsReadOnly();
        }
        public override Value Interpret(Env env)
        {
            var funcNameReferent = env.Lookup(FuncName);
            var funcValue = funcNameReferent as FuncValue;
            if (funcValue == null)
                throw new OperatorTypeException("function call", "function");
            if (funcValue.FuncArgs.Count() != FuncArgs.Count())
                throw new Exception("Arguments to function don't match its definition.");
            // Create a new scope for the function call.
            // Note that its parent is the *function's* environment,
            // not the call environment!
            Env callEnv = new Env(funcValue.FuncEnv);
            // Evaluate function arguments in the old scope and bind them in the new scope.
            for (int i = 0; i < funcValue.FuncArgs.Count(); i++)
            {
                callEnv.BindValue(funcValue.FuncArgs[i], FuncArgs[i].Interpret(env));
            }
            // Execute the function body in the new scope.
            var returnValue = funcValue.Call(callEnv);
            if ((object)returnValue == InternalValue.NoReturn)
                return InternalValue.Void;
            return returnValue;
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            printer.Write(FuncName + "(");
            foreach (var funcArg in FuncArgs)
                funcArg.PrettyPrint(printer, indent);
            printer.Write(")");
        }
    }

    class IdentifierExpr : Expr
    {
        public string IdentifierName { get; }
        public IList<string> PropNames { get; } // may be empty, but not null
        public IdentifierExpr(string identifierName, List<string> properties)
        {
            IdentifierName = identifierName;
            PropNames = properties != null
                ? properties.AsReadOnly()
                : new List<string>().AsReadOnly();
        }
        public override Value Interpret(Env env)
        {
            if (PropNames.Count() == 0)
            {
                return env.Lookup(IdentifierName);
            }
            else
            {
                object obj = env.Lookup(IdentifierName);
                foreach (var propName in PropNames)
                {
                    var objProperty = obj.GetType().GetProperty(propName);
                    if (objProperty == null) // FIXME: Err msg for chained props.
                        throw new PropertyNotFoundException(IdentifierName, propName);
                    obj = objProperty.GetValue(obj);
                }
                return Value.FromNative(obj);
            }
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            var namePieces = new List<string>();
            namePieces.Add(IdentifierName);
            namePieces.AddRange(PropNames);
            printer.Write(string.Join(".", namePieces));
        }
    }

    enum BinOp { MUL, DIV, ADD, SUB, EQU, NEQ, GT, LT, AND, OR, }
    enum UnaryOp { PLUS, MINUS, NOT, }

    /// <summary>
    /// Methods to display BinOp and UnaryOp values as strings.
    /// </summary>
    static class OpStringification
    {
        public static string StringRepr(this BinOp op)
        {
            switch (op)
            {
                case BinOp.ADD: return "+";
                case BinOp.SUB: return "-";
                case BinOp.MUL: return "*";
                case BinOp.DIV: return "/";
                case BinOp.AND: return "and";
                case BinOp.OR: return "or";
                case BinOp.EQU: return "=";
                case BinOp.NEQ: return "<>";
                case BinOp.GT: return ">";
                case BinOp.LT: return "<";
                default: throw new NotImplementedException(
                    "Unexpected binary operator: " + op.ToString());
            }
        }
        public static string StringRepr(this UnaryOp op, Expr operand)
        {
            switch (op)
            {
                case UnaryOp.PLUS: return "+(" + operand.ToString() + ")";
                case UnaryOp.MINUS: return "-(" + operand.ToString() + ")";
                case UnaryOp.NOT: return "not (" + operand.ToString() + ")";
                default: throw new NotImplementedException(
                    "Unexpected unary operator: " + op.ToString());
            }
        }
    }

    class UnaryOpExpr : Expr
    {
        public UnaryOp ExprOperator { get; }
        public Expr Operand { get; }
        public UnaryOpExpr(UnaryOp op, Expr operand)
        {
            ExprOperator = op;
            Operand = operand;
        }
        public override Value Interpret(Env env)
        {
            var operand = Operand.Interpret(env);
            var operandNumber = operand as NumberValue;
            switch (ExprOperator)
            {
                case UnaryOp.PLUS:
                    if (operandNumber == null)
                        throw new OperatorTypeException("+ (unary)", "number");
                    return operandNumber;
                case UnaryOp.MINUS:
                    if (operandNumber == null)
                        throw new OperatorTypeException("- (unary)", "number");
                    return new NumberValue(-operandNumber.Number);
                case UnaryOp.NOT:
                    return BoolValue.FromBool(!operand.IsTrue);
                default:
                    throw new NotImplementedException("Unexpected operator: " + ExprOperator.ToString());
            }
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            printer.Write(ExprOperator.StringRepr(Operand));
        }
    }

    class BinOpExpr : Expr
    {
        public BinOp ExprOperator { get; }
        public Expr Lhs { get; }
        public Expr Rhs { get; }
        public BinOpExpr(BinOp op, Expr lhs, Expr rhs)
        {
            ExprOperator = op;
            Lhs = lhs;
            Rhs = rhs;
        }
        public override Value Interpret(Env env)
        {
            var lhs = Lhs.Interpret(env);
            var rhs = Rhs.Interpret(env);
            var lhsNumber = lhs as NumberValue;
            var rhsNumber = rhs as NumberValue;
            switch (ExprOperator)
            {
                case BinOp.AND:
                    return lhs.IsTrue && rhs.IsTrue ? rhs : BoolValue.False;
                case BinOp.OR:
                    return lhs.IsTrue ? lhs : (rhs.IsTrue ? rhs : BoolValue.False);
                case BinOp.EQU:
                    return lhs.Equ(rhs);
                case BinOp.NEQ:
                    return BoolValue.Not(lhs.Equ(rhs));
                case BinOp.MUL:
                    if (lhsNumber == null || rhsNumber == null)
                        throw new OperatorTypeException("*", "number");
                    return new NumberValue(lhsNumber.Number * rhsNumber.Number);
                case BinOp.DIV:
                    if (lhsNumber == null || rhsNumber == null)
                        throw new OperatorTypeException("/", "number");
                    return new NumberValue(lhsNumber.Number / rhsNumber.Number);
                case BinOp.ADD:
                    if (lhsNumber == null || rhsNumber == null)
                        throw new OperatorTypeException("+", "number");
                    return new NumberValue(lhsNumber.Number + rhsNumber.Number);
                case BinOp.SUB:
                    if (lhsNumber == null || rhsNumber == null)
                        throw new OperatorTypeException("-", "number");
                    return new NumberValue(lhsNumber.Number - rhsNumber.Number);
                case BinOp.GT:
                case BinOp.LT:
                    throw new NotImplementedException("GT and LT are not yet implemented.");
                default:
                    throw new NotImplementedException("Unexpected operator: " + ExprOperator.ToString());
            }
        }
        public override void PrettyPrint(TextWriter printer, int indent)
        {
            var str = string.Format("({0}) {1} ({2})", Lhs, ExprOperator.StringRepr(), Rhs);
            printer.Write(str);
        }
    }
}
