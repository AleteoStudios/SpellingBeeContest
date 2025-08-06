using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    public Button muteButton;
    public Image muteButtonImage;

    public Sprite iconSoundOn;
    public Sprite iconSoundOff;

    private float previousVolume = 1f;
    private bool isMuted = false;

    void Start()
    {
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(SetVolume);

        muteButton.onClick.AddListener(ToggleMute);
        UpdateMuteButtonImage();
    }

    void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        if (volume == 0f)
            isMuted = true;
        else
        {
            isMuted = false;
            previousVolume = volume;
        }

        UpdateMuteButtonImage();
    }

    void ToggleMute()
    {
        if (isMuted)
        {
            AudioListener.volume = previousVolume;
            volumeSlider.value = previousVolume;
            isMuted = false;
        }
        else
        {
            previousVolume = AudioListener.volume;
            AudioListener.volume = 0f;
            volumeSlider.value = 0f;
            isMuted = true;
        }

        UpdateMuteButtonImage();
    }

    void UpdateMuteButtonImage()
    {
        muteButtonImage.sprite = isMuted ? iconSoundOff : iconSoundOn;
    }
}
