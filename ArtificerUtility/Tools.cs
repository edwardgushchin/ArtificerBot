using System.Text;

namespace Artificer.Utility;

public static class Tools
{
    public static void DictionaryToCsv<Ta, Tb>(Dictionary<Ta, Tb> dict, string filePath) where Ta : notnull
    {
        if (dict == null)
            throw new ArgumentNullException(nameof(dict));
    
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException(null, nameof(filePath));

        using var writer = new StreamWriter(filePath, Encoding.UTF8, new FileStreamOptions { Access = FileAccess.Write, Mode = FileMode.Create});
        writer.WriteLine($"key;value");
        foreach (var item in dict)
        {
            writer.WriteLine($"{item.Key};{item.Value}");
        }
    }
}