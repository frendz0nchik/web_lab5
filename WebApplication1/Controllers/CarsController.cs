using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly CarRepository _repository;
        private readonly RabbitMQService _rabbitMQService;

        public CarsController(CarContext context)
        {
            _repository = new CarRepository(context);
            _rabbitMQService = new RabbitMQService();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> Get()
        {
            return await _repository.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> Get(int id)
        {
            var car = await _repository.GetByIdAsync(id);
            if (car == null)
                return NotFound();
            return car;
        }

        [HttpPost]
        public async Task<ActionResult<Car>> Post(Car car)
        {
            if (car == null)
                return BadRequest();

            await _repository.AddAsync(car);

            // Генерация события
            var carEvent = new CarEvent { EventType = "CREATE", Car = car };
            _rabbitMQService.PublishEvent(carEvent);

            return Ok(car);
        }

        [HttpPut]
        public async Task<ActionResult<Car>> Put(Car car)
        {
            if (car == null)
                return BadRequest();

            if (await _repository.GetByIdAsync(car.car_id) == null)
                return NotFound();

            await _repository.UpdateAsync(car);

            // Генерация события
            var carEvent = new CarEvent { EventType = "UPDATE", Car = car };
            _rabbitMQService.PublishEvent(carEvent);

            return Ok(car);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Car>> Delete(int id)
        {
            var car = await _repository.GetByIdAsync(id);
            if (car == null)
                return NotFound();

            await _repository.DeleteAsync(id);

            // Генерация события
            var carEvent = new CarEvent { EventType = "DELETE", Car = car };
            _rabbitMQService.PublishEvent(carEvent);

            return Ok(car);
        }
    }
}
