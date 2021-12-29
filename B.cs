using System.Collections;
using System.IO;
using System;

/* ================= *\
||                   ||
||        B's        ||
||                   ||
||  Created:         ||
||     2021.11.17    ||
||                   ||
||  Edited:          ||
||     2021.12.29    ||
||                   ||
\* ================= */

public class B
{
    public static void Main() { new B().Start(); }

    public static bool DebugMode { get { return B._debugMode; } }
    private static bool _debugMode = false;

    public static void ToggleDebugMode()
    {
        Util.ToggleBool(ref B._debugMode);
        Console.Clear();
    }

    private Option _option = null;
    private bool _running = true;

    private void Start()
    {
        Console.Title = "B";
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.TreatControlCAsInput = true;

        while (this._running)
        {
            try
            {
                if (this._option != null && this._option.IsRunning)
                    this._option.Loop();
                else
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 9);
                    new Input.Option("B's")
                        .AddKeybind(new Keybind(() => this._option = new Adventure(), "Adventure", '1'))
                        .AddKeybind(new Keybind(() => this._option = new NumberGuesser(), "Number Guesser", '2'))
                        .AddKeybind(new Keybind(() => this._option = new MoneyTracker(), "Money Tracker", '3'))
                        .AddSpacer()
                        .AddKeybind(new Keybind(() => this._running = false, "Quit", key: ConsoleKey.Escape))
                        .Request();
                }
            }
            catch (Exception e)
            {
                Util.SetConsoleSize(140, 30);
                Util.Print(e.ToString());
                Util.WaitForKey(ConsoleKey.F1);
                Console.Clear();
            }
        }
    }
}

public sealed class Adventure : Option
{
    private const string CHAR_EMPTY = "  ";
    private const string CHAR_PLAYER = "()";
    private const string CHAR_DOOR = "[]";
    private const string CHAR_COIN = "<>";
    private const string CHAR_WALL = "▓▓";
    private const string CHAR_INTERACTABLE = "░░";
    private const string CHAR_BORDER_HORIZONTAL = "==";
    private const string CHAR_BORDER_VERTICAL = "||";
    private const string CHAR_CORNER_A = "//";
    private const string CHAR_CORNER_B = @"\\";
    private const string MESSAGE_EMPTY = "...";

    public static Grid CurrentGrid;
    public static string Message = Adventure.MESSAGE_EMPTY;
    public static int Coins
    {
        get { return Adventure.coins; }
        set { Adventure.coins = Math.Max(0, value); }
    }

    private static int coins;
    private static Vector2 posPlayer;

    private Stage _stage = Stage.MainMenu;
    private int _speed = 1;

    private int Speed
    {
        get { return this._speed; }
        set { this._speed = Math.Max(1, value); }
    }

    public sealed override void Loop()
    {
        switch (this._stage)
        {
            case Stage.MainMenu:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    new Input.Option("Adventure")
                        .AddKeybind(new Keybind(() =>
                        {
                            this._stage = Stage.Game;
                            Grid.InitializeGrids();
                            Adventure.CurrentGrid = Grid.GridFirst;
                            Adventure.ResetPlayerPosition();
                            Adventure.Coins = 0;
                            this.Speed = 1;
                            Console.Clear();
                        }, "New Game", '1'))
                        .AddSpacer()
                        .AddKeybind(new Keybind(() => this.Quit(), "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Game:
                {
                    Console.SetCursorPosition(0, 0);
                    int consoleHeight = Adventure.CurrentGrid.Height + 14;

                    if (B.DebugMode)
                    {
                        Util.Print();
                        // Extra spaces are added to the end to clear leftover text
                        Util.Print(string.Format("{0,-7}", Adventure.CurrentGrid), 1);
                        Util.Print(string.Format("Pos: {0,-8}", Adventure.posPlayer), 1);
                        consoleHeight += 3;
                    }

                    Util.SetConsoleSize(Adventure.CurrentGrid.RealWidth + 8, consoleHeight);
                    Util.Print();
                    string borderHorizontal = Util.StringOf(Adventure.CHAR_BORDER_HORIZONTAL, Adventure.CurrentGrid.Width);
                    Util.Print(string.Format("{0}{1}{2}", Adventure.CHAR_CORNER_A, borderHorizontal, Adventure.CHAR_CORNER_B), 2);
                    string s;
                    Vector2 pos;

                    for (int y = Adventure.CurrentGrid.Height - 1; y >= 0; y--)
                    {
                        s = CHAR_BORDER_VERTICAL;

                        for (int x = 0; x < Adventure.CurrentGrid.Width; x++)
                        {
                            pos = new Vector2(x, y);

                            if (pos == Adventure.posPlayer)
                                s += Adventure.CHAR_PLAYER;
                            else if (Adventure.CurrentGrid.HasCoinAt(pos))
                                s += Adventure.CHAR_COIN;
                            else
                                s += Adventure.CurrentGrid.GetTile(pos).Chars;
                        }

                        Util.Print(s + Adventure.CHAR_BORDER_VERTICAL, 2);
                    }

                    Util.Print(string.Format("{0}{1}{2}", Adventure.CHAR_CORNER_B, borderHorizontal, Adventure.CHAR_CORNER_A), 2);
                    Util.Print();
                    Util.Print(string.Format("> {0}", Adventure.Message), 3);
                    Adventure.Message = string.Format("{0,-" + (Adventure.CurrentGrid.RealWidth - 7) + "}", Adventure.MESSAGE_EMPTY);
                    Util.Print();
                    string format = "{0,9}: {1,-5}";
                    Util.Print(string.Format(format, "Coins", Adventure.Coins));
                    Util.Print(string.Format(format, "Speed", this.Speed));
                    Util.Print();
                    Util.Print("Move) W A S D", 2);
                    Util.Print("Speed) + -", 1);
                    new Input.Option()
                        .AddKeybind(new Keybind(() => this.MovePlayer(Direction.Up), keyChar: 'w', key: ConsoleKey.NumPad8))
                        .AddKeybind(new Keybind(() => this.MovePlayer(Direction.Left), keyChar: 'a', key: ConsoleKey.NumPad4))
                        .AddKeybind(new Keybind(() => this.MovePlayer(Direction.Down), keyChar: 's', key: ConsoleKey.NumPad2))
                        .AddKeybind(new Keybind(() => this.MovePlayer(Direction.Right), keyChar: 'd', key: ConsoleKey.NumPad6))
                        .AddSpacer()
                        .AddKeybind(new Keybind(() => this._stage = Stage.MainMenu, "Quit", key: ConsoleKey.Escape))
                        .AddKeybind(new Keybind(() => this.Speed++, key: ConsoleKey.Add))
                        .AddKeybind(new Keybind(() => this.Speed--, key: ConsoleKey.Subtract))
                        .Request();
                }
                break;
        }
    }

    private void MovePlayer(Direction direction)
    {
        Vector2 newPos;
        Tile tile;
        bool stop = false;

        for (int i = 0; i < this.Speed && !stop; i++)
        {
            newPos = Adventure.posPlayer + direction.ToVector2();

            if (newPos.x >= 0 && newPos.x < Adventure.CurrentGrid.Width && newPos.y >= 0 && newPos.y < Adventure.CurrentGrid.Height)
            {
                tile = Adventure.CurrentGrid.GetTile(newPos);
                Adventure.CurrentGrid.Interact(newPos);
                stop = tile.StopMovement || tile.IsDoor;

                if (!stop)
                    Adventure.posPlayer = newPos;
            }
        }
    }

    public static void ResetPlayerPosition() { Adventure.posPlayer = new Vector2(Adventure.CurrentGrid.Width / 2, Adventure.CurrentGrid.Height / 2); }

    public sealed class Tile
    {
        private static readonly Dictionary<char, Tile> TileMap = new Dictionary<char, Tile>();

        static Tile()
        {
            Tile.TileMap.Add(' ', new Tile(Adventure.CHAR_EMPTY));
            Tile.TileMap.Add('c', new Tile(Adventure.CHAR_EMPTY, coin: true));
            Tile.TileMap.Add('d', new Tile(Adventure.CHAR_DOOR, door: true));
            Tile.TileMap.Add('w', new Tile(Adventure.CHAR_WALL, true));
            Tile.TileMap.Add('i', new Tile(Adventure.CHAR_INTERACTABLE, true, true));
        }

        public readonly string Chars;
        public readonly bool StopMovement;
        public readonly bool IsInteractable;
        public readonly bool IsCoin;
        public readonly bool IsDoor;

        public Tile(string chars, bool stopMovement = false, bool interactable = false, bool coin = false, bool door = false)
        {
            if (chars.Length != 2) { throw new ArgumentException("chars.Length != 2"); }
            this.Chars = chars;
            this.StopMovement = stopMovement;
            this.IsInteractable = interactable;
            this.IsCoin = coin;
            this.IsDoor = door;
        }

        public sealed override string ToString() { return string.Format("Tile: chars:'{0}', stopMovement: {1}, isInteractable: {2}", this.Chars, this.StopMovement, this.IsInteractable); }

        public static explicit operator Tile(char c)
        {
            try { return Tile.TileMap[c]; }
            catch (ArgumentException) { throw new ArgumentException(string.Format("Invalid tile character \"{0}\"", c)); }
        }
    }

    public sealed class Grid
    {
        public static Grid GridFirst { get { return Grid.gridFirst; } }
        public static Grid GridSecond { get { return Grid.gridSecond; } }

        private static Grid gridFirst;
        private static Grid gridSecond;

        public int RealWidth { get { return this._width * 2; } }
        public int Width { get { return this._width; } }
        public int Height { get { return this._height; } }

        private readonly Dictionary<Vector2, Action> _interactionList = new Dictionary<Vector2, Action>();
        private readonly Dictionary<Vector2, Grid> _doorList = new Dictionary<Vector2, Grid>();
        private readonly List<Vector2> _coinList = new List<Vector2>();
        private readonly Tile[][] _tileGrid;
        private readonly int _width;
        private readonly int _height;

        // Private Initialization Cache
        private readonly int _initInteractables = 0;
        private readonly int _initDoors = 0;
        private bool _seald = false;

        public Grid(string[] raw)
        {
            if (raw.Length > 0)
            {
                this._width = raw[0].Length;
                this._height = raw.Length;
                this._tileGrid = new Tile[this._height][];
                string str;
                char[] ca;
                Tile tile;

                for (int y = 0; y < _height; y++)
                {
                    str = raw[y];

                    if (str.Length == this._width)
                    {
                        this._tileGrid[y] = new Tile[this._width];
                        ca = str.ToCharArray();

                        for (int x = 0; x < _width; x++)
                        {
                            tile = (Tile)ca[x];
                            this._tileGrid[y][x] = tile;

                            if (tile.IsCoin)
                                this._coinList.Add(new Vector2(x, y));
                            else if (tile.IsInteractable)
                                this._initInteractables++;
                            else if (tile.IsDoor)
                                this._initDoors++;
                        }
                    }
                    else
                        throw new ArgumentException("Grid Init Error: Rows must be same length");
                }
            }
            else
                throw new ArgumentException("Grid Init Error: Must have at least one row");
        }

        public Tile GetTile(Vector2 pos) { return this.GetTile(pos.x, pos.y); }

        public Tile GetTile(int x, int y) { return this._tileGrid[y][x]; }

        public bool HasCoinAt(Vector2 pos) { return this._coinList.Contains(pos); }

        public void AddInteraction(Vector2 pos, Action action) { this.AddFeature(pos, action, "Interaction", tile => tile.IsInteractable, this._interactionList); }

        public void AddDoor(Vector2 pos, Grid grid) { this.AddFeature(pos, grid, "Door", tile => tile.IsDoor, this._doorList); }

        public void Interact(Vector2 pos)
        {
            if (this._seald)
            {
                Tile tile = this.GetTile(pos);

                if (tile.IsCoin && this._coinList.Contains(pos))
                {
                    this._coinList.Remove(pos);
                    Adventure.Coins++;
                    Adventure.Message = "You picked up a coin!";
                }

                if (tile.IsInteractable && this._interactionList.ContainsKey(pos))
                    this._interactionList[pos]();

                if (tile.IsDoor && this._doorList.ContainsKey(pos))
                {
                    Adventure.CurrentGrid = this._doorList[pos];
                    Adventure.ResetPlayerPosition();
                    Console.Clear();
                }
            }
            else
                throw new InvalidOperationException("Interact Error: Cannot interact with unsealed grid");
        }

        public void Seal()
        {
            if (this._initDoors != this._doorList.Count)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented doors");

            if (this._initInteractables != this._interactionList.Count)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented interactables");

            this._seald = true;
        }

        private void AddFeature<T>(Vector2 pos, T obj, string name, Func<Tile, bool> check, Dictionary<Vector2, T> list)
        {
            if (!this._seald)
            {
                if (check.Invoke(this.GetTile(pos)))
                {
                    if (!list.ContainsKey(pos))
                        list.Add(pos, obj);
                    else
                        throw new InvalidOperationException(string.Format("Add {0} Error: {1} already exists at {2}", name, name, pos));
                }
                else
                    throw new ArgumentException(string.Format("Add {0} Error: Tile is not {1} - {2}", name, name, pos));
            }
            else
                throw new InvalidOperationException(string.Format("Add {0} Error: Cannot add {1} to a sealed grid", name, name));
        }

        public sealed override string ToString() { return string.Format("Grid: {0}x{1}", this._width, this._height); }

        private static string[] CreateGrid(Vector2 dimensions)
        {
            string[] sa = new string[dimensions.y];
            string s = Util.StringOf(" ", dimensions.x);
            for (int i = 0; i < sa.Length; i++) sa[i] = s;
            return sa;
        }

        public static void InitializeGrids()
        {
            // ' ' | EMPTY
            // 'c' | COIN
            // 'd' | DOOR
            // 'w' | WALL
            // 'i' | TILE_INTERACTABLE

            // Grid First
            string[] sa = Grid.CreateGrid(new Vector2(15));
            sa[13] = " wwwwwwwwwwwww ";
            sa[12] = "  w         w  ";
            sa[11] = "       i       ";
            sa[7] = "   w       w  d";
            sa[3] = "   w   c   w   ";
            sa[1] = " wwwwwwwwwwwww ";
            Grid.gridFirst = new Grid(sa);
            Grid.gridFirst.AddInteraction(new Vector2(7, 11), () => Adventure.Message = "You touched it!");

            // Grid Second
            sa = Grid.CreateGrid(new Vector2(17, 21));
            sa[15] = "        d        ";
            sa[14] = " www         www ";
            sa[13] = " w             w ";
            sa[12] = " w     w w     w ";
            sa[11] = " w    w   w    w ";
            sa[10] = " w             w ";
            sa[9] = " w    w   w    w ";
            sa[8] = " w     w w     w ";
            sa[7] = " w             w ";
            sa[6] = " www         www ";
            Grid.gridSecond = new Grid(sa);

            // Add Doors after initializing each room
            Grid.gridFirst.AddDoor(new Vector2(14, 7), Grid.gridSecond);
            Grid.gridSecond.AddDoor(new Vector2(8, 15), Grid.gridFirst);

            // Seal Grids
            Grid.gridFirst.Seal();
            Grid.gridSecond.Seal();
        }
    }

    private enum Stage
    {
        MainMenu,
        Game,
    }
}

public sealed class NumberGuesser : Option
{
    private static readonly string[] _winMessages = new string[]
    {
        "Right on!",
        "Perfect!",
        "Correct!",
    };

    private Stage _stage = Stage.MainMenu;
    private int _numMax = 100;
    private int _guessNum;

    public sealed override void Loop()
    {
        Console.Clear();

        switch (this._stage)
        {
            case Stage.MainMenu:
                {
                    Util.SetConsoleSize(20, 8);
                    new Input.Option("Number Guesser")
                        .AddKeybind(new Keybind(() =>
                        {
                            this._guessNum = Util.Random.Next(this._numMax) + 1;
                            Input.Int = 0;
                            this._stage = Stage.Game;
                        }, "New Game", '1'))
                        .AddSpacer()
                        .AddKeybind(new Keybind(() => this._stage = Stage.Settings, "Settings", '9'))
                        .AddKeybind(new Keybind(() => this.Quit(), "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Game:
                {
                    string guessMessage = "Between 0 - " + this._numMax;
                    bool won = this._guessNum == Input.Int;
                    int consoleHeight = 7;

                    if (B.DebugMode)
                    {
                        Util.Print();
                        Util.Print(string.Format("Number: {0,-3}", this._guessNum), 1);
                        consoleHeight += 2;
                    }

                    Util.SetConsoleSize(20, consoleHeight);
                    Util.Print();
                    Util.Print(Input.Int, 2);
                    Util.Print();
                    guessMessage = Input.Int.ToString().Length == 0 ? "..." :
                        won ? NumberGuesser._winMessages[Util.Random.Next(NumberGuesser._winMessages.Length)] :
                            Input.Int < this._guessNum ? "too low..." : "TOO HIGH!!!";

                    Util.Print(guessMessage, 2);

                    if (won)
                    {
                        Util.WaitForInput();
                        this._stage = Stage.MainMenu;
                    }
                    else
                    {
                        Util.Print();
                        Util.Print("Enter a Number!", 1);

                        if (Input.RequestInt() == ConsoleKey.Escape)
                            this._stage = Stage.MainMenu;
                    }
                }
                break;

            case Stage.Settings:
                {
                    Util.SetConsoleSize(20, 7);
                    new Input.Option("Settings")
                        .AddKeybind(new Keybind(() =>
                        {
                            Input.Int = this._numMax;
                            this._stage = Stage.Settings_MaxNumber;
                        }, "Max Number", '1'))
                        .AddSpacer()
                        .AddKeybind(new Keybind(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Settings_MaxNumber:
                {
                    Util.SetConsoleSize(20, 5);
                    Util.Print();
                    Util.Print(string.Format("Max - {0}", Input.Int), 2);
                    Util.Print();
                    Util.Print("Enter Max Number", 2);
                    ConsoleKey key = Input.RequestInt();

                    if (key == ConsoleKey.Enter)
                    {
                        if (Input.Int < 1)
                            Input.Int = 1;

                        this._numMax = Input.Int;
                        this._stage = Stage.Settings;
                    }
                    else if (key == ConsoleKey.Escape)
                        this._stage = Stage.Settings;
                }
                break;
        }
    }

    private enum Stage
    {
        MainMenu,
        Game,
        Settings,
        Settings_MaxNumber,
    }
}

public sealed class MoneyTracker : Option
{
    public static readonly DirectoryInfo Directory = new DirectoryInfo(Environment.CurrentDirectory + @"\data");

    private readonly List<Account> _accounts = new List<Account>();
    private Account _selectedAccount = null;
    private Stage _stage = Stage.Initialization;

    public sealed override void Loop()
    {
        Console.Clear();

        switch (this._stage)
        {
            case Stage.Initialization:
                {
                    if (!MoneyTracker.Directory.Exists)
                        MoneyTracker.Directory.Create();

                    MoneyTracker.Directory.Attributes = FileAttributes.Hidden;

                    foreach (FileInfo file in MoneyTracker.Directory.GetFiles())
                        this._accounts.Add(new Account(file.Name));

                    this._stage = Stage.MainMenu;
                }
                break;

            case Stage.MainMenu:
                {
                    int consoleHeight = 7;
                    bool selected = this._selectedAccount != null;

                    if (selected)
                        consoleHeight++;

                    Util.SetConsoleSize(20, consoleHeight);
                    Input.Option iob = new Input.Option("Money Tracker")
                        .AddKeybind(new Keybind(() => this._stage = Stage.Account, "Account", '1'));

                    if (selected)
                        iob.AddKeybind(new Keybind(() => this._stage = Stage.Transaction, "Transaction", '2'));

                    iob.AddSpacer()
                        .AddKeybind(new Keybind(() => this.Quit(), "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Account:
                {
                    if (this._selectedAccount != null)
                    {
                        Util.SetConsoleSize(24, 12);
                        Util.Print();
                        Util.Print("Selected Account:", 3);
                        Util.Print(this._selectedAccount.Name, 2);
                    }
                    else
                        Util.SetConsoleSize(24, 9);

                    new Input.Option("Account")
                        .AddKeybind(new Keybind(() => this._stage = Stage.Account_Create, "Create", '1'))
                        .AddKeybind(new Keybind(() => this._stage = Stage.Account_Select, "Select", '2'))
                        .AddKeybind(new Keybind(() => this._stage = Stage.Account_Remove, "Remove", '3'))
                        .AddSpacer()
                        .AddKeybind(new Keybind(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Account_Create:
                {
                    Util.SetConsoleSize(42, 5);
                    Util.Print();
                    Util.Print(string.Format("New Account Name: {0}", Input.Str), 2, false);
                    ConsoleKey key = Input.RequestString();

                    if (key == ConsoleKey.Enter)
                    {
                        if (Input.Str.Length > 0)
                        {
                            Account account = new Account(Input.Str);
                            Util.Print();
                            Util.Print();

                            if (!account.Exists)
                            {
                                account.Load();
                                this._accounts.Add(account);
                                this._selectedAccount = account;
                                Util.Print(string.Format("\"{0}\" created!", account.Name), 2);
                                Input.Str = string.Empty;
                                this._stage = Stage.Account;
                            }
                            else
                                Util.Print("Name already taken!", 4);

                            Util.WaitForInput();
                        }
                    }
                    else if (key == ConsoleKey.Escape)
                    {
                        Input.Str = string.Empty;
                        this._stage = Stage.Account;
                    }
                }
                break;

            case Stage.Account_Select:
                {
                    int consoleHeight = 3;
                    int amountAccounts = this._accounts.Count;

                    if (amountAccounts > 0)
                        consoleHeight += amountAccounts + 1;

                    Util.SetConsoleSize(27, consoleHeight);
                    Input.Option iob = new Input.Option();

                    if (amountAccounts > 0)
                    {
                        for (int i = 0; i < amountAccounts; i++)
                        {
                            Account account = this._accounts[i];
                            iob.AddKeybind(new Keybind(() =>
                            {
                                this._selectedAccount = account;
                                this._stage = Stage.Account;
                            }, account.Name, keyChar: (char)('1' + i)));
                        }

                        iob.AddSpacer();
                    }

                    iob.AddKeybind(new Keybind(() => this._stage = Stage.Account, "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Account_Remove:
                {
                    int consoleHeight = 5;
                    int amountAccounts = this._accounts.Count;

                    if (amountAccounts > 0)
                        consoleHeight += amountAccounts + 1;

                    Util.SetConsoleSize(27, consoleHeight);
                    Input.Option iob = new Input.Option("Remove Account");

                    if (amountAccounts > 0)
                    {
                        for (int i = 0; i < amountAccounts; i++)
                        {
                            Account account = this._accounts[i];
                            iob.AddKeybind(new Keybind(() =>
                            {
                                if (this._selectedAccount == account)
                                    this._selectedAccount = null;

                                this._accounts.Remove(account);
                                account.Delete();
                                this._stage = Stage.Account;
                            }, account.Name, keyChar: (char)('1' + i)));
                        }

                        iob.AddSpacer();
                    }

                    iob.AddKeybind(new Keybind(() => this._stage = Stage.Account, "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Transaction:
                {
                    Util.SetConsoleSize(20, 9);
                    new Input.Option("Transaction")
                        .AddKeybind(new Keybind(() =>
                        {
                            Input.Str = string.Empty;
                            this._stage = Stage.Transaction_Add;
                        }, "Add", '1'))
                        .AddKeybind(new Keybind(() => this._stage = Stage.Transaction_Delete, "Delete", '2'))
                        .AddKeybind(new Keybind(() => this._stage = Stage.Transaction_Edit, "Edit", '3'))
                        .AddSpacer()
                        .AddKeybind(new Keybind(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Transaction_Add:
                {
                    Util.SetConsoleSize(20, 5);
                    Util.Print();
                    Util.Print(string.Format("Amount: {0}", Input.Str), 2, false);
                    ConsoleKey key = Input.RequestString();

                    if (key == ConsoleKey.Enter)
                    {
                        double amount;

                        if (double.TryParse(Input.Str, out amount))
                        {
                            // TODO
                        }
                        else
                        {
                            // TODO
                        }
                    }
                    else if (key == ConsoleKey.Escape)
                    {
                        // TODO
                    }
                }
                break;

            case Stage.Transaction_Delete:
                {
                    this._stage = Stage.Transaction;
                    // TODO
                }
                break;

            case Stage.Transaction_Edit:
                {
                    this._stage = Stage.Transaction;
                    // TODO
                }
                break;
        }
    }

    private sealed class Account
    {
        private const char SEPERATOR = '|';

        public readonly string Name;
        public bool Exists
        {
            get
            {
                this._file.Refresh();
                return this._file.Exists;
            }
        }

        private readonly List<Transaction> _transactions = new List<Transaction>();
        private readonly FileInfo _file;

        public Account(string name)
        {
            this.Name = name;
            this._file = new FileInfo(MoneyTracker.Directory.ToString() + @"\" + name);
        }

        public void Load()
        {
            if (this.Exists)
            {
                string[] line;
                int amount;

                using (StreamReader streamReader = new StreamReader(this._file.OpenRead()))
                {
                    while (!streamReader.EndOfStream)
                    {
                        line = streamReader.ReadLine().Split(Account.SEPERATOR);

                        if (line.Length != 2)
                            throw new ArgumentException("String needs 2 comma-seperated values");

                        if (!int.TryParse(line[0], out amount))
                            throw new ArgumentException("Value is not a number");

                        this._transactions.Add(new Transaction(amount, line[1]));
                    }
                }
            }
            else
                this._file.Create().Close();
        }

        public void Save()
        {
            this._file.Refresh();

            using (StreamWriter streamWriter = this.Exists ? new StreamWriter(this._file.OpenWrite()) : this._file.CreateText())
                foreach (Transaction transaction in this._transactions)
                    streamWriter.WriteLine(string.Format("{0}{1}{2}", transaction.Amount, Account.SEPERATOR, transaction.Description));
        }

        public void Delete()
        {
            if (this.Exists)
                this._file.Delete();
        }

        private sealed class Transaction
        {
            public readonly string Description;
            public readonly double Amount;

            public Transaction(double amount, string description)
            {
                this.Amount = amount;
                this.Description = description;
            }
        }
    }

    private enum Stage
    {
        Initialization,
        MainMenu,
        Account,
        Account_Create,
        Account_Select,
        Account_Remove,
        Transaction,
        Transaction_Add,
        Transaction_Delete,
        Transaction_Edit,
    }
}

public abstract class Option
{
    private bool _running = true;
    public bool IsRunning { get { return this._running; } }

    public void Quit() { this._running = false; }

    public abstract void Loop();
}

public enum Direction
{
    Up,
    Left,
    Down,
    Right,
}

public static class DirectionToVector2
{
    public static Vector2 ToVector2(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Vector2.Up;
            case Direction.Left: return Vector2.Left;
            case Direction.Down: return Vector2.Down;
            case Direction.Right: return Vector2.Right;
        }

        return Vector2.Zero;
    }
}

public static class Input
{
    private const int MAX_STRING_LENGTH = 20;

    public static string Str = string.Empty;
    public static int Int = 0;

    public static ConsoleKey RequestString()
    {
        return Input.Request(ref Input.Str,
            (keyInfo, str0) =>
            {
                if (str0.Length < Input.MAX_STRING_LENGTH)
                    str0 += keyInfo.KeyChar;

                return str0;
            },
            (str0) => str0.Substring(0, Math.Max(0, str0.Length - 1)));
    }

    public static ConsoleKey RequestInt()
    {
        return Input.Request(ref Input.Int,
            (keyInfo, num0) =>
            {
                string numStr = num0.ToString();
                numStr += keyInfo.KeyChar;
                int num1 = num0;

                if (!int.TryParse(numStr, out num0))
                    num0 = num1;

                return num0;
            },
            (num0) =>
            {
                string numStr = num0.ToString();
                numStr = numStr.Substring(0, Math.Max(0, numStr.Length - 1));
                int.TryParse(numStr, out num0);
                return num0;
            });
    }

    private static ConsoleKey Request<T>(ref T tObj, Func<ConsoleKeyInfo, T, T> funcDefault, Func<T, T> funcBackspace)
    {
        ConsoleKeyInfo keyInfo = Util.GetInput();

        if (keyInfo.Key == ConsoleKey.Backspace)
            tObj = funcBackspace.Invoke(tObj);
        else if (keyInfo.Key == ConsoleKey.F12)
            B.ToggleDebugMode();
        else if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape)
            tObj = funcDefault.Invoke(keyInfo, tObj);

        return keyInfo.Key;
    }

    public sealed class Option
    {
        private readonly List<Keybind> _keybinds = new List<Keybind>();
        private string _message;

        public Option(string message = null) { this._message = message; }

        public Option AddKeybind(Keybind keybind)
        {
            this._keybinds.Add(keybind);
            return this;
        }

        public Option AddSpacer() { return this.AddKeybind(null); }

        public void Request()
        {
            // Add Debug Keybind
            this.AddKeybind(new Keybind(() => B.ToggleDebugMode(), key: ConsoleKey.F12));

            if (this._message != null)
            {
                Util.Print();
                Util.Print(this._message, 2);
            }

            bool printLine = true;
            string s;

            foreach (Keybind keybind in this._keybinds)
            {
                // If keybind is null, add spacer in display
                // If keybind description is null, don't display option
                if (keybind != null)
                {
                    if (keybind.Description != null)
                    {
                        s = keybind.KeyChar == Util.NULLCHAR ? keybind.Key.ToString() : keybind.KeyChar.ToString();

                        if (printLine)
                        {
                            printLine = false;
                            Util.Print();
                        }

                        Util.Print(string.Format("{0}) {1}", s, keybind.Description), 1);
                    }
                }
                else if (!printLine)
                    printLine = true;
            }

            ConsoleKeyInfo inputKeyInfo = Util.GetInput();

            foreach (Keybind keybind in this._keybinds)
            {
                if (keybind != null && (keybind.Key == inputKeyInfo.Key || (keybind.KeyChar != Util.NULLCHAR && keybind.KeyChar == inputKeyInfo.KeyChar)))
                {
                    keybind.Action.Invoke();
                    break;
                }
            }
        }
    }
}

public sealed class Keybind
{
    public readonly ConsoleKey Key;
    public readonly char KeyChar;
    public readonly string Description;
    public readonly Action Action;

    public Keybind(Action action, string description = null, char keyChar = Util.NULLCHAR, ConsoleKey key = default(ConsoleKey))
    {
        this.KeyChar = keyChar;
        this.Key = key;
        this.Description = description;
        this.Action = action;
    }
}

public sealed class List<T>
{
    private T[] _items = new T[0];

    public int Count { get { return this._items.Length; } }

    public void Add(T t)
    {
        T[] newItems = new T[this._items.Length + 1];
        Array.Copy(this._items, newItems, this._items.Length);
        newItems[this._items.Length] = t;
        this._items = newItems;
    }

    public void Remove(T t)
    {
        T[] newItems = new T[this._items.Length - 1];
        int index = 0;

        for (int i = 0; i < this._items.Length; i++)
        {
            if (this._items[i].Equals(t))
                continue;

            newItems[index] = this._items[i];
            index++;
        }

        this._items = newItems;
    }

    public bool Contains(T t)
    {
        foreach (T item in this._items)
        {
            if (item.Equals(t))
                return true;
        }

        return false;
    }

    public IEnumerator GetEnumerator() { return this._items.GetEnumerator(); }

    public T this[int index] { get { return this._items[index]; } }
}

public sealed class Dictionary<T, V>
{
    private Tuple<T, V>[] _tuples = new Tuple<T, V>[0];

    public int Count { get { return this._tuples.Length; } }

    public void Add(T t, V v)
    {
        Tuple<T, V>[] newTuples = new Tuple<T, V>[this._tuples.Length + 1];
        Array.Copy(this._tuples, newTuples, this._tuples.Length);
        newTuples[this._tuples.Length] = new Tuple<T, V>(t, v);
        this._tuples = newTuples;
    }

    public bool ContainsKey(T t)
    {
        foreach (Tuple<T, V> tuple in this._tuples)
        {
            if (tuple.Item1.Equals(t))
                return true;
        }

        return false;
    }

    public V this[T t]
    {
        get { return this._tuples[this.GetIndex(t)].Item2; }
        set { this._tuples[this.GetIndex(t)] = new Tuple<T, V>(t, value); }
    }

    private int GetIndex(T t)
    {
        for (int i = 0; i < this._tuples.Length; i++)
            if (this._tuples[i].Item1.Equals(t))
                return i;

        throw new ArgumentException("Key not found");
    }
}

public sealed class Vector2
{
    public static readonly Vector2 Up = new Vector2(0, 1);
    public static readonly Vector2 Left = new Vector2(-1, 0);
    public static readonly Vector2 Right = new Vector2(1, 0);
    public static readonly Vector2 Down = new Vector2(0, -1);
    public static readonly Vector2 Zero = new Vector2(0);

    public int x = 0;
    public int y = 0;

    public Vector2(int size) : this(size, size) { }

    public Vector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override int GetHashCode() { return (this.x * this.y).GetHashCode(); }

    public override bool Equals(object obj) { return obj is Vector2 && this == (Vector2)obj; }

    public static Vector2 operator +(Vector2 vecA, Vector2 vecB) { return new Vector2(vecA.x + vecB.x, vecA.y + vecB.y); }

    public static bool operator ==(Vector2 vecA, Vector2 vecB) { return vecA.x == vecB.x && vecA.y == vecB.y; }

    public static bool operator !=(Vector2 vecA, Vector2 vecB) { return !(vecA == vecB); }

    public sealed override string ToString() { return string.Format("({0}, {1})", this.x, this.y); }
}

public static class Util
{
    public const char NULLCHAR = default(char);

    public static readonly Random Random = new Random();

    public static void WaitForInput() { Util.GetInput(); }

    public static ConsoleKeyInfo GetInput() { return Console.ReadKey(true); }

    public static void WaitForKey(ConsoleKey key, bool displayMessage = true)
    {
        if (displayMessage)
        {
            Util.Print();
            Util.Print(string.Format("Press {0} to continue...", key), 0);
        }

        bool keyPressed = false;

        while (!keyPressed)
            if (Util.GetInput().Key == key)
                keyPressed = true;
    }

    public static string StringOf(string str, int repeat)
    {
        string s = string.Empty;

        for (int i = 0; i < repeat; i++)
            s += str;

        return s;
    }

    public static void Print(object message = null, int offsetLeft = 0, bool newLine = true)
    {
        string messageStr = message == null ? string.Empty : message.ToString();
        messageStr = string.Format("{0," + (messageStr.Length + offsetLeft).ToString() + "}", messageStr);

        if (newLine)
            Console.WriteLine(messageStr);
        else
            Console.Write(messageStr);
    }

    public static void SetConsoleSize(int width, int height)
    {
        Console.SetWindowSize(width, height);
        Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
        Console.SetWindowSize(width, height);
    }

    public static void ToggleBool(ref bool b) { b = !b; }
}
