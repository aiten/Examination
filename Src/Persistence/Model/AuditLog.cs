namespace Persistence.Model;

using System;

using Base.Persistence.Model;

public class AuditLog : EntityObject
{
    public string?  Message         { get; set; }
    public string?  MessageTemplate { get; set; }
    public string?  Level           { get; set; }
    public DateTime TimeStamp       { get; set; }
    public string?  Exception       { get; set; }
    public string?  Properties      { get; set; }
}