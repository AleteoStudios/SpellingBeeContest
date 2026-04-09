using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ProtectedButton : MonoBehaviour
{
    [Header("UI References (Arrastra aquí los objetos de tu panel)")]
    [SerializeField] GameObject upgradePanel, panelValue; // El Panel que contiene todo
    [SerializeField] TextMeshProUGUI messageText; // El texto del mensaje
    [SerializeField] Button buyButton; // El botón de "Comprar"
    [SerializeField] Button closeButton; // El botón de "Cerrar" (opcional)

    [Header("Configuración del Mensaje")]
    [SerializeField, TextArea] string upgradeMessage = "Esta función requiere una licencia Anual o Perpetua activa.\n¡Actualiza ahora para desbloquearla!";
    [SerializeField] string buyUrl = "https://spelling-bee-contest-632985740776.us-west1.run.app"; // Tu URL de compra

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnButtonClick);

        // Configuramos los botones del panel
        
        if (closeButton != null) closeButton.onClick.AddListener(HidePanel);

        // Aseguramos que el panel inicie oculto
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    void OnButtonClick()
    {
        // Validamos la licencia
        StartCoroutine(LicenseValidator.CheckLicense(isValid => {
            if (isValid)
            {
                if(btn.tag == "FilesButton")
                {
                    SceneManager.LoadScene(1);
                }

                else
                {
                   panelValue.SetActive(true);
                }
                    
                Debug.Log("Acceso concedido: Ejecutando acción del botón...");
                // AQUÍ: Llama a la función que hace la acción real del botón
            }
            else
            {
                ShowPanel();
            }
        }));
    }

    void ShowPanel()
    {
        if (upgradePanel != null)
        {
            if (messageText != null) messageText.text = upgradeMessage;
            upgradePanel.SetActive(true);
        }
    }

    void HidePanel()
    {
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    public void OnBuyClick()
    {
        
            Application.OpenURL(buyUrl);
        
    }
}