﻿using UnityEngine;
using System.Collections;
using DG.Tweening;

public class WalkingState : RollerState
{
    private Tween _rollPosTween;
    private Tween _rollSpherifyTween;
    private float _idleTimer = 0f;

    Coroutine _reachCoroutine = null;

    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER WALKING STATE");

        // Handle Transition
        switch ( prevState )
        {
        case P_ControlState.ROLLING:            
                CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
                //PlayerManager.instance.Player.AnimationController.PlayRollToWalkAnim();
                _rollPosTween = _roller.RollSphere.transform.DOMoveY( 1.5f, RollerConstants.instance.RollExitSpeed ).SetEase( Ease.OutCubic ).OnComplete(TransitionFromRollComplete);
                _rollSpherifyTween = DOTween.To( () => _roller.Spherify, x => _roller.Spherify = x, 0.0f, RollerConstants.instance.RollExitSpeed ).SetEase( Ease.OutCubic );
                break;       
        }

        _idleTimer = 0f;        
        //PlayerManager.instance.Player.AnimationController.PlayWalkAnim();
    }

    void TransitionFromRollComplete()
    {
        _roller.BecomeWalker();

        _rollPosTween.Kill( true );
        _rollPosTween = null;

        _rollSpherifyTween.Kill( true );
        _rollSpherifyTween = null;

        _roller.SpherifyScale = RollerConstants.instance.BreathSpherizeScale;
    }

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT WALKING STATE");

        _roller.Player.PlayerSingController.StopSinging();

        if ( _rollPosTween != null )
	    {
	        _rollPosTween.Kill();
	        _rollPosTween = null;
	    }

        if( _reachCoroutine != null )
        {            
            StopCoroutine( _reachCoroutine );
            _reachCoroutine = null;
        }

		RollerParent.Idling = false;
    }

    public override void HandleInput( InputCollection input )
    {
        if ( _rollPosTween == null )
        {
            // hmm this is bad, scales could b the same lol
            // yup, it's bad!
            if (_roller.SpherifyScale == RollerConstants.instance.RitualSphereizeScale && _roller.Spherify > 0.0f)
            {
                _roller.Spherify -= Time.deltaTime * RollerConstants.instance.RitualDeflateSpeed;

                if (_roller.Spherify < 0.0f)
                {
                    _roller.Spherify = 0.0f;
                    _roller.SpherifyScale = RollerConstants.instance.BreathSpherizeScale;
                    _roller.BreathTimer = 0.0f;
                }
            }
            else
            {
                _roller.BreathTimer += Time.deltaTime * RollerConstants.instance.BreathSpeed;

                // The Spherize Curve is set to PingPong in the Animation Curve.
                // Max Spherize Size is the value of the second key on the curve
                _roller.Spherify = RollerConstants.instance.BreathSpherizeCurve.Evaluate( _roller.BreathTimer );
            }

            // Check for sitting after idling for a while.
            IdleTimer( input );

            if (input.LeftBumper.WasPressed)
            {
                _idleTimer = 0f;
                IncrementLeftArmGesture();
            }

            if (input.RightBumper.WasPressed)
            {
                _idleTimer = 0f;
                IncrementRightArmGesture();
            }

            // A BUTTON 
            if (input.AButton.WasPressed)
            {
                // End coroutine waiting to see if the player should auto reach if the player inputs for arms  
                if (_reachCoroutine != null)
                {
                    StopCoroutine( _reachCoroutine );
                    _reachCoroutine = null;
                }

                HandlePickup( PlayerArmIK.ArmType.BOTH );

                if (_roller.IK.ArmFocus != null)
                {
                    HandleGrabObject();
                }
            }

            if (_roller.IK.ArmsIdle)
            {
                if (_reachCoroutine == null)
                {
                    _reachCoroutine = StartCoroutine( ReachWaitRoutine() );
                }
            }

            // Update how far the arms are reaching
            _roller.UpdateArmReachIK( input.LeftTrigger.Value, input.RightTrigger.Value );

            // B BUTTON
            if (input.BButton.IsPressed)
            {
                if (GameManager.Instance.State == GameManager.GameState.MAIN)
                {
                    _roller.ChangeState( P_ControlState.ROLLING );
                }
            }
            else if (input.XButton.IsPressed)   // X BUTTON
            {
                _roller.ChangeState( P_ControlState.RITUAL );
            }
            else if (input.YButton.WasPressed)  // Y BUTTON
            {
                _roller.Player.PlayerSingController.BeginSinging();
                //_roller.ChangeState( P_ControlState.SING);
            }
            else if (input.YButton.WasReleased)
            {
                _roller.Player.PlayerSingController.StopSinging();
            }
        }
        
    }

    public override void HandleFixedInput(InputCollection input)
	{	
		_roller.IKMovement(RollerConstants.instance.WalkSpeed, 
									  RollerConstants.instance.WalkAcceleration, 
									  RollerConstants.instance.WalkDeceleration, 
									  RollerConstants.instance.WalkTurnSpeed);
    }

    void IdleTimer(InputCollection input)
    {
        // handle idle timing
        _idleTimer += Time.deltaTime;

        Vector3 vec = new Vector3(input.LeftStickX, 0f, input.LeftStickY);
        if (input.ActiveDevice.AnyButtonIsPressed || vec.magnitude >= 0.25f)
        {
            _idleTimer = 0f;
        }
        else
        {
            // Left Stick Button
            if (input.LeftStickButton.IsPressed)
            {
                _roller.Player.AnimationController.SitButtonPress();
                _roller.ChangeState(P_ControlState.SIT);
            }
        }

        if (_idleTimer >= RollerConstants.instance.IdleSittingTimer)
        {
            // go to sitting State
            _roller.ChangeState(P_ControlState.SIT);
        }
    }

    IEnumerator ReachWaitRoutine()
    {
        //Debug.Log( "Starting Reach Timer" );

        yield return new WaitForSeconds( Random.Range( RollerConstants.instance.IKReachWaitMin, RollerConstants.instance.IKReachWaitMax ) );

        //Debug.Log( "Prepping Reach" );

        // Flip to decide where arm is reaching
        CheckForReachable( JohnTech.CoinFlip() ? PlayerArmIK.ArmType.LEFT : PlayerArmIK.ArmType.RIGHT );

        _reachCoroutine = null;
    }

}
