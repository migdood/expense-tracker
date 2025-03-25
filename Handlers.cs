using Microsoft.Data.Sqlite;

public partial class Program
{
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
        SqliteCommand cmd2 = new("SELECT MAX(id) FROM expenses;", con);
        SqliteDataReader reader = cmd2.ExecuteReader();
        reader.Read();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"Added Successfully, ID: ");
        Console.ResetColor();
        Console.WriteLine($"{reader[0]}");
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
        SqliteCommand cmd2 = new("SELECT MAX(id) FROM expenses;", con);
        SqliteDataReader reader = cmd2.ExecuteReader();
        reader.Read();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"Added Successfully, ID: ");
        Console.ResetColor();
        Console.WriteLine($"{reader[0]}");
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
        con.Close();
      }
    }
    catch (Exception e)
    {
      Console.WriteLine($"Error: {e}");
    }
  }
  public static void HandlerList(DateTime? date = null)
  {
    Console.WriteLine($"Displaying Data from {date?.ToString("yyyy-MM") ?? DateTime.Today.ToString("yyyy-MM")}");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine(@$"# {StringSpacer("ID", 5)}{StringSpacer("Date", 14)}{StringSpacer("Description", 14)}{StringSpacer("Price(egp)", 5)}");
    Console.ResetColor();
    using (SqliteCommand cmd = new(@$"
    SELECT * FROM expenses 
    WHERE STRFTIME('%Y', date) = STRFTIME('%Y', @date) 
    AND STRFTIME('%m', date) = STRFTIME('%m', @date) 
    ORDER BY date ASC;", con))
    {
      cmd.Parameters.AddWithValue("@date", date?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd"));
      con.Open();
      SqliteDataReader reader = cmd.ExecuteReader();
      while (reader.Read())
        Console.WriteLine(@$"# {StringSpacer(reader[0].ToString() ?? string.Empty, 5)}{StringSpacer(reader[1].ToString() ?? string.Empty, 14)}{StringSpacer(reader[2].ToString() ?? string.Empty, 14)}{StringSpacer(reader[3].ToString() ?? string.Empty, 5)}");
      con.Close();
    }
  }
  public static void HandlerSummary(DateTime? date = null)
  {
    Console.WriteLine($"Displaying summary for {date?.ToString("yyyy-MM") ?? DateTime.Today.ToString("yyyy-MM")}");
    try
    {
      using (SqliteCommand cmd = new(@$"
    SELECT COUNT(id), SUM(price) FROM expenses 
    WHERE STRFTIME('%Y', date) = STRFTIME('%Y', @date) 
      AND STRFTIME('%m', date) = STRFTIME('%m', @date);", con))
      {
        cmd.Parameters.AddWithValue("@date", date?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd"));
        con.Open();
        SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
          Console.ForegroundColor = ConsoleColor.DarkYellow;
          Console.Write(StringSpacer("Total Bills: ", 20));
          Console.ResetColor();
          Console.WriteLine($"{reader[0].ToString() ?? string.Empty}");
          Console.ForegroundColor = ConsoleColor.DarkYellow;
          Console.Write(StringSpacer("Total Amount Paid: ", 20));
          Console.ResetColor();
          Console.WriteLine($"{reader[1].ToString() ?? string.Empty} EGP");
        }
        con.Close();
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
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
}