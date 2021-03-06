﻿using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RitualState : RollerState
{
	private float ritualTimer = 0f;
	private bool hasExploded = false;
    private bool wakeUpExplode = false;

	public override void Enter(P_ControlState prevState)
	{
		Debug.Log("[RollerState] ENTER RITUAL STATE");
        _roller.IK.SetState( PlayerIKControl.WalkState.RITUAL );
		ritualTimer = 0f;
		hasExploded = false;

        if( prevState == P_ControlState.SIT )
        {
            wakeUpExplode = true;
        }


        _roller.Spherify = 0.0f;
        _roller.SpherifyScale = RollerConstants.instance.RitualSphereizeScale;

		_roller.Face.TransitionFacePose( "Pop" );

		AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_ACTIONFX, 4 );
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("[RollerState] EXIT RITUAL STATE");

        wakeUpExplode = false;

        if ( nextState == P_ControlState.WALKING )
		{
			_roller.IK.SetState( PlayerIKControl.WalkState.WALK );
			_roller.Face.BecomeIdle();
			AudioManager.instance.StopController( AudioManager.AudioControllerNames.PLAYER_ACTIONFX );
		}

        _roller.ExplodeParticleSystem.Stop();
	}

	public override void HandleFixedInput(InputCollection input)
	{
		if ( !hasExploded && ritualTimer > RollerConstants.instance.RitualTime )
		{
			hasExploded = true;
			_roller.HandlePondReturn();

            CameraManager.instance.ScreenShake( 0.4f, 0.5f, 20 );

            if ( _roller.CurrentHeldObject != null )
			{
				Pickupable obj = _roller.CurrentHeldObject.GetComponent<Pickupable>();
				if( obj )
				{
					obj.DropSelf();
				}

				_roller.IK.LetGoBothArms();
				_roller.CurrentHeldObject = null;
			}
		}
		else if ( !hasExploded )
		{
			ritualTimer += Time.deltaTime;

            if( !wakeUpExplode )
            {
                // Update how far the arms are reaching
                _roller.UpdateArmReachIK( input.LeftTrigger.Value, input.RightTrigger.Value );

                _roller.IKMovement( RollerConstants.instance.WalkSpeed,
                                            RollerConstants.instance.WalkAcceleration,
                                            RollerConstants.instance.WalkDeceleration,
                                            RollerConstants.instance.WalkTurnSpeed );
                if (!input.XButton.IsPressed)
                {
                    _roller.ChangeState( P_ControlState.WALKING );
                }
            }

            _roller.Spherify = Mathf.Lerp( 0.0f, RollerConstants.instance.RitualMaxSpherize, RollerConstants.instance.RitualPopCurve.Evaluate( ritualTimer / RollerConstants.instance.RitualTime ) );
            //_roller.SpherifyScale = Mathf.Lerp( _roller.SpherifyScale, RollerConstants.instance.RitualSphereizeScale, Time.deltaTime * 15.0f );

            
		}
	}
}
