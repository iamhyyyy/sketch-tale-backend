using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace SmartCarWash.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
        {
            var items = await _unitOfWork.AuditLogRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(items);
        }

        public async Task<AuditLogDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.AuditLogRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<AuditLogDto>(item);
        }
        public async Task<List<AuditLogDto>> GetByUserId(Guid id)
        {
            var items = await _unitOfWork.AuditLogRepository.GetByUserId(id);

            return _mapper.Map<List<AuditLogDto>>(items);
        }
        public async Task<List<AuditLogDto>> GetByEntityName(string name)
        {
            var items = await _unitOfWork.AuditLogRepository.GetByEntityName(name);

            return _mapper.Map<List<AuditLogDto>>(items);
        }
        public async Task<List<AuditLogDto>> GetByPrimaryKeyOfEntity(Guid id)
        {
            var items = await _unitOfWork.AuditLogRepository.GetByPrimaryKeyOfEntity(id);

            return _mapper.Map<List<AuditLogDto>>(items);
        }

    }
}