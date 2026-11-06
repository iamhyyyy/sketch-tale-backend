using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Repositories
{
    public interface IDrawingRepository
    {
        // Lấy danh sách 85 loại nhân vật đang Active để đưa cho AI
        Task<List<CharTypeDto>> GetActiveCharTypesAsync();

        // Lưu lịch sử bản vẽ (ngay cả khi AI fail vẫn phải lưu để lấy ID)
        Task AddDrawingAsync(Drawing drawing);

        // Lấy thông tin bản vẽ cũ theo ID (dùng cho Giai đoạn 2)
        Task<Drawing> GetByIdAsync(Guid drawingId);

        // Lấy tên của CharType dựa vào ID (để nối vào Prompt tự chế nếu GĐ 1 fail)
        Task<string> GetCharTypeNameAsync(Guid charTypeId);
    }
}
