//#define REMOVE_AUDIO //actually removes Audio files from build by stripping the properties ( this should be added to the global defines for it to be totally effective)
//#define MUSIC_DEBUG //logs a bunch of info
//#define AUDIO_DEBUG // logs a bunch of info
//#define AUDIO_OFF // generally mute audio 

using System.Collections.Generic;
using Holoville.HOTween;
using UnityEngine;

public enum SoundInterruptType
{
	DontCare, //plays sound regardless if the same sound is playing or not
	Interrupt, //stops instances of the same sound and plays again from beginning
	DontInterrupt, //if an instance of the sound is playing, dont play the sound
	DontInterruptButInterruptOthers //doesnt interrupt this sound, but does interrupt other sounds
}

public class SoundManager : MonoBehaviour
{
	static private SoundManager _instance = null;
#if !REMOVE_AUDIO
	public AudioClip edx_defaultClip; //if we couldnt find a clip play this instead

	public AudioClip defaultClip;
	public List<AudioClip> sfxClips = new List<AudioClip>();
	public List<AudioClip> musicClips = new List<AudioClip>();
#endif
	private AudioSource musicAudioSourceA;
	private AudioSource musicAudioSourceB;
	private AudioSource currentMusicAudioSource;
	private AudioSource[] sfxAudioSourceArray;
	[Range(0, 1)] public float musicVolume = 1f;
	public float fadeTime = 2;
	[Range(0, 1)] public float sfxVolume = 1f;
	public bool muteAllSound = false;
	public bool muteMusic = false;
	public bool muteSfx = false;
	public int audioPoolSize = 8;
	private List<AudioSource> audioSourcePool = new List<AudioSource>();
	private Dictionary<string,string[]> sfxSets = new Dictionary<string, string[]>();
	private Tweener musicFadeOutTweener;
	private Tweener musicFadeInTweener;

	static public SoundManager Get()
	{
		return _instance;
	}

	void OnDestroy()
	{
		if(musicFadeOutTweener != null)
		{
			musicFadeOutTweener.Kill();
		}
	
		if(musicFadeInTweener != null)
		{
			musicFadeInTweener.Kill();
		}
	
		musicFadeOutTweener = null;
		musicFadeInTweener = null;
	}

	void Awake()
	{
	#if (AUDIO_OFF)
		muteAllSound = true;
	#endif
	
		if ( _instance != null)
		{
			Debug.Log ( "Found a previous SoundManager instance! Destroying this one!");
			Destroy( this);
			return;
		}
		
		_instance = this;
#if !REMOVE_AUDIO
		defaultClip = edx_defaultClip;
#endif
		//Create the music sources (2) for crossFading 
		musicAudioSourceA = gameObject.AddComponent<AudioSource>();
		musicAudioSourceB = gameObject.AddComponent<AudioSource>();
	
		musicAudioSourceA.volume = musicAudioSourceB.volume = musicVolume;
		musicAudioSourceA.loop = musicAudioSourceB.loop = true;
		musicAudioSourceA.rolloffMode = AudioRolloffMode.Linear;
		musicAudioSourceB.rolloffMode = AudioRolloffMode.Linear;
	
		currentMusicAudioSource = musicAudioSourceA;
	
		//initialize the Sfx pool
		int i = 0;
		while(i < 8)
		{
			AudioSource source = gameObject.AddComponent<AudioSource>();
			source.rolloffMode = AudioRolloffMode.Linear;
			audioSourcePool.Add(source);
			++i;
		}
	
		//init tweens so I dont have to keep on checking for nulls below
		musicFadeInTweener = HOTween.To(musicAudioSourceA, 0, new TweenParms().Prop("volume", musicVolume));
		musicFadeOutTweener = HOTween.To(musicAudioSourceB, 0, new TweenParms().Prop("volume", musicVolume));
	}

	private AudioSource getSourceFromPool()
	{
		foreach(AudioSource adSource in audioSourcePool)
		{
			if(!adSource.isPlaying)
			{
				return adSource;
			}
		}
	
		Debug.LogWarning("Audio Source Pool passed limit of " + audioPoolSize + ", stealing sound from first source");
		return  audioSourcePool[0];
	}

#region Play Music	

	public void setMusicVolume(float value)
	{
		musicVolume = value;
		currentMusicAudioSource.volume = value;
	}

	public void setMusicOnOff( bool value) // true = On, false = off 
	{
		if ( !muteMusic == value)
			return; 

		if( value)
		{
			muteMusic = false;
			currentMusicAudioSource.Play();
		}
		else
		{
			pauseMusic();
			muteMusic = true;
		}
	}

	public AudioClip getMusicByName(string name)
	{
#if !REMOVE_AUDIO
		return findAudioInListByName(name, musicClips);
#else
	return null;
#endif
	}

	public int musicListCount
	{
		get
		{
			return musicClips.Count;
		}
	}


	public void pauseMusic()
	{
		if ( currentMusicAudioSource != null)
			currentMusicAudioSource.Pause();
	}
	
	public void unPauseMusic()
	{
		if ( !muteMusic && currentMusicAudioSource != null)
			currentMusicAudioSource.Play();	
	}

	public void playMusic(int musicClipIndex)
	{
		playMusic (musicClipIndex, musicVolume);
	}

	public void playMusic(int musicClipIndex, float volume)
	{
		if (musicClipIndex < 0 || musicClipIndex >= musicClips.Count)
		{
			debugMusic ("Tried to play music index : " + musicClipIndex + " but list size is " + musicClips.Count);
		}
		else
		{
			playMusic (musicClips [musicClipIndex], volume);
		}
	}
	
	public void playMusic(string musicClipName)
	{
		playMusic(musicClipName, musicVolume);	
	}
	
	public AudioSource playSoundNow( AudioClip clip) 
	{
		// this is a test
		//playSfx(clip);
		
		#if (AUDIO_DEBUG)
			Debug.Log ( "playing sound fx:" + clip.name );
		#endif
	
		AudioSource source = getSourceFromPool();
	
		source.volume = sfxVolume;
		source.clip = clip;
		source.Play();
		source.loop = false;
		
		return source;
	}		

	public void playMusic(string musicClipName, float volume)
	{
		if(!muteAllSound && !muteMusic)
		{
			AudioClip musicClip = getMusicByName(musicClipName);
		
			if(musicClip == null)
			{
				debugMusic("Tried to play music : " + musicClipName + " but soundManager could not find it!");
				return;
			}

			playMusic (musicClip, volume);
		}
	}

	public void playMusic(AudioClip musicClip)
	{
		playMusic(musicClip, musicVolume);	
	}

	public void playMusic(AudioClip musicClip, float volume)
	{
		if ( muteMusic )
		{
			currentMusicAudioSource = musicAudioSourceA;
			currentMusicAudioSource.clip = musicClip;
			currentMusicAudioSource.volume = volume;
			currentMusicAudioSource.Play();
			currentMusicAudioSource.Pause();
			return;
		}

		//we have music playing already
		if( currentMusicAudioSource.isPlaying)
		{
			debugMusic("music is aleady playing, checking options");
			
			//check if we are in the middle of fadingIn or Out 
			if(musicFadeOutTweener.IsTweening(musicFadeOutTweener.target) || musicFadeInTweener.IsTweening(musicFadeInTweener.target))
			{
				debugMusic("A Fade is happening...");
				
				//at this point we only care if one of the musics that was fading is the one we want
				//and we want to play it at its current volume back up to 1, the other music we fade out 
				//unless neither of them are the one we want, then do a crossfade
				
				AudioSource fadeOutTarget = null;
				AudioSource fadeInTarget = null;
				
				if(musicFadeInTweener.target != null && ((AudioSource)musicFadeInTweener.target).clip.name == musicClip.name)
				{
					fadeInTarget = (AudioSource)musicFadeInTweener.target;
					
					if(musicFadeOutTweener.target != null)
					{
						fadeOutTarget = (AudioSource)musicFadeOutTweener.target;
					}
				}
				
				if(fadeInTarget == null && musicFadeOutTweener.target != null && ((AudioSource)musicFadeOutTweener.target).clip.name == musicClip.name)
				{
					fadeInTarget = (AudioSource)musicFadeOutTweener.target;
					
					if(musicFadeInTweener.target != null)
					{
						fadeOutTarget = (AudioSource)musicFadeOutTweener.target;
					}
				}
				
				//clean up faders 
				musicFadeInTweener.Kill();
				musicFadeOutTweener.Kill();
				
				if(fadeOutTarget != null)
				{
					debugMusic("Fading out:" + fadeOutTarget.clip.name);
					fadeOutMusic(fadeOutTarget);
				}
				
				if(fadeInTarget != null)
				{
					debugMusic("Fading in:" + fadeInTarget.clip.name);
					doMusicPlay(musicClip, fadeInTarget.volume, volume);	
				}
				
				//make sure the audioSources match up
				if(fadeOutTarget == null)
				{
					if(fadeInTarget != null)
					{
						AudioSource otherSource = (fadeInTarget == musicAudioSourceA) ? musicAudioSourceB : musicAudioSourceA;
						if(otherSource.clip != null)
						{
							debugMusic("Stopping music :" + otherSource.clip.name);
							otherSource.Stop();
						}
					}
				}
				
				//we want a new clip to play if neither of the fades where for this clip
				if(fadeInTarget == null)
				{
					doMusicCrossFade(musicClip, volume);
				}
			}
			//check if its the same clip that is already playing
			else if(currentMusicAudioSource.clip.name == musicClip.name)
			{
				debugMusic("The same music is playing already, so we will ignore play.");
				return; //this could be some other behavior like restarting it or something 
			}
			//another music is playing, crossfade from one music to another
			else
			{
				debugMusic("Another music is playing, we will do a standard crossfade.");
				doMusicCrossFade(musicClip, volume);
			}
		}
		//nothing was playing fade in new music
		else if ( !muteMusic)
		{
			debugMusic("no music was playing, playing: " + musicClip.name);
			doMusicPlay(musicClip, volume);
		}
	}
	
	private void doMusicCrossFade(AudioClip musicClip, float endVolume)
	{
		doMusicCrossFade(musicClip, 0f, endVolume);
	}

	private void doMusicCrossFade(AudioClip musicClip, float startVolume, float endVolume)
	{
		debugMusic("crossfading from: " + currentMusicAudioSource.clip.name + " to: " + musicClip.name);
	
		//clean up faders 
		musicFadeInTweener.Kill();
		musicFadeOutTweener.Kill();
	
		fadeOutMusic(currentMusicAudioSource);
	
		AudioSource nextMusicAudioSource = (currentMusicAudioSource == musicAudioSourceA) ? musicAudioSourceB : musicAudioSourceA;
		nextMusicAudioSource.clip = musicClip;
		nextMusicAudioSource.volume = startVolume;
		nextMusicAudioSource.Play();
		musicFadeInTweener = HOTween.To(nextMusicAudioSource, fadeTime, new TweenParms().Prop("volume", endVolume).OnComplete(musicCrossFadeComplete, nextMusicAudioSource));
	
		currentMusicAudioSource = nextMusicAudioSource;
	}

	private void musicCrossFadeComplete(TweenEvent tweenEvent)
	{
		debugMusic("Done with crossfade current music is now: " + currentMusicAudioSource.clip.name + " ,vol:" + currentMusicAudioSource.volume);//((AudioSource)tweenEvent.parms[0]).clip.name );
		/*if ( musicFadeOutTweener.isComplete && musicFadeInTweener.isComplete )
	{	
		currentMusicAudioSource = ((AudioSource)tweenEvent.parms[0]);
	}*/
	}

	private void doMusicPlay(AudioClip musicClip, float startVolume, float endVolume)
	{
		debugMusic("playing music: " + musicClip.name);
	
		currentMusicAudioSource.clip = musicClip;
		currentMusicAudioSource.volume = startVolume;
		currentMusicAudioSource.Play();
		musicFadeInTweener = HOTween.To(currentMusicAudioSource, fadeTime, new TweenParms().Prop("volume", endVolume).Ease(EaseType.EaseInCubic));
	}

	private void doMusicPlay(AudioClip musicClip, float volume)
	{
		doMusicPlay(musicClip, 0f, volume);
	}

	public void stopMusic()
	{
		currentMusicAudioSource.Stop();
	}
	
	public void stopMusicByName( string name )
	{
		if (currentMusicAudioSource != null && currentMusicAudioSource.clip != null && currentMusicAudioSource.clip.name == name )
		{
			currentMusicAudioSource.Stop();
		}
	}

	public void fadeOutMusic()
	{
		fadeOutMusic(currentMusicAudioSource);
	}

	public void fadeOutMusic(AudioSource musicSource)
	{
		if(musicSource == null || musicSource.clip == null)
		{
			return;
		}
		debugMusic("fading out music: " + musicSource.clip.name);
	
		musicFadeOutTweener = HOTween.To(musicSource, fadeTime, new TweenParms().Prop("volume", 0f).OnComplete(this.doneMusicFade, musicSource));
	}
			
	public void doneMusicFade(TweenEvent tweenEvent)
	{
		AudioSource fadedTargetAudioSource = ((AudioSource)tweenEvent.parms[0]);
		if(fadedTargetAudioSource.volume < .1f && fadedTargetAudioSource != (musicFadeInTweener.target as AudioSource))
		{
			if(fadedTargetAudioSource.clip != null)
			{
				debugMusic("done music fade stoping music: " + fadedTargetAudioSource.clip.name);
				fadedTargetAudioSource.Stop(); 
			}
		}
		else
		{
			debugMusic("music Fade is done, but volume is above .1(" + fadedTargetAudioSource.volume + "} so wasnt killed.");
		}
	}

	private void debugMusic(string output)
	{	
	#if (MUSIC_DEBUG)
		Debug.Log(output);
	#endif
	}
#endregion

#region Play Sound Effects

	public void setSoundVolume(float value)
	{
		sfxVolume = value;
		foreach( AudioSource clip in audioSourcePool)
		{
			clip.volume = value;
		}	
	}

	public void setSoundOnOff( bool value) // true = On, false = off 
	{
		if ( !muteSfx == value)
			return; 

		if ( value)
		{
			muteSfx = false;
		}
		else
		{
			stopAllPlayingSounds();
			muteSfx = true;
		}
	}

	public void stopAllPlayingSounds()
	{
		//I havent actually tested if this works correctly, if it doesnt just stoping all 
		//sounds in the list directly is probably easy too
		playSfx( "notreallyanysound", 1f, SoundInterruptType.DontInterruptButInterruptOthers );
	}
	
	public void createSet(string setName, string[] clipNames)
	{
		sfxSets.Add(setName, clipNames);
	}

	public void playRandomSfxFromSet(string setName)
	{
		playRandomSfxFromSet(setName, sfxVolume);
	}

	public void playRandomSfxFromSet(string setName, float volume)
	{
		if(!muteAllSound && !muteSfx)
		{
			AudioClip clip = getSfxByName(sfxSets["setName"][UnityEngine.Random.Range(0, sfxSets["setName"].Length)]);
			playSfx(clip.name, volume);
		}
	}

	public void playSfx(string clipName)
	{
		playSfx(clipName, sfxVolume);
	}

	public void playSfx(string clipName, float volume)
	{
		playSfx(clipName, volume, SoundInterruptType.DontCare);
	}

	public void playSfx(string clipName, float volume, SoundInterruptType interruptType)
	{
		playSfx(clipName, volume, interruptType, new string[0]);
	}

	public void playSfx(string clipName, float volume, SoundInterruptType interruptType, string[] alsoInterrupt)
	{
#if !REMOVE_AUDIO
		AudioClip theClip = findAudioInListByName(clipName, sfxClips);
	
		if(theClip == defaultClip) //if we cant find the audio in list by that name, attempt to find the closest possible
		{
			theClip = findClosestAudioInListByName(clipName);
		}
	
		if(interruptType == SoundInterruptType.DontInterrupt || interruptType == SoundInterruptType.DontInterruptButInterruptOthers)
		{
			playSfx(theClip, volume, interruptType, new List<string>(alsoInterrupt), clipName);	
		}
		else
		{
			playSfx(theClip, volume, interruptType, new List<string>(alsoInterrupt));
		}
	
#endif
	}

	public void playSfx(AudioClip soundClip, float volume)
	{
		playSfx(soundClip, volume, SoundInterruptType.DontCare, new List<string>());
	}

	public void playSfx(AudioClip soundClip, float volume, SoundInterruptType interruptType, string[] alsoInterrupt)
	{
		playSfx(soundClip, volume, SoundInterruptType.DontCare, new List<string>(alsoInterrupt));
	}

	public void playSfx(AudioClip soundClip, float volume, SoundInterruptType interruptType, List<string> alsoInterrupt)
	{
		playSfx(soundClip, volume, interruptType, alsoInterrupt, "");
	}

	public void playSfx(AudioClip soundClip, float volume, SoundInterruptType interruptType, List<string> alsoInterrupt, string dontInterrupt)
	{		
		if(!muteAllSound && !muteSfx)
		{
			switch(interruptType)
			{
			case SoundInterruptType.DontCare:			
				play(soundClip, volume);
				break;
			case SoundInterruptType.DontInterrupt:
				playClipOnlyIfSimilarNotPlaying(soundClip, dontInterrupt, volume);
				break;
			case SoundInterruptType.Interrupt:
				stopPlayingSoundList(alsoInterrupt, soundClip.name);
				play(soundClip, volume);	
				break;
			case SoundInterruptType.DontInterruptButInterruptOthers:
				stopPlayingSoundList(alsoInterrupt);
				playClipOnlyIfSimilarNotPlaying(soundClip, dontInterrupt, volume);
				break;
			}
		}
	}

	private void playClipOnlyIfSimilarNotPlaying(AudioClip soundClip, string abridgedClipName, float volume)
	{
		if(abridgedClipName.Length > 1)
		{
			bool similarClipPlaying = false;
			List<AudioClip> dontInterruptClipNames = findClosestAudioInListByName(abridgedClipName, true);
			foreach(AudioClip dontInterruptClipName in dontInterruptClipNames)
			{
				if(SfxIsPlaying(dontInterruptClipName))
				{
					similarClipPlaying = true;
				}
			}
			if(similarClipPlaying == false)
			{
				play(soundClip, volume);	
			}
		}
		else
		{
			if(!SfxIsPlaying(soundClip))
			{
				play(soundClip, volume);
			}
		}
	}

	public void stopPlayingSoundList(List<string> alsoInterrupt)
	{
		stopPlayingSoundList(alsoInterrupt, "thisisnotreallyasound");
	}

	public void stopPlayingSoundList(AudioClip[] alsoInterrupt)
	{
		List<string> interruptNames = new List<string>();
		foreach(AudioClip clip in alsoInterrupt)
		{
			interruptNames.Add(clip.name);
		}
		stopPlayingSoundList(interruptNames, "thisisnotreallyasound");
	}

	public void stopPlayingSoundList(List<AudioClip> alsoInterrupt)
	{
		List<string> interruptNames = new List<string>();
		foreach(AudioClip clip in alsoInterrupt)
		{
			interruptNames.Add(clip.name);
		}
		stopPlayingSoundList(interruptNames, "thisisnotreallyasound");
	}

	public void stopPlayingSoundList(List<string> alsoInterrupt, string callingSoundName)
	{
		alsoInterrupt.Add(callingSoundName);
	
		foreach(string soundName in alsoInterrupt)
		{
		#if (AUDIO_DEBUG)
			//Debug.Log ( "calling sound name when stopping sounds:" + callingSoundName + " : " + soundName);
		#endif

			List<AudioClip> audioClips = findClosestAudioInListByName(soundName, true);
			foreach(AudioClip audioClip in audioClips)
			{
				if(SfxIsPlaying(audioClip))
				{		
				#if (AUDIO_DEBUG)
					Debug.Log ( "stopping sound fx:" + audioClip.name );
				#endif
					getPlayingSfxSource(audioClip).Stop();
				}
			}
		}
	}

	public bool SfxIsPlaying(AudioClip soundClip)
	{
		foreach(AudioSource audioSource in audioSourcePool)
		{
			if(audioSource.clip == soundClip && audioSource.isPlaying)
			{
				return true;
			}
		}
	
		return false;
	}

	public AudioSource getPlayingSfxSource(AudioClip soundClip)
	{
		foreach(AudioSource audioSource in audioSourcePool)
		{
			if(audioSource.clip == soundClip)
			{
				return audioSource;
			}
		}
	
		return null;	
	}

	public void playSfx(AudioClip soundClip)
	{		
		if(!muteAllSound && !muteSfx)
		{
			playSfx(soundClip, sfxVolume, SoundInterruptType.DontCare, new List<string>());
		}
	}

	public AudioClip getSfxByName(string name)
	{
#if !REMOVE_AUDIO
		return findAudioInListByName(name, sfxClips);
#else
	return null;
#endif
	}

	private AudioClip findAudioInListByName(string name, List<AudioClip>theList)
	{ 
		foreach(AudioClip clip in theList)
		{
			if(clip && clip.name == name)
			{
				return clip;
			}
		}
#if !REMOVE_AUDIO
		return defaultClip;
#else
	return null;
#endif
	}

	private void play(AudioClip clip, float volume)
	{
	#if (AUDIO_DEBUG)
		Debug.Log ( "playing sound fx:" + clip.name );
	#endif
	
		AudioSource source = getSourceFromPool();
	
		source.volume = volume;
		source.clip = clip;
		source.Play();
		source.loop = false;
	}

	private AudioClip findClosestAudioInListByName(string clipName)
	{
		return findClosestAudioInListByName(clipName, false)[0];
	}

	// tries to find a clip thats similar, if it finds one thats clipname_mr or clipName_fr
	// if it finds clipName_01  etc, it will randomly pick one from the set
	// returns a list, most times a list with 1 clip
	private List<AudioClip> findClosestAudioInListByName(string clipName, bool returnCompleteList)
	{
		List<AudioClip> randomSet = new List<AudioClip>();

#if !REMOVE_AUDIO
		AudioClip genderClip = defaultClip;
	
		List<AudioClip> genderSet = new List<AudioClip>();
	
		foreach(AudioClip aClip in sfxClips)
		{
			if(aClip != null && aClip.name.IndexOf(clipName) > -1)
			{
				if((aClip.name.Contains("_mr") || aClip.name.Contains("_male")) )
				{
					genderClip = aClip;
					genderSet.Add(aClip);
					continue;
				}

				if((aClip.name.Contains("_fr") || aClip.name.Contains("_female")) )
				{
					genderClip = aClip;
					genderSet.Add(aClip);
					continue;
				}
			
				randomSet.Add(aClip);
			}
		}
	
		if(genderSet.Count > 1)
		{
			if(returnCompleteList)
			{
				return genderSet;
			}
		
			return new List<AudioClip>(){genderSet[UnityEngine.Random.Range(0, genderSet.Count)]};
		}
	
		if(genderClip != defaultClip)
		{
			return new List<AudioClip>(){genderClip};
		}
	
		if(randomSet.Count > 0)
		{
			if(returnCompleteList)
			{
				return randomSet;
			}
		
			return new List<AudioClip>(){ randomSet[UnityEngine.Random.Range(0, randomSet.Count)]};
		}
			
		return new List<AudioClip>(){defaultClip};
#else
	return null;
#endif
	}

	public void playSfx3d(GameObject target, string name, float minDistance, float maxDistance, float volumeScale)
	{		
 	#if !REMOVE_AUDIO
		if(!muteAllSound && !muteSfx)
		{
			AudioSource[] sources = target.GetComponents<AudioSource>();
			AudioSource source = null;
			foreach(AudioSource s in sources){
				if(s.clip.name==name){
					source = s;
					break;
				}
			}
			if(source == null)
			{
				source = target.AddComponent<AudioSource>();
			}
			else if(source.clip && source.clip.name != name)
			{
				source = target.AddComponent<AudioSource>(); 
			}
			
			if(source.clip == null)
			{
				source.clip = findAudioInListByName(name, sfxClips);
			}
		
			if(source.isPlaying)
			{
				return;
			}
			
			source.rolloffMode = AudioRolloffMode.Linear;

			source.minDistance = minDistance;
			source.maxDistance = maxDistance;
			source.PlayOneShot(source.clip, sfxVolume*volumeScale);
		}
	#endif
	}	
#endregion
}
