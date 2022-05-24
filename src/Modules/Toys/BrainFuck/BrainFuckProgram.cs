using B.Inputs;
using B.Utils;

namespace B.Modules.Toys.BrainFuck
{
    public sealed class BrainFuckProgram
    {
        #region Public Properties

        // BrainFuckProgram Title.
        public readonly string Title;
        // Instruction tape for the BrainFuckProgram.
        public readonly char[] Instructions;

        #endregion



        #region Constructors

        // Creates a new BrainFuckProgram by loading the instructions from the specified file.
        public BrainFuckProgram(string title, string fullFilePath)
        {
            Title = title;
            Instructions =
                File.ReadAllText(fullFilePath)
                    .Replace(" ", string.Empty)
                    .ReplaceLineEndings(string.Empty)
                    .ToCharArray();
        }

        #endregion



        #region Public Methods

        // Handles one step through the BrainFuckProgram with current runtime info.
        public void HandleStep(in byte[] memory, ref uint memoryIndex, ref uint instructionIndex, ref uint bracketDepth, ref string output)
        {
            switch (Instructions[instructionIndex])
            {
                // Move Right One Cell
                case '>': memoryIndex++; break;

                // Move Left One Cell
                case '<': memoryIndex--; break;

                // Increase Current Cell Value
                case '+': memory[memoryIndex]++; break;

                // Decrease Current Cell Value
                case '-': memory[memoryIndex]--; break;

                // Output Current Cell
                case '.': output += (char)memory[memoryIndex]; break;

                // Input Current Cell Value from User
                case ',':
                    {
                        Window.Clear();
                        Window.SetSize(30, 10);
                        Cursor.Set(1, 1);
                        Window.Print($"Output: {output}");
                        Cursor.Set(1, 3);
                        Window.Print("Press Input Key...");
                        memory[memoryIndex] = (byte)Input.Get().KeyChar;
                    }
                    break;

                // Loop Begin Bracket
                case '[':
                    {
                        if (memory[memoryIndex] == 0)
                        {
                            bracketDepth++;

                            while (Instructions[instructionIndex] != ']' || bracketDepth != 0)
                            {
                                switch (Instructions[++instructionIndex])
                                {
                                    case '[': bracketDepth++; break;
                                    case ']': bracketDepth--; break;
                                }
                            }
                        }
                    }
                    break;

                // Loop End Bracket
                case ']':
                    {
                        if (memory[memoryIndex] != 0)
                        {
                            bracketDepth++;

                            while (Instructions[instructionIndex] != '[' || bracketDepth != 0)
                            {
                                switch (Instructions[--instructionIndex])
                                {
                                    case '[': bracketDepth--; break;
                                    case ']': bracketDepth++; break;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
