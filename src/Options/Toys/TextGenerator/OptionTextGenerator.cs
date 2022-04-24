using B.Inputs;
using B.Utils;
using B.Utils.Extensions;

namespace B.Options.Toys.TextGenerator
{
    public sealed class OptionTextGenerator : Option<OptionTextGenerator.Stages>
    {
        public static string Title => "Text Generator";

        // Current Font from Index
        private FontType FontInfo => Fonts.FontArray[_fontIndex];

        // Index of current Font
        private int _fontIndex = 0;

        public OptionTextGenerator() : base(Stages.Text)
        {
            // Reset text
            Input.String = Program.Title;
            // Select random font
            SelectRandomFont();
        }

        public sealed override void Loop()
        {
            switch (Stage)
            {
                case Stages.Text:
                    {
                        Window.Clear();
                        var fontInfo = FontInfo;
                        // Create output
                        string output = fontInfo.Font.Render(Input.String);
                        // Copy to clipboard
                        Clipboard.Text = output;
                        Window.SetSize(Window.SizeMax.x - 5, fontInfo.Font.Height + 12);
                        // Print info
                        Cursor.Set(2, 1);
                        Window.Print($"Input: \"{Input.String}\"");
                        Cursor.Set(2, 2);
                        Window.Print($"Font: {fontInfo.Name}");
                        // Print output
                        Cursor.Set(0, 4);
                        output.Split("\r\n").ForEach(line =>
                        {
                            Cursor.x = 2;
                            Window.Print(line);
                            Cursor.y++;
                        });
                        // Request
                        Cursor.y++;
                        Input.RequestLine(keybinds: new Keybind[] {
                            Keybind.Create(() => _fontIndex++, "Next Font", key: ConsoleKey.DownArrow),
                            Keybind.Create(() => _fontIndex--, "Prev Font", key: ConsoleKey.UpArrow),
                            Keybind.Create(SelectRandomFont, "Random Font", key: ConsoleKey.F5),
                            Keybind.Create(() =>
                            {
                                Window.Clear();
                                Input.ScrollIndex = _fontIndex;
                                SetStage(Stages.FontSelect);
                            }, "Select Font", key: ConsoleKey.F1),
                            Keybind.CreateOptionExit(this),
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
                            }, "Select", key: ConsoleKey.Enter));
                    }
                    break;
            }
        }

        private void SelectRandomFont() => _fontIndex = Fonts.FontArray.RandomIndex();

        public enum Stages
        {
            Text,
            FontSelect,
        }
    }
}
