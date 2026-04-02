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
             "Lam Solar y Lunar",
             "Güneş ve Ay Lam’ı",
             "Lam Solaire et Lunaire",
             "太陽と月のラーム")
        },
        {
            "noon_tanween",
            ("أحكام النون الساكنة و التنوين",
             "Rules of Noon Sakinah and Tanween",
             "Regeln für stummes Nun und Tanwin",
             "Reglas del Noon Sakinah y Tanween",
             "Sakin Nun ve Tenvin Kuralları",
             "Règles du Noon Sakinah et Tanween",
             "ヌーン・サキナとタンウィーンの規則")
        },
        {
            "meem_sakinah",
            ("أحكام الميم الساكنة",
             "Rules of Meem Sakinah",
             "Regeln für stummes Mim",
             "Reglas del Meem Sakinah",
             "Sakin Mim Kuralları",
             "Règles du Meem Sakinah",
             "ミーム・サキナの規則")
        },
        {
            "noon_meem_mushaddad",
            ("أحكام النون والميم المشددتين",
             "Rules of stressed Noon and Meem",
             "Regeln für betontes Nun und Mim",
             "Reglas del Noon y Meem Mushaddadah",
             "Şeddeli Nun ve Mim Kuralları",
             "Règles du Noon et Meem Mushaddadah",
             "シャッダ付きヌーンとミームの規則")
        },
        {
            "qalqalah",
            ("قلقلة الحروف",
             "Qalqalah letters",
             "Qalqalah-Buchstaben",
             "Letras de Qalqalah",
             "Kalkale Harfleri",
             "Lettres de Qalqalah",
             "カルカラ文字")
        },
        {
            "mad",
            ("المدود",
             "Types of Madd (elongation)",
             "Arten des Madd (Verlängerung)",
             "Tipos de Madd (elongación)",
             "Madd Türleri (uzatma)",
             "Types de Madd (élongation)",
             "マッドの種類（延長）")
        },
        {
            "waqf",
            ("علامات الوقف",
             "Stopping signs",
             "Waqf-Zeichen (Haltezeichen)",
             "Señales de Parada (Waqf)",
             "Duruş İşaretleri (Vakıf)",
             "Signes d’Arrêt (Waqf)",
             "停止記号（ワクフ）")
        },
        {
            "tafkhim_tarqiq",
            ("التفخيم و الترقيق",
             "Tafkhim and Tarqiq (emphasis / lightness)",
             "Tafkhim und Tarqiq (Verstärkung / Abschwächung)",
             "Tafkhim y Tarqiq (énfasis / ligereza)",
             "Tafhîm ve Tarkîk (vurgu / hafiflik)",
             "Tafkhim et Tarqiq (emphase / légèreté)",
             "タフキームとタルキーク（強調／軽減）")
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
                "es" => kvp.Value.Es,
                "tr" => kvp.Value.Tr,
                "fr" => kvp.Value.Fr,
                "ja" => kvp.Value.Ja,
                _ => kvp.Value.Ar
            };
            result.Add((kvp.Key, title));
        }
        return result;
    }
}
