using B.Options;
using B.Utils;

namespace B.Inputs
{
    static class Input
    {
        public static int ScrollIndex;
        public static string String;

        public static int? Int => int.TryParse(Input.String, out int num) ? num : null;
        public static decimal? Decimal => decimal.TryParse(Input.String, out decimal num) ? num : null;

        static Input()
        {
            Input.ScrollIndex = 0;
            Input.String = string.Empty;
        }

        public static ConsoleKeyInfo RequestLine(int maxLength)
        {
            ConsoleKeyInfo keyInfo = Util.GetKey();

            if (keyInfo.Key == ConsoleKey.Backspace)
                Input.String = Input.String.Substring(0, Math.Max(0, Input.String.Length - 1));
            else if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape && Input.String.Length < maxLength)
                Input.String += keyInfo.KeyChar;

            return keyInfo;
        }

        public static ConsoleKeyInfo RequestScroll<T>(
            T[] items,
            Func<T, string> getText,
            Func<T, ConsoleColor?> getTextColor = null!,
            Func<T, ConsoleColor?> getBackgroundColor = null!,
            string title = null!,
            int? maxEntriesPerPage = null,
            ScrollType scrollType = ScrollType.Indent,
            bool navigationKeybinds = true,
            Keybind exitKeybind = null!,
            params Keybind[] extraKeybinds)
        {
            int maxEntries = maxEntriesPerPage.HasValue ? maxEntriesPerPage.Value : items.Length;
            Input.Choice iob = new();
            ConsoleKeyInfo keyInfo;

            if (maxEntries > 0 && items.Length > 0)
            {
                if (title != null)
                {
                    Util.PrintLine();
                    Util.PrintLine($"  {title}");
                    Util.PrintLine();
                }

                int startIndex = Input.ScrollIndex - (Input.ScrollIndex % maxEntries);
                int endIndex = Math.Min(startIndex + maxEntries, items.Length);

                for (int i = startIndex; i < endIndex; i++)
                {
                    bool isIndex = Input.ScrollIndex == i;
                    string preface;

                    switch (scrollType)
                    {
                        case ScrollType.Side: preface = isIndex ? $" > " : "   "; break;

                        // ScrollType.Indent (default)
                        default: preface = isIndex ? $" > " : "  "; break;
                    }

                    Util.Print(preface);
                    T item = items[i];
                    string text = getText(item);
                    string output = string.Format("{0,-" + (text.Length + 1) + "}", text);
                    ConsoleColor? colorText = null;
                    ConsoleColor? colorBackground = null;

                    if (getTextColor != null)
                        colorText = getTextColor(item);

                    if (getBackgroundColor != null)
                        colorBackground = getBackgroundColor(item);

                    Util.Print(output, colorText, colorBackground);
                    Util.PrintLine();
                }

                if (navigationKeybinds)
                {
                    Util.PrintLine();
                    Util.PrintLine(" Use Up/Down Arrow to navigate.");
                }

                bool hasExtraKeybinds = extraKeybinds != null && extraKeybinds.Length != 0;

                if (hasExtraKeybinds)
                    Util.PrintLine();

                // Get page index before it's modified
                int lastPageIndex = Input.ScrollIndex % maxEntries;

                if (navigationKeybinds)
                    iob.Add(() => Input.ScrollIndex--, key: ConsoleKey.UpArrow)
                        .Add(() => Input.ScrollIndex++, key: ConsoleKey.DownArrow);

                if (hasExtraKeybinds)
                    iob.Add(extraKeybinds!);

                keyInfo = iob.AddSpacer()
                    .Add(exitKeybind)
                    .Request();

                Input.ScrollIndex = Util.Clamp(Input.ScrollIndex, 0, items.Length - 1);
                int newPageIndex = Input.ScrollIndex % maxEntries;
                int oneLessThanMax = maxEntries - 1;

                // If crossing into new page, clear console
                if ((lastPageIndex == oneLessThanMax && newPageIndex == 0) || (lastPageIndex == 0 && newPageIndex == oneLessThanMax))
                    Util.Clear();
            }
            else
            {
                Util.PrintLine("  No entries.");
                Util.PrintLine();
                keyInfo = iob.Add(exitKeybind)
                    .Request();
            }

            return keyInfo;
        }

        public sealed class Choice
        {
            private readonly Utils.List<Keybind> _keybinds = new Utils.List<Keybind>();
            private readonly string? _message;

            public Choice(string? message = null) => this._message = message;

            public Choice Add(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey))
            {
                this._keybinds.Add(new Keybind(action, description, keyChar, key));
                return this;
            }

            public Choice Add(params Keybind[] keybinds)
            {
                this._keybinds.Add(keybinds);
                return this;
            }

            public Choice AddSpacer()
            {
                this._keybinds.Add(new Keybind[] { null! });
                return this;
            }

            public Choice AddExit(IOption option, bool addSpacerBefore = true)
            {
                if (addSpacerBefore)
                    this.AddSpacer();

                return this.Add(() => option.Quit(), "Exit", key: ConsoleKey.Escape);
            }

            public ConsoleKeyInfo Request()
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

                ConsoleKeyInfo keyInfo = Util.GetKey();

                // Activate function for pressed keybind
                foreach (Keybind keybind in this._keybinds)
                {
                    if (keybind != null && keybind.Action != null && (keybind.Key == keyInfo.Key || (keybind.KeyChar.HasValue && keybind.KeyChar == keyInfo.KeyChar)))
                    {
                        keybind.Action!.Invoke();
                        break;
                    }
                }

                return keyInfo;
            }
        }

        public enum ScrollType
        {
            Indent,
            Side,
        }
    }
}
