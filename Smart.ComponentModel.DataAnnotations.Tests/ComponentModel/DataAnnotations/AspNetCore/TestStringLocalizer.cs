namespace Smart.ComponentModel.DataAnnotations.AspNetCore;

using System.Globalization;

using Microsoft.Extensions.Localization;

internal sealed class TestStringLocalizer : IStringLocalizer
{
    private readonly Dictionary<string, string> values;

    public TestStringLocalizer(Dictionary<string, string> values)
    {
        this.values = values;
    }

    public LocalizedString this[string name] =>
        values.TryGetValue(name, out var value) ? new LocalizedString(name, value) : new LocalizedString(name, name, resourceNotFound: true);

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var value = this[name];
            return new LocalizedString(name, String.Format(CultureInfo.CurrentCulture, value.Value, arguments), value.ResourceNotFound);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        values.Select(static x => new LocalizedString(x.Key, x.Value));
}
