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

        private static (string Name, FiggleFont Font)[] _fonts = null!;

        private (string Name, FiggleFont Font) FontInfo => _fonts[_fontIndex];

        private int _fontIndex = 0;

        public OptionTextGenerator() : base(Stages.MainMenu)
        {
            // Initialize font list first time only
            if (_fonts is null)
            {
                // Get all fonts
                PropertyInfo[] properties = typeof(FiggleFonts).GetProperties();
                _fonts = properties.FromEach(property => (property.Name, (FiggleFont)property.GetValue(null)!)).ToArray();
            }

            Input.String = "text";
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
                        string output = fontInfo.Font.Render(Input.String);
                        Window.Size = new(Window.SIZE_MAX.x - 5, fontInfo.Font.Height + 5);
                        Cursor.Position = new(2, 1);
                        Window.Print($"Input: \"{Input.String}\"");
                        Cursor.Position = new(2, 2);
                        Window.Print($"Font: {fontInfo.Name}");
                        Cursor.Position = new(0, 4);
                        Window.Print(output);
                        ClipboardService.SetText(output);
                        Input.RequestLine(keybinds: new Keybind[] {
                            new(() => _fontIndex++, key: ConsoleKey.DownArrow),
                            new(() => _fontIndex--, key: ConsoleKey.UpArrow),
                            new(SelectRandomFont, key: ConsoleKey.F5),
                            Keybind.CreateOptionExitKeybind(this),
                        });
                        _fontIndex = _fontIndex.Clamp(0, _fonts.Length); // TODO make sure this works
                    }
                    break;
            }
        }

        private void SelectRandomFont() => _fontIndex = _fonts.RandomIndex();

        public enum Stages
        {
            MainMenu,
            FontSelector,
        }
    }
}
