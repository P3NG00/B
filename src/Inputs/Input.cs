using B.Utils;
using B.Utils.Enums;
using B.Utils.Extensions;

namespace B.Inputs
{
    public static class Input
    {
        #region Constants

        // Maximum length to be input for decimals.
        public const int DECIMAL_LENGTH = 27;

        #endregion



        #region Universal Variables

        // The action to perform while processing input.
        // Usually set by a keybind the player interacts with.
        public static volatile Action? Action = null;
        // String being typed out when using Input.RequestLine().
        public static string String = string.Empty;
        // The index of scrolling when using Input.RequestScroll().
        public static int ScrollIndex = 0;

        #endregion



        #region Universal Properties

        // User string input as a decimal.
        // Null if input is not a decimal.
        public static decimal? Decimal => decimal.TryParse(String, out decimal num) ? num : null;
        // User string input as an integer.
        // Null if input is not an integer.
        public static int? Int => int.TryParse(String, out int num) ? num : null;
        // Max amount of entries to display in a scroll request.
        public static int MaxEntries => Window.SizeMax.y - 21;

        #endregion



        #region Universal Methods

        // Initializes Mouse and Keyboard input.
        public static void Initialize()
        {
            // Mouse needs to be initialized after Program.Settings has been initialized
            Mouse.Initialize();
            Keyboard.Initialize();
        }

        // Resets the user's input string.
        public static void ResetString() => String = string.Empty;

        // Waits for action to be set.
        // Usually by the user interacting with keybinds.
        // Invokes the action, resets.
        // Returns last pressed key.
        public static ConsoleKeyInfo Get()
        {
            Action = null;
            ProgramThread.TryUnlock();
            Util.WaitFor(() => Action is not null);
            ProgramThread.TryLock();
            Action!();
            return Keyboard.LastInput;
        }

        // Hangs the program until the given key is pressed by the user.
        public static void WaitFor(ConsoleKey key, bool silent = false)
        {
            if (!silent)
            {
                Cursor.NextLine(1, 2);
                Window.Print($"Press {key} to continue...");
            }

            Util.WaitFor(() => Get().Key == key);
        }

        // Sets current input action to a void function so that
        // input is skipped and the program continues to process.
        public static void SkipInput()
        {
            Input.Action = Util.Void;
            Keyboard.ResetInput();
        }

        // Allows the user to type out a string, one char at a time.
        public static void RequestLine(int maxLength = int.MaxValue, params Keybind[] keybinds)
        {
            Choice choice = new();
            keybinds.ForEach(keybind => choice.AddKeybind(keybind));
            ConsoleKeyInfo keyInfo = choice.Request();

            if (keyInfo == default)
                return;
            else if (keyInfo.Key == ConsoleKey.Backspace)
                String = String.Substring(0, Math.Max(0, String.Length - 1));
            else if (keyInfo.Key == ConsoleKey.Delete)
                ResetString();
            else if (!char.IsControl(keyInfo.KeyChar) && String.Length < maxLength)
                String += keyInfo.KeyChar;
        }

        // Allows the user to scroll through a list of items.
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
            Choice choice = new();
            ConsoleKeyInfo keyInfo;

            // If title exists, print it
            if (title != null)
            {
                Cursor.x = 2;
                Window.Print($" {title} ", PrintType.Title);
                Cursor.NextLine(lines: 2);
            }

            if (maxEntriesAdjusted > 0 && itemCount > 0)
            {
                // Get indexes of current page
                int startIndex = ScrollIndex - (ScrollIndex % maxEntriesAdjusted);
                int endIndex = Math.Min(startIndex + maxEntriesAdjusted, itemCount);

                // Scroll through current page
                for (int i = startIndex; i < endIndex; i++)
                {
                    // 'j' caches 'i' because it will change before being referenced later
                    int j = i;
                    Cursor.x = 1;
                    Window.Print(ScrollIndex == j ? '>' : ' ');
                    T item = items.ElementAt(j);
                    string text = getText(item);
                    ConsoleColor? colorText = getTextColor?.Invoke(item, j);
                    ConsoleColor? colorBG = getBackgroundColor?.Invoke(item, j);
                    Cursor.x = 3;
                    // Regiser each entry as a Keybind
                    Keybind.RegisterKeybind(Keybind.Create(() => ScrollIndex = j, text, colorPair: new(colorText, colorBG)));
                    Cursor.NextLine();
                }

                // If navigation keybinds...
                if (navigationKeybinds)
                {
                    choice.AddText(new Text("Use Up/Down to navigate"));
                    choice.AddKeybind(Keybind.Create(() => ScrollIndex--, key: ConsoleKey.UpArrow));
                    choice.AddKeybind(Keybind.Create(() => ScrollIndex++, key: ConsoleKey.DownArrow));
                    // Window is cleared in Left/Right keybinds because the page will always need to be re-printed.
                    choice.AddKeybind(Keybind.Create(() =>
                    {
                        ScrollIndex += maxEntriesAdjusted;
                        Window.Clear();
                    }, key: ConsoleKey.RightArrow));
                    choice.AddKeybind(Keybind.Create(() =>
                    {
                        ScrollIndex -= maxEntriesAdjusted;
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

                // Add exit keybind
                AddExitKeybind();
                // Get page index before it's modified
                int previousPageIndex = ScrollIndex % maxEntriesAdjusted;
                // Request input
                Cursor.NextLine();
                keyInfo = choice.Request();
                // Fix index if it's out of bounds
                ScrollIndex = ScrollIndex.Clamp(0, itemCount - 1);
                // Get page indexes after it's modified
                int newPageIndex = ScrollIndex % maxEntriesAdjusted;
                int oneLessThanMax = maxEntriesAdjusted - 1;
                // If scrolling into new page, clear console
                if ((previousPageIndex == oneLessThanMax && newPageIndex == 0) || (previousPageIndex == 0 && newPageIndex == oneLessThanMax))
                    Window.Clear();
            }
            else
            {
                Cursor.x = 2;
                Window.Print("No entries.");
                Cursor.NextLine(lines: 2);
                extraKeybinds.ForEach(keybind => choice.AddKeybind(keybind));
                AddExitKeybind();
                keyInfo = choice.Request();
            }

            return keyInfo;

            // Local functions
            void AddExitKeybind()
            {
                // If exit keybind...
                if (exitKeybind is not null)
                {
                    // Add spacer from previous output if keybind is meant to be displayed
                    if (exitKeybind.Display)
                        choice.AddSpacer();

                    // Add exit keybind
                    choice.AddKeybind(exitKeybind);
                }
            }
        }

        #endregion
    }
}
