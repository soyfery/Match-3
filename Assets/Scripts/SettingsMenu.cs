using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
public class SettingsMenu : MonoBehaviour
    {
        public AudioMixer MasterMixer;
        public Slider Slider;

        void Start()
        {
            Slider.value = PlayerPrefs.GetFloat("Music", 0.75f);

            SetMusicVolume(Slider.value);

            Slider.onValueChanged.AddListener(SetMusicVolume);
        }

        public void SetMusicVolume(float volume)
        {
            MasterMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }
}
