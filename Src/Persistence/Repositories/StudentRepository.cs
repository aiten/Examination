namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;

using Persistence.Model;

public interface IStudentRepository : IGenericRepository<Student>
{
    Task<Student?> GetStudentByNameAsync(string lastName, string firstName);
}

public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StudentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Student?> GetStudentByNameAsync(string lastName, string firstName)
    {
        return await DbSet
            .Include(s => s.Classes)
            .FirstOrDefaultAsync(s => s.FirstName == firstName && s.LastName == lastName);
    }
}