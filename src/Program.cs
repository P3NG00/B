﻿using B.Inputs;
using B.Options;
using B.Options.Games.Adventure;
using B.Options.Games.NumberGuesser;
using B.Options.Games.OptionCheckers;
using B.Options.Tools.Backup;
using B.Options.Tools.FTP;
using B.Options.Tools.MoneyTracker;
using B.Options.Tools.Settings;
using B.Options.Toys.BrainFuck;
using B.Options.Toys.Canvas;
using B.Options.Toys.ExpressionSolver;
using B.Utils;

namespace B
{
    public sealed class Program : Option<Program.Stages>
    {
        public const string Title = "B";

        // Code entry point
        public static int Main()
        {
            Program program = new Program();
            return program.Start();
        }

        // Program Info
        public static ProgramSettings Settings { get; private set; } = new();
        public static string DataPath => Environment.CurrentDirectory + @"\data\";

        // Option Groups
        private static (string GroupTitle, (Type OptionType, Func<string> GetTitle)[] Options)[] OptionGroups => new (string, (Type, Func<string>)[])[]
        {
            ("Games", new (Type, Func<string>)[] {
                (typeof(OptionAdventure), () => OptionAdventure.Title),
                (typeof(OptionCheckers), () => OptionCheckers.Title),
                (typeof(OptionNumberGuesser), () => OptionNumberGuesser.Title),
            }),
            ("Tools", new (Type, Func<string>)[] {
                (typeof(OptionFTP), () => OptionFTP.Title),
                (typeof(OptionMoneyTracker), () => OptionMoneyTracker.Title),
                (typeof(OptionBackup), () => OptionBackup.Title),
                (typeof(OptionSettings), () => OptionSettings.Title),
            }),
            ("Toys", new (Type, Func<string>)[] {
                (typeof(OptionCanvas), () => OptionCanvas.Title),
                (typeof(OptionBrainFuck), () => OptionBrainFuck.Title),
                (typeof(OptionExpressionSolver), () => OptionExpressionSolver.Title),
            }),
        };
        private (string GroupTitle, (Type OptionType, Func<string> GetTitle)[] Options) _optionGroup;
        private IOption _selectedOption = null!;

        public Program() : base(Stages.MainMenu) { }

        private int Start()
        {
            // Initialize program
            try { this.Initialize(); }
            catch (Exception e)
            {
                this.HandleException(e);
                Environment.Exit(1);
            }

            // Program loop
            while (this.IsRunning())
            {
                try { this.Loop(); }
                catch (Exception e) { this.HandleException(e); }
            }

            // Save before exiting
            try { Util.Serialize(ProgramSettings.Path, Program.Settings); }
            catch (Exception e) { this.HandleException(e); }

            // If reached, return nothing
            return 0;
        }

        private void Initialize()
        {
            // Initialize window properties
            External.Initialize();

            // Set console window title
            Console.Title = Program.Title;

            // Console input ctrl+c override
            if (OperatingSystem.IsWindows())
                Console.TreatControlCAsInput = true;

            // Load program settings
            if (File.Exists(ProgramSettings.Path))
            {
                // If settings can't be loaded, just handle exception.
                // Settings are already initialized to default values.
                try { Program.Settings = Util.Deserialize<ProgramSettings>(ProgramSettings.Path); }
                catch (Exception) { }
            }

            // Set console colors
            Program.Settings.UpdateColors();
        }

        public override void Loop()
        {
            // If directory doesn't exist, create it and add hidden attribute
            if (!Directory.Exists(Program.DataPath))
            {
                DirectoryInfo mainDirectory = Directory.CreateDirectory(Program.DataPath);
                mainDirectory.Attributes = FileAttributes.Hidden;
            }

            switch (this.Stage)
            {
                case Stages.MainMenu:
                    {
                        // Display main menu options
                        Window.ClearAndSetSize(22, Program.OptionGroups.Length + 6);
                        Input.Choice iob = new($"{Program.Title}'s");

                        for (int i = 0; i < Program.OptionGroups.Length; i++)
                        {
                            var optionGroup = Program.OptionGroups[i];
                            iob.Add(() =>
                            {
                                this._optionGroup = optionGroup;
                                this.SetStage(Stages.Group);
                            }, optionGroup.GroupTitle, (char)('1' + i));
                        }

                        iob.AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.Group:
                    {
                        // TODO test
                        Window.ClearAndSetSize(22, this._optionGroup.Options.Length + 6);
                        Input.Choice iob = new(this._optionGroup.GroupTitle);

                        for (int i = 0; i < this._optionGroup.Options.Length; i++)
                        {
                            var option = this._optionGroup.Options[i];
                            iob.Add(() =>
                            {
                                this._selectedOption = (IOption)Activator.CreateInstance(option.OptionType)!;
                                this.SetStage(Stages.Option);
                            }, option.GetTitle(), (char)('1' + i));
                        }

                        iob.AddSpacer()
                            .Add(() => this.SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Option:
                    {
                        if (this._selectedOption != null && this._selectedOption.IsRunning())
                            this._selectedOption.Loop();
                        else
                        {
                            this._selectedOption = null!;
                            this.SetStage(Stages.Group);
                        }
                    }
                    break;
            }
        }

        private void HandleException(Exception e)
        {
            Window.ClearAndSetSize(Window.SIZE_MAX);
            Window.Print("An exception was thrown!", (2, 1), ConsoleColor.Red);
            Window.Print(e, (2, 3), ConsoleColor.White, ConsoleColor.Black);
            Vector2 cursorPos = Cursor.GetPositionVector();
            cursorPos.x = 2;
            cursorPos.y += 2;
            Input.WaitFor(ConsoleKey.F1, cursorPos);
            Window.Clear();
            this._selectedOption = null!;
            this.SetStage(Stages.MainMenu);
        }

        public enum Stages
        {
            MainMenu,
            Group,
            Option,
        }
    }
}
