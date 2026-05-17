using System.Collections.Generic;

namespace Kuttab.Core.Services;

public static class RuleGroupTranslator
{
    // Group IDs
    // lam, noon_tanween, meem_sakinah, noon_meem_mushaddad, qalqalah, mad, waqf, tafkhim_tarqiq
    private static readonly Dictionary<string, (string Ar, string En, string De, string Es, string Tr, string Fr, string Ja)> Groups = new()
    {
        {
            "lam",
            ("اللام الشمسية و القمرية",
             "Sun and Moon Lam",
             "Sonnen- und Mond-Lam",
             "Lam solar y lunar",
             "Güneş ve Ay Lam",
             "Lam solaire et lunaire",
             "太陽と月のラーム")
        },
        {
            "noon_tanween",
            ("أحكام النون الساكنة و التنوين",
             "Rules of Noon Sakinah and Tanween",
             "Regeln für stummes Nun und Tanwin",
             "Reglas de Nun Sakinah y Tanwin",
             "Sessiz Nun ve Tenvin kuralları",
             "Règles de Noon Sakinah et Tanwin",
             "ヌーン・サキーナとタンウィーンの規則")
        },
        {
            "meem_sakinah",
            ("أحكام الميم الساكنة",
             "Rules of Meem Sakinah",
             "Regeln für stummes Mim",
             "Reglas de Meem Sakinah",
             "Sessiz Mim kuralları",
             "Règles de Meem Sakinah",
             "ミーム・サキーンの規則")
        },
        {
            "noon_meem_mushaddad",
            ("أحكام النون والميم المشددتين",
             "Rules of stressed Noon and Meem",
             "Regeln für betontes Nun und Mim",
             "Reglas de Noon y Meem acentuadas",
             "Vurgulu Nun ve Meem kuralları",
             "Règles de Noon et Meem accentués",
             "強調されたヌーンとミームの規則")
        },
        {
            "qalqalah",
            ("قلقلة الحروف",
             "Qalqalah letters",
             "Qalqalah-Buchstaben",
             "Letras Qalqalah",
             "Kalkale harfleri",
             "Lettres Qalqalah",
             "カルカラ文字")
        },
        {
            "mad",
            ("المدود",
             "Types of Madd (elongation)",
             "Arten des Madd (Verlängerung)",
             "Tipos de Madd (prolongación)",
             "Medd türleri (uzatma)",
             "Types de Madd (prolongation)",
             "マッド（延長）の種類")
        },
        {
            "waqf",
            ("علامات الوقف",
             "Stopping signs",
             "Waqf-Zeichen (Haltezeichen)",
             "Señales de parada",
             "Duraklama işaretleri",
             "Signes d'arrêt",
             "停止記号")
        },
        {
            "tafkhim_tarqiq",
            ("التفخيم و الترقيق",
             "Tafkhim and Tarqiq (emphasis / lightness)",
             "Tafkhim und Tarqiq (Verstärkung / Abschwächung)",
             "Tafkhim y Tarqiq (énfasis / suavidad)",
             "Tafhim ve Tarrik (vurgu / hafiflik)",
             "Tafkhim et Tarqiq (emphase / légèreté)",
             "タフヒームとタルキーク（強調と軽さ）")
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
            "es" => value.Es,
            "tr" => value.Tr,
            "fr" => value.Fr,
            "ja" => value.Ja,
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
