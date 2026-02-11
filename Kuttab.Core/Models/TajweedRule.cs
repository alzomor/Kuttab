using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kuttab.Core.Models;

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

public class TajweedRuleMatch
{
    public string RuleName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int MatchStart { get; set; }
    public int MatchLength { get; set; }
    public string MatchedText { get; set; } = string.Empty;
    public string SurroundingText { get; set; } = string.Empty;
    public int SurroundingStart { get; set; }
    public int HighlightStartInSurrounding { get; set; }
    public int HighlightLengthInSurrounding { get; set; }
}
