using System.Collections.Generic;

namespace Kuttab.Core.Services;

public static class RuleNameTranslator
{
    private static readonly Dictionary<string, (string En, string De, string Es, string Tr, string Fr, string Ja)> Translations = new()
    {
        { "اللام الشمسية", ("Sun letters (Lam Shamsiyyah)", "Sonnenbuchstaben (Lam Shamsiyyah)", "Letras Solares (Lam Shamsiyyah)", "Güneş Harfleri (Lam Şemsiyye)", "Lettres Solaires (Lam Shamsiyyah)", "太陽文字（ラーム・シャムシーヤ）") },
        { "اللام القمرية", ("Moon letters (Lam Qamariyyah)", "Mondbuchstaben (Lam Qamariyyah)", "Letras Lunares (Lam Qamariyyah)", "Ay Harfleri (Lam Kameriyye)", "Lettres Lunaires (Lam Qamariyyah)", "月文字（ラーム・カマリーヤ）") },
        { "إظهار النون الساكنة والتنوين", ("Clear pronunciation of Noon Sakinah and Tanween (Izhar)", "Deutliche Aussprache von stummem Nun und Tanwin (Izhar)", "Pronunciación Clara de Noon Sakinah y Tanween (Izhar)", "Sakin Nun ve Tenvinin Açık Okunuşu (İzhar)", "Prononciation Claire de Noon Sakinah et Tanween (Izhar)", "ヌーン・サキナとタンウィーンの明瞭発音（イズハール）") },
        { "إقلاب النون الساكنة والتنوين", ("Conversion of Noon Sakinah and Tanween (Iqlab)", "Umwandlung von stummem Nun und Tanwin (Iqlab)", "Conversión de Noon Sakinah y Tanween (Iqlab)", "Sakin Nun ve Tenvinin Dönüşümü (İklab)", "Conversion de Noon Sakinah et Tanween (Iqlab)", "ヌーン・サキナとタンウィーンの変換（イクラーブ）") },
        { "إدغام النون الساكنة والتنوين - بغنة", ("Merging Noon Sakinah and Tanween with nasalization (Idgham with Ghunnah)", "Verschmelzung von stummem Nun und Tanwin mit Nasalierung (Idgham mit Ghunnah)", "Fusión de Noon Sakinah y Tanween con nasalización (Idgham con Ghunnah)", "Sakin Nun ve Tenvinin Ğunne ile Birleşimi", "Fusion de Noon Sakinah et Tanween avec nasalisation (Idgham avec Ghunnah)", "ガンナ付きヌーン・サキナとタンウィーンの合併（イドガーム）") },
        { "إدغام النون الساكنة والتنوين - بدون غنة", ("Merging Noon Sakinah and Tanween without nasalization (Idgham without Ghunnah)", "Verschmelzung von stummem Nun und Tanwin ohne Nasalierung (Idgham ohne Ghunnah)", "Fusión de Noon Sakinah y Tanween sin nasalización (Idgham sin Ghunnah)", "Sakin Nun ve Tenvinin Ğunnesiz Birleşimi", "Fusion de Noon Sakinah et Tanween sans nasalisation (Idgham sans Ghunnah)", "ヌーン・サキナとタンウィーンのガンナなし合併（イドガーム）") },
        { "إخفاء النون الساكنة والتنوين", ("Concealment of Noon Sakinah and Tanween (Ikhfa Noon and Tanween)", "Verdeckung von stummem Nun und Tanwin (Ikhfa Noon und Tanween)", "Ocultamiento de Noon Sakinah y Tanween (Ikhfa)", "Sakin Nun ve Tenvinin Gizlenmesi (İhfa)", "Dissimulation de Noon Sakinah et Tanween (Ikhfa)", "ヌーン・サキナとタンウィーンの隠蔽（イフファー）") },
        { "إخفاء الميم الساكنة", ("Concealment of Meem Sakinah (Ikhfa Meem)", "Verdeckung von stummem Mim (Ikhfa Meem)", "Ocultamiento de Meem Sakinah (Ikhfa Meem)", "Sakin Mimin Gizlenmesi (İhfa Mim)", "Dissimulation de Meem Sakinah (Ikhfa Meem)", "ミーム・サキナの隠蔽（イフファー・ミーム）") },
        { "إدغام الميم الساكنة", ("Merging Meem Sakinah (Idgham Meem)", "Verschmelzung von stummem Mim (Idgham Meem)", "Fusión de Meem Sakinah (Idgham Meem)", "Sakin Mimin Birleşimi (İdğam Mim)", "Fusion de Meem Sakinah (Idgham Meem)", "ミーム・サキナの合併（イドガーム・ミーム）") },
        { "إظهار الميم الساكنة", ("Clear pronunciation of Meem Sakinah (Izhar Meem)", "Deutliche Aussprache von stummem Mim (Izhar Meem)", "Pronunciación Clara de Meem Sakinah (Izhar Meem)", "Sakin Mimin Açık Okunuşu (İzhar Mim)", "Prononciation Claire de Meem Sakinah (Izhar Meem)", "ミーム・サキナの明瞭発音（イズハール・ミーム）") },
        { "أحكام النون والميم المشددتين", ("Rules of stressed Noon and Meem (Noon and Meem Mushaddadah)", "Regeln für betontes Nun und Mim (Nun und Mim Mushaddadah)", "Reglas de Noon y Meem Mushaddadah", "Şeddeli Nun ve Mim Kuralları", "Règles du Noon et Meem Mushaddadah", "シャッダ付きヌーンとミームの規則") },
        { "قلقلة الحروف", ("Echoing letters (Qalqalah)", "Echo-Buchstaben (Qalqalah)", "Letras de Eco (Qalqalah)", "Titreşimli Harfler (Kalkale)", "Lettres d'Écho (Qalqalah)", "反響文字（カルカラ）") },
        { "المد الطبيعي", ("Natural prolongation (Madd Tabee’i)", "Natürliche Verlängerung (Madd Tabii)", "Prolongación Natural (Madd Tabii)", "Doğal Uzatma (Med Tabiî)", "Prolongation Naturelle (Madd Tabii)", "自然延長（マッド・タビーイー）") },
        { "المد المنفصل", ("Separated prolongation (Madd Munfasil)", "Getrennte Verlängerung (Madd Munfasil)", "Prolongación Separada (Madd Munfasil)", "Ayrı Uzatma (Med Munfasil)", "Prolongation Séparée (Madd Munfasil)", "分離延長（マッド・ムンファシル）") },
        { "المد المتصل", ("Connected prolongation (Madd Muttasil)", "Verbundene Verlängerung (Madd Muttasil)", "Prolongación Conectada (Madd Muttasil)", "Bağlı Uzatma (Med Muttasil)", "Prolongation Connectée (Madd Muttasil)", "連結延長（マッド・ムッタシル）") },
        { "المد اللازم المخفف الكلمي", ("Light obligatory prolongation in words (Madd Lazim Mukhaffaf Kalimi)", "Leichte notwendige Verlängerung im Wort (Madd Lazim Mukhaffaf Kalimi)", "Prolongación Obligatoria Leve en Palabras (Madd Lazim Mukhaffaf Kalimi)", "Kelimedeki Hafif Zorunlu Uzatma", "Prolongation Obligatoire Légère dans les Mots (Madd Lazim Mukhaffaf Kalimi)", "語中の軽い義務延長（マッド・ラーズィム・ムハッファフ・カリミー）") },
        { "المد اللازم المثقل الكلمي", ("Heavy obligatory prolongation in words (Madd Lazim Muthaqqal Kalimi)", "Starke notwendige Verlängerung im Wort (Madd Lazim Muthaqqal Kalimi)", "Prolongación Obligatoria Pesada en Palabras (Madd Lazim Muthaqqal Kalimi)", "Kelimedeki Ağır Zorunlu Uzatma", "Prolongation Obligatoire Lourde dans les Mots (Madd Lazim Muthaqqal Kalimi)", "語中の重い義務延長（マッド・ラーズィム・ムサッカル・カリミー）") },
        { "المد اللازم المخفف الحرفي", ("Light obligatory prolongation in letters (Madd Lazim Mukhaffaf Harfi)", "Leichte notwendige Verlängerung im Buchstaben (Madd Lazim Mukhaffaf Harfi)", "Prolongación Obligatoria Leve en Letras (Madd Lazim Mukhaffaf Harfi)", "Harfteki Hafif Zorunlu Uzatma", "Prolongation Obligatoire Légère dans les Lettres (Madd Lazim Mukhaffaf Harfi)", "文字中の軽い義務延長（マッド・ラーズィム・ムハッファフ・ハルフィー）") },
        { "المد اللازم المثقل الحرفي", ("Heavy obligatory prolongation in letters (Madd Lazim Muthaqqal Harfi)", "Starke notwendige Verlängerung im Buchstaben (Madd Lazim Muthaqqal Harfi)", "Prolongación Obligatoria Pesada en Letras (Madd Lazim Muthaqqal Harfi)", "Harfteki Ağır Zorunlu Uzatma", "Prolongation Obligatoire Lourde dans les Lettres (Madd Lazim Muthaqqal Harfi)", "文字中の重い義務延長（マッド・ラーズィム・ムサッカル・ハルフィー）") },
        { "مد اللين", ("Soft prolongation (Madd Leen)", "Weiche Verlängerung (Madd Leen)", "Prolongación Suave (Madd Leen)", "Yumuşak Uzatma (Med Leyyin)", "Prolongation Douce (Madd Leen)", "柔らかい延長（マッド・ライン）") },
        { "مد البدل", ("Replacement prolongation (Madd Badal)", "Ersatzverlängerung (Madd Badal)", "Prolongación de Sustitución (Madd Badal)", "Değiştirme Uzatması (Med Bedel)", "Prolongation de Substitution (Madd Badal)", "置換延長（マッド・バダル）") },
        { "مد الصلة الصغرى", ("Minor connecting prolongation (Madd Silah Sughra)", "Kleine Verbindungsverlängerung (Madd Silah Sughra)", "Prolongación de Conexión Menor (Madd Silah Sughra)", "Küçük Bağlantı Uzatması (Med Sıla Suğrâ)", "Prolongation de Connexion Mineure (Madd Silah Sughra)", "小連結延長（マッド・スィラ・スグラー）") },
        { "مد الصلة الكبرى", ("Major connecting prolongation (Madd Silah Kubra)", "Große Verbindungsverlängerung (Madd Silah Kubra)", "Prolongación de Conexión Mayor (Madd Silah Kubra)", "Büyük Bağlantı Uzatması (Med Sıla Kübrâ)", "Prolongation de Connexion Majeure (Madd Silah Kubra)", "大連結延長（マッド・スィラ・クブラー）") },
        { "مد العوض", ("Compensation prolongation (Madd ‘Iwad)", "Ersatzverlängerung (Madd Iwad)", "Prolongación de Compensación (Madd Iwad)", "Tazminat Uzatması (Med İvaz)", "Prolongation de Compensation (Madd Iwad)", "補償延長（マッド・イワド）") },
        { "المد العارض للسكون", ("Prolongation due to incidental sukoon (Madd ‘Arid Lissukoon)", "Verlängerung wegen vorübergehendem Sukoon (Madd Arid Lissukoon)", "Prolongación por Sukoon Incidental (Madd Arid Lissukoon)", "Arızi Sukûn Uzatması (Med Ârız Lissükûn)", "Prolongation due au Sukoon Accidentel (Madd Arid Lissukoon)", "偶発的スクーンによる延長（マッド・アーリド・リッスクーン）") },
        { "جواز الوقف مستوي الطرفين", ("Permissible stop, both options equal", "Erlaubter Halt, beide Optionen gleichwertig", "Parada Permisible, ambas opciones iguales", "Caiz Vakıf, her iki seçenek eşit", "Arrêt Permissible, les deux options égales", "許容停止（両方の選択肢が等しい）") },
        { "جواز الوقف والوصل أولى", ("Permissible to stop, but continuation is preferable", "Halt ist erlaubt, aber das Fortsetzen ist vorzuziehen", "Parada Permisible, pero continuar es preferible", "Caiz Vakıf, devam etmek tercih edilir", "Arrêt Permissible, mais continuer est préférable", "許容停止（ただし続行が好ましい）") },
        { "جواز الوصل والوقف أولى", ("Permissible to continue, but stopping is preferable", "Fortsetzen ist erlaubt, aber das Anhalten ist vorzuziehen", "Continuar Permisible, pero parar es preferible", "Devam Caiz, fakat durmak tercih edilir", "Continuer Permissible, mais s'arrêter est préférable", "続行許容（ただし停止が好ましい）") },
        { "الوقف الممنوع", ("Prohibited stop", "Verbotener Halt", "Parada Prohibida", "Yasak Vakıf", "Arrêt Interdit", "禁止停止") },
        { "الوقف اللازم", ("Mandatory stop", "Obligatorischer Halt", "Parada Obligatoria", "Zorunlu Vakıf", "Arrêt Obligatoire", "義務的停止") },
        { "الوقف المتعانق", ("Paired stop (Mutually exclusive stop)", "Gekoppelter Halt (wechselseitiger Halt)", "Parada Enlazada (Parada Mutuamente Exclusiva)", "Çiftli Vakıf (Karşılıklı Dışlayıcı Duruş)", "Arrêt Couplé (Arrêt Mutuellement Exclusif)", "対停止（相互排他停止）") },
        { "حروف الاستعلاء المفخمة (خص ضغط قظ)", ("Elevated (emphatic) letters (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)", "Erhabene (betonte) Buchstaben (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)", "Letras Elevadas Enfáticas (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)", "Yükseltilmiş Vurgulu Harfler (Ha, Sad, Dad, Ğayn, Tı, Kaf, Zı)", "Lettres Élevées Emphatiques (Kha, Saad, Daad, Gha, Tta, Qaaf, Dhaad)", "高揚強調文字（ハー、サード、ダード、ガイン、ター、カーフ、ザー）") },
        { "الألف المفخمة (بالتبعية)", ("Emphatic Alif (by following the preceding letter)", "Betontes Alif (abhängig vom vorherigen Buchstaben)", "Alif Enfático (siguiendo la letra anterior)", "Vurgulu Elif (önceki harfe bağlı)", "Alif Emphatique (en suivant la lettre précédente)", "強調されたアリフ（前の文字に従う）") },
        { "لام لفظ الجلالة المفخمة", ("Emphatic Lam in the word of Majesty (Allah)", "Betontes Lam im Gottesnamen (Allah)", "Lam Enfático en la Palabra de Majestad (Allah)", "Celal Lafzındaki Vurgulu Lam (Allah)", "Lam Emphatique dans le Mot de Majesté (Allah)", "尊厳の言葉における強調されたラーム（アッラー）") },
        { "الراء المفخمة", ("Emphatic Raa’", "Betontes Raa", "Raa’ Enfático", "Vurgulu Ra", "Raa’ Emphatique", "強調されたラー") }
    };

    public static string GetLocalizedName(string arabicName, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(arabicName))
            return arabicName;

        if (!Translations.TryGetValue(arabicName, out var value))
            return arabicName;

        return languageCode switch
        {
            "en" => value.En,
            "de" => value.De,
            "es" => value.Es,
            "tr" => value.Tr,
            "fr" => value.Fr,
            "ja" => value.Ja,
            _ => arabicName
        };
    }
}
