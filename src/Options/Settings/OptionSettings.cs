using B.Inputs;
using B.Options.Adventure;
using B.Options.BrainFuck;
using B.Options.MoneyTracker;
using B.Utils;

namespace B.Options.Settings
{
    public sealed class OptionSettings : Option
    {
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
        private Stage _stage = Stage.MainMenu;

        public override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        Util.ClearConsole(35, 11);
                        new Input.Option("Settings")
                            .Add(() => this._stage = Stage.Color, "Color", '1')
                            .Add(() => this._stage = Stage.WindowSize, "Window Size", '2')
                            .Add(() => this._stage = Stage.DeleteData, "Delete Data", '3')
                            .AddSpacer()
                            .Add(null!, $"Debug Mode - {Program.Settings.DebugMode}", key: ConsoleKey.F12)
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stage.WindowSize:
                    {
                        this._size = Program.WINDOW_SIZE_MAX is not null ?
                            Vector2.Clamp(this._size, Program.WINDOW_SIZE_MIN, Program.WINDOW_SIZE_MAX) :
                            Vector2.Max(this._size, Program.WINDOW_SIZE_MIN);
                        Util.ClearConsole(this._size);
                        Util.PrintLine($"Detected Max Size: {Program.WINDOW_SIZE_MAX}");
                        Util.Print($"Current Size: {this._size}");

                        new Input.Option()
                            .Add(() => this._size.x++, keyChar: '8', key: ConsoleKey.RightArrow)
                            .Add(() => this._size.x--, keyChar: '2', key: ConsoleKey.LeftArrow)
                            .Add(() => this._size.y++, keyChar: '6', key: ConsoleKey.DownArrow)
                            .Add(() => this._size.y--, keyChar: '4', key: ConsoleKey.UpArrow)
                            .Add(() => this._stage = Stage.MainMenu, key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stage.Color:
                    {
                        Util.ClearConsole(20, 8);
                        new Input.Option("Color")
                            .Add(() => this._stage = Stage.Color_Theme, "Themes", '1')
                            .Add(() => this._stage = Stage.Color_Custom, "Customize", '2')
                            .AddSpacer()
                            .Add(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stage.Color_Theme:
                    {
                        ColorTheme theme = this._colorThemes[Input.ScrollIndex];
                        Program.Settings.ColorBackground = theme.ColorBG;
                        Program.Settings.ColorText = theme.ColorText;
                        Program.Settings.UpdateColors();
                        Util.ClearConsole(32, this._colorThemes.Length + 8);
                        Util.PrintLine();
                        Util.PrintLine("  Color Themes");
                        Input.RequestScroll(this._colorThemes, theme => theme.Title, null,
                            new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this._stage = Stage.Color;
                            }, "Back", key: ConsoleKey.Escape));
                    }
                    break;

                case Stage.Color_Custom:
                    {
                        Program.Settings.UpdateColors();
                        Util.ClearConsole(32, 27);
                        Util.PrintLine();
                        Util.PrintLine("  Colors");
                        ConsoleColor[] colors = Enum.GetValues<ConsoleColor>();
                        Input.RequestScroll(colors,
                            color => color.ToString(),
                            null,
                            new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this._stage = Stage.Color;
                            }, "Exit", key: ConsoleKey.Escape),
                            new(() => Program.Settings.ColorBackground = colors[Input.ScrollIndex], "Set Background", '1'),
                            new(() => Program.Settings.ColorText = colors[Input.ScrollIndex], "Set Foreground", '2'));
                    }
                    break;

                case Stage.DeleteData:
                    {
                        Util.ClearConsole(20, 10);
                        new Input.Option("Delete Data") // TODO Adventure, BrainFuck, Money Tracker
                            .Add(() => this.AskDelete("Adventure", () => File.Delete(OptionAdventure.FilePath)), "Adventure", '1')
                            .Add(() => this.AskDelete("BrainFuck", () => Directory.Delete(OptionBrainFuck.DirectoryPath, true)), "BrainFuck", '2')
                            .Add(() => this.AskDelete("Money Tracker", () => Directory.Delete(OptionMoneyTracker.DirectoryPath, true)), "Money Tracker", '3')
                            .Add(() => this.AskDelete("Settings", () => File.Delete(ProgramSettings.Path)), "Settings", '4')
                            .AddSpacer()
                            .Add(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;
            }
        }

        private void AskDelete(string title, Action deleteAction)
        {
            Util.ClearConsole(30, 7);
            new Input.Option($"Delete {title} Data?")
                .Add(null!, "NO", key: ConsoleKey.Escape)
                .AddSpacer()
                .Add(deleteAction, "yes", key: ConsoleKey.Enter)
                .Request();
        }

        private enum Stage
        {
            MainMenu,
            WindowSize,
            Color,
            Color_Theme,
            Color_Custom,
            DeleteData,
        }
    }
}
