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
                ca[s.Length % ca.Length] = Util.RandomFrom("!?@#$%&*+=~XO§®¡©█■".ToCharArray());
                return new string(ca);
            },
            // Bar Fill
            s =>
            {
                char[] ca = new char[9];
                int fillDepth = s.Length % (ca.Length + 1);
                for (int i = 0; i < ca.Length; i++)
                {
                    if (i < fillDepth)
                        ca[i] = '#';
                    else
                        ca[i] = '-';
                }
                return new string(ca);
            },
        };

        private Func<string, string> _scrambler;
        private SftpClient _client = null!;
        private SftpFile[] _files = null!;
        private Stages _stage = Stages.Login;
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

        private SftpFile CurrentFile => this._files[Input.ScrollIndex];

        public OptionFTP() : base(Stages.Login)
        {
            Input.String = string.Empty;
            this._scrambler = Util.RandomFrom(OptionFTP._scramblers);
            DirectoryInfo downloadDir = new(OptionFTP.DownloadPath);

            if (!downloadDir.Exists)
                downloadDir.Create();
        }

        public override void Loop()
        {
            switch (this._stage)
            {
                case Stages.Login:
                    {
                        int consoleWidth = 16;
                        Util.ClearConsole(consoleWidth, 5);
                        Util.PrintLine();
                        Util.PrintLine("     Login");
                        string scrambled = this._scrambler(Input.String);
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
                                        this._stage = Stages.Navigate;
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
                        // TODO account for newly acquired Program.WINDOW_SIZE_MAX variable when displaying size of list
                        int entryAmount = this._files.Length;
                        int consoleHeight = Math.Min(entryAmount, OptionFTP.MAX_LIST_ENTRIES) + 14;
                        Util.SetConsoleSize(OptionFTP.WIDTH, consoleHeight);
                        Util.ResetTextCursor();
                        string header = $"index: ({Input.ScrollIndex + 1} / {entryAmount}) | path > '{this.Path}'";
                        Util.PrintLine();
                        Util.PrintLine($" {header,-98}");
                        Util.PrintLine();
                        SftpFile currentFile = this.CurrentFile;
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
                                new(() => this._stage = Stages.Download, "Download", key: ConsoleKey.PageDown),
                                new(() => this.Delete(currentFile), "Delete", key: ConsoleKey.Delete),
                                new(() =>
                                {
                                    if (currentFile.IsDirectory)
                                        this.Path += "/" + currentFile.Name;
                                    else
                                        this._stage = Stages.FileInteract;
                                }, "Select", key: ConsoleKey.Enter),
                                new(() => this.RefreshFiles(), "Refresh", key: ConsoleKey.F5),
                                new(() => this.PreviousDirectory(), "Back", key: ConsoleKey.Backspace)});
                    }
                    break;

                case Stages.FileInteract:
                    {
                        Util.ClearConsole(OptionFTP.WIDTH, 8);
                        SftpFile file = this.CurrentFile;
                        new Input.Option($"{file.FullName} | {file.Attributes.Size} bytes")
                            .Add(() => this._stage = Stages.Download, "Download", key: ConsoleKey.PageDown)
                            .Add(() => this.Delete(file), "Delete", key: ConsoleKey.Delete)
                            .AddSpacer()
                            .Add(() => this._stage = Stages.Navigate, "Back", key: ConsoleKey.Escape)
                            .Request();
                        Util.ClearConsole();
                    }
                    break;

                case Stages.Download:
                    {
                        Util.ClearConsole(OptionFTP.WIDTH, 5);
                        Util.PrintLine();
                        Util.PrintLine(" Downloading...");
                        Util.PrintLine();
                        SftpFile file = this.CurrentFile;
                        Util.PrintLine($"  {file.FullName}");
                        // May hang while downloading files
                        this.Download(file);
                        Util.ClearConsole();
                        this._stage = Stages.Navigate;
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
                    this._client.DownloadFile(file.FullName, stream);
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
        }

        public override void Quit()
        {
            if (this._client != null && this._client.IsConnected)
                this._client.Disconnect();

            base.Quit();
        }
    }
}
