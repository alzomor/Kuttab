using System.Collections.Generic;

namespace Kuttab.Core.Services;

public static class RuleNameTranslator
{
    private static readonly Dictionary<string, (string En, string De, string Es, string Tr, string Fr, string Ja)> Translations = new()
    {
        { "اللام الشمسية", ("Sun letters (Lam Shamsiyyah)", "Sonnenbuchstaben (Lam Shamsiyyah)", "Letras solares (Lam Shamsiyyah)", "Güneş harfleri (Lam Şemsiyye)", "Lettres solaires (Lam Shamsiyyah)", "太陽文字 (ラム・シャムスィーヤ)") },
        { "اللام القمرية", ("Moon letters (Lam Qamariyyah)", "Mondbuchstaben (Lam Qamariyyah)", "Letras lunares (Lam Qamariyyah)", "Ay harfleri (Lam Kamariyye)", "Lettres lunaires (Lam Qamariyyah)", "月文字 (ラム・カマリーヤ)") },
        { "إظهار النون الساكنة والتنوين", ("Clear pronunciation of Noon Sakinah and Tanween (Izhar)", "Deutliche Aussprache von stummem Nun und Tanwin (Izhar)", "Pronunciación clara de Nun Sakinah y Tanwin (Izhar)", "Sessiz Nun ve Tenvin'in açık telaffuzu (İzhâr)", "Prononciation claire de Noon Sakinah et Tanwin (Izhar)", "ヌーン・サキーナとタンウィーンの明確な発音 (イズハル)") },
        { "إقلاب النون الساكنة والتنوين", ("Conversion of Noon Sakinah and Tanween (Iqlab)", "Umwandlung von stummem Nun und Tanwin (Iqlab)", "Conversión de Nun Sakinah y Tanwin (Iqlab)", "Sessiz Nun ve Tenwin'in dönüşümü (İklâb)", "Conversion de Noon Sakinah et Tanwin (Iqlab)", "ヌーン・サキーナとタンウィーンの変換 (イクラーブ)") },
        { "إدغام النون الساكنة والتنوين - بغنة", ("Merging Noon Sakinah and Tanween with nasalization (Idgham with Ghunnah)", "Verschmelzung von stummem Nun und Tanwin mit Nasalierung (Idgham mit Ghunnah)", "Fusión de Nun Sakinah y Tanwin con nasalización (Idgham con Ghunnah)", "Sessiz Nun ve Tenwin'in nazalleşmeyle birleşmesi (İdgâm bi'ğünne)", "Fusion de Noon Sakinah et Tanwin avec nasalisation (Idgham avec Ghunnah)", "ヌーン・サキーナとタンウィーンの鼻音化による融合 (イドガーム・ビ・グンナ)") },
        { "إدغام النون الساكنة والتنوين - بدون غنة", ("Merging Noon Sakinah and Tanween without nasalization (Idgham without Ghunnah)", "Verschmelzung von stummem Nun und Tanwin ohne Nasalierung (Idgham ohne Ghunnah)", "Fusión de Nun Sakinah y Tanwin sin nasalización (Idgham sin Ghunnah)", "Sessiz Nun ve Tenwin'in nazalleşme olmadan birleşmesi (İdgâm biğayri ġunne)", "Fusion de Noon Sakinah et Tanwin sans nasalisation (Idgham sans Ghunnah)", "ヌーン・サキーナとタンウィーンの鼻音化なしの融合 (イドガーム・ビ・ガイリ・グンナ)") },
        { "إخفاء النون الساكنة والتنوين", ("Concealment of Noon Sakinah and Tanween (Ikhfa Noon and Tanween)", "Verdeckung von stummem Nun und Tanwin (Ikhfa Noon und Tanween)", "Ocultamiento de Nun Sakinah y Tanwin (Ikhfa)", "Sessiz Nun ve Tenwin'in gizlenmesi (İhfâ)", "Dissimulation de Noon Sakinah et Tanwin (Ikhfa)", "ヌーン・サキーナとタンウィーンの隠蔽 (イフファー)") },
        { "إخفاء الميم الساكنة", ("Concealment of Meem Sakinah (Ikhfa Meem)", "Verdeckung von stummem Mim (Ikhfa Meem)", "Ocultamiento de Meem Sakinah (Ikhfa)", "Sessiz Mim'in gizlenmesi (İhfâ)", "Dissimulation de Meem Sakinah (Ikhfa)", "ミーム・サキーンの隠蔽 (イフファー)") },
        { "إدغام الميم الساكنة", ("Merging Meem Sakinah (Idgham Meem)", "Verschmelzung von stummem Mim (Idgham Meem)", "Fusión de Meem Sakinah (Idgham)", "Sessiz Mim'in birleşmesi (İdgâm)", "Fusion de Meem Sakinah (Idgham)", "ミーム・サキーンの融合 (イドガーム)") },
        { "إظهار الميم الساكنة", ("Clear pronunciation of Meem Sakinah (Izhar Meem)", "Deutliche Aussprache von stummem Mim (Izhar Meem)", "Pronunciación clara de Meem Sakinah (Izhar)", "Sessiz Mim'in açık telaffuzu (İzhâr)", "Prononciation claire de Meem Sakinah (Izhar)", "ミーム・サキーンの明確な発音 (イズハル)") },
        { "أحكام النون والميم المشددتين", ("Rules of stressed Noon and Meem (Noon and Meem Mushaddadah)", "Regeln für betontes Nun und Mim (Nun und Mim Mushaddadah)", "Reglas de Noon y Meem acentuadas (Noon y Meem Mushaddadah)", "Vurgulu Nun ve Meem kuralları (Nûn ve Mîm Muşaddade)", "Règles de Noon et Meem accentués (Noon et Meem Mushaddadah)", "強調されたヌーンとミームの規則 (ヌーン・ワ・ミーム・ムシャッダダ)") },
        { "قلقلة الحروف", ("Echoing letters (Qalqalah)", "Echo-Buchstaben (Qalqalah)", "Letras resonantes (Qalqalah)", "Yankılı harfler (Kalkale)", "Lettres résonantes (Qalqalah)", "反響文字 (カルカラ)") },
        { "المد الطبيعي", ("Natural prolongation (Madd Tabee'i)", "Natürliche Verlängerung (Madd Tabii)", "Prolongación natural (Madd Tabee'i)", "Doğal uzatma (Medd Tabîî)", "Prolongation naturelle (Madd Tabee'i)", "自然な延長 (マッド・タビーイー)") },
        { "المد المنفصل", ("Separated prolongation (Madd Munfasil)", "Getrennte Verlängerung (Madd Munfasil)", "Prolongación separada (Madd Munfasil)", "Ayrık uzatma (Medd Münfasıl)", "Prolongation séparée (Madd Munfasil)", "分離された延長 (マッド・ムンファシル)") },
        { "المد المتصل", ("Connected prolongation (Madd Muttasil)", "Verbundene Verlängerung (Madd Muttasil)", "Prolongación conectada (Madd Muttasil)", "Bağlı uzatma (Medd Muttasıl)", "Prolongation connectée (Madd Muttasil)", "接続された延長 (マッド・ムッタシル)") },
        { "المد اللازم المخفف الكلمي", ("Light obligatory prolongation in words (Madd Lazim Mukhaffaf Kalimi)", "Leichte notwendige Verlängerung im Wort (Madd Lazim Mukhaffaf Kalimi)", "Prolongación obligatoria ligera en palabras (Madd Lazim Mukhaffaf Kalimi)", "Kelimelerde hafif zorunlu uzatma (Medd Lâzim Muhaffaf Kelîmî)", "Prolongation obligatoire légère dans les mots (Madd Lazim Mukhaffaf Kalimi)", "単語内の軽い必須延長 (マッド・ラーズィム・ムハッファフ・カリーミー)") },
        { "المد اللازم المثقل الكلمي", ("Heavy obligatory prolongation in words (Madd Lazim Muthaqqal Kalimi)", "Starke notwendige Verlängerung im Wort (Madd Lazim Muthaqqal Kalimi)", "Prolongación obligatoria pesada en palabras (Madd Lazim Muthaqqal Kalimi)", "Kelimelerde ağır zorunlu uzatma (Medd Lâzim Müthakkel Kelîmî)", "Prolongation obligatoire lourde dans les mots (Madd Lazim Muthaqqal Kalimi)", "単語内の重い必須延長 (マッド・ラーズィム・ムサッカル・カリーミー)") },
        { "المد اللازم المخفف الحرفي", ("Light obligatory prolongation in letters (Madd Lazim Mukhaffaf Harfi)", "Leichte notwendige Verlängerung im Buchstaben (Madd Lazim Mukhaffaf Harfi)", "Prolongación obligatoria ligera en letras (Madd Lazim Mukhaffaf Harfi)", "Harflerde hafif zorunlu uzatma (Medd Lâzim Muhaffaf Harfî)", "Prolongation obligatoire légère dans les lettres (Madd Lazim Mukhaffaf Harfi)", "文字内の軽い必須延長 (マッド・ラーズィム・ムハッファフ・ハルフィー)") },
        { "المد اللازم المثقل الحرفي", ("Heavy obligatory prolongation in letters (Madd Lazim Muthaqqal Harfi)", "Starke notwendige Verlängerung im Buchstaben (Madd Lazim Muthaqqal Harfi)", "Prolongación obligatoria pesada en letras (Madd Lazim Muthaqqal Harfi)", "Harflerde ağır zorunlu uzatma (Medd Lâzim Müthakkel Harfî)", "Prolongation obligatoire lourde dans les lettres (Madd Lazim Muthaqqal Harfi)", "文字内の重い必須延長 (マッド・ラーズィム・ムサッカル・ハルフィー)") },
        { "مد اللين", ("Soft prolongation (Madd Leen)", "Weiche Verlängerung (Madd Leen)", "Prolongación suave (Madd Leen)", "Yumuşak uzatma (Medd Leyn)", "Prolongation douce (Madd Leen)", "柔らかい延長 (マッド・リーン)") },
        { "مد البدل", ("Replacement prolongation (Madd Badal)", "Ersatzverlängerung (Madd Badal)", "Prolongación de reemplazo (Madd Badal)", "Yerine koyma uzatması (Medd Bedel)", "Prolongation de remplacement (Madd Badal)", "置換延長 (マッド・バダル)") },
        { "مد الصلة الصغرى", ("Minor connecting prolongation (Madd Silah Sughra)", "Kleine Verbindungsverlängerung (Madd Silah Sughra)", "Prolongación de conexión menor (Madd Silah Sughra)", "Küçük bağlantı uzatması (Medd Silatu Sughra)", "Prolongation de connexion mineure (Madd Silah Sughra)", "小さな接続延長 (マッド・シラ・スグラ)") },
        { "مد الصلة الكبرى", ("Major connecting prolongation (Madd Silah Kubra)", "Große Verbindungsverlängerung (Madd Silah Kubra)", "Prolongación de conexión mayor (Madd Silah Kubra)", "Büyük bağlantı uzatması (Medd Silatu Kubra)", "Prolongation de connexion majeure (Madd Silah Kubra)", "大きな接続延長 (マッド・シラ・クブラ)") },
        { "مد العوض", ("Compensation prolongation (Madd 'Iwad)", "Ersatzverlängerung (Madd Iwad)", "Prolongación de compensación (Madd 'Iwad)", "Telafi uzatması (Medd 'İvâd)", "Prolongation de compensation (Madd 'Iwad)", "補償延長 (マッド・イワード)") },
        { "المد العارض للسكون", ("Prolongation due to incidental sukoon (Madd 'Arid Lissukoon)", "Verlängerung wegen vorübergehendem Sukoon (Madd Arid Lissukoon)", "Prolongación por sukun incidental (Madd 'Arid Lissukoon)", "Geçici sukun nedeniyle uzatma (Medd 'Ârıd li's-sukûn)", "Prolongation due to sukun incidentel (Madd 'Arid Lissukoon)", "偶発スクーンによる延長 (マッド・アーリド・リッ・スクーン)") },
        { "جواز الوقف مستوي الطرفين", ("Permissible stop, both options equal", "Erlaubter Halt, beide Optionen gleichwertig", "Parada permisible, ambas opciones iguales", "Duraklama izni, her iki seçenek eşit", "Arrêt permis, deux options égales", "許容される停止、両方の選択肢が等しい") },
        { "جواز الوقف والوصل أولى", ("Permissible to stop, but continuation is preferable", "Halt ist erlaubt, aber das Fortsetzen ist vorzuziehen", "Parada permisible, pero la continuación es preferible", "Duraklama izni var ama devam etmek daha iyi", "Arrêt permis, mais la continuation est préférable", "停止は許容されるが継続が好ましい") },
        { "جواز الوصل والوقف أولى", ("Permissible to continue, but stopping is preferable", "Fortsetzen ist erlaubt, aber das Anhalten ist vorzuziehen", "Continuación permisible, pero la parada es preferible", "Devam etme izni var ama duraklamak daha iyi", "Continuation permise, mais l'arrêt est préférable", "継続は許容されるが停止が好ましい") },
        { "الوقف الممنوع", ("Prohibited stop", "Verbotener Halt", "Parada prohibida", "Yasaklanmış duraklama", "Arrêt interdit", "禁止された停止") },
        { "الوقف اللازم", ("Mandatory stop", "Obligatorischer Halt", "Parada obligatoria", "Zorunlu duraklama", "Arrêt obligatoire", "必須の停止") },
        { "الوقف المتعانق", ("Paired stop (Mutually exclusive stop)", "Gekoppelter Halt (wechselseitiger Halt)", "Parada emparejada (mutuamente exclusiva)", "Eşleşik duraklama (karşılıklı olarak özel)", "Arrêt apparié (mutuellement exclusif)", "対になった停止 (相互排他的)") },
        { "حروف الاستعلاء المفخمة (خص ضغط قظ)", ("Elevated (emphatic) letters (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)", "Erhabene (betonte) Buchstaben (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)", "Letras elevadas (enfáticas) (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)", "Yükseltilmiş (vurgulu) harfler (Ha, Sa, Da, Ğa, Tta, Kaf, Dha)", "Lettres élevées (emphatiques) (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)", "高い（強調された）文字 (ハー、サード、ダード、ガー、タ、カーフ、ダード)") },
        { "الألف المفخمة (بالتبعية)", ("Emphatic Alif (by following the preceding letter)", "Betontes Alif (abhängig vom vorherigen Buchstaben)", "Alif enfático (siguiendo la letra precedente)", "Vurgulu Alif (önceki harfi takip ederek)", "Alif emphatique (en suivant la lettre précédente)", "強調されたアリフ（前の文字に従って）") },
        { "لام لفظ الجلالة المفخمة", ("Emphatic Lam in the word of Majesty (Allah)", "Betontes Lam im Gottesnamen (Allah)", "Lam enfático en la palabra de Majestad (Allah)", "Majestet kelimesinde vurgulu Lam (Allah)", "Lam emphatique dans le mot de Majesté (Allah)", "威厳の言葉における強調されたラーム（アッラー）") },
        { "الراء المفخمة", ("Emphatic Raa'", "Betontes Raa", "Raa enfático", "Vurgulu Ra", "Raa emphatique", "強調されたラー") }
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
            "es" => value.Es,
            "tr" => value.Tr,
            "fr" => value.Fr,
            "ja" => value.Ja,
            _ => arabicName
        };

        // Remove the trailing parenthetical transliteration e.g. " (Madd Tabee'i)"
        var parenIdx = raw.LastIndexOf(" (", System.StringComparison.Ordinal);
        if (parenIdx > 0 && raw.EndsWith(")"))
            return raw.Substring(0, parenIdx);

        return raw;
    }
}
