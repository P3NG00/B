using B.Utils;

namespace B.Inputs
{
    static class Input
    {
        public static string String = string.Empty;
        public static int? Int => int.TryParse(Input.String, out int num) ? num : null;
        public static decimal? Decimal => decimal.TryParse(Input.String, out decimal num) ? num : null;

        public static ConsoleKey Request(int maxLength)
        {
            ConsoleKeyInfo keyInfo = Util.GetInput();

            if (keyInfo.Key == ConsoleKey.Backspace)
                Input.String = Input.String.Substring(0, Math.Max(0, Input.String.Length - 1));
            else if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape)
            {
                if (Input.String.Length < maxLength)
                    Input.String += keyInfo.KeyChar;
            }

            return keyInfo.Key;
        }

        public sealed class Option
        {
            private readonly Utils.List<Keybind> _keybinds = new Utils.List<Keybind>();
            private readonly string? _message;

            public Option(string? message = null) => this._message = message;

            public Option Add(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey))
            {
                this._keybinds.Add(new(action, description, keyChar, key));
                return this;
            }

            public Option AddSpacer()
            {
                this._keybinds.Add(null!);
                return this;
            }

            // Input Option Requests log the last pressed key in Util.LastInput.
            // This is because the method uses Util.GetInput() which will log the pressed key and return it.
            public void Request()
            {
                bool printLine = false;

                if (this._message is not null)
                {
                    Util.PrintLine();
                    Util.Print($"  {this._message}");
                    printLine = true;
                }

                foreach (Keybind keybind in this._keybinds)
                {
                    // If keybind is null, add spacer in display
                    // If keybind description is null, don't display option
                    if (keybind != null)
                    {
                        if (keybind.Description != null)
                        {
                            string s = keybind.KeyChar == null ? keybind.Key.ToString() : keybind.KeyChar.Value.ToString();

                            if (printLine)
                            {
                                printLine = false;
                                Util.PrintLine();
                            }

                            Util.PrintLine();
                            Util.Print($" {s}) {keybind.Description}");
                        }
                    }
                    else if (!printLine)
                        printLine = true;
                }

                Util.PrintLine();
                ConsoleKeyInfo inputKeyInfo = Util.GetInput();

                foreach (Keybind keybind in this._keybinds)
                {
                    if (keybind != null && keybind.Action != null && (keybind.Key == inputKeyInfo.Key || (keybind.KeyChar.HasValue && keybind.KeyChar == inputKeyInfo.KeyChar)))
                    {
                        keybind.Action!.Invoke();
                        break;
                    }
                }
            }
        }
    }
}
