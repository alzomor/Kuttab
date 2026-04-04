using System.Collections.Generic;

namespace Kuttab.Core.Services;

/// <summary>
/// Provides explanations for Tajweed rules in multiple languages.
/// Based on the reference document: تجويد_مختصر.odp
/// </summary>
public static class TajweedRulesExplanation
{
    public class RuleExplanation
    {
        public string Arabic { get; set; } = string.Empty;
        public string English { get; set; } = string.Empty;
        public string German { get; set; } = string.Empty;
        public string Spanish { get; set; } = string.Empty;
        public string Turkish { get; set; } = string.Empty;
        public string French { get; set; } = string.Empty;
        public string Japanese { get; set; } = string.Empty;
    }

    private static readonly Dictionary<string, RuleExplanation> Explanations = new()
    {
        // اللام الشمسية - Sun Letters
        {
            "اللام الشمسية",
            new RuleExplanation
            {
                Arabic = @"اللام الشمسية
هي لام التعريف التي لا تُنطق.
إذا تلاها حرف ليس من حروف اللام القمرية.
تُرسم في المصحف معراة (بدون تشكيل) ويليها حرف مشدد.
الحروف الشمسية: ت ث د ذ ر ز س ش ص ض ط ظ ل ن",
                English = @"Sun Letters (Lam Shamsiyyah)
The definite article 'Lam' that is not pronounced.
When followed by a letter that is not from the Moon Letters.
Written in the Mushaf without diacritics, followed by a doubled letter.
Sun Letters: ت ث د ذ ر ز س ش ص ض ط ظ ل ن",
                German = @"Sonnenbuchstaben (Lam Shamsiyyah)
Das bestimmte Artikel 'Lam', das nicht ausgesprochen wird.
Wenn ein Buchstabe folgt, der nicht zu den Mondbuchstaben gehört.
Im Mushaf ohne Vokalzeichen geschrieben, gefolgt von einem verdoppelten Buchstaben.
Sonnenbuchstaben: ت ث د ذ ر ز س ش ص ض ط ظ ل ن",
                Spanish = @"Letras Solares (Lam Shamsiyyah)
El artículo definido 'Lam' que no se pronuncia.
Cuando es seguido por una letra que no es de las Letras Lunares.
Escrito en el Mushaf sin diacríticos, seguido de una letra duplicada.
Letras Solares: ت ث د ذ ر ز س ش ص ض ط ظ ل ن",
                Turkish = @"Güneş Harfleri (Lam Şemsiyye)
Telaffuz edilmeyen belirli tanımlık 'Lam'.
Ay Harflerinden olmayan bir harf takip ettiğinde.
Mushaf'ta harekesiz yazılır, ardından şeddeli bir harf gelir.
Güneş Harfleri: ت ث د ذ ر ز س ش ص ض ط ظ ل ن",
                French = @"Lettres Solaires (Lam Shamsiyyah)
L'article défini 'Lam' qui n'est pas prononcé.
Lorsqu'il est suivi d'une lettre qui n'est pas parmi les Lettres Lunaires.
Écrit dans le Mushaf sans signes diacritiques, suivi d'une lettre doublée.
Lettres Solaires: ت ث د ذ ر ز س ش ص ض ط ظ ل ن",
                Japanese = @"太陽文字（ラーム・シャムシーヤ）
発音されない定冠詞「ラーム」。
月文字以外の文字が続く場合。
ムスハフでは母音記号なしで書かれ、その後に重複した文字が続きます。
太陽文字: ت ث د ذ ر ز س ش ص ض ط ظ ل ن"
            }
        },

        // اللام القمرية - Moon Letters
        {
            "اللام القمرية",
            new RuleExplanation
            {
                Arabic = @"اللام القمرية
هي لام التعريف التي تُنطق.
إذا تلى اللام أحد هذه الحروف تكون لام قمرية: ابغ حجك وخف عقيمه
يُرسم فوقها سكون (رأس حاء) في المصحف.",
                English = @"Moon Letters (Lam Qamariyyah)
The definite article 'Lam' that is pronounced.
When the Lam is followed by one of these letters: ا ب غ ح ج ك و خ ف ع ق ي م ه
A sukoon (small circle) is written above it in the Mushaf.",
                German = @"Mondbuchstaben (Lam Qamariyyah)
Das bestimmte Artikel 'Lam', das ausgesprochen wird.
Wenn dem Lam einer dieser Buchstaben folgt: ا ب غ ح ج ك و خ ف ع ق ي م ه
Ein Sukoon (kleiner Kreis) wird darüber im Mushaf geschrieben.",
                Spanish = @"Letras Lunares (Lam Qamariyyah)
El artículo definido 'Lam' que se pronuncia.
Cuando el Lam es seguido por una de estas letras: ا ب غ ح ج ك و خ ف ع ق ي م ه
Un sukoon (pequeño círculo) se escribe encima en el Mushaf.",
                Turkish = @"Ay Harfleri (Lam Kameriyye)
Telaffuz edilen belirli tanımlık 'Lam'.
Lam'ı şu harflerden biri takip ettiğinde: ا ب غ ح ج ك و خ ف ع ق ي م ه
Mushaf'ta üzerine sukun (küçük daire) yazılır.",
                French = @"Lettres Lunaires (Lam Qamariyyah)
L'article défini 'Lam' qui est prononcé.
Lorsque le Lam est suivi de l'une de ces lettres: ا ب غ ح ج ك و خ ف ع ق ي م ه
Un sukoon (petit cercle) est écrit au-dessus dans le Mushaf.",
                Japanese = @"月文字（ラーム・カマリーヤ）
発音される定冠詞「ラーム」。
ラームの後にこれらの文字のいずれかが続く場合: ا ب غ ح ج ك و خ ف ع ق ي م ه
ムスハフでは上にスクーン（小さな円）が書かれます。"
            }
        },

        // القلقلة - Qalqalah
        {
            "قلقلة الحروف",
            new RuleExplanation
            {
                Arabic = @"القلقلة
هي التباعد بين طرفي النطق عند نطق الحرف المقلقل.
حروفها: قطب جد (ق ط ب ج د)
تُقلقل هذه الحروف عند سكونها أو الوقوف عليها.
لا تُضاف أي حركة لهذه الحروف وتبقى ساكنة.
الحرف المشدد لا يُقلقل ما لم يتم الوقوف عليه.",
                English = @"Qalqalah (Echoing/Bouncing)
It is the vibration or echo when pronouncing the Qalqalah letter.
Its letters: ق ط ب ج د (Qaf, Ta, Ba, Jeem, Dal)
These letters are bounced when they are silent or when stopping on them.
No vowel is added to these letters; they remain silent.
A doubled letter is not bounced unless stopped upon.",
                German = @"Qalqalah (Echo/Vibration)
Es ist die Vibration oder das Echo beim Aussprechen des Qalqalah-Buchstabens.
Seine Buchstaben: ق ط ب ج د (Qaf, Ta, Ba, Dschim, Dal)
Diese Buchstaben werden vibriert, wenn sie stumm sind oder wenn man auf ihnen stoppt.
Keine Vokalzeichen werden hinzugefügt; sie bleiben stumm.
Ein verdoppelter Buchstabe wird nicht vibriert, es sei denn, man stoppt darauf.",
                Spanish = @"Qalqalah (Eco/Rebote)
Es la vibración o eco al pronunciar la letra Qalqalah.
Sus letras: ق ط ب ج د (Qaf, Ta, Ba, Yim, Dal)
Estas letras rebotan cuando están en silencio o al detenerse en ellas.
No se añade vocal a estas letras; permanecen en silencio.
Una letra duplicada no rebota a menos que se detenga en ella.",
                Turkish = @"Kalkale (Yankı/Titreşim)
Kalkale harfini telaffuz ederken oluşan titreşim veya yankıdır.
Harfleri: ق ط ب ج د (Kaf, Ta, Ba, Cim, Dal)
Bu harfler sessiz olduklarında veya üzerlerinde durulduğunda titreştirilir.
Bu harflere hiçbir hareke eklenmez; sessiz kalırlar.
Şeddeli bir harf, üzerinde durulmadıkça titreştirilmez.",
                French = @"Qalqalah (Écho/Vibration)
C'est la vibration ou l'écho lors de la prononciation de la lettre Qalqalah.
Ses lettres: ق ط ب ج د (Qaf, Ta, Ba, Jim, Dal)
Ces lettres rebondissent lorsqu'elles sont silencieuses ou lorsqu'on s'arrête dessus.
Aucune voyelle n'est ajoutée à ces lettres; elles restent silencieuses.
Une lettre doublée ne rebondit pas sauf si on s'arrête dessus.",
                Japanese = @"カルカラ（反響/振動）
カルカラ文字を発音する際の振動または反響です。
その文字: ق ط ب ج د (カーフ、ター、バー、ジーム、ダール)
これらの文字は、無音の場合または停止する場合に反響します。
これらの文字には母音は追加されず、無音のままです。
重複した文字は、停止しない限り反響しません。"
            }
        },

        // إظهار النون الساكنة والتنوين
        {
            "إظهار النون الساكنة والتنوين",
            new RuleExplanation
            {
                Arabic = @"إظهار النون الساكنة والتنوين
هو إخراج النون الساكنة أو التنوين من مخرجها بوضوح بدون غنة.
يكون الإظهار إذا جاء بعد النون الساكنة أو التنوين أحد حروف الحلق: ء هـ ع ح غ خ
تُنطق النون أو التنوين بوضوح دون إدغام أو إخفاء.",
                English = @"Izhar of Noon Sakinah and Tanween (Clear Pronunciation)
It is pronouncing the Noon Sakinah or Tanween clearly from its articulation point without nasalization.
Izhar occurs when one of the throat letters follows Noon Sakinah or Tanween: ء هـ ع ح غ خ
The Noon or Tanween is pronounced clearly without merging or hiding.",
                German = @"Izhar von Noon Sakinah und Tanween (Deutliche Aussprache)
Es ist die deutliche Aussprache des stummen Nun oder Tanwin von seinem Artikulationspunkt ohne Nasalierung.
Izhar tritt auf, wenn einer der Halsbuchstaben nach Noon Sakinah oder Tanween folgt: ء هـ ع ح غ خ
Das Nun oder Tanwin wird deutlich ausgesprochen, ohne Verschmelzung oder Verbergung.",
                Spanish = @"Izhar de Noon Sakinah y Tanween (Pronunciación Clara)
Es pronunciar el Noon Sakinah o Tanween claramente desde su punto de articulación sin nasalización.
Izhar ocurre cuando una de las letras de garganta sigue a Noon Sakinah o Tanween: ء هـ ع ح غ خ
El Noon o Tanween se pronuncia claramente sin fusión u ocultación.",
                Turkish = @"Noon Sakin ve Tenvin'in İzharı (Açık Telaffuz)
Noon Sakin veya Tenvin'i çıkış noktasından nazalizasyon olmadan açıkça telaffuz etmektir.
İzhar, Noon Sakin veya Tenvin'i boğaz harflerinden biri takip ettiğinde gerçekleşir: ء هـ ع ح غ خ
Nun veya Tenvin birleşme veya gizleme olmadan açıkça telaffuz edilir.",
                French = @"Izhar de Noon Sakinah et Tanween (Prononciation Claire)
C'est prononcer le Noon Sakinah ou Tanween clairement depuis son point d'articulation sans nasalisation.
Izhar se produit lorsqu'une des lettres de gorge suit Noon Sakinah ou Tanween: ء هـ ع ح غ خ
Le Noon ou Tanween est prononcé clairement sans fusion ni dissimulation.",
                Japanese = @"ヌーン・サーキナとタンウィーンのイズハール（明瞭な発音）
ヌーン・サーキナまたはタンウィーンを鼻音化せずに調音点から明確に発音することです。
イズハールは、喉の文字のいずれかがヌーン・サーキナまたはタンウィーンに続く場合に発生します: ء هـ ع ح غ خ
ヌーンまたはタンウィーンは、融合や隠蔽なしに明確に発音されます。"
            }
        },

        // إقلاب النون الساكنة والتنوين
        {
            "إقلاب النون الساكنة والتنوين",
            new RuleExplanation
            {
                Arabic = @"إقلاب النون الساكنة والتنوين
هو قلب النون الساكنة أو التنوين ميماً مُخفاة عند الباء.
يكون الإقلاب إذا جاء بعد النون الساكنة أو التنوين حرف الباء.
تُقلب النون أو التنوين إلى ميم مع غنة بمقدار حركتين.
علامته في المصحف: ميم صغيرة فوق النون.",
                English = @"Iqlab of Noon Sakinah and Tanween (Conversion)
It is converting the Noon Sakinah or Tanween into a hidden Meem when followed by Ba.
Iqlab occurs when the letter Ba follows Noon Sakinah or Tanween.
The Noon or Tanween is converted to Meem with nasalization for two counts.
Its sign in the Mushaf: a small Meem above the Noon.",
                German = @"Iqlab von Noon Sakinah und Tanween (Umwandlung)
Es ist die Umwandlung des stummen Nun oder Tanwin in ein verborgenes Mim, wenn Ba folgt.
Iqlab tritt auf, wenn der Buchstabe Ba nach Noon Sakinah oder Tanween folgt.
Das Nun oder Tanwin wird in Mim mit Nasalierung für zwei Zählungen umgewandelt.
Sein Zeichen im Mushaf: ein kleines Mim über dem Nun.",
                Spanish = @"Iqlab de Noon Sakinah y Tanween (Conversión)
Es convertir el Noon Sakinah o Tanween en un Meem oculto cuando es seguido por Ba.
Iqlab ocurre cuando la letra Ba sigue a Noon Sakinah o Tanween.
El Noon o Tanween se convierte en Meem con nasalización por dos tiempos.
Su signo en el Mushaf: un pequeño Meem sobre el Noon.",
                Turkish = @"Noon Sakin ve Tenvin'in İklabı (Dönüştürme)
Noon Sakin veya Tenvin'i Ba harfi takip ettiğinde gizli Mim'e dönüştürmektir.
İklab, Ba harfi Noon Sakin veya Tenvin'i takip ettiğinde gerçekleşir.
Nun veya Tenvin iki sayım için nazalizasyonla Mim'e dönüştürülür.
Mushaf'taki işareti: Nun'un üzerinde küçük bir Mim.",
                French = @"Iqlab de Noon Sakinah et Tanween (Conversion)
C'est convertir le Noon Sakinah ou Tanween en un Meem caché lorsqu'il est suivi de Ba.
Iqlab se produit lorsque la lettre Ba suit Noon Sakinah ou Tanween.
Le Noon ou Tanween est converti en Meem avec nasalisation pour deux temps.
Son signe dans le Mushaf: un petit Meem au-dessus du Noon.",
                Japanese = @"ヌーン・サーキナとタンウィーンのイクラーブ（変換）
バーが続く場合、ヌーン・サーキナまたはタンウィーンを隠れたミームに変換することです。
イクラーブは、文字バーがヌーン・サーキナまたはタンウィーンに続く場合に発生します。
ヌーンまたはタンウィーンは、2拍の鼻音化を伴ってミームに変換されます。
ムスハフでの記号：ヌーンの上の小さなミーム。"
            }
        },

        // إدغام النون الساكنة والتنوين - بغنة
        {
            "إدغام النون الساكنة والتنوين - بغنة",
            new RuleExplanation
            {
                Arabic = @"إدغام النون الساكنة والتنوين بغنة
هو إدخال النون الساكنة أو التنوين في الحرف التالي مع غنة.
يكون إذا جاء بعد النون الساكنة أو التنوين أحد حروف: ي ن م و (ينمو)
الغنة بمقدار حركتين.
استثناءات: صنوان، قنوان، الدنيا، بنيان (لا إدغام فيها).",
                English = @"Idgham with Ghunnah of Noon Sakinah and Tanween (Merging with Nasalization)
It is merging the Noon Sakinah or Tanween into the following letter with nasalization.
It occurs when one of the letters: ي ن م و (Ya, Noon, Meem, Waw) follows Noon Sakinah or Tanween.
The nasalization lasts for two counts.
Exceptions: صنوان، قنوان، الدنيا، بنيان (no merging in these words).",
                German = @"Idgham mit Ghunnah von Noon Sakinah und Tanween (Verschmelzung mit Nasalierung)
Es ist die Verschmelzung des stummen Nun oder Tanwin in den folgenden Buchstaben mit Nasalierung.
Es tritt auf, wenn einer der Buchstaben: ي ن م و (Ya, Nun, Mim, Waw) nach Noon Sakinah oder Tanween folgt.
Die Nasalierung dauert zwei Zählungen.
Ausnahmen: صنوان، قنوان، الدنيا، بنيان (keine Verschmelzung in diesen Wörtern).",
                Spanish = @"Idgham con Ghunnah de Noon Sakinah y Tanween (Fusión con Nasalización)
Es fusionar el Noon Sakinah o Tanween en la letra siguiente con nasalización.
Ocurre cuando una de las letras: ي ن م و (Ya, Noon, Meem, Waw) sigue a Noon Sakinah o Tanween.
La nasalización dura dos tiempos.
Excepciones: صنوان، قنوان، الدنيا، بنيان (sin fusión en estas palabras).",
                Turkish = @"Noon Sakin ve Tenvin'in Gunne ile İdgamı (Nazalizasyonla Birleşme)
Noon Sakin veya Tenvin'i nazalizasyonla sonraki harfe birleştirmektir.
Şu harflerden biri Noon Sakin veya Tenvin'i takip ettiğinde gerçekleşir: ي ن م و (Ya, Nun, Mim, Vav)
Nazalizasyon iki sayım sürer.
İstisnalar: صنوان، قنوان، الدنيا، بنيان (bu kelimelerde birleşme yoktur).",
                French = @"Idgham avec Ghunnah de Noon Sakinah et Tanween (Fusion avec Nasalisation)
C'est fusionner le Noon Sakinah ou Tanween dans la lettre suivante avec nasalisation.
Cela se produit lorsqu'une des lettres: ي ن م و (Ya, Noon, Meem, Waw) suit Noon Sakinah ou Tanween.
La nasalisation dure deux temps.
Exceptions: صنوان، قنوان، الدنيا، بنيان (pas de fusion dans ces mots).",
                Japanese = @"ヌーン・サーキナとタンウィーンのグンナを伴うイドガーム（鼻音化を伴う融合）
ヌーン・サーキナまたはタンウィーンを鼻音化を伴って次の文字に融合させることです。
次の文字のいずれかがヌーン・サーキナまたはタンウィーンに続く場合に発生します: ي ن م و (ヤー、ヌーン、ミーム、ワーウ)
鼻音化は2拍続きます。
例外: صنوان، قنوان، الدنيا، بنيان（これらの単語では融合なし）。"
            }
        },

        // إدغام النون الساكنة والتنوين - بدون غنة
        {
            "إدغام النون الساكنة والتنوين - بدون غنة",
            new RuleExplanation
            {
                Arabic = @"إدغام النون الساكنة والتنوين بدون غنة
هو إدخال النون الساكنة أو التنوين في الحرف التالي بدون غنة.
يكون إذا جاء بعد النون الساكنة أو التنوين حرف اللام أو الراء.
تُدغم النون أو التنوين في اللام أو الراء إدغاماً كاملاً.",
                English = @"Idgham without Ghunnah of Noon Sakinah and Tanween (Merging without Nasalization)
It is merging the Noon Sakinah or Tanween into the following letter without nasalization.
It occurs when the letter Lam or Ra follows Noon Sakinah or Tanween.
The Noon or Tanween is completely merged into Lam or Ra.",
                German = @"Idgham ohne Ghunnah von Noon Sakinah und Tanween (Verschmelzung ohne Nasalierung)
Es ist die Verschmelzung des stummen Nun oder Tanwin in den folgenden Buchstaben ohne Nasalierung.
Es tritt auf, wenn der Buchstabe Lam oder Ra nach Noon Sakinah oder Tanween folgt.
Das Nun oder Tanwin wird vollständig in Lam oder Ra verschmolzen.",
                Spanish = @"Idgham sin Ghunnah de Noon Sakinah y Tanween (Fusión sin Nasalización)
Es fusionar el Noon Sakinah o Tanween en la letra siguiente sin nasalización.
Ocurre cuando la letra Lam o Ra sigue a Noon Sakinah o Tanween.
El Noon o Tanween se fusiona completamente en Lam o Ra.",
                Turkish = @"Noon Sakin ve Tenvin'in Gunnesiz İdgamı (Nazalizasyonsuz Birleşme)
Noon Sakin veya Tenvin'i nazalizasyon olmadan sonraki harfe birleştirmektir.
Lam veya Ra harfi Noon Sakin veya Tenvin'i takip ettiğinde gerçekleşir.
Nun veya Tenvin tamamen Lam veya Ra'ya birleştirilir.",
                French = @"Idgham sans Ghunnah de Noon Sakinah et Tanween (Fusion sans Nasalisation)
C'est fusionner le Noon Sakinah ou Tanween dans la lettre suivante sans nasalisation.
Cela se produit lorsque la lettre Lam ou Ra suit Noon Sakinah ou Tanween.
Le Noon ou Tanween est complètement fusionné dans Lam ou Ra.",
                Japanese = @"ヌーン・サーキナとタンウィーンのグンナなしイドガーム（鼻音化なし融合）
ヌーン・サーキナまたはタンウィーンを鼻音化なしで次の文字に融合させることです。
文字ラームまたはラーがヌーン・サーキナまたはタンウィーンに続く場合に発生します。
ヌーンまたはタンウィーンは完全にラームまたはラーに融合されます。"
            }
        },

        // إخفاء النون الساكنة والتنوين
        {
            "إخفاء النون الساكنة والتنوين",
            new RuleExplanation
            {
                Arabic = @"إخفاء النون الساكنة والتنوين
هو نطق النون الساكنة أو التنوين بحالة بين الإظهار والإدغام مع بقاء الغنة.
يكون إذا جاء بعد النون الساكنة أو التنوين أحد حروف الإخفاء الـ15:
ت ث ج د ذ ز س ش ص ض ط ظ ف ق ك
الغنة بمقدار حركتين.",
                English = @"Ikhfa of Noon Sakinah and Tanween (Concealment)
It is pronouncing the Noon Sakinah or Tanween in a state between Izhar and Idgham with nasalization.
It occurs when one of the 15 Ikhfa letters follows Noon Sakinah or Tanween:
ت ث ج د ذ ز س ش ص ض ط ظ ف ق ك
The nasalization lasts for two counts.",
                German = @"Ikhfa von Noon Sakinah und Tanween (Verbergung)
Es ist die Aussprache des stummen Nun oder Tanwin in einem Zustand zwischen Izhar und Idgham mit Nasalierung.
Es tritt auf, wenn einer der 15 Ikhfa-Buchstaben nach Noon Sakinah oder Tanween folgt:
ت ث ج د ذ ز س ش ص ض ط ظ ف ق ك
Die Nasalierung dauert zwei Zählungen.",
                Spanish = @"Ikhfa de Noon Sakinah y Tanween (Ocultación)
Es pronunciar el Noon Sakinah o Tanween en un estado entre Izhar e Idgham con nasalización.
Ocurre cuando una de las 15 letras Ikhfa sigue a Noon Sakinah o Tanween:
ت ث ج د ذ ز س ش ص ض ط ظ ف ق ك
La nasalización dura dos tiempos.",
                Turkish = @"Noon Sakin ve Tenvin'in İhfası (Gizleme)
Noon Sakin veya Tenvin'i nazalizasyonla İzhar ve İdgam arasında bir durumda telaffuz etmektir.
15 İhfa harfinden biri Noon Sakin veya Tenvin'i takip ettiğinde gerçekleşir:
ت ث ج د ذ ز س ش ص ض ط ظ ف ق ك
Nazalizasyon iki sayım sürer.",
                French = @"Ikhfa de Noon Sakinah et Tanween (Dissimulation)
C'est prononcer le Noon Sakinah ou Tanween dans un état entre Izhar et Idgham avec nasalisation.
Cela se produit lorsqu'une des 15 lettres Ikhfa suit Noon Sakinah ou Tanween:
ت ث ج د ذ ز س ش ص ض ط ظ ف ق ك
La nasalisation dure deux temps.",
                Japanese = @"ヌーン・サーキナとタンウィーンのイフファー（隠蔽）
ヌーン・サーキナまたはタンウィーンを鼻音化を伴ってイズハールとイドガームの間の状態で発音することです。
15のイフファー文字のいずれかがヌーン・サーキナまたはタンウィーンに続く場合に発生します:
ت ث ج د ذ ز س ش ص ض ط ظ ف ق ك
鼻音化は2拍続きます。"
            }
        },

        // إخفاء الميم الساكنة
        {
            "إخفاء الميم الساكنة",
            new RuleExplanation
            {
                Arabic = @"إخفاء الميم الساكنة (الإخفاء الشفوي)
هو إخفاء الميم الساكنة عند حرف الباء مع بقاء الغنة.
يكون إذا جاء بعد الميم الساكنة حرف الباء.
تُخفى الميم مع غنة بمقدار حركتين.",
                English = @"Ikhfa of Meem Sakinah (Labial Concealment)
It is concealing the silent Meem when followed by Ba with nasalization.
It occurs when the letter Ba follows Meem Sakinah.
The Meem is concealed with nasalization for two counts.",
                German = @"Ikhfa von Meem Sakinah (Labiale Verbergung)
Es ist das Verbergen des stummen Mim, wenn Ba mit Nasalierung folgt.
Es tritt auf, wenn der Buchstabe Ba nach Meem Sakinah folgt.
Das Mim wird mit Nasalierung für zwei Zählungen verborgen.",
                Spanish = @"Ikhfa de Meem Sakinah (Ocultación Labial)
Es ocultar el Meem silencioso cuando es seguido por Ba con nasalización.
Ocurre cuando la letra Ba sigue a Meem Sakinah.
El Meem se oculta con nasalización por dos tiempos.",
                Turkish = @"Mim Sakin'in İhfası (Dudak Gizlemesi)
Ba harfi takip ettiğinde sessiz Mim'i nazalizasyonla gizlemektir.
Ba harfi Mim Sakin'i takip ettiğinde gerçekleşir.
Mim iki sayım için nazalizasyonla gizlenir.",
                French = @"Ikhfa de Meem Sakinah (Dissimulation Labiale)
C'est dissimuler le Meem silencieux lorsqu'il est suivi de Ba avec nasalisation.
Cela se produit lorsque la lettre Ba suit Meem Sakinah.
Le Meem est dissimulé avec nasalisation pour deux temps.",
                Japanese = @"ミーム・サーキナのイフファー（唇音の隠蔽）
バーが続く場合、無音のミームを鼻音化を伴って隠すことです。
文字バーがミーム・サーキナに続く場合に発生します。
ミームは2拍の鼻音化を伴って隠されます。"
            }
        },

        // إدغام الميم الساكنة
        {
            "إدغام الميم الساكنة",
            new RuleExplanation
            {
                Arabic = @"إدغام الميم الساكنة (إدغام مثلين صغير)
هو إدغام الميم الساكنة في ميم متحركة.
يكون إذا جاء بعد الميم الساكنة ميم أخرى.
تُدغم الميمان معاً مع غنة بمقدار حركتين.",
                English = @"Idgham of Meem Sakinah (Small Same-Letter Merging)
It is merging the silent Meem into a voweled Meem.
It occurs when another Meem follows Meem Sakinah.
Both Meems merge together with nasalization for two counts.",
                German = @"Idgham von Meem Sakinah (Kleine Gleichbuchstaben-Verschmelzung)
Es ist die Verschmelzung des stummen Mim in ein vokaliertes Mim.
Es tritt auf, wenn ein anderes Mim nach Meem Sakinah folgt.
Beide Mims verschmelzen zusammen mit Nasalierung für zwei Zählungen.",
                Spanish = @"Idgham de Meem Sakinah (Fusión de Letras Iguales Pequeña)
Es fusionar el Meem silencioso en un Meem con vocal.
Ocurre cuando otro Meem sigue a Meem Sakinah.
Ambos Meems se fusionan juntos con nasalización por dos tiempos.",
                Turkish = @"Mim Sakin'in İdgamı (Küçük Aynı Harf Birleşmesi)
Sessiz Mim'i harekeli Mim'e birleştirmektir.
Başka bir Mim, Mim Sakin'i takip ettiğinde gerçekleşir.
Her iki Mim iki sayım için nazalizasyonla birleşir.",
                French = @"Idgham de Meem Sakinah (Petite Fusion de Lettres Identiques)
C'est fusionner le Meem silencieux dans un Meem voyellé.
Cela se produit lorsqu'un autre Meem suit Meem Sakinah.
Les deux Meems fusionnent ensemble avec nasalisation pour deux temps.",
                Japanese = @"ミーム・サーキナのイドガーム（小さな同文字融合）
無音のミームを母音付きミームに融合させることです。
別のミームがミーム・サーキナに続く場合に発生します。
両方のミームは2拍の鼻音化を伴って融合します。"
            }
        },

        // إظهار الميم الساكنة
        {
            "إظهار الميم الساكنة",
            new RuleExplanation
            {
                Arabic = @"إظهار الميم الساكنة (الإظهار الشفوي)
هو إظهار الميم الساكنة عند جميع الحروف ما عدا الباء والميم.
تُنطق الميم بوضوح من مخرجها.
يجب الحذر من إخفائها عند الواو والفاء.",
                English = @"Izhar of Meem Sakinah (Labial Clear Pronunciation)
It is clearly pronouncing the silent Meem before all letters except Ba and Meem.
The Meem is pronounced clearly from its articulation point.
Care must be taken not to conceal it before Waw and Fa.",
                German = @"Izhar von Meem Sakinah (Labiale Deutliche Aussprache)
Es ist die deutliche Aussprache des stummen Mim vor allen Buchstaben außer Ba und Mim.
Das Mim wird deutlich von seinem Artikulationspunkt ausgesprochen.
Man muss aufpassen, es nicht vor Waw und Fa zu verbergen.",
                Spanish = @"Izhar de Meem Sakinah (Pronunciación Clara Labial)
Es pronunciar claramente el Meem silencioso antes de todas las letras excepto Ba y Meem.
El Meem se pronuncia claramente desde su punto de articulación.
Se debe tener cuidado de no ocultarlo antes de Waw y Fa.",
                Turkish = @"Mim Sakin'in İzharı (Dudak Açık Telaffuzu)
Sessiz Mim'i Ba ve Mim dışındaki tüm harflerden önce açıkça telaffuz etmektir.
Mim çıkış noktasından açıkça telaffuz edilir.
Vav ve Fa'dan önce gizlememek için dikkatli olunmalıdır.",
                French = @"Izhar de Meem Sakinah (Prononciation Claire Labiale)
C'est prononcer clairement le Meem silencieux avant toutes les lettres sauf Ba et Meem.
Le Meem est prononcé clairement depuis son point d'articulation.
Il faut faire attention à ne pas le dissimuler avant Waw et Fa.",
                Japanese = @"ミーム・サーキナのイズハール（唇音の明瞭な発音）
バーとミーム以外のすべての文字の前で無音のミームを明確に発音することです。
ミームは調音点から明確に発音されます。
ワーウとファーの前でそれを隠さないように注意する必要があります。"
            }
        },

        // أحكام النون والميم المشددتين
        {
            "أحكام النون والميم المشددتين",
            new RuleExplanation
            {
                Arabic = @"أحكام النون والميم المشددتين
النون المشددة (نّ) والميم المشددة (مّ) يجب إظهار الغنة فيهما.
الغنة بمقدار حركتين.
تُسمى غنة مشددة.",
                English = @"Rules of Stressed Noon and Meem (Noon and Meem Mushaddadah)
The doubled Noon (نّ) and doubled Meem (مّ) require clear nasalization.
The nasalization lasts for two counts.
It is called stressed Ghunnah.",
                German = @"Regeln für betontes Nun und Mim (Nun und Mim Mushaddadah)
Das verdoppelte Nun (نّ) und verdoppelte Mim (مّ) erfordern deutliche Nasalierung.
Die Nasalierung dauert zwei Zählungen.
Es wird betonte Ghunnah genannt.",
                Spanish = @"Reglas de Noon y Meem Acentuadas (Noon y Meem Mushaddadah)
El Noon duplicado (نّ) y el Meem duplicado (مّ) requieren nasalización clara.
La nasalización dura dos tiempos.
Se llama Ghunnah acentuada.",
                Turkish = @"Şeddeli Nun ve Mim Kuralları (Nun ve Mim Müşeddede)
İkili Nun (نّ) ve ikili Mim (مّ) açık nazalizasyon gerektirir.
Nazalizasyon iki sayım sürer.
Şeddeli Gunne olarak adlandırılır.",
                French = @"Règles du Noon et Meem Accentués (Noon et Meem Mushaddadah)
Le Noon doublé (نّ) et le Meem doublé (مّ) nécessitent une nasalisation claire.
La nasalisation dure deux temps.
On l'appelle Ghunnah accentuée.",
                Japanese = @"強調されたヌーンとミームの規則（ヌーンとミーム・ムシャッダダ）
重複したヌーン（نّ）と重複したミーム（مّ）は明確な鼻音化を必要とします。
鼻音化は2拍続きます。
強調されたグンナと呼ばれます。"
            }
        },

        // المد الطبيعي
        {
            "المد الطبيعي",
            new RuleExplanation
            {
                Arabic = @"المد الطبيعي (المد الأصلي)
هو المد الذي لا يتوقف على سبب.
حروف المد: الألف الساكنة بعد فتح، الواو الساكنة بعد ضم، الياء الساكنة بعد كسر.
يُمد بمقدار حركتين فقط.
لا يتغير مقداره في الوصل أو الوقف.",
                English = @"Natural Prolongation (Madd Tabee'i)
It is the prolongation that does not depend on a cause.
Prolongation letters: silent Alif after Fathah, silent Waw after Dammah, silent Ya after Kasrah.
It is prolonged for exactly two counts.
Its duration does not change whether connecting or stopping.",
                German = @"Natürliche Verlängerung (Madd Tabii)
Es ist die Verlängerung, die nicht von einer Ursache abhängt.
Verlängerungsbuchstaben: stilles Alif nach Fathah, stilles Waw nach Dammah, stilles Ya nach Kasrah.
Es wird für genau zwei Zählungen verlängert.
Seine Dauer ändert sich nicht, ob man verbindet oder stoppt.",
                Spanish = @"Prolongación Natural (Madd Tabii)
Es la prolongación que no depende de una causa.
Letras de prolongación: Alif silencioso después de Fathah, Waw silencioso después de Dammah, Ya silencioso después de Kasrah.
Se prolonga exactamente por dos tiempos.
Su duración no cambia al conectar o detenerse.",
                Turkish = @"Doğal Uzatma (Med Tabii)
Bir sebebe bağlı olmayan uzatmadır.
Uzatma harfleri: Fethadan sonra sessiz Elif, Dammeden sonra sessiz Vav, Kesreden sonra sessiz Ya.
Tam olarak iki sayım uzatılır.
Süresi bağlama veya durdurma durumunda değişmez.",
                French = @"Prolongation Naturelle (Madd Tabii)
C'est la prolongation qui ne dépend pas d'une cause.
Lettres de prolongation: Alif silencieux après Fathah, Waw silencieux après Dammah, Ya silencieux après Kasrah.
Elle est prolongée exactement pour deux temps.
Sa durée ne change pas que l'on connecte ou s'arrête.",
                Japanese = @"自然な延長（マッド・タビーイー）
原因に依存しない延長です。
延長文字：ファトハの後の無音アリフ、ダンマの後の無音ワーウ、カスラの後の無音ヤー。
正確に2拍延長されます。
接続または停止のいずれでも持続時間は変わりません。"
            }
        },

        // المد المنفصل
        {
            "المد المنفصل",
            new RuleExplanation
            {
                Arabic = @"المد المنفصل (المد الجائز)
عندما يأتي حرف المد في آخر كلمة وتبدأ الكلمة التالية له بهمزة قطع.
مثال: ""بمَا أنزل"" - ""قالُوا ءامنا"" - ""وفِي أنفسكم""
يجوز مده أو قصره.
يُمد 4 أو 5 حركات.
ملاحظة: يا النداء وها التنبيه تُكتب في المصحف محذوفة الألف وموصولة بما بعدها ولكن حكمها حكم المد المنفصل.",
                English = @"Separated Prolongation (Madd Munfasil - Permissible Prolongation)
When the prolongation letter comes at the end of a word and the following word begins with Hamzah.
Examples: ""بمَا أنزل"" - ""قالُوا ءامنا"" - ""وفِي أنفسكم""
It is permissible to prolong or shorten it.
It is prolonged for 4 or 5 counts.
Note: Ya of calling and Ha of alerting are written connected in the Mushaf but follow the rule of Madd Munfasil.",
                German = @"Getrennte Verlängerung (Madd Munfasil - Erlaubte Verlängerung)
Wenn der Verlängerungsbuchstabe am Ende eines Wortes steht und das folgende Wort mit Hamzah beginnt.
Beispiele: ""بمَا أنزل"" - ""قالُوا ءامنا"" - ""وفِي أنفسكم""
Es ist erlaubt, es zu verlängern oder zu verkürzen.
Es wird für 4 oder 5 Zählungen verlängert.
Hinweis: Ya des Rufens und Ha der Warnung werden im Mushaf verbunden geschrieben, folgen aber der Regel von Madd Munfasil.",
                Spanish = @"Prolongación Separada (Madd Munfasil - Prolongación Permisible)
Cuando la letra de prolongación viene al final de una palabra y la siguiente palabra comienza con Hamzah.
Ejemplos: ""بمَا أنزل"" - ""قالُوا ءامنا"" - ""وفِي أنفسكم""
Es permisible prolongarla o acortarla.
Se prolonga por 4 o 5 tiempos.
Nota: Ya de llamada y Ha de alerta se escriben conectadas en el Mushaf pero siguen la regla de Madd Munfasil.",
                Turkish = @"Ayrı Uzatma (Med Munfasıl - İzin Verilen Uzatma)
Uzatma harfi bir kelimenin sonunda geldiğinde ve sonraki kelime Hemze ile başladığında.
Örnekler: ""بمَا أنزل"" - ""قالُوا ءامنا"" - ""وفِي أنفسكم""
Uzatmak veya kısaltmak caizdir.
4 veya 5 sayım uzatılır.
Not: Çağrı Ya'sı ve uyarı Ha'sı Mushaf'ta bağlı yazılır ancak Med Munfasıl kuralını takip eder.",
                French = @"Prolongation Séparée (Madd Munfasil - Prolongation Permissible)
Lorsque la lettre de prolongation vient à la fin d'un mot et que le mot suivant commence par Hamzah.
Exemples: ""بمَا أنزل"" - ""قالُوا ءامنا"" - ""وفِي أنفسكم""
Il est permis de la prolonger ou de la raccourcir.
Elle est prolongée pour 4 ou 5 temps.
Note: Ya d'appel et Ha d'alerte sont écrits connectés dans le Mushaf mais suivent la règle de Madd Munfasil.",
                Japanese = @"分離延長（マッド・ムンファスィル - 許容される延長）
延長文字が単語の最後に来て、次の単語がハムザで始まる場合。
例：""بمَا أنزل"" - ""قالُوا ءامنا"" - ""وفِي أنفسكم""
延長または短縮することが許可されています。
4または5拍延長されます。
注：呼びかけのヤーと警告のハーはムスハフでは接続して書かれますが、マッド・ムンファスィルの規則に従います。"
            }
        },

        // المد المتصل
        {
            "المد المتصل",
            new RuleExplanation
            {
                Arabic = @"المد المتصل (المد الواجب)
عندما يأتي حرف المد وبعده همزة في نفس الكلمة.
مثال: ""جَاءكم"" - ""سُوء"" - ""سِيء""
يُمد 4 أو 5 حركات وجوباً.
ملاحظة: ""ها"" في ""هَاؤم"" من أصل الكلمة فتُمد مد المتصل الواجب.",
                English = @"Connected Prolongation (Madd Muttasil - Obligatory Prolongation)
When the prolongation letter is followed by Hamzah in the same word.
Examples: ""جَاءكم"" - ""سُوء"" - ""سِيء""
It must be prolonged for 4 or 5 counts.
Note: ""Ha"" in ""هَاؤم"" is from the root of the word, so it follows Madd Muttasil.",
                German = @"Verbundene Verlängerung (Madd Muttasil - Obligatorische Verlängerung)
Wenn dem Verlängerungsbuchstaben Hamzah im selben Wort folgt.
Beispiele: ""جَاءكم"" - ""سُوء"" - ""سِيء""
Es muss für 4 oder 5 Zählungen verlängert werden.
Hinweis: ""Ha"" in ""هَاؤم"" ist aus der Wurzel des Wortes, daher folgt es Madd Muttasil.",
                Spanish = @"Prolongación Conectada (Madd Muttasil - Prolongación Obligatoria)
Cuando la letra de prolongación es seguida por Hamzah en la misma palabra.
Ejemplos: ""جَاءكم"" - ""سُوء"" - ""سِيء""
Debe prolongarse por 4 o 5 tiempos.
Nota: ""Ha"" en ""هَاؤم"" es de la raíz de la palabra, por lo que sigue Madd Muttasil.",
                Turkish = @"Bağlı Uzatma (Med Muttasıl - Zorunlu Uzatma)
Uzatma harfini aynı kelimede Hemze takip ettiğinde.
Örnekler: ""جَاءكم"" - ""سُوء"" - ""سِيء""
4 veya 5 sayım uzatılmalıdır.
Not: ""هَاؤم"" içindeki ""Ha"" kelimenin kökündendir, bu nedenle Med Muttasıl'i takip eder.",
                French = @"Prolongation Connectée (Madd Muttasil - Prolongation Obligatoire)
Lorsque la lettre de prolongation est suivie de Hamzah dans le même mot.
Exemples: ""جَاءكم"" - ""سُوء"" - ""سِيء""
Elle doit être prolongée pour 4 ou 5 temps.
Note: ""Ha"" dans ""هَاؤم"" vient de la racine du mot, donc il suit Madd Muttasil.",
                Japanese = @"接続延長（マッド・ムッタスィル - 義務的延長）
延長文字の後に同じ単語内でハムザが続く場合。
例：""جَاءكم"" - ""سُوء"" - ""سِيء""
4または5拍延長する必要があります。
注：""هَاؤم""の""ハー""は単語の語根からのものであるため、マッド・ムッタスィルに従います。"
            }
        },

        // المد اللازم المخفف الكلمي
        {
            "المد اللازم المخفف الكلمي",
            new RuleExplanation
            {
                Arabic = @"المد اللازم المخفف الكلمي
هو أن يأتي حرف ساكن سكوناً أصلياً (وصلاً ووقفاً) بعد حرف المد في كلمة.
الحرف الساكن ليس جزءاً من حرف مشدد.
يُمد بمقدار 6 حركات.
مثال: ""ءَالْـَٔـٰن"" (في موضعين في سورة يونس).",
                English = @"Light Obligatory Prolongation in Words (Madd Lazim Mukhaffaf Kalimi)
When a permanently silent letter (in connection and stopping) follows the prolongation letter in a word.
The silent letter is not part of a doubled letter.
It is prolonged for 6 counts.
Example: ""ءَالْـَٔـٰن"" (in two places in Surah Yunus).",
                German = @"Leichte notwendige Verlängerung im Wort (Madd Lazim Mukhaffaf Kalimi)
Wenn ein permanent stummer Buchstabe (beim Verbinden und Stoppen) dem Verlängerungsbuchstaben in einem Wort folgt.
Der stumme Buchstabe ist nicht Teil eines verdoppelten Buchstabens.
Es wird für 6 Zählungen verlängert.
Beispiel: ""ءَالْـَٔـٰن"" (an zwei Stellen in Surah Yunus).",
                Spanish = @"Prolongación Obligatoria Ligera en Palabras (Madd Lazim Mukhaffaf Kalimi)
Cuando una letra permanentemente silenciosa (al conectar y detenerse) sigue a la letra de prolongación en una palabra.
La letra silenciosa no es parte de una letra duplicada.
Se prolonga por 6 tiempos.
Ejemplo: ""ءَالْـَٔـٰن"" (en dos lugares en Surah Yunus).",
                Turkish = @"Kelimedeki Hafif Zorunlu Uzatma (Med Lazım Muhaffef Kelimi)
Kalıcı olarak sessiz bir harf (bağlama ve durdurma durumunda) bir kelimede uzatma harfini takip ettiğinde.
Sessiz harf şeddeli bir harfin parçası değildir.
6 sayım uzatılır.
Örnek: ""ءَالْـَٔـٰن"" (Yunus suresinde iki yerde).",
                French = @"Prolongation Obligatoire Légère dans les Mots (Madd Lazim Mukhaffaf Kalimi)
Lorsqu'une lettre en permanence silencieuse (en connexion et arrêt) suit la lettre de prolongation dans un mot.
La lettre silencieuse ne fait pas partie d'une lettre doublée.
Elle est prolongée pour 6 temps.
Exemple: ""ءَالْـَٔـٰن"" (en deux endroits dans Sourate Yunus).",
                Japanese = @"単語内の軽い義務的延長（マッド・ラーズィム・ムハッファフ・カリミー）
恒久的に無音の文字（接続と停止の場合）が単語内の延長文字に続く場合。
無音の文字は重複した文字の一部ではありません。
6拍延長されます。
例：""ءَالْـَٔـٰن""（ユーヌス章の2箇所）。"
            }
        },

        // المد اللازم المثقل الكلمي
        {
            "المد اللازم المثقل الكلمي",
            new RuleExplanation
            {
                Arabic = @"المد اللازم المثقل الكلمي
هو أن يأتي حرف ساكن سكوناً أصلياً (وصلاً ووقفاً) بعد حرف المد في كلمة.
الحرف الساكن جزء من حرف مشدد.
يُمد بمقدار 6 حركات.
مثال: ""الحَاقَّة"" - ""الطَّامَّة"" - ""الصَّاخَّة"".",
                English = @"Heavy Obligatory Prolongation in Words (Madd Lazim Muthaqqal Kalimi)
When a permanently silent letter (in connection and stopping) follows the prolongation letter in a word.
The silent letter is part of a doubled letter.
It is prolonged for 6 counts.
Examples: ""الحَاقَّة"" - ""الطَّامَّة"" - ""الصَّاخَّة"".",
                German = @"Starke notwendige Verlängerung im Wort (Madd Lazim Muthaqqal Kalimi)
Wenn ein permanent stummer Buchstabe (beim Verbinden und Stoppen) dem Verlängerungsbuchstaben in einem Wort folgt.
Der stumme Buchstabe ist Teil eines verdoppelten Buchstabens.
Es wird für 6 Zählungen verlängert.
Beispiele: ""الحَاقَّة"" - ""الطَّامَّة"" - ""الصَّاخَّة"".",
                Spanish = @"Prolongación Obligatoria Pesada en Palabras (Madd Lazim Muthaqqal Kalimi)
Cuando una letra permanentemente silenciosa (al conectar y detenerse) sigue a la letra de prolongación en una palabra.
La letra silenciosa es parte de una letra duplicada.
Se prolonga por 6 tiempos.
Ejemplos: ""الحَاقَّة"" - ""الطَّامَّة"" - ""الصَّاخَّة"".",
                Turkish = @"Kelimedeki Ağır Zorunlu Uzatma (Med Lazım Müsekkel Kelimi)
Kalıcı olarak sessiz bir harf (bağlama ve durdurma durumunda) bir kelimede uzatma harfini takip ettiğinde.
Sessiz harf şeddeli bir harfin parçasıdır.
6 sayım uzatılır.
Örnekler: ""الحَاقَّة"" - ""الطَّامَّة"" - ""الصَّاخَّة"".",
                French = @"Prolongation Obligatoire Lourde dans les Mots (Madd Lazim Muthaqqal Kalimi)
Lorsqu'une lettre en permanence silencieuse (en connexion et arrêt) suit la lettre de prolongation dans un mot.
La lettre silencieuse fait partie d'une lettre doublée.
Elle est prolongée pour 6 temps.
Exemples: ""الحَاقَّة"" - ""الطَّامَّة"" - ""الصَّاخَّة"".",
                Japanese = @"単語内の重い義務的延長（マッド・ラーズィム・ムサッカル・カリミー）
恒久的に無音の文字（接続と停止の場合）が単語内の延長文字に続く場合。
無音の文字は重複した文字の一部です。
6拍延長されます。
例：""الحَاقَّة"" - ""الطَّامَّة"" - ""الصَّاخَّة""。"
            }
        },

        // المد اللازم المخفف الحرفي
        {
            "المد اللازم المخفف الحرفي",
            new RuleExplanation
            {
                Arabic = @"المد اللازم المخفف الحرفي
هو أن يأتي حرف ساكن سكوناً أصلياً بعد حرف المد في حرف من الحروف المقطعة.
الحرف الساكن ليس جزءاً من حرف مشدد.
يُمد بمقدار 6 حركات.
الحروف المقطعة التي فيها مد لازم: سنقص لكم (س ن ق ص ل ك م).",
                English = @"Light Obligatory Prolongation in Letters (Madd Lazim Mukhaffaf Harfi)
When a permanently silent letter follows the prolongation letter in one of the disconnected letters.
The silent letter is not part of a doubled letter.
It is prolonged for 6 counts.
Disconnected letters with this prolongation: س ن ق ص ل ك م.",
                German = @"Leichte notwendige Verlängerung im Buchstaben (Madd Lazim Mukhaffaf Harfi)
Wenn ein permanent stummer Buchstabe dem Verlängerungsbuchstaben in einem der getrennten Buchstaben folgt.
Der stumme Buchstabe ist nicht Teil eines verdoppelten Buchstabens.
Es wird für 6 Zählungen verlängert.
Getrennte Buchstaben mit dieser Verlängerung: س ن ق ص ل ك م.",
                Spanish = @"Prolongación Obligatoria Ligera en Letras (Madd Lazim Mukhaffaf Harfi)
Cuando una letra permanentemente silenciosa sigue a la letra de prolongación en una de las letras desconectadas.
La letra silenciosa no es parte de una letra duplicada.
Se prolonga por 6 tiempos.
Letras desconectadas con esta prolongación: س ن ق ص ل ك م.",
                Turkish = @"Harfteki Hafif Zorunlu Uzatma (Med Lazım Muhaffef Harfi)
Kalıcı olarak sessiz bir harf, ayrık harflerden birinde uzatma harfini takip ettiğinde.
Sessiz harf şeddeli bir harfin parçası değildir.
6 sayım uzatılır.
Bu uzatmaya sahip ayrık harfler: س ن ق ص ل ك م.",
                French = @"Prolongation Obligatoire Légère dans les Lettres (Madd Lazim Mukhaffaf Harfi)
Lorsqu'une lettre en permanence silencieuse suit la lettre de prolongation dans l'une des lettres déconnectées.
La lettre silencieuse ne fait pas partie d'une lettre doublée.
Elle est prolongée pour 6 temps.
Lettres déconnectées avec cette prolongation: س ن ق ص ل ك م.",
                Japanese = @"文字内の軽い義務的延長（マッド・ラーズィム・ムハッファフ・ハルフィー）
恒久的に無音の文字が、分離文字の1つで延長文字に続く場合。
無音の文字は重複した文字の一部ではありません。
6拍延長されます。
この延長を持つ分離文字：س ن ق ص ل ك م。"
            }
        },

        // المد اللازم المثقل الحرفي
        {
            "المد اللازم المثقل الحرفي",
            new RuleExplanation
            {
                Arabic = @"المد اللازم المثقل الحرفي
هو أن يأتي حرف ساكن سكوناً أصلياً بعد حرف المد في حرف من الحروف المقطعة.
الحرف الساكن جزء من حرف مشدد (بسبب إدغام).
يُمد بمقدار 6 حركات.
مثال: ""الم"" - اللام تُدغم في الميم.",
                English = @"Heavy Obligatory Prolongation in Letters (Madd Lazim Muthaqqal Harfi)
When a permanently silent letter follows the prolongation letter in one of the disconnected letters.
The silent letter is part of a doubled letter (due to merging).
It is prolonged for 6 counts.
Example: ""الم"" - the Lam merges into the Meem.",
                German = @"Starke notwendige Verlängerung im Buchstaben (Madd Lazim Muthaqqal Harfi)
Wenn ein permanent stummer Buchstabe dem Verlängerungsbuchstaben in einem der getrennten Buchstaben folgt.
Der stumme Buchstabe ist Teil eines verdoppelten Buchstabens (wegen Verschmelzung).
Es wird für 6 Zählungen verlängert.
Beispiel: ""الم"" - das Lam verschmilzt mit dem Mim.",
                Spanish = @"Prolongación Obligatoria Pesada en Letras (Madd Lazim Muthaqqal Harfi)
Cuando una letra permanentemente silenciosa sigue a la letra de prolongación en una de las letras desconectadas.
La letra silenciosa es parte de una letra duplicada (debido a fusión).
Se prolonga por 6 tiempos.
Ejemplo: ""الم"" - el Lam se fusiona en el Meem.",
                Turkish = @"Harfteki Ağır Zorunlu Uzatma (Med Lazım Müsekkel Harfi)
Kalıcı olarak sessiz bir harf, ayrık harflerden birinde uzatma harfini takip ettiğinde.
Sessiz harf şeddeli bir harfin parçasıdır (birleşme nedeniyle).
6 sayım uzatılır.
Örnek: ""الم"" - Lam, Mim'e birleşir.",
                French = @"Prolongation Obligatoire Lourde dans les Lettres (Madd Lazim Muthaqqal Harfi)
Lorsqu'une lettre en permanence silencieuse suit la lettre de prolongation dans l'une des lettres déconnectées.
La lettre silencieuse fait partie d'une lettre doublée (en raison de la fusion).
Elle est prolongée pour 6 temps.
Exemple: ""الم"" - le Lam fusionne dans le Meem.",
                Japanese = @"文字内の重い義務的延長（マッド・ラーズィム・ムサッカル・ハルフィー）
恒久的に無音の文字が、分離文字の1つで延長文字に続く場合。
無音の文字は重複した文字の一部です（融合による）。
6拍延長されます。
例：""الم"" - ラームがミームに融合します。"
            }
        },

        // مد اللين
        {
            "مد اللين",
            new RuleExplanation
            {
                Arabic = @"مد اللين
أن يأتي بعد حرف اللين (الواو والياء الساكنتان المفتوح ما قبلهما) حرف ساكن سكوناً عرضياً بسبب الوقف.
يُمد بمقدار 2 أو 4 أو 6 حركات.
يلتزم القارئ بمقدار المد طوال القراءة.
يجب أن يكون مد اللين مساوياً أو أقل من المد العارض للسكون.
مثال: ""خَوْف"" - ""بَيْت"".",
                English = @"Soft Prolongation (Madd Leen)
When a temporarily silent letter (due to stopping) follows a soft letter (silent Waw or Ya preceded by Fathah).
It is prolonged for 2, 4, or 6 counts.
The reader must maintain the same prolongation duration throughout the recitation.
Madd Leen must be equal to or less than Madd 'Arid Lissukoon.
Examples: ""خَوْف"" - ""بَيْت"".",
                German = @"Weiche Verlängerung (Madd Leen)
Wenn ein vorübergehend stummer Buchstabe (wegen Stopp) einem weichen Buchstaben folgt (stummes Waw oder Ya mit Fathah davor).
Es wird für 2, 4 oder 6 Zählungen verlängert.
Der Leser muss die gleiche Verlängerungsdauer während der gesamten Rezitation beibehalten.
Madd Leen muss gleich oder kürzer als Madd 'Arid Lissukoon sein.
Beispiele: ""خَوْف"" - ""بَيْت"".",
                Spanish = @"Prolongación Suave (Madd Leen)
Cuando una letra temporalmente silenciosa (debido a detenerse) sigue a una letra suave (Waw o Ya silencioso precedido por Fathah).
Se prolonga por 2, 4 o 6 tiempos.
El lector debe mantener la misma duración de prolongación durante toda la recitación.
Madd Leen debe ser igual o menor que Madd 'Arid Lissukoon.
Ejemplos: ""خَوْف"" - ""بَيْت"".",
                Turkish = @"Yumuşak Uzatma (Med Leyn)
Geçici olarak sessiz bir harf (durma nedeniyle) yumuşak bir harfi takip ettiğinde (önünde Fetha olan sessiz Vav veya Ya).
2, 4 veya 6 sayım uzatılır.
Okuyucu tüm okuma boyunca aynı uzatma süresini korumalıdır.
Med Leyn, Med Arız Lis-sukun'a eşit veya daha kısa olmalıdır.
Örnekler: ""خَوْف"" - ""بَيْت"".",
                French = @"Prolongation Douce (Madd Leen)
Lorsqu'une lettre temporairement silencieuse (en raison de l'arrêt) suit une lettre douce (Waw ou Ya silencieux précédé de Fathah).
Elle est prolongée pour 2, 4 ou 6 temps.
Le lecteur doit maintenir la même durée de prolongation tout au long de la récitation.
Madd Leen doit être égal ou inférieur à Madd 'Arid Lissukoon.
Exemples: ""خَوْف"" - ""بَيْت"".",
                Japanese = @"柔らかい延長（マッド・リーン）
一時的に無音の文字（停止による）が柔らかい文字（ファトハが前にある無音ワーウまたはヤー）に続く場合。
2、4、または6拍延長されます。
読者は朗読全体を通して同じ延長時間を維持する必要があります。
マッド・リーンはマッド・アーリド・リッスクーンと同じかそれより短くなければなりません。
例：""خَوْف"" - ""بَيْت""。"
            }
        },

        // مد البدل
        {
            "مد البدل",
            new RuleExplanation
            {
                Arabic = @"مد البدل
هو أن تتقدم الهمزة على حرف المد في كلمة واحدة.
أصله همزتان: الأولى متحركة والثانية ساكنة، فتُبدل الثانية حرف مد.
يُمد بمقدار حركتين (كالمد الطبيعي).
مثال: ""ءَامَنُوا"" - ""إِيمَان"" - ""أُوتُوا"".",
                English = @"Replacement Prolongation (Madd Badal)
When the Hamzah precedes the prolongation letter in the same word.
Originally two Hamzahs: the first voweled and the second silent, so the second is replaced with a prolongation letter.
It is prolonged for two counts (like natural prolongation).
Examples: ""ءَامَنُوا"" - ""إِيمَان"" - ""أُوتُوا"".",
                German = @"Ersatzverlängerung (Madd Badal)
Wenn Hamzah dem Verlängerungsbuchstaben im selben Wort vorausgeht.
Ursprünglich zwei Hamzahs: das erste vokalisiert und das zweite stumm, so wird das zweite durch einen Verlängerungsbuchstaben ersetzt.
Es wird für zwei Zählungen verlängert (wie natürliche Verlängerung).
Beispiele: ""ءَامَنُوا"" - ""إِيمَان"" - ""أُوتُوا"".",
                Spanish = @"Prolongación de Reemplazo (Madd Badal)
Cuando el Hamzah precede a la letra de prolongación en la misma palabra.
Originalmente dos Hamzahs: el primero con vocal y el segundo silencioso, por lo que el segundo se reemplaza con una letra de prolongación.
Se prolonga por dos tiempos (como prolongación natural).
Ejemplos: ""ءَامَنُوا"" - ""إِيمَان"" - ""أُوتُوا"".",
                Turkish = @"Değiştirme Uzatması (Med Bedel)
Hemze aynı kelimede uzatma harfinden önce geldiğinde.
Aslen iki Hemze: birincisi harekeli ve ikincisi sessiz, bu nedenle ikincisi uzatma harfiyle değiştirilir.
İki sayım uzatılır (doğal uzatma gibi).
Örnekler: ""ءَامَنُوا"" - ""إِيمَان"" - ""أُوتُوا"".",
                French = @"Prolongation de Remplacement (Madd Badal)
Lorsque le Hamzah précède la lettre de prolongation dans le même mot.
Originalement deux Hamzahs: le premier voyellé et le second silencieux, donc le second est remplacé par une lettre de prolongation.
Elle est prolongée pour deux temps (comme la prolongation naturelle).
Exemples: ""ءَامَنُوا"" - ""إِيمَان"" - ""أُوتُوا"".",
                Japanese = @"置換延長（マッド・バダル）
同じ単語内でハムザが延長文字の前に来る場合。
元々は2つのハムザ：最初は母音付きで2番目は無音なので、2番目は延長文字に置き換えられます。
2拍延長されます（自然な延長のように）。
例：""ءَامَنُوا"" - ""إِيمَان"" - ""أُوتُوا""。"
            }
        },

        // مد الصلة الصغرى
        {
            "مد الصلة الصغرى",
            new RuleExplanation
            {
                Arabic = @"مد الصلة الصغرى
هو وصل هاء الضمير المفرد المذكر الغائب بواو إذا كانت الهاء مضمومة، أو ياء إذا كانت مكسورة، ووقعت بين متحركين.
ليس بعد الهاء همزة قطع.
يُمد حركتين كالمد الطبيعي.
علامته: واو صغيرة أو ياء مردودة للخلف.",
                English = @"Minor Connecting Prolongation (Madd Silah Sughra)
Connecting the singular masculine pronoun Ha with Waw if the Ha has Dammah, or Ya if it has Kasrah, when it falls between two voweled letters.
No Hamzah follows the Ha.
It is prolonged for two counts like natural prolongation.
Its sign: a small Waw or a reversed Ya.",
                German = @"Kleine Verbindungsverlängerung (Madd Silah Sughra)
Verbindung des männlichen Singular-Pronomen-Ha mit Waw, wenn Ha Dammah hat, oder Ya, wenn es Kasrah hat, wenn es zwischen zwei vokalisierten Buchstaben steht.
Kein Hamzah folgt dem Ha.
Es wird für zwei Zählungen wie natürliche Verlängerung verlängert.
Sein Zeichen: ein kleines Waw oder ein umgekehrtes Ya.",
                Spanish = @"Prolongación de Conexión Menor (Madd Silah Sughra)
Conectar el pronombre masculino singular Ha con Waw si el Ha tiene Dammah, o Ya si tiene Kasrah, cuando cae entre dos letras con vocal.
Ningún Hamzah sigue al Ha.
Se prolonga por dos tiempos como prolongación natural.
Su signo: un Waw pequeño o un Ya invertido.",
                Turkish = @"Küçük Bağlantı Uzatması (Med Sıla Suğra)
Tekil eril zamir Ha'yı, Ha Damme'ye sahipse Vav ile, Kesre'ye sahipse Ya ile bağlamak, iki harekeli harfin arasına düştüğünde.
Ha'dan sonra Hemze gelmez.
Doğal uzatma gibi iki sayım uzatılır.
İşareti: küçük bir Vav veya ters çevrilmiş bir Ya.",
                French = @"Prolongation de Connexion Mineure (Madd Silah Sughra)
Connecter le pronom masculin singulier Ha avec Waw si le Ha a Dammah, ou Ya s'il a Kasrah, lorsqu'il tombe entre deux lettres voyellées.
Aucun Hamzah ne suit le Ha.
Elle est prolongée pour deux temps comme la prolongation naturelle.
Son signe: un petit Waw ou un Ya inversé.",
                Japanese = @"小さな接続延長（マッド・スィラ・スグラー）
単数男性代名詞ハーを、ハーがダンマを持つ場合はワーウと、カスラを持つ場合はヤーと接続し、2つの母音付き文字の間に来る場合。
ハーの後にハムザは続きません。
自然な延長のように2拍延長されます。
その記号：小さなワーウまたは逆さのヤー。"
            }
        },

        // مد الصلة الكبرى
        {
            "مد الصلة الكبرى",
            new RuleExplanation
            {
                Arabic = @"مد الصلة الكبرى
هو وصل هاء الضمير المفرد المذكر الغائب بواو أو ياء، ووقعت بين متحركين.
بعد الهاء همزة قطع.
يُمد 4 أو 5 حركات كالمد المنفصل.
علامته: وضع مدة فوق الواو أو الياء.",
                English = @"Major Connecting Prolongation (Madd Silah Kubra)
Connecting the singular masculine pronoun Ha with Waw or Ya when it falls between two voweled letters.
A Hamzah follows the Ha.
It is prolonged for 4 or 5 counts like Madd Munfasil.
Its sign: a Maddah mark above the Waw or Ya.",
                German = @"Große Verbindungsverlängerung (Madd Silah Kubra)
Verbindung des männlichen Singular-Pronomen-Ha mit Waw oder Ya, wenn es zwischen zwei vokalisierten Buchstaben steht.
Ein Hamzah folgt dem Ha.
Es wird für 4 oder 5 Zählungen wie Madd Munfasil verlängert.
Sein Zeichen: ein Maddah-Zeichen über dem Waw oder Ya.",
                Spanish = @"Prolongación de Conexión Mayor (Madd Silah Kubra)
Conectar el pronombre masculino singular Ha con Waw o Ya cuando cae entre dos letras con vocal.
Un Hamzah sigue al Ha.
Se prolonga por 4 o 5 tiempos como Madd Munfasil.
Su signo: una marca Maddah sobre el Waw o Ya.",
                Turkish = @"Büyük Bağlantı Uzatması (Med Sıla Kübra)
Tekil eril zamir Ha'yı Vav veya Ya ile bağlamak, iki harekeli harfin arasına düştüğünde.
Ha'dan sonra Hemze gelir.
Med Munfasıl gibi 4 veya 5 sayım uzatılır.
İşareti: Vav veya Ya'nın üzerinde Medde işareti.",
                French = @"Prolongation de Connexion Majeure (Madd Silah Kubra)
Connecter le pronom masculin singulier Ha avec Waw ou Ya lorsqu'il tombe entre deux lettres voyellées.
Un Hamzah suit le Ha.
Elle est prolongée pour 4 ou 5 temps comme Madd Munfasil.
Son signe: une marque Maddah au-dessus du Waw ou Ya.",
                Japanese = @"大きな接続延長（マッド・スィラ・クブラー）
単数男性代名詞ハーを、2つの母音付き文字の間に来る場合にワーウまたはヤーと接続します。
ハムザがハーに続きます。
マッド・ムンファスィルのように4または5拍延長されます。
その記号：ワーウまたはヤーの上のマッダ記号。"
            }
        },

        // مد العوض
        {
            "مد العوض",
            new RuleExplanation
            {
                Arabic = @"مد العوض
هو إبدال تنوين الفتح ألفاً عند الوقف.
تاء التأنيث المربوطة المنونة بالفتح تُنطق هاء ساكنة عند الوقف.
يُمد بمقدار حركتين.
مثال: ""شَجَرَةً"" ⬅ ""شَجَرَهْ"" عند الوقف.",
                English = @"Compensation Prolongation (Madd 'Iwad)
Replacing the Fathah Tanween with Alif when stopping.
The feminine Ta Marbutah with Fathah Tanween is pronounced as a silent Ha when stopping.
It is prolonged for two counts.
Example: ""شَجَرَةً"" ⬅ ""شَجَرَهْ"" when stopping.",
                German = @"Ersatzverlängerung (Madd Iwad)
Ersetzen des Fathah Tanween durch Alif beim Stoppen.
Das weibliche Ta Marbutah mit Fathah Tanween wird als stilles Ha beim Stoppen ausgesprochen.
Es wird für zwei Zählungen verlängert.
Beispiel: ""شَجَرَةً"" ⬅ ""شَجَرَهْ"" beim Stoppen.",
                Spanish = @"Prolongación de Compensación (Madd Iwad)
Reemplazar el Tanween Fathah con Alif al detenerse.
El Ta Marbutah femenino con Tanween Fathah se pronuncia como un Ha silencioso al detenerse.
Se prolonga por dos tiempos.
Ejemplo: ""شَجَرَةً"" ⬅ ""شَجَرَهْ"" al detenerse.",
                Turkish = @"Tazminat Uzatması (Med İvaz)
Dururken Fetha Tenvin'i Elif ile değiştirmek.
Fetha Tenvinli dişil Ta Merbuta dururken sessiz Ha olarak telaffuz edilir.
İki sayım uzatılır.
Örnek: ""شَجَرَةً"" ⬅ ""شَجَرَهْ"" dururken.",
                French = @"Prolongation de Compensation (Madd Iwad)
Remplacer le Tanween Fathah par Alif lors de l'arrêt.
Le Ta Marbutah féminin avec Tanween Fathah est prononcé comme un Ha silencieux lors de l'arrêt.
Elle est prolongée pour deux temps.
Exemple: ""شَجَرَةً"" ⬅ ""شَجَرَهْ"" lors de l'arrêt.",
                Japanese = @"補償延長（マッド・イワド）
停止時にファトハ・タンウィーンをアリフに置き換えます。
ファトハ・タンウィーンを持つ女性形ター・マルブータは、停止時に無音のハーとして発音されます。
2拍延長されます。
例：""شَجَرَةً"" ⬅ ""شَجَرَهْ"" 停止時。"
            }
        },

        // المد العارض للسكون
        {
            "المد العارض للسكون",
            new RuleExplanation
            {
                Arabic = @"المد العارض للسكون
أن يأتي بعد حرف المد حرف ساكن سكوناً عارضاً بسبب الوقف.
يُمد بمقدار 2 أو 4 أو 6 حركات.
يلتزم القارئ بمقدار المد طوال القراءة.
مثال: ""نَسْتَعِين"" - ""العَالَمِين"".",
                English = @"Prolongation due to Incidental Sukoon (Madd 'Arid Lissukoon)
When a temporarily silent letter (due to stopping) follows the prolongation letter.
It is prolonged for 2, 4, or 6 counts.
The reader must maintain the same prolongation duration throughout the recitation.
Examples: ""نَسْتَعِين"" - ""العَالَمِين"".",
                German = @"Verlängerung wegen vorübergehendem Sukoon (Madd Arid Lissukoon)
Wenn ein vorübergehend stummer Buchstabe (wegen Stopp) dem Verlängerungsbuchstaben folgt.
Es wird für 2, 4 oder 6 Zählungen verlängert.
Der Leser muss die gleiche Verlängerungsdauer während der gesamten Rezitation beibehalten.
Beispiele: ""نَسْتَعِين"" - ""العَالَمِين"".",
                Spanish = @"Prolongación por Sukoon Incidental (Madd Arid Lissukoon)
Cuando una letra temporalmente silenciosa (debido a detenerse) sigue a la letra de prolongación.
Se prolonga por 2, 4 o 6 tiempos.
El lector debe mantener la misma duración de prolongación durante toda la recitación.
Ejemplos: ""نَسْتَعِين"" - ""العَالَمِين"".",
                Turkish = @"Geçici Sukun Nedeniyle Uzatma (Med Arız Lis-sukun)
Geçici olarak sessiz bir harf (durma nedeniyle) uzatma harfini takip ettiğinde.
2, 4 veya 6 sayım uzatılır.
Okuyucu tüm okuma boyunca aynı uzatma süresini korumalıdır.
Örnekler: ""نَسْتَعِين"" - ""العَالَمِين"".",
                French = @"Prolongation due au Sukoon Accidentel (Madd Arid Lissukoon)
Lorsqu'une lettre temporairement silencieuse (en raison de l'arrêt) suit la lettre de prolongation.
Elle est prolongée pour 2, 4 ou 6 temps.
Le lecteur doit maintenir la même durée de prolongation tout au long de la récitation.
Exemples: ""نَسْتَعِين"" - ""العَالَمِين"".",
                Japanese = @"偶発的スクーンによる延長（マッド・アーリド・リッスクーン）
一時的に無音の文字（停止による）が延長文字に続く場合。
2、4、または6拍延長されます。
読者は朗読全体を通して同じ延長時間を維持する必要があります。
例：""نَسْتَعِين"" - ""العَالَمِين""。"
            }
        },

        // جواز الوقف مستوي الطرفين
        {
            "جواز الوقف مستوي الطرفين",
            new RuleExplanation
            {
                Arabic = @"جواز الوقف مستوي الطرفين
علامته: ج
يجوز الوقف والوصل على حد سواء.
كلا الخيارين متساويان في الجواز.",
                English = @"Permissible Stop, Both Options Equal
Its sign: ج
Both stopping and continuing are equally permissible.
Neither option is preferred over the other.",
                German = @"Erlaubter Halt, beide Optionen gleichwertig
Sein Zeichen: ج
Sowohl das Stoppen als auch das Fortsetzen sind gleichermaßen erlaubt.
Keine Option wird der anderen vorgezogen.",
                Spanish = @"Parada Permisible, Ambas Opciones Iguales
Su signo: ج
Tanto detenerse como continuar son igualmente permisibles.
Ninguna opción se prefiere sobre la otra.",
                Turkish = @"İzin Verilen Duruş, Her İki Seçenek Eşit
İşareti: ج
Hem durmak hem de devam etmek eşit derecede caizdir.
Hiçbir seçenek diğerine tercih edilmez.",
                French = @"Arrêt Permissible, Les Deux Options Égales
Son signe: ج
S'arrêter et continuer sont également permissibles.
Aucune option n'est préférée à l'autre.",
                Japanese = @"許容される停止、両方の選択肢が同等
その記号：ج
停止と継続の両方が同等に許容されます。
どちらの選択肢も他方より優先されません。"
            }
        },

        // جواز الوقف والوصل أولى
        {
            "جواز الوقف والوصل أولى",
            new RuleExplanation
            {
                Arabic = @"جواز الوقف والوصل أولى
علامته: صلي
يجوز الوقف لكن الوصل أولى.
الأفضل الاستمرار في القراءة.",
                English = @"Permissible to Stop, but Continuation is Preferable
Its sign: صلي
Stopping is permissible, but continuing is preferable.
It is better to continue reading.",
                German = @"Halt ist erlaubt, aber das Fortsetzen ist vorzuziehen
Sein Zeichen: صلي
Das Stoppen ist erlaubt, aber das Fortsetzen ist vorzuziehen.
Es ist besser, weiterzulesen.",
                Spanish = @"Permisible Detenerse, pero Continuar es Preferible
Su signo: صلي
Detenerse es permisible, pero continuar es preferible.
Es mejor continuar leyendo.",
                Turkish = @"Durmak İzin Verilir, ancak Devam Etmek Tercih Edilir
İşareti: صلي
Durmak caizdir, ancak devam etmek tercih edilir.
Okumaya devam etmek daha iyidir.",
                French = @"Permis de S'arrêter, mais Continuer est Préférable
Son signe: صلي
S'arrêter est permis, mais continuer est préférable.
Il est préférable de continuer à lire.",
                Japanese = @"停止は許容されるが、継続が望ましい
その記号：صلي
停止は許容されますが、継続が望ましいです。
読み続ける方が良いです。"
            }
        },

        // جواز الوصل والوقف أولى
        {
            "جواز الوصل والوقف أولى",
            new RuleExplanation
            {
                Arabic = @"جواز الوصل والوقف أولى
علامته: قلي
يجوز الوصل لكن الوقف أولى.
الأفضل الوقف عند هذا الموضع.",
                English = @"Permissible to Continue, but Stopping is Preferable
Its sign: قلي
Continuing is permissible, but stopping is preferable.
It is better to stop at this position.",
                German = @"Fortsetzen ist erlaubt, aber das Anhalten ist vorzuziehen
Sein Zeichen: قلي
Das Fortsetzen ist erlaubt, aber das Anhalten ist vorzuziehen.
Es ist besser, an dieser Stelle anzuhalten.",
                Spanish = @"Permisible Continuar, pero Detenerse es Preferible
Su signo: قلي
Continuar es permisible, pero detenerse es preferible.
Es mejor detenerse en esta posición.",
                Turkish = @"Devam Etmek İzin Verilir, ancak Durmak Tercih Edilir
İşareti: قلي
Devam etmek caizdir, ancak durmak tercih edilir.
Bu konumda durmak daha iyidir.",
                French = @"Permis de Continuer, mais S'arrêter est Préférable
Son signe: قلي
Continuer est permis, mais s'arrêter est préférable.
Il est préférable de s'arrêter à cette position.",
                Japanese = @"継続は許容されるが、停止が望ましい
その記号：قلي
継続は許容されますが、停止が望ましいです。
この位置で停止する方が良いです。"
            }
        },

        // الوقف الممنوع
        {
            "الوقف الممنوع",
            new RuleExplanation
            {
                Arabic = @"الوقف الممنوع
علامته: لا
الوقف على هذا الموضع يفسد المعنى.
يجب عدم الوقف إلا لضرورة كانقطاع النفس.",
                English = @"Prohibited Stop
Its sign: لا
Stopping at this position corrupts the meaning.
One must not stop except out of necessity like running out of breath.",
                German = @"Verbotener Halt
Sein Zeichen: لا
Das Stoppen an dieser Stelle verdirbt die Bedeutung.
Man darf nicht stoppen, außer aus Notwendigkeit wie Atemlosigkeit.",
                Spanish = @"Parada Prohibida
Su signo: لا
Detenerse en esta posición corrompe el significado.
No se debe detener excepto por necesidad como quedarse sin aliento.",
                Turkish = @"Yasak Duruş
İşareti: لا
Bu konumda durmak anlamı bozar.
Nefes tükenmesi gibi zorunluluk dışında durulmamalıdır.",
                French = @"Arrêt Interdit
Son signe: لا
S'arrêter à cette position corrompt le sens.
On ne doit pas s'arrêter sauf par nécessité comme manquer de souffle.",
                Japanese = @"禁止された停止
その記号：لا
この位置で停止すると意味が損なわれます。
息切れなどの必要性がある場合を除き、停止してはなりません。"
            }
        },

        // الوقف اللازم
        {
            "الوقف اللازم",
            new RuleExplanation
            {
                Arabic = @"الوقف اللازم
علامته: مـ
الوقف على هذا الموضع لازم.
إذا لم يتم الوقف فسد المعنى.
الوقوف على رؤوس الآيات سنة.",
                English = @"Mandatory Stop
Its sign: مـ
Stopping at this position is mandatory.
Not stopping would corrupt the meaning.
Stopping at the end of verses is a Sunnah.",
                German = @"Obligatorischer Halt
Sein Zeichen: مـ
Das Stoppen an dieser Stelle ist obligatorisch.
Nicht zu stoppen würde die Bedeutung verderben.
Das Stoppen am Ende von Versen ist eine Sunnah.",
                Spanish = @"Parada Obligatoria
Su signo: مـ
Detenerse en esta posición es obligatorio.
No detenerse corrompería el significado.
Detenerse al final de los versículos es una Sunnah.",
                Turkish = @"Zorunlu Duruş
İşareti: مـ
Bu konumda durmak zorunludur.
Durmamak anlamı bozar.
Ayetlerin sonunda durmak sünnettir.",
                French = @"Arrêt Obligatoire
Son signe: مـ
S'arrêter à cette position est obligatoire.
Ne pas s'arrêter corromprait le sens.
S'arrêter à la fin des versets est une Sunnah.",
                Japanese = @"義務的停止
その記号：مـ
この位置で停止することは義務です。
停止しないと意味が損なわれます。
節の終わりで停止することはスンナです。"
            }
        },

        // الوقف المتعانق
        {
            "الوقف المتعانق",
            new RuleExplanation
            {
                Arabic = @"الوقف المتعانق
إذا وقفت على الموضع الأول لا تقف على الموضع الثاني.
وإذا وقفت على الموضع الثاني لا تقف على الموضع الأول.
يُختار أحد الموضعين فقط للوقف.",
                English = @"Paired Stop (Mutually Exclusive Stop)
If you stop at the first position, do not stop at the second position.
And if you stop at the second position, do not stop at the first position.
Only one of the two positions is chosen for stopping.",
                German = @"Gekoppelter Halt (wechselseitiger Halt)
Wenn man an der ersten Stelle stoppt, stoppt man nicht an der zweiten Stelle.
Und wenn man an der zweiten Stelle stoppt, stoppt man nicht an der ersten Stelle.
Nur eine der beiden Stellen wird zum Stoppen gewählt.",
                Spanish = @"Parada Emparejada (Parada Mutuamente Exclusiva)
Si te detienes en la primera posición, no te detengas en la segunda posición.
Y si te detienes en la segunda posición, no te detengas en la primera posición.
Solo se elige una de las dos posiciones para detenerse.",
                Turkish = @"Eşleştirilmiş Duruş (Karşılıklı Münhasır Duruş)
İlk konumda durursan, ikinci konumda durma.
Ve ikinci konumda durursan, ilk konumda durma.
Durmak için iki konumdan yalnızca biri seçilir.",
                French = @"Arrêt Jumelé (Arrêt Mutuellement Exclusif)
Si vous vous arrêtez à la première position, ne vous arrêtez pas à la deuxième position.
Et si vous vous arrêtez à la deuxième position, ne vous arrêtez pas à la première position.
Seule l'une des deux positions est choisie pour s'arrêter.",
                Japanese = @"対になった停止（相互排他的停止）
最初の位置で停止した場合、2番目の位置で停止しないでください。
そして2番目の位置で停止した場合、最初の位置で停止しないでください。
停止するために2つの位置のうち1つだけが選択されます。"
            }
        },

        // حروف الاستعلاء المفخمة
        {
            "حروف الاستعلاء المفخمة (خص ضفط قظ)",
            new RuleExplanation
            {
                Arabic = @"حروف الاستعلاء المفخمة
التفخيم: امتلاء الفم بصدى الحرف.
الترقيق: عكس التفخيم.
حروف التفخيم (حروف الاستعلاء): خص ضغط قظ (خ ص ض غ ط ق ظ)
هذه الحروف مفخمة دائماً في جميع أحوالها.",
                English = @"Elevated (Emphatic) Letters
Tafkheem (emphasis): filling the mouth with the echo of the letter.
Tarqeeq (lightness): the opposite of Tafkheem.
Emphatic letters (elevated letters): خ ص ض غ ط ق ظ (Kha, Saad, Daad, Ghain, Ta, Qaf, Dha)
These letters are always emphatic in all conditions.",
                German = @"Erhabene (betonte) Buchstaben
Tafkheem (Betonung): Füllen des Mundes mit dem Echo des Buchstabens.
Tarqeeq (Leichtigkeit): das Gegenteil von Tafkheem.
Betonte Buchstaben (erhabene Buchstaben): خ ص ض غ ط ق ظ (Kha, Saad, Daad, Ghain, Ta, Qaf, Dha)
Diese Buchstaben sind immer betont in allen Zuständen.",
                Spanish = @"Letras Elevadas (Enfáticas)
Tafkheem (énfasis): llenar la boca con el eco de la letra.
Tarqeeq (ligereza): lo opuesto a Tafkheem.
Letras enfáticas (letras elevadas): خ ص ض غ ط ق ظ (Kha, Saad, Daad, Ghain, Ta, Qaf, Dha)
Estas letras son siempre enfáticas en todas las condiciones.",
                Turkish = @"Yükseltilmiş (Vurgulu) Harfler
Tefhim (vurgu): ağzı harfin yankısıyla doldurmak.
Terkik (hafiflik): Tefhim'in tersi.
Vurgulu harfler (yükseltilmiş harfler): خ ص ض غ ط ق ظ (Ha, Sad, Dad, Gayn, Ta, Kaf, Za)
Bu harfler her durumda her zaman vurguludur.",
                French = @"Lettres Élevées (Emphatiques)
Tafkheem (emphase): remplir la bouche avec l'écho de la lettre.
Tarqeeq (légèreté): l'opposé de Tafkheem.
Lettres emphatiques (lettres élevées): خ ص ض غ ط ق ظ (Kha, Saad, Daad, Ghain, Ta, Qaf, Dha)
Ces lettres sont toujours emphatiques dans toutes les conditions.",
                Japanese = @"高められた（強調）文字
タフヒーム（強調）：口を文字の反響で満たすこと。
タルキーク（軽さ）：タフヒームの反対。
強調文字（高められた文字）：خ ص ض غ ط ق ظ（ハー、サード、ダード、ガイン、ター、カーフ、ザー）
これらの文字はすべての条件で常に強調されます。"
            }
        },

        // الألف المفخمة
        {
            "الألف المفخمة (بالتبعية)",
            new RuleExplanation
            {
                Arabic = @"الألف المفخمة (بالتبعية)
الألف حرف يمكن تفخيمه أو ترقيقه حسب ما قبله.
إذا جاء قبل الألف حرف مفخم، تُفخم الألف تبعاً له.
إذا جاء قبل الألف حرف مرقق، تُرقق الألف تبعاً له.
مثال: ""قَال"" (الألف مفخمة) - ""كَان"" (الألف مرققة).",
                English = @"Emphatic Alif (by Following the Preceding Letter)
Alif is a letter that can be emphatic or light depending on what precedes it.
If an emphatic letter precedes Alif, Alif becomes emphatic following it.
If a light letter precedes Alif, Alif becomes light following it.
Example: ""قَال"" (emphatic Alif) - ""كَان"" (light Alif).",
                German = @"Betontes Alif (abhängig vom vorherigen Buchstaben)
Alif ist ein Buchstabe, der je nach dem vorherigen Buchstaben betont oder leicht sein kann.
Wenn ein betonter Buchstabe vor Alif steht, wird Alif betont.
Wenn ein leichter Buchstabe vor Alif steht, wird Alif leicht.
Beispiel: ""قَال"" (betontes Alif) - ""كَان"" (leichtes Alif).",
                Spanish = @"Alif Enfático (Siguiendo la Letra Precedente)
Alif es una letra que puede ser enfática o ligera dependiendo de lo que la precede.
Si una letra enfática precede a Alif, Alif se vuelve enfático siguiéndola.
Si una letra ligera precede a Alif, Alif se vuelve ligero siguiéndola.
Ejemplo: ""قَال"" (Alif enfático) - ""كَان"" (Alif ligero).",
                Turkish = @"Vurgulu Elif (Önceki Harfi Takip Ederek)
Elif, kendisinden önce gelene bağlı olarak vurgulu veya hafif olabilen bir harftir.
Elif'ten önce vurgulu bir harf gelirse, Elif onu takip ederek vurgulu olur.
Elif'ten önce hafif bir harf gelirse, Elif onu takip ederek hafif olur.
Örnek: ""قَال"" (vurgulu Elif) - ""كَان"" (hafif Elif).",
                French = @"Alif Emphatique (En Suivant la Lettre Précédente)
Alif est une lettre qui peut être emphatique ou légère selon ce qui la précède.
Si une lettre emphatique précède Alif, Alif devient emphatique en la suivant.
Si une lettre légère précède Alif, Alif devient légère en la suivant.
Exemple: ""قَال"" (Alif emphatique) - ""كَان"" (Alif légère).",
                Japanese = @"強調されたアリフ（前の文字に従う）
アリフは、前に来るものに応じて強調または軽くなることができる文字です。
強調文字がアリフの前に来る場合、アリフはそれに従って強調されます。
軽い文字がアリフの前に来る場合、アリフはそれに従って軽くなります。
例：""قَال""（強調されたアリフ）- ""كَان""（軽いアリフ）。"
            }
        },

        // لام لفظ الجلالة المفخمة
        {
            "لام لفظ الجلالة المفخمة",
            new RuleExplanation
            {
                Arabic = @"لام لفظ الجلالة المفخمة
لام لفظ الجلالة ""الله"" تُفخم إذا سُبقت بفتح أو ضم.
تُرقق إذا سُبقت بكسر.
مثال مفخم: ""قالَ اللهُ"" - ""عَبْدُ اللهِ""
مثال مرقق: ""بِسْمِ اللهِ"" - ""لِلَّهِ"".",
                English = @"Emphatic Lam in the Word of Majesty (Allah)
The Lam in ""Allah"" is emphatic when preceded by Fathah or Dammah.
It becomes light when preceded by Kasrah.
Emphatic examples: ""قالَ اللهُ"" - ""عَبْدُ اللهِ""
Light examples: ""بِسْمِ اللهِ"" - ""لِلَّهِ"".",
                German = @"Betontes Lam im Gottesnamen (Allah)
Das Lam in ""Allah"" ist betont, wenn Fathah oder Dammah vorausgeht.
Es wird leicht, wenn Kasrah vorausgeht.
Betonte Beispiele: ""قالَ اللهُ"" - ""عَبْدُ اللهِ""
Leichte Beispiele: ""بِسْمِ اللهِ"" - ""لِلَّهِ"".",
                Spanish = @"Lam Enfático en la Palabra de Majestad (Allah)
El Lam en ""Allah"" es enfático cuando es precedido por Fathah o Dammah.
Se vuelve ligero cuando es precedido por Kasrah.
Ejemplos enfáticos: ""قالَ اللهُ"" - ""عَبْدُ اللهِ""
Ejemplos ligeros: ""بِسْمِ اللهِ"" - ""لِلَّهِ"".",
                Turkish = @"Celal Lafzındaki Vurgulu Lam (Allah)
""Allah"" kelimesindeki Lam, Fetha veya Damme ile önceldiğinde vurguludur.
Kesre ile önceldiğinde hafif olur.
Vurgulu örnekler: ""قالَ اللهُ"" - ""عَبْدُ اللهِ""
Hafif örnekler: ""بِسْمِ اللهِ"" - ""لِلَّهِ"".",
                French = @"Lam Emphatique dans le Mot de Majesté (Allah)
Le Lam dans ""Allah"" est emphatique lorsqu'il est précédé de Fathah ou Dammah.
Il devient léger lorsqu'il est précédé de Kasrah.
Exemples emphatiques: ""قالَ اللهُ"" - ""عَبْدُ اللهِ""
Exemples légers: ""بِسْمِ اللهِ"" - ""لِلَّهِ"".",
                Japanese = @"尊厳の言葉における強調されたラーム（アッラー）
""アッラー""のラームは、ファトハまたはダンマが前にある場合に強調されます。
カスラが前にある場合は軽くなります。
強調された例：""قالَ اللهُ"" - ""عَبْدُ اللهِ""
軽い例：""بِسْمِ اللهِ"" - ""لِلَّهِ""。"
            }
        },

        // الراء المفخمة
        {
            "الراء المفخمة",
            new RuleExplanation
            {
                Arabic = @"الراء المفخمة
الراء حرف يمكن تفخيمه أو ترقيقه.
تُفخم الراء في الحالات التالية:
1. إذا كانت مفتوحة أو مضمومة.
2. إذا كانت ساكنة وقبلها فتح أو ضم.
3. إذا كانت ساكنة وقبلها كسر عارض.
4. إذا كانت ساكنة وقبلها كسر أصلي وبعدها حرف استعلاء.
تُرقق في غير ذلك.",
                English = @"Emphatic Ra'
Ra is a letter that can be emphatic or light.
Ra is emphatic in the following cases:
1. When it has Fathah or Dammah.
2. When it is silent and preceded by Fathah or Dammah.
3. When it is silent and preceded by temporary Kasrah.
4. When it is silent, preceded by original Kasrah, and followed by an emphatic letter.
It is light in other cases.",
                German = @"Betontes Ra
Ra ist ein Buchstabe, der betont oder leicht sein kann.
Ra ist betont in folgenden Fällen:
1. Wenn es Fathah oder Dammah hat.
2. Wenn es stumm ist und Fathah oder Dammah vorausgeht.
3. Wenn es stumm ist und vorübergehendes Kasrah vorausgeht.
4. Wenn es stumm ist, ursprüngliches Kasrah vorausgeht und ein betonter Buchstabe folgt.
Es ist leicht in anderen Fällen.",
                Spanish = @"Ra Enfático
Ra es una letra que puede ser enfática o ligera.
Ra es enfático en los siguientes casos:
1. Cuando tiene Fathah o Dammah.
2. Cuando es silencioso y precedido por Fathah o Dammah.
3. Cuando es silencioso y precedido por Kasrah temporal.
4. Cuando es silencioso, precedido por Kasrah original, y seguido por una letra enfática.
Es ligero en otros casos.",
                Turkish = @"Vurgulu Ra
Ra vurgulu veya hafif olabilen bir harftir.
Ra aşağıdaki durumlarda vurguludur:
1. Fetha veya Damme'ye sahip olduğunda.
2. Sessiz olduğunda ve önünde Fetha veya Damme olduğunda.
3. Sessiz olduğunda ve önünde geçici Kesre olduğunda.
4. Sessiz olduğunda, önünde orijinal Kesre olduğunda ve ardından vurgulu bir harf geldiğinde.
Diğer durumlarda hafiftir.",
                French = @"Ra Emphatique
Ra est une lettre qui peut être emphatique ou légère.
Ra est emphatique dans les cas suivants:
1. Lorsqu'il a Fathah ou Dammah.
2. Lorsqu'il est silencieux et précédé de Fathah ou Dammah.
3. Lorsqu'il est silencieux et précédé de Kasrah temporaire.
4. Lorsqu'il est silencieux, précédé de Kasrah original, et suivi d'une lettre emphatique.
Il est léger dans les autres cas.",
                Japanese = @"強調されたラー
ラーは強調または軽くなることができる文字です。
ラーは次の場合に強調されます：
1. ファトハまたはダンマを持つ場合。
2. 無音でファトハまたはダンマが前にある場合。
3. 無音で一時的なカスラが前にある場合。
4. 無音で、元のカスラが前にあり、強調文字が後に続く場合。
他の場合は軽いです。"
            }
        }
    };

    /// <summary>
    /// Gets the explanation for a Tajweed rule in the specified language.
    /// </summary>
    /// <param name="arabicRuleName">The Arabic name of the rule.</param>
    /// <param name="languageCode">The language code (ar, en, de, es, tr, fr, ja).</param>
    /// <returns>The explanation in the specified language, or an empty string if not found.</returns>
    public static string GetExplanation(string arabicRuleName, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(arabicRuleName))
            return string.Empty;

        if (!Explanations.TryGetValue(arabicRuleName, out var explanation))
            return string.Empty;

        return languageCode switch
        {
            "ar" => explanation.Arabic,
            "de" => explanation.German,
            "es" => explanation.Spanish,
            "tr" => explanation.Turkish,
            "fr" => explanation.French,
            "ja" => explanation.Japanese,
            _ => explanation.English // English as fallback
        };
    }

    /// <summary>
    /// Checks if an explanation exists for the given rule.
    /// </summary>
    /// <param name="arabicRuleName">The Arabic name of the rule.</param>
    /// <returns>True if an explanation exists, false otherwise.</returns>
    public static bool HasExplanation(string arabicRuleName)
    {
        return !string.IsNullOrWhiteSpace(arabicRuleName) && Explanations.ContainsKey(arabicRuleName);
    }
}
