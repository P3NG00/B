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
            private readonly string _message;

            public Option(string message = Util.NULL_STRING) => this._message = message;

            public Option AddKeybind(Keybind keybind)
            {
                this._keybinds.Add(keybind);
                return this;
            }

            public Option AddSpacer() => this.AddKeybind(Keybind.NULL);

            public void Request()
            {
                if (this._message != string.Empty)
                    Util.Print(this._message, 2, linesBefore: 1);

                bool printLine = true;
                string s;

                foreach (Keybind keybind in this._keybinds)
                {
                    // If keybind is null, add spacer in display
                    // If keybind description is null, don't display option
                    if (keybind != Keybind.NULL)
                    {
                        if (keybind.Description != string.Empty)
                        {
                            s = keybind.KeyChar == Util.NULL_CHAR ? keybind.Key.ToString() : keybind.KeyChar.ToString();

                            if (printLine)
                            {
                                printLine = false;
                                Util.Print();
                            }

                            Util.Print($"{s}) {keybind.Description}", 1);
                        }
                    }
                    else if (!printLine)
                        printLine = true;
                }

                ConsoleKeyInfo inputKeyInfo = Util.GetInput();

                foreach (Keybind keybind in this._keybinds)
                {
                    if (keybind != null && keybind != Keybind.NULL && keybind.Action != null && (keybind.Key == inputKeyInfo.Key || (keybind.KeyChar != Util.NULL_CHAR && keybind.KeyChar == inputKeyInfo.KeyChar)))
                    {
                        keybind.Action!.Invoke();
                        break;
                    }
                }
            }
        }
    }
}
