using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// Respuestas de Auth
[Serializable] class SupaLoginRes { public string access_token; public string refresh_token; public int expires_in; public string token_type; }
[Serializable] class SupaError { public string error; public string error_description; }

public static class AuthSupabase
{
    // ⚠️ Rellena con tu proyecto
    public static string SUPABASE_URL = "https://tnprwaxqnwgybflnoxtt.supabase.co";
    public static string SUPABASE_ANON_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InRucHJ3YXhxbndneWJmbG5veHR0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTUyMzU5MDAsImV4cCI6MjA3MDgxMTkwMH0.lQieYtNePKb3e7AlOzBiFb5QMbB9PE7H8rNRPegvj0s";

    // almacenamiento básico (puedes cambiar a archivo encriptado si gustas)
    private const string PK_ACCESS = "supa_access_token";
    private const string PK_REFRESH = "supa_refresh_token";
    private const string PK_EXPIRES_AT = "supa_expires_at_utc"; // ticks

    public static string AccessToken => PlayerPrefs.GetString(PK_ACCESS, null);
    public static string RefreshToken => PlayerPrefs.GetString(PK_REFRESH, null);
    public static DateTime ExpiresAtUtc =>
        new DateTime(long.Parse(PlayerPrefs.GetString(PK_EXPIRES_AT, DateTime.MinValue.Ticks.ToString())), DateTimeKind.Utc);

    public static bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);
    public static bool IsTokenNearExpiry => (ExpiresAtUtc - DateTime.UtcNow) < TimeSpan.FromMinutes(2);

    public static void ClearSession()
    {
        PlayerPrefs.DeleteKey(PK_ACCESS);
        PlayerPrefs.DeleteKey(PK_REFRESH);
        PlayerPrefs.DeleteKey(PK_EXPIRES_AT);
        PlayerPrefs.Save();
    }

    // ---------------- AUTH ----------------

    public static IEnumerator Login(string email, string password, Action onOk, Action<string> onError)
    {
        var url = $"{SUPABASE_URL}/auth/v1/token?grant_type=password";
        var body = $"{{\"email\":\"{Escape(email)}\",\"password\":\"{Escape(password)}\"}}";
        using var www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new DownloadHandlerBuffer();
        SetAuthRequestHeaders(www);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"Red: {www.error}");
            yield break;
        }
        if (www.responseCode != 200)
        {
            onError?.Invoke(ParseError(www.downloadHandler.text) ?? "Credenciales inválidas o email no confirmado.");
            yield break;
        }

        var res = JsonUtility.FromJson<SupaLoginRes>(www.downloadHandler.text);
        SaveSession(res);
        onOk?.Invoke();
    }

    public static IEnumerator Refresh(Action onOk, Action<string> onError)
    {
        if (string.IsNullOrEmpty(RefreshToken))
        {
            onError?.Invoke("No hay refresh_token.");
            yield break;
        }

        var url = $"{SUPABASE_URL}/auth/v1/token?grant_type=refresh_token";
        var body = $"{{\"refresh_token\":\"{Escape(RefreshToken)}\"}}";
        using var www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new DownloadHandlerBuffer();
        SetAuthRequestHeaders(www);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"Red: {www.error}");
            yield break;
        }
        if (www.responseCode != 200)
        {
            onError?.Invoke(ParseError(www.downloadHandler.text) ?? "Refresh inválido.");
            yield break;
        }

        var res = JsonUtility.FromJson<SupaLoginRes>(www.downloadHandler.text);
        SaveSession(res);
        onOk?.Invoke();
    }

    public static void Logout() => ClearSession();

    // ---------------- REST (PostgREST) ----------------

    public static IEnumerator Get(string pathWithQuery, Action<string> onOk, Action<string> onError)
    {
        yield return EnsureValidToken();

        using var www = UnityWebRequest.Get($"{SUPABASE_URL}/rest/v1{pathWithQuery}");
        SetRestHeaders(www);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) onError?.Invoke(www.error);
        else if (www.responseCode == 401) onError?.Invoke("No autorizado.");
        else onOk?.Invoke(www.downloadHandler.text);
    }

    public static IEnumerator PostJson(string path, string jsonBody, Action<string> onOk, Action<string> onError)
    {
        yield return EnsureValidToken();

        using var www = new UnityWebRequest($"{SUPABASE_URL}/rest/v1{path}", "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        www.downloadHandler = new DownloadHandlerBuffer();
        SetRestHeaders(www);
        www.SetRequestHeader("Prefer", "return=representation"); // devolver filas insertadas

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) onError?.Invoke(www.error);
        else if (www.responseCode >= 400) onError?.Invoke(www.downloadHandler.text);
        else onOk?.Invoke(www.downloadHandler.text);
    }

    public static IEnumerator PatchJson(string path, string jsonBody, Action<string> onOk, Action<string> onError)
    {
        yield return EnsureValidToken();

        using var www = new UnityWebRequest($"{SUPABASE_URL}/rest/v1{path}", "PATCH");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        www.downloadHandler = new DownloadHandlerBuffer();
        SetRestHeaders(www);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) onError?.Invoke(www.error);
        else if (www.responseCode >= 400) onError?.Invoke(www.downloadHandler.text);
        else onOk?.Invoke(www.downloadHandler.text);
    }

    // ---------------- Helpers ----------------

    private static IEnumerator EnsureValidToken()
    {
        if (IsLoggedIn && IsTokenNearExpiry)
        {
            bool done = false; string err = null;
            yield return Refresh(() => { done = true; }, e => { err = e; done = true; });
            if (err != null) Debug.LogWarning($"Refresh falló: {err}");
        }
    }

    private static void SaveSession(SupaLoginRes res)
    {
        PlayerPrefs.SetString(PK_ACCESS, res.access_token);
        PlayerPrefs.SetString(PK_REFRESH, res.refresh_token);
        var expiresAt = DateTime.UtcNow.AddSeconds(res.expires_in - 30); // margen
        PlayerPrefs.SetString(PK_EXPIRES_AT, expiresAt.Ticks.ToString());
        PlayerPrefs.Save();
    }

    private static void SetAuthRequestHeaders(UnityWebRequest www)
    {
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("apikey", SUPABASE_ANON_KEY);
        www.SetRequestHeader("Authorization", $"Bearer {SUPABASE_ANON_KEY}");
    }

    private static void SetRestHeaders(UnityWebRequest www)
    {
        www.downloadHandler = www.downloadHandler ?? new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("apikey", SUPABASE_ANON_KEY);
        www.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
    }

    private static string Escape(string s) => s?.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string ParseError(string json)
    {
        try { var e = JsonUtility.FromJson<SupaError>(json); return $"{e.error}: {e.error_description}".Trim(':'); }
        catch { return null; }
    }
}
