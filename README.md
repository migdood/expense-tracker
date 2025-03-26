## How to use
```Bash
$ expense-tracker add Lunch 20
# Added Successfully, ID: 1

$ expense-tracker add 2025-03-01 Dinner 10
# Added Successfully, ID: 2

$ expense-tracker list
# ID  Date       Description  Price(egp)
# 1   2025-03-06  Lunch        20 
# 2   2025-03-07  Dinner       10

$ expense-tracker summary
# Displaying summary for 2025-03
# Total Bills:         11
# Total Amount Paid:   3423 EGP

$ expense-tracker delete 2
# Expense deleted successfully

$ expense-tracker update 1 2025-03-02 Lunch 25
# Updated ID:67
```

## TLDR;
Enjoy the app or suggest something.

## Expenses Tracker in CLI
I wanted to build something simple as my first console app, and I picked this. It is a simple expenses tracker that saves your expenses under date, description and price. You are able to update or remove entries, view a summary based on the month or see the full list of what your expenses were for a particular month. There were a lot of firsts for me during development, like SQLite, method overloading and nullable types, but it was very enjoyable coming up with the solutions.

### Technologies
- C# (.NET 8.0.406)
- SQLite (3.37.2)

### Planning & Decisions
This is meant to be something short and very simple, intended for use by anyone. For that reason SQLite was picked. I wanted to try something new so instead of writing to a file, I used a serverless-Database. 

I've never written a console app before so there were some discoveries; like when I found out I used the positional argument approach rather than the more common *"flags/options with associated values"* approach. I decided to keep it since it was a very simple app.

My first iteration was to use method overloading for all the default values, however I couldn't stand how inefficient it was to write the same exact method just using default values. I decided to go with making data types nullable "?" and using the if null operator "??", this turned out to be the best approach because I didn't need to duplicate methods to accommodate default values. Both were a great learning experience and will be added to my tool belt permanently. 

The original task wanted me to write the data to a txt file or use JSON, I was interested in SQLite for a while but never used it. This looked like the perfect opportunity and it was. Learning that SQLite doesn't force data types but they are more of a suggestion was a surprise, I needed to use `INTEGER` to allow my id column to use `AUTOINCREMENT`. It also doesn't have a `DATE` data type however supports the date functions which is super nice. But beyond that it was smooth sailing, learned how to use the basic SQLite commands like .tables, `PRAGMA table_info(table_name);`, and .q; this was all that's needed for this simple CRUD app.