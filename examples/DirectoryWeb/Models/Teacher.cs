namespace DirectoryWeb.Models;

public sealed class Teacher : Staff
{
    public Teacher()
    {
        Occupation = Occupation.Teacher;
    }

    public string[]? Subjects { get; set; }
}
