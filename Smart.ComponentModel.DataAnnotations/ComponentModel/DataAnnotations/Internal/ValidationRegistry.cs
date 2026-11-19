namespace Smart.ComponentModel.DataAnnotations.Internal;

using System.Collections.Concurrent;
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ValidationRegistry
{
    private static readonly ConcurrentDictionary<MemberKey, Func<object, bool?>> Conditions = new();

    private static readonly ConcurrentDictionary<MemberKey, Func<object, bool, bool>> Presences = new();

    private static readonly ConcurrentDictionary<PairKey, Func<object, int?>> Comparisons = new();

    private static readonly ConcurrentDictionary<PairKey, Func<object, bool>> Duplicates = new();

    private static readonly ConcurrentDictionary<MemberKey, Func<string>> DisplayNames = new();

    // ------------------------------------------------------------
    // Registration
    // ------------------------------------------------------------

    public static void RegisterCondition(Type type, string member, Func<object, bool?> reader) =>
        Conditions[new MemberKey(type, member)] = reader;

    public static void RegisterPresence(Type type, string member, Func<object, bool, bool> checker) =>
        Presences[new MemberKey(type, member)] = checker;

    public static void RegisterComparison(Type type, string member, string other, Func<object, int?> comparer) =>
        Comparisons[new PairKey(type, member, other)] = comparer;

    public static void RegisterDuplicate(Type type, string member, string keys, Func<object, bool> checker) =>
        Duplicates[new PairKey(type, member, keys)] = checker;

    public static void RegisterDisplayName(Type type, string member, Func<string> provider) =>
        DisplayNames[new MemberKey(type, member)] = provider;

    // ------------------------------------------------------------
    // Lookup
    // ------------------------------------------------------------

    internal static Func<object, bool?> GetCondition(Type type, string member) =>
        Find(Conditions, type, member) ?? throw CreateNotGeneratedException(type, member);

    internal static Func<object, bool, bool> GetPresence(Type type, string member) =>
        Find(Presences, type, member) ?? throw CreateNotGeneratedException(type, member);

    internal static Func<object, int?> GetComparison(Type type, string member, string other) =>
        Find(Comparisons, type, member, other) ?? throw CreateNotGeneratedException(type, member);

    internal static Func<object, bool> GetDuplicate(Type type, string member, string keys) =>
        Find(Duplicates, type, member, keys) ?? throw CreateNotGeneratedException(type, member);

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

    private static T? Find<T>(ConcurrentDictionary<PairKey, T> map, Type type, string member, string other)
        where T : class
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (map.TryGetValue(new PairKey(current, member, other), out var value))
            {
                return value;
            }
        }

        return null;
    }

    private static InvalidOperationException CreateNotGeneratedException(Type type, string member) =>
        new($"Validation code is not generated. Reference Usa.Smart.ComponentModel.DataAnnotations from the project that declares the type. type=[{type.FullName}], member=[{member}]");

    private readonly record struct MemberKey(Type Type, string Member);

    private readonly record struct PairKey(Type Type, string Member, string Other);
}
