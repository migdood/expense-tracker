using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Data.Sqlite;
using SQLitePCL;

public class Program
{
  readonly static SqliteConnection con = new("Data Source=expenses.db");
  public static void Main(string[] args)
  {
    #region Arg Handling

    if (args.Length <= 0)
    {
      Console.WriteLine("Usage: expense-tracker <command> [arguments]");
      return;
    }

    using (var command = new SqliteCommand(@"
      CREATE TABLE IF NOT EXISTS expenses(
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      date DATE, 
      description TEXT,
      price INT);
      ", con))
    {
      con.Open();
      if (command.ExecuteNonQuery() > 0)
      {
        Console.WriteLine("Database not found.\nCreated new one.\n\n");
      }
      con.Close();
    }

    switch (args[0])
    {
      case "add":
        if (args.Length < 3)
        {
          Console.WriteLine("Incorrect usage of 'add'\n\nUsage: 'add (optional <date>) <description> <price>'\n\n");
          return;
        }

        if (args.Length == 4)
        {
          if (!isDateValid(args[1]))
          {
            Console.WriteLine("date has to follow the format: <yyyy-MM-dd>");
            return;
          }
          if (!int.TryParse(args[3], out int priceWithDate))
          {
            Console.WriteLine($"price needs to be an integer\n\n");
            return;
          }
          HandlerAdd(args[1], args[2], priceWithDate);
          break;
        }
        if (!int.TryParse(args[2], out int price))
        {
          Console.WriteLine("price needs to be an integer\n\n");
          return;
        }
        HandlerAdd(args[1], price);
        break;
      case "update":
        if (args.Length != 5)
        {
          Console.WriteLine($"Incorrect usage of 'update'\n\nUsage: 'update <id> <new date> <new description> <new price>'\n\nargs length:{args.Length}");
          return;
        }
        if (!int.TryParse(args[1], out int id))
        {
          Console.WriteLine("id needs to be an integers");
          return;
        }
        if (!int.TryParse(args[4], out int UpdatePrice))
        {
          Console.WriteLine("price needs to be an int");
        }
        HandlerUpdate(id, args[2], args[3], UpdatePrice);
        break;
      case "list":
        if (args.Length >= 2 && args.Length < 3)
          HandlerList(args[1]);
        else if (args.Length == 1)
          HandlerList();
        else
          Console.WriteLine("Usage: list (optional)<date>");
        break;
      case "delete":
        List<int> ValidIDs = [];
        for (int i = 1; i < args.Length; i++)
        {
          if (!int.TryParse(args[i], out int DelID))
          {
            Console.WriteLine("id must be an int.\n\n");
            return;
          }
          ValidIDs.Add(DelID);
        }
        HandlerDelete(ValidIDs);
        break;
      default:
        Console.WriteLine($"{args[0]} is not a command. Type --help for command list.");
        break;
    }
    #endregion
  }
  public static void HandlerAdd(string description, int price)
  {
    try
    {
      using (SqliteCommand cmd = new(@"INSERT INTO expenses(date, description, price) 
                                        VALUES(
                                        @date,
                                        @description,
                                        @price)", con))
      {
        cmd.Parameters.AddWithValue("@date", $"{DateTime.Today:yyyy-MM-dd}");
        cmd.Parameters.AddWithValue("@description", description);
        cmd.Parameters.AddWithValue("@price", price);
        con.Open();
        cmd.ExecuteNonQuery();
        con.Close();
      }
    }
    catch (Exception e)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write("Error:");
      Console.ResetColor();
      Console.WriteLine($"{e}");
    }
  }
  public static void HandlerAdd(string date, string description, int price)
  {
    try
    {
      using (SqliteCommand cmd = new(@"INSERT INTO expenses(date, description, price) 
                                        VALUES(
                                        @date,
                                        @description,
                                        @price)", con))
      {
        cmd.Parameters.AddWithValue("@date", date);
        cmd.Parameters.AddWithValue("@description", description);
        cmd.Parameters.AddWithValue("@price", price);
        con.Open();
        cmd.ExecuteNonQuery();
        con.Close();
      }
    }
    catch (Exception e)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write("Error:");
      Console.ResetColor();
      Console.WriteLine($"{e}");
    }
  }
  public static void HandlerUpdate(int id, string date, string description, int price)
  {
    if (!isDateValid(date))
    {
      Console.WriteLine("Please make sure the date is in the correct format:\n<yyyy-MM-dd>");
    }
    try
    {
      using (SqliteCommand cmd = new(@"UPDATE expenses SET 
                                      date = @date,
                                      description = @description,
                                      price = @price
                                      WHERE id = @id;", con))
      {
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@date", date);
        cmd.Parameters.AddWithValue("@description", description);
        cmd.Parameters.AddWithValue("@price", price);
        con.Open();
        cmd.ExecuteNonQuery();
        con.Close();
        Console.WriteLine("--------------------");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Updated ID:{id}");
        Console.ResetColor();
        Console.WriteLine("--------------------");
        HandlerList();
        con.Close();
      }
    }
    catch (Exception e)
    {
      Console.WriteLine($"Error: {e}");
    }
  }
  public static void HandlerList()
  {
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"# {StringSpacer("ID")}{StringSpacer("Date")}{StringSpacer("Description")}{StringSpacer("Price(egp)")}");
    Console.ResetColor();
    using (SqliteCommand cmd = new(@$"
    SELECT * FROM expenses 
    WHERE STRFTIME('%Y', date) = STRFTIME('%Y', '{DateTime.Today:yyyy-MM-dd}') 
    AND STRFTIME('%m', date) = STRFTIME('%m', '{DateTime.Today:yyyy-MM-dd}') 
    ORDER BY date ASC;", con))
    {
      con.Open();
      SqliteDataReader reader = cmd.ExecuteReader();
      while (reader.Read())
        Console.WriteLine($"# {StringSpacer(reader[0].ToString() ?? string.Empty)}{StringSpacer(reader[1].ToString() ?? string.Empty)}{StringSpacer(reader[2].ToString() ?? string.Empty)}{StringSpacer(reader[3].ToString() ?? string.Empty)}");
      con.Close();
    }
  }
  public static void HandlerList(string date)
  {
    Console.WriteLine($"Displaying Data from {date}");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"# {StringSpacer("ID")}{StringSpacer("Date")}{StringSpacer("Description")}{StringSpacer("Price(egp)")}");
    Console.ResetColor();
    using (SqliteCommand cmd = new(@$"
    SELECT * FROM expenses 
    WHERE STRFTIME('%Y', date) = STRFTIME('%Y', @date) 
    AND STRFTIME('%m', date) = STRFTIME('%m', @date) 
    ORDER BY date ASC;", con))
    {
      cmd.Parameters.AddWithValue("@date", date);
      con.Open();
      SqliteDataReader reader = cmd.ExecuteReader();
      while (reader.Read())
        Console.WriteLine($"# {StringSpacer(reader[0].ToString() ?? string.Empty)}{StringSpacer(reader[1].ToString() ?? string.Empty)}{StringSpacer(reader[2].ToString() ?? string.Empty)}{StringSpacer(reader[3].ToString() ?? string.Empty)}");
      con.Close();
    }
  }
  public static void HandlerDelete(List<int> IDList)
  {
    try
    {
      string StringOfIDs = $"{IDList[0]}";
      for (int i = 1; i < IDList.Count; i++)
        StringOfIDs += $", {IDList[i]}";
      using (SqliteCommand cmd = new($"DELETE FROM expenses WHERE id IN ({StringOfIDs});", con))
      {
        //TODO: Add parameterization
        con.Open();
        if (cmd.ExecuteNonQuery() == 0)
        {
          con.Close();
          Console.WriteLine($"Please Check the IDs and try again.\nExiting...");
          return;
        }

        Console.WriteLine("--------------------");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"Deleted ID: {StringOfIDs}");
        Console.WriteLine();
        Console.ResetColor();
        Console.WriteLine("--------------------");
        HandlerList();
      }
    }
    catch (Exception e)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write("Error:");
      Console.ResetColor();
      Console.WriteLine($"{e}");
    }
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
  public static string StringSpacer(string str)
  {
    if (int.TryParse(str, out int num) || str == "ID" || str == "Price")
    {
      while (str.Length <= 5)
      {
        str += " ";
      }
      return str;
    }
    while (str.Length <= 14)
    {
      str += " ";
    }
    return str;
  }
}

//TODO:
// By default we will use today's date when adding
// It's possible to update the data
// And if the user inserts a custom date then we'll use it