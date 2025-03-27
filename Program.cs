using System.Diagnostics;
using Microsoft.Data.Sqlite;

public partial class Program
{
  readonly static SqliteConnection con = new("Data Source=expenses.db");
  public static List<string> Args = [];
  public static void Main(string[] command)
  {
    string Version = "0.6.0";
    #region Arg Handling

    if (command.Length <= 0 || command[0] == "help" || command[0] == "h")
    {
      Console.Write(StringSpacer($"Expense-Tracker {Version}", 35));
      Console.WriteLine("Written by Almigdad Bolad\n");
      Console.WriteLine("Usage: expense-tracker <command> <arguments N>\n");
      Console.WriteLine("<command>");
      Console.Write(StringSpacer("add", 15));
      Console.WriteLine("Adds an expense to the DB");
      Console.Write(StringSpacer("update", 15));
      Console.WriteLine("Modify the expense");
      Console.Write(StringSpacer("delete", 15));
      Console.WriteLine("Removes the expense");
      Console.Write(StringSpacer("list", 15));
      Console.WriteLine("Displays the details of the month's expenses");
      Console.Write(StringSpacer("summary", 15));
      Console.WriteLine("Displays a summary of the month");
      Console.WriteLine();
      return;
    }

    using (var cmd = new SqliteCommand(@"
      CREATE TABLE IF NOT EXISTS expenses(
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      date DATE, 
      description TEXT,
      price INT);
      ", con))
    {
      con.Open();
      if (cmd.ExecuteNonQuery() > 0)
        Console.WriteLine("Database not found.\nCreated new one.\n\n");
      con.Close();
    }
    for (int i = 1; i < command.Length; i++)
    {
      Args.Add(command[i]);
    }
    switch (command[0])
    {
      case "add":
        if (Args.Count < 2 || Args.Count > 3)
        {
          if (Args.Count < 1 || (Args[0].ToLower() != "h" && Args[0].ToLower() != "help"))
            Console.WriteLine($"Incorrect usage of 'add'\n");
          Console.WriteLine("Usage: 'add (optional <date>) <description> <price>'\n");
          Console.WriteLine("<Parameters>");
          Console.Write(StringSpacer("<date>", 15));
          Console.WriteLine("Optional, default value is current date, ensure correct format <yyyy-MM-dd>");
          Console.Write(StringSpacer("<description>", 15));
          Console.WriteLine("The name of the expense / bill");
          Console.Write(StringSpacer("<price>", 15));
          Console.WriteLine("The price of the expense in the selected currency");
          break; ;
        }
        if (Args.Count == 3)
        {
          if (!isDateValid(Args[0].ToLower()))
          {
            Console.WriteLine("date has to follow the format: <yyyy-MM-dd>");
            return;
          }
          if (!int.TryParse(Args[2], out int priceWithDate) || priceWithDate < 0)
          {
            Console.WriteLine($"price needs to be an integer and greater than zero\n");
            return;
          }
          HandlerAdd(Args[0].ToLower(), Args[1], priceWithDate);
          break;
        }
        if (!int.TryParse(Args[1], out int price) || price < 0)
        {
          Console.WriteLine("price needs to be an integer and greater than zero\n");
          return;
        }
        HandlerAdd(Args[0].ToLower(), price);
        break;
      case "update":
        if (Args.Count != 4 || (Args[0].ToLower() != "h" && Args[0].ToLower() != "help"))
        {
          Console.WriteLine($"Incorrect usage of 'update'\n\nUsage: 'update <id> <new date> <new description> <new price>'\n\n");
          break;
        }
        if (!int.TryParse(Args[0].ToLower(), out int id))
        {
          Console.WriteLine("id needs to be an integers");
          break;
        }
        if (!int.TryParse(Args[3], out int UpdatePrice) && UpdatePrice >= 0)
        {
          Console.WriteLine("price needs to be an integer and greater than zero");
          break;
        }
        HandlerUpdate(id, Args[1], Args[2], UpdatePrice);
        break;
      case "list":
        if (Args.Count == 1 && isDateValid(Args[0].ToLower(), true))
        {
          HandlerList(DateTime.Parse(Args[0].ToLower()));
          break;
        }
        else if (Args.Count == 0)
          HandlerList();
        else
        {
          if (Args.Count < 0 || (Args[0].ToLower() != "h" && Args[0].ToLower() != "help"))
            Console.WriteLine("Incorrect usage of command\n");
          Console.WriteLine("Usage: expense-tracker list <parameter>\n");
          Console.WriteLine("<Parameters>");
          Console.Write(StringSpacer("<date>", 15));
          Console.WriteLine("Displays expenses of the month in <date>, ensure correct format <yyyy-MM-dd>");
          Console.WriteLine("Usage: list (optional <date>)");
        }
        break;
      case "summary":
        if (Args.Count == 1 && isDateValid(Args[0].ToLower(), true))
        {
          HandlerSummary(DateTime.Parse(Args[0].ToLower()));
        }
        else if (Args.Count >= 1)
        {
          if (Args[0].ToLower() != "h" && Args[0].ToLower() != "help" && !isDateValid(Args[0].ToLower()))
            Console.WriteLine("Incorrect usage of command\n");
          Console.WriteLine("Usage: expense-tracker summary (optional <parameter>)\n");
          Console.WriteLine("<Parameters>");
          Console.Write(StringSpacer("<date>", 15));
          Console.WriteLine("Displays expenses of the month in <date>, ensure correct format <yyyy-MM-dd>");
          break;
        }
        else
          HandlerSummary();
        break;
      case "delete":
        if (Args.Count <= 0 || Args[0].ToLower() == "h" || Args[0].ToLower() == "help")
        {
          Console.WriteLine("Usage: expense-tracker delete <parameter>\n");
          Console.WriteLine("<Parameters>");
          Console.Write(StringSpacer("<id>", 15));
          Console.WriteLine("Allows inserting multiple IDs, separated by space, to be deleted");
          break;
        }
        List<int> ValidIDs = [];
        if (Args.Count >= 1)
          for (int i = 0; i < Args.Count; i++)
          {
            if (!int.TryParse(Args[i], out int DelID))
            {
              Console.WriteLine("id must be an int.\n");
              return;
            }
            ValidIDs.Add(DelID);
            Console.WriteLine($"{DelID} Was Added To List.");
          }
        if (ValidIDs.Count >= 1)
          HandlerDelete(ValidIDs);
        break;
      default:
        Console.WriteLine($"{Args[0].ToLower()} is not a command. Type h or help for command list.");
        break;
    }
    #endregion
  }
  public static bool isDateValid(string date, bool YearAndMonth = false)
  {
    string[] parts = date.Split('-');
    if (parts.Length == 2 && YearAndMonth == true)
    {
      if (!(int.TryParse(parts[0], out int year1) &&
          int.TryParse(parts[1], out int month1))) return false;
      if (year1 < 1900 || year1 > int.Parse(DateTime.Today.ToString("yyyy")) || month1 < 1 || month1 > 12) return false;
      return true;
    }
    if (parts.Length != 3) return false;
    if (!(int.TryParse(parts[0], out int year) &&
          int.TryParse(parts[1], out int month) &&
          int.TryParse(parts[2], out int day))) return false;
    if (year < 1900 || year > int.Parse(DateTime.Today.ToString("yyyy")) || month < 1 || month > 12 || day < 1 || day > 31) return false;
    return true;
  }
  public static string StringSpacer(string str, int spaces)
  {
    while (str.Length <= spaces)
    {
      str += " ";
    }
    return str;
  }
}

//TODO:
// [-] 1. After finishing all the command, write the help.
// [-] 2. If a month is empty then return a text saying it's empty instead of not having anything
// [-] 3. If it's list or summary then remove the days for the user to input
// [] 4. Add a way to change the currency
// [-] 5. If a price is negative in the "Add" / "Update", make sure to display an error
