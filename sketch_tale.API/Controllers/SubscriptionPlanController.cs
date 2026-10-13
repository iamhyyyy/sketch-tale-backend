using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;

        public SubscriptionPlanController(ISubscriptionPlanService SubscriptionPlanService)
        {
            _subscriptionPlanService = SubscriptionPlanService;
        } 

        [HttpGet("subscription-plans")]
        public async Task<ActionResult<List<SubscriptionPlanDto>>> GetAll()
        {
            var items = await _subscriptionPlanService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("subscription-plan/{id}")]
        public async Task<ActionResult<SubscriptionPlanDto>> GetById(Guid id)
        {
            var item = await _subscriptionPlanService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("subscription-plan")]
        public async Task<ActionResult<CreateSubscriptionPlanDto>> Create(CreateSubscriptionPlanDto dto)
        {
            var item = await _subscriptionPlanService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("subscription-plan/{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateSubscriptionPlanDto dto)
        {
            var item = await _subscriptionPlanService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("subscription-plan/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _subscriptionPlanService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}