namespace Shared;

using System.Threading.Tasks;

public class ExamHelper
{
    public async Task<int> CalculateGrade(int id, decimal percent)
    {
        return CalculateGrade(percent);
    }

    public static int CalculateGrade(decimal percent)
    {
        return percent switch
        {
            >= 0.88m => 1,
            >= 0.75m => 2,
            >= 0.63m => 3,
            >= 0.5m  => 4,
            _        => 5
        };
    }

}