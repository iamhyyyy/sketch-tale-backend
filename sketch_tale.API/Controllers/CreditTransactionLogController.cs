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

        [HttpGet("users/{Id}")]
        public async Task<ActionResult<List<CreditTransactionLogDto>>> GetByUserIdAsync(Guid Id)
        {
            var items = await _creditTransactionLogService.GetByUserIdAsync(Id);
            return Ok(items);
        }

        [HttpGet("parent-profile/{Id}")]
        public async Task<ActionResult<List<CreditTransactionLogDto>>> GetByParentIdAsync(Guid Id)
        {
            var items = await _creditTransactionLogService.GetByParentIdAsync(Id);
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