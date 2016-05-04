using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguageTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = new Tester();
            t.RunLexerTests();
            Console.Write("Press a key to exit...");
            Console.ReadKey();
        }
    }
}
