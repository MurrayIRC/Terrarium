﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

#region Audio Controller
/// <summary>
/// Audio controller used to setup Audio Sources in scene.
/// </summary>
/// <remarks>
/// Still needs delayed playback (needs to be made into monobehaviour for that)
/// Needs safe transform access for 3D sound.
/// </remarks>
[System.Serializable]
public class AudioController
{
    public string ControllerName = "";

	private AudioSource _source = null;
    public AudioSource Source { get { return _source; } }

	[SerializeField] private List<AudioClip> _audioClipList;
	public AudioClip MainClip { set { _audioClipList[0] = value; _source.clip = _audioClipList[0]; } }
	public void SelectClip( int index )
	{
		Debug.Assert( index >= 0 && index < _audioClipList.Count );
		_source.clip = _audioClipList[index];
	}
	public int ClipCount { get { return _audioClipList.Count; } }

	[SerializeField] private AudioMixerGroup _mixerGroup = null;
	public AudioMixerGroup MixerGroup { set { _mixerGroup = value; _source.outputAudioMixerGroup = _mixerGroup; } }

	[SerializeField, Range(0.0f, 1.0f)] private float _volume = 1.0f;
	public float Volume { get { return _source.volume; } set { _volume = value; _source.volume = _volume; } }

	[SerializeField] private bool _playOnAwake = false;
	public bool PlayOnAwake { set { _playOnAwake = value; _source.playOnAwake = _playOnAwake; } }

	[SerializeField] private bool _loop = false;
	public bool Loop { set { _loop = value; _source.loop = _loop; } }

    [SerializeField] private float _pitch = 1f;
    public float Pitch { set { _pitch = value; _source.pitch = _pitch; } }

	public AudioController()
	{
		_audioClipList = null;
		_mixerGroup = null;
		_volume = 1.0f;
		_playOnAwake = false;
		_loop = false;
	    _pitch = 1.0f;
	}

	public void Initialize(AudioSource source)
	{
		_source = source;

		_source.clip = _audioClipList[0];
		_source.outputAudioMixerGroup = _mixerGroup;
		_source.volume = _volume;
		_source.playOnAwake = _playOnAwake;
		_source.loop = _loop;
	    _source.pitch = _pitch;
	}

	#region AudioSource Methods

	public void StopAudioSource()
	{
		_source.Stop();
	}

	public void PauseAudioSource()
	{
		_source.Pause();
	}

	public void PlayAudioSource()
	{
		if( !_source.isPlaying )
		{
			// Just get some randomness to all sounds.
			_source.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
			
			_source.Play();	
		}
	}

	public void PlayRandomClip()
	{
        if( !_source.isPlaying )
        {
            _source.clip = _audioClipList[UnityEngine.Random.Range( 0, _audioClipList.Count )];

            _source.Play();
        }

	}

	public void PlaySpecificClip(int clipIndex)
	{
		if (!_source.isPlaying)
		{
			clipIndex = Mathf.Clamp(clipIndex, 0, _audioClipList.Count);
			_source.clip = _audioClipList[clipIndex];
			_source.Play();
		}
	}

	#endregion
}

#endregion

public class AudioManager : SingletonBehaviour<AudioManager> {

	public enum AudioControllerNames
	{
        MUSIC = 0,
	    PLAYER_SING = 1,
		PLAYER_FOOTSTEPS,
		PLAYER_ROLL,
		PLAYER_ACTIONFX,
		PLAYER_TRANSITIONFX,
		AMBIENCE,
		OPENING_CHORD,
		SUBTLE_MUSIC,
		FULL_MUSIC,
		WIND,
		PLANT_FX
	}


	// threshold of number of plants to decide whether to play music
	[Header("Values"), SerializeField] private int subtleMusicThreshold = 3;

	[SerializeField] private int fullMusicThreshold = 10;

	Coroutine _musicRoutine = null;
	bool _musicReady = true;

	[SerializeField] private AnimationCurve lowpassCurve;


	[Header("Audio Controllers"), SerializeField] private List<AudioController> _audioControllerList = new List<AudioController>();

    float[] _singData = new float[1024];

    void Awake () 
	{
		SetupAudioControllers();
	}

	private void Update()
	{
		// Control the music lowpass interp based on player's distance from the center.
		AudioMixer mixer = _audioControllerList[(int) AudioControllerNames.FULL_MUSIC].Source.outputAudioMixerGroup.audioMixer;
		mixer.SetFloat("lowpassFreq", lowpassCurve.Evaluate(PlayerManager.instance.DistanceFromPond));
	}

	/// <summary>
	/// Setups the audio controllers based upon AudioControllerNames enum.
	/// Adds additional Audio Controllers if there are more enum values than in the list in editor.
	/// </summary>
	private void SetupAudioControllers()
	{
		int controllerCount = Enum.GetNames(typeof(AudioControllerNames)).Length;
		GameObject newChild = null;
		for (int i = 0; i < controllerCount; ++i)
		{
			if(i >= _audioControllerList.Count)
			{
				_audioControllerList.Add(new AudioController());
				Debug.LogWarning("AudioManager: Too Many Enum Values, Adding Additional Controller");
			}

			newChild = new GameObject();
			newChild.transform.SetParent(this.transform);
			newChild.name = ((AudioControllerNames) i).ToString() + "_AudioController";

			_audioControllerList[i].Initialize( newChild.AddComponent<AudioSource>() );
		}
	}

	public override void Initialize ()
	{
		//MakeMeAPersistentSingleton();

		TimeManager.instance.MinuteCallback += CalculateMusicTimeState;
		TimeManager.instance.HourCallback += PlayHourlySong;

        CalculateMusicTimeState();
		_audioControllerList[(int)AudioControllerNames.OPENING_CHORD].PlayRandomClip();
		_audioControllerList[(int)AudioControllerNames.AMBIENCE].PlayAudioSource();
		_audioControllerList[(int)AudioControllerNames.WIND].PlayAudioSource();
		_audioControllerList[(int)AudioControllerNames.AMBIENCE].Loop = true;
        _audioControllerList[(int)AudioControllerNames.MUSIC].PlayAudioSource(); 

        isInitialized = true;
	}

	#region Controller Accessors

	public void SetControllerClip(AudioControllerNames controllerName, AudioClip clip)
	{
		_audioControllerList[(int) controllerName].MainClip = clip;
	}

	public void SetControllerMixer(AudioControllerNames controllerName, AudioMixerGroup mixer)
	{
		_audioControllerList[(int) controllerName].MixerGroup = mixer;
	}

	public void SetControllerVolume(AudioControllerNames controllerName, float vol)
	{
		_audioControllerList[(int) controllerName].Volume = vol;
	}

	public void SetControllerPlayOnAwake(AudioControllerNames controllerName, bool play)
	{
		_audioControllerList[(int) controllerName].PlayOnAwake = play;
	}

    public void SetControllerLoop(AudioControllerNames controllerName, bool loop)
    {
        _audioControllerList[(int) controllerName].Loop = loop;
    }

	public void PlayController(AudioControllerNames controllerName)
	{
		_audioControllerList[(int) controllerName].PlayAudioSource();
	}

	public void StopController(AudioControllerNames controllerName)
	{
		_audioControllerList[(int) controllerName].StopAudioSource();
	}

	public bool GetControllerIsPlaying(AudioControllerNames controllerName)
	{
		return _audioControllerList[(int) controllerName].Source.isPlaying;
	}

	public void PlayRandomAudioClip( AudioControllerNames controllerName )
	{
		_audioControllerList[(int) controllerName].PlayRandomClip();
	}

	public void SetClipAtIndex( AudioControllerNames controllerName, int clipIndex )
	{
		_audioControllerList[(int) controllerName].SelectClip(clipIndex);
	}

	public void PlayClipAtIndex( AudioControllerNames controllerName, int clipIndex )
	{
		_audioControllerList[(int) controllerName].SelectClip(clipIndex);
		_audioControllerList[(int) controllerName].PlayAudioSource();
	}

    #endregion


    public enum MusicTimeState
    {
        NULL = -1,
		SUNRISE = 0,
        MIDDAY,
        SUNSET,
		ON_THE_HOUR
    }
	MusicTimeState _musicTimeState = MusicTimeState.NULL;

    public MusicTimeState MusicTime { get { return _musicTimeState; } set { SetMusicTimeState(value); } }

    void SetMusicTimeState( MusicTimeState newTimeState )
    {
		if (_musicTimeState == MusicTimeState.NULL && newTimeState != MusicTimeState.NULL)
		{
			_musicTimeState = newTimeState;
			SetClipAtIndex( AudioControllerNames.MUSIC, (int)_musicTimeState );
		}
		else if (newTimeState != _musicTimeState)
		{
			StartCoroutine(ChangeMusicRoutine(newTimeState));
		}    
    }

	// Fades out the current music before switching.
	IEnumerator ChangeMusicRoutine(MusicTimeState newTimeState)
	{
		Tween volTween = DOTween.To(()=> _audioControllerList[(int) AudioControllerNames.MUSIC].Volume, 
			x=> _audioControllerList[(int) AudioControllerNames.MUSIC].Volume = x, 
			0f, 
			3f);

		yield return volTween.WaitForCompletion();

		yield return new WaitForSeconds(1.5f);

		_musicTimeState = newTimeState;
		SetClipAtIndex( AudioControllerNames.MUSIC, (int)_musicTimeState ); 
		SetControllerVolume(AudioControllerNames.MUSIC, 1f);
		PlayController(AudioControllerNames.MUSIC);
	}

    void CalculateMusicTimeState()
    {
        int realWorldHour = TimeManager.instance.RealWorldNow.TimeOfDay.Hours;
		int realWorldMinutes = TimeManager.instance.RealWorldNow.TimeOfDay.Minutes;

		if (realWorldMinutes < 1)
		{
			return;
		}

		if( realWorldHour > 0 && realWorldHour < 10)
		{
			SetMusicTimeState( MusicTimeState.SUNRISE );
		}
		else if (realWorldHour > 11 && realWorldHour < 17)
		{
			SetMusicTimeState( MusicTimeState.MIDDAY );
		}
		else 
		{
			SetMusicTimeState( MusicTimeState.SUNSET );
		}
    }

	void PlayHourlySong()
	{
		StartCoroutine(HourlyRoutine());
	}

	IEnumerator HourlyRoutine()
	{
		TimeManager.instance.MinuteCallback -= CalculateMusicTimeState;

		SetMusicTimeState(MusicTimeState.ON_THE_HOUR);
		SetControllerLoop(AudioControllerNames.MUSIC, false);

		while (GetControllerIsPlaying(AudioControllerNames.MUSIC))
		{
			yield return null;
		}

		SetControllerLoop(AudioControllerNames.MUSIC, true);
		TimeManager.instance.MinuteCallback += CalculateMusicTimeState;
		CalculateMusicTimeState();
		PlayController(AudioControllerNames.MUSIC);
	}


    // Player Sing
    public void PlaySing(int clipIndex, float pitch)
    {
		if ( !IsSinging )
		{
        	_audioControllerList[(int) AudioControllerNames.PLAYER_SING].Pitch = pitch;
        	_audioControllerList[(int) AudioControllerNames.PLAYER_SING].PlaySpecificClip(clipIndex);
		}
    }

    public bool IsSinging { get { return _audioControllerList[(int)AudioControllerNames.PLAYER_SING].Source.isPlaying; } }

	public int GetSingingClipCount()
	{
		return _audioControllerList[(int) AudioControllerNames.PLAYER_SING].ClipCount;
	}

	public void StopSing()
	{
		if (_audioControllerList[(int) AudioControllerNames.PLAYER_SING].Source.isPlaying)
		{
        	_audioControllerList[(int) AudioControllerNames.PLAYER_SING].StopAudioSource();
		}
	}

    public float GetCurrentMusicPitch()
    {      
        if(_audioControllerList[(int)AudioControllerNames.FULL_MUSIC].Source.isPlaying )
        {
            _audioControllerList[(int)AudioControllerNames.FULL_MUSIC].Source.GetSpectrumData( _singData, 0, FFTWindow.Rectangular );
        }    
        else if(_audioControllerList[(int)AudioControllerNames.SUBTLE_MUSIC].Source.isPlaying)
        {
            _audioControllerList[(int)AudioControllerNames.SUBTLE_MUSIC].Source.GetSpectrumData( _singData, 0, FFTWindow.Rectangular );
        }   
        else
        {
            _audioControllerList[(int)AudioControllerNames.AMBIENCE].Source.GetSpectrumData( _singData, 0, FFTWindow.Rectangular );
        }

        float maxV = 0f;
        int maxN = 0;
        for (int i = 0; i < 1024; i++)
        {
            if (!(_singData[i] > maxV) || !(_singData[i] > 0.02f))
                continue;

            maxV = _singData[i];
            maxN = i;
        }
        float freqN = (float) maxN;
        if (maxN > 0 && maxN < 1024 - 1)
        {
            float dL = _singData[maxN - 1] / _singData[maxN];
            float dR = _singData[maxN + 1] / _singData[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        return freqN * ((float)AudioSettings.outputSampleRate / 2f) / 1024 / 1024;
    }

	public void PlantAdded(int numPlants)
	{
		// check threshold
		if(numPlants >= subtleMusicThreshold)
		{
			// if subtle music has not started, start it
			if( !_audioControllerList[(int) AudioControllerNames.SUBTLE_MUSIC].Source.isPlaying && _musicReady )
			{
				_audioControllerList[(int) AudioControllerNames.SUBTLE_MUSIC].PlayRandomClip();
				_musicRoutine = StartCoroutine( MusicReadyRoutine() );
			}

		}
		else if(numPlants >= fullMusicThreshold )
		{
			// if full music has not started, and subtle music is not playing, start it
			// if subtle music has not started, start it
			if(!_audioControllerList[(int) AudioControllerNames.SUBTLE_MUSIC].Source.isPlaying &&
			   !_audioControllerList[(int) AudioControllerNames.FULL_MUSIC].Source.isPlaying && _musicReady )
			{
				_audioControllerList[(int) AudioControllerNames.FULL_MUSIC].PlayRandomClip();
				_musicRoutine = StartCoroutine( MusicReadyRoutine() );
			}
		}
	}

	IEnumerator MusicReadyRoutine()
	{
		_musicReady = false;

		yield return new WaitUntil( () => !_audioControllerList[(int) AudioControllerNames.SUBTLE_MUSIC].Source.isPlaying &&
			!_audioControllerList[(int) AudioControllerNames.FULL_MUSIC].Source.isPlaying ); 
		yield return new WaitForSeconds( 15f );

		_musicReady = true;
		_musicRoutine = null; 
	}

    private void OnValidate()
    {
        for( int i = 0; i < _audioControllerList.Count; i++ )
        {
            _audioControllerList[i].ControllerName = ( (AudioControllerNames)i ).ToString();
        }
    }
}
