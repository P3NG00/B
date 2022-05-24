using B.Inputs;
using B.Utils;
using B.Utils.Extensions;

namespace B.Modules.Tools.MoneyTracker
{
    public sealed class ModuleMoneyTracker : Module<ModuleMoneyTracker.Stages>
    {
        #region Constants

        // Constant maximum amount of transactions to list in transaction list.
        private const int MAX_TRANSACTIONS_PER_PAGE = 50;

        #endregion



        #region Universal Properties

        // Module Title.
        public static string Title => "Money Tracker";
        // Relative path where accounts are stored.
        public static string DirectoryPath => Program.DataPath + @"accounts\";

        #endregion



        #region Private Variables

        // List of loaded accounts.
        private List<Account> _accounts = new();
        // Transaction currently being modified.
        private Transaction? _tempTransaction;
        // Account currently being modified.
        private Account? _selectedAccount;

        #endregion



        #region Constructors

        // Creates a new instance of ModuleMoneyTracker.
        public ModuleMoneyTracker() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
            else
                foreach (string filePath in Directory.GetFiles(DirectoryPath))
                    AddAccount(Path.GetFileNameWithoutExtension(filePath), true);
        }

        #endregion



        #region Override Methods

        // Module Loop.
        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        int consoleHeight = 7;
                        bool accountSelected = _selectedAccount != null;

                        if (accountSelected)
                            consoleHeight++;

                        Window.SetSize(20, consoleHeight);
                        Cursor.Set(0, 1);
                        Choice choice = new(Title);
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Account), "Account", '1'));

                        if (accountSelected)
                            choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Transaction), "Transaction", '2'));

                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.Account:
                    {
                        bool selected = _selectedAccount is not null;
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

                        Choice choice = new("Account");
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
                        Window.SetSize(42, 5);
                        Cursor.Set(2, 1);
                        Window.Print($"New Account Name: {Input.String}");
                        bool exiting = false;
                        Input.RequestLine(20,
                            Keybind.Create(() =>
                            {
                                if (Input.String.Length > 0)
                                {
                                    string filePath = DirectoryPath + Input.String;

                                    if (!File.Exists(filePath))
                                    {
                                        Account account = AddAccount(Path.GetFileNameWithoutExtension(filePath));
                                        _selectedAccount = account;
                                        Cursor.NextLine(2, 2);
                                        Window.Print($"\"{Input.String}\" created!");
                                        Input.ResetString();
                                        exiting = true;
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
                            }, key: ConsoleKey.Escape)
                        );
                        if (exiting)
                            SetStage(Stages.Account);
                    }
                    break;

                case Stages.Account_Select:
                    {
                        int consoleHeight = 5;
                        int amountAccounts = _accounts.Count;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Window.SetSize(27, consoleHeight);
                        Cursor.y = 1;
                        Input.RequestScroll(
                            items: _accounts,
                            getText: account => account.Name,
                            exitKeybind: Keybind.Create(() =>
                            {
                                _selectedAccount = _accounts[Input.ScrollIndex];
                                SetStage(Stages.Account);
                            }, "Select", key: ConsoleKey.Escape)
                        );
                    }
                    break;

                case Stages.Account_Remove:
                    {
                        int consoleHeight = 5;
                        int amountAccounts = _accounts.Count;

                        if (amountAccounts > 0)
                            consoleHeight += amountAccounts + 1;

                        Window.SetSize(27, consoleHeight);
                        Cursor.Set(0, 1);
                        Choice choice = new("Remove Account");

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
                        Window.SetSize(20, 8);
                        Cursor.Set(0, 1);
                        Choice choice = new("Transaction");
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Transaction_View), "View", '1'));
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            Input.ResetString();
                            _tempTransaction = new();
                            SetStage(Stages.Transaction_Add_Amount);
                        }, "Add", '2'));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;

                case Stages.Transaction_View:
                    {
                        Window.SetSize(
                            (Input.DECIMAL_LENGTH * 2) + _selectedAccount!.Decimals + 9,
                            Math.Min(_selectedAccount.Transactions.Count, MAX_TRANSACTIONS_PER_PAGE) + 10);
                        Cursor.y = 1;
                        Input.RequestScroll(
                            items: _selectedAccount.Transactions,
                            getText: transaction => string.Format("{0," + (Input.DECIMAL_LENGTH + _selectedAccount.Decimals + 1) + ":0." + '0'.Loop(_selectedAccount.Decimals) + "} | {1," + Input.DECIMAL_LENGTH + "}", transaction.Amount, transaction.Description),
                            maxEntriesPerPage: MAX_TRANSACTIONS_PER_PAGE,
                            exitKeybind: Keybind.Create(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.Transaction);
                            }, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[]
                            {
                                Keybind.Create(_selectedAccount.IncrementDecimals, "Increase Decimals", '+'),
                                Keybind.Create(_selectedAccount.DecrementDecimals, "Decrease Decimals", '-'),
                                Keybind.CreateConfirmation(() => _selectedAccount!.Transactions.RemoveAt(Input.ScrollIndex), "Delete this transaction?", "Delete", key: ConsoleKey.Delete)
                            }
                        );
                    }
                    break;

                case Stages.Transaction_Add_Amount:
                case Stages.Transaction_Add_Description:
                    {
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
                                }, key: ConsoleKey.Escape)
                            );
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
                                }, key: ConsoleKey.Escape)
                            );
                        }
                    }
                    break;
            }
        }

        // Saves account data before exiting Module.
        public override void Quit()
        {
            // Save account data
            _accounts.ForEach(account => account.Save());
            // Quit
            base.Quit();
        }

        #endregion



        #region Private Methods

        // Add account to list either as new or with loaded values.
        private Account AddAccount(string name, bool deserialize = false)
        {
            Account account;

            if (deserialize)
                account = Data.Deserialize<Account>(DirectoryPath + name);
            else
            {
                account = new(name);
                account.Save();
            }

            _accounts.Add(account);
            return account;
        }

        #endregion



        #region Enums

        // Module Stages.
        public enum Stages
        {
            // Select account or transaction to manage.
            MainMenu,
            // Manage accounts.
            Account,
            // Create new account.
            Account_Create,
            // Select existing account.
            Account_Select,
            // Delete existing account.
            Account_Remove,
            // Manage transactions under account.
            Transaction,
            // View transactions.
            Transaction_View,
            // Add new transaction stages.
            Transaction_Add_Amount,
            Transaction_Add_Description,
        }

        #endregion
    }
}
