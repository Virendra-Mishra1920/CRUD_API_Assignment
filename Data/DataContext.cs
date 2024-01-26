namespace CRUD_API_Assignment.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Hobby> Hobbies { get; set; }
        
    }
}