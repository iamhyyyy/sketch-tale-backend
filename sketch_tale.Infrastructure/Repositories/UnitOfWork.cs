

using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;
using System.Collections;

namespace sketch_tale.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private Hashtable? _repositories;

    //Khai báo IRepo
    private ICategoryRepository _categoryRepository = null!;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        _repositories ??= new Hashtable();

        var type = typeof(T).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance =
                Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);

            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<T>)_repositories[type]!;
    }

    //thêm IRepo ở đây
    public ICategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
