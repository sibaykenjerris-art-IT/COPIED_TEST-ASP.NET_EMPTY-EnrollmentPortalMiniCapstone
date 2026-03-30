using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();


// ==============================
// DATABASE SETUP (NEW) - SQLite
// ==============================
string dbPath = Path.Combine(app.Environment.ContentRootPath, "Data", "students.db");
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "Data")); // Ensure Data folder exists

using (var connection = new SqliteConnection($"Data Source={dbPath}"))
{
    connection.Open();

    var tableCmd = connection.CreateCommand();
    tableCmd.CommandText =
    @"CREATE TABLE IF NOT EXISTS Students (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT,
        Email TEXT,
        Contact TEXT,
        Course TEXT,
        Year TEXT,
        Address TEXT,
        Status TEXT,
        Remarks TEXT
    );";

    tableCmd.ExecuteNonQuery();
}


// ==============================
// STAFF LOGIN (REPLACES ADMIN)
// ==============================
app.MapPost("/staff-login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    string username = form["username"].ToString(); // NEW: cast to string
    string password = form["password"].ToString(); // NEW: cast to string

    if (username == "staff" && password == "1234")
    {
        context.Response.Redirect("/staff.html");
    }
    else
    {
        context.Response.Redirect("/staff-login.html?error=1");
    }
});


// ==============================
// ENROLLMENT SAVE (SQLITE)
// ==============================
app.MapPost("/enroll", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText =
    @"INSERT INTO Students 
    (Name, Email, Contact, Course, Year, Address, Status, Remarks)
    VALUES ($name, $email, $contact, $course, $year, $address, 'Pending', 'Waiting Verification');";

    // NEW: Cast StringValues to string to avoid InvalidOperationException
    cmd.Parameters.AddWithValue("$name", form["Name"].ToString());
    cmd.Parameters.AddWithValue("$email", form["Email"].ToString());
    cmd.Parameters.AddWithValue("$contact", form["Contact"].ToString());
    cmd.Parameters.AddWithValue("$course", form["Course"].ToString());
    cmd.Parameters.AddWithValue("$year", form["Year"].ToString());
    cmd.Parameters.AddWithValue("$address", form["Address"].ToString());

    cmd.ExecuteNonQuery();

    context.Response.Redirect("/success.html"); // NEW: redirect to a success page after registration
});


// ==============================
// GET STUDENTS (FOR STAFF TABLE)
// ==============================
app.MapGet("/students", async () =>
{
    var list = new List<object>();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT * FROM Students";

    using var reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        list.Add(new
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Contact = reader.GetString(3),
            Course = reader.GetString(4),
            Year = reader.GetString(5),
            Address = reader.GetString(6),
            Status = reader.GetString(7),
            Remarks = reader.GetString(8)
        });
    }

    return Results.Json(list);
});


// ==============================
// UPDATE STATUS (APPROVE / REJECT)
// ==============================
app.MapPost("/update-status", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var idValue = form["id"].ToString();

    if (!int.TryParse(idValue, out int studentId))
    {
        context.Response.Redirect("/staff.html"); // FIX
        return; // VERY IMPORTANT
    }

    var cmd = connection.CreateCommand();
    cmd.CommandText =
    @"UPDATE Students 
      SET Status = $status, Remarks = $remarks
      WHERE Id = $id";

    cmd.Parameters.AddWithValue("$status", form["status"].ToString());
    cmd.Parameters.AddWithValue("$remarks", form["remarks"].ToString());
    cmd.Parameters.AddWithValue("$id", studentId);

    cmd.ExecuteNonQuery();

    context.Response.Redirect("/staff.html");
});


app.Run();