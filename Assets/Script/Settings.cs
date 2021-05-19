using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("Sound Settings")]
    public Text musicText;
    public Slider musicSlider;
    public Text sfxText;
    public Slider sfxSlider;

    private float defaultMusicVolume = 0.2f;
    private float defaultSfxVolume = 0.4f;

    public void Start()
    {
        musicSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("musicVol", defaultMusicVolume);
        sfxSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("sfxVol", defaultSfxVolume);
    }

    public void OnMusicSliderChange()
    {
        var value = musicSlider.GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("musicVol", value);
        musicText.text = ((int)(value * 100)).ToString();
    }

    public void OnSfxSliderChange()
    {
        var value = sfxSlider.GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("sfxVol", value);
        sfxText.text = ((int)(value * 100)).ToString();
    }

    public void OnReset()
    {
        musicSlider.GetComponent<Slider>().value = defaultMusicVolume;
        sfxSlider.GetComponent<Slider>().value = defaultSfxVolume;
    }
}
