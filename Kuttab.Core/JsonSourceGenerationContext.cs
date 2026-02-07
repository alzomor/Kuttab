using System.Collections.Generic;
using System.Text.Json.Serialization;
using Kuttab.Core.Models;

namespace Kuttab.Core;

[JsonSerializable(typeof(RulesContainer))]
[JsonSerializable(typeof(List<TajweedRule>))]
[JsonSerializable(typeof(TajweedRule))]
[JsonSerializable(typeof(TajweedCase))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class KuttabJsonContext : JsonSerializerContext
{
}
