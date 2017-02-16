﻿using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RitualState : RollerState
{
    //private Tween _tween;
	private float ritualTimer = 0f;
	float currentTurnSpeed = 0f;

	public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER RITUAL STATE");
        //_tween = transform.DOScaleY( 0.1f, RollerConstants.RITUAL_TIME ).OnComplete( OnCompleteRitual );
		_roller.IK.SetState(PlayerIKControl.WalkState.RITUAL);
		ritualTimer = 0f;
	    //PlayerManager.instance.Player.AnimationController.PlayIdleAnim();

		AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_ACTIONFX, 2 );
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT RITUAL STATE");
		_roller.IK.SetState(PlayerIKControl.WalkState.IDLE);
		/*
	    if (_tween != null)
	    {
	        _tween.Kill();
	        _tween = null;
	    }
	    transform.localScale = Vector3.one;
	    */

		AudioManager.instance.StopController( AudioManager.AudioControllerNames.PLAYER_ACTIONFX );
	}

	public override void HandleInput(InputCollection input)
	{
	    //bool isComplete = _tween.IsComplete();
		ritualTimer += Time.deltaTime;
		if (ritualTimer > RollerConstants.RITUAL_TIME)
		{
			OnCompleteRitual();
		}

		currentTurnSpeed = Mathf.Lerp(0, RollerConstants.RITUAL_TURN_SPEED, ritualTimer / RollerConstants.RITUAL_TIME);
		transform.Rotate(0f, currentTurnSpeed * Time.deltaTime, 0f);

	    if (/*!isComplete &&*/ !input.XButton.IsPressed)
		{
		    _roller.ChangeState(P_ControlState.WALKING);
		}
	}
		
    private void OnCompleteRitual()
    {
        GameManager.Instance.ChangeGameState( GameManager.GameState.POND_RETURN );

        transform.DOMoveY( -5.0f, 0.5f );

        StartCoroutine( DelayedCompleteRitual() );        
    }

    IEnumerator DelayedCompleteRitual()
    {
		float timer = 0f;
		float totalTime = RollerConstants.RITUAL_COMPLETEWAIT;
		float currentPaintSize = 0f;
		float maxPaintSize = 30f;
		Vector3 pos = transform.position;

		Tween paint = DOTween.To(()=> currentPaintSize, x=> currentPaintSize = x, maxPaintSize, totalTime);
		while(paint.IsPlaying())
		{
			GroundManager.instance.Ground.DrawOnPosition(pos, currentPaintSize);
			yield return null;
		}

		yield return paint.WaitForCompletion();

		//yield return new WaitForSeconds(totalTime);

        // TODO: implement plant watering here
        transform.localScale = Vector3.one;
		WaterPlantsCloseBy( currentPaintSize * .5f );
        PondManager.instance.HandlePondReturn();
    }

	void WaterPlantsCloseBy( float searchRadius )
	{
		Collider[] cols = Physics.OverlapSphere( transform.position, searchRadius );
		foreach( Collider col in cols )
		{
			Plantable plant = col.GetComponent<Plantable>();
			if( plant )
			{
				plant.WaterPlant();
			}
		}
	}
}
