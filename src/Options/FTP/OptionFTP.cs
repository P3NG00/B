using System.Net.Sockets;
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
        private const int MAX_LIST_ENTRIES = 50;
        private const int WIDTH = 140;
        private const string IP = "***REMOVED***"; // TODO store IP's in serializable Profiles
        private const int PORT = 22;
        private const string USER = "***REMOVED***";

        private static string DownloadPath => Environment.CurrentDirectory + @"\download\";
        private static Func<string, string>[] _scramblers = new Func<string, string>[]
        {
            // Center Waves
            s =>
            {
                string[] sa =
                {
                    @"    ^    ",
                    @"   / \   ",
                    @"  /   \  ",
                    @" /     \ ",
                    @"/       \",
                };
                return sa[s.Length % sa.Length];
            },
            // Crazy Scroll
            s =>
            {
                string[] sa =
                {
                    "    ~    ",
                    "   ~     ",
                    "     ~   ",
                    "  ~      ",
                    "      ~  ",
                    " ~       ",
                    "       ~ ",
                    "~        ",
                    "        ~",
                };
                return sa[s.Length % sa.Length];
            },
            // Star Scroll
            s =>
            {
                char[] ca = new char[7];
                Array.Fill(ca, '-');
                ca[s.Length % ca.Length] = '*';
                return new string(ca);
            },
            // Random Scroll
            s =>
            {
                char[] ca = new char[9];
                Array.Fill(ca, ' ');
                ca[s.Length % ca.Length] = Util.RandomFromList("!?@#$%&*+=~XO§®¡©█■".ToCharArray());
                return new string(ca);
            },
        };
        private static Func<string, string> _scrambler = null!;

        private SftpClient _client = null!;
        private IEnumerable<SftpFile> _files = null!;
        private Stage _stage = Stage.Login;
        private string Path
        {
            get => this._path;
            set
            {
                this._path = value;
                this.RefreshFiles();
            }
        }
        private string _path = string.Empty;

        private SftpFile CurrentFile => this._files.ElementAt(this.Index);
        private int Index
        {
            get => this._index;
            set
            {
                // Get last value
                int lastValue = this._index % OptionFTP.MAX_LIST_ENTRIES;
                // Update value
                this._index = Util.Clamp(value, 0, this._files.Count() - 1);
                // Get new value
                int newValue = this._index % OptionFTP.MAX_LIST_ENTRIES;

                // If crossing into new page, clear console
                int oneLess = OptionFTP.MAX_LIST_ENTRIES - 1;
                if ((lastValue == oneLess && newValue == 0) || (lastValue == 0 && newValue == oneLess))
                    Util.ClearConsole();
            }
        }
        private int _index = 0;

        public OptionFTP()
        {
            Input.String = string.Empty;
            OptionFTP._scrambler = Util.RandomFromList(OptionFTP._scramblers);
            DirectoryInfo downloadDir = new DirectoryInfo(OptionFTP.DownloadPath);

            if (!downloadDir.Exists)
                downloadDir.Create();
        }

        public override void Loop()
        {
            switch (this._stage)
            {
                case Stage.Login:
                    {
                        int consoleWidth = 16;
                        Util.ClearConsole(consoleWidth, 5);
                        Util.Print("Login", 5, linesBefore: 1);
                        string scrambled = OptionFTP._scrambler(Input.String);
                        int textDepth = (consoleWidth / 2) + (scrambled.Length / 2);
                        Util.Print(string.Format("{0," + textDepth + "}", scrambled));

                        switch (Input.Request(OptionFTP.MAX_LENGTH_PASSWORD))
                        {
                            case ConsoleKey.Enter:
                                {
                                    this._client = new SftpClient(OptionFTP.IP, OptionFTP.PORT, OptionFTP.USER, Input.String);
                                    Input.String = string.Empty;

                                    try
                                    {
                                        this._client.Connect();
                                        this.RefreshFiles();
                                        this._stage = Stage.Navigate;
                                    }
                                    catch (SshAuthenticationException)
                                    {
                                        Util.Print("Wrong password");
                                        Util.WaitForInput();
                                    }
                                    catch (SocketException)
                                    {
                                        Util.Print("Can't connect", 1);
                                        Util.WaitForInput();
                                    }
                                }
                                break;

                            case ConsoleKey.Escape: this.Quit(); break;
                        }
                    }
                    break;

                case Stage.Navigate:
                    {
                        int entryAmount = this._files.Count();
                        int consoleHeight = Math.Min(entryAmount, OptionFTP.MAX_LIST_ENTRIES) + 11;
                        Util.SetConsoleSize(OptionFTP.WIDTH, consoleHeight);
                        Console.SetCursorPosition(0, 0);
                        string header = $"index: ({this.Index + 1} / {entryAmount}) | path > '{this.Path}'";
                        Util.Print($"{header,-98}", 1, linesBefore: 1);
                        Util.Print();
                        Input.Option iob = new Input.Option();

                        if (entryAmount > 0)
                        {
                            int startIndex = this.Index - (this.Index % OptionFTP.MAX_LIST_ENTRIES);
                            int endIndex = Math.Min(startIndex + OptionFTP.MAX_LIST_ENTRIES, entryAmount);

                            for (int i = startIndex; i < endIndex; i++)
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
                            SftpFile currentFile = this.CurrentFile;
                            iob.AddKeybind(new Keybind(() => this.Index--, keyChar: '8', key: ConsoleKey.UpArrow))
                                .AddKeybind(new Keybind(() => this.Index++, keyChar: '2', key: ConsoleKey.DownArrow))
                                .AddKeybind(new Keybind(() => this.Download(currentFile), "Download", key: ConsoleKey.PageDown))
                                .AddKeybind(new Keybind(() => this.Delete(currentFile), "Delete", key: ConsoleKey.Delete))
                                .AddKeybind(new Keybind(() =>
                                {
                                    if (currentFile.IsDirectory)
                                        this.Path += "/" + currentFile.Name;
                                    else
                                        this._stage = Stage.FileInteract;
                                }, "Select", key: ConsoleKey.Enter));
                        }
                        else
                            Util.Print("Directory empty...", 3);

                        iob.AddKeybind(new Keybind(() =>
                                {
                                    if (this.Path != string.Empty)
                                        this.Path = this.Path.Substring(0, this.Path.LastIndexOf('/'));
                                    else
                                    {
                                        this._client.Disconnect();
                                        this.Quit();
                                    }
                                }, "Back", key: ConsoleKey.Escape))
                                .Request();
                    }
                    break;

                case Stage.FileInteract:
                    {
                        Util.ClearConsole(OptionFTP.WIDTH, 8);
                        SftpFile file = this.CurrentFile;
                        new Input.Option(file.FullName)
                            .AddKeybind(new Keybind(() => this.Download(file), "Download", key: ConsoleKey.PageDown))
                            .AddKeybind(new Keybind(() => this.Delete(file), "Delete", key: ConsoleKey.Delete))
                            .AddSpacer()
                            .AddKeybind(new Keybind(() => this._stage = Stage.Navigate, "Back", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;
            }
        }

        private void RefreshFiles()
        {
            this._files = this._client.ListDirectory(this.Path).OrderBy(x => !x.IsDirectory);
            this.Index = 0;
            Util.ClearConsole();
        }

        private void Download(SftpFile file)
        {
            if (file.IsDirectory)
            {
                foreach (SftpFile subFile in this._client.ListDirectory(this.Path + file.FullName))
                    this.Download(subFile);
            }
            else
            {
                string path = OptionFTP.DownloadPath + file.FullName.Substring(1);
                DirectoryInfo? newFileDir = new FileInfo(path).Directory;

                if (newFileDir != null)
                    newFileDir.Create();

                using (Stream stream = File.Open(path, FileMode.Create))
                    this._client.DownloadFile(file.FullName, stream);
            }
        }

        private void Delete(SftpFile file)
        {
            this._client.Delete(file.FullName);
            this.RefreshFiles();
        }

        private enum Stage
        {
            Login,
            Navigate,
            FileInteract,
        }
    }
}
