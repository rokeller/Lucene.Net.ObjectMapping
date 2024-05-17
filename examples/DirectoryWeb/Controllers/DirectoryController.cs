using DirectoryWeb.Models;
using DirectoryWeb.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryWeb.Controllers;

public class DirectoryController : Controller
{
    private const int PageSize = 25;

    private readonly ILogger<DirectoryController> logger;
    private readonly IDirectoryRepository directory;

    public DirectoryController(
        ILogger<DirectoryController> logger,
        IDirectoryRepository directory)
    {
        this.logger = logger;
        this.directory = directory;
    }

    public IActionResult Index()
    {
        return View(directory.GetPeople(page: 0, pageSize: PageSize));
    }

    public IActionResult Search()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Search(
        [Bind("Name")] SearchRequest request)
    {
        if (ModelState.IsValid)
        {
            IReadOnlyCollection<Person> results = directory.QueryPeople(
                p => p.FirstName == request.Name || p.LastName == request.Name);

            return View("SearchResults", results);
        }

        return View(request);
    }

    public IActionResult Delete(Guid id)
    {
        Person? person = directory.Get(id);

        if (null == person)
        {
            return NotFound();
        }

        return View(directory.Get(id));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid id)
    {
        directory.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Seed()
    {
        logger.LogInformation("Seeding directory ...");
        directory.Clean();
        directory.Add(new Teacher()
        {
            Id = Guid.NewGuid(),
            FirstName = "Walter",
            LastName = "White",
            YearOfBirth = 1958,
            Subjects = ["Chemistry",],
        });

        directory.Add(new Teacher()
        {
            Id = Guid.NewGuid(),
            FirstName = "Carmen",
            LastName = "Molina",
            YearOfBirth = 1973,
            Subjects = ["Chemistry",],
        });

        directory.Add(new Staff()
        {
            Id = Guid.NewGuid(),
            FirstName = "Hugo",
            LastName = "Archilleya",
            YearOfBirth = 1977,
            Occupation = Occupation.Janitor,
        });

        directory.Add(new Student()
        {
            Id = Guid.NewGuid(),
            FirstName = "Jesse",
            LastName = "Pinkman",
            YearOfBirth = 1984,
            Active = false,
        });
        directory.Add(new Student()
        {
            Id = Guid.NewGuid(),
            FirstName = "Walter, Jr.",
            LastName = "White",
            YearOfBirth = 1993,
            Grade = 8,
            Active = true,
        });

        return View();
    }
}
