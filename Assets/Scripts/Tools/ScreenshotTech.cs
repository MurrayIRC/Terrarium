﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotTech : MonoBehaviour {

	Coroutine _screenshotRoutine = null;
	const float SCREENSHOT_TIMER = 30.0f;
	const string SCREENSHOT_INDEXSAVEKEY = "ScreenshotIndex";
	const string SCREENSHOT_SAVEFOLDERNAME = "Screenshots";

	// Use this for initialization
	void Awake () 
	{		

		#if !UNITY_EDITOR
		if( !Directory.Exists( Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME ) )
		{
			Directory.CreateDirectory( Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME );
		}

        // Uncomment to start the game w/ Screenshots Enabled
		//_screenshotRoutine = StartCoroutine( DelayedCaptureScreenshot() );
		#else
		if( !Directory.Exists( Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME ) )
		{
			Directory.CreateDirectory( Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME );
		}
		#endif
	}
	
	// Update is called once per frame
	void Update () 
	{
        if ( Input.GetKeyDown( KeyCode.P ) )
        {
			//HandleScreenShot();
			StartCoroutine(ScreenshotEncode());
        }
        else if ( Input.GetKeyDown( KeyCode.Alpha9 ) )
        {
            if (_screenshotRoutine != null)
            {
                StopCoroutine( _screenshotRoutine );
                _screenshotRoutine = null;
            }
            else
            {
                _screenshotRoutine = StartCoroutine( DelayedCaptureScreenshot() );
            }
        }
	}

	void HandleScreenShot( int screenshotDetail = 4 )
	{	
		#if UNITY_STANDALONE && !UNITY_EDITOR	
		Application.CaptureScreenshot( Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmm") + ".png", screenshotDetail );
		#else
		Application.CaptureScreenshot( Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmm") + ".png", screenshotDetail );
		#endif

		Debug.Log(UIManager.GetPanelOfType<PanelOverlay>().ScreenshotOverlay.color);
		UIManager.GetPanelOfType<PanelOverlay>().ScreenshotOverlay.color = new Color( 1.0f, 1.0f, 1.0f, 0.0f );
		Debug.Log(UIManager.GetPanelOfType<PanelOverlay>().ScreenshotOverlay.color);
	}

	IEnumerator DelayedCaptureScreenshot()
	{
		yield return new WaitForSeconds( SCREENSHOT_TIMER );

		//HandleScreenShot( 2 );
		StartCoroutine( CaptureOverlayRoutine() );

		_screenshotRoutine = StartCoroutine( DelayedCaptureScreenshot() );
	}

	IEnumerator CaptureOverlayRoutine( int screenshotDetail = 4 )
	{
		UIManager.GetPanelOfType<PanelOverlay>().ScreenshotOverlay.color = Color.white;

		yield return new WaitForEndOfFrame();

		HandleScreenShot( screenshotDetail );

		yield return new WaitForEndOfFrame();
	}
}
