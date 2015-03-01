using System;
using Xunit;
using System.IO;
using System.Text;

namespace PSBrainfuck.Tests
{
    public class BrainfuckInterpreterTests
    {

        [Fact(DisplayName = "Test empty string")]
        public void TestEmptyString()
        {
            BrainfuckInterpreter bi = new BrainfuckInterpreter();
            string result = bi.Interpret("");
            Assert.Equal("", result);
        }

        [Theory(DisplayName = "Tests hello world")]
        [InlineData("++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.")]
        [InlineData(">++++++++[<+++++++++>-]<.>>+>+>++>[-]+<[>[->+<<++++>]<<]>.+++++++..+++.>>+++++++.<<<[[-]<[-]>]<+++++++++++++++.>>.+++.------.--------.>>+.>++++.")]
        public void TestHelloWorld(string instructions)
        {
            BrainfuckInterpreter bi = new BrainfuckInterpreter();
            string result = bi.Interpret(instructions);
            Assert.Equal("Hello World!\n", result);
        }

    }
}
