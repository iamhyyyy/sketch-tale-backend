using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/subscriptions")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService SubscriptionService)
        {
            _subscriptionService = SubscriptionService;
        } 

        [HttpGet]
        public async Task<ActionResult<List<SubscriptionDto>>> GetAll()
        {
            var items = await _subscriptionService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionDto>> GetById(Guid id)
        {
            var item = await _subscriptionService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("parent-profile/{parentProfileId}")]
        public async Task<ActionResult<List<SubscriptionDto>>> GetByParentProfileId(Guid parentProfileId)
        {
            var items = await _subscriptionService.GetByParentProfileId(parentProfileId);
            return Ok(items);
        }

        [HttpGet("subscription-plan/{subscriptionPlanId}")]
        public async Task<ActionResult<List<SubscriptionDto>>> GetBySubscriptionPlanId(Guid subscriptionPlanId)
        {
            var items = await _subscriptionService.GetBySubscriptionPlanId(subscriptionPlanId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<CreateSubscriptionDto>> Create(CreateSubscriptionDto dto)
        {
            var item = await _subscriptionService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch]
        public async Task<ActionResult> Update(Guid id, UpdateSubscriptionDto dto)
        {
            var item = await _subscriptionService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _subscriptionService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}