﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPDeathController : PlantController
{
	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Death;
	}
		
	public override void UpdateState(){}
	public override void StopState(){}
	public override void StartState(){}

	public override void WaterPlant(){}
	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}
}
