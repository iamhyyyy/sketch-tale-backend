

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
    private ICreditCostRepository _creditCostRepository = null!;
    private ISubscriptionPlanRepository _subscriptionPlanRepository = null!;
    private ISubscriptionRepository _subscriptionRepository = null!;
    private IAuditLogRepository _auditLogRepository = null!;
    private IRestrictedKeywordRepository _restrictedKeywordRepository = null!;
    private IStoryTemplateRepository _storyTemplateRepository = null!;
    private IStoryPageTemplateRepository _storyPageTemplateRepository = null!;
    private IStoryRoleTemplateRepository _storyRoleTemplateRepository = null!;
    private ICharacterSlotTemplateRepository _characterSlotTemplateRepository = null!;
    private IStoryQuizTemplateRepository _storyQuizTemplateRepository = null!;
    private IVocabularyTemplateRepository _vocabularyTemplateRepository = null!;
    private IParentProfileRepository _parentProfileRepository = null!;
    private IChildProfileRepository _childProfileRepository = null!;
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
    public ICreditCostRepository CreditCostRepository => _creditCostRepository ??= new CreditCostRepository(_context);
    public ISubscriptionPlanRepository SubscriptionPlanRepository => _subscriptionPlanRepository ??= new SubscriptionPlanRepository(_context);
    public ISubscriptionRepository SubscriptionRepository => _subscriptionRepository ??= new SubscriptionRepository(_context);
    public IAuditLogRepository AuditLogRepository => _auditLogRepository ??= new AuditLogRepository(_context);
    public IRestrictedKeywordRepository RestrictedKeywordRepository => _restrictedKeywordRepository ??= new RestrictedKeywordRepository(_context);
    public IStoryTemplateRepository StoryTemplateRepository => _storyTemplateRepository ??= new StoryTemplateRepository(_context);
    public IStoryPageTemplateRepository StoryPageTemplateRepository => _storyPageTemplateRepository ??= new StoryPageTemplateRepository(_context);
    public IParentProfileRepository ParentProfileRepository => _parentProfileRepository ??= new ParentProfileRepository(_context);
    public IChildProfileRepository ChildProfileRepository => _childProfileRepository ??= new ChildProfileRepository(_context);

    public IStoryRoleTemplateRepository StoryRoleTemplateRepository => _storyRoleTemplateRepository ??= new StoryRoleTemplateRepository(_context);

    public ICharacterSlotTemplateRepository CharacterSlotTemplateRepository => _characterSlotTemplateRepository ??= new CharacterSlotTemplateRepository(_context);

    public IVocabularyTemplateRepository VocabularyTemplateRepository => _vocabularyTemplateRepository ??= new VocabularyTemplateRepository(_context);

    public IStoryQuizTemplateRepository StoryQuizTemplateRepository => _storyQuizTemplateRepository ??= new StoryQuizTemplateRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
