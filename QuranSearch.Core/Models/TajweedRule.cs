namespace QuranSearch.Core.Models;

public class TajweedRule
{
    public string Name { get; set; } = string.Empty;
    public List<TajweedCase> Cases { get; set; } = new();
}

public class TajweedCase
{
    public string Description { get; set; } = string.Empty;
    public string Regex { get; set; } = string.Empty;
}

public class RulesContainer
{
    public List<TajweedRule> Rules { get; set; } = new();
}
