using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using System.Web.Script.Serialization;

public static class CmfAssistantResponseCache
{
    private static readonly object[] Gates = CreateGates();

    public static bool HasVerifiedSource(string context)
    {
        return !string.IsNullOrEmpty(context) && context.IndexOf("HSD Source Fingerprint:", StringComparison.Ordinal) >= 0;
    }

    public static void ClearRecommendations()
    {
        string directory = HostingEnvironment.MapPath("~/App_Data/cmf-assistant-cache");
        try
        {
            if (directory == null || !Directory.Exists(directory)) return;
            foreach (string path in Directory.GetFiles(directory, "recommendation-*.json")) File.Delete(path);
        }
        catch (Exception error)
        {
            System.Diagnostics.Trace.TraceWarning("CMF assistant cache clear failed: {0}", error.GetType().Name);
        }
    }

    public static IDisposable Enter(string key)
    {
        string hash = Fingerprint(key);
        object gate = Gates[Convert.ToInt32(hash.Substring(0, 2), 16) % Gates.Length];
        Monitor.Enter(gate);
        return new GateLease(gate);
    }

    public static string Fingerprint(object value)
    {
        string serialized = new JavaScriptSerializer { MaxJsonLength = 4194304 }.Serialize(value);
        using (SHA256 algorithm = SHA256.Create())
        {
            return BitConverter.ToString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(serialized))).Replace("-", string.Empty).ToLowerInvariant();
        }
    }

    public static T Read<T>(string key) where T : class
    {
        string path = GetPath(key);
        if (path == null || !File.Exists(path)) return null;
        try
        {
            return new JavaScriptSerializer { MaxJsonLength = 4194304 }.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (Exception error)
        {
            System.Diagnostics.Trace.TraceWarning("CMF assistant cache read failed: {0}", error.GetType().Name);
            return null;
        }
    }

    public static void Write<T>(string key, T response)
    {
        string path = GetPath(key);
        if (path == null) return;
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string json = new JavaScriptSerializer { MaxJsonLength = 4194304 }.Serialize(response);
            File.WriteAllText(temporary, json, Encoding.UTF8);
            if (File.Exists(path)) File.Replace(temporary, path, null);
            else File.Move(temporary, path);
        }
        catch (Exception error)
        {
            System.Diagnostics.Trace.TraceWarning("CMF assistant cache write failed: {0}", error.GetType().Name);
        }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static string GetPath(string key)
    {
        string directory = HostingEnvironment.MapPath("~/App_Data/cmf-assistant-cache");
        string prefix = key.StartsWith("cmf-recommendation:", StringComparison.Ordinal) ? "recommendation-" : string.Empty;
        return directory == null ? null : Path.Combine(directory, prefix + Fingerprint(key) + ".json");
    }

    private static object[] CreateGates()
    {
        object[] gates = new object[64];
        for (int index = 0; index < gates.Length; index++) gates[index] = new object();
        return gates;
    }

    private sealed class GateLease : IDisposable
    {
        private readonly object gate;
        public GateLease(object gate) { this.gate = gate; }
        public void Dispose() { Monitor.Exit(gate); }
    }
}