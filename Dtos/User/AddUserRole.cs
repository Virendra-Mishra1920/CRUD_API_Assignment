namespace CRUD_API_Assignment.Dtos.User
{
    public class AddUserRole
    {
        public string? UserId { get; set; }
        public List<int> RoleIds { get; set; } 
    }
}