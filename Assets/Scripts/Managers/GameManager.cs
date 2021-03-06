﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance = null;

	public static event Action<GameManager.GameState, GameManager.GameState> GameStateChanged;

	public enum SceneIndices
	{
		SCENE_INTRO = 0,
	}

	public enum GameState
	{
		NONE = 0,
		INIT,
		INTRO,			// Wait for player input to start match
		MAIN,
        POND_WAIT,
        POND_RETURN,    // When Player transitions back to Pond
        POND_POP       // When Player pops out of Pond (respawns)
	}

	[SerializeField] private GameState _state;
	public GameState State
	{
		get
		{
			return _state;
		}
	}

	void Awake()
	{
		if(Instance == null)
		{
			Instance = this;
		}
		else if(Instance != null)
		{
			Destroy(gameObject);
		}

        //DontDestroyOnLoad(gameObject);


        #if !UNITY_EDITOR
		Cursor.visible = false;
        #endif
        Application.targetFrameRate = 60;   // MAKES IOS VERSION CRASH

        AudioManager.instance.Initialize();
        ControlManager.instance.Initialize();
        UIManager.instance.Initialize();
    }

    void OnDestroy()
	{
		if(Instance == this)
			Instance = null;
	}

	void Update()
	{
		InputCollection input = ControlManager.instance.getInput();
		if ( _state == GameState.NONE )
        {

            if (input.LeftStickX.Value > RollerConstants.instance.IdleMaxMag ||
            input.LeftStickY.Value > RollerConstants.instance.IdleMaxMag ||
            input.AButton.IsPressed ||
            input.BButton.IsPressed ||
            input.XButton.IsPressed ||
            input.YButton.IsPressed)
            {
                ChangeGameState( GameState.INIT );
            }

        }
		else if(_state == GameState.MAIN && input.StartButton.WasPressed)
		{
			PausePanel pausePanel = UIManager.GetPanelOfType<PausePanel>();
			pausePanel.TogglePause();
		}

		// rly hacky Restart gdi
		if( Input.GetKey(KeyCode.Alpha0) )
		{
			SceneManager.LoadScene(0);
		}

    }

    public void ChangeGameState(GameState newState)
	{
		GameState prevState = _state;

		ChangeGameStateDisable(newState, prevState);

		ChangeGameStateEnable(newState, prevState);

	}

	private void ChangeGameStateDisable(GameState newState, GameState prevState)
	{
		if(newState != prevState)
		{
			switch(newState)
			{
			case GameState.INTRO:
				break;
			}
		}
	}

	private void ChangeGameStateEnable(GameState newState, GameState prevState)
	{
		bool newSceneLoaded = false;
		int newSceneIndex = 0;

		if( newState != prevState )
		{
			switch( newState )
			{
			case GameState.INIT:			// Only for game launch
				Initialize();
				break;
			case GameState.INTRO:
				CameraManager.instance.ChangeCameraState(CameraManager.CameraState.INTRO);
                break;
			default:
				break;
			}
			_state = newState;

            StartCoroutine( DelayedCompleteChangeScene( newSceneLoaded, newSceneIndex ) );

			if(GameStateChanged != null)
            {
                GameStateChanged( _state, prevState );
            }
				
		}

		Debug.Log("[GameManager] Transitioned from: " + prevState.ToString() + " to " + newState.ToString());

	}

	IEnumerator DelayedCompleteChangeScene(bool newSceneLoading, int newSceneIndex)
	{
		if(newSceneLoading)
		{
			SceneManager.LoadScene(newSceneIndex);

			yield return new WaitUntil(() => SceneManager.GetSceneAt(0).isLoaded);
		}

		yield return 0;
	}

	private void RestartGame()
	{
	}

	private void Initialize()
	{
		StartCoroutine(DelayedInitialize());
	}

	IEnumerator DelayedInitialize()
	{
// 		ControlManager.instance.Initialize();
// 
// 		yield return new WaitUntil( () => ControlManager.instance.IsInitialized );

		PlayerManager.instance.Initialize();

		yield return new WaitUntil( () => PlayerManager.instance.IsInitialized );

        TimeManager.instance.Initialize();

        yield return new WaitUntil( () => TimeManager.instance.IsInitialized );

        PlantManager.instance.Initialize();

        yield return new WaitUntil( () => PlantManager.instance.IsInitialized );

//         AudioManager.instance.Initialize();
// 
// 		yield return new WaitUntil( () => AudioManager.instance.IsInitialized );

		CameraManager.instance.Initialize();

		yield return new WaitUntil( () => CameraManager.instance.IsInitialized );

        PondManager.instance.Initialize();

        yield return new WaitUntil( () => PondManager.instance.IsInitialized );

        WeatherManager.instance.Initialize();

        yield return new WaitUntil( () => WeatherManager.instance.IsInitialized );

        CreatureManager.instance.Initialize();

        yield return new WaitUntil( () => CreatureManager.instance.IsInitialized );

        ChangeGameState( GameState.INTRO );
	}

	IEnumerator RestartScene(int sceneNum, float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		SceneManager.LoadScene(sceneNum);
	}
}
