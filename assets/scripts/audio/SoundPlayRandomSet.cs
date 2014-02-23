using UnityEngine;
using System.Collections;

public class SoundPlayRandomSet : MonoBehaviour
{
	#if !REMOVE_AUDIO
		public AudioClip[] sfxList;
	#endif
	
	public float volume = 1f;
	public bool overrideVolume = false;
	SoundManager soundManager;
	public bool stopOnDisable = false;
	
	public float lowRange = 8f;
	public float highRange = 15f;
	private float waitSeconds; 
	
	void Awake()
	{
		#if !REMOVE_AUDIO
			soundManager = SoundManager.Get();
			
			foreach ( AudioClip clip in sfxList)
				soundManager.sfxClips.Add ( clip ); 
		#endif 
	}
	
	void OnEnable()
	{
		#if !REMOVE_AUDIO
			waitSeconds = UnityEngine.Random.Range( lowRange, highRange);
			StartCoroutine ( waitSomeTime() );
		#endif
	}
	
	void OnDisable()
	{
		#if !REMOVE_AUDIO
			if ( stopOnDisable)
			{
				soundManager.stopPlayingSoundList( sfxList); 
			}
		#endif
	}
	
	private void timerFinished()
	{
		//Debug.Log ( "finished timer");
		
		playRandSoundFromList();
		waitSeconds = UnityEngine.Random.Range( lowRange, highRange);
		StartCoroutine ( waitSomeTime() );
	}
	
	private IEnumerator waitSomeTime( )
	{
		yield return new WaitForSeconds ( waitSeconds );
		timerFinished ();
	}
	
	private void playRandSoundFromList ()
	{
		#if !REMOVE_AUDIO
			soundManager.playSfx( sfxList[ UnityEngine.Random.Range( 0, sfxList.Length )], (overrideVolume)?volume : soundManager.sfxVolume );
		#endif
	}
}