namespace DirectoryWeb.Models;

public sealed class Student : Person
{
    public int? Grade { get; set; }
    public bool Active { get; set; }
}
