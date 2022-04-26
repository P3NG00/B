using B.Inputs;
using B.Options.Games.Adventure;
using B.Options.Tools.MoneyTracker;
using B.Options.Toys.BrainFuck;
using B.Utils;
using B.Utils.Extensions;
using B.Utils.Themes;

namespace B.Options.Tools.Settings
{
    public sealed class OptionSettings : Option<OptionSettings.Stages>
    {
        public static string Title => "Settings";

        private Vector2 _size = new(40, 20);

        public OptionSettings() : base(Stages.MainMenu) { }

        // TODO re-implement custom color selecting

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.Clear();
                        Window.SetSize(27, 14);
                        Cursor.y = 1;
                        var choice = Input.Choice.Create(OptionSettings.Title);
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Color_Theme), "Color", '1'));
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.WindowSize), "Window Size", '2'));
                        choice.AddKeybind(Keybind.Create(() => ClearSetStage(Stages.KeyPress), "Key Press", '3'));
                        choice.AddKeybind(Keybind.Create(() => ClearSetStage(Stages.Cursor), "Cursor", '4'));
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.DeleteData), "Delete Data", '5'));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(Program.Settings.Censor.Toggle, $"Censor - {Program.Settings.Censor.Active}", key: ConsoleKey.F10));
                        choice.AddKeybind(Keybind.Create(Program.Settings.DebugMode.Toggle, $"Debug Mode - {Program.Settings.DebugMode.Active}", key: ConsoleKey.F12));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        choice.Request();

                        // Local Method
                        void ClearSetStage(Stages stage)
                        {
                            Window.Clear();
                            SetStage(stage);
                        }
                    }
                    break;

                case Stages.WindowSize:
                    {
                        _size = _size.Clamp(Window.SizeMin, Window.SizeMax);
                        Window.Clear();
                        Window.SetSize(_size);
                        Cursor.Set(0, 0);
                        Window.Print($"Detected Max Size: {Window.SizeMax}");
                        Cursor.Set(0, 1);
                        Window.Print($"Current Size: {_size}");
                        Input.Choice choice = Input.Choice.Create();
                        choice.AddKeybind(Keybind.Create(() => _size.x++, keyChar: '8', key: ConsoleKey.RightArrow));
                        choice.AddKeybind(Keybind.Create(() => _size.x--, keyChar: '2', key: ConsoleKey.LeftArrow));
                        choice.AddKeybind(Keybind.Create(() => _size.y++, keyChar: '6', key: ConsoleKey.DownArrow));
                        choice.AddKeybind(Keybind.Create(() => _size.y--, keyChar: '4', key: ConsoleKey.UpArrow));
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.MainMenu), key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;

                // case Stages.Color:
                //     {
                //         Window.Clear();
                //         Window.SetSize(20, 8);
                //         Cursor.Set(0, 1);
                //         Input.Choice choice = Input.Choice.Create("Color");
                //         choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Color_Theme), "Themes", '1'));
                //         choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Color_Custom), "Customize", '2'));
                //         choice.AddSpacer();
                //         choice.AddKeybind(Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape));
                //         choice.Request();
                //     }
                //     break;

                case Stages.Color_Theme:
                    {
                        ColorTheme[] themes = Util.ColorThemes;
                        ColorTheme theme = themes[Input.ScrollIndex];
                        Program.Settings.ColorTheme = theme;
                        Program.Settings.UpdateColors();
                        Window.Clear();
                        Window.SetSize(28, themes.Length + 8);
                        Cursor.y = 1;
                        Input.RequestScroll(
                            items: themes,
                            getText: theme => theme.Title,
                            title: "Color Themes",
                            exitKeybind: Keybind.Create(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.MainMenu);
                            }, "Back", key: ConsoleKey.Escape));
                    }
                    break;

                // case Stages.Color_Custom:
                //     {
                //         Program.Settings.UpdateColors();
                //         Window.Clear();
                //         Window.SetSize(32, 27);
                //         ConsoleColor[] colors = Util.ConsoleColors;
                //         Cursor.y = 1;
                //         Input.RequestScroll(
                //             items: colors,
                //             getText: c => $" {c.ToString(),-12}",
                //             getTextColor: (c, i) => c,
                //             getBackgroundColor: (c, i) => (c == ConsoleColor.Black || c == ConsoleColor.Gray || c.ToString().StartsWith("Dark")) ? ConsoleColor.White : ConsoleColor.Gray,
                //             title: "Colors",
                //             exitKeybind: Keybind.Create(() =>
                //             {
                //                 Input.ScrollIndex = 0;
                //                 SetStage(Stages.Color);
                //             }, "Exit", key: ConsoleKey.Escape));
                //         // extraKeybinds: new Keybind[] {
                //         // Keybind.Create(() => Program.Settings.ColorBackground = colors[Input.ScrollIndex], "Set Background", '1'),
                //         // Keybind.Create(() => Program.Settings.ColorText = colors[Input.ScrollIndex], "Set Foreground", '2')});
                //     }
                //     break;

                case Stages.Cursor:
                    {
                        Window.SetSize(25, 8);
                        Cursor.Reset();
                        Cursor.Set(0, 1);
                        Input.Choice choice = Input.Choice.Create("Cursor");
                        choice.AddKeybind(Keybind.Create(Program.Settings.CursorVisible.Toggle, $"Visibility - {Cursor.Visible,-5}", '1'));
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            Window.Clear();
                            SetStage(Stages.Cursor_Size);
                        }, "Size", '2'));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;

                case Stages.Cursor_Size:
                    {
                        Window.SetSize(23, 10);
                        Cursor.Set(0, 1);
                        Input.Choice choice = Input.Choice.Create($"Cursor Size - {Cursor.Size,-3}");
                        choice.AddKeybind(Keybind.Create(() => Cursor.Size++, "+1", key: ConsoleKey.UpArrow));
                        choice.AddKeybind(Keybind.Create(() => Cursor.Size--, "-1", key: ConsoleKey.DownArrow));
                        choice.AddKeybind(Keybind.Create(() => Cursor.Size += 10, "+10", key: ConsoleKey.RightArrow));
                        choice.AddKeybind(Keybind.Create(() => Cursor.Size -= 10, "-10", key: ConsoleKey.LeftArrow));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            Window.Clear();
                            SetStage(Stages.Cursor);
                        }, "Back", key: ConsoleKey.Escape));
                        choice.Request(() =>
                        {
                            // Show cursor for changing
                            Cursor.Visible = true;
                            // Position cursor near open area in bottom-right corner
                            Cursor.Set(20, 8);
                        });
                        // Update cursor after request
                        Program.Settings.UpdateCursor();
                    }
                    break;

                case Stages.KeyPress:
                    {
                        Window.SetSize(35, 26);
                        ConsoleKeyInfo lastInput = Keyboard.LastInput;
                        ConsoleKey key = lastInput.Key;
                        char c = lastInput.KeyChar;
                        Cursor.Set(2, 1);
                        Window.Print(" Last Key Pressed ", PrintType.Title);
                        //    2
                        Print(3, "Key Char Num", (int)c);
                        Print(4, "Key Char", c.Unformat());
                        Print(5, "Key Num", (int)key);
                        Print(6, "Key", key);
                        //    7
                        Print(8, "Control", lastInput.IsControlHeld());
                        Print(9, "Shift", lastInput.IsShiftHeld());
                        Print(10, "Alt", lastInput.IsAltHeld());
                        //    11
                        Print(12, "Punctuation", char.IsPunctuation(c));
                        Print(13, "Whitespace", char.IsWhiteSpace(c));
                        Print(14, "Seperator", char.IsSeparator(c));
                        Print(15, "Control", char.IsControl(c));
                        Print(16, "Symbol", char.IsSymbol(c));
                        Print(17, "Number", char.IsNumber(c));
                        Print(18, "Letter", char.IsLetter(c));
                        Print(19, "Lower", char.IsLower(c));
                        Print(20, "Upper", char.IsUpper(c));
                        Print(21, "Digit", char.IsDigit(c));
                        Print(22, "Ascii", char.IsAscii(c));
                        //    23
                        Cursor.y = 24;
                        Keybind escapeKeybind = Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape, modifiers: ConsoleModifiers.Shift);
                        Input.RequestLine(keybinds: escapeKeybind);

                        void Print(int line, string title, object value)
                        {
                            string? output;

                            // Get output from value.
                            // Make false bools appear empty.
                            if (value is bool b && !b)
                                output = string.Empty;
                            else
                                output = value.ToString();

                            Cursor.x = 2;
                            Cursor.y = line;
                            Window.Print($"{title}: {output,-12}");
                        }
                    }
                    break;

                case Stages.DeleteData:
                    {
                        Window.Clear();
                        Window.SetSize(20, 10);
                        Cursor.Set(0, 1);
                        Input.Choice choice = Input.Choice.Create("Delete Data");
                        choice.AddKeybind(CreateDeleteKeybind(() => File.Delete(OptionAdventure.FilePath), OptionAdventure.Title, '1'));
                        choice.AddKeybind(CreateDeleteKeybind(() => Directory.Delete(OptionBrainFuck.DirectoryPath, true), OptionBrainFuck.Title, '2'));
                        choice.AddKeybind(CreateDeleteKeybind(() => Directory.Delete(OptionMoneyTracker.DirectoryPath, true), OptionMoneyTracker.Title, '3'));
                        choice.AddKeybind(CreateDeleteKeybind(() => File.Delete(ProgramSettings.Path), OptionSettings.Title, '4'));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape));
                        choice.Request();

                        Keybind CreateDeleteKeybind(Action deleteAction, string title, char num) => Keybind.CreateConfirmation(deleteAction, $"Delete saved data for {title}?", title, num);
                    }
                    break;
            }
        }

        public enum Stages
        {
            MainMenu,
            WindowSize,
            // Color,
            Color_Theme,
            // Color_Custom,
            Cursor,
            Cursor_Size,
            KeyPress,
            DeleteData,
        }
    }
}
