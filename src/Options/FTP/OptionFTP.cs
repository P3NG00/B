using System.Net.Sockets;
using B.Inputs;
using B.Utils;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace B.Options.FTP
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
            l =>
            {
                string[] sa =
                {
                    @"    ^    ",
                    @"   / \   ",
                    @"  /   \  ",
                    @" /     \ ",
                    @"/       \",
                };
                return sa[l % sa.Length];
            },
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
                ca[l % ca.Length] = Util.RandomFrom("!?@#$%&*+=~XO§®¡©█■".ToCharArray());
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
        };

        private Func<int, string> _scrambler;
        private SftpClient _client = null!;
        private SftpFile[] _files = null!;
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
        private Stages _lastStage = Stages.Navigate;

        private SftpFile CurrentFile => this._files[Input.ScrollIndex];

        public OptionFTP() : base(Stages.Login)
        {
            // When FTP is initialized, empty the input string
            // and select a random scrambler to show.
            Input.ResetString(); ;
            this._scrambler = Util.RandomFrom(OptionFTP._scramblers);

            // If Download Path doesn't exist, create it.
            if (Directory.Exists(OptionFTP.DownloadPath))
                Directory.CreateDirectory(OptionFTP.DownloadPath);
        }

        public override void Loop()
        {
            switch (this.Stage)
            {
                case Stages.Login:
                    {
                        int consoleWidth = 16;
                        Window.ClearAndSetSize(consoleWidth, 5);
                        Window.PrintLine();
                        Window.PrintLine("     Login");
                        string scrambled = this._scrambler(Input.String.Length);
                        int textDepth = (consoleWidth / 2) + (scrambled.Length / 2);
                        Window.PrintLine(string.Format("{0," + textDepth + "}", scrambled));
                        Input.RequestLine(OptionFTP.MAX_LENGTH_PASSWORD,
                            new Keybind(() =>
                            {
                                this._client = new(OptionFTP.SERVER_IP, OptionFTP.SERVER_PORT, OptionFTP.SERVER_USER, Input.String);
                                Input.ResetString(); ;

                                try
                                {
                                    this._client.Connect();
                                    this.RefreshFiles();
                                    this.SetStage(Stages.Navigate);
                                }
                                catch (SshAuthenticationException)
                                {
                                    Window.PrintLine("Wrong password");
                                    Input.Get();
                                }
                                catch (SocketException)
                                {
                                    Window.PrintLine(" Can't connect");
                                    Input.Get();
                                }
                            }, key: ConsoleKey.Enter),
                            new Keybind(() => Input.ResetString(), key: ConsoleKey.Delete),
                            new Keybind(() => this.Quit(), key: ConsoleKey.Escape));
                    }
                    break;

                case Stages.Navigate:
                    {
                        this._lastStage = this.Stage;
                        int entryAmount = this._files.Length;
                        int adjustedMaxEntries = Math.Min(Window.SIZE_MAX.y - 14, OptionFTP.MAX_LIST_ENTRIES);
                        int consoleHeight = Math.Min(entryAmount, adjustedMaxEntries) + 14;
                        Window.Size = (OptionFTP.WIDTH, consoleHeight);
                        Cursor.Reset();
                        Window.PrintLine();
                        Window.PrintLine($" {$"index: ({Input.ScrollIndex + 1} / {entryAmount}) | path > '{this.Path}'",-98}");
                        Window.PrintLine();
                        Input.RequestScroll(
                            items: this._files,
                            getText: file =>
                            {
                                string s = file.Name;

                                if (file.IsDirectory)
                                    s += "/";

                                return s;
                            },
                            maxEntriesPerPage: adjustedMaxEntries,
                            exitKeybind: new(() =>
                            {
                                if (this.Path == string.Empty)
                                {
                                    Input.ScrollIndex = 0;
                                    this.Quit();
                                }
                                else
                                    this.PreviousDirectory();
                            }, "Exit", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => this.SetStage(Stages.Download), "Download", key: ConsoleKey.PageDown),
                                new(() => this.SetStage(Stages.Delete), "Delete", key: ConsoleKey.Delete),
                                new(() =>
                                {
                                    SftpFile currentFile = this.CurrentFile;

                                    if (currentFile.IsDirectory)
                                        this.Path += "/" + currentFile.Name;
                                    else
                                        this.SetStage(Stages.FileInteract);
                                }, "Select", key: ConsoleKey.Enter),
                                new(() => this.RefreshFiles(), "Refresh", key: ConsoleKey.F5),
                                new(() => this.PreviousDirectory(), "Back", key: ConsoleKey.Backspace)});
                    }
                    break;

                case Stages.FileInteract:
                    {
                        this._lastStage = this.Stage;
                        Window.ClearAndSetSize(OptionFTP.WIDTH, 8);
                        SftpFile file = this.CurrentFile;
                        new Input.Choice($"{file.FullName} | {file.Attributes.Size} bytes")
                            .Add(() => this.SetStage(Stages.Download), "Download", key: ConsoleKey.PageDown)
                            .Add(() => this.SetStage(Stages.Delete), "Delete", key: ConsoleKey.Delete)
                            .AddSpacer()
                            .Add(() => this.SetStage(Stages.Navigate), "Back", key: ConsoleKey.Escape)
                            .Request();
                        Window.Clear();
                    }
                    break;

                case Stages.Download:
                    {
                        // May hang while downloading files
                        this.Download(this.CurrentFile);
                        Window.Clear();
                        this.SetStage(this._lastStage);
                    }
                    break;

                case Stages.Delete:
                    {
                        Window.ClearAndSetSize(OptionFTP.WIDTH, 9);
                        Window.PrintLine();
                        SftpFile currentFile = this.CurrentFile;
                        Window.PrintLine($"  {currentFile.FullName}");
                        new Input.Choice("Are you sure you want to delete this file?")
                            .Add(() => this.Delete(currentFile), "yes", key: ConsoleKey.Enter)
                            .AddSpacer()
                            .Add(null!, "NO", key: ConsoleKey.Escape)
                            .Request();
                        this.SetStage(Stages.Navigate);
                    }
                    break;
            }
        }

        private void RefreshFiles()
        {
            this._files = this._client.ListDirectory(this.Path).OrderBy(x => !x.IsDirectory).ToArray();
            Input.ScrollIndex = 0;
            Window.Clear();
        }

        private void Download(SftpFile file)
        {
            if (file.IsDirectory)
                foreach (SftpFile subFile in this._client.ListDirectory(file.FullName))
                    this.Download(subFile);
            else
            {
                string path = OptionFTP.DownloadPath + file.FullName.Substring(1);
                DirectoryInfo? newFileDir = new FileInfo(path).Directory;

                if (newFileDir != null)
                    newFileDir.Create();

                using (Stream stream = File.Open(path, FileMode.Create))
                {
                    Window.ClearAndSetSize(OptionFTP.WIDTH, 7);
                    Window.Print("Downloading...", (2, 1));
                    Window.Print(file.FullName, (2, 3));
                    Window.Print("Bytes Downloaded", (2, 5));
                    this._client.DownloadFile(file.FullName, stream, l => Window.Print($"{l,-(OptionFTP.WIDTH - 30)}", (19, 5)));
                }
            }
        }

        private void Delete(SftpFile file)
        {
            try { this._client.Delete(file.FullName); }
            catch (SshException)
            {
                Window.ClearAndSetSize(21, 6);
                Window.PrintLines(2);
                Window.PrintLine("       Error:");
                Window.PrintLine("  Can't delete file");
                Input.Get();
            }

            this.RefreshFiles();
        }

        private void PreviousDirectory()
        {
            if (this.Path != string.Empty)
                this.Path = this.Path.Substring(0, this.Path.LastIndexOf('/'));
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
            if (this._client != null && this._client.IsConnected)
                this._client.Disconnect();

            base.Quit();
        }
    }
}
