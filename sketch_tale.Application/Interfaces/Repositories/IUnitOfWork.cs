


using sketch_tale.Domain.Interfaces;

namespace sketch_tale.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> CompleteAsync();

    //thêm repo interface xún dứi

    IEducationThemeRepository EducationThemeRepository {  get; }
}
