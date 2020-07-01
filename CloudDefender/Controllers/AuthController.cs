using CloudDefender.Models.Requests;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CloudDefender.Controllers
{
    [Route("auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly DbSet<User> _userRepo;
        public AuthController(DataDbContext db) : base(db) 
        {
            _userRepo = _db.Set<User>();
        }

        /// <summary>
        /// Метод для регистрации пользователя.
        /// </summary>
        /// <param name="registerRequest">Объект с параметрами для регистрации пользователя</param>
        /// <response code="201">Возвращает идентификатор пользователя</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(string), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest registerRequest)
        {
            if (_userRepo.Any(u => u.Username == registerRequest.Username || u.Email == registerRequest.Email)) return BadRequest("Данный пользователь уже зарегистрирован");

            var newUser = new User
            {
                Email= registerRequest.Email,
                Username = registerRequest.Username
            };

            _userRepo.Add(newUser);
            _db.SaveChanges();

            return CreatedAtAction(nameof(WhoAmI), routeValues: new { userId = newUser.Id }, newUser.Id);
        }

        /// <summary>
        /// Проверяет наличие пользователя в базе, возвращает ник найденного пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <response code="200">Возвращает ник пользователя</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpGet("whoami/{userId}")]
        public IActionResult WhoAmI(Guid userId)
        {
            var user = _userRepo.FirstOrDefault(u => u.Id == userId);
            if (user == null) return BadRequest("Пользователь не найден");

            return Ok(user.Username);
        }
    }
}
