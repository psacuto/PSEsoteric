using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSBrainfuck
{
    /// <summary>
    /// Interpréteur de langage Ook
    /// </summary>
    public class BrainfuckInterpreter
    {
        private System.IO.Stream outputStream;
        private System.IO.Stream inputStream;


        /// <summary>
        /// Crée une nouvelle instance de BrainfuckInterpreter
        /// </summary>
        public BrainfuckInterpreter()
            : this(Console.OpenStandardInput(), Console.OpenStandardOutput())
        {

        }

        /// <summary>
        /// Crée une nouvelle instance de BrainfuckInterpreter
        /// </summary>
        /// <param name="outputStream">Le stream d'entrée</param>
        /// <param name="outputStream">Le stream de sortie</param>
        public BrainfuckInterpreter(System.IO.Stream inputStream, System.IO.Stream outputStream)
        {
            this.inputStream = inputStream;
            this.outputStream = outputStream;
        }

        private class Context
        {

            public Context(Action<string> putStringAction)
            {
                this.PutStringAction = putStringAction;
            }

            public int Pointer { get; set; }
            public byte[] Cells { get; set; }
            public string Instructions { get; set; }

            public int InstructionIndex { get; set; }

            public System.IO.Stream InputStream { get; set; }

            public System.IO.Stream OutputStream { get; set; }

            public Action<string> PutStringAction { get; private set; }

            public override string ToString()
            {
                return Pointer + Cells[Pointer] + " " + InstructionIndex + " " + GetOp(InstructionIndex - 1) + " " + GetOp(InstructionIndex) + " " + GetOp(InstructionIndex + 1);
            }

            private string GetOp(int pointer)
            {
                if (pointer >= 0 && pointer < Instructions.Length)
                    return Instructions[pointer].ToString();
                return string.Empty;
            }

        }

        //>	Move the pointer to the right
        //<	Move the pointer to the left
        //+	Increment the memory cell under the pointer
        //-	Decrement the memory cell under the pointer
        //.	Output the character signified by the cell at the pointer
        //,	Input a character and store it in the cell at the pointer
        //[	Jump past the matching ] if the cell under the pointer is 0
        //]	Jump back to the matching [ if the cell under the pointer is nonzero
        Dictionary<char, Func<Context, int>> ops = new Dictionary<char, Func<Context, int>>
        {
            { '>', Right },
            { '<', Left },
            { '+', Increment },
            { '-', Decrement },
            { '.', Write },
            { ',', Read },
            { '[', JumpPast },
            { ']', JumpBack },
        };

        private static int Right(Context context)
        {
#if DEBUG
            Debug.WriteLine("MOVERIGHT [" + context.Pointer + "]");
#endif
            context.Pointer++;
            return context.InstructionIndex + 1;
        }

        private static int Left(Context context)
        {
#if DEBUG
            Debug.WriteLine("MOVELEFT [" + context.Pointer + "]");
#endif
            context.Pointer--;
            return context.InstructionIndex + 1;
        }

        private static int Increment(Context context)
        {
            context.Cells[context.Pointer]++;
#if DEBUG
            byte val = context.Cells[context.Pointer];
            Debug.WriteLine("INCREMENT [" + context.Pointer + "] -> " + char.ConvertFromUtf32(val) + "(" + val + ")");
#endif
            return context.InstructionIndex + 1;
        }

        private static int Decrement(Context context)
        {
#if DEBUG
            byte val = context.Cells[context.Pointer];
            Debug.WriteLine("DECREMENT [" + context.Pointer + "] -> " + char.ConvertFromUtf32(val) + "(" + val + ")");
#endif
            context.Cells[context.Pointer]--;
            return context.InstructionIndex + 1;
        }

        private static int Write(Context context)
        {
            string s = char.ConvertFromUtf32(context.Cells[context.Pointer]);
            context.PutStringAction(s);
            context.OutputStream.WriteByte(context.Cells[context.Pointer]);
#if DEBUG
            Debug.WriteLine("PUTCHAR [" + s + "]");
#endif
            return context.InstructionIndex + 1;
        }

        private static int Read(Context context)
        {
            var readData = context.OutputStream.ReadByte();
            context.Cells[context.Pointer] = (byte)readData;
#if DEBUG
            Debug.WriteLine("GETCHAR [" + char.ConvertFromUtf32(readData) + "]");
#endif
            return context.InstructionIndex + 1;            
        }

        private static int JumpPast(Context context)
        {
            var currentVal = context.Cells[context.Pointer];
            if (currentVal == 0)
            {
                int jumpTo = FindClosingJump(context.InstructionIndex, context.Instructions) + 1;
#if DEBUG
                Debug.WriteLine("JUMPPAST [" + jumpTo + "]");
#endif
                return jumpTo;
            }
            return context.InstructionIndex + 1;
        }

        private static int JumpBack(Context context)
        {
            var currentVal = context.Cells[context.Pointer];
            if (currentVal != 0)
            {
                int jumpTo = FindOpeningJump(context.InstructionIndex, context.Instructions);
#if DEBUG
                Debug.WriteLine("JUMPBACK [" + jumpTo + "]");
#endif
                return jumpTo;
            }
            return context.InstructionIndex + 1;
        }

        private static int FindOpeningJump(int instructionIndex, string instructions)
        {
            int count = 1;
            int index = instructionIndex - 1;
            char op;
            while (count > 0)
            {
                op = instructions[index];
                if (op == ']')
                    count++;
                if (op == '[')
                    count--;
                if (count > 0)
                    index--;
            }
            return index;
        }

        private static int FindClosingJump(int instructionIndex, string instructions)
        {
            int count = 1;
            int index = instructionIndex + 1;
            char op;
            while (count > 0)
            {
                op = instructions[index];
                if (op == ']')
                    count--;
                if (op == '[')
                    count++;
                if (count > 0)
                    index++;
            }
            return index;
        }


        /// <summary>
        /// Interprète les instructions passées en paramètre
        /// </summary>
        /// <param name="instructions">Les instructions passées en paramètre</param>
        public string Interpret(string instructions)
        {
            string output = string.Empty;
            instructions = Clean(instructions);
            Context context = new Context((s) => output += s)
            {
                Pointer = 0,
                Cells = new byte[256],
                Instructions = instructions,
                InputStream = inputStream,
                OutputStream = outputStream,
            };
            while (context.InstructionIndex < instructions.Length)
            {
#if DEBUG
                Debug.WriteLine(context.ToString());
#endif
                char op = instructions[context.InstructionIndex];
                context.InstructionIndex = ops[op](context);
            }
            return output;
        }

        /// <summary>
        /// Nettoie la chaîne d'instructions des caractères ne correspondant pas à une instruction
        /// </summary>
        /// <param name="instructions">La chaîne d'instructions à nettoyer</param>
        /// <returns>La chaîne nettoyée</returns>
        private string Clean(string instructions)
        {
            return string.Concat(instructions.Where(c => ops.ContainsKey(c)));
        }
    }
}
