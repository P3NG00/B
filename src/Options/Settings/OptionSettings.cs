using B.Inputs;
using B.Options.Adventure;
using B.Options.BrainFuck;
using B.Options.MoneyTracker;
using B.Utils;

namespace B.Options.Settings
{
    public sealed class OptionSettings : Option<OptionSettings.Stages>
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

        public OptionSettings() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            switch (this.Stage)
            {
                case Stages.MainMenu:
                    {
                        Util.ClearAndSetSize(35, 12);
                        new Input.Choice("Settings")
                            .Add(() => this.Stage = Stages.Color, "Color", '1')
                            .Add(() => this.Stage = Stages.WindowSize, "Window Size", '2')
                            .Add(() => this.Stage = Stages.DeleteData, "Delete Data", '3')
                            .AddSpacer()
                            .Add(() => Program.Settings.Censor.Toggle(), $"Censor - {Program.Settings.Censor.Active}", key: ConsoleKey.F11)
                            .Add(null!, $"Debug Mode - {Program.Settings.DebugMode.Active}", key: ConsoleKey.F12)
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.WindowSize:
                    {
                        this._size = Vector2.Clamp(this._size, Program.WINDOW_MIN, Program.WINDOW_MAX);
                        Util.ClearAndSetSize(this._size);
                        Util.PrintLine($"Detected Max Size: {Program.WINDOW_MAX}");
                        Util.Print($"Current Size: {this._size}");

                        new Input.Choice()
                            .Add(() => this._size.x++, keyChar: '8', key: ConsoleKey.RightArrow)
                            .Add(() => this._size.x--, keyChar: '2', key: ConsoleKey.LeftArrow)
                            .Add(() => this._size.y++, keyChar: '6', key: ConsoleKey.DownArrow)
                            .Add(() => this._size.y--, keyChar: '4', key: ConsoleKey.UpArrow)
                            .Add(() => this.Stage = Stages.MainMenu, key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Color:
                    {
                        Util.ClearAndSetSize(20, 8);
                        new Input.Choice("Color")
                            .Add(() => this.Stage = Stages.Color_Theme, "Themes", '1')
                            .Add(() => this.Stage = Stages.Color_Custom, "Customize", '2')
                            .AddSpacer()
                            .Add(() => this.Stage = Stages.MainMenu, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Color_Theme:
                    {
                        ColorTheme theme = this._colorThemes[Input.ScrollIndex];
                        Program.Settings.ColorBackground = theme.ColorBG;
                        Program.Settings.ColorText = theme.ColorText;
                        Program.Settings.UpdateColors();
                        Util.ClearAndSetSize(32, this._colorThemes.Length + 8);
                        Input.RequestScroll(
                            items: this._colorThemes,
                            getText: theme => theme.Title,
                            title: "Color Themes",
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this.Stage = Stages.Color;
                            }, "Back", key: ConsoleKey.Escape));
                    }
                    break;

                case Stages.Color_Custom:
                    {
                        Program.Settings.UpdateColors();
                        Util.ClearAndSetSize(32, 27);
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
                                this.Stage = Stages.Color;
                            }, "Exit", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => Program.Settings.ColorBackground = colors[Input.ScrollIndex], "Set Background", '1'),
                                new(() => Program.Settings.ColorText = colors[Input.ScrollIndex], "Set Foreground", '2')});
                    }
                    break;

                case Stages.DeleteData:
                    {
                        Util.ClearAndSetSize(20, 10);
                        new Input.Choice("Delete Data")
                            .Add(() => this.AskDelete("Adventure", () => File.Delete(OptionAdventure.FilePath)), "Adventure", '1')
                            .Add(() => this.AskDelete("BrainFuck", () => Directory.Delete(OptionBrainFuck.DirectoryPath, true)), "BrainFuck", '2')
                            .Add(() => this.AskDelete("Money Tracker", () => Directory.Delete(OptionMoneyTracker.DirectoryPath, true)), "Money Tracker", '3')
                            .Add(() => this.AskDelete("Settings", () => File.Delete(ProgramSettings.Path)), "Settings", '4')
                            .AddSpacer()
                            .Add(() => this.Stage = Stages.MainMenu, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;
            }
        }

        private void AskDelete(string title, Action deleteAction)
        {
            Util.ClearAndSetSize(30, 7);
            new Input.Choice($"Delete {title} Data?")
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
        }
    }
}
