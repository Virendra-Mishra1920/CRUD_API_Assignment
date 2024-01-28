namespace CRUD_API_Assignment.Dtos.User
{
    public class GetUserResponseDto
    {
        public string? Id { get; set; }

        public string? UserName { get; set; }
        
        public bool isAdmin { get; set; }

        public int Age { get; set; }

        public List<string>? Hobbies{get;set;}=new List<string>();
        
    }
}