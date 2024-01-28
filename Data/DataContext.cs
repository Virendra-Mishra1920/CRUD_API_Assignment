namespace CRUD_API_Assignment.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles {get;set;}
        public DbSet<UserRole> UserRoles{get;set;}
        public DbSet<UserHobby> UserHobbies { get; set; }

    }
}