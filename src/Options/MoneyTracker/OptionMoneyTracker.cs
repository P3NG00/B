using B.Inputs;
using B.Utils;

namespace B.Options.MoneyTracker
{
    public sealed class OptionMoneyTracker : Option
    {
        private const int MAX_TRANSACTIONS_PER_PAGE = 50;

        public static readonly string DirectoryPath = Program.DataPath + @"accounts\";

        private readonly Utils.List<Account> _accounts = new Utils.List<Account>();
        private Account? _selectedAccount;
        private Transaction? _tempTransaction;
        private byte _tempTransactionState = 0;
        private Stage _stage = Stage.MainMenu;
        private int Index
        {
            get => this._index;
            set
            {
                // Get last value
                int lastValue = this._index % OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE;
                // Update value
                this._index = Util.Clamp(value, 0, this._selectedAccount!.Transactions.Length - 1);
                // Get new value
                int newValue = this._index % OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE;
                // If crossing into new page, clear console
                int oneLess = OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE - 1;

                if ((lastValue == oneLess && newValue == 0) || (lastValue == 0 && newValue == oneLess))
                    Util.ClearConsole();
            }
        }
        private int _index = 0;

        public OptionMoneyTracker()
        {
            if (!Directory.Exists(OptionMoneyTracker.DirectoryPath))
                Directory.CreateDirectory(OptionMoneyTracker.DirectoryPath);

            foreach (string filePath in Directory.GetFiles(OptionMoneyTracker.DirectoryPath))
                this.AddAccount(new FileInfo(filePath).Name, true);
        }

        public sealed override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        int consoleHeight = 7;
                        bool selected = this._selectedAccount != null;

                        if (selected)
                            consoleHeight++;

                        Util.ClearConsole(20, consoleHeight);
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
                            Util.ClearConsole(24, 12);
                            Util.Print("Selected Account:", 3, linesBefore: 1);
                            Util.Print(this._selectedAccount.Name, 2);
                        }
                        else
                            Util.ClearConsole(24, 9);

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
                        Util.ClearConsole(42, 5);
                        Util.Print($"New Account Name: {Input.String}", 2, false, 1);
                        ConsoleKey key = Input.Request(20);

                        if (key == ConsoleKey.Enter)
                        {
                            if (Input.String.Length > 0)
                            {
                                string filePath = OptionMoneyTracker.DirectoryPath + Input.String;

                                if (!File.Exists(filePath))
                                {
                                    Account account = this.AddAccount(new FileInfo(filePath).Name);
                                    this._selectedAccount = account;
                                    Util.Print($"\"{Input.String}\" created!", 2, linesBefore: 2);
                                    Input.String = string.Empty;
                                    this._stage = Stage.Account;
                                }
                                else
                                    Util.Print("Name already taken!", 4, linesBefore: 2);

                                Util.WaitForInput();
                            }
                        }
                        else if (key == ConsoleKey.Escape)
                        {
                            Input.String = string.Empty;
                            this._stage = Stage.Account;
                        }
                    }
                    break;

                case Stage.Account_Select:
                    {
                        int consoleHeight = 3;
                        int amountAccounts = this._accounts.Length;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Util.ClearConsole(27, consoleHeight);
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
                        int amountAccounts = this._accounts.Length;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Util.ClearConsole(27, consoleHeight);
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
                        Util.ClearConsole(20, 10);
                        new Input.Option("Transaction")
                            .AddKeybind(new Keybind(() =>
                            {
                                this._stage = Stage.Transaction_View;
                                Util.ClearConsole();
                            }, "View", '1'))
                            .AddKeybind(new Keybind(() =>
                            {
                                Input.String = string.Empty;
                                this._tempTransaction = new Transaction();
                                this._tempTransactionState = 0;
                                this._stage = Stage.Transaction_Add;
                            }, "Add", '2'))
                            .AddKeybind(new Keybind(() => this._stage = Stage.Transaction_Delete, "Delete", '3'))
                            .AddKeybind(new Keybind(() => this._stage = Stage.Transaction_Edit, "Edit", '4'))
                            .AddSpacer()
                            .AddKeybind(new Keybind(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;

                case Stage.Transaction_View:
                    {
                        int consoleWidth = (Util.MAX_CHARS_DECIMAL * 2) + this._selectedAccount!.Decimals + 8;
                        int consoleHeight = Math.Min(this._selectedAccount.Transactions.Length, OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE) + 11;
                        Util.SetConsoleSize(consoleWidth, consoleHeight);
                        Console.SetCursorPosition(0, 0);
                        Util.Print();
                        decimal total = 0m;
                        int startIndex = this.Index - (this.Index % OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE);
                        int endIndex = Math.Min(startIndex + OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE, this._selectedAccount.Transactions.Length);

                        for (int i = startIndex; i < endIndex; i++)
                        {
                            Transaction transaction = this._selectedAccount.Transactions[i];
                            total += transaction.Amount;
                            string message = string.Format("{0," + (Util.MAX_CHARS_DECIMAL + this._selectedAccount.Decimals + 1) + ":0." + Util.StringOf("0", this._selectedAccount.Decimals) + "} | {1," + Util.MAX_CHARS_DECIMAL + "}", transaction.Amount, transaction.Description);

                            if (i == this.Index)
                                Util.Print($"> {message}", 1);
                            else
                                Util.Print($"{message} ", 2);
                        }

                        Util.Print("Total: " + total, 2, linesBefore: 1); // TODO fix total, now innacurately only showing total of transactions on page
                        Util.Print("Use Up/Down to navigate", 2, linesBefore: 1);
                        new Input.Option()
                            .AddKeybind(new Keybind(() => this.Index++, key: ConsoleKey.DownArrow))
                            .AddKeybind(new Keybind(() => this.Index--, key: ConsoleKey.UpArrow))
                            .AddKeybind(new Keybind(() => this._selectedAccount.Decimals++, "Increase Decimals", '+'))
                            .AddKeybind(new Keybind(() => this._selectedAccount.Decimals--, "Decrease Decimals", '-'))
                            .AddSpacer()
                            .AddKeybind(new Keybind(() =>
                            {
                                this.Index = 0;
                                this._stage = Stage.Transaction;
                            }, "Back", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;

                case Stage.Transaction_Add:
                    {
                        Util.ClearConsole(Util.MAX_CHARS_DECIMAL + 4, 7);
                        Util.Print("Amount", 2, linesBefore: 1);
                        ConsoleKey key;

                        if (this._tempTransactionState == 0)
                        {
                            Util.Print(Input.String, 2, false);
                            key = Input.Request(Util.MAX_CHARS_DECIMAL);

                            if (key == ConsoleKey.Enter)
                            {
                                decimal? amount = Input.Decimal;

                                if (amount.HasValue)
                                {
                                    this._tempTransaction!.Amount = amount.Value;
                                    Input.String = this._tempTransaction.Description;
                                    this._tempTransactionState = 1;
                                }
                            }
                            else if (key == ConsoleKey.Escape)
                            {
                                this._tempTransaction = null;
                                this._tempTransactionState = 0;
                                Input.String = string.Empty;
                                this._stage = Stage.Transaction;
                            }
                        }
                        else
                        {
                            Util.Print(this._tempTransaction!.Amount, 2);
                            Util.Print("Description:", 2, linesBefore: 1);
                            Util.Print(Input.String, 2, false);
                            key = Input.Request(Util.MAX_CHARS_DECIMAL);

                            if (key == ConsoleKey.Enter)
                            {
                                if (Input.String.Length > 0)
                                {
                                    this._tempTransaction.Description = Input.String;
                                    this._selectedAccount!.Transactions.Add(this._tempTransaction);
                                    this._tempTransaction = null;
                                    this._tempTransactionState = 0;
                                    Input.String = string.Empty;
                                    this._stage = Stage.Transaction;
                                }
                            }
                            else if (key == ConsoleKey.Escape)
                            {
                                this._tempTransaction.Description = Input.String;
                                Input.String = this._tempTransaction.Amount.ToString();
                                this._tempTransactionState = 0;
                            }
                        }
                    }
                    break;

                case Stage.Transaction_Delete:
                    {
                        Util.ClearConsole(31, this._selectedAccount!.Transactions.Length + 4);
                        Util.Print("Delete", 2, linesBefore: 1);
                        // this._selectedAccount.PrintTransactions(); // TODO

                        Util.WaitForInput();
                        // TODO add keybinds to delete a transaction
                        this._stage = Stage.Transaction;
                    }
                    break;

                case Stage.Transaction_Edit:
                    {
                        Util.ClearConsole();
                        this._stage = Stage.Transaction;
                        // TODO
                    }
                    break;
            }
        }

        public sealed override void Save()
        {
            foreach (Account account in this._accounts)
                account.Save();
        }

        private Account AddAccount(string name, bool deserialize = false)
        {
            Account account;

            if (deserialize)
                account = Util.Deserialize<Account>(OptionMoneyTracker.DirectoryPath + name)!;
            else
            {
                account = new Account(name);
                account.Save();
            }

            this._accounts.Add(account);
            return account;
        }

        private enum Stage
        {
            MainMenu,
            Account,
            Account_Create,
            Account_Select,
            Account_Remove,
            Transaction,
            Transaction_View,
            Transaction_Add,
            Transaction_Delete,
            Transaction_Edit,
        }

        public override void Quit()
        {
            this.Save();
            base.Quit();
        }
    }
}
