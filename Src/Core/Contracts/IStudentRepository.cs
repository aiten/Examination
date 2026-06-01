using System.Threading.Tasks;

using Core.Entities;

namespace Core.Contracts;

using Base.Core.Contracts;

public interface IStudentRepository : IGenericRepository<Student>
{
    Task ImportStudentsAsync(string[] studentLines);
}