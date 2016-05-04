using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptLanguageParser;

namespace ScriptLanguageTests
{
    class Tester
    {
        public delegate void TestRunner(TextReader input, TextWriter output);

        public void RunTests()
        {
            Console.WriteLine("Deleting old test output.");
            var testOutputFilenames = ListTests("*.testout.txt");
            foreach (var filename in testOutputFilenames)
            {
                File.Delete(filename);
            }
            Console.WriteLine("Running lexer tests.");
            RunTestBatch("lex*", RunLexerTest);
        }

        public void RunTestBatch(string filenamePattern, TestRunner runner)
        {
            var testInputFilenames = ListTests(filenamePattern + ".input.txt");
            foreach (var inputFilename in testInputFilenames)
            {
                var expectedOutputFilename = inputFilename.Replace(".input.", ".output.");
                var testOutputFilename = inputFilename.Replace(".input.", ".testout.");
                Console.WriteLine(Path.GetFileName(inputFilename) + " => " + Path.GetFileName(testOutputFilename));
                using (var input = new StreamReader(inputFilename))
                {
                    //var output = Console.Out;
                    using (var output = new StreamWriter(testOutputFilename))
                    {
                        try
                        {
                            RunLexerTest(input, output);
                        }
                        catch (Exception ex)
                        {
                            output.WriteLine(ex.Message);
                            continue;
                        }
                    }
                }
                using (var expectedOutput = new StreamReader(expectedOutputFilename))
                {
                    using (var testOutput = new StreamReader(testOutputFilename))
                    {
                        var line = 1;
                        string expLine, tstLine;
                        while (true)
                        {
                            expLine = expectedOutput.ReadLine();
                            tstLine = testOutput.ReadLine();
                            if (expLine == null && tstLine != null)
                            {
                                Console.WriteLine("[FAIL] Test output is longer than expected output.");
                                break;
                            }
                            else if (expLine != null && tstLine == null)
                            {
                                Console.WriteLine("[FAIL] Expected output is longer than test output.");
                                break;
                            }
                            if (expLine == null || tstLine == null)
                            {
                                Console.WriteLine("[PASS] Test output matches expected output.");
                                break;
                            }
                            if (expLine != tstLine)
                            {
                                Console.WriteLine("[FAIL] Output differs on line " + line + ":");
                                Console.WriteLine("  Expect:  " + expLine);
                                Console.WriteLine("  Test:    " + tstLine);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<string> ListTests(string filenamePattern)
        {
            var testDir = Path.Combine(Environment.CurrentDirectory, @"..\..\testfiles");
            var files = Directory.GetFiles(testDir, filenamePattern);
            return files;
        }

        public void RunLexerTest(TextReader input, TextWriter output)
        {
            var lexer = new Lexer(input.ReadToEnd());
            while (lexer.FollowingToken.Type != TokenType.EOF)
            {
                lexer.Advance();
                output.WriteLine(lexer.CurrentToken.ToString());
            }
        }

        public void RunParserTest(TextReader input, TextWriter output)
        {
            var lexer = new Lexer(input.ReadToEnd());
            var parser = new Parser(lexer);
            var script = parser.ParseScript();
            output.Write(script.ToString());
        }
    }
}
