using System.Collections.Generic;

namespace Kuttab.Core.Services;

public static class RuleGroupTranslator
{
    // Group IDs
    // lam, noon_tanween, meem_sakinah, noon_meem_mushaddad, qalqalah, mad, waqf, tafkhim_tarqiq
    private static readonly Dictionary<string, (string Ar, string En, string De)> Groups = new()
    {
        {
            "lam",
            ("اللام الشمسية و القمرية",
             "Sun and Moon Lam",
             "Sonnen- und Mond-Lam")
        },
        {
            "noon_tanween",
            ("أحكام النون الساكنة و التنوين",
             "Rules of Noon Sakinah and Tanween",
             "Regeln für stummes Nun und Tanwin")
        },
        {
            "meem_sakinah",
            ("أحكام الميم الساكنة",
             "Rules of Meem Sakinah",
             "Regeln für stummes Mim")
        },
        {
            "noon_meem_mushaddad",
            ("أحكام النون والميم المشددتين",
             "Rules of stressed Noon and Meem",
             "Regeln für betontes Nun und Mim")
        },
        {
            "qalqalah",
            ("قلقلة الحروف",
             "Qalqalah letters",
             "Qalqalah-Buchstaben")
        },
        {
            "mad",
            ("المدود",
             "Types of Madd (elongation)",
             "Arten des Madd (Verlängerung)")
        },
        {
            "waqf",
            ("علامات الوقف",
             "Stopping signs",
             "Waqf-Zeichen (Haltezeichen)")
        },
        {
            "tafkhim_tarqiq",
            ("التفخيم و الترقيق",
             "Tafkhim and Tarqiq (emphasis / lightness)",
             "Tafkhim und Tarqiq (Verstärkung / Abschwächung)")
        }
    };

    public static string GetGroupTitle(string? groupId, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(groupId))
            return string.Empty;

        if (!Groups.TryGetValue(groupId, out var value))
            return string.Empty;

        return languageCode switch
        {
            "en" => value.En,
            "de" => value.De,
            _ => value.Ar
        };
    }

    /// <summary>
    /// Returns all group IDs in display order
    /// </summary>
    public static List<string> GetAllGroupIds()
    {
        return new List<string>(Groups.Keys);
    }

    /// <summary>
    /// Returns all groups as (groupId, localizedTitle) pairs for the given language
    /// </summary>
    public static List<(string GroupId, string Title)> GetAllGroups(string languageCode)
    {
        var result = new List<(string, string)>();
        foreach (var kvp in Groups)
        {
            var title = languageCode switch
            {
                "en" => kvp.Value.En,
                "de" => kvp.Value.De,
                _ => kvp.Value.Ar
            };
            result.Add((kvp.Key, title));
        }
        return result;
    }
}
