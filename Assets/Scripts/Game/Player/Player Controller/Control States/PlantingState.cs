﻿using UnityEngine;
using DG.Tweening;

public class PlantingState : RollerState 
{
    Tween _plantTween = null;

    public override void Enter( P_ControlState prevState )
    {
        Debug.Log( "[RollerState] ENTER PLANTING STATE" );

		_roller.Face.TransitionFacePose( "Plant Seed" );

        // Handle transition
        switch ( prevState )
        {
            case P_ControlState.CARRYING:            
                HandleBeginPlanting();
                break;
        }

    }

    public override void Exit( P_ControlState nextState )
    {
        Debug.Log("[RollerState] EXITTING PLANTING STATE");

        switch( nextState )
        {
            case P_ControlState.CARRYING:
                // Bring Seed back into hands
                _plantTween.Restart();				
                break;
            case P_ControlState.WALKING:
                HandleBothArmRelease();
				_roller.Face.BecomeIdle();                                
                break;
        }			

        if (_plantTween != null)
        {
            _plantTween.Kill();
            _plantTween = null;
        }
    }

    public override void HandleInput( InputCollection input )
    {
        // A BUTTON
//        if (!input.AButton.IsPressed)
//        {
//            // Return to Carry State
//            _roller.ChangeState( P_ControlState.CARRYING );
//        }

        // B BUTTON
        if (input.BButton.IsPressed)
        {
            // Drop Seed
            _roller.ChangeState( P_ControlState.WALKING );
        }

    }

    void HandleBeginPlanting()
    {
        // Right now just gonna move seed Down...
        if ( _roller.CurrentHeldObject != null )
        {
			if( _roller.CurrentHeldObject.GetComponent<Seed>() ) 
			{
                Vector3 plantPos = this.transform.position;
                plantPos.y = RollerConstants.instance.PlantingEndY;  
                plantPos += transform.forward * RollerConstants.instance.PlantingEndX;

				_plantTween = _roller.CurrentHeldObject.transform.DOMove(plantPos, RollerConstants.instance.PlantingTime ).OnComplete( () => HandlePlantingEnd() ).SetAutoKill( false ).SetEase(Ease.InBack); 
			}      
			else 
			{ 
				HandlePlantingEnd(); 
			} 
		}        
    }

    void HandlePlantingEnd()
    {
        // Handle a separate function for planting the seed

		Seed seed = _roller.CurrentHeldObject.GetComponent<Seed>();
		if( seed != null )
		{
			seed.TryPlanting();
		}

        CheckPlantEffectRadius();   // Maybe don't let this happen every time? idk

        HandleBothArmRelease();
        _roller.ChangeState( P_ControlState.WALKING);
    }

    void CheckPlantEffectRadius()
    {
        Collider[] colArray = Physics.OverlapSphere( this.transform.position, RollerConstants.instance.PlantingEffectRadius );
        BigPlantPickupable checkPlant = null;
        if( colArray.Length > 0 )
        {
            foreach( Collider c in colArray )
            {
                checkPlant = c.GetComponent<BigPlantPickupable>();
                if( checkPlant != null )
                {
                    checkPlant.PunchTreeRotation();
                    //checkPlant.ShiverTree();
                    CameraManager.instance.ScreenShake( 0.25f, 0.25f, 5, 15 );
                }
            }
        }
        
    }
}
