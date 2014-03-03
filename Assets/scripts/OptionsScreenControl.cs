using UnityEngine;
using System.Collections;

public class OptionsScreenControl : MonoBehaviour 
{
	public SliderObject ed_musicSlider;
	public SliderObject ed_soundSlider;
	public CheckBoxGambeObject ed_musicCheckBox;
	public CheckBoxGambeObject ed_soundCheckBox;

	// Use this for initialization
	void OnEnable () 
	{
		ed_musicSlider.sliderValue = SoundManager.Get().musicVolume;
		ed_soundSlider.sliderValue = SoundManager.Get().sfxVolume;

		ed_soundCheckBox.setCheckBox( !SoundManager.Get().muteSfx );
		ed_musicCheckBox.setCheckBox( !SoundManager.Get().muteMusic );
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
