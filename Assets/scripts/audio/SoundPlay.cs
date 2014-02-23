using UnityEngine;
using System.Collections;

public class SoundPlay : MonoBehaviour 
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
	public SoundInterruptType interruptType = SoundInterruptType.DontCare;
	public string[] alsoInterruptSoundList;
	
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
			playOrAddClip(onStartClip);
		}
#endif
		if ( onStartClipName != "")
		{
			soundManager.playSfx( onStartClipName, (overrideVolume)?volume : soundManager.sfxVolume, interruptType, alsoInterruptSoundList);
		}
	}
	
	void OnEnable ()
	{
#if !REMOVE_AUDIO
		if (onEnableClip != null)
		{
			playOrAddClip (onEnableClip);
		}
#endif 
		
		if ( onEnableClipName != "")
		{
			soundManager.playSfx( onEnableClipName, (overrideVolume)?volume : soundManager.sfxVolume, interruptType, alsoInterruptSoundList);
		}
	}
	
	void OnDisable()
	{
#if !REMOVE_AUDIO
		if (onDisableClip != null)
		{
			playOrAddClip (onDisableClip);
		}
#endif
		
		if ( onDisableClipName != "")
		{
			soundManager.playSfx( onDisableClipName, (overrideVolume)?volume : soundManager.sfxVolume, interruptType, alsoInterruptSoundList );
		}
	}
	
	void playOrAddClip ( AudioClip aClip)
	{
#if !REMOVE_AUDIO
		AudioClip soundClip = soundManager.getSfxByName( aClip.name) ;
		if ( soundClip != soundManager.defaultClip  )
		{
			soundManager.playSfx (aClip.name, (overrideVolume)? volume : soundManager.sfxVolume, interruptType, alsoInterruptSoundList);
		}
		else
		{
			soundManager.sfxClips.Add ( aClip);
			soundManager.playSfx( aClip.name, (overrideVolume)? volume : soundManager.sfxVolume, interruptType, alsoInterruptSoundList);
		}
#endif
	}
}