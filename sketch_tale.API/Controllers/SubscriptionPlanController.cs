using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/subscription-plans")]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;

        public SubscriptionPlanController(ISubscriptionPlanService SubscriptionPlanService)
        {
            _subscriptionPlanService = SubscriptionPlanService;
        } 

        [HttpGet]
        public async Task<ActionResult<List<SubscriptionPlanDto>>> GetAll()
        {
            var items = await _subscriptionPlanService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionPlanDto>> GetById(Guid id)
        {
            var item = await _subscriptionPlanService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("code/{plan-code}")]
        public async Task<ActionResult<SubscriptionPlanDto>> GetByPlanCode(string planCode)
        {
            var item = await _subscriptionPlanService.GetPlanByCode(planCode);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<CreateSubscriptionPlanDto>> Create(CreateSubscriptionPlanDto dto)
        {
            var item = await _subscriptionPlanService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateSubscriptionPlanDto dto)
        {
            var item = await _subscriptionPlanService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _subscriptionPlanService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}