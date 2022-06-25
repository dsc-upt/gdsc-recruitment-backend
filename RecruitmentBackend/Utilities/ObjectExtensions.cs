using Newtonsoft.Json;

namespace RecruitmentBackend.Utilities;

public static class ObjectExtensions
{
    public static void WriteJson(this object obj)
    {
        var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        var serializedObject = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        Console.WriteLine(serializedObject);
    }
}
