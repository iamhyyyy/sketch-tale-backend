using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/credit-transaction-logs")]
    public class CreditTransactionLogController : ControllerBase
    {
        private readonly ICreditTransactionLogService _creditTransactionLogService;

        public CreditTransactionLogController(ICreditTransactionLogService creditTransactionLogService)
        {
            _creditTransactionLogService = creditTransactionLogService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CreditTransactionLogDto>>> GetAll()
        {
            var items = await _creditTransactionLogService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CreditTransactionLogDto>> GetById(Guid id)
        {
            var item = await _creditTransactionLogService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<List<CreditTransactionLogDto>>> GetByUserIdAsync(Guid id)
        {
            var items = await _creditTransactionLogService.GetByUserIdAsync(id);
            return Ok(items);
        }

        [HttpGet("parent-profile/{id}")]
        public async Task<ActionResult<List<CreditTransactionLogDto>>> GetByParentIdAsync(Guid id)
        {
            var items = await _creditTransactionLogService.GetByParentIdAsync(id);
            return Ok(items);
        }

        //[HttpPost]
        //public async Task<ActionResult<CreateCreditTransactionLogDto>> Create(CreateCreditTransactionLogDto dto)
        //{
        //    var item = await _creditTransactionLogService.AddAsync(dto);
        //    return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        //}

    }
}