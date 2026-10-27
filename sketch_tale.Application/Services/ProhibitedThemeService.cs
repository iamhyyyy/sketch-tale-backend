using AutoMapper;
using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;

namespace sketch_tale.Application.Services
{
    public class ProhibitedThemeService : IProhibitedThemeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;

        public ProhibitedThemeService(IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authService = authService;
        }

        public async Task<IEnumerable<ProhibitedThemeDto>> GetAllAsync()
        {
            var items = await _unitOfWork.ProhibitedThemeRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<ProhibitedThemeDto>>(items);
        }

        public async Task<ProhibitedThemeDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.ProhibitedThemeRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<ProhibitedThemeDto>(item);
        }

        public async Task<ProhibitedThemeDto> AddAsync(CreateProhibitedThemeDto dto)
        {
            var item = _mapper.Map<ProhibitedTheme>(dto);

            var currentUserRole = await _authService.GetCurrentUserRoleAsync();

            if (string.Equals(currentUserRole, "contentmanager", StringComparison.OrdinalIgnoreCase))
                if(item.Status != CommonStatus.Draft && item.Status != CommonStatus.Pending)
                    throw new BadHttpRequestException("Bạn chỉ được khởi tạo với trạng thái bản nháp hoặc đợi duyệt");
            

            await _unitOfWork.ProhibitedThemeRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ProhibitedThemeDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateProhibitedThemeDto dto)
        {
            var item = await _unitOfWork.ProhibitedThemeRepository.GetByIdAsync(id);
            if (item == null) return false;

            var currentUserRole = await _authService.GetCurrentUserRoleAsync();

            if (string.Equals(currentUserRole, "contentmanager", StringComparison.OrdinalIgnoreCase))
            {
                if (item.Status != CommonStatus.Draft && item.Status != CommonStatus.Pending)
                    throw new UnauthorizedAccessException("Bạn không có quyền cập nhật mục này");
                if (dto.Status != CommonStatus.Draft && dto.Status != CommonStatus.Pending)
                    throw new BadHttpRequestException("Bạn chỉ được cập nhật với trạng thái bản nháp hoặc đợi duyệt");
            }

            _mapper.Map(dto, item);

            _unitOfWork.ProhibitedThemeRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var item = await _unitOfWork.ProhibitedThemeRepository.GetByIdAsync(id);
            if (item == null) return false;

            var currentUserRole = await _authService.GetCurrentUserRoleAsync();

            if (string.Equals(currentUserRole, "contentmanager", StringComparison.OrdinalIgnoreCase))
            {
                if (item.Status != CommonStatus.Draft && item.Status != CommonStatus.Pending)
                    throw new BadHttpRequestException("Bạn không có quyền xóa mục này");
            }

            item.Status = CommonStatus.Inactive;

            _unitOfWork.ProhibitedThemeRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}