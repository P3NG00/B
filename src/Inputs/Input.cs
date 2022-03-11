using B.Options;
using B.Utils;
using B.Utils.Extensions;

namespace B.Inputs
{
    static class Input
    {
        public const int DECIMAL_LENGTH = 27;

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

        // TODO use Cursor Position positioning
        public static ConsoleKeyInfo RequestScroll<T>(
            IEnumerable<T> items,                               // Items to scroll through
            Func<T, string> getText,                            // Function to get text from item
            Func<T, ConsoleColor?> getTextColor = null!,        // Function to get text color from item
            Func<T, ConsoleColor?> getBackgroundColor = null!,  // Function to get background color from item
            string title = null!,                               // Scroll title
            int? maxEntriesPerPage = null,                      // Max entries to display per page
            bool navigationKeybinds = true,                     // Whether to add scroll navigation keybinds
            Keybind exitKeybind = null!,                        // Exit keybind (seperate because spacer is added before this keybind)
            params Keybind[] extraKeybinds)                     // Extra keybinds
        {
            int itemCount = items.Count();
            int maxEntriesAdjusted = maxEntriesPerPage.HasValue ? maxEntriesPerPage.Value : itemCount;
            Input.Choice choice = Input.Choice.Create();
            ConsoleKeyInfo keyInfo;

            if (maxEntriesAdjusted > 0 && itemCount > 0)
            {
                if (title != null)
                {
                    Window.PrintLine();
                    Window.PrintLine($"  {title}");
                    Window.PrintLine();
                }

                int startIndex = Input.ScrollIndex - (Input.ScrollIndex % maxEntriesAdjusted);
                int endIndex = Math.Min(startIndex + maxEntriesAdjusted, itemCount);

                for (int i = startIndex; i < endIndex; i++)
                {
                    Window.Print(Input.ScrollIndex == i ? " > " : "   ");
                    T item = items.ElementAt(i);
                    string text = getText(item);
                    string output = string.Format("{0,-" + (text.Length + 1) + "}", text);
                    ConsoleColor? colorText = null;
                    ConsoleColor? colorBackground = null;

                    if (getTextColor != null)
                        colorText = getTextColor(item);

                    if (getBackgroundColor != null)
                        colorBackground = getBackgroundColor(item);

                    Window.Print(output, colorText: colorText, colorBG: colorBackground);
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
                {
                    choice.Add(() => Input.ScrollIndex--, key: ConsoleKey.UpArrow);
                    choice.Add(() => Input.ScrollIndex++, key: ConsoleKey.DownArrow);
                    choice.Add(() =>
                    {
                        Input.ScrollIndex += maxEntriesAdjusted;
                        Window.Clear();
                    }, key: ConsoleKey.RightArrow);
                    choice.Add(() =>
                    {
                        Input.ScrollIndex -= maxEntriesAdjusted;
                        Window.Clear();
                    }, key: ConsoleKey.LeftArrow);
                }

                if (hasExtraKeybinds)
                    choice.Add(extraKeybinds!);

                choice.AddSpacer();
                choice.Add(exitKeybind);
                keyInfo = choice.Request();
                Input.ScrollIndex = Input.ScrollIndex.Clamp(0, itemCount - 1);
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
                choice.Add(exitKeybind);
                keyInfo = choice.Request();
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
            private readonly List<Keybind> _keybinds = new();
            private readonly List<string> _messages = new();

            private Choice(string? title = null)
            {
                if (title is not null)
                    AddMessage(title);
            }

            public void AddMessage(string title) => _messages.Add(title);

            public void AddMessageSpacer() => _messages.Add(string.Empty);

            public void Add(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), bool control = false, bool shift = false, bool alt = false) => _keybinds.Add(new Keybind(action, description, keyChar, key, control, shift, alt));

            public void Add(params Keybind[] keybinds) => _keybinds.AddRange(keybinds);

            public void AddRoutine(Action<List<Keybind>> keybindRoutine)
            {
                List<Keybind> keybinds = new();
                keybindRoutine(keybinds);
                _keybinds.AddRange(keybinds);
            }

            public void AddSpacer() => _keybinds.AddRange(new Keybind[] { null! });

            public void AddExit(IOption option)
            {
                string phrase = string.Empty;

                switch (Program.CurrentLevel)
                {
                    case Program.Level.Program: phrase = "Quit"; break;
                    case Program.Level.Group: phrase = "Back"; break;
                    case Program.Level.Option: phrase = "Exit"; break;
                }

                Add(() => option.Quit(), phrase, key: ConsoleKey.Escape);
            }

            public ConsoleKeyInfo Request()
            {
                bool printLine = false;

                // Print messages before keybinds
                if (!_messages.IsEmpty())
                {
                    Window.PrintLine();
                    _messages.ForEach(msg => Window.PrintLine($"  {msg}"));
                    printLine = true;
                }

                // Print keybinds
                foreach (Keybind keybind in _keybinds)
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
                            if (keybind.HasModifier(ConsoleModifiers.Control)) preface += "Ctrl+";
                            if (keybind.HasModifier(ConsoleModifiers.Shift)) preface += "Shift+";
                            if (keybind.HasModifier(ConsoleModifiers.Alt)) preface += "Alt+";
                            Window.PrintLine($" {preface}{(keybind.KeyChar == null ? keybind.Key.ToString() : keybind.KeyChar.Value.ToString())}) {keybind.Description}");
                        }
                    }
                    else if (!printLine)
                        printLine = true;
                }

                ConsoleKeyInfo keyInfo = Input.Get();

                // Activate function for pressed keybind
                foreach (Keybind keybind in _keybinds)
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
                return Request();
            }

            public ConsoleKeyInfo Request((int x, int y) position)
            {
                Cursor.SetPosition(position);
                return Request();
            }

            public ConsoleKeyInfo Request(int x, int y)
            {
                Cursor.SetPosition(x, y);
                return Request();
            }

            public static Choice Create(string? title = null) => new(title);
        }
    }
}
