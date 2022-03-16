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
                Cursor.x = 0;
                Cursor.y += 2;
                Window.Print($"Press {key} to continue...");
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

        // This function will start printing the title where the cursor is left.
        // Make sure the cursor position is correct before calling this function.
        public static ConsoleKeyInfo RequestScroll<T>(
            IEnumerable<T> items,                                   // Items to scroll through
            Func<T, string> getText,                                // Function to get text from item
            Func<T, int, ConsoleColor?> getTextColor = null!,       // Function to get text color from item
            Func<T, int, ConsoleColor?> getBackgroundColor = null!, // Function to get background color from item
            string title = null!,                                   // Scroll title
            int? maxEntriesPerPage = null,                          // Max entries to display per page
            bool navigationKeybinds = true,                         // Whether to add scroll navigation keybinds
            Keybind exitKeybind = null!,                            // Exit keybind (seperate because spacer is added before this keybind)
            params Keybind[] extraKeybinds)                         // Extra keybinds
        {
            int itemCount = items.Count();
            int maxEntriesAdjusted = maxEntriesPerPage.HasValue ? maxEntriesPerPage.Value : itemCount;
            Input.Choice choice = Input.Choice.Create();
            ConsoleKeyInfo keyInfo;

            if (maxEntriesAdjusted > 0 && itemCount > 0)
            {
                if (title != null)
                {
                    Window.Print(title);
                    Cursor.y += 2;
                }

                int startIndex = Input.ScrollIndex - (Input.ScrollIndex % maxEntriesAdjusted);
                int endIndex = Math.Min(startIndex + maxEntriesAdjusted, itemCount);

                for (int i = startIndex; i < endIndex; i++)
                {
                    Cursor.x = 1;
                    Window.Print(Input.ScrollIndex == i ? '>' : ' ');
                    T item = items.ElementAt(i);
                    string text = getText(item);
                    string output = string.Format("{0,-" + (text.Length + 1) + "}", text);
                    ConsoleColor? colorText = null;
                    ConsoleColor? colorBackground = null;

                    if (getTextColor != null)
                        colorText = getTextColor(item, i);

                    if (getBackgroundColor != null)
                        colorBackground = getBackgroundColor(item, i);

                    Cursor.x = 3;
                    Window.Print(output, colorText, colorBackground);
                    Cursor.y++;
                }

                if (navigationKeybinds)
                {
                    choice.AddMessage("Use Up/Down to navigate");
                    // Scroll up and down
                    choice.Add(() => Input.ScrollIndex--, key: ConsoleKey.UpArrow);
                    choice.Add(() => Input.ScrollIndex++, key: ConsoleKey.DownArrow);
                    // Scroll next/previous page (automatically clears window)
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

                if (extraKeybinds != null && extraKeybinds.Length != 0)
                {
                    if (navigationKeybinds)
                        choice.AddSpacer();

                    choice.Add(extraKeybinds!);
                }

                if (exitKeybind is not null)
                {
                    if (exitKeybind.ToString() is not null)
                        choice.AddSpacer();

                    choice.Add(exitKeybind);
                }

                // Get page index before it's modified
                int previousPageIndex = Input.ScrollIndex % maxEntriesAdjusted;

                // Request input
                Cursor.y++;
                keyInfo = choice.Request();

                // Adjust index
                Input.ScrollIndex = Input.ScrollIndex.Clamp(0, itemCount - 1);

                // If scrolling into new page, clear console
                int newPageIndex = Input.ScrollIndex % maxEntriesAdjusted;
                int oneLessThanMax = maxEntriesAdjusted - 1;

                if ((previousPageIndex == oneLessThanMax && newPageIndex == 0) || (previousPageIndex == 0 && newPageIndex == oneLessThanMax))
                    Window.Clear();
            }
            else
            {
                Window.Print("No entries.");
                choice.Add(exitKeybind);
                keyInfo = choice.Request();
            }

            return keyInfo;
        }

        public sealed class Choice
        {
            private readonly List<Keybind> _keybinds = new();

            private Choice(string? title = null)
            {
                if (!string.IsNullOrWhiteSpace(title))
                {
                    AddMessage(title);
                    AddSpacer();
                }
            }

            // TODO create 'AddConfirmation' method to add universal confirmation method for things like
            // deleting files in OptionFTP
            // removing accounts in OptionMoneyTracker
            // starting a new game in OptionAdventure
            // deleting data in OptionSettings

            public void AddMessage(string message) => _keybinds.Add(new(null!, message));

            public void AddSpacer() => _keybinds.Add(new(null!, string.Empty));

            public void Add(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), bool control = false, bool shift = false, bool alt = false) => _keybinds.Add(new Keybind(action, description, keyChar, key, control, shift, alt));

            public void Add(params Keybind[] keybinds) => _keybinds.AddRange(keybinds);

            public void AddRoutine(Action<List<Keybind>> keybindRoutine)
            {
                List<Keybind> keybinds = new();
                keybindRoutine(keybinds);
                _keybinds.AddRange(keybinds);
            }

            public void AddExit(IOption option)
            {
                string phrase = string.Empty;

                switch (Program.CurrentLevel)
                {
                    case Program.Levels.Program: phrase = "Quit"; break;
                    case Program.Levels.Group: phrase = "Back"; break;
                    case Program.Levels.Option: phrase = "Exit"; break;
                }

                Add(() => option.Quit(), phrase, key: ConsoleKey.Escape);
            }

            // This will reset cursor position for each keybind printed.
            // Make sure this is called when the cursor is in the row (Cursor.y) you want it to begin printing.
            public ConsoleKeyInfo Request(Action? final = null)
            {
                // Print keybinds
                _keybinds.ForEach(keybind =>
                {
                    // If keybind is not null, display it
                    if (keybind != null)
                    {
                        Cursor.x = keybind.Action is null ? 2 : 1;
                        string kStr = keybind.ToString();

                        // Handle keybind display
                        if (kStr is not null)
                        {
                            // Print
                            Window.Print(kStr);

                            // Go to next line
                            Cursor.y++;
                        }
                    }
                });

                // Reset cursor
                Cursor.x = 0;

                // Do any last input
                if (final is not null)
                    final();

                // Wait for user input
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

            public static Choice Create(string? title = null) => new(title);
        }
    }
}
