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
        private const int MAX_LENGTH_PASSWORD = 50;
        private const int MAX_LIST_ENTRIES = 50;
        private const int WIDTH = 140;
        private const string IP = "***REMOVED***";
        private const int PORT = 22;
        private const string USER = "***REMOVED***";

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
            Input.String = string.Empty;
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
                        Util.ClearConsole(consoleWidth, 5);
                        Util.PrintLine();
                        Util.PrintLine("     Login");
                        string scrambled = this._scrambler(Input.String.Length);
                        int textDepth = (consoleWidth / 2) + (scrambled.Length / 2);
                        Util.PrintLine(string.Format("{0," + textDepth + "}", scrambled));

                        switch (Input.RequestLine(OptionFTP.MAX_LENGTH_PASSWORD))
                        {
                            case ConsoleKey.Enter:
                                {
                                    this._client = new(OptionFTP.IP, OptionFTP.PORT, OptionFTP.USER, Input.String);
                                    Input.String = string.Empty;

                                    try
                                    {
                                        this._client.Connect();
                                        this.RefreshFiles();
                                        this.Stage = Stages.Navigate;
                                    }
                                    catch (SshAuthenticationException)
                                    {
                                        Util.PrintLine("Wrong password");
                                        Util.GetKey();
                                    }
                                    catch (SocketException)
                                    {
                                        Util.PrintLine(" Can't connect");
                                        Util.GetKey();
                                    }
                                }
                                break;

                            case ConsoleKey.Delete: Input.String = string.Empty; break;

                            case ConsoleKey.Escape: this.Quit(); break;
                        }
                    }
                    break;

                case Stages.Navigate:
                    {
                        this._lastStage = this.Stage;
                        // TODO account for newly acquired Program.WINDOW_SIZE_MAX variable when displaying size of list
                        int entryAmount = this._files.Length;
                        int consoleHeight = Math.Min(entryAmount, OptionFTP.MAX_LIST_ENTRIES) + 14;
                        Util.SetConsoleSize(OptionFTP.WIDTH, consoleHeight);
                        Util.ResetTextCursor();
                        string header = $"index: ({Input.ScrollIndex + 1} / {entryAmount}) | path > '{this.Path}'";
                        Util.PrintLine();
                        Util.PrintLine($" {header,-98}");
                        Util.PrintLine();
                        Input.RequestScroll(
                            items: this._files,
                            getText: file =>
                            {
                                string s = file.Name;

                                if (file.IsDirectory)
                                    s += "/";

                                return s;
                            },
                            maxEntriesPerPage: OptionFTP.MAX_LIST_ENTRIES,
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this.Quit();
                            }, "Exit", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => this.Stage = Stages.Download, "Download", key: ConsoleKey.PageDown),
                                new(() => this.Stage = Stages.Delete, "Delete", key: ConsoleKey.Delete),
                                new(() =>
                                {
                                    SftpFile currentFile = this.CurrentFile;

                                    if (currentFile.IsDirectory)
                                        this.Path += "/" + currentFile.Name;
                                    else
                                        this.Stage = Stages.FileInteract;
                                }, "Select", key: ConsoleKey.Enter),
                                new(() => this.RefreshFiles(), "Refresh", key: ConsoleKey.F5),
                                new(() => this.PreviousDirectory(), "Back", key: ConsoleKey.Backspace)});
                    }
                    break;

                case Stages.FileInteract:
                    {
                        this._lastStage = this.Stage;
                        Util.ClearConsole(OptionFTP.WIDTH, 8);
                        SftpFile file = this.CurrentFile;
                        new Input.Option($"{file.FullName} | {file.Attributes.Size} bytes")
                            .Add(() => this.Stage = Stages.Download, "Download", key: ConsoleKey.PageDown)
                            .Add(() => this.Stage = Stages.Delete, "Delete", key: ConsoleKey.Delete)
                            .AddSpacer()
                            .Add(() => this.Stage = Stages.Navigate, "Back", key: ConsoleKey.Escape)
                            .Request();
                        Util.ClearConsole();
                    }
                    break;

                case Stages.Download:
                    {
                        // May hang while downloading files
                        this.Download(this.CurrentFile);
                        Util.ClearConsole();
                        this.Stage = this._lastStage;
                    }
                    break;

                case Stages.Delete:
                    {
                        Util.ClearConsole(OptionFTP.WIDTH, 9);
                        Util.PrintLine();
                        SftpFile currentFile = this.CurrentFile;
                        Util.PrintLine($"  {currentFile.FullName}");
                        new Input.Option("Are you sure you want to delete this file?")
                            .Add(() => this.Delete(currentFile), "yes", key: ConsoleKey.Enter)
                            .AddSpacer()
                            .Add(null!, "NO", key: ConsoleKey.Escape)
                            .Request();
                        this.Stage = Stages.Navigate;
                    }
                    break;
            }
        }

        private void RefreshFiles()
        {
            this._files = this._client.ListDirectory(this.Path).OrderBy(x => !x.IsDirectory).ToArray();
            Input.ScrollIndex = 0;
            Util.ClearConsole();
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
                    this._client.DownloadFile(file.FullName, stream, l =>
                    {
                        Util.ClearConsole(OptionFTP.WIDTH, 7);
                        Util.PrintLine();
                        Util.PrintLine(" Downloading...");
                        Util.PrintLine();
                        Util.PrintLine($"  {file.FullName}");
                        Util.PrintLine();
                        Util.PrintLine($"  Bytes downloaded: {l}");
                    });
            }
        }

        private void Delete(SftpFile file)
        {
            try { this._client.Delete(file.FullName); }
            catch (SshException)
            {
                Util.ClearConsole(21, 6);
                Util.PrintLines(2);
                Util.PrintLine("       Error:");
                Util.PrintLine("  Can't delete file");
                Util.GetKey();
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
