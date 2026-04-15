using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System;

// IMPORT YOUR MODEL
using YourProjectName.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


// ==============================
// REDIRECT TO WELCOME PAGE
// ==============================
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/welcome.html");
    return Task.CompletedTask;
});


// ==============================
// ENABLE STATIC FILES
// ==============================
app.UseDefaultFiles();
app.UseStaticFiles();


// ==============================
// DATABASE PATH
// ==============================
string dbPath = Path.Combine(app.Environment.ContentRootPath, "Data", "students.db");

// CREATE DATA FOLDER IF NOT EXIST
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "Data"));


// ==============================
// CREATE TABLE
// ==============================
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
// STAFF LOGIN
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
// ENROLL STUDENT (USES MODEL)
// ==============================
app.MapPost("/enroll", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    Student s = new Student
    {
        Name = form["Name"].ToString(),
        Email = form["Email"].ToString(),
        Contact = form["Contact"].ToString(),
        Course = form["Course"].ToString(),
        Year = form["Year"].ToString(),
        Address = form["Address"].ToString()
    };

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();

    cmd.CommandText =
    @"INSERT INTO Students 
    (Name, Email, Contact, Course, Year, Address, Status, Remarks)
    VALUES ($name, $email, $contact, $course, $year, $address, 'Pending', 'Waiting Verification');";

    cmd.Parameters.AddWithValue("$name", s.Name);
    cmd.Parameters.AddWithValue("$email", s.Email);
    cmd.Parameters.AddWithValue("$contact", s.Contact);
    cmd.Parameters.AddWithValue("$course", s.Course);
    cmd.Parameters.AddWithValue("$year", s.Year);
    cmd.Parameters.AddWithValue("$address", s.Address);

    cmd.ExecuteNonQuery();

    context.Response.Redirect("/success.html");
});


// ==============================
// GET STUDENTS
// ==============================
app.MapGet("/students", () =>
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

    return Results.Json(list);
});


// ==============================
// UPDATE STATUS
// ==============================
app.MapPost("/update-status", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    int id = int.Parse(form["id"]);
    string status = form["status"];
    string remarks = form["remarks"];

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText =
    @"UPDATE Students 
      SET Status = $status, Remarks = $remarks
      WHERE Id = $id";

    cmd.Parameters.AddWithValue("$status", status);
    cmd.Parameters.AddWithValue("$remarks", remarks);
    cmd.Parameters.AddWithValue("$id", id);

    cmd.ExecuteNonQuery();

    return Results.Ok();
});


// ==============================
// DELETE STUDENT
// ==============================
app.MapPost("/delete-student", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    int id = int.Parse(form["id"]);

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "DELETE FROM Students WHERE Id = $id";

    cmd.Parameters.AddWithValue("$id", id);

    cmd.ExecuteNonQuery();

    return Results.Ok();
});

app.Run();