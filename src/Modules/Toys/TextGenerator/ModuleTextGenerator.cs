using B.Inputs;
using B.Utils;
using B.Utils.Extensions;

namespace B.Modules.Toys.TextGenerator
{
    public sealed class ModuleTextGenerator : Module<ModuleTextGenerator.Stages>
    {
        #region Universal Properties

        /// Module Title.
        public static string Title => "Text Generator";

        #endregion



        #region Private Variables

        // Gets current FontType from current font index.
        private FontType FontInfo => Fonts.FontArray[_fontIndex];
        // Current font index.
        private int _fontIndex = 0;

        #endregion



        #region Constructors

        // Creates new instance of ModuleTextGenerator.
        public ModuleTextGenerator() : base(Stages.Text)
        {
            // Reset text
            Input.String = Program.Title;
            // Select random font
            SelectRandomFont();
        }

        #endregion



        #region Override Methods

        // Module Loop.
        public sealed override void Loop()
        {
            switch (Stage)
            {
                case Stages.Text:
                    {
                        var fontInfo = FontInfo;
                        // Create output
                        string output = fontInfo.Font.Render(Input.String);
                        // Copy to clipboard
                        Clipboard.Text = output;
                        int width = Window.SizeMax.x - 2;
                        Window.SetSize(width, fontInfo.Font.Height + 13);
                        // Print info
                        Cursor.Set(2, 1);
                        Window.Print($"Input: \"{Input.String}\"");
                        Cursor.Set(2, 2);
                        Window.Print($"Font: {fontInfo.Name}");
                        // Check width of output
                        string[] split = output.Split("\r\n");
                        Cursor.y = 5;
                        if (split[0].Length + 4 >= width)
                        {
                            // Too big, print message
                            OutputPrint("Output is too long. Text has been copied to your clipboard to paste elsewhere.");
                        }
                        else
                        {
                            // Within width, print output
                            split.ForEach(line => OutputPrint(line));
                        }
                        void OutputPrint(string s)
                        {
                            Cursor.x = 2;
                            Window.Print(s);
                            Cursor.NextLine();
                        }
                        // Request
                        Cursor.NextLine();
                        Input.RequestLine(keybinds: new Keybind[] {
                            Keybind.Create(() => _fontIndex++, "Next Font", key: ConsoleKey.DownArrow),
                            Keybind.Create(() => _fontIndex--, "Prev Font", key: ConsoleKey.UpArrow),
                            Keybind.Create(SelectRandomFont, "Random Font", key: ConsoleKey.F5),
                            Keybind.Create(() =>
                            {
                                Input.ScrollIndex = _fontIndex;
                                SetStage(Stages.FontSelect);
                            }, "Select Font", key: ConsoleKey.F1),
                            Keybind.CreateModuleExit(this),
                        });
                        // Fix font index
                        if (_fontIndex < 0)
                            _fontIndex = Fonts.FontArray.Length - 1;
                        else if (_fontIndex >= Fonts.FontArray.Length)
                            _fontIndex = 0;
                    }
                    break;

                case Stages.FontSelect:
                    {
                        Cursor.y = 1;
                        Window.SetSize(27, 55);
                        Input.RequestScroll(
                            items: Fonts.FontArray,
                            getText: font => font.Name,
                            title: "Select Font",
                            exitKeybind: Keybind.Create(() => SetStage(Stages.Text), "Back", key: ConsoleKey.Escape),
                            extraKeybinds: Keybind.Create(() =>
                            {
                                _fontIndex = Input.ScrollIndex;
                                SetStage(Stages.Text);
                            }, "Select", key: ConsoleKey.Enter)
                        );
                    }
                    break;
            }
        }

        #endregion



        #region Private Methods

        // Selects new font at random.
        private void SelectRandomFont() => _fontIndex = Fonts.FontArray.RandomIndex();

        #endregion



        #region Enums

        // Module Stages.
        public enum Stages
        {
            // Text input.
            Text,
            // Font selection.
            FontSelect,
        }

        #endregion
    }
}
