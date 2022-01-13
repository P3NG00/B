using B.Utils;

namespace B.Inputs
{
    static class Input
    {
        public static string Str = string.Empty;
        public static int Int = 0;

        public static ConsoleKey RequestString(int maxLength)
        {
            return Input.Request(ref Input.Str,
                (keyInfo, str) =>
                {
                    if (str.Length < maxLength)
                        str += keyInfo.KeyChar;

                    return str;
                },
                (str) => str.Substring(0, Math.Max(0, str.Length - 1)));
        }

        public static ConsoleKey RequestInt()
        {
            return Input.Request(ref Input.Int,
                (keyInfo, num) =>
                {
                    string numStr = num.ToString();
                    numStr += keyInfo.KeyChar;
                    int n = num;

                    if (!int.TryParse(numStr, out num))
                        num = n;

                    return num;
                },
                (str) =>
                {
                    string numStr = str.ToString();
                    numStr = numStr.Substring(0, Math.Max(0, numStr.Length - 1));
                    int.TryParse(numStr, out str);
                    return str;
                });
        }

        private static ConsoleKey Request<T>(ref T tObj, Func<ConsoleKeyInfo, T, T> funcDefault, Func<T, T> funcBackspace)
        {
            ConsoleKeyInfo keyInfo = Util.GetInput();

            if (keyInfo.Key == ConsoleKey.Backspace)
                tObj = funcBackspace.Invoke(tObj);
            else if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape)
                tObj = funcDefault.Invoke(keyInfo, tObj);

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
                if (this._message != Util.NULL_STRING)
                    Util.Print(this._message, 2, linesBefore: 1);

                bool printLine = true;
                string s;

                foreach (Keybind keybind in this._keybinds)
                {
                    // If keybind is null, add spacer in display
                    // If keybind description is null, don't display option
                    if (keybind != Keybind.NULL)
                    {
                        if (keybind.Description != Util.NULL_STRING)
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
                    if (!Keybind.IsNull(keybind) && (keybind.Key == inputKeyInfo.Key || (keybind.KeyChar != Util.NULL_CHAR && keybind.KeyChar == inputKeyInfo.KeyChar)))
                    {
                        keybind.Action!.Invoke();
                        break;
                    }
                }
            }
        }
    }
}
