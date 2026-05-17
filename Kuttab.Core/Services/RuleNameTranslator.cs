using System.Collections.Generic;

namespace Kuttab.Core.Services;

public static class RuleNameTranslator
{
    private static readonly Dictionary<string, (string En, string De)> Translations = new()
    {
        { "اللام الشمسية", ("Sun letters (Lam Shamsiyyah)", "Sonnenbuchstaben (Lam Shamsiyyah)") },
        { "اللام القمرية", ("Moon letters (Lam Qamariyyah)", "Mondbuchstaben (Lam Qamariyyah)") },
        { "إظهار النون الساكنة والتنوين", ("Clear pronunciation of Noon Sakinah and Tanween (Izhar)", "Deutliche Aussprache von stummem Nun und Tanwin (Izhar)") },
        { "إقلاب النون الساكنة والتنوين", ("Conversion of Noon Sakinah and Tanween (Iqlab)", "Umwandlung von stummem Nun und Tanwin (Iqlab)") },
        { "إدغام النون الساكنة والتنوين - بغنة", ("Merging Noon Sakinah and Tanween with nasalization (Idgham with Ghunnah)", "Verschmelzung von stummem Nun und Tanwin mit Nasalierung (Idgham mit Ghunnah)") },
        { "إدغام النون الساكنة والتنوين - بدون غنة", ("Merging Noon Sakinah and Tanween without nasalization (Idgham without Ghunnah)", "Verschmelzung von stummem Nun und Tanwin ohne Nasalierung (Idgham ohne Ghunnah)") },
        { "إخفاء النون الساكنة والتنوين", ("Concealment of Noon Sakinah and Tanween (Ikhfa Noon and Tanween)", "Verdeckung von stummem Nun und Tanwin (Ikhfa Noon und Tanween)") },
        { "إخفاء الميم الساكنة", ("Concealment of Meem Sakinah (Ikhfa Meem)", "Verdeckung von stummem Mim (Ikhfa Meem)") },
        { "إدغام الميم الساكنة", ("Merging Meem Sakinah (Idgham Meem)", "Verschmelzung von stummem Mim (Idgham Meem)") },
        { "إظهار الميم الساكنة", ("Clear pronunciation of Meem Sakinah (Izhar Meem)", "Deutliche Aussprache von stummem Mim (Izhar Meem)") },
        { "أحكام النون والميم المشددتين", ("Rules of stressed Noon and Meem (Noon and Meem Mushaddadah)", "Regeln für betontes Nun und Mim (Nun und Mim Mushaddadah)") },
        { "قلقلة الحروف", ("Echoing letters (Qalqalah)", "Echo-Buchstaben (Qalqalah)") },
        { "المد الطبيعي", ("Natural prolongation (Madd Tabee’i)", "Natürliche Verlängerung (Madd Tabii)") },
        { "المد المنفصل", ("Separated prolongation (Madd Munfasil)", "Getrennte Verlängerung (Madd Munfasil)") },
        { "المد المتصل", ("Connected prolongation (Madd Muttasil)", "Verbundene Verlängerung (Madd Muttasil)") },
        { "المد اللازم المخفف الكلمي", ("Light obligatory prolongation in words (Madd Lazim Mukhaffaf Kalimi)", "Leichte notwendige Verlängerung im Wort (Madd Lazim Mukhaffaf Kalimi)") },
        { "المد اللازم المثقل الكلمي", ("Heavy obligatory prolongation in words (Madd Lazim Muthaqqal Kalimi)", "Starke notwendige Verlängerung im Wort (Madd Lazim Muthaqqal Kalimi)") },
        { "المد اللازم المخفف الحرفي", ("Light obligatory prolongation in letters (Madd Lazim Mukhaffaf Harfi)", "Leichte notwendige Verlängerung im Buchstaben (Madd Lazim Mukhaffaf Harfi)") },
        { "المد اللازم المثقل الحرفي", ("Heavy obligatory prolongation in letters (Madd Lazim Muthaqqal Harfi)", "Starke notwendige Verlängerung im Buchstaben (Madd Lazim Muthaqqal Harfi)") },
        { "مد اللين", ("Soft prolongation (Madd Leen)", "Weiche Verlängerung (Madd Leen)") },
        { "مد البدل", ("Replacement prolongation (Madd Badal)", "Ersatzverlängerung (Madd Badal)") },
        { "مد الصلة الصغرى", ("Minor connecting prolongation (Madd Silah Sughra)", "Kleine Verbindungsverlängerung (Madd Silah Sughra)") },
        { "مد الصلة الكبرى", ("Major connecting prolongation (Madd Silah Kubra)", "Große Verbindungsverlängerung (Madd Silah Kubra)") },
        { "مد العوض", ("Compensation prolongation (Madd ‘Iwad)", "Ersatzverlängerung (Madd Iwad)") },
        { "المد العارض للسكون", ("Prolongation due to incidental sukoon (Madd ‘Arid Lissukoon)", "Verlängerung wegen vorübergehendem Sukoon (Madd Arid Lissukoon)") },
        { "جواز الوقف مستوي الطرفين", ("Permissible stop, both options equal", "Erlaubter Halt, beide Optionen gleichwertig") },
        { "جواز الوقف والوصل أولى", ("Permissible to stop, but continuation is preferable", "Halt ist erlaubt, aber das Fortsetzen ist vorzuziehen") },
        { "جواز الوصل والوقف أولى", ("Permissible to continue, but stopping is preferable", "Fortsetzen ist erlaubt, aber das Anhalten ist vorzuziehen") },
        { "الوقف الممنوع", ("Prohibited stop", "Verbotener Halt") },
        { "الوقف اللازم", ("Mandatory stop", "Obligatorischer Halt") },
        { "الوقف المتعانق", ("Paired stop (Mutually exclusive stop)", "Gekoppelter Halt (wechselseitiger Halt)") },
        { "حروف الاستعلاء المفخمة (خص ضغط قظ)", ("Elevated (emphatic) letters (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)", "Erhabene (betonte) Buchstaben (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)") },
        { "الألف المفخمة (بالتبعية)", ("Emphatic Alif (by following the preceding letter)", "Betontes Alif (abhängig vom vorherigen Buchstaben)") },
        { "لام لفظ الجلالة المفخمة", ("Emphatic Lam in the word of Majesty (Allah)", "Betontes Lam im Gottesnamen (Allah)") },
        { "الراء المفخمة", ("Emphatic Raa’", "Betontes Raa") }
    };

    public static string GetLocalizedName(string arabicName, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(arabicName))
            return arabicName;

        if (!Translations.TryGetValue(arabicName, out var value))
            return arabicName;

        var raw = languageCode switch
        {
            "en" => value.En,
            "de" => value.De,
            _ => arabicName
        };

        // Remove the trailing parenthetical transliteration e.g. " (Madd Tabee'i)"
        var parenIdx = raw.LastIndexOf(" (", System.StringComparison.Ordinal);
        if (parenIdx > 0 && raw.EndsWith(")"))
            return raw.Substring(0, parenIdx);

        return raw;
    }
}
