using B.Inputs;
using B.Options.Games.Adventure;
using B.Options.Tools.MoneyTracker;
using B.Options.Toys.BrainFuck;
using B.Utils;

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
                        Window.ClearAndSetSize(35, 13);
                        Input.CreateChoice(OptionSettings.Title)
                            .Add(() => SetStage(Stages.Color), "Color", '1')
                            .Add(() => SetStage(Stages.WindowSize), "Window Size", '2')
                            .Add(() => SetStage(Stages.KeyPress), "Key Press", '3')
                            .Add(() => SetStage(Stages.DeleteData), "Delete Data", '4')
                            .AddSpacer()
                            .Add(() => Program.Settings.Censor.Toggle(), $"Censor - {Program.Settings.Censor.Active}", key: ConsoleKey.F11)
                            .Add(null!, $"Debug Mode - {Program.Settings.DebugMode.Active}", key: ConsoleKey.F12)
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.WindowSize:
                    {
                        _size = _size.Clamp(Window.SIZE_MIN, Window.SIZE_MAX);
                        Window.ClearAndSetSize(_size);
                        Window.PrintLine($"Detected Max Size: {Window.SIZE_MAX}");
                        Window.Print($"Current Size: {_size}");

                        Input.CreateChoice()
                            .Add(() => _size.x++, keyChar: '8', key: ConsoleKey.RightArrow)
                            .Add(() => _size.x--, keyChar: '2', key: ConsoleKey.LeftArrow)
                            .Add(() => _size.y++, keyChar: '6', key: ConsoleKey.DownArrow)
                            .Add(() => _size.y--, keyChar: '4', key: ConsoleKey.UpArrow)
                            .Add(() => SetStage(Stages.MainMenu), key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Color:
                    {
                        Window.ClearAndSetSize(20, 8);
                        Input.CreateChoice("Color")
                            .Add(() => SetStage(Stages.Color_Theme), "Themes", '1')
                            .Add(() => SetStage(Stages.Color_Custom), "Customize", '2')
                            .AddSpacer()
                            .Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Color_Theme:
                    {
                        ColorTheme theme = _colorThemes[Input.ScrollIndex];
                        Program.Settings.ColorBackground = theme.ColorBG;
                        Program.Settings.ColorText = theme.ColorText;
                        Program.Settings.UpdateColors();
                        Window.ClearAndSetSize(32, _colorThemes.Length + 8);
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
                        Window.ClearAndSetSize(32, 27);
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
                        Window.ClearAndSetSize(20, 10);
                        Input.CreateChoice("Delete Data")
                            .Add(CreateDeleteKeybind("Adventure", () => File.Delete(OptionAdventure.FilePath), '1'))
                            .Add(CreateDeleteKeybind(OptionBrainFuck.Title, () => Directory.Delete(OptionBrainFuck.DirectoryPath, true), '2'))
                            .Add(CreateDeleteKeybind("Money Tracker", () => Directory.Delete(OptionMoneyTracker.DirectoryPath, true), '3'))
                            .Add(CreateDeleteKeybind("Settings", () => File.Delete(ProgramSettings.Path), '4'))
                            .AddSpacer()
                            .Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape)
                            .Request();

                        Keybind CreateDeleteKeybind(string title, Action deleteAction, char num) => new Keybind(() =>
                        {
                            Window.ClearAndSetSize(30, 7);
                            Input.CreateChoice($"Delete {title} Data?")
                                .Add(null!, "NO", key: ConsoleKey.Escape)
                                .AddSpacer()
                                .Add(deleteAction, "yes", key: ConsoleKey.Enter)
                                .Request();
                        }, title, num);
                    }
                    break;

                case Stages.KeyPress:
                    {
                        Window.ClearAndSetSize(35, 12);
                        Window.PrintLine();
                        Window.PrintLine(" Last Pressed");
                        Window.PrintLine();
                        ConsoleKeyInfo lastInput = Input.LastInput;
                        Window.PrintLine($" Key: {lastInput.Key}");
                        Window.PrintLine($" Key Num: {(int)lastInput.Key}");
                        Window.PrintLine($" Key Char: {lastInput.KeyChar.Unformat()}");
                        Window.PrintLine($" Key Char Num: {(int)lastInput.KeyChar}");
                        Window.PrintLine($" Modifiers: {lastInput.Modifiers}");
                        Window.PrintLine($" Control: {lastInput.Modifiers.HasFlag(ConsoleModifiers.Control)}");
                        Window.PrintLine($" Shift: {lastInput.Modifiers.HasFlag(ConsoleModifiers.Shift)}");
                        Window.PrintLine($" Alt: {lastInput.Modifiers.HasFlag(ConsoleModifiers.Alt)}");

                        if (Input.Get().Key == ConsoleKey.Escape)
                            SetStage(Stages.MainMenu);
                    }
                    break;
            }
        }

        private void AskDelete(string title, Action deleteAction)
        {
            Window.ClearAndSetSize(30, 7);
            Input.CreateChoice($"Delete {title} Data?")
                .Add(null!, "NO", key: ConsoleKey.Escape)
                .AddSpacer()
                .Add(deleteAction, "yes", key: ConsoleKey.Enter)
                .Request();
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
