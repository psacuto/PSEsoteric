using System;
using Xunit;
using System.IO;
using System.Text;

namespace PSBrainfuck.Tests
{
    public class BrainfuckInterpreterTests
    {

        [Fact]
        public void TestEmptyString()
        {
            BrainfuckInterpreter bi = new BrainfuckInterpreter();
            string result = bi.Interpret("");
            Assert.Equal("", result);
        }

        [Fact]
        public void TestHelloWorld()
        {
            BrainfuckInterpreter bi = new BrainfuckInterpreter();
            string result = bi.Interpret("++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.");
            Assert.Equal("Hello World!\n", result);
        }

        [Fact]
        public void TestHelloWorld2()
        {
            BrainfuckInterpreter bi = new BrainfuckInterpreter();
            string result = bi.Interpret(">++++++++[<+++++++++>-]<.>>+>+>++>[-]+<[>[->+<<++++>]<<]>.+++++++..+++.>>+++++++.<<<[[-]<[-]>]<+++++++++++++++.>>.+++.------.--------.>>+.>++++.");
            Assert.Equal("Hello World!\n", result);
        }
    }
}
