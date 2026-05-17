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
Sonnenbuchstaben: ت ث د ذ ر ز س ش ص ض ط ظ ل ن"
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
Ein Sukoon (kleiner Kreis) wird darüber im Mushaf geschrieben."
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
Ein verdoppelter Buchstabe wird nicht vibriert, es sei denn, man stoppt darauf."
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
Das Nun oder Tanwin wird deutlich ausgesprochen, ohne Verschmelzung oder Verbergung."
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
Sein Zeichen im Mushaf: ein kleines Mim über dem Nun."
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
Ausnahmen: صنوان، قنوان، الدنيا، بنيان (keine Verschmelzung in diesen Wörtern)."
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
Das Nun oder Tanwin wird vollständig in Lam oder Ra verschmolzen."
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
Die Nasalierung dauert zwei Zählungen."
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
Das Mim wird mit Nasalierung für zwei Zählungen verborgen."
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
Beide Mims verschmelzen zusammen mit Nasalierung für zwei Zählungen."
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
Man muss aufpassen, es nicht vor Waw und Fa zu verbergen."
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
Es wird betonte Ghunnah genannt."
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
Seine Dauer ändert sich nicht, ob man verbindet oder stoppt."
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
Hinweis: Ya des Rufens und Ha der Warnung werden im Mushaf verbunden geschrieben, folgen aber der Regel von Madd Munfasil."
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
Hinweis: ""Ha"" in ""هَاؤم"" ist aus der Wurzel des Wortes, daher folgt es Madd Muttasil."
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
Beispiel: ""ءَالْـَٔـٰن"" (an zwei Stellen in Surah Yunus)."
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
Beispiele: ""الحَاقَّة"" - ""الطَّامَّة"" - ""الصَّاخَّة""."
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
Getrennte Buchstaben mit dieser Verlängerung: س ن ق ص ل ك م."
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
Beispiel: ""الم"" - das Lam verschmilzt mit dem Mim."
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
Beispiele: ""خَوْف"" - ""بَيْت""."
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
Beispiele: ""ءَامَنُوا"" - ""إِيمَان"" - ""أُوتُوا""."
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
Sein Zeichen: ein kleines Waw oder ein umgekehrtes Ya."
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
Sein Zeichen: ein Maddah-Zeichen über dem Waw oder Ya."
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
Beispiel: ""شَجَرَةً"" ⬅ ""شَجَرَهْ"" beim Stoppen."
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
Beispiele: ""نَسْتَعِين"" - ""العَالَمِين""."
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
Keine Option wird der anderen vorgezogen."
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
Es ist besser, weiterzulesen."
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
Es ist besser, an dieser Stelle anzuhalten."
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
Man darf nicht stoppen, außer aus Notwendigkeit wie Atemlosigkeit."
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
Das Stoppen am Ende von Versen ist eine Sunnah."
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
Nur eine der beiden Stellen wird zum Stoppen gewählt."
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
Diese Buchstaben sind immer betont in allen Zuständen."
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
Beispiel: ""قَال"" (betontes Alif) - ""كَان"" (leichtes Alif)."
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
Leichte Beispiele: ""بِسْمِ اللهِ"" - ""لِلَّهِ""."
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
Es ist leicht in anderen Fällen."
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
            "es" => !string.IsNullOrEmpty(explanation.Spanish) ? explanation.Spanish : explanation.English,
            "tr" => !string.IsNullOrEmpty(explanation.Turkish) ? explanation.Turkish : explanation.English,
            "fr" => !string.IsNullOrEmpty(explanation.French) ? explanation.French : explanation.English,
            "ja" => !string.IsNullOrEmpty(explanation.Japanese) ? explanation.Japanese : explanation.English,
            _ => explanation.English
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
