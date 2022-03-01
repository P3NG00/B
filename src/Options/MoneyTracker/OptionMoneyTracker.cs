using B.Inputs;
using B.Utils;

namespace B.Options.MoneyTracker
{
    public sealed class OptionMoneyTracker : Option<OptionMoneyTracker.Stages>
    {
        public const string Title = "Money Tracker";

        private const int MAX_TRANSACTIONS_PER_PAGE = 50;

        public static readonly string DirectoryPath = Program.DataPath + @"accounts\";

        private readonly Utils.List<Account> _accounts = new();
        private Transaction? _tempTransaction;
        private Account? _selectedAccount;

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

                        Window.ClearAndSetSize(20, consoleHeight);
                        Input.Choice iob = new Input.Choice(OptionMoneyTracker.Title)
                            .Add(() => this.SetStage(Stages.Account), "Account", '1');

                        if (selected)
                            iob.Add(() => this.SetStage(Stages.Transaction), "Transaction", '2');

                        iob.AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.Account:
                    {
                        if (this._selectedAccount != null)
                        {
                            Window.ClearAndSetSize(24, 12);
                            Window.PrintLine();
                            Window.PrintLine("   Selected Account:");
                            Window.PrintLine($"  {this._selectedAccount.Name}");
                        }
                        else
                            Window.ClearAndSetSize(24, 9);

                        new Input.Choice("Account")
                            .Add(() => this.SetStage(Stages.Account_Create), "Create", '1')
                            .Add(() => this.SetStage(Stages.Account_Select), "Select", '2')
                            .Add(() => this.SetStage(Stages.Account_Remove), "Remove", '3')
                            .AddSpacer()
                            .Add(() => this.SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Account_Create:
                    {
                        Window.ClearAndSetSize(42, 5);
                        Window.PrintLine();
                        Window.Print($"  New Account Name: {Input.String}");
                        Input.RequestLine(20,
                            new Keybind(() =>
                            {
                                if (Input.String.Length > 0)
                                {
                                    string filePath = OptionMoneyTracker.DirectoryPath + Input.String;

                                    if (!File.Exists(filePath))
                                    {
                                        Account account = this.AddAccount(Path.GetFileNameWithoutExtension(filePath));
                                        this._selectedAccount = account;
                                        Window.PrintLines(2);
                                        Window.PrintLine($"  \"{Input.String}\" created!");
                                        Input.ResetString(); ;
                                        this.SetStage(Stages.Account);
                                    }
                                    else
                                    {
                                        Window.PrintLines(2);
                                        Window.PrintLine("    Name already taken!");
                                    }

                                    Input.Get();
                                }
                            }, key: ConsoleKey.Enter),
                            new Keybind(() =>
                            {
                                Input.ResetString(); ;
                                this.SetStage(Stages.Account);
                            }, key: ConsoleKey.Escape));
                    }
                    break;

                case Stages.Account_Select:
                    {
                        int consoleHeight = 3;
                        int amountAccounts = this._accounts.Length;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Window.ClearAndSetSize(27, consoleHeight);
                        Window.PrintLine();
                        Input.Choice iob = new();

                        if (amountAccounts > 0)
                        {
                            for (int i = 0; i < amountAccounts; i++)
                            {
                                Account account = this._accounts[i];
                                iob.Add(() =>
                                {
                                    this._selectedAccount = account;
                                    this.SetStage(Stages.Account);
                                }, account.Name, keyChar: (char)('1' + i));
                            }

                            iob.AddSpacer();
                        }

                        iob.Add(() => this.SetStage(Stages.Account), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Account_Remove:
                    {
                        int consoleHeight = 5;
                        int amountAccounts = this._accounts.Length;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Window.ClearAndSetSize(27, consoleHeight);
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
                                    this.SetStage(Stages.Account);
                                }, account.Name, keyChar: (char)('1' + i));
                            }

                            iob.AddSpacer();
                        }

                        iob.Add(() => this.SetStage(Stages.Account), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Transaction:
                    {
                        Window.ClearAndSetSize(20, 10);
                        new Input.Choice("Transaction")
                            .Add(() =>
                            {
                                this.SetStage(Stages.Transaction_View);
                                Window.Clear();
                            }, "View", '1')
                            .Add(() =>
                            {
                                Input.ResetString(); ;
                                this._tempTransaction = new();
                                this.SetStage(Stages.Transaction_Add_Amount);
                            }, "Add", '2')
                            .Add(() => this.SetStage(Stages.Transaction_Delete), "Delete", '3')
                            .Add(() => this.SetStage(Stages.Transaction_Edit), "Edit", '4')
                            .AddSpacer()
                            .Add(() => this.SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Transaction_View:
                    {
                        Window.Size = new(
                            (Util.MAX_CHARS_DECIMAL * 2) + this._selectedAccount!.Decimals + 9,
                            Math.Min(this._selectedAccount.Transactions.Length, OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE) + 9);
                        Cursor.Reset();
                        Window.PrintLine();
                        Input.RequestScroll(
                            items: this._selectedAccount.Transactions.Items,
                            getText: transaction => string.Format("{0," + (Util.MAX_CHARS_DECIMAL + this._selectedAccount.Decimals + 1) + ":0." + Util.StringOf('0', this._selectedAccount.Decimals) + "} | {1," + Util.MAX_CHARS_DECIMAL + "}", transaction.Amount, transaction.Description),
                            maxEntriesPerPage: OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE,
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this.SetStage(Stages.Transaction);
                            }, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => this._selectedAccount.Decimals++, "Increase Decimals", '+'),
                                new(() => this._selectedAccount.Decimals--, "Decrease Decimals", '-')});
                    }
                    break;

                case Stages.Transaction_Add_Amount: this.ShowTransactionStage(); break;

                case Stages.Transaction_Add_Description: this.ShowTransactionStage(); break;

                case Stages.Transaction_Delete:
                    {
                        Window.ClearAndSetSize(31, this._selectedAccount!.Transactions.Length + 4);
                        // Util.PrintLine();
                        // Util.PrintLine("  Delete");
                        // this._selectedAccount.PrintTransactions(); // TODO

                        // Util.GetKey();
                        // TODO add keybinds to delete a transaction
                        this.SetStage(Stages.Transaction);
                    }
                    break;

                case Stages.Transaction_Edit:
                    {
                        Window.Clear();
                        this.SetStage(Stages.Transaction);
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

        private void ShowTransactionStage()
        {
            Window.ClearAndSetSize(4 + Util.MAX_CHARS_DECIMAL, 7);
            Window.PrintLine();
            Window.PrintLine("  Amount:");

            if (this.Stage == Stages.Transaction_Add_Amount)
            {
                Window.Print($"  {Input.String}");
                Input.RequestLine(Util.MAX_CHARS_DECIMAL,
                    new Keybind(() =>
                    {
                        decimal? amount = Input.Decimal;

                        if (amount.HasValue)
                        {
                            this._tempTransaction!.Amount = amount.Value;
                            Input.String = this._tempTransaction.Description;
                            this.SetStage(Stages.Transaction_Add_Description);
                        }
                    }, key: ConsoleKey.Enter),
                    new Keybind(() =>
                    {
                        this._tempTransaction = null;
                        Input.ResetString(); ;
                        this.SetStage(Stages.Transaction);
                    }, key: ConsoleKey.Escape));
            }
            else
            {
                Window.PrintLine($"  {this._tempTransaction!.Amount}");
                Window.PrintLine();
                Window.PrintLine("  Description:");
                Window.Print($"  {Input.String}");
                Input.RequestLine(Util.MAX_CHARS_DECIMAL,
                    new Keybind(() =>
                    {
                        if (Input.String.Length > 0)
                        {
                            this._tempTransaction.Description = Input.String.Trim();
                            this._selectedAccount!.Transactions.Add(this._tempTransaction);
                            this._tempTransaction = null;
                            Input.ResetString(); ;
                            this.SetStage(Stages.Transaction);
                        }
                    }, key: ConsoleKey.Enter),
                    new Keybind(() =>
                    {
                        this._tempTransaction.Description = Input.String;
                        Input.String = this._tempTransaction.Amount.ToString();
                        this.SetStage(Stages.Transaction_Add_Amount);
                    }, key: ConsoleKey.Escape));
            }
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
            Transaction_Add_Amount,
            Transaction_Add_Description,
            Transaction_Delete,
            Transaction_Edit,
        }
    }
}
