using System.Collections.Immutable;
using System.Linq.Expressions;
using DirectoryWeb.Analyzers;
using DirectoryWeb.Models;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace DirectoryWeb.Repositories;


internal sealed class LuceneDirectoryRepository : IDirectoryRepository, IDisposable
{
    private static readonly TimeSpan flushInterval = TimeSpan.FromSeconds(5);

    private readonly ILogger<LuceneDirectoryRepository> logger;
    private readonly Lucene.Net.Store.Directory luceneDir;
    private readonly IndexWriter writer;
    private readonly SearcherManager searcherManager;
    private readonly Timer timer;
    private readonly object syncRoot = new object();

    private bool disposedValue;
    private bool dirty;

    public LuceneDirectoryRepository(ILogger<LuceneDirectoryRepository> logger)
    {
        this.logger = logger;

        DirectoryInfo root = new DirectoryInfo("data/directory");
        FSDirectory fsDir = FSDirectory.Open(root.FullName);
        luceneDir = new NRTCachingDirectory(fsDir, 5, 20);

        IndexWriterConfig config = new IndexWriterConfig(
            Consts.LuceneVersion, DirectoryAnalyzer.Default)
        {
            OpenMode = OpenMode.CREATE_OR_APPEND,
        };

        writer = new IndexWriter(luceneDir, config);
        searcherManager = new SearcherManager(writer, applyAllDeletes: true, null);

        timer = new Timer(OnTimer, null, flushInterval, flushInterval);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                searcherManager.Dispose();
                writer.Dispose();
                luceneDir.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #region IDirectory Implementation

    public void Add(Person person)
    {
        writer.Add(person);
        MarkDirty();
    }

    public void Update(Person person)
    {
        writer.Update(person, p => p.Id == person.Id);
        MarkDirty();
    }

    public void Delete(Guid personId)
    {
        writer.Delete<Person>(p => p.Id == personId);
        MarkDirty();
    }

    public void Clean()
    {
        writer.DeleteAll();
        MarkDirty();
    }

    public Person? Get(Guid personId)
    {
        IReadOnlyCollection<Person> results = QueryPaged<Person>(
            0, 2,
            (q) => q.Where(p => p.Id == personId));

        if (results.Count == 1)
        {
            return results.First();
        }

        return null;
    }

    public IReadOnlyCollection<Person> QueryPeople(
        Expression<Func<Person, bool>> predicate,
        int page = 0,
        int pageSize = 10)
    {
        return QueryPaged<Person>(
            page,
            pageSize,
            (q) => q.Where(predicate)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName));
    }

    public IReadOnlyCollection<Person> GetPeople(int page = 0, int pageSize = 10)
    {
        return QueryPaged<Person>(
            page,
            pageSize,
            (q) => q.OrderBy(p => p.LastName).ThenBy(p => p.FirstName));
    }

    public IReadOnlyCollection<Staff> GetStaff(int page = 0, int pageSize = 10)
    {
        return QueryPaged<Staff>(
            page,
            pageSize,
            (q) => q.OrderBy(p => p.LastName).ThenBy(p => p.FirstName));
    }

    public IReadOnlyCollection<Teacher> GetTeachers(int page = 0, int pageSize = 10)
    {
        return QueryPaged<Teacher>(
            page,
            pageSize,
            (q) => q.OrderBy(p => p.LastName).ThenBy(p => p.FirstName));
    }

    public IReadOnlyCollection<Student> GetStudents(int page = 0, int pageSize = 10)
    {
        return QueryPaged<Student>(
            page,
            pageSize,
            (q) => q.OrderBy(p => p.LastName).ThenBy(p => p.FirstName));
    }

    #endregion

    private IReadOnlyCollection<T> QueryPaged<T>(
        int page,
        int pageSize,
        Func<IQueryable<T>, IQueryable<T>> query) where T : Person
    {
        IndexSearcher searcher = searcherManager.Acquire();

        try
        {
            return query(searcher.AsQueryable<T>())
                .Skip(pageSize * page)
                .Take(pageSize)
                .ToImmutableList();
        }
        finally
        {
            searcherManager.Release(searcher);
        }
    }

    private void OnTimer(object? state)
    {
        lock (syncRoot)
        {
            if (!dirty)
            {
                return;
            }

            logger.LogInformation("Directory is dirty, commit now.");
            dirty = false;
        }

        writer.Commit();
        searcherManager.MaybeRefresh();
        logger.LogInformation("Directory committed.");
    }

    private void MarkDirty()
    {
        lock (syncRoot)
        {
            dirty = true;
        }
    }
}
