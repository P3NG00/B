using B.Inputs;
using B.Utils;

namespace B.Options.MoneyTracker
{
    public sealed class OptionMoneyTracker : Option
    {
        public static readonly string DirectoryPath = Program.DirectoryPath + @"accounts\";

        private readonly Utils.List<Account> _accounts = new Utils.List<Account>();
        private Account? _selectedAccount;
        private Account.Transaction? _tempTransaction;
        private byte _tempTransactionState = 0;
        private Stage _stage = Stage.MainMenu;

        public OptionMoneyTracker()
        {
            if (!Directory.Exists(OptionMoneyTracker.DirectoryPath))
                Directory.CreateDirectory(OptionMoneyTracker.DirectoryPath);

            foreach (string filePath in Directory.GetFiles(OptionMoneyTracker.DirectoryPath))
                this.AddAccount(new FileInfo(filePath).Name, true);
        }

        public sealed override void Loop()
        {
            Console.Clear();

            switch (this._stage)
            {
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
                            .AddKeybind(new Keybind(() =>
                            {
                                this.Save();
                                this.Quit();
                            }, "Back", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;

                case Stage.Account:
                    {
                        if (this._selectedAccount != null)
                        {
                            Util.SetConsoleSize(24, 12);
                            Util.Print("Selected Account:", 3, linesBefore: 1);
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
                        Util.Print($"New Account Name: {Input.Str}", 2, false, 1);
                        ConsoleKey key = Input.RequestString(20);

                        if (key == ConsoleKey.Enter)
                        {
                            if (Input.Str.Length > 0)
                            {
                                string filePath = OptionMoneyTracker.DirectoryPath + Input.Str;

                                if (!File.Exists(filePath))
                                {
                                    Account account = this.AddAccount(new FileInfo(filePath).Name);
                                    this._selectedAccount = account;
                                    Util.Print($"\"{Input.Str}\" created!", 2, linesBefore: 2);
                                    Input.Str = string.Empty;
                                    this._stage = Stage.Account;
                                }
                                else
                                    Util.Print("Name already taken!", 4, linesBefore: 2);

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
                        int amountAccounts = this._accounts.Length;

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
                        int amountAccounts = this._accounts.Length;

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
                        Util.SetConsoleSize(20, 10);
                        new Input.Option("Transaction")
                            .AddKeybind(new Keybind(() => this._stage = Stage.Transaction_View, "View", '1'))
                            .AddKeybind(new Keybind(() =>
                            {
                                Input.Str = string.Empty;
                                this._tempTransaction = new Account.Transaction();
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
                        Util.SetConsoleSize(this._selectedAccount!.Decimals + 29, this._selectedAccount.Transactions.Length + 7);
                        this._selectedAccount.PrintTransactions();
                        new Input.Option()
                            .AddKeybind(new Keybind(() => this._selectedAccount.Decimals++, "Increase Decimals", '+'))
                            .AddKeybind(new Keybind(() => this._selectedAccount.Decimals--, "Decrease Decimals", '-'))
                            .AddSpacer()
                            .AddKeybind(new Keybind(() => this._stage = Stage.Transaction, "Back", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;

                case Stage.Transaction_Add:
                    {
                        Util.SetConsoleSize(20, 7);
                        Util.Print("Amount", 2, linesBefore: 1);
                        ConsoleKey key;

                        if (this._tempTransactionState == 0)
                        {
                            Util.Print(Input.Str, 2, false);
                            key = Input.RequestString(8);

                            if (key == ConsoleKey.Enter)
                            {
                                if (double.TryParse(Input.Str, out this._tempTransaction!.Amount))
                                {
                                    Input.Str = this._tempTransaction.Description;
                                    this._tempTransactionState = 1;
                                }
                            }
                            else if (key == ConsoleKey.Escape)
                            {
                                this._tempTransaction = null;
                                this._tempTransactionState = 0;
                                Input.Str = string.Empty;
                                this._stage = Stage.Transaction;
                            }
                        }
                        else
                        {
                            Util.Print(this._tempTransaction!.Amount, 2);
                            Util.Print("Description:", 2, linesBefore: 1);
                            Util.Print(Input.Str, 2, false);
                            key = Input.RequestString(16);

                            if (key == ConsoleKey.Enter)
                            {
                                if (Input.Str.Length > 0)
                                {
                                    this._tempTransaction.Description = Input.Str;
                                    this._selectedAccount!.Transactions.Add(this._tempTransaction);
                                    this._tempTransaction = null;
                                    this._tempTransactionState = 0;
                                    Input.Str = string.Empty;
                                    this._stage = Stage.Transaction;
                                }
                            }
                            else if (key == ConsoleKey.Escape)
                            {
                                this._tempTransaction.Description = Input.Str;
                                Input.Str = this._tempTransaction.Amount.ToString();
                                this._tempTransactionState = 0;
                            }
                        }
                    }
                    break;

                case Stage.Transaction_Delete:
                    {
                        Util.SetConsoleSize(31, this._selectedAccount!.Transactions.Length + 4);
                        Util.Print("Delete", 2, linesBefore: 1);
                        this._selectedAccount.PrintTransactions();

                        Util.WaitForInput();
                        // TODO add keybinds to delete a transaction
                        this._stage = Stage.Transaction;
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
    }
}
