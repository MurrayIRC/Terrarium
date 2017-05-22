﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceManager : MonoBehaviour
{
    private Coroutine _blinkRoutine;
    private Coroutine _idleRoutine;

	[SerializeField, Space(10)] SkinnedMeshRenderer _leftEyeSkinnedMesh;
	[SerializeField] SkinnedMeshRenderer _rightEyeSkinnedMesh;
	[SerializeField] SkinnedMeshRenderer _mouthSkinnedMesh; 

	private Coroutine _facePoseRoutine;
	[SerializeField, Space(10)] bool _updateFaceOnValidate = true;
	[SerializeField] private int _facePoseIndex = 0;
	private FacePose _currFacePose;

	private EyeBlendData _blinkEyeData;

	[Space(10),SerializeField] Vector2 _blinkWaitRange = new Vector2( 1.0f, 5.0f );
	[SerializeField] float _blinkTime = 0.1f; 

	[SerializeField] float _idleReturnWaitTime = 5.0f;

	FacePose _idlePose;

	void Awake()
	{		
		InitiateBlinkLoop();

		_blinkEyeData = FindFacePose( "Blink" ).LeftEyePose;
		_idlePose = FindFacePose( "Idle" );

		_currFacePose = _idlePose;

		//_mouthPoseIndex = MouthPoseManager.instance.StartMouthPoseIndex;
		//SetMouthPose( MouthPoseManager.instance.MouthPoseArray[_mouthPoseIndex] );
	}

	// ===============
	// E M O T I O N S
	// ===============

	public void BecomeEncumbered()
	{
		// Used in Carrying
	}

	public void BecomeInterested()
	{
		// used in pickup and arm reach i thnk
	}

	public void BecomeFeisty()
	{
		// used wehn Planting
	}

	public void BecomeIdle()
	{
		StartFaceTransition( _idlePose );	
	}

	public void BecomeDesirous()
	{
		// Done on Reach
	}

	public void BecomeHappy()
	{
		// On Approach Stuff	
	}

	#region Blink Hell

    private void InitiateBlinkLoop()
    {
        if (_blinkRoutine != null)
            StopCoroutine(_blinkRoutine);

        _blinkRoutine = StartCoroutine( BlinkRoutine( Random.Range( _blinkWaitRange.x, _blinkWaitRange.y ) ) );
    }

    private IEnumerator BlinkRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

		float timer = 0.0f;
		float blinkProgress = 0.0f;

		while( timer < _blinkTime )
		{
			blinkProgress = timer / _blinkTime;

			SetRightEyeBlendValues( _blinkEyeData, blinkProgress );
			SetLeftEyeBlendValues( _blinkEyeData, blinkProgress );

			timer += Time.deltaTime;

			yield return 0;
		}
        
		timer = 0.0f;
		blinkProgress = 0.0f;

		while( timer < _blinkTime )
		{
			blinkProgress = timer / _blinkTime;

			_rightEyeSkinnedMesh.SetBlendShapeWeight( 0, Mathf.Lerp( _blinkEyeData.DespairBlendValue, _currFacePose.RightEyePose.DespairBlendValue, blinkProgress ) );
			_rightEyeSkinnedMesh.SetBlendShapeWeight( 1, Mathf.Lerp( _blinkEyeData.WideBlendValue, _currFacePose.RightEyePose.WideBlendValue, blinkProgress ) );
			_rightEyeSkinnedMesh.SetBlendShapeWeight( 2, Mathf.Lerp( _blinkEyeData.AngryBlendValue, _currFacePose.RightEyePose.AngryBlendValue, blinkProgress ) );
			_rightEyeSkinnedMesh.SetBlendShapeWeight( 3, Mathf.Lerp( _blinkEyeData.HalfOpenBlendValue, _currFacePose.RightEyePose.HalfOpenBlendValue, blinkProgress ) );
			_rightEyeSkinnedMesh.SetBlendShapeWeight( 4, Mathf.Lerp( _blinkEyeData.ClosedBlendValue, _currFacePose.RightEyePose.ClosedBlendValue, blinkProgress ) );
			_rightEyeSkinnedMesh.SetBlendShapeWeight( 5, Mathf.Lerp( _blinkEyeData.SadBlendValue, _currFacePose.RightEyePose.SadBlendValue, blinkProgress ) );
			_rightEyeSkinnedMesh.SetBlendShapeWeight( 6, Mathf.Lerp( _blinkEyeData.HappyBlendValue, _currFacePose.RightEyePose.HappyBlendValue, blinkProgress ) );

			_leftEyeSkinnedMesh.SetBlendShapeWeight( 0, Mathf.Lerp( _blinkEyeData.DespairBlendValue, _currFacePose.LeftEyePose.DespairBlendValue, blinkProgress ) );
			_leftEyeSkinnedMesh.SetBlendShapeWeight( 1, Mathf.Lerp( _blinkEyeData.WideBlendValue, _currFacePose.LeftEyePose.WideBlendValue, blinkProgress ) );
			_leftEyeSkinnedMesh.SetBlendShapeWeight( 2, Mathf.Lerp( _blinkEyeData.AngryBlendValue, _currFacePose.LeftEyePose.AngryBlendValue, blinkProgress ) );
			_leftEyeSkinnedMesh.SetBlendShapeWeight( 3, Mathf.Lerp( _blinkEyeData.HalfOpenBlendValue, _currFacePose.LeftEyePose.HalfOpenBlendValue, blinkProgress ) );
			_leftEyeSkinnedMesh.SetBlendShapeWeight( 4, Mathf.Lerp( _blinkEyeData.ClosedBlendValue, _currFacePose.LeftEyePose.ClosedBlendValue, blinkProgress ) );
			_leftEyeSkinnedMesh.SetBlendShapeWeight( 5, Mathf.Lerp( _blinkEyeData.SadBlendValue, _currFacePose.LeftEyePose.SadBlendValue, blinkProgress ) );
			_leftEyeSkinnedMesh.SetBlendShapeWeight( 6, Mathf.Lerp( _blinkEyeData.HappyBlendValue, _currFacePose.LeftEyePose.HappyBlendValue, blinkProgress ) );

			timer += Time.deltaTime;

			yield return 0;
		}

		SetRightEyeBlendValues( _currFacePose.RightEyePose, 1.0f );
		SetLeftEyeBlendValues( _currFacePose.LeftEyePose, 1.0f );

        InitiateBlinkLoop();
    }

	#endregion

	#region Idle Return 

    void StartReturnIdle()
    {
        if( this.enabled )
        {
            if (_idleRoutine != null)
            {
                StopCoroutine( _idleRoutine );
            }
            _idleRoutine = StartCoroutine( DelayedDefaultExpression() );
        }
    }

    IEnumerator DelayedDefaultExpression()
    {
		yield return new WaitForSeconds( _idleReturnWaitTime );

		StartFaceTransition( _idlePose );

        _idleRoutine = null;
    }

	#endregion

	#region Face Transition Methods

	public void TransitionFacePose( string poseName, bool returnToIdle = false )
	{
		if( _currFacePose != null )
		{
			StartFaceTransition( FindFacePose( poseName ) );

			if( returnToIdle )
			{
				StartReturnIdle();
			}	
		}
		else
		{
			// On Start make sure shit sets properly
			_currFacePose = FindFacePose( poseName );
			SetFacePose( FindFacePose( poseName ) );
		}
	}

	public void DelayedTransitionFacePose( string poseName, float delayTime, bool returnToIdle = false )
	{
		StartCoroutine( DelayedTransitionRoutine( poseName, delayTime, returnToIdle ) );
	}

	IEnumerator DelayedTransitionRoutine( string poseName, float delayTime, bool returnToIdle )
	{
		yield return new WaitForSeconds( delayTime );

		TransitionFacePose( poseName, returnToIdle );
	}

	/// <summary>
	/// Sets face pose fully to mouthpose value
	/// </summary>
	/// <param name="mouthPose">Mouth pose.</param>
	void SetFacePose( FacePose mouthPose )
	{
		SetRightEyeBlendValues( mouthPose.RightEyePose, 1.0f );
		SetLeftEyeBlendValues( mouthPose.LeftEyePose, 1.0f );
		SetMouthBlendValues( mouthPose.MouthPose, 1.0f );

		_currFacePose = mouthPose;
	}

	/// <summary>
	/// Begins or queues up Face Pose Transition
	/// </summary>
	/// <param name="newPose">New pose.</param>
	void StartFaceTransition( FacePose newPose )
	{
		Debug.Assert( newPose != null );

		if( _facePoseRoutine != null )
		{
			StartCoroutine( WaitForFaceTransition( newPose ) );
		}
		else
		{
			_facePoseRoutine = StartCoroutine( FaceTransitionRoutine( newPose ) );
		}
	
	}

	/// <summary>
	/// Waits for face transition to complete before starting new transition
	/// </summary>
	/// <returns>The for face transition.</returns>
	/// <param name="newPose">New pose.</param>
	IEnumerator WaitForFaceTransition( FacePose newPose )
	{
		yield return new WaitUntil( () => _facePoseRoutine == null );

		_facePoseRoutine = StartCoroutine( FaceTransitionRoutine( newPose ) );
	}

	IEnumerator FaceTransitionRoutine( FacePose newPose )
	{
		float timer = 0.0f;
		float mouthTransProgress = 0.0f;

		while( timer < MouthPoseManager.instance.MouthPoseTransitionTime )
		{
			mouthTransProgress = MouthPoseManager.instance.MouthTransitionAnimCurve.Evaluate( timer / MouthPoseManager.instance.MouthPoseTransitionTime );

			SetRightEyeBlendValues( newPose.RightEyePose, mouthTransProgress );
			SetLeftEyeBlendValues( newPose.LeftEyePose, mouthTransProgress );
			SetMouthBlendValues( newPose.MouthPose, mouthTransProgress );

			timer += Time.deltaTime;

			yield return 0;
		}

		SetFacePose( newPose );

		_facePoseRoutine = null;
	}

	void SetRightEyeBlendValues( EyeBlendData newPose, float transitionProgress )
	{
		_rightEyeSkinnedMesh.SetBlendShapeWeight( 0, Mathf.Lerp( _currFacePose.RightEyePose.DespairBlendValue, newPose.DespairBlendValue, transitionProgress ) );
		_rightEyeSkinnedMesh.SetBlendShapeWeight( 1, Mathf.Lerp( _currFacePose.RightEyePose.WideBlendValue, newPose.WideBlendValue, transitionProgress ) );
		_rightEyeSkinnedMesh.SetBlendShapeWeight( 2, Mathf.Lerp( _currFacePose.RightEyePose.AngryBlendValue, newPose.AngryBlendValue, transitionProgress ) );
		_rightEyeSkinnedMesh.SetBlendShapeWeight( 3, Mathf.Lerp( _currFacePose.RightEyePose.HalfOpenBlendValue, newPose.HalfOpenBlendValue, transitionProgress ) );
		_rightEyeSkinnedMesh.SetBlendShapeWeight( 4, Mathf.Lerp( _currFacePose.RightEyePose.ClosedBlendValue, newPose.ClosedBlendValue, transitionProgress ) );
		_rightEyeSkinnedMesh.SetBlendShapeWeight( 5, Mathf.Lerp( _currFacePose.RightEyePose.SadBlendValue, newPose.SadBlendValue, transitionProgress ) );
		_rightEyeSkinnedMesh.SetBlendShapeWeight( 6, Mathf.Lerp( _currFacePose.RightEyePose.HappyBlendValue, newPose.HappyBlendValue, transitionProgress ) );
	}

	void SetLeftEyeBlendValues( EyeBlendData newPose, float transitionProgress )
	{
		_leftEyeSkinnedMesh.SetBlendShapeWeight( 0, Mathf.Lerp( _currFacePose.LeftEyePose.DespairBlendValue, newPose.DespairBlendValue, transitionProgress ) );
		_leftEyeSkinnedMesh.SetBlendShapeWeight( 1, Mathf.Lerp( _currFacePose.LeftEyePose.WideBlendValue, newPose.WideBlendValue, transitionProgress ) );
		_leftEyeSkinnedMesh.SetBlendShapeWeight( 2, Mathf.Lerp( _currFacePose.LeftEyePose.AngryBlendValue, newPose.AngryBlendValue, transitionProgress ) );
		_leftEyeSkinnedMesh.SetBlendShapeWeight( 3, Mathf.Lerp( _currFacePose.LeftEyePose.HalfOpenBlendValue, newPose.HalfOpenBlendValue, transitionProgress ) );
		_leftEyeSkinnedMesh.SetBlendShapeWeight( 4, Mathf.Lerp( _currFacePose.LeftEyePose.ClosedBlendValue, newPose.ClosedBlendValue, transitionProgress ) );
		_leftEyeSkinnedMesh.SetBlendShapeWeight( 5, Mathf.Lerp( _currFacePose.LeftEyePose.SadBlendValue, newPose.SadBlendValue, transitionProgress ) );
		_leftEyeSkinnedMesh.SetBlendShapeWeight( 6, Mathf.Lerp( _currFacePose.LeftEyePose.HappyBlendValue, newPose.HappyBlendValue, transitionProgress ) );
	}

	void SetMouthBlendValues( MouthBlendData newPose, float transitionProgress )
	{
		_mouthSkinnedMesh.SetBlendShapeWeight( 0, Mathf.Lerp( _currFacePose.MouthPose.LittleHappyBlendValue, newPose.LittleHappyBlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 1, Mathf.Lerp( _currFacePose.MouthPose.LittleUpsetBlendValue, newPose.LittleUpsetBlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 2, Mathf.Lerp( _currFacePose.MouthPose.LittleFrownBlendValue, newPose.LittleFrownBlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 3, Mathf.Lerp( _currFacePose.MouthPose.LittleOBlendValue, newPose.LittleOBlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 4, Mathf.Lerp( _currFacePose.MouthPose.BigFrownBlendValue, newPose.BigFrownBlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 5, Mathf.Lerp( _currFacePose.MouthPose.SquigglySmileBlendValue, newPose.SquigglySmileBlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 6, Mathf.Lerp( _currFacePose.MouthPose.BigHappyBlendValue, newPose.BigHappyBlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 7, Mathf.Lerp( _currFacePose.MouthPose.Despair1BlendValue, newPose.Despair1BlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 8, Mathf.Lerp( _currFacePose.MouthPose.BigUpsetBlendValue, newPose.BigUpsetBlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 9, Mathf.Lerp( _currFacePose.MouthPose.LittleSmileBlendValue, newPose.LittleSmileBlendValue, transitionProgress ) );
        _mouthSkinnedMesh.SetBlendShapeWeight( 10, Mathf.Lerp( _currFacePose.MouthPose.BigSmileBlendValue, newPose.BigSmileBlendValue, transitionProgress ) );
		_mouthSkinnedMesh.SetBlendShapeWeight( 11, Mathf.Lerp( _currFacePose.MouthPose.OvalOBlendValue, newPose.OvalOBlendValue, transitionProgress ) );        
    }

	#endregion

	/// <summary>
	/// Finds the face pose in the Mouth Pose Manager face pose list
	/// </summary>
	/// <returns>The face pose.</returns>
	/// <param name="poseName">Pose name.</param>
	FacePose FindFacePose( string poseName )
	{
		FacePose pose = null;

		pose = MouthPoseManager.instance.FacePoseList.Find( x => x != null && x.PoseName == poseName );

		Debug.Assert( pose != null );

		return pose;
	}


    void OnValidate()
    {
        if( _facePoseIndex > MouthPoseManager.instance.FacePoseList.Count - 1 )
        {
            _facePoseIndex = MouthPoseManager.instance.FacePoseList.Count - 1;
        }
        else if( _facePoseIndex < 0 )
        {
            _facePoseIndex = 0;
        }            

		if( _updateFaceOnValidate )
		{
			if (Application.isPlaying && this.enabled )
			{
				StartFaceTransition( MouthPoseManager.instance.FacePoseList[_facePoseIndex] );
			}
			else
			{
				SetFacePose( MouthPoseManager.instance.FacePoseList[_facePoseIndex] );
			}	
		}			
    }
}
