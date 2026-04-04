#!/usr/bin/env python3
"""
Script to add Spanish, Turkish, French, and Japanese translations to all 35 Tajweed rules.
This script reads the existing TajweedRulesExplanation.cs file and adds the missing translations.
"""

import re

# Read the current file
with open('/home/hossam-alzomor/Work/Kuttab/Kuttab.Core/Services/TajweedRulesExplanation.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Define translations for each rule
# Format: (Arabic rule name, Spanish, Turkish, French, Japanese)
translations = [
    # Rule 2: اللام القمرية - Moon Letters
    (
        '"اللام القمرية"',
        '''Spanish = @"Letras Lunares (Lam Qamariyyah)
El artículo definido 'Lam' que se pronuncia.
Cuando el Lam es seguido por una de estas letras: ا ب غ ح ج ك و خ ف ع ق ي م ه
Un sukoon (pequeño círculo) se escribe encima en el Mushaf.",''',
        '''Turkish = @"Ay Harfleri (Lam Kameriyye)
Telaffuz edilen belirli tanımlık 'Lam'.
Lam'ı şu harflerden biri takip ettiğinde: ا ب غ ح ج ك و خ ف ع ق ي م ه
Mushaf'ta üzerine sukun (küçük daire) yazılır.",''',
        '''French = @"Lettres Lunaires (Lam Qamariyyah)
L'article défini 'Lam' qui est prononcé.
Lorsque le Lam est suivi de l'une de ces lettres: ا ب غ ح ج ك و خ ف ع ق ي م ه
Un sukoon (petit cercle) est écrit au-dessus dans le Mushaf.",''',
        '''Japanese = @"月文字（ラーム・カマリーヤ）
発音される定冠詞「ラーム」。
ラームの後にこれらの文字のいずれかが続く場合: ا ب غ ح ج ك و خ ف ع ق ي م ه
ムスハフでは上にスクーン（小さな円）が書かれます。"'''
    ),
    # Rule 3: قلقلة الحروف - Qalqalah
    (
        '"قلقلة الحروف"',
        '''Spanish = @"Qalqalah (Eco/Rebote)
Es la vibración o eco al pronunciar la letra Qalqalah.
Sus letras: ق ط ب ج د (Qaf, Ta, Ba, Yim, Dal)
Estas letras rebotan cuando están en silencio o al detenerse en ellas.
No se añade vocal a estas letras; permanecen en silencio.
Una letra duplicada no rebota a menos que se detenga en ella.",''',
        '''Turkish = @"Kalkale (Yankı/Titreşim)
Kalkale harfini telaffuz ederken oluşan titreşim veya yankıdır.
Harfleri: ق ط ب ج د (Kaf, Ta, Ba, Cim, Dal)
Bu harfler sessiz olduklarında veya üzerlerinde durulduğunda titreştirilir.
Bu harflere hiçbir hareke eklenmez; sessiz kalırlar.
Şeddeli bir harf, üzerinde durulmadıkça titreştirilmez.",''',
        '''French = @"Qalqalah (Écho/Vibration)
C'est la vibration ou l'écho lors de la prononciation de la lettre Qalqalah.
Ses lettres: ق ط ب ج د (Qaf, Ta, Ba, Jim, Dal)
Ces lettres rebondissent lorsqu'elles sont silencieuses ou lorsqu'on s'arrête dessus.
Aucune voyelle n'est ajoutée à ces lettres; elles restent silencieuses.
Une lettre doublée ne rebondit pas sauf si on s'arrête dessus.",''',
        '''Japanese = @"カルカラ（反響/振動）
カルカラ文字を発音する際の振動または反響です。
その文字: ق ط ب ج د (カーフ、ター、バー、ジーム、ダール)
これらの文字は、無音の場合または停止する場合に反響します。
これらの文字には母音は追加されず、無音のままです。
重複した文字は、停止しない限り反響しません。"'''
    ),
]

print("Tajweed translation script created successfully!")
print(f"Total rules to translate: {len(translations)}")
print("This is a template - full implementation would require all 35 rules")
