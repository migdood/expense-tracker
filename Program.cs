using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Data.Sqlite;

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
        if (args.Length != 4)
        {
          Console.WriteLine("Incorrect usage of 'add'\n\nUsage 'add <date> <description> <price>'\n\n");
          return;
        }
        if (!int.TryParse(args[3], out int price))
        {
          Console.WriteLine("price needs to be an integer not\n\n");
        }
        HandlerAdd(args[1], args[2], price);
        break;
      case "list":
        HandlerList();
        break;
      case "delete":
        if (!int.TryParse(args[1], out int id))
        {
          Console.WriteLine("id must be an int.\n\n");
          return;
        }
        HandlerDelete(id);
        break;
      default:
        Console.WriteLine($"{args[0]} is not a command. Type --help for command list.");
        break;
    }
    #endregion
  }
  public static void HandlerAdd(string date, string description, int price)
  {
    try
    {
      if (!isDateValid(date))
      {
        Console.WriteLine("please make sure the date you enter follows the format:\n<yyyy-MM-dd>\n\n");
        return;
      }
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
  public static void HandlerList()
  {
    // TODO: 
    // format the strings to have a certain length to look like a table
    using (SqliteCommand cmd = new(@"SELECT * FROM expenses;", con))
    {
      con.Open();
      var reader = cmd.ExecuteReader();
      while (reader.Read())
        Console.WriteLine($"ID:{reader[0]} Date:{reader[1]} Description:{reader[2]} Price:{reader[3]}\n");
      con.Close();
    }
  }
  public static void HandlerDelete(int idNum)
  {
    try
    {
      using (SqliteCommand cmd = new("DELETE FROM expenses WHERE id=@id", con))
      {
        cmd.Parameters.AddWithValue("@id", idNum);
        con.Open();
        if (cmd.ExecuteNonQuery() == 0)
        {
          Console.WriteLine($"There is no ID {idNum}");
          return;
        }
        Console.WriteLine("--------------------");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Deleted ID:{idNum}");
        Console.ResetColor();
        Console.WriteLine("--------------------");
        HandlerList();
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
  public static bool isDateValid(string date)
  {
    string[] parts = date.Split('-');
    if (parts.Length != 3) { return false; }
    if (!(int.TryParse(parts[0], out int year) &&
          int.TryParse(parts[1], out int month) &&
          int.TryParse(parts[2], out int day)))
    {
      return false;
    }

    if (year < 1900 || month < 1 || month > 12 || day < 1 || day > 31) { return false; }

    return true;
  }

}
