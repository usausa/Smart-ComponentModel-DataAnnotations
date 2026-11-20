namespace Example.Web.Models;

using System.ComponentModel.DataAnnotations;

using Smart.ComponentModel.DataAnnotations;

[AnyRequired(nameof(Phone), nameof(Mail))]
public sealed class SampleForm
{
    [Display(Name = "氏名")]
    [Required]
    [Ms932MaxLength(20)]
    [FullWidth]
    public string? Name { get; set; }

    [Display(Name = "氏名カナ")]
    [Katakana(AllowSpace = true)]
    public string? Kana { get; set; }

    [Display(Name = "社員番号")]
    [Ms932FixedLength(6)]
    [UpperAlphanumeric]
    public string? Code { get; set; }

    [Display(Name = "電話番号")]
    [NumericHyphen]
    public string? Phone { get; set; }

    [Display(Name = "メール")]
    [EmailAddress]
    public string? Mail { get; set; }

    [Display(Name = "メール(確認)")]
    [EqualTo(nameof(Mail))]
    public string? ConfirmMail { get; set; }

    [Display(Name = "期間を指定する")]
    public bool UsePeriod { get; set; }

    [Display(Name = "開始日")]
    [RequiredWhen(nameof(UsePeriod))]
    [EmptyWhen(nameof(UsePeriod), ConditionValue = false)]
    public DateOnly? From { get; set; }

    [Display(Name = "終了日")]
    [RequiredWhen(nameof(UsePeriod))]
    [CompareToWhen(nameof(UsePeriod), CompareToOperation.GreaterThanOrEqual, nameof(From))]
    public DateOnly? To { get; set; }

    [Display(Name = "最小")]
    [Range(0, 100)]
    public int? Minimum { get; set; }

    [Display(Name = "最大")]
    [Range(0, 100)]
    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(Minimum))]
    public int? Maximum { get; set; }
}
