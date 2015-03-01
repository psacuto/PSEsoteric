using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace PSBrainfuck.Tests
{
    [TestClass]
    public class BrainfuckInterpreterTests
    {

        [TestMethod]
        public void TestEmptyString()
        {
            BrainfuckInterpreter bi = new BrainfuckInterpreter();
            string result = bi.Interpret("");
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void TestHelloWorld()
        {
            BrainfuckInterpreter bi = new BrainfuckInterpreter();
            string result = bi.Interpret("++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.");
            Assert.AreEqual("Hello World!\n", result);
        }

        [TestMethod]
        public void TestHelloWorld2()
        {
            BrainfuckInterpreter bi = new BrainfuckInterpreter();
            string result = bi.Interpret(">++++++++[<+++++++++>-]<.>>+>+>++>[-]+<[>[->+<<++++>]<<]>.+++++++..+++.>>+++++++.<<<[[-]<[-]>]<+++++++++++++++.>>.+++.------.--------.>>+.>++++.");
            Assert.AreEqual("Hello World!\n", result);
        }
    }
}
