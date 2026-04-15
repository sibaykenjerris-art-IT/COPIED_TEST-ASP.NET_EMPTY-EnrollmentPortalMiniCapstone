namespace YourProjectName.Models
{
    public class Student
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Contact { get; set; }

        public string Course { get; set; }

        public string Year { get; set; } // ✅ FIXED (STRING)

        public string Address { get; set; }
    }
}

