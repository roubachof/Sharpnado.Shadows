using System.Diagnostics;

#if MACCATALYST || IOS
using UIKit;
#endif

namespace ShadowsSample.Maui.Helpers;

public static class FontDiagnostics
{
    public static void ListAllFonts()
    {
#if MACCATALYST || IOS
        Debug.WriteLine("=== Available Font Families ===");
        
        var fontFamilies = UIFont.FamilyNames;
        foreach (var familyName in fontFamilies.OrderBy(f => f))
        {
            Debug.WriteLine($"\nFamily: {familyName}");
            
            var fontNames = UIFont.FontNamesForFamilyName(familyName);
            foreach (var fontName in fontNames)
            {
                Debug.WriteLine($"  - {fontName}");
            }
        }
        
        Debug.WriteLine("\n=== Searching for Font Awesome ===");
        var fontAwesomeFamilies = fontFamilies.Where(f => f.Contains("Awesome", StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (fontAwesomeFamilies.Any())
        {
            foreach (var family in fontAwesomeFamilies)
            {
                Debug.WriteLine($"\n✅ Found: {family}");
                var names = UIFont.FontNamesForFamilyName(family);
                foreach (var name in names)
                {
                    Debug.WriteLine($"  - {name}");
                }
            }
        }
        else
        {
            Debug.WriteLine("❌ No Font Awesome fonts found!");
        }
#else
        Debug.WriteLine("Font diagnostics only available on iOS/MacCatalyst");
#endif
    }
    
    public static void TestFontAwesome()
    {
#if MACCATALYST || IOS
        Debug.WriteLine("\n=== Testing Font Awesome Loading ===");
        
        var testNames = new[]
        {
            "Font Awesome 5 Free Solid",
            "Font Awesome 5 Free Regular",
            "Font Awesome 5 Free",
            "FontAwesome5Free-Solid",
            "FontAwesome5Free-Regular",
            "Font-Awesome-5-Free-Solid-900",
        };
        
        foreach (var name in testNames)
        {
            var font = UIFont.FromName(name, 12);
            if (font != null)
            {
                Debug.WriteLine($"✅ '{name}' -> Found: {font.Name}");
            }
            else
            {
                Debug.WriteLine($"❌ '{name}' -> Not found");
            }
        }
#else
        Debug.WriteLine("Font diagnostics only available on iOS/MacCatalyst");
#endif
    }
}
