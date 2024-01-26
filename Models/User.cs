using System.ComponentModel.DataAnnotations;

namespace CRUD_API_Assignment.Models
{
    public class User
    {
        [Key]
        public string? Id { get; set; }

        [Required]
        public string? UserName { get; set; }

        [Required]
        public string? Password { get; set; }

        public bool isAdmin { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public List<Hobby>? Hobbies { get; set; }
    }
}