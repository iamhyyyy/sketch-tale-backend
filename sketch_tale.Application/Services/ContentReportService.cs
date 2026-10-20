using AutoMapper;
using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;

namespace SmartCarWash.Application.Services
{
    public class ContentReportService : IContentReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;

        public ContentReportService(IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authService = authService;
        }

        public async Task<IEnumerable<ContentReportDto>> GetAllAsync()
        {
            var items = await _unitOfWork.ContentReportRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<ContentReportDto>>(items);
        }

        public async Task<ContentReportDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.ContentReportRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<ContentReportDto>(item);
        }

        public async Task<List<ContentReportDto>> GetByParentIdAsync(Guid parentId)
        {
            var items = await _unitOfWork.ContentReportRepository.GetByParentIdAsync(parentId);

            return _mapper.Map<List<ContentReportDto>>(items);
        }

        public async Task<ContentReportDto> AddAsync(CreateContentReportDto dto)
        {
            var item = _mapper.Map<ContentReport>(dto);

            var currentUserRole = await _authService.GetCurrentUserRoleAsync();

            if (string.Equals(currentUserRole, "parent", StringComparison.OrdinalIgnoreCase))
                item.Status = ContentReportStatus.Pending;
            
            await _unitOfWork.ContentReportRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ContentReportDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateContentReportDto dto)
        {
            var item = await _unitOfWork.ContentReportRepository.GetByIdAsync(id);
            if (item == null) return false;

            var currentUserRole = await _authService.GetCurrentUserRoleAsync();

            if (string.Equals(currentUserRole, "parent", StringComparison.OrdinalIgnoreCase))
            {
                if (item.Status != ContentReportStatus.Pending) throw new UnauthorizedAccessException("Bạn không có quyền cập nhật mục này");
                item.Status = ContentReportStatus.Pending;
            }

            _mapper.Map(dto, item);

            _unitOfWork.ContentReportRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var item = await _unitOfWork.ContentReportRepository.GetByIdAsync(id);
            if (item == null) return false;

            var currentUserRole = await _authService.GetCurrentUserRoleAsync();

            if (string.Equals(currentUserRole, "parent", StringComparison.OrdinalIgnoreCase))
                if (item.Status != ContentReportStatus.Pending) 
                    throw new BadHttpRequestException("Bạn không có quyền xóa mục này");

            item.Status = ContentReportStatus.Dismissed;

            _unitOfWork.ContentReportRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}