using CRUD_API_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

using CRUD_API_Assignment.Services.UserService;

namespace CRUD_API_Assignment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController:ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
            
        }

        [HttpGet("GetAllUsers")]
        public async Task< ActionResult<ServiceResponse<List<GetUserResponseDto>>>> GetUsers()
        {
            return Ok( await _userService.GetAllUsers());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse< GetUserResponseDto>>> GetUser(string id)
        {
            var response=await _userService.GetUserById(id);
            if(response.Data is null)
            return NotFound(response);
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<GetUserResponseDto>>>> AddUser(AddUserResquestDto user)
        {
            return Ok(await _userService.AddUser(user));

        }

       [HttpPut]

        public async Task<ActionResult<ServiceResponse<GetUserResponseDto>>> UpdateUser(UpdateUserRequestDto updatedUser)
        {
            var response=await _userService.UpdateUser(updatedUser);
            if(response.Data is null)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult<ServiceResponse<List<GetUserResponseDto>>>> DeleteUser(string id)
        {
            var response=await _userService.DeleteUser(id);
            if(response.Success==false)
            return NotFound(response);
        
            return Ok(response);
        }

    }
}