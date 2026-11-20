namespace Smart.ComponentModel.DataAnnotations.Generator.Models;

using SourceGenerateHelper;

internal sealed record RegistrationModel(
    EquatableArray<MemberModel> Members,
    EquatableArray<ComparerModel> Comparers,
    EquatableArray<DisplayNameModel> DisplayNames)
{
    public bool IsEmpty =>
        (Members.Count == 0) && (Comparers.Count == 0) && (DisplayNames.Count == 0);
}
