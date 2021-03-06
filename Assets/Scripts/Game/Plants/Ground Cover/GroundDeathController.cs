﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDeathController : PlantController 
{
	public enum GroundPlantType : int 
	{
		NONE = -1,
		TWIST,
		CAPP
	}
	[SerializeField]GroundPlantType _type = GroundPlantType.NONE;

	Animator _anim = null;
	Material _mat = null;

	[SerializeField] Color[] _deathColors = new Color[2];

	Color[] _originalColors = new Color[2];
	Color[] _interpColors = new Color[2];
	int[] _shaderIDs = new int[2];
	DeathState _curState = DeathState.Dying;

	enum DeathState
	{
		Flying,
		Dying
	}

	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Death;
		_anim = GetComponentInChildren<Animator>();
		_mat = GetComponentInChildren<MeshRenderer>().material;
	}

	public override void UpdateState()
	{
		if( _curState == DeathState.Dying )
		{
			Decay();
		}
		else
		{
			StopState();
		}
	}

	protected virtual void Decay()
	{
		if( _myPlant.DeathTimer < _myPlant.DeathDuration )
		{
			FadeColor();
			_myPlant.DeathTimer += Time.deltaTime * _myPlant.CurDecayRate;
		}
		else
		{
			_curState = DeathState.Flying;
		}
	}

	public override  void StartState()
	{
		_shaderIDs[0] = Shader.PropertyToID( "_Color");
		_shaderIDs[1] = Shader.PropertyToID( "_Color2");

		_originalColors[0] = _mat.GetColor( _shaderIDs[0] );
		_originalColors[1] = _mat.GetColor( _shaderIDs[1] );

		_interpColors[0] = _originalColors[0]; 
		_interpColors[1] = _originalColors[1];

		ColorManager.ExecutePaletteChange += HandlePalatteChange;
	}

	public override void StopState()
	{
		StompPlant();
	}

	void FadeColor()
	{
		//iterate through all items and move their color
		//THIS IS HARD CODED AS HELL!
//		_interpColors[0] = Color.Lerp( _originalColors[0], _deathColors[0], _myPlant.DeathTimer / _myPlant.DeathDuration );
//		_interpColors[1] = Color.Lerp( _originalColors[1], _deathColors[1], _myPlant.DeathTimer / _myPlant.DeathDuration );
//
//		_mat.SetColor( _shaderIDs[0], _interpColors[0] );
//		_mat.SetColor( _shaderIDs[1], _interpColors[1] );
	}

	void OnTriggerEnter( Collider col)
	{
		if( col.GetComponent<Player>() )
		{
				StompPlant();
		}
	}

	public override void StompPlant()
	{
		_anim.SetBool( "isSquishing", true );
	}

	public override void WaterPlant(){}
	public override void TouchPlant(){}
	public override void GrabPlant(){}

	void OnDestroy()
	{
		ColorManager.ExecutePaletteChange -= HandlePalatteChange;
	}

	void HandlePalatteChange( ColorManager.EnvironmentPalette newPalette, ColorManager.EnvironmentPalette prevPalette )
	{
//		// TODO make coroutine to change colors, remove fading color changes
//
//		Debug.Log( "Transitioning Ground Cover Color" );
//
//		switch( _type )
//		{
//		case GroundPlantType.TWIST:
//			StartCoroutine( DelayedTransitionColors( newPalette.twistPlant ) );
//			break;		
//		case GroundPlantType.CAPP:
//			StartCoroutine( DelayedTransitionColors( newPalette.cappPlant ) );
//			break;		
//		default:
//			break;
//		}
	}

	IEnumerator DelayedTransitionColors( Gradient newGradient, float transitionTime = ColorManager.PALETTE_TRANSITIONTIME )
	{
		float timer = 0.0f;
		Color topColor = _mat.GetColor( _shaderIDs[0] );
		Color midColor = _mat.GetColor( _shaderIDs[1] );

		while( timer < transitionTime )
		{
			timer +=  Time.deltaTime;

			_interpColors[0] = Colorx.Slerp( topColor, newGradient.Evaluate(0.0f), timer / transitionTime );
			_interpColors[1] = Colorx.Slerp( midColor, newGradient.Evaluate(0.5f), timer / transitionTime );

			LateUpdateColors();

			yield return 0;
		}	
	}

	public void LateUpdateColors()
	{
		_mat.SetColor( _shaderIDs[0], _interpColors[0] );
		_mat.SetColor( _shaderIDs[1], _interpColors[1] );
	}
}
