using System;
using System.Collections.Generic;
using System.Xml;
using Kuttab.Core.Interfaces;

namespace Kuttab.Core.Services;

/// <summary>
/// Parses quran-data.xml to provide Quran page mapping.
/// Maps any (surah, aya) to a mushaf page number (1-604) and returns all ayahs on a page.
/// Data source: Tanzil.net (http://tanzil.net)
/// </summary>
public class QuranPageService
{
    private readonly List<PageEntry> _pages = new();
    private readonly List<SuraEntry> _suras = new();
    private bool _loaded = false;

    public bool IsLoaded => _loaded;

    public struct PageEntry
    {
        public int PageIndex;
        public int Sura;
        public int Aya;
    }

    public struct SuraEntry
    {
        public int Index;
        public int AyaCount;
        public int StartOffset;
        public string Name;
        public string TName;
    }

    public struct AyaReference
    {
        public int Sura;
        public int Aya;
    }

    /// <summary>
    /// Load page and sura data from quran-data.xml content.
    /// </summary>
    public void LoadFromXml(string xmlContent)
    {
        _pages.Clear();
        _suras.Clear();

        var doc = new XmlDocument();
        doc.LoadXml(xmlContent);

        // Parse suras
        var suraNodes = doc.SelectNodes("//suras/sura");
        if (suraNodes != null)
        {
            foreach (XmlNode node in suraNodes)
            {
                var entry = new SuraEntry
                {
                    Index = int.Parse(node.Attributes?["index"]?.Value ?? "0"),
                    AyaCount = int.Parse(node.Attributes?["ayas"]?.Value ?? "0"),
                    StartOffset = int.Parse(node.Attributes?["start"]?.Value ?? "0"),
                    Name = node.Attributes?["name"]?.Value ?? "",
                    TName = node.Attributes?["tname"]?.Value ?? ""
                };
                _suras.Add(entry);
            }
        }

        // Parse pages
        var pageNodes = doc.SelectNodes("//pages/page");
        if (pageNodes != null)
        {
            foreach (XmlNode node in pageNodes)
            {
                var entry = new PageEntry
                {
                    PageIndex = int.Parse(node.Attributes?["index"]?.Value ?? "0"),
                    Sura = int.Parse(node.Attributes?["sura"]?.Value ?? "0"),
                    Aya = int.Parse(node.Attributes?["aya"]?.Value ?? "0")
                };
                _pages.Add(entry);
            }
        }

        _loaded = _pages.Count > 0;
    }

    /// <summary>
    /// Get the mushaf page number (1-604) for a given surah and aya.
    /// </summary>
    public int GetPageForAya(int sura, int aya)
    {
        if (!_loaded || _pages.Count == 0) return 1;

        int page = 1;
        for (int i = 0; i < _pages.Count; i++)
        {
            var p = _pages[i];
            // Page starts at (p.Sura, p.Aya). If our aya comes before this page start, 
            // it belongs to the previous page.
            if (sura < p.Sura || (sura == p.Sura && aya < p.Aya))
                break;
            page = p.PageIndex;
        }
        return page;
    }

    /// <summary>
    /// Get all (surah, aya) references on a given page.
    /// </summary>
    public List<AyaReference> GetAyasOnPage(int pageIndex)
    {
        var result = new List<AyaReference>();
        if (!_loaded || pageIndex < 1 || pageIndex > _pages.Count) return result;

        // Find page start
        var pageStart = _pages[pageIndex - 1];
        int startSura = pageStart.Sura;
        int startAya = pageStart.Aya;

        // Find next page start (or end of Quran)
        int endSura, endAya;
        if (pageIndex < _pages.Count)
        {
            var nextPage = _pages[pageIndex]; // 0-indexed, so pageIndex = next page
            endSura = nextPage.Sura;
            endAya = nextPage.Aya;
        }
        else
        {
            // Last page - ends at Surah 114, last aya
            endSura = 115; // sentinel beyond last surah
            endAya = 1;
        }

        // Enumerate all ayahs from start to (exclusive) end
        int currentSura = startSura;
        int currentAya = startAya;

        while (true)
        {
            // Check if we've reached the end (next page start)
            if (currentSura > endSura || 
                (currentSura == endSura && currentAya >= endAya))
                break;

            // Safety: don't go beyond surah 114
            if (currentSura > 114) break;

            result.Add(new AyaReference { Sura = currentSura, Aya = currentAya });

            // Advance to next aya
            int ayaCount = GetAyaCount(currentSura);
            if (currentAya < ayaCount)
            {
                currentAya++;
            }
            else
            {
                // Move to next surah
                currentSura++;
                currentAya = 1;
            }
        }

        return result;
    }

    /// <summary>
    /// Get the number of ayahs in a surah.
    /// </summary>
    public int GetAyaCount(int suraIndex)
    {
        if (suraIndex >= 1 && suraIndex <= _suras.Count)
            return _suras[suraIndex - 1].AyaCount;
        return 0;
    }

    /// <summary>
    /// Get the total number of pages (should be 604).
    /// </summary>
    public int TotalPages => _pages.Count;
}
