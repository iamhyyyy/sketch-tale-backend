using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;

namespace SmartCarWash.Application.Services
{
    public class EducationThemeService : IEducationThemeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EducationThemeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EducationThemeDto>> GetAllAsync()
        {
            var items = await _unitOfWork.EducationThemeRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<EducationThemeDto>>(items);
        }

        public async Task<EducationThemeDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.EducationThemeRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<EducationThemeDto>(item);
        }

        public async Task<EducationThemeDto> AddAsync(CreateEducationThemeDto dto)
        {
            var item = _mapper.Map<EducationTheme>(dto);
            item.Id = Guid.NewGuid();
            await _unitOfWork.EducationThemeRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<EducationThemeDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateEducationThemeDto dto)
        {
            var item = await _unitOfWork.EducationThemeRepository.GetByIdAsync(id);
            if (item == null) return false;

            _mapper.Map(dto, item);

            _unitOfWork.EducationThemeRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}