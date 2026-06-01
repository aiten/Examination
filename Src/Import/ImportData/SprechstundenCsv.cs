using Base.Tools.CsvImport;

using System;

namespace Import.ImportData;

internal class SprechstundeCsv
{
    public required string Lehrkraft { get; set; }
    public required string Wochentag { get; set; }

    [CsvImportFormat(Format = "HH:mm")]
    public TimeOnly? Von { get; set; }

    [CsvImportFormat(Format = "HH:mm")]
    public TimeOnly? Bis { get; set; }

    public required string Raum { get; set; }
}