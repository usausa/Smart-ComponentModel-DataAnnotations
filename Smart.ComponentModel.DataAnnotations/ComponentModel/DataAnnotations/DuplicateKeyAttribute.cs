namespace Smart.ComponentModel.DataAnnotations;

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Smart.ComponentModel.DataAnnotations.Internal;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class DuplicateKeyAttribute : ValidationAttribute
{
    private readonly string[] members;

    public IReadOnlyList<string> Members => members;

    public override bool RequiresValidationContext => true;

    public override object TypeId => this;

    public DuplicateKeyAttribute(params string[] members)
        : base(static () => ValidationMessages.DuplicateKey)
    {
        this.members = members;
    }

#if NET11_0_OR_GREATER
    public override string FormatMessage(string format, string name) =>
        Format(format, name);
#else
    public override string FormatErrorMessage(string name) =>
        Format(ErrorMessageString, name);
#endif

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (members.Length == 0)
        {
            throw new InvalidOperationException("Members are not specified.");
        }

        if (value is not IEnumerable values)
        {
            return ValidationResult.Success;
        }

        var duplicated = members.Length == 1 ? HasDuplicateKey(values, members[0]) : HasDuplicateKeys(values, members);
        return duplicated
            ? new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.ToMemberNames())
            : ValidationResult.Success;
    }

    private static bool HasDuplicateKey(IEnumerable values, string member)
    {
        var keys = new HashSet<object>();
        Type? type = null;
        Func<object, object?>? getter = null;
        foreach (var item in values)
        {
            if (item is null)
            {
                continue;
            }

            if (item.GetType() != type)
            {
                type = item.GetType();
                getter = ValidationRegistry.GetMember(type, member);
            }

            var key = getter!(item);
            if ((key is IEnumerable items) && (key is not string))
            {
                foreach (var element in items)
                {
                    if ((element is not null) && !keys.Add(element))
                    {
                        return true;
                    }
                }
            }
            else if ((key is not null) && !keys.Add(key))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasDuplicateKeys(IEnumerable values, string[] members)
    {
        var keys = new HashSet<object[]>(KeyComparer.Instance);
        var getters = new Func<object, object?>[members.Length];
        Type? type = null;
        foreach (var item in values)
        {
            if (item is null)
            {
                continue;
            }

            if (item.GetType() != type)
            {
                type = item.GetType();
                for (var i = 0; i < members.Length; i++)
                {
                    getters[i] = ValidationRegistry.GetMember(type, members[i]);
                }
            }

            if ((ReadKeys(getters, item) is { } key) && !keys.Add(key))
            {
                return true;
            }
        }

        return false;
    }

    private static object[]? ReadKeys(Func<object, object?>[] getters, object item)
    {
        var key = new object[getters.Length];
        for (var i = 0; i < getters.Length; i++)
        {
            if (getters[i](item) is not { } value)
            {
                return null;
            }

            key[i] = value;
        }

        return key;
    }

    private string Format(string format, string name) =>
        String.Format(CultureInfo.CurrentCulture, format, name, String.Join(", ", members));

    private sealed class KeyComparer : IEqualityComparer<object[]>
    {
        public static KeyComparer Instance { get; } = new();

        public bool Equals(object[]? x, object[]? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if ((x is null) || (y is null) || (x.Length != y.Length))
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (!x[i].Equals(y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(object[] obj)
        {
            var hash = default(HashCode);
            foreach (var value in obj)
            {
                hash.Add(value);
            }

            return hash.ToHashCode();
        }
    }
}
