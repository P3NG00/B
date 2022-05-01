using B.Utils;
using B.Utils.Enums;

namespace B.Inputs
{
    public sealed class Choice
    {
        #region Private Variables

        private readonly List<IEntry> _entries = new();

        #endregion



        #region Constructors

        public Choice(string? title = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return;

            string titleText = $" {title} ";
            AddText(new(titleText, PrintType.Title));
            AddSpacer();
        }

        #endregion



        #region Private Methods

        private void Add(IEntry entry)
        {
            if (entry is null)
                throw new Exception("Entry cannot be null!");

            _entries.Add(entry);
        }

        #endregion



        #region Public Methods

        public void AddKeybind(Keybind keybind) => Add(new EntryKeybind(keybind));

        public void AddText(Text text) => Add(new EntryText(text));

        public void AddSpacer() => Add(new EntrySpacer());

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
                Cursor.y++;
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

        private interface IEntry
        {
            public virtual void Print() => Util.Void();
        }

        private sealed class EntryKeybind : IEntry
        {
            public Keybind Keybind { get; private set; }

            public EntryKeybind(Keybind keybind) => Keybind = keybind;
        }

        private sealed class EntrySpacer : IEntry { }

        private sealed class EntryText : IEntry
        {
            public Text Text { get; private set; }

            public EntryText(Text text) => Text = text;

            public void Print() => Text.Print();
        }

        #endregion
    }
}
