using System.Reflection;
using B.Inputs;
using B.Utils;
using B.Utils.Extensions;
using Figgle;
using TextCopy;

namespace B.Options.Toys.TextGenerator
{
    public sealed class OptionTextGenerator : Option<OptionTextGenerator.Stages>
    {
        public static string Title => "Text Generator";

        // Font and Name list
        private static FontType[] _fonts = null!;

        // Current Font from Index
        private FontType FontInfo => _fonts[_fontIndex];

        // Index of current Font
        private int _fontIndex = 0;

        public OptionTextGenerator() : base(Stages.MainMenu)
        {
            // Initialize font list first time only
            if (_fonts is null)
            {
                // Get all font properties
                PropertyInfo[] properties = typeof(FiggleFonts).GetProperties();
                // From each property, put font into array
                _fonts = properties.FromEach(property => new FontType(property.Name, (FiggleFont)property.GetValue(null)!)).ToArray();
                // Sort array by name
                Array.Sort(_fonts, (fontA, fontB) => fontA.Name.CompareTo(fontB.Name));
            }

            // Reset text
            Input.String = Program.Title;
            // Select random font
            SelectRandomFont();
        }

        public sealed override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.Clear();
                        var fontInfo = FontInfo;
                        // Create output
                        string output = fontInfo.Font.Render(Input.String);
                        // Copy to clipboard
                        ClipboardService.SetText(output);
                        Window.Size = new(Window.SIZE_MAX.x - 5, fontInfo.Font.Height + 12);
                        // Print info
                        Cursor.Position = new(2, 1);
                        Window.Print($"Input: \"{Input.String}\"");
                        Cursor.Position = new(2, 2);
                        Window.Print($"Font: {fontInfo.Name}");
                        // Print output
                        Cursor.Position = new(0, 4);
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
                            _fontIndex = _fonts.Length - 1;
                        else if (_fontIndex >= _fonts.Length)
                            _fontIndex = 0;
                    }
                    break;

                case Stages.FontSelect:
                    {
                        Window.SetSize(27, 55);
                        Cursor.Position = new(2, 1);
                        Input.RequestScroll(
                            items: _fonts,
                            getText: font => font.Name,
                            title: "Select Font",
                            exitKeybind: Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape),
                            extraKeybinds: Keybind.Create(() =>
                            {
                                _fontIndex = Input.ScrollIndex;
                                SetStage(Stages.MainMenu);
                            }, "Select", key: ConsoleKey.Enter));
                    }
                    break;
            }
        }

        private void SelectRandomFont() => _fontIndex = _fonts.RandomIndex();

        public enum Stages
        {
            MainMenu,
            FontSelect,
        }
    }
}
