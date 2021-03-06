﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ButterflyData
{
	public Transform _butterflyTransform = null;

    public Vector3 _parentOffset = Vector3.zero;
	public Vector3 _targetPosition = Vector3.zero;
    public Vector3 _pivotDir = Vector3.zero;

    public float _moveSpeed = 0.0f;
    
    const float PIVOTSPEED_MAX = 5.0f;
	const float MOVESPEED_MIN = 1.0f;
	const float MOVESPEED_MAX = 15.0f;

    public Color _currColor = Color.white;

	public ButterflyData()
	{
        RandomizeMovement();
    }

    public void RandomizeMovement()
    {
        _moveSpeed = Random.Range( MOVESPEED_MIN, MOVESPEED_MAX );
        _pivotDir = JohnTech.RandomRangeVec( -PIVOTSPEED_MAX, PIVOTSPEED_MAX );
    }
}

public class ButterflyCloud : AmbientCreature {

    [SerializeField] GameObject _creatureObject = null;
    [SerializeField] private int _creatureCount = 2;

	[SerializeField] Gradient _butterflyGradient = null;	

    Vector3 _idlePosition = Vector3.zero;   // pivot point for butterfly group
    const float IDLE_MINDIST = 0.5f;    // How close butterflies get to idle position until the stop tryna get back

    int index = 0;  // b/c i'm a big baby and i'm doing gross iterations rn
    
	List<ButterflyData> _butterflyList = new List<ButterflyData>();

    // Sine Movement Values
	const float FLOATING_YOFFSET = 0.0f;
    const float FLOATING_YAMP = 0.05f;
    const float FLOATING_YVEL = 0.25f;

    // butterfly spawn values
    const float SPAWN_MINDIST = 0.5f;
    const float SPAWN_MAXDIST = 2f;
	const float SPAWN_HEIGHT = 5.0f;

	const float YOFFSET_MIN = -0.1f;
	const float YOFFSET_MAX = 2.0f;

	[ReadOnlyAttribute, SerializeField]Transform _focusTrans = null;
	Vector3 _focusDir = Vector3.zero;
    const float PLAYER_CHECKRADIUS = 9.0f;     // How big of a radius Butteflies check    
    const float PLAYER_APPROACHSPEED = 0.1f;    // How quickly butterflies chase
    const float FOCUS_LOOKATSPEED = 1.0f;

    // Use this for initialization
    void Awake ()
    {
        ColorManager.ExecutePaletteChange += HandlePaletteChange;

        _idlePosition = transform.position;
        
        SpawnCreatures();
	}

    public override void InitializeCreature( Vector3 startPos )
    {
        _idlePosition = startPos;

        foreach(ButterflyData data in _butterflyList)
        {
            data._butterflyTransform.position = this.transform.position + Random.insideUnitSphere * Random.Range( SPAWN_MINDIST, SPAWN_MAXDIST );
            data._parentOffset = data._butterflyTransform.position - this.transform.position;
            data._targetPosition = data._butterflyTransform.position;
        }

    }

    private void FixedUpdate()
    {

        if (_focusTrans == null)
        {
            // Check if an object of interest is within radius 
            Collider[] colArr = Physics.OverlapSphere( _idlePosition, PLAYER_CHECKRADIUS );

            if (colArr.Length > 0)
            {
                index = 0;
                while (_focusTrans == null && index < colArr.Length)
                {
                    if (colArr[index].gameObject.GetComponent<Player>())
                    {
                        _focusTrans = colArr[index].transform;
                    }

                    index++;
                }
            }

            // Move toward idle position if no focus transform
            if ( ( this.transform.position - _idlePosition ).magnitude > IDLE_MINDIST )
            {
                transform.position = Vector3.Lerp( transform.position, _idlePosition, PLAYER_APPROACHSPEED * Time.deltaTime );
            }
        }

        MoveButterflies();

        if ( _focusTrans != null )
        {
            _focusDir = _focusTrans.position - this.transform.position;
            _focusDir.y = 0.0f;

			if ( ( this.transform.position - _idlePosition ).magnitude < PLAYER_CHECKRADIUS )
            {
                this.transform.position += _focusDir.normalized * PLAYER_APPROACHSPEED * Time.deltaTime;
            }
            else
            {
                _focusTrans = null;
            }
        }
    }

	private void MoveButterflies()
	{
		// Sine Float the Swarm
		this.transform.SetPosY(_idlePosition.y + ( FLOATING_YAMP * Mathf.Sin( FLOATING_YVEL * ( Time.time ) ) ) );

		foreach( ButterflyData bData in _butterflyList )
		{
            // Pivot around parent
            bData._parentOffset = Quaternion.Euler( bData._pivotDir * Time.deltaTime ) * bData._parentOffset;
			bData._parentOffset.y = Mathf.Clamp( bData._parentOffset.y, YOFFSET_MIN, YOFFSET_MAX );	// Clamp 

            // Adjust the Target Positions for each butterfly
            bData._targetPosition = this.transform.position + bData._parentOffset;

            // And move the butterflies towards their target pos
            bData._butterflyTransform.position = Vector3.MoveTowards( bData._butterflyTransform.position, bData._targetPosition, bData._moveSpeed * Time.deltaTime );

            // Look at whatever the swarm is focused on
            if( _focusTrans != null)
            {
                bData._butterflyTransform.rotation = Quaternion.Slerp( bData._butterflyTransform.rotation, Quaternion.LookRotation( ( _focusTrans.position - bData._butterflyTransform.position ).normalized, Vector3.up ), FOCUS_LOOKATSPEED * Time.deltaTime );
            }            
		}
	}

    private void SpawnCreatures()
    {
        GameObject tmpCreature = null;
		ButterflyData tmpData = null;
        for ( int i = 0; i < _creatureCount; i++ )
        {
            tmpCreature = Instantiate( _creatureObject, this.transform) as GameObject;
            
            Color newColor = ColorManager.instance.ActivePalette.butterflyGradient.Evaluate( Random.value );
           
            tmpData = new ButterflyData();            

			tmpCreature.transform.position = transform.position + ( Random.insideUnitSphere * Random.Range( SPAWN_MINDIST, SPAWN_MAXDIST ) ) + ( Vector3.up * SPAWN_HEIGHT );
            tmpData._butterflyTransform = tmpCreature.transform;
			tmpData._parentOffset = tmpCreature.transform.position - this.transform.position;
			tmpData._targetPosition = tmpCreature.transform.position;

            tmpData._currColor = newColor;
            foreach (MeshRenderer m in tmpCreature.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                m.material.color = tmpData._currColor;
            }

            StartCoroutine( DelayedStartAnim( tmpCreature.GetComponent<Animator>() ) );

			_butterflyList.Add( tmpData );                     
        }
    }

	IEnumerator DelayedStartAnim( Animator anim )
	{
		yield return new WaitForSeconds( Random.Range( 0.1f, 0.75f ) );

		// This was deprecated in 5.6 and doesn't work. Using StartPlayback for now but I have no idea if it actually works.
		//anim.SetTime(0.0f);

        anim.Play("Take 001");

		//anim.StartPlayback();
	}

	// Worried about mem leak, need to look into disposing of non Mono classes
	private void OnDestroy()
	{
	}

    void HandlePaletteChange( ColorManager.EnvironmentPalette prevPallette, ColorManager.EnvironmentPalette nextPallette )
    {
        if( Application.isPlaying )
        {
            StartCoroutine( TransitionColor( nextPallette.butterflyGradient ) );
        }        
    }

    IEnumerator TransitionColor( Gradient gradient, float transitionTime = ColorManager.PALETTE_TRANSITIONTIME )
    {
        float timer = 0.0f;
        int index = 0;

        Color[] colorList = new Color[_butterflyList.Count];

        for(int i = 0; i < _butterflyList.Count; ++i)
        {
            colorList[i] = ColorManager.instance.ActivePalette.butterflyGradient.Evaluate( Random.value );
        }

        while ( timer < transitionTime )
        {
            timer += Time.deltaTime;

            foreach (ButterflyData b in _butterflyList )
            {
                foreach (MeshRenderer m in b._butterflyTransform.gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    m.material.color = Colorx.Slerp( b._currColor, colorList[index], timer / transitionTime );
                }
                index++;
            }

            index = 0;

            yield return 0;
        }

        index = 0;

        for( index = 0; index < _butterflyList.Count; ++index )
        {
            _butterflyList[index]._currColor = colorList[index];
        }
    }

}
