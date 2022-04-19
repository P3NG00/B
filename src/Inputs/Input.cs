using B.Utils;
using B.Utils.Extensions;
using B.Utils.Themes;

namespace B.Inputs
{
    public static class Input
    {
        #region Constants

        public const int DECIMAL_LENGTH = 27;

        #endregion



        #region Universal Variables

        public static int ScrollIndex = 0;
        public static string String = string.Empty;

        public static ConsoleKeyInfo LastInput { get; private set; } = default(ConsoleKeyInfo);

        #endregion



        #region Universal Properties

        public static int MaxEntries => Window.SizeMax.y - 21;

        public static int? Int => int.TryParse(Input.String, out int num) ? num : null;
        public static decimal? Decimal => decimal.TryParse(Input.String, out decimal num) ? num : null;

        #endregion



        #region Universal Methods

        public static void ResetString() => Input.String = string.Empty;

        public static ConsoleKeyInfo Get()
        {
            Input.LastInput = Console.ReadKey(true);

            if (Input.LastInput.Key == ConsoleKey.F12)
                Program.Settings.DebugMode.Toggle();

            return Input.LastInput;
        }

        public static void WaitFor(ConsoleKey key, bool silent = false)
        {
            if (!silent)
            {
                Cursor.x = 1;
                Cursor.y += 2;
                Window.Print($"Press {key} to continue...");
            }

            while (true)
                if (Input.Get().Key == key)
                    break;
        }

        public static void RequestLine(int maxLength = int.MaxValue, params Keybind[] keybinds)
        {
            Input.Choice choice = Input.Choice.Create();
            keybinds.ForEach(keybind => choice.AddKeybind(keybind));
            ConsoleKeyInfo keyInfo = choice.Request();

            if (keyInfo.Key == ConsoleKey.Backspace)
                Input.String = Input.String.Substring(0, Math.Max(0, Input.String.Length - 1));
            else if (keyInfo.Key == ConsoleKey.Delete)
                ResetString();
            else if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape && !char.IsControl(keyInfo.KeyChar) && Input.String.Length < maxLength)
                Input.String += keyInfo.KeyChar;
        }

        // This will reset cursor position for each entry printed.
        // Make sure this is called when the cursor is in the row (Cursor.y) you want it to begin printing.
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
            int maxEntriesAdjusted = Math.Min(MaxEntries, maxEntriesPerPage.HasValue ? maxEntriesPerPage.Value : itemCount);
            Input.Choice choice = Input.Choice.Create();
            ConsoleKeyInfo keyInfo;

            // If title exists, print it
            if (title != null)
            {
                Cursor.x = 2;
                Window.Print($" {title} ", PrintType.Title);
                Cursor.y += 2;
            }

            if (maxEntriesAdjusted > 0 && itemCount > 0)
            {
                // Get indexes of current page
                int startIndex = Input.ScrollIndex - (Input.ScrollIndex % maxEntriesAdjusted);
                int endIndex = Math.Min(startIndex + maxEntriesAdjusted, itemCount);

                // Scroll through current page
                for (int i = startIndex; i < endIndex; i++)
                {
                    Cursor.x = 1;
                    Window.Print(Input.ScrollIndex == i ? '>' : ' ');
                    T item = items.ElementAt(i);
                    string text = getText(item);
                    string output = string.Format("{0,-" + (text.Length + 1) + "}", text);
                    ConsoleColor? colorText = getTextColor is null ? null : getTextColor(item, i);
                    ConsoleColor? colorBG = getBackgroundColor is null ? null : getBackgroundColor(item, i);
                    Cursor.x = 3;
                    Window.Print(output, new ColorPair(colorText, colorBG));
                    Cursor.y++;
                }

                // If navigation keybinds...
                if (navigationKeybinds)
                {
                    // Add message
                    choice.AddText(new Text("Use Up/Down to navigate"));
                    // Scroll up
                    choice.AddKeybind(Keybind.Create(() => Input.ScrollIndex--, key: ConsoleKey.UpArrow));
                    // Scroll down
                    choice.AddKeybind(Keybind.Create(() => Input.ScrollIndex++, key: ConsoleKey.DownArrow));
                    // Scroll previous page
                    choice.AddKeybind(Keybind.Create(() =>
                    {
                        Input.ScrollIndex += maxEntriesAdjusted;
                        // Window is cleared here since it is only detected
                        // later if you crossed from the ends of the pages
                        Window.Clear();
                    }, key: ConsoleKey.RightArrow));
                    // Scroll next page
                    choice.AddKeybind(Keybind.Create(() =>
                    {
                        Input.ScrollIndex -= maxEntriesAdjusted;
                        Window.Clear();
                    }, key: ConsoleKey.LeftArrow));
                }

                // If extra keybinds added...
                if (extraKeybinds is not null && extraKeybinds.Length != 0)
                {
                    // Add spacer from navigation keybinds if added
                    if (navigationKeybinds)
                        choice.AddSpacer();

                    // Add extra keybinds
                    extraKeybinds.ForEach(keybind => choice.AddKeybind(keybind));
                }

                // If exit keybind...
                if (exitKeybind is not null)
                {
                    // Add spacer from previous output if keybind is meant to be displayed
                    if (exitKeybind.ToString() is not null)
                        choice.AddSpacer();

                    // Add exit keybind
                    choice.AddKeybind(exitKeybind);
                }

                // Get page index before it's modified
                int previousPageIndex = Input.ScrollIndex % maxEntriesAdjusted;

                // Request input
                Cursor.y++;
                keyInfo = choice.Request();

                // Fix index if it's out of bounds
                Input.ScrollIndex = Input.ScrollIndex.Clamp(0, itemCount - 1);

                // Get page indexes after it's modified
                int newPageIndex = Input.ScrollIndex % maxEntriesAdjusted;
                int oneLessThanMax = maxEntriesAdjusted - 1;

                // If scrolling into new page, clear console
                if ((previousPageIndex == oneLessThanMax && newPageIndex == 0) || (previousPageIndex == 0 && newPageIndex == oneLessThanMax))
                    Window.Clear();
            }
            else
            {
                Cursor.x = 2;
                Window.Print("No entries.");
                Cursor.y += 2;
                extraKeybinds.ForEach(keybind => choice.AddKeybind(keybind));
                choice.AddKeybind(exitKeybind);
                keyInfo = choice.Request();
            }

            return keyInfo;
        }

        #endregion



        #region Input Entry Choice

        private interface IEntry
        {
            void Print();
        }

        private sealed class EntryKeybind : IEntry
        {
            private readonly Keybind _keybind;

            public Keybind Keybind => _keybind;

            public EntryKeybind(Keybind keybind) => _keybind = keybind;

            public void Print() => Window.Print(_keybind.ToString());
        }

        private sealed class EntrySpacer : IEntry
        {
            public void Print() => Util.Void();
        }

        private sealed class EntryText : IEntry
        {
            private readonly Text _text;

            public EntryText(Text text) => _text = text;

            public void Print() => _text.Print();
        }

        public sealed class Choice
        {
            private readonly List<IEntry> _entries = new();

            private Choice(string? title = null)
            {
                if (string.IsNullOrWhiteSpace(title))
                    return;

                string titleText = $" {title} ";
                Add(new EntryText(new(titleText, PrintType.Title)));
                Add(new EntrySpacer());
            }

            private void Add(IEntry entry)
            {
                if (entry is null)
                    throw new Exception("Entry cannot be null!");

                _entries.Add(entry);
            }

            public void AddKeybind(Keybind keybind) => Add(new EntryKeybind(keybind));

            public void AddText(Text text) => Add(new EntryText(text));

            public void AddSpacer() => Add(new EntrySpacer());

            // This will reset cursor position for each entry printed.
            // Make sure this is called when the cursor is in the row (Cursor.y) you want it to begin printing.
            public ConsoleKeyInfo Request(Action? final = null)
            {
                // Print entries
                foreach (IEntry entry in _entries)
                {
                    if (entry is EntryKeybind entryKeybind && !entryKeybind.Keybind.Display)
                        continue;

                    Cursor.x = 2;
                    entry.Print();
                    Cursor.y++;
                }

                // Reset cursor
                Cursor.x = 0;

                // Do any last input
                if (final is not null)
                    final();

                // Wait for user input
                ConsoleKeyInfo keyInfo = Input.Get();

                // Activate function for pressed keybind
                foreach (IEntry entry in _entries)
                {
                    if (entry is EntryKeybind entryKeybind)
                    {
                        Keybind keybind = entryKeybind.Keybind;

                        if (keybind == keyInfo)
                        {
                            keybind.Action();
                            break;
                        }
                    }
                }

                return keyInfo;
            }

            public static Choice Create(string? title = null) => new(title);
        }

        #endregion
    }
}
