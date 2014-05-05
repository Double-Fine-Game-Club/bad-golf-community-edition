using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarAudio : MonoBehaviour {

	// This script reads some of the car's current properties and plays sounds accordingly.
	// The engine sound can be a simple single clip which is looped and pitched, or it
	// can be a crossfaded blend of four clips which represent the timbre of the engine
	// at different RPM and Throttle state.

	// the engine clips should all be a steady pitch, not rising or falling.

	// when using four channel engine crossfading, the four clips should be:
	// lowAccelClip : The engine at low revs, with throttle open (i.e. begining acceleration at very low speed)
	// highAccelClip : Thenengine at high revs, with throttle open (i.e. accelerating, but almost at max speed)
	// lowDecelClip : The engine at low revs, with throttle at minimum (i.e. idling or engine-braking at very low speed)
	// highDecelClip : Thenengine at high revs, with throttle at minimum (i.e. engine-braking at very high speed)

	// For proper crossfading, the clips pitches should all match, with an octave offset between low and high.


	public enum EngineAudioOptions                                                      // Options for the engine audio      
	{
		Simple,                                                                         // Simple style audio
		FourChannel                                                                     // four Channel audio
	}

	public Camera followCamera;																// reference to the follow camera
	public EngineAudioOptions engineSoundStyle = EngineAudioOptions.FourChannel;        // Set the default audio options to be four channel
	public AudioClip lowAccelClip;                                                      // Audio clip for low acceleration
    public AudioClip lowDecelClip;                                                      // Audio clip for low deceleration
    public AudioClip highAccelClip;                                                     // Audio clip for high acceleration
    public AudioClip highDecelClip;                                                     // Audio clip for high deceleration
    public AudioClip skidClip;                                                          // Audio clip for the car skidding
	public float pitchMultiplier = 1f;                                                  // Used for altering the pitch of audio clips
	public float lowPitchMin = 1f;                                                      // The lowest possible pitch for the low sounds
	public float lowPitchMax = 6f;                                                      // The highest possible pitch for the low sounds
	public float highPitchMultiplier = 0.25f;                                           // Used for altering the pitch of high sounds
	public float maxRolloffDistance = 500;                                              // The maximum distance where rollof starts to take place
	public float dopplerLevel = 1;                                                      // The mount of doppler effect used in the audio
	public bool useDoppler = true;                                                      // Toggle for using doppler

	AudioSource lowAccel;                                                               // Source for the low acceleration sounds
    AudioSource lowDecel;                                                               // Source for the low deceleration sounds
    AudioSource highAccel;                                                              // Source for the high acceleration sounds
    AudioSource highDecel;                                                              // Source for the high deceleration sounds
    AudioSource skidSource;                                                             // Source for the low acceleration sounds
	bool startedSound;                                                                  // flag for knowing if we have started sounds
    CarController carController;                                                        // Reference to car we are controlling

	SoundManager sm;
	AudioListener al;
	networkVariables nvs;

	void Awake(){
		sm = SoundManager.Get();
	}

	void Destroy(){
		//Remove audioSources from al
		Destroy (highAccel);
		Destroy (skidSource);
		if (engineSoundStyle == EngineAudioOptions.FourChannel) {
			Destroy (lowAccel);
			Destroy (lowDecel);
			Destroy (highDecel);
		}
	}

	void Start(){
		if(sm==null) this.Awake();
		al = FindObjectOfType<AudioListener> ();
		nvs = GameObject.FindWithTag("NetObj").GetComponent("networkVariables") as networkVariables;

		// get the carcontroller ( this will not be null as we have require component)
		carController = GetComponent<CarController>();

		highAccel = addAudioSource(highAccelClip);
		if (engineSoundStyle == EngineAudioOptions.FourChannel) {
			lowAccel = addAudioSource(lowAccelClip);
			lowDecel = addAudioSource(lowDecelClip);
			highDecel = addAudioSource(highDecelClip);
		}
		skidSource = addAudioSource(skidClip);
		

	}

	AudioSource addAudioSource(AudioClip clip){
		AudioSource source = null;
		if(nvs.gameMode==GameMode.Online){
			//The sound moves around with the object it is attached to
			source = gameObject.AddComponent<AudioSource> ();
		}else{	//GameMode.Local
			//The sound can be heard anywhere
			source = al.gameObject.AddComponent<AudioSource> ();
		}
		source.clip = clip;
		return source;
	}

	void StartSound () {

        // setup the simple audio source
		SetUpEngineAudioSource(highAccel);

        // if we have four channel audio setup the four audio sources
		if (engineSoundStyle == EngineAudioOptions.FourChannel)
		{
			SetUpEngineAudioSource(lowAccel);
			SetUpEngineAudioSource(lowDecel);
			SetUpEngineAudioSource(highDecel);
		}

        // setup the skid sound source
		SetUpEngineAudioSource(skidSource);
		
        // flag that we have started the sounds playing
		startedSound = true;
	}



	void StopSound()
	{
        //Stop all audio sources on this object:
		highAccel.Stop ();
		if (engineSoundStyle == EngineAudioOptions.FourChannel)
		{
			lowAccel.Stop();
			lowDecel.Stop();
			highDecel.Stop();
		}
		skidSource.Stop ();

		startedSound = false;
	}
	
	// Update is called once per frame
	void Update () {
		bool isMuted = sm.muteSfx || sm.muteAllSound;

        // get the distance to main camera
		float camDist = (followCamera.transform.position-transform.position).sqrMagnitude;

        // stop sound if the object is beyond the maximum roll off distance
		if (startedSound && (camDist > maxRolloffDistance*maxRolloffDistance || isMuted))
		{
			StopSound ();
		}

        // start the sound if not playing and it is nearer than the maximum distance
		if (!startedSound && camDist < maxRolloffDistance*maxRolloffDistance && !isMuted)
		{
			StartSound();
		}

		if (startedSound) {
			float masterVolume = sm.sfxVolume;

            // The pitch is interpolated between the min and max values, according to the car's revs.
			float pitch = ULerp (lowPitchMin,lowPitchMax,carController.RevsFactor);
			
            // clamp to minimum pitch (note, not clamped to max for high revs while burning out)
			pitch = Mathf.Min(lowPitchMax,pitch);

			if (engineSoundStyle == EngineAudioOptions.Simple)
			{
				// for 1 channel engine sound, it's oh so simple:
                highAccel.pitch = pitch*pitchMultiplier*highPitchMultiplier;
				highAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
				highAccel.volume = 1 * masterVolume;
			} else {

                // for 4 channel engine sound, it's a little more complex:

                // adjust the pitches based on the multipliers
				lowAccel.pitch = pitch*pitchMultiplier;
				lowDecel.pitch = pitch*pitchMultiplier;
				highAccel.pitch =  pitch*highPitchMultiplier*pitchMultiplier;
				highDecel.pitch = Mathf.Min (1, pitch*highPitchMultiplier*pitchMultiplier);

                // get values for fading the sounds based on the acceleration
				float accFade = Mathf.Abs (carController.AccelInput);
				float decFade = 1-accFade;
				
                // get the high fade value based on the cars revs
				float highFade = Mathf.InverseLerp(0.2f,0.8f,carController.RevsFactor);
				float lowFade = 1-highFade;

                // adjust the values to be more realistic
				highFade = 1-((1-highFade)*(1-highFade));
				lowFade = 1-((1-lowFade)*(1-lowFade));
				accFade = 1-((1-accFade)*(1-accFade));
				decFade = 1-((1-decFade)*(1-decFade));

                // adjust the source volumes based on the fade values
				lowAccel.volume = lowFade*accFade * masterVolume;
				lowDecel.volume = lowFade*decFade * masterVolume;
				highAccel.volume = highFade*accFade * masterVolume;
				highDecel.volume = highFade*decFade * masterVolume;
				
                // adjust the doppler levels
				highAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
				lowAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
				highDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
				lowDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
			}

            // adjust the skid source based on the cars current skidding state
			skidSource.volume = Mathf.Clamp01(carController.AvgSkid * 3 - 1) * masterVolume;
			skidSource.pitch = Mathf.Lerp (0.8f, 1.3f, carController.SpeedFactor);
			skidSource.dopplerLevel = useDoppler ? dopplerLevel : 0;
		}
	}

	
    // sets up and adds new audio source to the game object
	void SetUpEngineAudioSource(AudioSource source)
	{
        // reset the audio source component properties
		source.volume = 0;
		source.loop = true;

        // start the clip from a random point
		source.time = Random.Range(0f, source.clip.length);
		source.Play();
		source.minDistance = 5;
		source.maxDistance = maxRolloffDistance;
		source.dopplerLevel = 0;
		return;
	}

	// unclamped versions of Lerp and Inverse Lerp, to allow value to exceed the from-to range
	float ULerp (float from, float to, float value) {
		return (1.0f - value)*from + value*to;
	}
	
	float UInverseLerp (float from, float to, float value) {
		return (value - from) / (to - from);
	}
}
