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
		INTRO			// Wait for player input to start match
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

		DontDestroyOnLoad(gameObject);


		#if !UNITY_EDITOR 
		Cursor.visible = false;
		#else
		Application.targetFrameRate = 60;	// MAKES IOS VERSION CRASH
		#endif

		// Have to add safeguards for when NONE isn't selected
		if( _state == GameState.NONE )
		{
			ChangeGameState(GameState.INIT);	
		}
	}

	void OnDestroy()
	{
		if(Instance == this)
			Instance = null;
	}

	void Start()
	{
		
	}

	void Update()
	{

		switch(_state)
		{
		case GameState.INTRO:
			break;
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

		if(newState != prevState)
		{
			switch(newState)
			{
			case GameState.INIT:			// Only for game launch
				Initialize();
				break;
			case GameState.INTRO:           
				break;
			default:
				break;
			}
			_state = newState;

			StartCoroutine(DelayedCompleteChangeScene(newSceneLoaded, newSceneIndex));

			if(GameStateChanged != null)
				GameStateChanged(_state, prevState);
		}

		Debug.Log("Transitioned from: " + prevState.ToString() + " to " + newState.ToString());

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
		UIManager.instance.Initialize();

		yield return new WaitUntil( () => UIManager.instance.IsInitialized );

		ControlManager.instance.Initialize();

		yield return new WaitUntil( () => ControlManager.instance.IsInitialized );

		AudioManager.instance.Initialize();

		yield return new WaitUntil( () => AudioManager.instance.IsInitialized );

		CameraManager.instance.Initialize();

		yield return new WaitUntil( () => CameraManager.instance.IsInitialized );

        ChangeGameState(GameState.INTRO);

	}

	IEnumerator RestartScene(int sceneNum, float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		SceneManager.LoadScene(sceneNum);
	}
}