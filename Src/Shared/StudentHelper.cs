namespace Shared;

using System.Threading.Tasks;

public class StudentHelper
{
    public static string FullName(string firstName, string lastName) => $"{lastName}, {firstName}";
}