using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passInput;
    [SerializeField] TextMeshProUGUI feedback;
    [SerializeField] GameObject panelClose;

    public void OnClickLogin()
    {
        feedback.text = "Conectando...";
        StartCoroutine(AuthSupabase.Login(
            emailInput.text.Trim(),
            passInput.text,
            onOk: () => {
                feedback.text = "✅ Acceso concedido";
                panelClose.SetActive(false);
            },
            onError: e => feedback.text = $"❌ {e}"
        ));
    }

    public void OnClickLogout()
    {
        AuthSupabase.Logout();
        feedback.text = "Sesión cerrada.";
    }
}
