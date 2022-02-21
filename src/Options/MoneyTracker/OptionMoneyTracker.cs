using B.Inputs;
using B.Utils;

namespace B.Options.MoneyTracker
{
    public sealed class OptionMoneyTracker : Option<OptionMoneyTracker.Stages>
    {
        private const int MAX_TRANSACTIONS_PER_PAGE = 50;

        public static readonly string DirectoryPath = Program.DataPath + @"accounts\";

        private readonly Utils.List<Account> _accounts = new();
        private Account? _selectedAccount;
        private Transaction? _tempTransaction;
        private byte _tempTransactionState = 0; // TODO remove. create different enum for each state

        public OptionMoneyTracker() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(OptionMoneyTracker.DirectoryPath))
                Directory.CreateDirectory(OptionMoneyTracker.DirectoryPath);
            else
                foreach (string filePath in Directory.GetFiles(OptionMoneyTracker.DirectoryPath))
                    this.AddAccount(Path.GetFileNameWithoutExtension(filePath), true);
        }

        public override void Loop()
        {
            switch (this.Stage)
            {
                case Stages.MainMenu:
                    {
                        int consoleHeight = 7;
                        bool selected = this._selectedAccount != null;

                        if (selected)
                            consoleHeight++;

                        Util.ClearAndSetSize(20, consoleHeight);
                        Input.Choice iob = new Input.Choice("Money Tracker")
                            .Add(() => this.Stage = Stages.Account, "Account", '1');

                        if (selected)
                            iob.Add(() => this.Stage = Stages.Transaction, "Transaction", '2');

                        iob.AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.Account:
                    {
                        if (this._selectedAccount != null)
                        {
                            Util.ClearAndSetSize(24, 12);
                            Util.PrintLine();
                            Util.PrintLine("   Selected Account:");
                            Util.PrintLine($"  {this._selectedAccount.Name}");
                        }
                        else
                            Util.ClearAndSetSize(24, 9);

                        // TODO if account is not selected, show Create, Select, and Remove
                        // TODO if account is selected, show Create Transaction, View Transactions (in view, add interaction to Transactions in RequestScroll)

                        new Input.Choice("Account")
                            .Add(() => this.Stage = Stages.Account_Create, "Create", '1')
                            .Add(() => this.Stage = Stages.Account_Select, "Select", '2')
                            .Add(() => this.Stage = Stages.Account_Remove, "Remove", '3')
                            .AddSpacer()
                            .Add(() => this.Stage = Stages.MainMenu, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Account_Create:
                    {
                        Util.ClearAndSetSize(42, 5);
                        Util.PrintLine();
                        Util.Print($"  New Account Name: {Input.String}");
                        ConsoleKey key = Input.RequestLine(20).Key;

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
                                    this.Stage = Stages.Account;
                                }
                                else
                                {
                                    Util.PrintLines(2);
                                    Util.PrintLine("    Name already taken!");
                                }

                                Util.GetKey();
                            }
                        }
                        else if (key == ConsoleKey.Escape)
                        {
                            Input.String = string.Empty;
                            this.Stage = Stages.Account;
                        }
                    }
                    break;

                case Stages.Account_Select:
                    {
                        int consoleHeight = 3;
                        int amountAccounts = this._accounts.Length;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Util.ClearAndSetSize(27, consoleHeight);
                        Util.PrintLine();
                        Input.Choice iob = new();

                        if (amountAccounts > 0)
                        {
                            for (int i = 0; i < amountAccounts; i++)
                            {
                                Account account = this._accounts[i];
                                iob.Add(() =>
                                {
                                    this._selectedAccount = account;
                                    this.Stage = Stages.Account;
                                }, account.Name, keyChar: (char)('1' + i));
                            }

                            iob.AddSpacer();
                        }

                        iob.Add(() => this.Stage = Stages.Account, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Account_Remove:
                    {
                        int consoleHeight = 5;
                        int amountAccounts = this._accounts.Length;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Util.ClearAndSetSize(27, consoleHeight);
                        Input.Choice iob = new("Remove Account");

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
                                    this.Stage = Stages.Account;
                                }, account.Name, keyChar: (char)('1' + i));
                            }

                            iob.AddSpacer();
                        }

                        iob.Add(() => this.Stage = Stages.Account, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Transaction:
                    {
                        Util.ClearAndSetSize(20, 10);
                        new Input.Choice("Transaction")
                            .Add(() =>
                            {
                                this.Stage = Stages.Transaction_View;
                                Util.Clear();
                            }, "View", '1')
                            .Add(() =>
                            {
                                Input.String = string.Empty;
                                this._tempTransaction = new();
                                this._tempTransactionState = 0;
                                this.Stage = Stages.Transaction_Add;
                            }, "Add", '2')
                            .Add(() => this.Stage = Stages.Transaction_Delete, "Delete", '3')
                            .Add(() => this.Stage = Stages.Transaction_Edit, "Edit", '4')
                            .AddSpacer()
                            .Add(() => this.Stage = Stages.MainMenu, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Transaction_View:
                    {
                        int consoleWidth = (Util.MAX_CHARS_DECIMAL * 2) + this._selectedAccount!.Decimals + 8;
                        int consoleHeight = Math.Min(this._selectedAccount.Transactions.Length, OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE) + 9;
                        Util.SetConsoleSize(consoleWidth, consoleHeight);
                        Util.ResetTextCursor();
                        Util.PrintLine();
                        Input.RequestScroll(
                            items: this._selectedAccount.Transactions.Items,
                            getText: transaction => string.Format("{0," + (Util.MAX_CHARS_DECIMAL + this._selectedAccount.Decimals + 1) + ":0." + Util.StringOf('0', this._selectedAccount.Decimals) + "} | {1," + Util.MAX_CHARS_DECIMAL + "}", transaction.Amount, transaction.Description),
                            maxEntriesPerPage: OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE,
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this.Stage = Stages.Transaction;
                            }, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => this._selectedAccount.Decimals++, "Increase Decimals", '+'),
                                new(() => this._selectedAccount.Decimals--, "Decrease Decimals", '-')});
                    }
                    break;

                case Stages.Transaction_Add:
                    {
                        Util.ClearAndSetSize(Util.MAX_CHARS_DECIMAL + 4, 7);
                        Util.PrintLine();
                        Util.PrintLine("  Amount");
                        ConsoleKey key;

                        if (this._tempTransactionState == 0)
                        {
                            Util.Print($"  {Input.String}");
                            key = Input.RequestLine(Util.MAX_CHARS_DECIMAL).Key;

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
                                this.Stage = Stages.Transaction;
                            }
                        }
                        else
                        {
                            Util.PrintLine($"  {this._tempTransaction!.Amount}");
                            Util.PrintLine();
                            Util.PrintLine("  Description:");
                            Util.Print($"  {Input.String}");
                            key = Input.RequestLine(Util.MAX_CHARS_DECIMAL).Key;

                            if (key == ConsoleKey.Enter)
                            {
                                if (Input.String.Length > 0)
                                {
                                    this._tempTransaction.Description = Input.String;
                                    this._selectedAccount!.Transactions.Add(this._tempTransaction);
                                    this._tempTransaction = null;
                                    this._tempTransactionState = 0;
                                    Input.String = string.Empty;
                                    this.Stage = Stages.Transaction;
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

                case Stages.Transaction_Delete:
                    {
                        Util.ClearAndSetSize(31, this._selectedAccount!.Transactions.Length + 4);
                        Util.PrintLine();
                        Util.PrintLine("  Delete");
                        // this._selectedAccount.PrintTransactions(); // TODO

                        Util.GetKey();
                        // TODO add keybinds to delete a transaction
                        this.Stage = Stages.Transaction;
                    }
                    break;

                case Stages.Transaction_Edit:
                    {
                        Util.Clear();
                        this.Stage = Stages.Transaction;
                        // TODO
                    }
                    break;
            }
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

        public override void Save()
        {
            foreach (Account account in this._accounts)
                account.Save();
        }

        public override void Quit()
        {
            this.Save();
            base.Quit();
        }

        public enum Stages
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
