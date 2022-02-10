using B.Utils;

namespace B.Options.BrainFuck
{
    public sealed class BrainFuckProgram
    {
        public readonly string Title;
        public readonly char[] Instructions;

        public BrainFuckProgram(string title, string filePath)
        {
            this.Title = title;
            this.Instructions = File.ReadAllText(filePath).Replace(" ", string.Empty).ReplaceLineEndings(string.Empty).ToCharArray();
        }

        public void HandleStep(in byte[] memory, ref byte memoryIndex, ref uint instructionIndex, ref uint bracketDepth, ref string output)
        {
            switch (this.Instructions[instructionIndex])
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
                case ',': memory[memoryIndex] = (byte)Util.GetInput().KeyChar; break;

                // Loop Begin Bracket
                case '[':
                    {
                        if (memory[memoryIndex] == 0)
                        {
                            bracketDepth++;

                            while (this.Instructions[instructionIndex] != ']' || bracketDepth != 0)
                            {
                                switch (this.Instructions[++instructionIndex])
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

                            while (this.Instructions[instructionIndex] != '[' || bracketDepth != 0)
                            {
                                switch (this.Instructions[--instructionIndex])
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
    }
}
