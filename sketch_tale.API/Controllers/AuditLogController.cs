using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService AuditLogService)
        {
            _auditLogService = AuditLogService;
        } 

        [HttpGet]
        public async Task<ActionResult<List<AuditLogDto>>> GetAll()
        {
            var items = await _auditLogService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLogDto>> GetById(Guid id)
        {
            var item = await _auditLogService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("user-id/{id}")]
        public async Task<ActionResult<List<AuditLogDto>>> GetByUserId(Guid id)
        {
            var items = await _auditLogService.GetByUserId(id);
            return Ok(items);
        }

        [HttpGet("entity-name/{name}")]
        public async Task<ActionResult<List<AuditLogDto>>> GetByEntityName(string name)
        {
            var items = await _auditLogService.GetByEntityName(name);
            return Ok(items);
        }

        [HttpGet("entity-id/{id}")]
        public async Task<ActionResult<List<AuditLogDto>>> GetByPrimaryKeyOfEntity(Guid id)
        {
            var items = await _auditLogService.GetByPrimaryKeyOfEntity(id);
            return Ok(items);
        }
    }
}