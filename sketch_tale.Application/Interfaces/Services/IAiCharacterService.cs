using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IAiCharacterService
    {
        // Dùng cho Giai đoạn 1: Phân tích ảnh và trả về kết quả nhận diện
        Task<AiAnalyzeResult> AnalyzeImageAsync(string imageUrl);

        // Dùng cho Giai đoạn 2: Trả về URL ảnh nhân vật được AI sinh ra
        Task<string> GenerateImageAsync(string finalPrompt, string originalImageUrl);
    }

    // Record phụ trợ dùng nội bộ giữa Service AI và Workflow Service
    public class AiAnalyzeResult
    {
        public bool IsRecognized { get; set; }
        public List<Guid> SuggestedCharTypeIds { get; set; } = new();
        public string Prompt { get; set; }
    }
}
