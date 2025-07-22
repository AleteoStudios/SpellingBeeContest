using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class Timer : NetworkBehaviour
{
    public Slider timeSlider;
    public TextMeshProUGUI timeLabel;



    [SerializeField] private NetworkVariable<float> time = new NetworkVariable<float>(
        value: 0f,
        writePerm: NetworkVariableWritePermission.Server);

    [SerializeField] private TextMeshProUGUI txtTime;
    public TextMeshProUGUI txtTimeCnt;
    public TextMeshProUGUI txtTimeEva;

    public Toggle toggle;

    public GameObject panelIncorrect;
    public Image imageStatus;
    public Sprite spriteIncorrect;

    
    [SerializeField]
    private NetworkVariable<bool> startTimer = new NetworkVariable<bool>(value: false,
        writePerm: NetworkVariableWritePermission.Server);

    private int selectedTime;

    void Start()
    {
        txtTime.text = "00:25";
        txtTimeCnt.text = "00:25";
        txtTimeEva.text = "00:25";
        timeSlider.onValueChanged.AddListener(UpdateTimeLabel);


    }

    void UpdateTimeLabel(float value)
    {

        selectedTime = (int)value;
        timeLabel.text = "Tiempo: " + selectedTime.ToString("0") + " segundos";
    }

    public void ToggleCrtl(bool enabled)
    {
        startTimer.Value = enabled;
    }

    public void StopTimer()
    {
        startTimer.Value = false;
    }

    private void Update()
    {
        if (!startTimer.Value)
        {
            time.Value = selectedTime;
        }

        UpdateTimeLabel(timeSlider.value);

        if (Input.GetKeyDown("up"))
        {
            startTimer.Value = true;
        }

        if (Input.GetKeyDown("down"))
        {
            startTimer.Value = false;
        }

        if (startTimer.Value == true)
        {
            StartTimer();
        }

        if (time.Value <= 0)
        {
            startTimer.Value = false;
            toggle.isOn = false;
            ResetTimer();
            panelIncorrect.SetActive(true);
            imageStatus.sprite = spriteIncorrect;
        }

        int seconds = (int)time.Value;

        txtTime.text = "00" + ":" + seconds.ToString().PadLeft(2, '0');
        txtTimeCnt.text = "00" + ":" + seconds.ToString().PadLeft(2, '0');
        txtTimeEva.text = "00" + ":" + seconds.ToString().PadLeft(2, '0');


    }

    public void StartTimer()
    {

        time.Value -= Time.deltaTime;
        int seconds = (int)time.Value;

        txtTime.text = "00" + ":" + seconds.ToString().PadLeft(2, '0');
        txtTimeCnt.text = "00" + ":" + seconds.ToString().PadLeft(2, '0');
        txtTimeEva.text = "00" + ":" + seconds.ToString().PadLeft(2, '0');
    }

    public void ResetTimer()
    {
        time.Value = selectedTime;
    }


}
