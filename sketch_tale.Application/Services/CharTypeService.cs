using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.Services
{
    public class CharTypeService : ICharTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CharTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CharTypeDto>> GetAllAsync()
        {
            var items = await _unitOfWork.CharTypeRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<CharTypeDto>>(items);
        }

        public async Task<CharTypeDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.CharTypeRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<CharTypeDto>(item);
        }

        public async Task<CharTypeDto> AddAsync(CreateCharTypeDto dto)
        {
            var item = _mapper.Map<CharType>(dto);

            await _unitOfWork.CharTypeRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CharTypeDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateCharTypeDto dto)
        {
            var item = await _unitOfWork.CharTypeRepository.GetByIdAsync(id);
            if (item == null) return false;

            _mapper.Map(dto, item);

            _unitOfWork.CharTypeRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var item = await _unitOfWork.CharTypeRepository.GetByIdAsync(id);
            if (item == null) return false;

            item.Status = CommonStatus.Inactive;

            _unitOfWork.CharTypeRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}