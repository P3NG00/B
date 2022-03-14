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
        public const string Title = "Settings";

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
                        Window.SetSize(35, 13);
                        Input.Choice choice = Input.Choice.Create(OptionSettings.Title);
                        choice.Add(() => SetStage(Stages.Color), "Color", '1');
                        choice.Add(() => SetStage(Stages.WindowSize), "Window Size", '2');
                        choice.Add(() =>
                        {
                            Window.Clear();
                            SetStage(Stages.KeyPress);
                        }, "Key Press", '3');
                        choice.Add(() => SetStage(Stages.DeleteData), "Delete Data", '4');
                        choice.AddSpacer();
                        choice.Add(() => Program.Settings.Censor.Toggle(), $"Censor - {Program.Settings.Censor.Active}", key: ConsoleKey.F11);
                        choice.Add(null!, $"Debug Mode - {Program.Settings.DebugMode.Active}", key: ConsoleKey.F12);
                        choice.AddSpacer();
                        choice.AddExit(this);
                        choice.Request();
                    }
                    break;

                case Stages.WindowSize:
                    {
                        _size = _size.Clamp(Window.SIZE_MIN, Window.SIZE_MAX);
                        Window.Clear();
                        Window.SetSize(_size);
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
                        Input.RequestScroll(
                            items: _colorThemes,
                            getText: theme => theme.Title,
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
                        ConsoleColor[] colors = Util.OrderedConsoleColors;
                        Input.RequestScroll(
                            items: colors,
                            getText: c => $" {c.ToString(),-12}",
                            getTextColor: c => c,
                            getBackgroundColor: c => c == ConsoleColor.Black || c == ConsoleColor.Gray || c.ToString().StartsWith("Dark") ? ConsoleColor.White : ConsoleColor.Gray,
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

                case Stages.DeleteData:
                    {
                        Window.Clear();
                        Window.SetSize(20, 10);
                        Input.Choice choice = Input.Choice.Create("Delete Data");
                        choice.Add(CreateDeleteKeybind("Adventure", () => File.Delete(OptionAdventure.FilePath), '1'));
                        choice.Add(CreateDeleteKeybind(OptionBrainFuck.Title, () => Directory.Delete(OptionBrainFuck.DirectoryPath, true), '2'));
                        choice.Add(CreateDeleteKeybind("Money Tracker", () => Directory.Delete(OptionMoneyTracker.DirectoryPath, true), '3'));
                        choice.Add(CreateDeleteKeybind("Settings", () => File.Delete(ProgramSettings.Path), '4'));
                        choice.AddSpacer();
                        choice.Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape);
                        choice.Request();

                        Keybind CreateDeleteKeybind(string title, Action deleteAction, char num) => new Keybind(() =>
                        {
                            Window.Clear();
                            Window.SetSize(30, 7);
                            Input.Choice choice = Input.Choice.Create($"Delete {title} Data?");
                            choice.Add(null!, "NO", key: ConsoleKey.Escape);
                            choice.AddSpacer();
                            choice.Add(deleteAction, "yes", key: ConsoleKey.Enter);
                            choice.Request();
                        }, title, num);
                    }
                    break;

                case Stages.KeyPress:
                    {
                        Window.SetSize(35, 12);
                        Cursor.Position = new(1, 1);
                        Window.Print("Last Pressed");
                        ConsoleKeyInfo lastInput = Input.LastInput;
                        Print(3, "Key", lastInput.Key);
                        Print(4, "Key Num", (int)lastInput.Key);
                        Print(5, "Key Char", lastInput.KeyChar.Unformat());
                        Print(6, "Key Char Num", (int)lastInput.KeyChar);
                        Print(7, "Modifiers", lastInput.Modifiers);
                        Print(8, "Control", lastInput.IsControlHeld());
                        Print(9, "Shift", lastInput.IsShiftHeld());
                        Print(10, "Alt", lastInput.IsAltHeld());

                        void Print(int line, string title, object value)
                        {
                            Cursor.Position = new(1, line);
                            Window.Print($"{title}: {value,-10}");
                        }

                        if (Input.Get().Key == ConsoleKey.Escape)
                            SetStage(Stages.MainMenu);
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
            DeleteData,
            KeyPress,
        }
    }
}
