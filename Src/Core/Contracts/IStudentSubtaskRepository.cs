using Core.Entities;

namespace Core.Contracts;

using System.Collections.Generic;
using System.Threading.Tasks;

using Base.Core.Contracts;

public interface IStudentSubtaskRepository : IGenericRepository<StudentSubtask>
{
    Task<IList<StudentSubtask>> GetAllPossibleAsync(int examId, int studentExamId);
}