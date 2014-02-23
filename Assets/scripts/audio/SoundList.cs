using UnityEngine;
using System.Collections;

public class SoundList : MonoBehaviour 
{
#if !REMOVE_AUDIO
	public AudioClip[] sfxList;
	public AudioClip[] musicList;
#endif 
	
	void Awake()
	{
#if !REMOVE_AUDIO
		SoundManager soundManager = SoundManager.Get();
		
		foreach ( AudioClip clip in sfxList)
			soundManager.sfxClips.Add ( clip ); 
		foreach ( AudioClip clip in musicList )
			soundManager.musicClips.Add ( clip );
#endif 
	}
		
}