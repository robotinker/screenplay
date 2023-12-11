using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviour 
{
	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	void Update () 
	{
		if (Input.GetButtonDown("Exit") || (Input.GetButton("ControllerExit1") && Input.GetButton("ControllerExit2")))
		{
            System.Diagnostics.Process.Start("/Users/ted/Projects/GameBuilds/GameShell.app");

			Application.Quit();
		}
	}
}
