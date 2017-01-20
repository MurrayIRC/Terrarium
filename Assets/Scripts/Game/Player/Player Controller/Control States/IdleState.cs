﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : RollerState
{
    
    void Awake()
    {
        _controlState = P_ControlState.IDLING;
    }
    	
    public override void Enter( RollerController parent, P_ControlState prevState )
    {
        Debug.Log( "ENTER IDLE STATE" );

        // Handle transition
        switch( prevState )
        {
            case P_ControlState.WALKING:
                break;
        }

        // Set Idle Anim
        PlayerManager.instance.Player.AnimationController.PlayIdleAnim();

        roller = parent;
    }

    public override void Exit( P_ControlState nextState )
    {
        Debug.Log( "EXITTING IDLE STATE" );
    }

    public override void HandleInput( InputCollection input )
    {
        Vector3 vec = new Vector2( input.LeftStickX, input.LeftStickY );

        if( vec.magnitude > IDLE_MAXMAG )
        {
            roller.ChangeState( Idling, Walking );
        }

    }
}
