namespace Develop;

using System.ComponentModel.DataAnnotations;

using Smart.ComponentModel.DataAnnotations;

internal static class Program
{
    public static void Main()
    {
        var form = new SampleForm
        {
            Name = "abc",
            UsePeriod = true,
            From = new DateOnly(2026, 1, 2),
            To = new DateOnly(2026, 1, 1),
            Entries = [new SampleEntry { Code = "A" }, new SampleEntry { Code = "A" }]
        };

        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, true);
        foreach (var result in results)
        {
            Console.WriteLine($"{String.Join(",", result.MemberNames)}: {result.ErrorMessage}");
        }
    }
}

[AnyRequired(nameof(Phone), nameof(Mail))]
internal sealed class SampleForm
{
    [FullWidth]
    [Ms932MaxLength(20)]
    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Mail { get; set; }

    public bool UsePeriod { get; set; }

    [Display(Name = "Start")]
    [RequiredWhen(nameof(UsePeriod))]
    public DateOnly? From { get; set; }

    [RequiredWhen(nameof(UsePeriod))]
    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(From))]
    public DateOnly? To { get; set; }

    [DuplicateKey(nameof(SampleEntry.Code))]
    public List<SampleEntry> Entries { get; set; } = [];
}

internal sealed class SampleEntry
{
    public string? Code { get; set; }
}
