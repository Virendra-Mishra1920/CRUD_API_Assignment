using CRUD_API_Assignment.Models;
using System.Threading.Tasks;
namespace CRUD_API_Assignment.Services.UserService
{
    public interface IUserService
    {
        Task<ServiceResponse<List<GetUserResponseDto>>> GetAllUsers();
        Task<ServiceResponse<GetUserResponseDto>> GetUserById(string id);
        Task<ServiceResponse<string>> AddUser(AddUserResquestDto user, string password);
        Task<ServiceResponse<GetUserResponseDto>> UpdateUser(UpdateUserRequestDto updatedUser);
        Task<ServiceResponse<GetUserResponseDto>> DeleteUser(string id);
        Task<bool> IsUserAlreadyExist(string username);
        Task<ServiceResponse<string>> Login(string userName, string password);
        Task<ServiceResponse<Role>> AddRole(Role role);
        Task<ServiceResponse<bool>> AssignRoleToUser(AddUserRole obj);
         
    }
}