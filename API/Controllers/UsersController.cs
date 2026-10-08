using Microsoft.AspNetCore.Mvc;
using BLL.IRepository;
using DTO.Users;
using System.Collections.Generic;
using AutoMapper;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // 1. Get: מחזיר את כל המשתמשים
        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> GetAllUsers()
        {
            var users = _userRepository.GetAll();
            var dtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(dtos);
        }

        // 2. Get לפי ID: מחזיר משתמש בודד
        [HttpGet("{id}")]
        public ActionResult<UserDto> GetUserById(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound(new { message = "המשתמש לא נמצא" });
            }
            var dto = _mapper.Map<UserDto>(user);
            return Ok(dto);
        }

        // 3. Post: הרשמת משתמש חדש
        [HttpPost]
        public ActionResult RegisterUser([FromBody] RegisterUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<DAL.Entities.User>(model);
            _userRepository.Add(user);

            return Ok(new { message = "המשתמש נוצר בהצלחה!", data = model });
        }
    }
}