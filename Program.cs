using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Data.Sqlite;

public class Program
{
  readonly static SqliteConnection con = new("Data Source=test.db");
  public static void Main(string[] args)
  {
    #region Arg Handling
    if (args.Length <= 0)
    {
      Console.WriteLine("Usage: expense-tracker <command> [arguments]");
      return;
    }

    using (var command = new SqliteCommand(@"
      CREATE TABLE IF NOT EXISTS users(
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      name TEXT, 
      age INTEGER);
      ", con))
    {
      con.Open();
      if (command.ExecuteNonQuery() > 0)
      {
        Console.WriteLine("Created Database.");
      }
      con.Close();
    }

    switch (args[0])
    {
      case "add":
        if (args.Length != 3)
        {
          Console.WriteLine("Adding must include Name and Age.");
          return;
        }
        if (!int.TryParse(args[2], out int age))
        {
          Console.WriteLine("Age Must be an int.");
          return;
        }
        HandlerAdd(args[1], age);
        break;
      case "list":
        HandlerList();
        break;
      case "delete":
        if (!int.TryParse(args[1], out int id))
        {
          Console.WriteLine("ID must be an int.");
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
  public static void HandlerAdd(string name, int age)
  {
    try
    {

      using (SqliteCommand cmd = new("INSERT INTO users(name, age) VALUES(@name, @age);", con))
      {
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@age", age);
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
    using (SqliteCommand cmd = new(@"SELECT * FROM users;", con))
    {
      con.Open();
      var reader = cmd.ExecuteReader();
      while (reader.Read())
        Console.WriteLine($"ID:{reader[0]}, Name:{reader[1]}, Age:{reader[2]}");
      con.Close();
    }
  }
  public static void HandlerDelete(int idNum)
  {
    try
    {
      using (SqliteCommand cmd = new("DELETE FROM users WHERE id=@id", con))
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
}
