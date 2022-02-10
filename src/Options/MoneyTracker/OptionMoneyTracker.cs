using B.Inputs;
using B.Utils;

namespace B.Options.MoneyTracker
{
    public sealed class OptionMoneyTracker : Option
    {
        private const int MAX_TRANSACTIONS_PER_PAGE = 50;

        public static readonly string DirectoryPath = Program.DataPath + @"accounts\";

        private readonly Utils.List<Account> _accounts = new();
        private Account? _selectedAccount;
        private Transaction? _tempTransaction;
        private byte _tempTransactionState = 0;
        private Stage _stage = Stage.MainMenu;

        public OptionMoneyTracker()
        {
            if (!Directory.Exists(OptionMoneyTracker.DirectoryPath))
                Directory.CreateDirectory(OptionMoneyTracker.DirectoryPath);

            foreach (string filePath in Directory.GetFiles(OptionMoneyTracker.DirectoryPath))
                this.AddAccount(Path.GetFileNameWithoutExtension(filePath), true);
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
                            .Add(() => this._stage = Stage.Account, "Account", '1');

                        if (selected)
                            iob.Add(() => this._stage = Stage.Transaction, "Transaction", '2');

                        iob.AddExit(this)
                            .Request();
                    }
                    break;

                case Stage.Account:
                    {
                        if (this._selectedAccount != null)
                        {
                            Util.ClearConsole(24, 12);
                            Util.PrintLine();
                            Util.PrintLine("   Selected Account:");
                            Util.PrintLine($"  {this._selectedAccount.Name}");
                        }
                        else
                            Util.ClearConsole(24, 9);

                        new Input.Option("Account")
                            .Add(() => this._stage = Stage.Account_Create, "Create", '1')
                            .Add(() => this._stage = Stage.Account_Select, "Select", '2')
                            .Add(() => this._stage = Stage.Account_Remove, "Remove", '3')
                            .AddSpacer()
                            .Add(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stage.Account_Create:
                    {
                        Util.ClearConsole(42, 5);
                        Util.PrintLine();
                        Util.Print($"  New Account Name: {Input.String}");
                        ConsoleKey key = Input.RequestLine(20);

                        if (key == ConsoleKey.Enter)
                        {
                            if (Input.String.Length > 0)
                            {
                                string filePath = OptionMoneyTracker.DirectoryPath + Input.String;

                                if (!File.Exists(filePath))
                                {
                                    Account account = this.AddAccount(Path.GetFileNameWithoutExtension(filePath));
                                    this._selectedAccount = account;
                                    Util.PrintLines(2);
                                    Util.PrintLine($"  \"{Input.String}\" created!");
                                    Input.String = string.Empty;
                                    this._stage = Stage.Account;
                                }
                                else
                                {
                                    Util.PrintLines(2);
                                    Util.PrintLine("    Name already taken!");
                                }

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
                        Input.Option iob = new();

                        if (amountAccounts > 0)
                        {
                            for (int i = 0; i < amountAccounts; i++)
                            {
                                Account account = this._accounts[i];
                                iob.Add(() =>
                                {
                                    this._selectedAccount = account;
                                    this._stage = Stage.Account;
                                }, account.Name, keyChar: (char)('1' + i));
                            }

                            iob.AddSpacer();
                        }

                        iob.Add(() => this._stage = Stage.Account, "Back", key: ConsoleKey.Escape)
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
                        Input.Option iob = new("Remove Account");

                        if (amountAccounts > 0)
                        {
                            for (int i = 0; i < amountAccounts; i++)
                            {
                                Account account = this._accounts[i];
                                iob.Add(() =>
                                {
                                    if (this._selectedAccount == account)
                                        this._selectedAccount = null;

                                    this._accounts.Remove(account);
                                    account.Delete();
                                    this._stage = Stage.Account;
                                }, account.Name, keyChar: (char)('1' + i));
                            }

                            iob.AddSpacer();
                        }

                        iob.Add(() => this._stage = Stage.Account, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stage.Transaction:
                    {
                        Util.ClearConsole(20, 10);
                        new Input.Option("Transaction")
                            .Add(() =>
                            {
                                this._stage = Stage.Transaction_View;
                                Util.ClearConsole();
                            }, "View", '1')
                            .Add(() =>
                            {
                                Input.String = string.Empty;
                                this._tempTransaction = new();
                                this._tempTransactionState = 0;
                                this._stage = Stage.Transaction_Add;
                            }, "Add", '2')
                            .Add(() => this._stage = Stage.Transaction_Delete, "Delete", '3')
                            .Add(() => this._stage = Stage.Transaction_Edit, "Edit", '4')
                            .AddSpacer()
                            .Add(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stage.Transaction_View:
                    {
                        int consoleWidth = (Util.MAX_CHARS_DECIMAL * 2) + this._selectedAccount!.Decimals + 8;
                        int consoleHeight = Math.Min(this._selectedAccount.Transactions.Length, OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE) + 8;
                        Util.SetConsoleSize(consoleWidth, consoleHeight);
                        Util.ResetTextCursor();
                        Input.RequestScroll(this._selectedAccount.Transactions.Items,
                            transaction => string.Format("{0," + (Util.MAX_CHARS_DECIMAL + this._selectedAccount.Decimals + 1) + ":0." + Util.StringOf("0", this._selectedAccount.Decimals) + "} | {1," + Util.MAX_CHARS_DECIMAL + "}", transaction.Amount, transaction.Description),
                            OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE,
                            new(() => this._selectedAccount.Decimals++, "Increase Decimals", '+'),
                            new(() => this._selectedAccount.Decimals--, "Decrease Decimals", '-'),
                            new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this._stage = Stage.Transaction;
                            }, "Back", key: ConsoleKey.Escape));
                    }
                    break;

                case Stage.Transaction_Add:
                    {
                        Util.ClearConsole(Util.MAX_CHARS_DECIMAL + 4, 7);
                        Util.PrintLine();
                        Util.PrintLine("  Amount");
                        ConsoleKey key;

                        if (this._tempTransactionState == 0)
                        {
                            Util.Print($"  {Input.String}");
                            key = Input.RequestLine(Util.MAX_CHARS_DECIMAL);

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
                            Util.PrintLine($"  {this._tempTransaction!.Amount}");
                            Util.PrintLine();
                            Util.PrintLine("  Description:");
                            Util.Print($"  {Input.String}");
                            key = Input.RequestLine(Util.MAX_CHARS_DECIMAL);

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
                        Util.PrintLine();
                        Util.PrintLine("  Delete");
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
                account = new(name);
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
