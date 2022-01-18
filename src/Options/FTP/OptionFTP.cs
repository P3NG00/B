using B.Inputs;
using B.Utils;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace B.Options.FTP
{
    public sealed class OptionFTP : Option
    {
        private const int MAX_LENGTH_PASSWORD = 50;

        // TODO store IP's in serializable Profiles
        private const string IP = "***REMOVED***";
        private const int PORT = 22;
        private const string USER = "***REMOVED***";

        private SftpClient _client = null!;
        private IEnumerable<SftpFile> _files = null!;
        private string _path = string.Empty;
        private Stage _stage = Stage.Login;

        private int Index
        {
            get => this._index;
            set => this._index = Util.Clamp(value, 0, this._files.Count() - 1);
        }
        private int _index = 0;

        private SftpFile CurrentFile => this._files.ElementAt(this.Index);

        public OptionFTP() => Input.String = string.Empty;

        public override void Loop()
        {
            switch (this._stage)
            {
                case Stage.Login:
                    {
                        Console.Clear();
                        Util.SetConsoleSize(16, 5);
                        Util.Print("Login", 5, linesBefore: 1);

                        // TODO make cool random patterns that appear when typing in password
                        // examples:
                        // password length - shows how many characters are input in the password
                        // scrolling text - each character typed puts the char at the end of the line to the front of the line
                        // random characters - each character typed makes random characters appear on this line
                        // hash code - each character typed makes the hash code for your password appear on this line
                        // etc......
                        Util.Print(string.Format("{0,8}", Input.String.Length));

                        switch (Input.Request(OptionFTP.MAX_LENGTH_PASSWORD))
                        {
                            case ConsoleKey.Enter:
                                {
                                    this._client = new SftpClient(OptionFTP.IP, OptionFTP.PORT, OptionFTP.USER, Input.String);
                                    Input.String = string.Empty;
                                    this._client.KeepAliveInterval = TimeSpan.FromSeconds(5); // TODO test, necessary?

                                    try
                                    {
                                        this._client.Connect();
                                        this.RefreshFiles();
                                        this._stage = Stage.Navigate;
                                    }
                                    catch (SshAuthenticationException)
                                    {
                                        Util.Print("Wrong password", 1);
                                        Util.WaitForInput();
                                    }
                                    // TODO test wrong passwords
                                }
                                break;

                            case ConsoleKey.Escape: this.Quit(); break;
                        }
                    }
                    break;

                case Stage.Navigate:
                    {
                        Console.SetCursorPosition(0, 0);

                        int consoleHeight = this._files.Count() + 10;

                        // TODO limit pages to 50 per page

                        if (consoleHeight > Util.MAX_CONSOLE_HEIGHT)
                        {
                            // TODO fix console height
                            // an error will be throw if the height is too big
                        }

                        // TODO change width
                        Util.SetConsoleSize(100, this._files.Count() + 10);
                        Util.Print($"path > '{this._path}/'", 1, linesBefore: 1);
                        Util.Print();

                        for (int i = 0; i < this._files.Count(); i++)
                        {
                            SftpFile file = this._files.ElementAt(i);
                            string fileName = file.Name;

                            if (file.IsDirectory)
                                fileName += "/";

                            if (i == this.Index)
                                Util.Print($"> {fileName}", 1);
                            else
                                Util.Print(string.Format("{0,-" + (fileName.Length + 1) + "}", fileName), 2);
                        }

                        Util.Print("Use Up/Down Arrow to navigate.", 1, linesBefore: 1);

                        new Input.Option()
                            .AddKeybind(new Keybind(() => this.Index--, keyChar: '8', key: ConsoleKey.UpArrow))
                            .AddKeybind(new Keybind(() => this.Index++, keyChar: '2', key: ConsoleKey.DownArrow))
                            .AddKeybind(new Keybind(() =>
                            {
                                this._path = this._path.Substring(0, this._path.LastIndexOf('/'));
                                this.RefreshFiles();
                            }, "Previous", key: ConsoleKey.Backspace))
                            .AddKeybind(new Keybind(() =>
                            {
                                SftpFile file = this.CurrentFile;

                                if (file.IsDirectory)
                                {
                                    this._path += "/" + file.Name;
                                    this.RefreshFiles();
                                }
                                else
                                {
                                    // TODO interact with files
                                }
                            }, "Select", key: ConsoleKey.Enter))
                            .AddKeybind(new Keybind(() =>
                            {
                                this._client.Disconnect();
                                this.Quit();
                            }, "Back", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;
            }
        }

        private void RefreshFiles()
        {
            this._files = this._client.ListDirectory(this._path).OrderBy(x => !x.IsDirectory);
            this.Index = 0;
            Console.Clear();
        }

        private enum Stage
        {
            Login,
            Navigate,
        }
    }
}
