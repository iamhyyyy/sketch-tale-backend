using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Services
{
    public class CreditTransactionLogService : ICreditTransactionLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreditTransactionLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CreditTransactionLogDto>> GetAllAsync()
        {
            var items = await _unitOfWork.CreditTransactionLogRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<CreditTransactionLogDto>>(items);
        }

        public async Task<CreditTransactionLogDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.CreditTransactionLogRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<CreditTransactionLogDto>(item);
        }

        public async Task<List<CreditTransactionLogDto>> GetByParentIdAsync(Guid parentId)
        {
            var items = await _unitOfWork.CreditTransactionLogRepository.GetByParentIdAsync(parentId);

            return _mapper.Map<List<CreditTransactionLogDto>>(items);
        }

        public async Task<List<CreditTransactionLogDto>> GetByUserIdAsync(Guid userId)
        {
            var items = await _unitOfWork.CreditTransactionLogRepository.GetByUserIdAsync(userId);

            return _mapper.Map<List<CreditTransactionLogDto>>(items);
        }

        public async Task<CreditTransactionLogDto> AddAsync(CreateCreditTransactionLogDto dto)
        {
            var item = _mapper.Map<CreditTransactionLog>(dto);

            await _unitOfWork.CreditTransactionLogRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CreditTransactionLogDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateCreditTransactionLogDto dto)
        {
            var item = await _unitOfWork.CreditTransactionLogRepository.GetByIdAsync(id);
            if (item == null) return false;

            _mapper.Map(dto, item);

            _unitOfWork.CreditTransactionLogRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var item = await _unitOfWork.CreditTransactionLogRepository.GetByIdAsync(id);
            if (item == null) return false;

            _unitOfWork.CreditTransactionLogRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}