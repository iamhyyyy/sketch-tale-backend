using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;

namespace SmartCarWash.Application.Services
{
    public class CreditCostService : ICreditCostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreditCostService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CreditCostDto>> GetAllAsync()
        {
            var items = await _unitOfWork.CreditCostRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<CreditCostDto>>(items);
        }

        public async Task<CreditCostDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.CreditCostRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<CreditCostDto>(item);
        }

        public async Task<CreditCostDto> AddAsync(CreateCreditCostDto dto)
        {
            var item = _mapper.Map<CreditCost>(dto);

            await _unitOfWork.CreditCostRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CreditCostDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateCreditCostDto dto)
        {
            var item = await _unitOfWork.CreditCostRepository.GetByIdAsync(id);
            if (item == null) return false;

            _mapper.Map(dto, item);

            _unitOfWork.CreditCostRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var item = await _unitOfWork.CreditCostRepository.GetByIdAsync(id);
            if (item == null) return false;

            item.IsActive = false;

            _unitOfWork.CreditCostRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}