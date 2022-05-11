using System.Net.Sockets;
using B.Inputs;
using B.Utils;
using B.Utils.Extensions;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace B.Modules.Tools.FTP
{
    public sealed class ModuleFTP : Module<ModuleFTP.Stages>
    {
        #region Constants

        private const int MAX_LENGTH_PROFILE_INPUT = 20;
        private const int MAX_LENGTH_PASSWORD = 50;
        private const int WIDTH = 140;

        #endregion



        #region Universal Properties

        public static string Title => "FTP";
        public static string DirectoryPath => Program.DataPath + @"ftp\";
        public static string DownloadPath => Environment.CurrentDirectory + @"\download\";

        #endregion



        #region Private Properties

        private static Func<int, string>[] _scramblers => new Func<int, string>[]
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
                                                '\\', '\''}.Random();
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

        private SftpFile CurrentFile => _files[Input.ScrollIndex];
        private string CurrentPath
        {
            get => _currentPath;
            set
            {
                _currentPath = value;
                RefreshFiles();
            }
        }

        #endregion



        #region Private Variables

        private readonly Keybind[] _profileCreationKeybinds;

        private List<ServerProfile> _profiles = new();
        private string _currentPath = string.Empty;
        private ServerProfile _currentProfile = null!;
        private Func<int, string> _scrambler;
        private SftpClient _client = null!;
        private SftpFile[] _files = null!;

        #endregion



        #region Constructors

        public ModuleFTP() : base(Stages.Profile)
        {
            // Empty the input string
            Input.ResetString();
            // Select a random scrambler to show.
            _scrambler = _scramblers.Random();
            // If Download Path doesn't exist, create it.
            if (Directory.Exists(DownloadPath))
                Directory.CreateDirectory(DownloadPath);
            // Check save data
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
            else
            {
                foreach (var file in Directory.GetFiles(DirectoryPath))
                {
                    var profile = Data.Deserialize<ServerProfile>(file);
                    profile.Name = Path.GetFileNameWithoutExtension(file);
                    _profiles.Add(profile);
                }
            }
            // Create keybinds
            _profileCreationKeybinds = new Keybind[]
            {
                // ConsoleKey.Enter
                Keybind.Create(() =>
                {
                    switch (Stage)
                    {
                        case Stages.Profile_Create:
                            {
                                string name = Input.String;

                                if (!String.IsNullOrWhiteSpace(name))
                                {
                                    _currentProfile.Name = name;
                                    Input.ResetString();
                                    SetStage(Stages.Profile_Create_IP);
                                }
                            }
                            break;

                        case Stages.Profile_Create_IP:
                            {
                                string ip = Input.String;

                                if (!String.IsNullOrWhiteSpace(ip))
                                {
                                    _currentProfile.IP = ip;
                                    Input.ResetString();
                                    SetStage(Stages.Profile_Create_Port);
                                }
                            }
                            break;

                        case Stages.Profile_Create_Port:
                            {
                                int? port = Input.Int;

                                if (port.HasValue)
                                {
                                    _currentProfile.Port = port.Value;
                                    Input.ResetString();
                                    SetStage(Stages.Profile_Create_User);
                                }
                            }
                            break;

                        case Stages.Profile_Create_User:
                            {
                                string username = Input.String;

                                if (!String.IsNullOrWhiteSpace(username))
                                {
                                    _currentProfile.Username = username;
                                    Input.ResetString();
                                    _profiles.Add(_currentProfile);
                                    SetStage(Stages.Profile);
                                }
                            }
                            break;
                    }
                }, "Next", key: ConsoleKey.Enter),
                // ConsoleKey.Escape
                Keybind.Create(() =>
                {
                    switch (Stage)
                    {
                        case Stages.Profile_Create:
                            {
                                _currentProfile = null!;
                                SetStage(Stages.Profile);
                            }
                            break;

                        case Stages.Profile_Create_IP:
                            {
                                Input.String = _currentProfile.Name;
                                SetStage(Stages.Profile_Create);
                            }
                            break;

                        case Stages.Profile_Create_Port:
                            {
                                Input.String = _currentProfile.IP;
                                SetStage(Stages.Profile_Create_IP);
                            }
                            break;

                        case Stages.Profile_Create_User:
                            {
                                Input.String = _currentProfile.Port.ToString();
                                SetStage(Stages.Profile_Create_Port);
                            }
                            break;
                    }
                }, "Previous", key: ConsoleKey.Escape)
            };
        }

        #endregion



        #region Override Methods

        public sealed override void Loop()
        {
            switch (Stage)
            {
                case Stages.Profile:
                    {
                        bool hasProfiles = _profiles.Count > 0;
                        int consoleHeight = hasProfiles ? _profiles.Count + 12 : 10;
                        Window.SetSize(27, consoleHeight);
                        Cursor.y = 1;
                        List<Keybind> keybinds = new();
                        keybinds.Add(Keybind.Create(() =>
                        {
                            // Setup profile creation
                            _currentProfile = new();
                            SetStage(Stages.Profile_Create);
                        }, "Create", key: ConsoleKey.F1));

                        if (hasProfiles)
                        {
                            keybinds.Add(Keybind.CreateConfirmation(() =>
                            {
                                ServerProfile profile = _profiles[Input.ScrollIndex];
                                File.Delete(profile.Path);
                                _profiles.Remove(profile);
                                Input.ScrollIndex = 0;
                            }, $"Remove {_profiles[Input.ScrollIndex].Name}?", "Delete", key: ConsoleKey.Delete));
                        }

                        keybinds.Add(Keybind.Create(() =>
                        {
                            // Load profile
                            _currentProfile = _profiles[Input.ScrollIndex];
                            SetStage(Stages.Login);
                        }, "Login", key: ConsoleKey.Enter));

                        Input.RequestScroll(
                            items: _profiles,
                            getText: profile => profile.Name,
                            title: "Profiles",
                            exitKeybind: Keybind.CreateModuleExit(this),
                            extraKeybinds: keybinds.ToArray()
                        );
                    }
                    break;

                case Stages.Profile_Create:
                case Stages.Profile_Create_IP:
                case Stages.Profile_Create_Port:
                case Stages.Profile_Create_User:
                    {
                        Window.SetSize(31, 9);
                        PrintCreationInfo(1, "Title", Stage == Stages.Profile_Create ? Input.String : _currentProfile.Name);
                        if (Stage >= Stages.Profile_Create_IP)
                            PrintCreationInfo(2, "IP", Stage == Stages.Profile_Create_IP ? Input.String : _currentProfile.IP);
                        if (Stage >= Stages.Profile_Create_Port)
                            PrintCreationInfo(3, "Port", Stage == Stages.Profile_Create_Port ? Input.String : _currentProfile.Port);
                        if (Stage >= Stages.Profile_Create_User)
                            PrintCreationInfo(4, "User", Input.String);

                        // Request user input
                        Cursor.y = 6;
                        Input.RequestLine(MAX_LENGTH_PROFILE_INPUT, _profileCreationKeybinds);

                        // Local functions
                        void PrintCreationInfo(int line, string inputName, object inputValue)
                        {
                            Cursor.Set(2, line);
                            Window.Print($"{inputName}: {inputValue,-MAX_LENGTH_PROFILE_INPUT}");
                        }
                    }
                    break;

                case Stages.Login:
                    {
                        int consoleWidth = 16;
                        Window.SetSize(consoleWidth, 7);
                        Cursor.Set(5, 1);
                        Window.Print("Login");
                        string scrambled = _scrambler(Input.String.Length);
                        int textStart = (int)((consoleWidth / 2f) - (scrambled.Length / 2f));
                        Cursor.Set(textStart, 2);
                        Window.Print(scrambled);
                        Cursor.Set(0, 4);
                        Input.RequestLine(MAX_LENGTH_PASSWORD,
                            Keybind.Create(() =>
                            {
                                string profileUser = _currentProfile.Username;
                                string profileIP = _currentProfile.IP;
                                int profilePort = _currentProfile.Port;
                                _client = new(profileIP, profilePort, profileUser, Input.String);
                                _client.KeepAliveInterval = TimeSpan.FromSeconds(10);
                                Input.ResetString();

                                try
                                {
                                    _client.Connect();
                                    RefreshFiles();
                                    SetStage(Stages.Navigate);
                                }
                                catch (SshAuthenticationException e) { PrintError("Incorrect Auth", e); }
                                catch (SocketException e) { PrintError("Can't connect", e); }
                                catch (SshConnectionException e) { PrintError("Error", e); }
                            }, "Login", key: ConsoleKey.Enter),
                            Keybind.Create(() => SetStage(Stages.Profile), "Back", key: ConsoleKey.Escape)
                        );

                        // Local functions
                        void PrintError(string msg, Exception e)
                        {
                            if (Program.Settings.DebugMode)
                                Program.HandleException(e);
                            else
                            {
                                // TODO after adding descriptions to keybinds, might need to change where error is printed
                                Cursor.x = 1;
                                Window.Print(msg);
                                Input.Get();
                                Window.Clear();
                            }
                        }
                    }
                    break;

                case Stages.Navigate:
                    {
                        int entryAmount = _files.Length;
                        int consoleHeight = Math.Min(entryAmount, Input.MaxEntries) + 14;
                        Window.SetSize(WIDTH, consoleHeight);
                        List<Keybind> keybinds = new();
                        if (entryAmount != 0)
                        {
                            keybinds.Add(Keybind.Create(DownloadCurrent, "Download", key: ConsoleKey.PageDown));
                            keybinds.Add(CreateConfirmKeybindForCurrent());
                            keybinds.Add(Keybind.Create(() =>
                                {
                                    SftpFile currentFile = CurrentFile;

                                    if (currentFile.IsDirectory)
                                        CurrentPath += "/" + currentFile.Name;
                                    else
                                        SetStage(Stages.FileInteract);
                                }, "Select", key: ConsoleKey.Enter)
                            );
                        }
                        keybinds.Add(Keybind.Create(RefreshFiles, "Refresh", key: ConsoleKey.F5));
                        keybinds.Add(Keybind.Create(PreviousDirectory, "Back", key: ConsoleKey.Backspace));
                        Cursor.y = 1;
                        Input.RequestScroll(
                            items: _files,
                            getText: file =>
                            {
                                string s = file.Name;
                                if (file.IsDirectory)
                                    s += "/";
                                return s;
                            },
                            title: $"{$"index: ({Input.ScrollIndex + 1} / {entryAmount}) | path > '{CurrentPath}'",-98}",
                            exitKeybind: Keybind.Create(() =>
                            {
                                if (CurrentPath == string.Empty)
                                {
                                    Input.ScrollIndex = 0;
                                    SetStage(Stages.Profile);
                                }
                                else
                                    PreviousDirectory();
                            }, "Exit", key: ConsoleKey.Escape),
                            extraKeybinds: keybinds.ToArray()
                        );
                    }
                    break;

                case Stages.FileInteract:
                    {
                        SftpFile file = CurrentFile;
                        string title = $"{file.FullName} | {file.Attributes.Size} bytes";
                        Window.SetSize(title.Length + 6, 8);
                        Cursor.Set(2, 1);
                        Choice choice = new(title);
                        choice.AddKeybind(Keybind.Create(DownloadCurrent, "Download", key: ConsoleKey.PageDown));
                        choice.AddKeybind(CreateConfirmKeybindForCurrent());
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Navigate), "Back", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;
            }
        }

        public sealed override void Save() => _profiles.ForEach(profile => Data.Serialize(DirectoryPath + profile.Name, profile));

        public sealed override void Quit()
        {
            if (_client != null && _client.IsConnected)
                _client.Disconnect();

            Save();
            base.Quit();
        }

        #endregion



        #region Private Methods

        private void RefreshFiles()
        {
            // Get file list from server in current path with folders first.
            _files = _client.ListDirectory(CurrentPath).OrderBy(x => !x.IsDirectory).ToArray();
            Input.ScrollIndex = 0;
            Window.Clear();
        }

        private void DownloadCurrent()
        {
            Download(CurrentFile);
            Window.Clear();
        }

        private void Download(SftpFile file)
        {
            if (file.IsDirectory)
                _client.ListDirectory(file.FullName).ForEach(Download);
            else
            {
                string path = DownloadPath + file.FullName.Substring(1);
                DirectoryInfo? newFileDir = new FileInfo(path).Directory;

                if (newFileDir != null)
                    newFileDir.Create();

                using (Stream stream = File.Open(path, FileMode.Create))
                {
                    Window.Clear();
                    Window.SetSize(WIDTH, 7);
                    Cursor.Set(2, 1);
                    Window.Print("Downloading...");
                    Cursor.Set(2, 3);
                    Window.Print(file.FullName);
                    Cursor.Set(2, 5);
                    Window.Print("%");
                    _client.DownloadFile(file.FullName, stream, l =>
                    {
                        double percentage = (double)l / (double)file.Attributes.Size;
                        percentage *= 100d;
                        int progress = (int)Math.Ceiling(percentage);
                        Cursor.Set(4, 5);
                        Window.Print($"{progress,3}");
                    });
                    Window.Clear();
                }
            }
        }

        private Keybind CreateConfirmKeybindForCurrent() => Keybind.CreateConfirmation(DeleteCurrent, $"Are you sure you want to delete {CurrentFile.Name}?", "Delete", key: ConsoleKey.Delete);

        private void DeleteCurrent()
        {
            try { _client.Delete(CurrentFile.FullName); }
            catch (SshException)
            {
                Window.Clear();
                Window.SetSize(21, 6);
                Cursor.Set(7, 2);
                Window.Print("Error:");
                Cursor.Set(2, 3);
                Window.Print("Can't delete file");
                Input.Get();
            }

            SetStage(Stages.Navigate);
            RefreshFiles();
        }

        private void PreviousDirectory()
        {
            if (CurrentPath != string.Empty)
                CurrentPath = CurrentPath.Substring(0, CurrentPath.LastIndexOf('/'));
        }

        #endregion



        #region Enums

        public enum Stages
        {
            Profile,
            Profile_Create,
            Profile_Create_IP,
            Profile_Create_Port,
            Profile_Create_User,
            Login,
            Navigate,
            FileInteract,
        }

        #endregion
    }
}
