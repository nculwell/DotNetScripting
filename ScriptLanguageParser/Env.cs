using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguageParser
{
    class Env
    {
        private Dictionary<string, Value> _scope = new Dictionary<string, Value>();
        private Env _parent;
        public Env() { }
        public Env(Env parent) { _parent = parent; }
        public Value Lookup(string name)
        {
            Value v;
            if (_scope.TryGetValue(name, out v))
            {
                return v;
            }
            if (_parent == null)
            {
                throw new IdentifierNotBoundException(name);
            }
            return _parent.Lookup(name);
        }
        public bool IsLocal(string name)
        {
            return _scope.ContainsKey(name);
        }
        public void BindValue(string name, Value value)
        {
            if (_scope.ContainsKey(name))
                throw new DuplicateIdentifierException("Identifier redefined in local scope: " + name);
            _scope.Add(name, value);
        }
        public void SetValue(string name, Value value)
        {
            if (_scope.ContainsKey(name))
            {
                _scope[name] = value;
            }
            else
            {
                if (_parent== null)
                {
                    throw new IdentifierNotBoundException(name);
                }
                else
                {
                    _parent.SetValue(name, value);
                }
            }
        }
    }
}
