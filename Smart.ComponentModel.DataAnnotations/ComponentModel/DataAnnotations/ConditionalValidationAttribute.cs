namespace Smart.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

using Smart.ComponentModel.DataAnnotations.Internal;

public abstract class ConditionalValidationAttribute : ValidationAttribute
{
    public string ConditionProperty { get; }

    public bool ConditionValue { get; set; } = true;

    public override bool RequiresValidationContext => true;

    protected ConditionalValidationAttribute(string conditionProperty, Func<string> errorMessageAccessor)
        : base(errorMessageAccessor)
    {
        ConditionProperty = conditionProperty;
    }

    protected bool IsConditionMet(object instance) =>
        (ValidationRegistry.GetCondition(instance.GetType(), ConditionProperty)(instance) is { } condition) && (condition == ConditionValue);
}
