using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactoring
{
    public class Tusc
    {
        private static System.Collections.Generic.List<Refactoring.User> TuscUsers { get; set; }
        private static System.Collections.Generic.List<Product> TuscProducts { get; set; }
        private static User TuscUser;
        private const int MaxLoginRetries = 3;
        private static int MenuSelection = 0;

        public static void Start(List<User> usrs, List<Product> prods)
        {
            TuscUsers = usrs;
            TuscProducts = prods;

            DisplayWelcomeMessage();

            if (PerformUserLogin())
                PurchaseItems();

            CloseProgram();
        }

        #region Login Methods

        //The user will have a limited amount of tries to login. Username and password are done together for security purposes.
        private static bool PerformUserLogin()
        {
            bool LoggedInStatus = false;
            int tryCount = 0;

            while (tryCount < MaxLoginRetries && !LoggedInStatus)
            {
                WriteLine("Enter Username:");
                string UserName = Console.ReadLine();

                WriteLine("Enter Password:");
                string UserPassword = Console.ReadLine();

                if (LoginUser(UserName, UserPassword))
                {
                    LoggedInStatus = true;
                    DisplayLoginSuccessMessage();
                }
                else
                    DisplayFailedLoginMessage();

                tryCount++;
            }

            return LoggedInStatus;
        }


        private static bool LoginUser(string UserName, string UserPassword)
        {
            bool LoginStatus = false;

            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(UserPassword))
            {
                for (int i = 0; i < TuscUsers.Count() && !LoginStatus; i++)
                {
                    TuscUser = TuscUsers[i];
                    if (TuscUser.Name == UserName)
                    {
                        if (TuscUser.Name == UserName && TuscUser.Pwd == UserPassword)
                            LoginStatus = true;
                    }
                }
            }

            return LoginStatus;
        }
        #endregion

        #region Purchase Methods
        private static void PurchaseItems()
        {
            DisplayAccountBalance();

            while (true)
            {
                DisplayMenu();
                
                MenuSelection = GetUserSelection();
  
                if (MenuSelection == TuscProducts.Count())
                {
                    UpdateData();
                    return;
                }
                else if(MenuSelectionValid()){   
                    int ItemQuantity = QueryItemPurchaseQuanity();

                    if (CheckStockandCost(ItemQuantity))
                        PurchaseItem(ItemQuantity);
                    else
                        DisplayPurchaseCancelled();
               }
            }
        }

        private static bool MenuSelectionValid()
        {
            if (MenuSelection > -1 && MenuSelection < TuscProducts.Count())
                return true;
            else
                WriteErrorLine("Invalid selection");
                return false;
        }


        private static void PurchaseItem(int ItemQuantity)
        {
            TuscUser.Bal = TuscUser.Bal - TuscProducts[MenuSelection].Price * ItemQuantity;
            TuscProducts[MenuSelection].Qty = TuscProducts[MenuSelection].Qty - ItemQuantity;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("You bought " + ItemQuantity + " " + TuscProducts[MenuSelection].Name);
            Console.WriteLine("Your new balance is " + TuscUser.Bal.ToString("C"));
            Console.ResetColor();
        }

        private static bool CheckStockandCost(int ItemQuantity)
        {
            bool isValidPurchase = true;

            if (ItemQuantity == 0){
                Console.Clear();
                isValidPurchase = false;
            }else if (TuscUser.Bal - TuscProducts[MenuSelection].Price * ItemQuantity < 0){
                Console.Clear();
                WriteErrorLine("You do not have enough money to buy that.");
                isValidPurchase = false;
            }
            else if (TuscProducts[MenuSelection].Qty <= ItemQuantity)
            {
                Console.Clear();
                WriteErrorLine("Sorry, " + TuscProducts[MenuSelection].Name + " is out of stock");
                isValidPurchase = false;
            }

            return isValidPurchase;
        }

        private static int QueryItemPurchaseQuanity()
        {
            WriteLine("You want to buy: " + TuscProducts[MenuSelection].Name);
            Console.WriteLine("Your balance is " + TuscUser.Bal.ToString("C"));
            Console.WriteLine("Enter amount to purchase:");
            return ValidIntegerInput();
        }
        #endregion

        #region Class Methods

        private static int ValidIntegerInput()
        {
            string UserInput = Console.ReadLine();
            return UserInput != "" ?Convert.ToInt32(UserInput) : 0;
        }

        private static int GetUserSelection()
        {
            Console.WriteLine("Enter a number:");
            return ValidIntegerInput() - 1;
        }

        private static void UpdateData()
        {
            string json = JsonConvert.SerializeObject(TuscUsers, Formatting.Indented);
            File.WriteAllText(@"Data\Users.json", json);

            // Write out new quantities
            string json2 = JsonConvert.SerializeObject(TuscProducts, Formatting.Indented);
            File.WriteAllText(@"Data\Products.json", json2);
        }

        private static void CloseProgram()
        {
            WriteLine("Press Enter key to exit");
            Console.ReadLine();
        }
        #endregion

        #region Display Methods
        private static void DisplayFailedLoginMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("You entered an invalid user name or password.");
            Console.ResetColor();
        }

        private static void DisplayMenu()
        {
            WriteLine("What would you like to buy?");
            for (int i = 0; i < TuscProducts.Count(); i++)
            {
                Product prod = TuscProducts[i];
                Console.WriteLine(i + 1 + ": " + prod.Name + " (" + prod.Price.ToString("C") + ")");
            }
            Console.WriteLine(TuscProducts.Count + 1 + ": Exit");
        }

        private static void DisplayPurchaseCancelled()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLine("Purchase cancelled");
            Console.ResetColor();
        }

        private static void DisplayAccountBalance()
        {
                WriteLine("Your balance is " + TuscUser.Bal.ToString("C"));
        }

        private static void DisplayLoginSuccessMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine("Login successful! Welcome " + TuscUser.Name + "!");
            Console.ResetColor();
        }

        private static void DisplayWelcomeMessage()
        {
            Console.WriteLine("Welcome to TUSC");
            Console.WriteLine("---------------");
        }


        private static void WriteLine(string text)
        {
            Console.WriteLine();
            Console.WriteLine(text);
        }

        private static void WriteErrorLine(string text)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine(text);
            Console.ResetColor();
        }
        #endregion

    }
}
