using B.Inputs;
using B.Options.Games.Adventure;
using B.Options.Tools.MoneyTracker;
using B.Options.Toys.BrainFuck;
using B.Utils;
using B.Utils.Extensions;

namespace B.Options.Tools.Settings
{
    public sealed class OptionSettings : Option<OptionSettings.Stages>
    {
        public static string Title => "Settings";

        private readonly ColorTheme[] _colorThemes =
        {
            new("Default", ConsoleColor.Black, ConsoleColor.White),
            new("Light", ConsoleColor.DarkGray, ConsoleColor.White),
            new("Gentle", ConsoleColor.DarkCyan, ConsoleColor.White),
            new("Sky", ConsoleColor.White, ConsoleColor.DarkCyan),
            new("Sunshine", ConsoleColor.DarkYellow, ConsoleColor.Yellow),
            new("Starlight", ConsoleColor.Yellow, ConsoleColor.Black),
            new("Purple Banana", ConsoleColor.Yellow, ConsoleColor.DarkMagenta),
            new("Salmon", ConsoleColor.Yellow, ConsoleColor.Red),
            new("Console", ConsoleColor.White, ConsoleColor.Black),
            new("Creeper", ConsoleColor.Black, ConsoleColor.Green),
            new("Hacker", ConsoleColor.Green, ConsoleColor.Black),
        };

        private Vector2 _size = new(40, 20);

        public OptionSettings() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.Clear();
                        bool windows = OperatingSystem.IsWindows();
                        Window.SetSize(35, windows ? 14 : 13);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create(OptionSettings.Title);
                        choice.Add(() => SetStage(Stages.Color), "Color", '1');
                        choice.Add(() => SetStage(Stages.WindowSize), "Window Size", '2');
                        choice.Add(() =>
                        {
                            Window.Clear();
                            SetStage(Stages.KeyPress);
                        }, "Key Press", '3');
                        choice.Add(() =>
                        {
                            Window.Clear();
                            SetStage(Stages.Cursor);
                        }, "Cursor", '4');
                        choice.Add(() => SetStage(Stages.DeleteData), "Delete Data", '5');
                        choice.AddSpacer();
                        choice.Add(() => Program.Settings.Censor.Toggle(), $"Censor - {Program.Settings.Censor.Active}", key: ConsoleKey.F10);
                        // Action is null because F12 will toggle Debug Mode in Program using LastInput
                        choice.Add(Util.Void, $"Debug Mode - {Program.Settings.DebugMode.Active}", key: ConsoleKey.F12);
                        choice.AddSpacer();
                        choice.AddExit(this);
                        choice.Request();
                    }
                    break;

                case Stages.WindowSize:
                    {
                        _size = _size.Clamp(Window.SIZE_MIN, Window.SIZE_MAX);
                        Window.Clear();
                        Window.Size = _size;
                        Cursor.Position = new(0, 0);
                        Window.Print($"Detected Max Size: {Window.SIZE_MAX}");
                        Cursor.Position = new(0, 1);
                        Window.Print($"Current Size: {_size}");
                        Input.Choice choice = Input.Choice.Create();
                        choice.Add(() => _size.x++, keyChar: '8', key: ConsoleKey.RightArrow);
                        choice.Add(() => _size.x--, keyChar: '2', key: ConsoleKey.LeftArrow);
                        choice.Add(() => _size.y++, keyChar: '6', key: ConsoleKey.DownArrow);
                        choice.Add(() => _size.y--, keyChar: '4', key: ConsoleKey.UpArrow);
                        choice.Add(() => SetStage(Stages.MainMenu), key: ConsoleKey.Escape);
                        choice.Request();
                    }
                    break;

                case Stages.Color:
                    {
                        Window.Clear();
                        Window.SetSize(20, 8);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create("Color");
                        choice.Add(() => SetStage(Stages.Color_Theme), "Themes", '1');
                        choice.Add(() => SetStage(Stages.Color_Custom), "Customize", '2');
                        choice.AddSpacer();
                        choice.Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape);
                        choice.Request();
                    }
                    break;

                case Stages.Color_Theme:
                    {
                        ColorTheme theme = _colorThemes[Input.ScrollIndex];
                        Program.Settings.ColorBackground = theme.ColorBG;
                        Program.Settings.ColorText = theme.ColorText;
                        Program.Settings.UpdateColors();
                        Window.Clear();
                        Window.SetSize(32, _colorThemes.Length + 8);
                        Cursor.Position = new(2, 1);
                        Input.RequestScroll(
                            items: _colorThemes,
                            getText: theme => theme.Title,
                            // getText: theme => $" {theme.Title,-15}",
                            // getBackgroundColor: (c, i) => i % 2 == 0 ? ConsoleColor.White : ConsoleColor.Gray, // TODO implement once more complex themes are create with secondary/tertiary colors
                            title: "Color Themes",
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.Color);
                            }, "Back", key: ConsoleKey.Escape));
                    }
                    break;

                case Stages.Color_Custom:
                    {
                        Program.Settings.UpdateColors();
                        Window.Clear();
                        Window.SetSize(32, 27);
                        ConsoleColor[] colors = Util.ConsoleColors;
                        Cursor.Position = new(2, 1);
                        Input.RequestScroll(
                            items: colors,
                            getText: c => $" {c.ToString(),-12}",
                            getTextColor: (c, i) => c,
                            getBackgroundColor: (c, i) => c == ConsoleColor.Black || c == ConsoleColor.Gray || c.ToString().StartsWith("Dark") ? ConsoleColor.White : ConsoleColor.Gray,
                            title: "Colors",
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.Color);
                            }, "Exit", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => Program.Settings.ColorBackground = colors[Input.ScrollIndex], "Set Background", '1'),
                                new(() => Program.Settings.ColorText = colors[Input.ScrollIndex], "Set Foreground", '2')});
                    }
                    break;

                case Stages.Cursor:
                    {
                        Window.SetSize(23, 8);
                        Cursor.Reset();
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create("Cursor");
                        choice.Add(() =>
                        {
                            Program.Settings.CursorVisible.Toggle();
                            Program.Settings.UpdateCursor();
                        }, $"Visibility - {Cursor.Visible,-5}", '1');
                        choice.Add(() =>
                        {
                            Window.Clear();
                            SetStage(Stages.Cursor_Size);
                        }, "Size", '2');
                        choice.AddSpacer();
                        choice.Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape);
                        choice.Request();
                    }
                    break;

                case Stages.Cursor_Size:
                    {
                        Window.SetSize(21, 10);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create($"Cursor Size - {Cursor.Size,-3}");
                        choice.Add(() => Cursor.Size++, "+1", key: ConsoleKey.UpArrow);
                        choice.Add(() => Cursor.Size--, "-1", key: ConsoleKey.DownArrow);
                        choice.Add(() => Cursor.Size += 10, "+10", key: ConsoleKey.RightArrow);
                        choice.Add(() => Cursor.Size -= 10, "-10", key: ConsoleKey.LeftArrow);
                        choice.AddSpacer();
                        choice.Add(() =>
                        {
                            Window.Clear();
                            SetStage(Stages.Cursor);
                        }, "Back", key: ConsoleKey.Escape);
                        choice.Request(() =>
                        {
                            // Show cursor for changing
                            Cursor.Visible = true;
                            // Position cursor near open area in bottom-right corner
                            Cursor.Position = new(18, 8);
                        });
                        Program.Settings.UpdateCursor();
                    }
                    break;

                case Stages.KeyPress:
                    {
                        Window.SetSize(35, 11);
                        Cursor.Position = new(1, 1);
                        Window.Print("Last Pressed");
                        ConsoleKeyInfo lastInput = Input.LastInput;
                        Print(3, "Key", lastInput.Key);
                        Print(4, "Key Num", (int)lastInput.Key);
                        Print(5, "Key Char", lastInput.KeyChar.Unformat());
                        Print(6, "Key Char Num", (int)lastInput.KeyChar);
                        Print(7, "Control", lastInput.IsControlHeld());
                        Print(8, "Shift", lastInput.IsShiftHeld());
                        Print(9, "Alt", lastInput.IsAltHeld());

                        void Print(int line, string title, object value)
                        {
                            Cursor.Position = new(1, line);
                            Window.Print($"{title}: {value,-10}");
                        }

                        if (Input.Get().Key == ConsoleKey.Escape)
                            SetStage(Stages.MainMenu);
                    }
                    break;

                case Stages.DeleteData:
                    {
                        Window.Clear();
                        Window.SetSize(20, 10);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create("Delete Data");
                        choice.Add(CreateDeleteKeybind(() => File.Delete(OptionAdventure.FilePath), OptionAdventure.Title, '1'));
                        choice.Add(CreateDeleteKeybind(() => Directory.Delete(OptionBrainFuck.DirectoryPath, true), OptionBrainFuck.Title, '2'));
                        choice.Add(CreateDeleteKeybind(() => Directory.Delete(OptionMoneyTracker.DirectoryPath, true), OptionMoneyTracker.Title, '3'));
                        choice.Add(CreateDeleteKeybind(() => File.Delete(ProgramSettings.Path), OptionSettings.Title, '4'));
                        choice.AddSpacer();
                        choice.Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape);
                        choice.Request();

                        Keybind CreateDeleteKeybind(Action deleteAction, string title, char num) => Keybind.CreateConfirmationKeybind(deleteAction, $"Delete saved data for {title}?", title, num);
                    }
                    break;
            }
        }

        private void AskDelete(string title, Action deleteAction)
        {
            Window.Clear();
            Window.SetSize(30, 7);
            Input.Choice choice = Input.Choice.Create($"Delete {title} Data?");
            choice.Add(null!, "NO", key: ConsoleKey.Escape);
            choice.AddSpacer();
            choice.Add(deleteAction, "yes", key: ConsoleKey.Enter);
            choice.Request();
        }

        public enum Stages
        {
            MainMenu,
            WindowSize,
            Color,
            Color_Theme,
            Color_Custom,
            Cursor,
            Cursor_Size,
            KeyPress,
            DeleteData,
        }
    }
}
