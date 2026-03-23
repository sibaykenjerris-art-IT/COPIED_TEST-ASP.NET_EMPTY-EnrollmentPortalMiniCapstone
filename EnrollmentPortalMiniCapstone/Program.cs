using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


// ==============================
// SET LOGIN AS FIRST PAGE
// ==============================
var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("login.html");
app.UseDefaultFiles(options);

app.UseStaticFiles();


// ==============================
// STUDENT LOGIN
// ==============================
app.MapPost("/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    string username = form["username"];
    string password = form["password"];

    if (username == "student" && password == "1234")
    {
        context.Response.Redirect("/index.html"); // success
    }
    else
    {
        context.Response.Redirect("/login.html?error=1"); // NEW
    }
});


// ==============================
// ADMIN LOGIN
// ==============================
app.MapPost("/admin-login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    string username = form["username"];
    string password = form["password"];

    if (username == "admin" && password == "admin123")
    {
        context.Response.Redirect("/admin.html"); // success
    }
    else
    {
        context.Response.Redirect("/admin-login.html?error=1"); // NEW
    }
});

// ==============================
// SAVE ENROLLMENT
// ==============================
app.MapPost("/enroll", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    var student = new
    {
        Name = form["Name"],
        Email = form["Email"],
        Contact = form["Contact"],
        Course = form["Course"],
        Year = form["Year"],
        Address = form["Address"]
    };

    string filePath = Path.Combine(app.Environment.ContentRootPath, "Data", "students.json");

    List<object> students = new List<object>();

    if (File.Exists(filePath))
    {
        string json = await File.ReadAllTextAsync(filePath);
        if (!string.IsNullOrWhiteSpace(json))
        {
            students = JsonSerializer.Deserialize<List<object>>(json) ?? new List<object>();
        }
    }

    students.Add(student);

    string newJson = JsonSerializer.Serialize(students, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    await File.WriteAllTextAsync(filePath, newJson);

    await context.Response.WriteAsync("Enrollment Successful!");
});


// ==============================
// GET STUDENTS (ADMIN TABLE)
// ==============================
app.MapGet("/students", async (HttpContext context) =>
{
    string filePath = Path.Combine(app.Environment.ContentRootPath, "Data", "students.json");

    if (File.Exists(filePath))
    {
        string json = await File.ReadAllTextAsync(filePath);
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json);
    }
});

app.Run();