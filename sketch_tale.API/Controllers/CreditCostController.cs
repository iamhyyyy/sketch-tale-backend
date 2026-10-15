using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/credit-costs")]
    public class CreditCostController : ControllerBase
    {
        private readonly ICreditCostService _creditCostService;

        public CreditCostController(ICreditCostService CreditCostService)
        {
            _creditCostService = CreditCostService;
        } 

        [HttpGet]
        public async Task<ActionResult<List<CreditCostDto>>> GetAll()
        {
            var items = await _creditCostService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CreditCostDto>> GetById(Guid id)
        {
            var item = await _creditCostService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<CreateCreditCostDto>> Create(CreateCreditCostDto dto)
        {
            var item = await _creditCostService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateCreditCostDto dto)
        {
            var item = await _creditCostService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _creditCostService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}