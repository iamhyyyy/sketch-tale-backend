

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
    private IEducationThemeRepository _educationThemeRepository = null!;
    private IStoryTemplateRepository _storyTemplateRepository = null!;
    private IStoryPageTemplateRepository _storyPageTemplateRepository = null!;
    private IStoryRoleTemplateRepository _storyRoleTemplateRepository = null!;
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
    public IEducationThemeRepository EducationThemeRepository => _educationThemeRepository ??= new EducationThemeRepository(_context);
    public IStoryTemplateRepository StoryTemplateRepository => _storyTemplateRepository ??= new StoryTemplateRepository(_context);
    public IStoryPageTemplateRepository StoryPageTemplateRepository => _storyPageTemplateRepository ??= new StoryPageTemplateRepository(_context);


    public IStoryRoleTemplateRepository StoryRoleTemplateRepository => _storyRoleTemplateRepository ??= new StoryRoleTemplateRepository(_context);

    public ICharacterSlotTemplateRepository CharacterSlotTemplateRepository => throw new NotImplementedException();

    public IVocabularyTemplateRepository VocabularyTemplateRepository => throw new NotImplementedException();

    public IStoryQuizTemplateRepository StoryQuizTemplateRepository => throw new NotImplementedException();

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
