namespace Smart.ComponentModel.DataAnnotations;

internal static class ValidationMessages
{
    public const string Required = "The {0} field is required.";

    public const string Empty = "The {0} field must be empty.";

    public const string AnyRequired = "At least one of the fields {1} is required.";

    public const string LessThan = "The field {0} must be less than {1}.";

    public const string LessThanOrEqual = "The field {0} must be less than or equal to {1}.";

    public const string GreaterThan = "The field {0} must be greater than {1}.";

    public const string GreaterThanOrEqual = "The field {0} must be greater than or equal to {1}.";

    public const string MaxCount = "The field {0} must contain at most {1} elements.";

    public const string ElementRequired = "The field {0} must contain at least one element and no empty elements.";

    public const string DuplicateKey = "The field {0} must not contain duplicate {1}.";
}
