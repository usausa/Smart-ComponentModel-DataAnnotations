namespace Smart.ComponentModel.DataAnnotations.AotTests;

using System.ComponentModel.DataAnnotations;

[AnyRequired(nameof(Phone), nameof(Mail))]
internal sealed class Form
{
    public bool UsePeriod { get; set; }

    public string? Phone { get; set; }

    [Display(Name = "Mail address")]
    public string? Mail { get; set; }

    [Display(Name = "Start")]
    [RequiredWhen(nameof(UsePeriod))]
    public DateOnly? From { get; set; }

    [RequiredWhen(nameof(UsePeriod))]
    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(From))]
    public DateOnly? To { get; set; }

    [EmptyWhen(nameof(UsePeriod), ConditionValue = false)]
    public int? Days { get; set; }

    [DuplicateKey(nameof(Entry.Code))]
    [DuplicateKey(nameof(Entry.Code), nameof(Entry.Branch))]
    public List<Entry>? Entries { get; set; }
}

internal sealed class Entry
{
    public string? Code { get; set; }

    public int Branch { get; set; }
}
