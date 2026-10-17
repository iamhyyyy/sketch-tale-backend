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

        [HttpGet("parent-profile/{id}")]
        public async Task<ActionResult<List<SubscriptionDto>>> GetByParentProfileId(Guid id)
        {
            var items = await _subscriptionService.GetByParentProfileId(id);
            return Ok(items);
        }

        [HttpGet("subscription-plan/{id}")]
        public async Task<ActionResult<List<SubscriptionDto>>> GetBySubscriptionPlanId(Guid id)
        {
            var items = await _subscriptionService.GetBySubscriptionPlanId(id);
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