using Microsoft.Data.Sqlite;

public partial class Program
{
  readonly static SqliteConnection con = new("Data Source=expenses.db");
  public static List<string> Args = [];
  public static void Main(string[] command)
  {
    string Version = "0.6.0";
    #region Arg Handling

    if (command.Length <= 0)
    {
      Console.Write(StringSpacer($"Expense-Tracker {Version}", 35));
      Console.WriteLine("Written by Almigdad Bolad\n");
      Console.WriteLine("Usage: expense-tracker <command> <arguments N>\n");
      Console.WriteLine("<command>");
      Console.Write(StringSpacer("add", 15));
      Console.WriteLine("adds an expense to the DB");
      Console.Write(StringSpacer("update", 15));
      Console.WriteLine("Modify the expense");
      Console.Write(StringSpacer("list", 15));
      Console.WriteLine("Displays the details of the month's expenses");
      Console.Write(StringSpacer("summary", 15));
      Console.WriteLine("Displays a summary of the month");
      Console.Write(StringSpacer("delete", 15));
      Console.WriteLine("Removes the expense");
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
        if (Args.Count < 2)
        {
          Console.WriteLine("Incorrect usage of 'add'\n\nUsage: 'add (optional <date>) <description> <price>'\n\n");
          return;
        }
        if (Args.Count == 3)
        {
          if (!isDateValid(Args[0]))
          {
            Console.WriteLine("date has to follow the format: <yyyy-MM-dd>");
            return;
          }
          if (!int.TryParse(Args[2], out int priceWithDate))
          {
            Console.WriteLine($"price needs to be an integer\n\n");
            return;
          }
          HandlerAdd(Args[0], Args[1], priceWithDate);
          break;
        }
        if (!int.TryParse(Args[1], out int price))
        {
          Console.WriteLine("price needs to be an integer\n\n");
          return;
        }
        HandlerAdd(Args[0], price);
        break;
      case "update":
        if (Args.Count != 4)
        {
          Console.WriteLine($"Incorrect usage of 'update'\n\nUsage: 'update <id> <new date> <new description> <new price>'\n\n");
          break;
        }
        if (!int.TryParse(Args[0], out int id))
        {
          Console.WriteLine("id needs to be an integers");
          break;
        }
        if (!int.TryParse(Args[3], out int UpdatePrice))
        {
          Console.WriteLine("price needs to be an int");
          break;
        }
        HandlerUpdate(id, Args[1], Args[2], UpdatePrice);
        break;
      case "list":
        if (Args.Count == 1)
        {
          if (isDateValid(Args[0]))
          {
            HandlerList(DateTime.Parse(Args[0]));
            break;
          }
          else
          {
            Console.WriteLine("Incorrect usage of command\n");
            Console.WriteLine("Usage: expense-tracker list <parameter>\n");
            Console.WriteLine("<Parameters>");
            Console.Write(StringSpacer("<date>", 15));
            Console.WriteLine("Displays expenses of the month in <date>, ensure correct format <yyyy-MM-dd>");
            Console.Write(StringSpacer("summary", 15));
            Console.WriteLine("Displays summary of the current month\n");
          }
        }
        else if (Args.Count == 0)
          HandlerList();
        else
          Console.WriteLine("Usage: list (optional <date>)");
        break;
      case "summary":
        if (Args.Count == 1)
        {
          if (!isDateValid(Args[0]))
          {
            Console.WriteLine("Incorrect usage of command\n");
            Console.WriteLine("Usage: expense-tracker summary (optional <parameter>)\n");
            Console.WriteLine("<Parameters>");
            Console.Write(StringSpacer("<date>", 15));
            Console.WriteLine("Displays expenses of the month in <date>, ensure correct format <yyyy-MM-dd>");
          }
          else
            HandlerSummary(DateTime.Parse(Args[0]));
        }
        else
          HandlerSummary();
        break;
      case "delete":
        List<int> ValidIDs = [];
        for (int i = 1; i < Args.Count; i++)
        {
          if (!int.TryParse(Args[i], out int DelID))
          {
            Console.WriteLine("id must be an int.\n\n");
            return;
          }
          ValidIDs.Add(DelID);
        }
        HandlerDelete(ValidIDs);
        break;
      default:
        Console.WriteLine($"{Args[0]} is not a command. Type --help for command list.");
        break;
    }
    #endregion
  }
  public static bool isDateValid(string date)
  {
    string[] parts = date.Split('-');
    if (parts.Length != 3) return false;
    if (!(int.TryParse(parts[0], out int year) &&
          int.TryParse(parts[1], out int month) &&
          int.TryParse(parts[2], out int day))) return false;
    if (year < 1900 || month < 1 || month > 12 || day < 1 || day > 31) return false;
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
// 1. After finishing all the command, write the help.
// 2. If a month is empty then return a text saying it's empty instead of not having anything
// 3. If it's list or summary then remove the days for the user to input
// 4. Add a way to change the currency
// 5. If a price is negative in the "Add" / "Update", make sure to display an error
// By default we will use today's date when adding
// It's possible to update the data
// And if the user inserts a custom date then we'll use it
