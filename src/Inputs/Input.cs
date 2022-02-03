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

            public Option AddKeybind(Keybind keybind)
            {
                this._keybinds.Add(keybind);
                return this;
            }

            public Option AddSpacer() => this.AddKeybind(Keybind.NULL);

            // TODO create AddExitKeybind() method for consistent exit keybinds (take in instance of option for quitting. if instance is null, exit program?) (+ silent mode w no console output) (+ bool for adding spacer before exit bind)

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
                    if (keybind != Keybind.NULL)
                    {
                        if (keybind.Description is not null)
                        {
                            string s = keybind.KeyChar is null ? keybind.Key.ToString() : keybind.KeyChar.Value.ToString();

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
                    if (keybind != null && keybind != Keybind.NULL && keybind.Action != null && (keybind.Key == inputKeyInfo.Key || (keybind.KeyChar.HasValue && keybind.KeyChar == inputKeyInfo.KeyChar)))
                    {
                        keybind.Action!.Invoke();
                        break;
                    }
                }
            }
        }

        public sealed class Scroll
        {
            private readonly string? _message;

            public Scroll(string? message = null)
            {
                this._message = message;
            }

            // TODO
        }
    }
}
