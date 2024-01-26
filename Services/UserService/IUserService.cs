using CRUD_API_Assignment.Models;
using System.Threading.Tasks;
namespace CRUD_API_Assignment.Services.UserService
{
    public interface IUserService
    {
        Task<ServiceResponse<List<GetUserResponseDto>>> GetAllUsers();
        Task<ServiceResponse<GetUserResponseDto>> GetUserById(string id);
        Task<ServiceResponse<List<GetUserResponseDto>>> AddUser(AddUserResquestDto user);
        Task<ServiceResponse<GetUserResponseDto>> UpdateUser(UpdateUserRequestDto updatedUser);
        Task<ServiceResponse<List<GetUserResponseDto>>> DeleteUser(string id);
         
    }
}