using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Enable static files (HTML, CSS, JS)
app.UseDefaultFiles();
app.UseStaticFiles();


// ==============================
// STEP 1: DATABASE SETUP
// ==============================
string dbPath = Path.Combine(app.Environment.ContentRootPath, "Data", "students.db");

// Make sure folder exists
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "Data"));

// Create table if not exists
using (var connection = new SqliteConnection($"Data Source={dbPath}"))
{
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText =
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

    cmd.ExecuteNonQuery();
}


// ==============================
// STEP 2: STAFF LOGIN
// ==============================
app.MapPost("/staff-login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    string username = form["username"];
    string password = form["password"];

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
// STEP 3: ENROLL STUDENT
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

    cmd.Parameters.AddWithValue("$name", form["Name"].ToString());
    cmd.Parameters.AddWithValue("$email", form["Email"].ToString());
    cmd.Parameters.AddWithValue("$contact", form["Contact"].ToString());
    cmd.Parameters.AddWithValue("$course", form["Course"].ToString());
    cmd.Parameters.AddWithValue("$year", form["Year"].ToString());
    cmd.Parameters.AddWithValue("$address", form["Address"].ToString());

    cmd.ExecuteNonQuery();

    context.Response.Redirect("/success.html");
});


// ==============================
// STEP 4: GET STUDENTS (API)
// ==============================
app.MapGet("/students", () =>
{
    var students = new List<object>();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT * FROM Students";

    using var reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        students.Add(new
        {
            id = reader.GetInt32(0),
            name = reader.GetString(1),
            email = reader.GetString(2),
            contact = reader.GetString(3),
            course = reader.GetString(4),
            year = reader.GetString(5),
            address = reader.GetString(6),
            status = reader.GetString(7),
            remarks = reader.GetString(8)
        });
    }

    return Results.Json(students);
});


// ==============================
// STEP 5: UPDATE STATUS
// ==============================
app.MapPost("/update-status", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    int id = int.Parse(form["id"]);

    var cmd = connection.CreateCommand();
    cmd.CommandText =
    @"UPDATE Students 
      SET Status = $status, Remarks = $remarks
      WHERE Id = $id";

    cmd.Parameters.AddWithValue("$status", form["status"].ToString());
    cmd.Parameters.AddWithValue("$remarks", form["remarks"].ToString());
    cmd.Parameters.AddWithValue("$id", id);

    cmd.ExecuteNonQuery();

    return Results.Ok(); // IMPORTANT
});


// ==============================
// STEP 6: DELETE STUDENT
// ==============================
app.MapPost("/delete-student", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    int id = int.Parse(form["id"]);

    var cmd = connection.CreateCommand();
    cmd.CommandText = "DELETE FROM Students WHERE Id = $id";

    cmd.Parameters.AddWithValue("$id", id);

    cmd.ExecuteNonQuery();

    return Results.Ok(); // IMPORTANT
});

app.Run();