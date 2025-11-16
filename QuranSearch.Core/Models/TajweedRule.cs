using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuranSearch.Core.Models;

public class TajweedRule
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("group")]
    public string? Group { get; set; }
    
    [JsonPropertyName("cases")]
    public List<TajweedCase> Cases { get; set; } = new();
}

public class TajweedCase
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("regex")]
    public string Regex { get; set; } = string.Empty;
}

public class RulesContainer
{
    [JsonPropertyName("rules")]
    public List<TajweedRule> Rules { get; set; } = new();
}
