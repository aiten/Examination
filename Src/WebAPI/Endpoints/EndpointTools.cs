namespace WebAPI.Endpoints;

using Shared.Exceptions;

public static class EndpointTools
{
    public static void CheckId(int id, int dtoId)
    {
        if (id != dtoId)
        {
            throw new IllegalValuesException("The ID in the URL does not match the ID in the request body");
        }
    }

    public static void CheckIdMustBe0(int id)
    {
        if (id != 0)
        {
            throw new IllegalValuesException("The ID in the request body must be 0");
        }
    }

    public static void CheckExamId(int examId, int dtoExamId)
    {
        if (examId != dtoExamId)
        {
            throw new IllegalValuesException("The ExamId in the body does not match the URL");
        }
    }
}