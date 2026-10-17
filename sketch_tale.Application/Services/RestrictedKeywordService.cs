using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace SmartCarWash.Application.Services
{
    public class RestrictedKeywordService : IRestrictedKeywordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RestrictedKeywordService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RestrictedKeywordDto>> GetAllAsync()
        {
            var items = await _unitOfWork.RestrictedKeywordRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<RestrictedKeywordDto>>(items);
        }

        public async Task<RestrictedKeywordDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.RestrictedKeywordRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<RestrictedKeywordDto>(item);
        }

        public async Task<RestrictedKeywordDto> AddAsync(CreateRestrictedKeywordDto dto)
        {
            var item = _mapper.Map<RestrictedKeyword>(dto);



            await _unitOfWork.RestrictedKeywordRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<RestrictedKeywordDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateRestrictedKeywordDto dto)
        {
            var item = await _unitOfWork.RestrictedKeywordRepository.GetByIdAsync(id);
            if (item == null) return false;

            _mapper.Map(dto, item);

            _unitOfWork.RestrictedKeywordRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var item = await _unitOfWork.RestrictedKeywordRepository.GetByIdAsync(id);
            if (item == null) return false;

            item.Status = CommonStatus.Inactive;

            _unitOfWork.RestrictedKeywordRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}