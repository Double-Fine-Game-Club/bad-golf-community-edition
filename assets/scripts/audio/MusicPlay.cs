using UnityEngine;
using System.Collections;

public class MusicPlay : MonoBehaviour 
{
	public string onStartClipName = "";
	public string onEnableClipName = "";
	public string onDisableClipName = "";
	
#if !REMOVE_AUDIO
	public AudioClip onStartClip;
	public AudioClip onEnableClip;
	public AudioClip onDisableClip;
#endif
	
	public float volume = 1f;
	public bool overrideVolume = false;
	
	public bool onDisableStopMusic = false;

	SoundManager soundManager;
	
	void Awake()
	{
		soundManager = SoundManager.Get();	
	}
	
	void Start()
	{
		if (soundManager == null)
			Awake();
#if !REMOVE_AUDIO
		if (onStartClip != null)
		{
			playOrAddClip (onStartClip);
		}
#endif
		if ( onStartClipName != "")
		{
			soundManager.playMusic( onStartClipName, (overrideVolume)?volume : soundManager.sfxVolume);
		}
	}
	
	void OnEnable ()
	{
#if !REMOVE_AUDIO
		if (onEnableClip)
		{
			playOrAddClip (onEnableClip);
		}
#endif
		
		if ( onEnableClipName != "")
		{
			soundManager.playMusic( onEnableClipName, (overrideVolume)?volume : soundManager.sfxVolume);
		}
	}
	
	void OnDisable()
	{
#if !REMOVE_AUDIO
		if (onDisableClip)
		{
			playOrAddClip (onDisableClip);
		}
#endif
		
		if ( onDisableClipName != "")
		{
			soundManager.playMusic( onDisableClipName, (overrideVolume)?volume : soundManager.sfxVolume);
		}
		
		if ( onDisableStopMusic )
		{
			soundManager.fadeOutMusic( );
		}
	}
	
	void playOrAddClip ( AudioClip aClip)
	{
#if !REMOVE_AUDIO
		AudioClip musicClip = soundManager.getMusicByName(aClip.name);
			
		if ( musicClip != soundManager.defaultClip )
		{
			soundManager.playMusic (aClip.name, (overrideVolume)? volume : soundManager.musicVolume );
		}
		else
		{
			soundManager.musicClips.Add ( aClip);
			soundManager.playMusic (aClip.name, (overrideVolume)? volume : soundManager.musicVolume );
		}
#endif
	}
}