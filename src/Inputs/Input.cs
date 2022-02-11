using B.Utils;

namespace B.Inputs
{
    static class Input
    {
        public static string String = string.Empty;
        public static int? Int => int.TryParse(Input.String, out int num) ? num : null;
        public static decimal? Decimal => decimal.TryParse(Input.String, out decimal num) ? num : null;
        public static int ScrollIndex = 0;

        public static ConsoleKey RequestLine(int maxLength)
        {
            ConsoleKeyInfo keyInfo = Util.GetKey();

            if (keyInfo.Key == ConsoleKey.Backspace)
                Input.String = Input.String.Substring(0, Math.Max(0, Input.String.Length - 1));
            else if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape && Input.String.Length < maxLength)
                Input.String += keyInfo.KeyChar;

            return keyInfo.Key;
        }

        public static ConsoleKey RequestScroll<T>(T[] items, Func<T, string> getText, int? maxEntriesPerPage = null, Keybind exitKeybind = null!, params Keybind[] extraKeybinds)
        {
            Util.PrintLine();
            int maxEntries = maxEntriesPerPage.HasValue ? maxEntriesPerPage.Value : items.Length;
            Input.Option iob = new();

            if (maxEntries > 0)
            {
                int startIndex = Input.ScrollIndex - (Input.ScrollIndex % maxEntries);
                int endIndex = Math.Min(startIndex + maxEntries, items.Length);

                for (int i = startIndex; i < endIndex; i++)
                {
                    string text = getText(items[i]);
                    Util.PrintLine(Input.ScrollIndex == i ? $" > {text}" : string.Format("  {0,-" + (text.Length + 1) + "}", text));
                }

                Util.PrintLine();
                Util.PrintLine(" Use Up/Down Arrow to navigate.");
                bool hasExtraKeybinds = extraKeybinds != null && extraKeybinds.Length != 0;

                if (hasExtraKeybinds)
                    Util.PrintLine();

                // Get page index before it's modified
                int lastPageIndex = Input.ScrollIndex % maxEntries;
                iob.Add(() => Input.ScrollIndex--, key: ConsoleKey.UpArrow)
                    .Add(() => Input.ScrollIndex++, key: ConsoleKey.DownArrow);

                if (hasExtraKeybinds)
                    iob.Add(extraKeybinds!);

                iob.AddSpacer()
                    .Add(exitKeybind)
                    .Request();

                // Fix value
                Input.ScrollIndex = Util.Clamp(Input.ScrollIndex, 0, items.Length - 1);
                // Get new value
                int newPageIndex = Input.ScrollIndex % maxEntries;
                // If crossing into new page, clear console
                int oneLessThanMax = maxEntries - 1;

                if ((lastPageIndex == oneLessThanMax && newPageIndex == 0) || (lastPageIndex == 0 && newPageIndex == oneLessThanMax))
                    Util.ClearConsole();
            }
            else
            {
                Util.PrintLine("  No entries.");
                iob.Add(exitKeybind)
                    .Request();
            }

            return Util.LastInput.Key;
        }

        public sealed class Option
        {
            private readonly Utils.List<Keybind> _keybinds = new Utils.List<Keybind>();
            private readonly string? _message;

            public Option(string? message = null) => this._message = message;

            public Option Add(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey))
            {
                this._keybinds.Add(new Keybind(action, description, keyChar, key));
                return this;
            }

            public Option Add(params Keybind[] keybinds)
            {
                this._keybinds.Add(keybinds);
                return this;
            }

            public Option AddSpacer()
            {
                this._keybinds.Add(new Keybind[] { null! });
                return this;
            }

            public Option AddExit(Options.Option option, bool spacer = true)
            {
                if (spacer)
                    this.AddSpacer();

                return this.Add(() => option.Quit(), "Exit", key: ConsoleKey.Escape);
            }

            // Input Option Requests log the last pressed key in Util.LastInput.
            // This is because the method uses Util.GetInput() which will log the pressed key and return it.
            public void Request()
            {
                // Print out input options
                bool printLine = false;

                if (this._message is not null)
                {
                    Util.PrintLine();
                    Util.PrintLine($"  {this._message}");
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
                            if (printLine)
                            {
                                printLine = false;
                                Util.PrintLine();
                            }

                            Util.PrintLine($" {(keybind.KeyChar == null ? keybind.Key.ToString() : keybind.KeyChar.Value.ToString())}) {keybind.Description}");
                        }
                    }
                    else if (!printLine)
                        printLine = true;
                }

                ConsoleKeyInfo inputKeyInfo = Util.GetKey();

                // Activate function for pressed keybind
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
