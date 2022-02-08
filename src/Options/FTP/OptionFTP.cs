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

        private SftpFile CurrentFile => this._files[Input.ScrollIndex];

        public OptionFTP()
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
                case Stage.Login:
                    {
                        int consoleWidth = 16;
                        Util.ClearConsole(consoleWidth, 5);
                        Util.PrintLine();
                        Util.PrintLine("     Login");
                        string scrambled = this._scrambler(Input.String);
                        int textDepth = (consoleWidth / 2) + (scrambled.Length / 2);
                        Util.PrintLine(string.Format("{0," + textDepth + "}", scrambled));

                        switch (Input.Request(OptionFTP.MAX_LENGTH_PASSWORD))
                        {
                            case ConsoleKey.Enter:
                                {
                                    this._client = new(OptionFTP.IP, OptionFTP.PORT, OptionFTP.USER, Input.String);
                                    Input.String = string.Empty;

                                    try
                                    {
                                        this._client.Connect();
                                        this.RefreshFiles();
                                        this._stage = Stage.Navigate;
                                    }
                                    catch (SshAuthenticationException)
                                    {
                                        Util.PrintLine("Wrong password");
                                        Util.WaitForInput();
                                    }
                                    catch (SocketException)
                                    {
                                        Util.PrintLine(" Can't connect");
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
                        // TODO account for newly acquired Program.WINDOW_SIZE_MAX variable when displaying size of list
                        int entryAmount = this._files.Length;
                        int consoleHeight = Math.Min(entryAmount, OptionFTP.MAX_LIST_ENTRIES) + 13;
                        Util.SetConsoleSize(OptionFTP.WIDTH, consoleHeight);
                        Util.ResetTextCursor();
                        string header = $"index: ({Input.ScrollIndex + 1} / {entryAmount}) | path > '{this.Path}'";
                        Util.PrintLine();
                        Util.PrintLine($" {header,-98}");
                        Util.PrintLine();
                        SftpFile currentFile = this.CurrentFile;
                        Input.RequestScroll(this._files,
                            file =>
                            {
                                string s = file.Name;

                                if (file.IsDirectory)
                                    s += "/";

                                return s;
                            },
                            OptionFTP.MAX_LIST_ENTRIES,
                            new(() => this._stage = Stage.Download, "Download", key: ConsoleKey.PageDown),
                            new(() => this.Delete(currentFile), "Delete", key: ConsoleKey.Delete),
                            new(() =>
                            {
                                if (currentFile.IsDirectory)
                                    this.Path += "/" + currentFile.Name;
                                else
                                    this._stage = Stage.FileInteract;
                            }, "Select", key: ConsoleKey.Enter),
                            new(() => this.RefreshFiles(), "Refresh", key: ConsoleKey.F5),
                            new(() => this.PreviousDirectory(), "Back", key: ConsoleKey.Backspace),
                            new(() => this.Quit(), "Exit", key: ConsoleKey.Escape));
                    }
                    break;

                case Stage.FileInteract:
                    {
                        Util.ClearConsole(OptionFTP.WIDTH, 8);
                        SftpFile file = this.CurrentFile;
                        new Input.Option(file.FullName)
                            .Add(() => this._stage = Stage.Download, "Download", key: ConsoleKey.PageDown)
                            .Add(() => this.Delete(file), "Delete", key: ConsoleKey.Delete)
                            .AddSpacer()
                            .Add(() => this._stage = Stage.Navigate, "Back", key: ConsoleKey.Escape)
                            .Request();
                        Util.ClearConsole();
                    }
                    break;

                case Stage.Download:
                    {
                        Util.ClearConsole(OptionFTP.WIDTH, 5);
                        Util.PrintLine();
                        Util.PrintLine(" Downloading...");
                        SftpFile file = this.CurrentFile;
                        Util.PrintLine();
                        Util.PrintLine($"  {file.FullName}");
                        // May hang while downloading files
                        this.Download(file);
                        Util.ClearConsole();
                        this._stage = Stage.Navigate;
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
            {
                foreach (SftpFile subFile in this._client.ListDirectory(file.FullName))
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

        private void PreviousDirectory()
        {
            if (this.Path != string.Empty)
                this.Path = this.Path.Substring(0, this.Path.LastIndexOf('/'));
        }

        private enum Stage
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
