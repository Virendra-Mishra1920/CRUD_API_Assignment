namespace CRUD_API_Assignment
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User,GetUserResponseDto>();
            CreateMap<AddUserResquestDto,User>();
            CreateMap<AddHobbyRequestDto,Hobby>();
            
        }
        
    }
}