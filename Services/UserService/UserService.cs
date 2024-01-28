using CRUD_API_Assignment.Models;
using System.Threading.Tasks;
using System.Linq;
using  Microsoft.Extensions.Configuration;

namespace CRUD_API_Assignment.Services.UserService
{
    public class UserService: IUserService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;

        public UserService(IMapper mapper, DataContext dataContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _mapper = mapper;
            _dataContext = dataContext;
        }

        public async Task<ServiceResponse<List<GetUserResponseDto>>> GetAllUsers()
        {
            var serviceResponse=new ServiceResponse<List<GetUserResponseDto>>();
            serviceResponse.Data=_dataContext.Users.Select(u=>_mapper.Map<GetUserResponseDto>(u)).ToList();

            List<GetUserResponseDto> getUserResponseDtos = new List<GetUserResponseDto>();
            var ids = new List<string>();
            var names = new List<string>();

            foreach (var item in serviceResponse.Data)
            {
                var userResponse = new GetUserResponseDto();
                userResponse.Id = item.Id;
                userResponse.UserName = item.UserName;
                userResponse.Age = item.Age;
                userResponse.isAdmin = item.isAdmin;
                var hobbies = _dataContext.UserHobbies.Where(x => x.UserId == item.Id).Select(x => x.HobbyName).ToList();
                if(hobbies.Count>0)
                userResponse!.Hobbies!.AddRange(hobbies!);
                getUserResponseDtos.Add(userResponse) ;
            }

            serviceResponse.Data = getUserResponseDtos;

            return serviceResponse;

        }

        public async Task<ServiceResponse< GetUserResponseDto>> GetUserById(string id)
        {
              var serviceResponse=new ServiceResponse<GetUserResponseDto>();
            try
            {
                var user=_dataContext.Users.FirstOrDefault(user=>user.Id==id);
                if(user is null)
                {
                    serviceResponse.Success=false;
                    serviceResponse.Message=$"User with Id '{id}' not found";
                    return serviceResponse;

                }
                
                serviceResponse.Data=_mapper.Map<GetUserResponseDto>(user);
                var userResponse = new GetUserResponseDto();
                userResponse.Id = user.Id;
                userResponse.Age = user.Age;
                userResponse.UserName = user.UserName;
                userResponse.isAdmin=user.isAdmin;
                var hobbies = _dataContext.UserHobbies.Where(u=>u.UserId==user.Id).Select(x=>x.HobbyName).ToList();
                if(hobbies.Count>0)
                userResponse.Hobbies!.AddRange(hobbies!);
                serviceResponse.Data = userResponse;
                
            }
            catch (System.Exception ex)
            {
                serviceResponse.Success=false;
                serviceResponse.Message=ex.Message;
            }
          
            return serviceResponse;

        }

        public async Task<ServiceResponse<string>> AddUser(AddUserResquestDto newuser, string password)
        {
            var serviceResponse=new ServiceResponse<string>();
            if(await IsUserAlreadyExist(newuser.UserName!))
            {
                serviceResponse.Success=false;
                serviceResponse.Message=$"User already exist by name {newuser.UserName}";
                return serviceResponse;

            }
            CreateHashPassword(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user=_mapper.Map<User>(newuser);
             user.Id=Guid.NewGuid().ToString();

            user.PasswordHash=passwordHash;
            user.PasswordSalt=passwordSalt;

            var hobbies = new List<UserHobby>();
            
            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();

            foreach (string hobby in newuser.Hobbies!)
            {
                var uby = new UserHobby();
                uby.HobbyName=hobby;
                uby.UserId = user.Id;
                _dataContext.UserHobbies.Add(uby);

            }

            _dataContext.SaveChanges();


             serviceResponse.Data=user.Id;

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetUserResponseDto>> UpdateUser(UpdateUserRequestDto updatedUser)
        {
             var serviceResponse=new ServiceResponse<GetUserResponseDto>();
            try
            {
                var user=await _dataContext.Users.FirstOrDefaultAsync(u=>u.Id==updatedUser.Id);
                if(user is null)
                {
                    throw new Exception($"User with Id '{updatedUser.Id}' not found");
                }
                user.UserName=updatedUser.UserName;
                user.Age=updatedUser.Age;
                user.isAdmin=updatedUser.isAdmin;

                if (updatedUser.Hobbies!.Count > 0)
                {
                    foreach (string hobby in updatedUser.Hobbies!)
                    {
                        var _user = await _dataContext.UserHobbies.FirstOrDefaultAsync(x => x.UserId == updatedUser.Id);
                        if (_user != null)
                        {
                            _user.HobbyName = hobby;
                        }

                    }

                    await _dataContext.SaveChangesAsync();


                }



                


                await _dataContext.SaveChangesAsync();

                var hobbies = _dataContext.UserHobbies.Where(x => x.UserId == user.Id).Select(x => x.HobbyName).ToList();

                var getUserResponseDto = new GetUserResponseDto();
                getUserResponseDto.UserName = user.UserName;
                getUserResponseDto.Id = user.Id;
                getUserResponseDto.isAdmin = user.isAdmin;
                if (hobbies.Count > 0)
                    getUserResponseDto.Hobbies!.AddRange(hobbies!);

                serviceResponse.Data = getUserResponseDto;
                
            }
            catch (System.Exception ex )
            {
                serviceResponse.Success=false;
                serviceResponse.Message=ex.Message;
            }
           
           
            return serviceResponse;

        }

        public async Task<ServiceResponse<GetUserResponseDto>> DeleteUser(string id)
        {
             var serviceResponse=new ServiceResponse<GetUserResponseDto>();
            try
            {
                var user=await _dataContext.Users.FirstOrDefaultAsync(u=>u.Id==id);
                if(user is null)
                {
                    throw new Exception($"User with Id '{id}' not found");
                }


               _dataContext.Users.Remove(user);
               await _dataContext.SaveChangesAsync();

                serviceResponse.Data=_mapper.Map<GetUserResponseDto>(user);
               serviceResponse.Message=$"User delete successfully with Id {id}";

            }
            catch (System.Exception ex )
            {
                serviceResponse.Success=false;
                serviceResponse.Message=ex.Message;
            }
           
           
            return serviceResponse;

        }

        public async Task<ServiceResponse<string>> Login(string userName, string password)
        {
            var serviceResponse=new ServiceResponse<string>();
            var user=await _dataContext.Users.FirstOrDefaultAsync(u=>u.UserName.ToLower()==userName);
            if(user is null)
            {
                serviceResponse.Success=false;
                serviceResponse.Message=$"user '{userName}' not found";
                return serviceResponse;
            }

            else if(!VerifyPassword(password,user.PasswordHash,user.PasswordSalt))
            {
                serviceResponse.Success=false;
                serviceResponse.Message=$"Invalid Password";
                return serviceResponse;
            }

            else
            {
                serviceResponse.Data=CreateToken(user);
            }
            return serviceResponse;
        }

        public async Task< bool> IsUserAlreadyExist(string username)
        {
            if(await _dataContext.Users.AnyAsync(u=>u.UserName.ToLower()==username))
            {
                return true;
            }

            return false;

        }

        public void CreateHashPassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac=new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt=hmac.Key;
                passwordHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));


            }
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac=new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims=new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name,user.UserName),
                
            };

            var userRoles=_dataContext.UserRoles.Where(u=>u.UserId==user.Id);
            var roleIds=userRoles.Select(x=>x.RoleId).ToList();
            var roles=_dataContext.Roles.Where(r=>roleIds.Contains(r.Id)).ToList();
            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role,role.Name));
            }


            var appsettingsToken=_configuration.GetSection("AppSettings:Token").Value;
            if(appsettingsToken is null)
            {
                throw new Exception("appsettings token is empty");
            }

            SymmetricSecurityKey key=new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appsettingsToken));
            SigningCredentials cred=new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor=new SecurityTokenDescriptor()
            {
                Subject=new ClaimsIdentity(claims),
                Expires=DateTime.Now.AddDays(1),
                SigningCredentials=cred
            };


            JwtSecurityTokenHandler tokenhandler=new JwtSecurityTokenHandler();
            SecurityToken token=tokenhandler.CreateToken(tokenDescriptor);

            return tokenhandler.WriteToken(token);

        }

        public async Task<ServiceResponse<Role>> AddRole(Role role)
        {
            var serviceResponse=new ServiceResponse<Role>();
            var data=await _dataContext.Roles.AddAsync(role);
            await _dataContext.SaveChangesAsync();
            //serviceResponse.Data=data.Entity;
            return serviceResponse;

        }

        public async Task<ServiceResponse<bool>> AssignRoleToUser(AddUserRole obj)
        {
            var serviceResponse=new ServiceResponse<bool>();
            try
            {
                    var userRoles=new List<UserRole>();
                    var user=_dataContext.Users.FirstOrDefault(u=>u.Id==obj.UserId);
                    if(user is null)
                    {
                        serviceResponse.Success=false;
                        serviceResponse.Message=$"User not found with Id {obj.UserId}";
                        return serviceResponse;

                    }

                    foreach(int roleId in obj.RoleIds)
                    {
                        var _userRole=new UserRole();
                        _userRole.RoleId=roleId;
                        _userRole.UserId=user.Id;
                        userRoles.Add(_userRole);
                    }

                    await _dataContext.UserRoles.AddRangeAsync(userRoles);
                    await _dataContext.SaveChangesAsync();
                    serviceResponse.Data=true;
                
            }
            catch (System.Exception ex)
            {
                serviceResponse.Data=false;
                serviceResponse.Success=false;
                serviceResponse.Message=ex.Message;

            }
           
            return serviceResponse;

        }
        
    }
}