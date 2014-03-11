using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MusicPlayMulti : MonoBehaviour 
{
	public List<AudioClip> musicClips;

	public bool overrideVolume = false;
	[Range(0,1)]
	public float volume = 1f;

	public bool onDisableStopMusic = false;

	public const float delay = 1f; // time to wait before playing, this script plays before config is loaded, because of coroutines, so I added this (invadererik)
	public float timeLeftInMusic = 0;	// time until this music ends and the next should be played

	SoundManager soundManager;
	
	void Awake()
	{
		soundManager = SoundManager.Get();	
	}
	
	void Start()
	{
		Random.seed = System.Environment.TickCount;
		if (soundManager == null)
			Awake();
	}

	void Update()
	{
		if(timeLeftInMusic>delay && soundManager.isMusicPlaying())	//.2f gives it some leeway
		{
			timeLeftInMusic -= Time.deltaTime;
		}
		else if(timeLeftInMusic<delay)	
		{
			StartCoroutine( waitThenChooseRandomMusic(delay) );
			timeLeftInMusic=float.MaxValue;	//prevents the starting of more coroutines
		}
	}

	public void ChooseRandomMusic()
	{
		var numClips = musicClips.Count;
		if (numClips > 0)
		{
			AudioClip clip = musicClips[Mathf.FloorToInt(Random.Range(0, numClips - float.Epsilon))] as AudioClip;
			soundManager.playMusic(clip, getVolume());
			timeLeftInMusic = clip.length;
		}
	}

	public int Count
	{
		get { return musicClips.Count; }
	}

	public bool playMusic(int index)
	{
		var result = false;
		if (index >= 0 && index < musicClips.Count)
		{
			soundManager.playMusic(musicClips[index], getVolume());
			timeLeftInMusic = musicClips[index].length;
			result = true;
		}

		return result;
	}

	public float getVolume()
	{
		if (overrideVolume)
		{
			return volume;
		}
		else
		{
			return soundManager.musicVolume;
		}
	}

	IEnumerator waitThenChooseRandomMusic(float time)
	{
		yield return new WaitForSeconds( time+1 );	//"time" is added to clip.length but +1 isn't.  It makes no sense.
		ChooseRandomMusic();
	}

	void OnDisable()
	{
		if ( onDisableStopMusic )
		{
			soundManager.fadeOutMusic( );
		}
	}
}