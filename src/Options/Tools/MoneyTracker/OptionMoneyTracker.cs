using B.Inputs;
using B.Utils;
using B.Utils.Extensions;

namespace B.Options.Tools.MoneyTracker
{
    public sealed class OptionMoneyTracker : Option<OptionMoneyTracker.Stages>
    {
        #region Constants

        private const int MAX_TRANSACTIONS_PER_PAGE = 50;

        #endregion



        #region Universal Properties

        public static string Title => "Money Tracker";
        public static string DirectoryPath => Program.DataPath + @"accounts\";

        #endregion



        #region Private Variables

        private List<Account> _accounts = new();
        private Transaction? _tempTransaction;
        private Account? _selectedAccount;

        #endregion



        #region Constructors

        public OptionMoneyTracker() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(OptionMoneyTracker.DirectoryPath))
                Directory.CreateDirectory(OptionMoneyTracker.DirectoryPath);
            else
                foreach (string filePath in Directory.GetFiles(OptionMoneyTracker.DirectoryPath))
                    AddAccount(Path.GetFileNameWithoutExtension(filePath), true);
        }

        #endregion



        #region Override Methods

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

                        Window.Clear();
                        Window.SetSize(20, consoleHeight);
                        Cursor.Set(0, 1);
                        Input.Choice choice = Input.Choice.Create(OptionMoneyTracker.Title);
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Account), "Account", '1'));

                        if (selected)
                            choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Transaction), "Transaction", '2'));

                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.Account:
                    {
                        Window.Clear();
                        bool selected = _selectedAccount != null;
                        Window.SetSize(24, selected ? 12 : 9);
                        Cursor.Set(0, 1);

                        if (selected)
                        {
                            Cursor.Set(3, 1);
                            Window.Print("Selected Account:");
                            Cursor.Set(2, 2);
                            Window.Print(_selectedAccount!.Name);
                            Cursor.Set(0, 4);
                        }

                        Input.Choice choice = Input.Choice.Create("Account");
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Account_Create), "Create", '1'));
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Account_Select), "Select", '2'));
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Account_Remove), "Remove", '3'));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;

                case Stages.Account_Create:
                    {
                        Window.Clear();
                        Window.SetSize(42, 5);
                        Cursor.Set(2, 1);
                        Window.Print($"New Account Name: {Input.String}");
                        Input.RequestLine(20,
                            Keybind.Create(() =>
                            {
                                if (Input.String.Length > 0)
                                {
                                    string filePath = OptionMoneyTracker.DirectoryPath + Input.String;

                                    if (!File.Exists(filePath))
                                    {
                                        Account account = AddAccount(Path.GetFileNameWithoutExtension(filePath));
                                        _selectedAccount = account;
                                        Cursor.x = 2;
                                        Cursor.y += 2;
                                        Window.Print($"\"{Input.String}\" created!");
                                        Input.ResetString();
                                        SetStage(Stages.Account);
                                    }
                                    else
                                    {
                                        Cursor.Set(4, 3);
                                        Window.Print("Name already taken!");
                                    }

                                    Input.Get();
                                }
                            }, key: ConsoleKey.Enter),
                            Keybind.Create(() =>
                            {
                                Input.ResetString();
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

                        Window.Clear();
                        Window.SetSize(27, consoleHeight);
                        Cursor.Set(0, 1);
                        Input.Choice choice = Input.Choice.Create();

                        // TODO turn into Input.RequestScroll because accounts can reach more than single digit numbers on a keyboard
                        if (amountAccounts > 0)
                        {
                            for (int i = 0; i < amountAccounts; i++)
                            {
                                Account account = _accounts[i];
                                choice.AddKeybind(Keybind.Create(() =>
                                {
                                    _selectedAccount = account;
                                    SetStage(Stages.Account);
                                }, account.Name, keyChar: (char)('1' + i)));
                            }

                            choice.AddSpacer();
                        }

                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Account), "Back", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;

                case Stages.Account_Remove:
                    {
                        int consoleHeight = 5;
                        int amountAccounts = _accounts.Count;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Window.Clear();
                        Window.SetSize(27, consoleHeight);
                        Cursor.Set(0, 1);
                        Input.Choice choice = Input.Choice.Create("Remove Account");

                        if (amountAccounts > 0)
                        {
                            for (int i = 0; i < amountAccounts; i++)
                            {
                                Account account = _accounts[i];

                                choice.AddKeybind(Keybind.CreateConfirmation(() =>
                                {
                                    if (_selectedAccount == account)
                                        _selectedAccount = null;

                                    _accounts.Remove(account);
                                    account.Delete();
                                    SetStage(Stages.Account);
                                }, $"Delete account {account.Name}?", account.Name, keyChar: (char)('1' + i)));
                            }

                            choice.AddSpacer();
                        }

                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Account), "Back", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;

                case Stages.Transaction:
                    {
                        Window.Clear();
                        Window.SetSize(20, 10);
                        Cursor.Set(0, 1);
                        Input.Choice choice = Input.Choice.Create("Transaction");
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            SetStage(Stages.Transaction_View);
                            Window.Clear();
                        }, "View", '1'));
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            Input.ResetString();
                            _tempTransaction = new();
                            SetStage(Stages.Transaction_Add_Amount);
                        }, "Add", '2'));
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Transaction_Delete), "Delete", '3'));
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Transaction_Edit), "Edit", '4'));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;

                case Stages.Transaction_View:
                    {
                        Window.SetSize(
                            (Input.DECIMAL_LENGTH * 2) + _selectedAccount!.Decimals + 9,
                            Math.Min(_selectedAccount.Transactions.Count, OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE) + 9);
                        Cursor.y = 1;
                        Input.RequestScroll(
                            items: _selectedAccount.Transactions,
                            getText: transaction => string.Format("{0," + (Input.DECIMAL_LENGTH + _selectedAccount.Decimals + 1) + ":0." + '0'.Loop(_selectedAccount.Decimals) + "} | {1," + Input.DECIMAL_LENGTH + "}", transaction.Amount, transaction.Description),
                            maxEntriesPerPage: OptionMoneyTracker.MAX_TRANSACTIONS_PER_PAGE,
                            exitKeybind: Keybind.Create(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.Transaction);
                            }, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                Keybind.Create(() => _selectedAccount.Decimals++, "Increase Decimals", '+'),
                                Keybind.Create(() =>
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
                        throw new NotImplementedException();
                        // Window.Clear();
                        // Window.SetSize(31, _selectedAccount!.Transactions.Count + 4);
                        // Util.PrintLine();
                        // Util.PrintLine("  Delete");
                        // _selectedAccount.PrintTransactions(); // TODO

                        // Util.GetKey();
                        // TODO add keybinds to delete a transaction
                        // SetStage(Stages.Transaction);
                    }
                // break;

                case Stages.Transaction_Edit:
                    {
                        throw new NotImplementedException();
                        // Window.Clear();
                        // SetStage(Stages.Transaction);
                        // TODO
                    }
                    // break;
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

        #endregion



        #region Private Methods

        private Account AddAccount(string name, bool deserialize = false)
        {
            Account account;

            if (deserialize)
                account = Data.Deserialize<Account>(OptionMoneyTracker.DirectoryPath + name);
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
            Window.Clear();
            Window.SetSize(4 + Input.DECIMAL_LENGTH, 7);
            Cursor.Set(2, 1);
            Window.Print("Amount:");
            Cursor.Set(2, 2);

            if (Stage == Stages.Transaction_Add_Amount)
            {
                Window.Print(Input.String);
                Input.RequestLine(Input.DECIMAL_LENGTH,
                    Keybind.Create(() =>
                    {
                        decimal? amount = Input.Decimal;

                        if (amount.HasValue)
                        {
                            _tempTransaction!.Amount = amount.Value;
                            Input.String = _tempTransaction.Description;
                            SetStage(Stages.Transaction_Add_Description);
                        }
                    }, key: ConsoleKey.Enter),
                    Keybind.Create(() =>
                    {
                        _tempTransaction = null;
                        Input.ResetString();
                        SetStage(Stages.Transaction);
                    }, key: ConsoleKey.Escape));
            }
            else
            {
                Window.Print(_tempTransaction!.Amount);
                Cursor.Set(2, 4);
                Window.Print("Description:");
                Cursor.Set(2, 5);
                Window.Print(Input.String);
                Input.RequestLine(Input.DECIMAL_LENGTH,
                    Keybind.Create(() =>
                    {
                        if (Input.String.Length > 0)
                        {
                            _tempTransaction.Description = Input.String.Trim();
                            _selectedAccount!.Transactions.Add(_tempTransaction);
                            _tempTransaction = null;
                            Input.ResetString();
                            SetStage(Stages.Transaction);
                        }
                    }, key: ConsoleKey.Enter),
                    Keybind.Create(() =>
                    {
                        _tempTransaction.Description = Input.String;
                        Input.String = _tempTransaction.Amount.ToString();
                        SetStage(Stages.Transaction_Add_Amount);
                    }, key: ConsoleKey.Escape));
            }
        }

        #endregion



        #region Enums

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

        #endregion
    }
}
