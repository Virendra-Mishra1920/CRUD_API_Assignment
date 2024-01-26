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
            if(await IsUserAlreadyExist(newuser.UserName))
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

             _dataContext.Users.Add(user);
             await _dataContext.SaveChangesAsync();
             foreach(var hobby in newuser.Hobbies)
             {
                var hb=_mapper.Map<Hobby>(hobby);
                await _dataContext.Hobbies.AddAsync(hb);
                await _dataContext.SaveChangesAsync();
             }

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
               // user.Password=updatedUser.Password;
                user.Age=updatedUser.Age;
                user.isAdmin=updatedUser.isAdmin;
                 await _dataContext.SaveChangesAsync();
                 //var hb=_mapper.Map<Hobby>(updatedUser.Hobbies);
                 List<string> hobbies=new List<string>();

                var hbs=await _dataContext.Hobbies.Where(u=>u.UserId==user.Id).ToListAsync();
                if(updatedUser.Hobbies.Count>0)
                {
                    foreach (var updatedhobby in updatedUser.Hobbies)
                    {
                        if(hbs.Count>0)
                        {
                            foreach(var item in hbs)
                           {
                               if( !hobbies.Contains(item.Name))
                               {
                                 hobbies.Add(item.Name);
                                 item.Name=updatedhobby.Name;
                               }
                           }
                           
                           await _dataContext.SaveChangesAsync();

                        }
                        
                    }
                     
                }
               

               serviceResponse.Data=_mapper.Map<GetUserResponseDto>(user);
                
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

                var hbs=await _dataContext.Hobbies.Where(u=>u.UserId==id).ToListAsync();
                if(hbs.Count>0)
                {
                    foreach(var item in hbs)
                   {
                    _dataContext.Hobbies.Remove(item);
                   }
                   await _dataContext.SaveChangesAsync();

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
                new Claim(ClaimTypes.Name,user.UserName)
            };

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
        
    }
}