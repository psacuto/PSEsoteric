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

        /// <summary>
        /// Crée une nouvelle instance de BrainfuckInterpreter
        /// </summary>
        /// <param name="debug">Option de traces de debug</param>
        public BrainfuckInterpreter(bool debug)
            : this()
        {
            this.debug = debug;
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

            public override string ToString()
            {
                return Pointer + " " + Cells[Pointer] + " "  + InstructionIndex + " " + Instructions[InstructionIndex];
            }

            public Action<string> PutStringAction { get; private set; }
        }

        //>	Move the pointer to the right
        //<	Move the pointer to the left
        //+	Increment the memory cell under the pointer
        //-	Decrement the memory cell under the pointer
        //.	Output the character signified by the cell at the pointer
        //,	Input a character and store it in the cell at the pointer
        //[	Jump past the matching ] if the cell under the pointer is 0
        //]	Jump back to the matching [ if the cell under the pointer is nonzero
        Dictionary<char, Action<Context>> ops = new Dictionary<char, Action<Context>>
        {
            { '>', new Action<Context>(Right) },
            { '<', new Action<Context>(Left) },
            { '+', new Action<Context>(Increment) },
            { '-', new Action<Context>(Decrement) },
            { '.', new Action<Context>(Write) },
            { ',', new Action<Context>(Read) },
            { '[', new Action<Context>(JumpPast) },
            { ']', new Action<Context>(JumpBack) },
        };
        private bool p;
        private bool debug;

        private static void Right(Context context)
        {
            context.Pointer++;
        }

        private static void Left(Context context)
        {
            context.Pointer--;
        }

        private static void Increment(Context context)
        {
            context.Cells[context.Pointer]++;
        }

        private static void Decrement(Context context)
        {
            context.Cells[context.Pointer]--;
        }

        private static void Write(Context context)
        {
            context.PutStringAction(char.ConvertFromUtf32(context.Cells[context.Pointer]));
            context.OutputStream.WriteByte(context.Cells[context.Pointer]);
        }

        private static void Read(Context context)
        {
            var readData = context.OutputStream.ReadByte();
            context.Cells[context.Pointer] = (byte)readData;
        }

        private static void JumpPast(Context context)
        {
            var currentVal = context.Cells[context.Pointer];
            if (currentVal == 0)
                context.InstructionIndex = FindClosingJump(context.InstructionIndex, context.Instructions);
        }

        private static void JumpBack(Context context)
        {
            var currentVal = context.Cells[context.Pointer];
            if (currentVal != 0)
                context.InstructionIndex = FindOpeningJump(context.InstructionIndex, context.Instructions);
        }

        private static int FindOpeningJump(int instructionIndex, string instructions)
        {
            int count = 1;
            int index = instructionIndex - 1;
            while (count > 0)
            {
                if (instructions[index] == ']')
                    count++;
                if (instructions[index] == '[')
                    count--;
                index--;
            }
            return index+1;
        }

        private static int FindClosingJump(int instructionIndex, string instructions)
        {
            int count = 1;
            int index = instructionIndex + 1;
            while (count > 0)
            {
                if (instructions[index] == ']')
                    count--;
                if (instructions[index] == '[')
                    count++;
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
                char op = instructions[context.InstructionIndex];
                ops[op](context);
                context.InstructionIndex++;
                if (debug)
                    Debug.WriteLine(context.ToString());
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
