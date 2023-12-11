using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DynamicIntegerText : MonoBehaviour {

	Text my_text;

	int prev_val = 0;
	int final_val = 0;
	float interim_val = 0f;
	int rounded_interim = 0;

	public float wait_time = 2f;
	float wait_timer = 0f;
	public float transfer_time = 1f;
	float transfer_timer = 1f;

	//float test_timer = 0f;
	//int test_diff = -1;


	// Use this for initialization
	void Start () {
		my_text = GetComponent<Text>();
		//my_text.text = "4";
	}
		
	public void set_target_val (int new_val)
	{
		if (int.TryParse(my_text.text, out prev_val))
		{
			wait_timer = wait_time;
			final_val = new_val;
			interim_val = prev_val * 1f;

			if (final_val > prev_val)
			{
				my_text.text += " <color=green>+" + (final_val - prev_val).ToString() + "</color>";

			}
			else
			{
				my_text.text += " <color=red>" + (final_val - prev_val).ToString() + "</color>";

			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (wait_timer > 0f)
		{
			wait_timer -= Time.deltaTime;
			if (wait_timer <= 0f)
			{
				wait_timer = 0f;
				transfer_timer = 0f;
			}
		}
		else if (transfer_timer < transfer_time)
		{
			transfer_timer += Time.deltaTime;
			if (transfer_timer >= transfer_time)
			{
				my_text.text = final_val.ToString();
			}
			else
			{
				interim_val = Mathf.Lerp(prev_val * 1f, final_val * 1f, transfer_timer / transfer_time);
				rounded_interim = Mathf.RoundToInt(interim_val);
				if (final_val > prev_val)
				{
					my_text.text = rounded_interim.ToString() + " <color=green>+" + (final_val - rounded_interim).ToString() + "</color>";

				}
				else
				{
					my_text.text = rounded_interim.ToString() + " <color=red>" + (final_val - rounded_interim).ToString() + "</color>";

				}
			}

		}
/*
		test_timer += Time.deltaTime;
		if (test_timer > 4f)
		{
			test_timer = 0f;
			test_diff *= -2;
			set_new_val (4 + test_diff);
		}*/
	}
}
