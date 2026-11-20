namespace Smart.ComponentModel.DataAnnotations.Internal;

using System.Collections.Concurrent;
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ValidationRegistry
{
    private static readonly ConcurrentDictionary<MemberKey, Func<object, object?>> Members = new();

    private static readonly ConcurrentDictionary<Type, Func<object, object, int>> Comparers = new();

    private static readonly ConcurrentDictionary<MemberKey, Func<string>> DisplayNames = new();

    // ------------------------------------------------------------
    // Registration
    // ------------------------------------------------------------

    public static void RegisterMember(Type type, string member, Func<object, object?> getter) =>
        Members[new MemberKey(type, member)] = getter;

    public static void RegisterComparer(Type type, Func<object, object, int> comparer) =>
        Comparers[type] = comparer;

    public static void RegisterDisplayName(Type type, string member, Func<string> provider) =>
        DisplayNames[new MemberKey(type, member)] = provider;

    // ------------------------------------------------------------
    // Lookup
    // ------------------------------------------------------------

    internal static Func<object, object?> GetMember(Type type, string member) =>
        Find(Members, type, member) ?? throw new InvalidOperationException($"Validation code is not generated. Reference Usa.Smart.ComponentModel.DataAnnotations from the project that declares the type. type=[{type.FullName}], member=[{member}]");

    internal static Func<object, object, int> GetComparer(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (Comparers.TryGetValue(current, out var comparer))
            {
                return comparer;
            }
        }

        throw new InvalidOperationException($"Comparer is not generated. type=[{type.FullName}]");
    }

    internal static Func<string>? FindDisplayName(Type type, string member) =>
        Find(DisplayNames, type, member);

    private static T? Find<T>(ConcurrentDictionary<MemberKey, T> map, Type type, string member)
        where T : class
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (map.TryGetValue(new MemberKey(current, member), out var value))
            {
                return value;
            }
        }

        return null;
    }

    private readonly record struct MemberKey(Type Type, string Member);
}
