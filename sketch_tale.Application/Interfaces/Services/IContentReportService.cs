using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces
{
    public interface IContentReportService
    {
        Task<IEnumerable<ContentReportDto>> GetAllAsync();
        Task<ContentReportDto?> GetByIdAsync(Guid id);
        Task<List<ContentReportDto>> GetByParentIdAsync(Guid parentId);
        Task<ContentReportDto> AddAsync(CreateContentReportDto dto);
        Task<bool> Update(Guid id, UpdateContentReportDto dto);
        Task<bool> Delete(Guid id);
    }
}