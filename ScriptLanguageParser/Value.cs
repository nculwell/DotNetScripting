using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguageParser
{
    abstract class Value
    {
        public virtual bool IsTrue { get { return true; } }

        public abstract string TypeName { get; }

        public abstract Value Equ(Value rhs);

        public static Value FromNative(object val)
        {
            switch (val.GetType().FullName)
            {
                case "System.String": return new StringValue(val as string);
                case "System.Int32": return new NumberValue(new decimal((Int32)val));
                case "System.Int64": return new NumberValue(new decimal((Int64)val));
                case "System.UInt32": return new NumberValue(new decimal((UInt32)val));
                case "System.UInt64": return new NumberValue(new decimal((UInt64)val));
                case "System.Decimal": return new NumberValue((Decimal)val);
                case "System.Boolean": return BoolValue.FromBool((Boolean)val);
                default: throw new ArgumentException(
                        "Unable to wrap native value of type '" + val.GetType().FullName + "'.");
            }
        }
    }

    /// <summary>
    /// This class implements some "values" that are only used to communicate
    /// state within the interpreter. It is a bug for a user program to see
    /// these values directly.
    ///
    /// FIXME: How do we hide "void" from the user? Should it be some other
    /// value instead? Or maybe that one should be visible? 
    /// </summary>
    class InternalValue : Value
    {
        private static InternalValue _noReturn = new InternalValue();
        private static InternalValue _void = new InternalValue();

        /// <summary> Indicates that the previous statement didn't have a value
        /// because it didn't result in a function return. </summary>
        public static InternalValue NoReturn { get { return _noReturn; } }
        /// <summary> Indicates that the previous statement didn't have a value
        /// because it resulted in a void function return. </summary>
        public static InternalValue Void { get { return _void; } }

        public override string TypeName { get { return "InternalValue"; } }
        public override bool IsTrue
        {
            get { throw new NotImplementedException("InternalValue has no truth value."); }
        }
        public override Value Equ(Value rhs)
        {
            throw new NotImplementedException("InternalValue cannot be compared.");
        }
        public override string ToString()
        {
            return (object)this == _noReturn ? "NoReturn" : "Void";
        }
    }

    class BoolValue : Value
    {
        public override string TypeName { get { return "Boolean"; } }
        private static BoolValue _false = new BoolValue(false);
        private static BoolValue _true = new BoolValue(true);
        public static Value False { get { return _false; } }
        public static Value True { get { return _true; } }
        public static Value FromBool(bool value) { return value ? True : False; }
        public static Value Not(Value value) { return FromBool(!value.IsTrue); }
        public static Value Not(bool value) { return FromBool(!value); }

        private bool _isTrue;
        private BoolValue(bool isTrue) { _isTrue = IsTrue; }
        public override bool IsTrue { get { return _isTrue; } }
        public override Value Equ(Value rhs)
        {
            var r = rhs as BoolValue;
            return FromBool(r != null && IsTrue == rhs.IsTrue);
        }
        public override string ToString()
        {
            return _isTrue ? "True" : "False";
        }
    }

    class StringValue : Value
    {
        public override string TypeName { get { return "String"; } }
        public string String { get; }
        public StringValue(string s) { String = s; }
        public override Value Equ(Value rhs)
        {
            var r = rhs as StringValue;
            return BoolValue.FromBool(r != null && String == r.String);
        }
        public override string ToString()
        {
            return "\"" + String + "\"";
        }
    }

    class NumberValue : Value
    {
        public override string TypeName { get { return "Number"; } }
        public decimal Number { get; }
        public NumberValue(decimal n) { Number = n; }
        public NumberValue(int n) { Number = new decimal(n); }
        public override Value Equ(Value rhs)
        {
            var r = rhs as NumberValue;
            return BoolValue.FromBool(r != null && Number == r.Number);
        }
        public override string ToString()
        {
            return Number.ToString();
        }
    }

    class FuncValue : Value
    {
        public override string TypeName { get { return "Function"; } }
        public string FuncName { get; }
        public Env FuncEnv { get; }
        public List<string> FuncArgs { get; }
        private StatementBlock _funcBody;
        public FuncValue(string funcName, Env funcEnv, List<string> funcArgs, StatementBlock funcBody)
        {
            FuncName = funcName;
            FuncEnv = funcEnv;
            FuncArgs = funcArgs;
            _funcBody = funcBody;
        }
        public Value Call(Env callEnv)
        {
            // This is defined here so it can be overridden.
            // The callEnv argument has the function arguments
            // bound in the local scope.
            return _funcBody.Interpret(callEnv);
        }
        public override Value Equ(Value rhs)
        {
            return BoolValue.FromBool((object)this == rhs);
        }
        public override string ToString()
        {
            return "<func:" + FuncName + ">";
        }
    }

    class NativeObjectValue : Value
    {
        public override string TypeName { get { return "NativeObject"; } }
        public object NativeObject { get; }
        public NativeObjectValue(object nativeObject)
        {
            if (nativeObject == null)
            {
                throw new ArgumentNullException("nativeObject");
            }
            NativeObject = nativeObject;
        }
        public override Value Equ(Value rhs)
        {
            return BoolValue.FromBool(NativeObject == rhs);
        }
        public override string ToString()
        {
            return "<object:" + NativeObject.GetType().FullName + ">";
        }
    }
}
