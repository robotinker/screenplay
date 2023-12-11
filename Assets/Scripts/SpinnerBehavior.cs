using UnityEngine;
using System.Collections;

public class SpinnerBehavior : MonoBehaviour {

	public delegate void on_spinner_stop (float my_rot);
	public static event on_spinner_stop onSpinnerStop;

	bool running;
	Rigidbody2D my_body;

	// Use this for initialization
	void Start () {
		my_body = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
		if (running && my_body.angularVelocity == 0)
		{
			running = false;
			onSpinnerStop(my_body.rotation);
		}
		else if (!running && my_body.angularVelocity != 0)
		{
			running = true;
		}
	}
}
