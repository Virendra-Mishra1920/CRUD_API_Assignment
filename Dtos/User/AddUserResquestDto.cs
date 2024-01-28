namespace CRUD_API_Assignment.Dtos.User
{
    public class AddUserResquestDto
    {

        public string? UserName { get; set; }

        public string? Password { get; set; }
        
        public bool isAdmin { get; set; }

        public int Age { get; set; }

        public List<string>? Hobbies{get;set;}

        
    }
}