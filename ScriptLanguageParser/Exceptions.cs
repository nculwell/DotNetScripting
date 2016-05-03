using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptLanguageParser
{
    [Serializable]
    public class SyntaxException : Exception
    {
        internal SyntaxException(Lexer lexer, string message)
            : base(string.Format("Syntax error (line {0}, col{1}, token '{2}'): {3}",
                lexer.Line, lexer.Column, lexer.CurrentToken.Text, message))
        { }
    }

    [Serializable]
    public class InterpreterException : Exception
    {
        internal InterpreterException(string message) : base(message) { }
    }

    [Serializable]
    public class PropertyAssignmentException : InterpreterException
    {
        internal PropertyAssignmentException(IdentifierExpr lvalueRef, Value assignmentValue)
            : base(string.Format(
                "Property '{0}' does not exist or does not accept values of type '{1}'.",
                lvalueRef.ToString(), assignmentValue.TypeName))
        { }
    }

    [Serializable]
    public class IdentifierNotBoundException : InterpreterException
    {
        internal IdentifierNotBoundException(string identifierName)
            : base("Identifier not bound: " + identifierName)
        { }
    }

    [Serializable]
    public class OperatorTypeException : InterpreterException
    {
        internal OperatorTypeException(string operatorName, string typeName)
            : base("Invalid operands to '" + operatorName + "': must have type " + typeName + ".")
        { }
    }

    [Serializable]
    public class DuplicateIdentifierException : InterpreterException
    {
        internal DuplicateIdentifierException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public class PropertyNotFoundException : InterpreterException
    {
        internal PropertyNotFoundException(string varName, string propName)
            : base(string.Format("Property '{0}' not found in object '{1}'.", propName, varName))
        {
        }
    }

    //[Serializable]
    //public class UnboundIdentifierException : InterpreterException
    //{
    //    internal UnboundIdentifierException(string message) : base(message)
    //    {
    //    }
    //}
}
