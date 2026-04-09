using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class UserLicense
{
    public string id;
    public string user_id;
    public string valid_until;
    public string created_at;
    public string license_type;
}

public static class LicenseValidator
{
    public static IEnumerator CheckLicense(Action<bool> onResult)
    {
        // Esta consulta busca licencias activas para el usuario autenticado
        string url = "/user_licenses?status=eq.active&select=valid_until,package_name,license_type,created_at";

        bool isValid = false;

        yield return AuthSupabase.Get(url, 
            onOk: (json) => {
                // Supabase devuelve un array JSON: [{"package_name":"...", "status":"...", "valid_until":"..."}]
                // Necesitas un pequeño helper para deserializar arrays en Unity
                string wrappedJson = "{\"items\":" + json + "}";
                var licenses = SupaJsonHelper.FromJson<UserLicense>(wrappedJson);
                bool hasAdvancedAccess = false; // Por defecto, no tienen acceso

                foreach (var lic in licenses)
                {
                    Debug.Log("Valor detectado en licencia: '" + lic.license_type + "'");

                    // Ahora puedes distinguir claramente
                    if (lic.license_type == "annual" || lic.license_type == "perpetual")
                    {
                        // Lógica de acceso avanzado
                        if (DateTime.Parse(lic.valid_until) > DateTime.UtcNow)
                        {
                            hasAdvancedAccess = true;
                            break;
                        }
                    }
                    else
                    {
                        Debug.Log("Licencia básica detectada, acceso denegado.");
                    }
                }
                onResult?.Invoke(hasAdvancedAccess); 
            },
            onError: (err) => {
                Debug.LogError("Error validando licencia: " + err);
                onResult?.Invoke(false);
            }
        );
    }
}

// Helper necesario para deserializar arrays en Unity
public static class SupaJsonHelper {
    public static T[] FromJson<T>(string json) {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.items;
    }
    [Serializable] private class Wrapper<T> { public T[] items; }
}