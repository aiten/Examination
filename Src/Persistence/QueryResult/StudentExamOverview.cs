namespace Core.QueryResult;

public record StudentExamOverview(
    int      Id,
    int      StudentId,
    string   FirstName,
    string   LastName,
    string   LoginName,
    string   RegistrationCode,
    int      CountRated,
    decimal? Points,
    decimal? Percent,
    int?     Grade
);