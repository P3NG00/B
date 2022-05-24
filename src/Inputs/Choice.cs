using B.Utils;
using B.Utils.Enums;

namespace B.Inputs
{
    // Used to display a neat list of text and keybinds easily.
    public sealed class Choice
    {
        #region Private Variables

        // Added entries.
        private readonly List<IEntry> _entries = new();

        #endregion



        #region Constructors

        // Creates a new Choice with optional header.
        public Choice(string? header = null)
        {
            if (string.IsNullOrWhiteSpace(header))
                return;

            string titleText = $" {header} ";
            AddText(new(titleText, PrintType.Title));
            AddSpacer();
        }

        #endregion



        #region Private Methods

        // Adds entry to end of list.
        private void Add(IEntry entry)
        {
            if (entry is null)
                throw new Exception("Entry cannot be null!");

            _entries.Add(entry);
        }

        #endregion



        #region Public Methods

        // Adds an interactable keybind entry.
        public void AddKeybind(Keybind keybind) => Add(new EntryKeybind(keybind));

        // Adds a text entry to display.
        public void AddText(Text text) => Add(new EntryText(text));

        // Adds a space after the previous entry.
        public void AddSpacer() => Add(new EntrySpacer());

        // Displays organized text and keybinds to the user.
        // Entries are displayed in order they were added.
        // This will reset cursor position for each entry printed.
        // Make sure this is called when the cursor is in the row (Cursor.y) you want it to begin printing.
        public ConsoleKeyInfo Request(Action? final = null)
        {
            // Print entries
            foreach (var entry in _entries)
            {
                if (entry is EntryKeybind entryKeybind)
                {
                    Cursor.x = 2;
                    Keybind.RegisterKeybind(entryKeybind.Keybind);

                    if (!entryKeybind.Keybind.Display)
                        continue;
                }

                Cursor.x = 2;
                entry.Print();
                Cursor.NextLine();
            }

            // Do any last input
            if (final is not null)
                final();

            // Wait for user input
            ConsoleKeyInfo keyInfo = Input.Get();

            return keyInfo;
        }

        #endregion



        #region Choice Entries

        // Entries can override how to print themselves.
        private interface IEntry
        {
            public virtual void Print() { }
        }

        // Keybinds don't print and are registered the when choice is requested.
        private sealed class EntryKeybind : IEntry
        {
            public Keybind Keybind { get; private set; }

            public EntryKeybind(Keybind keybind) => Keybind = keybind;
        }

        // Spacer doesn't print and is used to separate entries.
        private sealed class EntrySpacer : IEntry { }

        // Text prints to the user.
        private sealed class EntryText : IEntry
        {
            public Text Text { get; private set; }

            public EntryText(Text text) => Text = text;

            public void Print() => Text.Print();
        }

        #endregion
    }
}
