using B.Options;
using B.Utils;

namespace B.Inputs
{
    public sealed class Keybind
    {
        #region Public Properties

        public readonly ConsoleKey? Key;
        public readonly char? KeyChar;
        public readonly string Description;
        public readonly Action Action;

        public bool Display => !string.IsNullOrWhiteSpace(Description);

        #endregion



        #region Private Properties

        private readonly ConsoleModifiers _modifiers;

        #endregion



        #region Constructor

        private Keybind(Action? action = null, string? description = null, char? keyChar = null, ConsoleKey? key = null, ConsoleModifiers? modifiers = null)
        {
            if (action is null)
                action = Util.Void;

            KeyChar = keyChar;
            Key = key;

            if (description is null)
                Description = string.Empty;
            else
                Description = description;

            Action = action;

            if (modifiers.HasValue)
                _modifiers = modifiers.Value;
            else
                _modifiers = default;
        }

        #endregion



        #region Public Methods

        public bool HasModifier(ConsoleModifiers modifier) => _modifiers.HasFlag(modifier);

        public override bool Equals(object? obj)
        {
            if (obj is Keybind keybind)
                return this == keybind;
            else if (obj is ConsoleKeyInfo keyInfo)
                return this == keyInfo;

            return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            string preface = string.Empty;
            if (HasModifier(ConsoleModifiers.Control)) preface += "Ctrl+";
            if (HasModifier(ConsoleModifiers.Shift)) preface += "Shift+";
            if (HasModifier(ConsoleModifiers.Alt)) preface += "Alt+";
            return $"{preface}{(KeyChar.HasValue ? KeyChar.Value.ToString() : Key.ToString())}) {Description}";
        }

        #endregion



        #region Universal Methods

        public static Keybind Create(Action? action = null, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), ConsoleModifiers? modifiers = null)
        {
            Keybind keybind = new Keybind(action, description, keyChar, key, modifiers);
            return keybind;
        }

        public static Keybind CreateOptionExit(IOption option)
        {
            string phrase = string.Empty;

            switch (Program.Instance.Stage)
            {
                case Program.Levels.Program: phrase = "Quit"; break;
                case Program.Levels.Group: phrase = "Back"; break;
                case Program.Levels.Option: phrase = "Exit"; break;
                default: throw new Exception($"Invalid Program Stage: {Program.Instance.Stage}");
            }

            Action exitAction = () => option.Quit();
            Keybind optionExitKeybind = new Keybind(exitAction, phrase, key: ConsoleKey.Escape);
            return optionExitKeybind;
        }

        public static Keybind CreateConfirmation(Action action, string message, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), ConsoleModifiers? modifiers = null)
        {
            Action confirmationAction = () =>
            {
                Window.Clear();
                Window.Size = new(message.Length + 4, 7);
                Cursor.Position = new(2, 1);
                Input.Choice choice = Input.Choice.Create(message);
                choice.AddKeybind(Keybind.Create(description: "NO", key: ConsoleKey.Escape));
                choice.AddSpacer();
                choice.AddKeybind(Keybind.Create(action, "yes", key: ConsoleKey.Enter));
                choice.Request();
                // Clear window after confirmation
                Window.Clear();
            };

            Keybind confirmationKeybind = new Keybind(confirmationAction, description, keyChar, key, modifiers);
            return confirmationKeybind;
        }

        [Obsolete] // TODO remove function
        public static Keybind CreateMessage(string message)
        {
            Keybind messageKeybind = new Keybind(description: message);
            return messageKeybind;
        }

        [Obsolete] // TODO remove function
        public static Keybind CreateSpacerKeybind()
        {
            Keybind spacerKeybind = new Keybind(description: string.Empty);
            return spacerKeybind;
        }

        #endregion



        #region Operator Overrides

        public static bool operator ==(Keybind keybind, ConsoleKeyInfo keyInfo)
        {
            bool isKey = keybind.Key == keyInfo.Key;
            bool isKeyChar;
            char? keychar = keybind.KeyChar;

            if (keychar.HasValue)
                isKeyChar = keychar.Value == keyInfo.KeyChar;
            else
                isKeyChar = false;

            bool keyMatches = isKey || isKeyChar;

            if (!keyMatches)
                return false;

            return keybind._modifiers.Equals(keyInfo.Modifiers);
        }

        public static bool operator !=(Keybind keybind, ConsoleKeyInfo keyInfo) => !(keybind == keyInfo);

        #endregion
    }
}
