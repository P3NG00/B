using System.Net.Sockets;
using B.Inputs;
using B.Utils;
using B.Utils.Extensions;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace B.Options.Tools.FTP
{
    public sealed class OptionFTP : Option<OptionFTP.Stages>
    {
        public const string Title = "FTP";

        private const int MAX_LENGTH_PASSWORD = 50;
        private const int MAX_LIST_ENTRIES = 40;
        private const int WIDTH = 140;
        private const string SERVER_IP = "***REMOVED***";
        private const string SERVER_USER = "***REMOVED***";
        private const int SERVER_PORT = 22;

        private static string DownloadPath => Environment.CurrentDirectory + @"\download\";
        private static Func<int, string>[] _scramblers = new Func<int, string>[]
        {
            // Center Waves
            l => new string[]
                {
                    @"    ^    ",
                    @"   / \   ",
                    @"  /   \  ",
                    @" /     \ ",
                    @"/       \",
                }.FromRemainder(l),
            // Star Scroll
            l =>
            {
                char[] ca = new char[7];
                Array.Fill(ca, '-');
                ca[l % ca.Length] = '*';
                return new string(ca);
            },
            // Random Scroll
            l =>
            {
                char[] ca = new char[9];
                Array.Fill(ca, ' ');
                ca[l % ca.Length] = new char[] {'!', '?', '@', '#', '$', '%', '&', '*', '+', '=',
                                                '~', 'X', 'O', 'Q', '§', '®', '¡', '¿', '©', '█',
                                                '■', '"', '/', '·', '|', ':', ';', '<', '>', '-',
                                                '\\'}.Random();
                return new string(ca);
            },
            // Bar Fill
            l =>
            {
                char[] ca = new char[9];
                int fillDepth = l % (ca.Length + 1);

                for (int i = 0; i < ca.Length; i++)
                    ca[i] = i < fillDepth ? '#' : '-';

                return new string(ca);
            },
            // Spinner
            l => Convert.ToString(new char[] { '|', '/', '-', '\\' }.FromRemainder(l)),
            // Dots
            l => new string[]
                {
                    "     ",
                    "·    ",
                    "· ·  ",
                    "· · ·",
                }.FromRemainder(l),
        };

        private Func<int, string> _scrambler;
        private SftpClient _client = null!;
        private SftpFile[] _files = null!;
        private string Path
        {
            get => _path;
            set
            {
                _path = value;
                RefreshFiles();
            }
        }
        private string _path = string.Empty;
        private Stages _lastStage = Stages.Navigate;

        private SftpFile CurrentFile => _files[Input.ScrollIndex];

        public OptionFTP() : base(Stages.Login)
        {
            // Clear console to prevent console flashing during login
            Window.Clear();

            // Empty the input string
            Input.ResetString();

            // Select a random scrambler to show.
            _scrambler = OptionFTP._scramblers.Random();

            // If Download Path doesn't exist, create it.
            if (Directory.Exists(OptionFTP.DownloadPath))
                Directory.CreateDirectory(OptionFTP.DownloadPath);
        }

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.Login:
                    {
                        int consoleWidth = 16;
                        Window.SetSize(consoleWidth, 5);
                        Cursor.Position = new(5, 1);
                        Window.Print("Login");
                        string scrambled = _scrambler(Input.String.Length);
                        int textStart = (int)((consoleWidth / 2f) - (scrambled.Length / 2f));
                        Cursor.Position = new(textStart, 2);
                        Window.Print(scrambled);
                        Cursor.Position = new(0, 3);
                        Input.RequestLine(OptionFTP.MAX_LENGTH_PASSWORD,
                            new Keybind(() =>
                            {
                                _client = new(OptionFTP.SERVER_IP, OptionFTP.SERVER_PORT, OptionFTP.SERVER_USER, Input.String);
                                Input.ResetString();

                                try
                                {
                                    _client.Connect();
                                    RefreshFiles();
                                    SetStage(Stages.Navigate);
                                }
                                catch (SshAuthenticationException) { PrintError("Wrong password"); }
                                catch (SocketException) { PrintError("Can't connect"); }
                                catch (SshConnectionException) { PrintError("Error"); } // TODO change so that if Debug mode is on, show full error output
                            }, key: ConsoleKey.Enter),
                            new Keybind(() => Input.ResetString(), key: ConsoleKey.Delete),
                            new Keybind(() => Quit(), key: ConsoleKey.Escape));

                        void PrintError(string msg)
                        {
                            Window.Print(msg);
                            Input.Get();
                            Window.Clear();
                        }
                    }
                    break;

                case Stages.Navigate:
                    {
                        _lastStage = Stage;
                        int entryAmount = _files.Length;
                        int adjustedMaxEntries = Math.Min(Window.SIZE_MAX.y - 14, OptionFTP.MAX_LIST_ENTRIES);
                        int consoleHeight = Math.Min(entryAmount, adjustedMaxEntries) + 14;
                        Window.SetSize(OptionFTP.WIDTH, consoleHeight);
                        Cursor.Position = new(1, 1);
                        Input.RequestScroll(
                            items: _files,
                            getText: file =>
                            {
                                string s = file.Name;

                                if (file.IsDirectory)
                                    s += "/";

                                return s;
                            },
                            title: $"{$"index: ({Input.ScrollIndex + 1} / {entryAmount}) | path > '{Path}'",-98}",
                            maxEntriesPerPage: adjustedMaxEntries,
                            exitKeybind: new(() =>
                            {
                                if (Path == string.Empty)
                                {
                                    Input.ScrollIndex = 0;
                                    Quit();
                                }
                                else
                                    PreviousDirectory();
                            }, "Exit", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => SetStage(Stages.Download), "Download", key: ConsoleKey.PageDown),
                                new(() => SetStage(Stages.Delete), "Delete", key: ConsoleKey.Delete),
                                new(() =>
                                {
                                    SftpFile currentFile = CurrentFile;

                                    if (currentFile.IsDirectory)
                                        Path += "/" + currentFile.Name;
                                    else
                                        SetStage(Stages.FileInteract);
                                }, "Select", key: ConsoleKey.Enter),
                                new(() => RefreshFiles(), "Refresh", key: ConsoleKey.F5),
                                new(() => PreviousDirectory(), "Back", key: ConsoleKey.Backspace)});
                    }
                    break;

                case Stages.FileInteract:
                    {
                        _lastStage = Stage;
                        Window.Clear();
                        Window.SetSize(OptionFTP.WIDTH, 8);
                        SftpFile file = CurrentFile;
                        Input.Choice choice = Input.Choice.Create($"{file.FullName} | {file.Attributes.Size} bytes");
                        choice.Add(() => SetStage(Stages.Download), "Download", key: ConsoleKey.PageDown);
                        choice.Add(() => SetStage(Stages.Delete), "Delete", key: ConsoleKey.Delete);
                        choice.AddSpacer();
                        choice.Add(() => SetStage(Stages.Navigate), "Back", key: ConsoleKey.Escape);
                        choice.Request();
                        Window.Clear();
                    }
                    break;

                case Stages.Download:
                    {
                        // May hang while downloading files
                        Download(CurrentFile);
                        Window.Clear();
                        SetStage(_lastStage);
                    }
                    break;

                case Stages.Delete:
                    {
                        Window.Clear();
                        Window.SetSize(OptionFTP.WIDTH, 9);
                        SftpFile currentFile = CurrentFile;
                        Cursor.Position = new(2, 1);
                        Window.Print(currentFile.FullName);
                        Input.Choice choice = Input.Choice.Create("Are you sure you want to delete this file?");
                        choice.Add(() => Delete(currentFile), "yes", key: ConsoleKey.Enter);
                        choice.AddSpacer();
                        choice.Add(null!, "NO", key: ConsoleKey.Escape);
                        Cursor.Position = new(0, 2);
                        choice.Request();
                        SetStage(Stages.Navigate);
                        Window.Clear();
                    }
                    break;
            }
        }

        private void RefreshFiles()
        {
            _files = _client.ListDirectory(Path).OrderBy(x => !x.IsDirectory).ToArray();
            Input.ScrollIndex = 0;
            Window.Clear();
        }

        private void Download(SftpFile file)
        {
            if (file.IsDirectory)
                foreach (SftpFile subFile in _client.ListDirectory(file.FullName))
                    Download(subFile);
            else
            {
                string path = OptionFTP.DownloadPath + file.FullName.Substring(1);
                DirectoryInfo? newFileDir = new FileInfo(path).Directory;

                if (newFileDir != null)
                    newFileDir.Create();

                using (Stream stream = File.Open(path, FileMode.Create))
                {
                    Window.Clear();
                    Window.SetSize(OptionFTP.WIDTH, 7);
                    Cursor.Position = new(2, 1);
                    Window.Print("Downloading...");
                    Cursor.Position = new(2, 3);
                    Window.Print(file.FullName);
                    Cursor.Position = new(2, 5);
                    Window.Print("Bytes Downloaded");
                    _client.DownloadFile(file.FullName, stream, l =>
                    {
                        Cursor.Position = new(19, 5);
                        Window.Print($"{l,-(OptionFTP.WIDTH - 30)}");
                    });
                }
            }
        }

        private void Delete(SftpFile file)
        {
            try { _client.Delete(file.FullName); }
            catch (SshException)
            {
                Window.Clear();
                Window.SetSize(21, 6);
                Cursor.Position = new(7, 2);
                Window.Print("Error:");
                Cursor.Position = new(2, 3);
                Window.Print("Can't delete file");
                Input.Get();
            }

            RefreshFiles();
        }

        private void PreviousDirectory()
        {
            if (Path != string.Empty)
                Path = Path.Substring(0, Path.LastIndexOf('/'));
        }

        public enum Stages
        {
            Login,
            Navigate,
            FileInteract,
            Download,
            Delete,
        }

        public override void Quit()
        {
            if (_client != null && _client.IsConnected)
                _client.Disconnect();

            base.Quit();
        }
    }
}
