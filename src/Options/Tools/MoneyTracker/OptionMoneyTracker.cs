using B.Inputs;
using B.Utils;

namespace B.Options.Tools.MoneyTracker
{
    public sealed class OptionMoneyTracker : Option<OptionMoneyTracker.Stages>
    {
        public const string Title = "Money Tracker";

        private const int MAX_TRANSACTIONS_PER_PAGE = 50;

        public static readonly string DirectoryPath = Program.DataPath + @"accounts\";

        private List<Account> _accounts = new();
        private Transaction? _tempTransaction;
        private Account? _selectedAccount;

        public OptionMoneyTracker() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(OptionMoneyTracker.DirectoryPath))
                Directory.CreateDirectory(OptionMoneyTracker.DirectoryPath);
            else
                foreach (string filePath in Directory.GetFiles(OptionMoneyTracker.DirectoryPath))
                    AddAccount(Path.GetFileNameWithoutExtension(filePath), true);
        }

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        int consoleHeight = 7;
                        bool selected = _selectedAccount != null;

                        if (selected)
                            consoleHeight++;

                        Window.ClearAndSetSize(20, consoleHeight);
                        Input.Choice choice = Input.Choice.Create(OptionMoneyTracker.Title)
                            .Add(() => SetStage(Stages.Account), "Account", '1');

                        if (selected)
                            choice.Add(() => SetStage(Stages.Transaction), "Transaction", '2');

                        choice.AddSpacer()
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.Account:
                    {
                        if (_selectedAccount != null)
                        {
                            Window.ClearAndSetSize(24, 12);
                            Window.PrintLine();
                            Window.PrintLine("   Selected Account:");
                            Window.PrintLine($"  {_selectedAccount.Name}");
                        }
                        else
                            Window.ClearAndSetSize(24, 9);

                        Input.Choice.Create("Account")
                            .Add(() => SetStage(Stages.Account_Create), "Create", '1')
                            .Add(() => SetStage(Stages.Account_Select), "Select", '2')
                            .Add(() => SetStage(Stages.Account_Remove), "Remove", '3')
                            .AddSpacer()
                            .Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape)
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
                                        Account account = AddAccount(Path.GetFileNameWithoutExtension(filePath));
                                        _selectedAccount = account;
                                        Window.PrintLines(2);
                                        Window.PrintLine($"  \"{Input.String}\" created!");
                                        Input.ResetString(); ;
                                        SetStage(Stages.Account);
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
                                SetStage(Stages.Account);
                            }, key: ConsoleKey.Escape));
                    }
                    break;

                case Stages.Account_Select:
                    {
                        int consoleHeight = 3;
                        int amountAccounts = _accounts.Count;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Window.ClearAndSetSize(27, consoleHeight);
                        Window.PrintLine();
                        Input.Choice choice = Input.Choice.Create();

                        if (amountAccounts > 0)
                        {
                            for (int i = 0; i < amountAccounts; i++)
                            {
                                Account account = _accounts[i];
                                choice.Add(() =>
                                {
                                    _selectedAccount = account;
                                    SetStage(Stages.Account);
                                }, account.Name, keyChar: (char)('1' + i));
                            }

                            choice.AddSpacer();
                        }

                        choice.Add(() => SetStage(Stages.Account), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Account_Remove:
                    {
                        int consoleHeight = 5;
                        int amountAccounts = _accounts.Count;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Window.ClearAndSetSize(27, consoleHeight);
                        Input.Choice choice = Input.Choice.Create("Remove Account");

                        if (amountAccounts > 0)
                        {
                            for (int i = 0; i < amountAccounts; i++)
                            {
                                Account account = _accounts[i];
                                choice.Add(() =>
                                {
                                    if (_selectedAccount == account)
                                        _selectedAccount = null;

                                    _accounts.Remove(account);
                                    account.Delete();
                                    SetStage(Stages.Account);
                                }, account.Name, keyChar: (char)('1' + i));
                            }

                            choice.AddSpacer();
                        }

                        choice.Add(() => SetStage(Stages.Account), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Transaction:
                    {
                        Window.ClearAndSetSize(20, 10);
                        Input.Choice.Create("Transaction")
                            .Add(() =>
                            {
                                SetStage(Stages.Transaction_View);
                                Window.Clear();
                            }, "View", '1')
                            .Add(() =>
                            {
                                Input.ResetString(); ;
                                _tempTransaction = new();
                                SetStage(Stages.Transaction_Add_Amount);
                            }, "Add", '2')
                            .Add(() => SetStage(Stages.Transaction_Delete), "Delete", '3')
                            .Add(() => SetStage(Stages.Transaction_Edit), "Edit", '4')
                            .AddSpacer()
                            .Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Transaction_View:
                    {
                        Window.SetSize(
                            (Input.DECIMAL_LENGTH * 2) + _selectedAccount!.Decimals + 9,
                            Math.Min(_selectedAccount.Transactions.Count, OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE) + 9);
                        Cursor.Reset();
                        Window.PrintLine();
                        Input.RequestScroll(
                            items: _selectedAccount.Transactions,
                            getText: transaction => string.Format("{0," + (Input.DECIMAL_LENGTH + _selectedAccount.Decimals + 1) + ":0." + Util.StringOf('0', _selectedAccount.Decimals) + "} | {1," + Input.DECIMAL_LENGTH + "}", transaction.Amount, transaction.Description),
                            maxEntriesPerPage: OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE,
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.Transaction);
                            }, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => _selectedAccount.Decimals++, "Increase Decimals", '+'),
                                new(() =>
                                {
                                    // Checking before decrementing to avoid underflowing to max value
                                    if (_selectedAccount.Decimals != 0)
                                        _selectedAccount.Decimals--;
                                }, "Decrease Decimals", '-')});
                    }
                    break;

                case Stages.Transaction_Add_Amount: ShowTransactionStage(); break;

                case Stages.Transaction_Add_Description: ShowTransactionStage(); break;

                case Stages.Transaction_Delete:
                    {
                        Window.ClearAndSetSize(31, _selectedAccount!.Transactions.Count + 4);
                        // Util.PrintLine();
                        // Util.PrintLine("  Delete");
                        // _selectedAccount.PrintTransactions(); // TODO

                        // Util.GetKey();
                        // TODO add keybinds to delete a transaction
                        SetStage(Stages.Transaction);
                    }
                    break;

                case Stages.Transaction_Edit:
                    {
                        Window.Clear();
                        SetStage(Stages.Transaction);
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

            _accounts.Add(account);
            return account;
        }

        private void ShowTransactionStage()
        {
            Window.ClearAndSetSize(4 + Input.DECIMAL_LENGTH, 7);
            Window.PrintLine();
            Window.PrintLine("  Amount:");

            if (Stage == Stages.Transaction_Add_Amount)
            {
                Window.Print($"  {Input.String}");
                Input.RequestLine(Input.DECIMAL_LENGTH,
                    new Keybind(() =>
                    {
                        decimal? amount = Input.Decimal;

                        if (amount.HasValue)
                        {
                            _tempTransaction!.Amount = amount.Value;
                            Input.String = _tempTransaction.Description;
                            SetStage(Stages.Transaction_Add_Description);
                        }
                    }, key: ConsoleKey.Enter),
                    new Keybind(() =>
                    {
                        _tempTransaction = null;
                        Input.ResetString(); ;
                        SetStage(Stages.Transaction);
                    }, key: ConsoleKey.Escape));
            }
            else
            {
                Window.PrintLine($"  {_tempTransaction!.Amount}");
                Window.PrintLine();
                Window.PrintLine("  Description:");
                Window.Print($"  {Input.String}");
                Input.RequestLine(Input.DECIMAL_LENGTH,
                    new Keybind(() =>
                    {
                        if (Input.String.Length > 0)
                        {
                            _tempTransaction.Description = Input.String.Trim();
                            _selectedAccount!.Transactions.Add(_tempTransaction);
                            _tempTransaction = null;
                            Input.ResetString(); ;
                            SetStage(Stages.Transaction);
                        }
                    }, key: ConsoleKey.Enter),
                    new Keybind(() =>
                    {
                        _tempTransaction.Description = Input.String;
                        Input.String = _tempTransaction.Amount.ToString();
                        SetStage(Stages.Transaction_Add_Amount);
                    }, key: ConsoleKey.Escape));
            }
        }

        public override void Save()
        {
            foreach (Account account in _accounts)
                account.Save();
        }

        public override void Quit()
        {
            Save();
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
