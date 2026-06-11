using Core.Entities;

namespace Core.Contracts;

using System.Collections.Generic;
using System.Threading.Tasks;

using Base.Core.Contracts;

public interface IStudentSubtaskRepository : IGenericRepository<StudentSubtask>
{
    Task<IList<StudentSubtask>> GetAllForStudentAsync(int examId, int studentExamId);
    Task<IList<StudentSubtask>> GetAllForSubtaskAsync(int examId, int subtaskId);
}