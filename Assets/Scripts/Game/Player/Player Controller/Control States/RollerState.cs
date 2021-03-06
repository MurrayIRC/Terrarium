﻿using UnityEngine;

// Interface for character control states.
public class RollerState : MonoBehaviour
{
	// =================
	// V A R I A B L E S
	// =================

	protected RollerController _roller;
	public RollerController RollerParent { get { return _roller; } set { _roller = value; } }

    int _grabbedObjPrevLayer = 0;

	// ============================
	// V I R T U A L  M E T H O D S
	// ============================

	public virtual void Enter( P_ControlState prevState ) {}
	public virtual void Exit( P_ControlState nextState ) {}

	public virtual void HandleFixedInput( InputCollection input ) {}
    public virtual void HandleInput( InputCollection input ) { }

	// ==========================
	// H E L P E R  M E T H O D S
	// ==========================

	protected void HandlePickup( PlayerArmIK.ArmType armType )
	{
		CheckForPickupObject( armType );
	}
		
	private void CheckForPickupObject( PlayerArmIK.ArmType armType )
	{
		if( _roller.IK.ArmFocus == null )
		{
			Vector3 _pickupCenter = _roller.transform.position + ( Vector3.up * RollerConstants.instance.PickupCheckHeight ) + _roller.transform.forward;	

			Collider[] overlapArray = Physics.OverlapSphere( _pickupCenter, RollerConstants.instance.PickupCheckRadius );

			if ( overlapArray.Length > 0 )
			{
				Pickupable pickup = null;
				//if the pickupable is a plant, we can only pick it up if it's still in seed stage
				if( overlapArray.Length > 0 )
				{
					foreach ( Collider col in overlapArray )
					{
						pickup = col.gameObject.GetComponent<Pickupable>();

						if ( pickup != null )
						{
							if( pickup.GetComponent<BigPlantPickupable>() != null 
								&& (int)pickup.GetComponent<BPGrowthController>().CurStage < 1 )
							{
								// Hacky way to check if you can Hug a Tree yet
								pickup = null;
							}
							else
							{
								break;	
							}
						}
					}
				}            

				if ( pickup != null )
				{
					_roller.IK.SetArmTarget( pickup.transform, armType );	
				}

			}
		}
		else
		{
			_roller.IK.SetArmTarget( _roller.IK.ArmFocus, armType );
		}
		
	}

    protected void CheckForReachable( PlayerArmIK.ArmType armType )
    {
        Vector3 _pickupCenter = _roller.transform.position + ( Vector3.up * RollerConstants.instance.PickupCheckHeight );

        Collider[] overlapArray = Physics.OverlapSphere( _pickupCenter, RollerConstants.instance.IKReachCheckRadius, 1 << LayerMask.NameToLayer("Touchable") );

        if (overlapArray.Length > 0)
        {
            Transform reachingTrans = overlapArray[Random.Range( 0, overlapArray.Length )].transform;
            armType = Vector3.Distance( _roller.IK.LeftArm.transform.position, reachingTrans.position ) < Vector3.Distance( _roller.IK.RightArm.transform.position, reachingTrans.position ) ? PlayerArmIK.ArmType.LEFT : PlayerArmIK.ArmType.RIGHT;
            SetAmbientArmReach( reachingTrans, armType );
        }
    }

    private void SetAmbientArmReach( Transform reachable, PlayerArmIK.ArmType armType )
    {
        _roller.IK.SetAmbientReachTarget( reachable, armType );
    }

	public void HandleGrabObject()
	{
		PickUpObject( _roller.IK.ArmFocus.GetComponent<Pickupable>() );		
	}

	private void PickUpObject( Pickupable pickup )
	{
		/*
		if (pickup.GetComponent<Bibi>())
		{
			HandleBibiPickup(pickup as Bibi);
            return;
		}
		*/

		_roller.IK.HandleArmsGrab();

		_roller.CurrentHeldObject = pickup;

		if( pickup.Carryable )
		{
			_grabbedObjPrevLayer = _roller.CurrentHeldObject.gameObject.layer;
			_roller.CurrentHeldObject.gameObject.layer = LayerMask.NameToLayer("HeldObject");

			_roller.Player.AnimationController.SetLifting( true );
			_roller.IK.DisableArmIK();
		}        

		_roller.ChangeState( P_ControlState.PICKINGUP );		

		AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_ACTIONFX, 1 );
	}

	/*
	void HandleBibiPickup(Bibi bibi)
	{
		bibi.OnPickup();
		_roller.IK.LetGoBothArms();
	}
	*/

	public void HandleBothArmRelease()
	{
		_roller.IK.LetGoBothArms();

		if ( _roller.CurrentHeldObject != null )
		{
			if( _roller.CurrentHeldObject.Carryable )
			{
				_roller.CurrentHeldObject.gameObject.layer = _grabbedObjPrevLayer;
				_roller.Player.AnimationController.SetLifting( false );
				_roller.IK.EnableArmIK();
			}
		    _roller.CurrentHeldObject.DropSelf();
			_roller.CurrentHeldObject = null;
		}
	}

	protected void HandleArmRelease( PlayerArmIK.ArmType armType )
	{
		DropHeldObject( armType );
	}

	void DropHeldObject( PlayerArmIK.ArmType armType )
	{
		// update the ik
		_roller.IK.LetGoOneArm( armType );

		if ( _roller.IK.ArmsIdle && _roller.CurrentHeldObject != null )
		{
			if( _roller.CurrentHeldObject.Carryable )
			{
				_roller.CurrentHeldObject.gameObject.layer = _grabbedObjPrevLayer;
			}
		    _roller.CurrentHeldObject.DropSelf();
			_roller.CurrentHeldObject = null;
		}
		
	}

    protected void IncrementLeftArmGesture()
    {
        _roller.IK.LeftArm.IncrementGestureIndex();
    }
    protected void IncrementRightArmGesture()
    {
        _roller.IK.RightArm.IncrementGestureIndex();
    }
}
