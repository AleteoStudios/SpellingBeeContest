using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class Timer : NetworkBehaviour
{
    public LetterManager letterManager;

    public Slider timeSlider;
    public TextMeshProUGUI timeLabel;
    public TextMeshProUGUI txtTime;
    public TextMeshProUGUI txtSpellerTime;
    public Toggle toggle;

    [SerializeField]
    private NetworkVariable<float> syncedTime = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone);

    private float selectedTime = 30f;
    private bool startTimer = false;

    void Start()
    {
        
            timeSlider.onValueChanged.AddListener(UpdateTimeLabel);
            toggle.onValueChanged.AddListener(ToggleCrtl);
        

        // Inicializar visualmente
        UpdateUI();
    }

    void Update()
    {
        if (startTimer)
        {
            syncedTime.Value -= Time.deltaTime;

            if (syncedTime.Value <= 0f)
            {
                syncedTime.Value = 0f;
                startTimer = false;
                toggle.isOn = false;
                letterManager.IncorrectBtnServerRpc();
            }
        }

        // Actualizar UI en todos los clientes y el host
        UpdateUI();
    }

    void UpdateUI()
    {
        int seconds = Mathf.CeilToInt(syncedTime.Value);
        txtTime.text = "00:" + seconds.ToString("00");
        txtSpellerTime.text = "00:" + seconds.ToString("00");

        if (timeSlider != null)
            timeSlider.value = (int)selectedTime;

        if (timeLabel != null)
            timeLabel.text = "Tiempo: " + selectedTime.ToString("0") + " segundos";
    }

    public void UpdateTimeLabel(float value)
    {
        
        selectedTime = value;
        syncedTime.Value = selectedTime;
    }

    public void ToggleCrtl(bool enabled)
    {
        if (!IsServer) return;

        startTimer = enabled;

    }



    public void StopTimer()
    {
        startTimer = false;
    }

    public void ResetTimer()
    {
        syncedTime.Value = selectedTime;
    }
}
