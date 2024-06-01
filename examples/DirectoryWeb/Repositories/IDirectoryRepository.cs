using System.Linq.Expressions;
using DirectoryWeb.Models;

namespace DirectoryWeb.Repositories;

public interface IDirectoryRepository
{
    void Add(Person person);
    void Update(Person person);
    void Delete(Guid personId);
    void Clean();

    Person? Get(Guid personId);

    IReadOnlyCollection<Person> QueryPeople(
        Expression<Func<Person, bool>> predicate,
        int page = 0,
        int pageSize = 10);

    IReadOnlyCollection<Person> GetPeople(int page = 0, int pageSize = 10);
    IReadOnlyCollection<Staff> GetStaff(int page = 0, int pageSize = 10);
    IReadOnlyCollection<Teacher> GetTeachers(int page = 0, int pageSize = 10);
    IReadOnlyCollection<Student> GetStudents(int page = 0, int pageSize = 10);
}
