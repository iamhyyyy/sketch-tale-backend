using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Repositories
{
    public class DrawingRepository : IDrawingRepository
    {
        private readonly AppDbContext _context;

        public DrawingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CharTypeDto>> GetActiveCharTypesAsync()
        {
            return await _context.CharTypes
                .Where(x => x.Status == CommonStatus.Active)
                .Select(x => new CharTypeDto { Id = x.Id, Name = x.Name })
                .ToListAsync();
        }

        public async Task AddDrawingAsync(Drawing drawing)
        {
            await _context.Drawings.AddAsync(drawing);
            await _context.SaveChangesAsync();
        }

        public async Task<Drawing> GetByIdAsync(Guid drawingId)
        {
            return await _context.Drawings.FindAsync(drawingId);
        }

        public async Task<string> GetCharTypeNameAsync(Guid charTypeId)
        {
            var charType = await _context.CharTypes.FindAsync(charTypeId);
            return charType?.Name ?? "Nhân vật";
        }
    }
}
