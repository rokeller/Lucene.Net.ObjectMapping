namespace DirectoryWeb.Models;

public class Person
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public int YearOfBirth { get; set; }
}
