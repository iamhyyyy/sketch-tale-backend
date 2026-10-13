using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;

namespace SmartCarWash.Application.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionPlanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubscriptionPlanDto>> GetAllAsync()
        {
            var items = await _unitOfWork.SubscriptionPlanRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<SubscriptionPlanDto>>(items);
        }

        public async Task<SubscriptionPlanDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<SubscriptionPlanDto>(item);
        }

        public async Task<SubscriptionPlanDto> AddAsync(CreateSubscriptionPlanDto dto)
        {
            var item = _mapper.Map<SubscriptionPlan>(dto);
            item.Id = Guid.NewGuid();
            await _unitOfWork.SubscriptionPlanRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<SubscriptionPlanDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateSubscriptionPlanDto dto)
        {
            var item = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(id);
            if (item == null) return false;

            _mapper.Map(dto, item);

            _unitOfWork.SubscriptionPlanRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var item = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(id);
            if (item == null) return false;

            item.IsActive = false;

            _unitOfWork.SubscriptionPlanRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}