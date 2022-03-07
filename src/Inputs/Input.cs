using B.Options;
using B.Utils;

namespace B.Inputs
{
    static class Input
    {
        public static int ScrollIndex = 0;
        public static string String = string.Empty;

        public static int? Int => int.TryParse(Input.String, out int num) ? num : null;
        public static decimal? Decimal => decimal.TryParse(Input.String, out decimal num) ? num : null;

        public static ConsoleKeyInfo LastInput { get; private set; } = default(ConsoleKeyInfo);

        public static void ResetString() => Input.String = string.Empty;

        public static ConsoleKeyInfo Get()
        {
            Input.LastInput = Console.ReadKey(true);

            if (Input.LastInput.Key == ConsoleKey.F12)
            {
                Program.Settings.DebugMode.Toggle();
                // Toggling Debug mode clears console to avoid leftover characters
                Window.Clear();
            }

            return Input.LastInput;
        }

        public static void WaitFor(ConsoleKey key, bool silent = false)
        {
            if (!silent)
            {
                Window.PrintLine(); // TODO replace with Cursor Position positioning
                Window.PrintLine($"Press {key} to continue...");
            }

            bool wait = true;

            while (wait)
                if (Input.Get().Key == key)
                    wait = false;
        }

        public static void RequestLine(int maxLength = int.MaxValue, params Keybind[] keybinds)
        {
            ConsoleKeyInfo keyInfo = Input.Get();

            if (keyInfo.Key == ConsoleKey.Backspace)
                Input.String = Input.String.Substring(0, Math.Max(0, Input.String.Length - 1));
            else if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape && Input.String.Length < maxLength)
                Input.String += keyInfo.KeyChar;

            foreach (Keybind keybind in keybinds)
            {
                if (keybind == keyInfo)
                {
                    keybind.Action();
                    break;
                }
            }
        }

        public static ConsoleKeyInfo RequestScroll<T>(
            T[] items,                                          // Items to scroll through
            Func<T, string> getText,                            // Function to get text from item
            Func<T, ConsoleColor?> getTextColor = null!,        // Function to get text color from item
            Func<T, ConsoleColor?> getBackgroundColor = null!,  // Function to get background color from item
            string title = null!,                               // Scroll title
            int? maxEntriesPerPage = null,                      // Max entries to display per page
            bool navigationKeybinds = true,                     // Whether to add scroll navigation keybinds
            Keybind exitKeybind = null!,                        // Exit keybind (seperate because spacer is added before this keybind)
            params Keybind[] extraKeybinds)                     // Extra keybinds
        {
            int maxEntriesAdjusted = maxEntriesPerPage.HasValue ? maxEntriesPerPage.Value : items.Length;
            Input.Choice iob = new();
            ConsoleKeyInfo keyInfo;

            if (maxEntriesAdjusted > 0 && items.Length > 0)
            {
                if (title != null)
                {
                    Window.PrintLine();
                    Window.PrintLine($"  {title}");
                    Window.PrintLine();
                }

                int startIndex = Input.ScrollIndex - (Input.ScrollIndex % maxEntriesAdjusted);
                int endIndex = Math.Min(startIndex + maxEntriesAdjusted, items.Length);

                for (int i = startIndex; i < endIndex; i++)
                {
                    Window.Print(Input.ScrollIndex == i ? " > " : "   ");
                    T item = items[i];
                    string text = getText(item);
                    string output = string.Format("{0,-" + (text.Length + 1) + "}", text);
                    ConsoleColor? colorText = null;
                    ConsoleColor? colorBackground = null;

                    if (getTextColor != null)
                        colorText = getTextColor(item);

                    if (getBackgroundColor != null)
                        colorBackground = getBackgroundColor(item);

                    Window.Print(output, colorText, colorBackground);
                    Window.PrintLine();
                }

                if (navigationKeybinds)
                {
                    Window.PrintLine();
                    Window.PrintLine(" Use Up/Down to navigate.");
                }

                bool hasExtraKeybinds = extraKeybinds != null && extraKeybinds.Length != 0;

                if (hasExtraKeybinds)
                    Window.PrintLine();

                // Get page index before it's modified
                int lastPageIndex = Input.ScrollIndex % maxEntriesAdjusted;

                if (navigationKeybinds)
                    iob.Add(() => Input.ScrollIndex--, key: ConsoleKey.UpArrow)
                        .Add(() => Input.ScrollIndex++, key: ConsoleKey.DownArrow)
                        .Add(() =>
                        {
                            Input.ScrollIndex += maxEntriesAdjusted;
                            Window.Clear();
                        }, key: ConsoleKey.RightArrow)
                        .Add(() =>
                        {
                            Input.ScrollIndex -= maxEntriesAdjusted;
                            Window.Clear();
                        }, key: ConsoleKey.LeftArrow);

                if (hasExtraKeybinds)
                    iob.Add(extraKeybinds!);

                keyInfo = iob.AddSpacer()
                    .Add(exitKeybind)
                    .Request();

                Input.ScrollIndex = Input.ScrollIndex.Clamp(0, items.Length - 1);
                int newPageIndex = Input.ScrollIndex % maxEntriesAdjusted;
                int oneLessThanMax = maxEntriesAdjusted - 1;

                // If crossing into new page, clear console
                if ((lastPageIndex == oneLessThanMax && newPageIndex == 0) || (lastPageIndex == 0 && newPageIndex == oneLessThanMax))
                    Window.Clear();
            }
            else
            {
                Window.PrintLine();
                Window.PrintLine("  No entries.");
                Window.PrintLine();
                keyInfo = iob.Add(exitKeybind)
                    .Request();
            }

            return keyInfo;
        }

        public static ConsoleKeyInfo RequestScroll<T>(
            T[] items,                                          // Items to scroll through
            Vector2 position,                                   // Position to begin printing
            Func<T, string> getText,                            // Function to get text from item
            Func<T, ConsoleColor?> getTextColor = null!,        // Function to get text color from item
            Func<T, ConsoleColor?> getBackgroundColor = null!,  // Function to get background color from item
            string title = null!,                               // Scroll title
            int? maxEntriesPerPage = null,                      // Max entries to display per page
            bool navigationKeybinds = true,                     // Whether to add scroll navigation keybinds
            Keybind exitKeybind = null!,                        // Exit keybind (seperate because spacer is added before this keybind)
            params Keybind[] extraKeybinds)                     // Extra keybinds
        {
            Cursor.SetPosition(position);
            return Input.RequestScroll(items, getText, getTextColor, getBackgroundColor, title, maxEntriesPerPage, navigationKeybinds, exitKeybind, extraKeybinds);
        }

        public sealed class Choice
        {
            private readonly Utils.List<Keybind> _keybinds = new Utils.List<Keybind>();
            private readonly string? _message;

            public Choice(string? message = null) => this._message = message;

            public Choice Add(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), bool control = false, bool shift = false, bool alt = false)
            {
                this._keybinds.Add(new Keybind(action, description, keyChar, key, control, shift, alt));
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
                    Window.PrintLine();
                    Window.PrintLine($"  {this._message}");
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
                                Window.PrintLine();
                            }

                            string preface = string.Empty;
                            if (keybind.ModifierControl) preface += "Ctrl+";
                            if (keybind.ModifierShift) preface += "Shift+";
                            if (keybind.ModifierAlt) preface += "Alt+";
                            Window.PrintLine($" {preface}{(keybind.KeyChar == null ? keybind.Key.ToString() : keybind.KeyChar.Value.ToString())}) {keybind.Description}");
                        }
                    }
                    else if (!printLine)
                        printLine = true;
                }

                ConsoleKeyInfo keyInfo = Input.Get();

                // Activate function for pressed keybind
                foreach (Keybind keybind in this._keybinds)
                {
                    if (keybind is not null && keybind == keyInfo)
                    {
                        keybind.Action!.Invoke();
                        break;
                    }
                }

                return keyInfo;
            }

            public ConsoleKeyInfo Request(Vector2 position)
            {
                Cursor.SetPosition(position);
                return this.Request();
            }
        }
    }
}
