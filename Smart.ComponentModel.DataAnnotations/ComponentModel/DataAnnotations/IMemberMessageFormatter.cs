namespace Smart.ComponentModel.DataAnnotations;

internal interface IMemberMessageFormatter
{
    bool AppliesToType { get; }

    IReadOnlyList<string> MessageMembers { get; }

    string FormatMessage(string? format, string name, IReadOnlyList<string> memberDisplayNames);
}
