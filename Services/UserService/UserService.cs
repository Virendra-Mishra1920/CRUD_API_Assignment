using CRUD_API_Assignment.Models;
using System.Threading.Tasks;
using System.Linq;

namespace CRUD_API_Assignment.Services.UserService
{
    public class UserService: IUserService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;

        public UserService(IMapper mapper, DataContext dataContext)
        {
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

        public async Task<ServiceResponse< List<GetUserResponseDto>>> AddUser(AddUserResquestDto newuser)
        {
             var serviceResponse=new ServiceResponse<List<GetUserResponseDto>>();
             var user=_mapper.Map<User>(newuser);
             user.Id=Guid.NewGuid().ToString();
             
             _dataContext.Users.Add(user);
             await _dataContext.SaveChangesAsync();
             foreach(var hobby in newuser.Hobbies)
             {
                var hb=_mapper.Map<Hobby>(hobby);
                await _dataContext.Hobbies.AddAsync(hb);
                await _dataContext.SaveChangesAsync();
             }

             serviceResponse.Data=_dataContext.Users.Select(u=>_mapper.Map<GetUserResponseDto>(u)).ToList();
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
                user.Password=updatedUser.Password;
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

        public async Task<ServiceResponse<List<GetUserResponseDto>>> DeleteUser(string id)
        {
             var serviceResponse=new ServiceResponse<List<GetUserResponseDto>>();
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

               serviceResponse.Data=await _dataContext.Users.Select(u=>_mapper.Map<GetUserResponseDto>(u)).ToListAsync();
               serviceResponse.Message=$"User delete successfully with Id {id}";

            }
            catch (System.Exception ex )
            {
                serviceResponse.Success=false;
                serviceResponse.Message=ex.Message;
            }
           
           
            return serviceResponse;

        }
        
    }
}