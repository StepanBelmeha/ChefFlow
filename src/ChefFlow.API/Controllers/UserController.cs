namespace ChefFlow.API.Controllers
{
    using ChefFlow.API.Data;
    using ChefFlow.API.DTO;
    using ChefFlow.API.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class Usercontroller(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context
                .Users
                .ToList();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public IActionResult Createuser(AddUserDto userDto)
        {
            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Password = userDto.Password
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user);
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult UpdateUser(int id, AddUserDto userDto)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Name = userDto.Name;
            user.Email = userDto.Email;
            _context.SaveChanges();
            return Ok(user);
        }
    
        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok();
        }
    }
}