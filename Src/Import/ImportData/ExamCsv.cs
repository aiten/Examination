using Base.Tools.CsvImport;

using System;

namespace Import.ImportData;

internal class ExamCsv
{
    public          int    Year  { get; set; }
    public required string Class { get; set; }

    [CsvImportFormat(Format = "dd.MM.yyyy")]
    public DateOnly Date { get; set; }

    [CsvImportFormat(Format = "h:mm")]
    public TimeOnly From { get; set; }

    [CsvImportFormat(Format = "h:mm")]
    public TimeOnly To { get; set; }

    public int PIN { get; set; }

    public required string  Teacher     { get; set; }
    public required string  Subject     { get; set; }
    public required string  Description { get; set; }
    public          string? SubTasks    { get; set; }
}