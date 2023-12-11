using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DynamicMeter : MonoBehaviour {

	Slider green_bar;
	Slider red_bar;
	Slider white_bar;

	public float wait_time = 2.0f;
	float wait_timer = 0f;
	public float slide_time_initial = 0.5f;
	public float slide_time_final = 1f;
	float slide_timer = 0f;

	//float test_timer = 0f;
	//float test_direction = -1f;

	float prev_val = 0f;
	float final_val = 1f;

	// Use this for initialization
	void Awake () {
		green_bar = transform.Find("GreenBar").GetComponent<Slider>();
		red_bar = transform.Find("RedBar").GetComponent<Slider>();
		white_bar = transform.Find("WhiteBar").GetComponent<Slider>();

		//set_val(0.5f);
	}

	public void set_val (float new_val)
	{
		red_bar.value = new_val;
		white_bar.value = new_val;
		green_bar.value = new_val;
	}

	public void set_target_val (float target_val)
	{
		prev_val = white_bar.value;
		final_val = target_val;
		wait_timer = wait_time;
		slide_timer = 0f;
		if (final_val > white_bar.value)
		{
			//green_bar.value = final_val;
			red_bar.value = white_bar.value;
		}
		else
		{
			red_bar.value = white_bar.value;
			//white_bar.value = final_val;
			green_bar.value = final_val;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (wait_timer > 0f)
		{
			wait_timer -= Time.deltaTime;

			// INITIAL SLIDE
			if (final_val > prev_val && green_bar.value < final_val)
			{
				slide_timer += Time.deltaTime;
				green_bar.value = Mathf.Lerp(prev_val, final_val, slide_timer / slide_time_initial);
				if (green_bar.value >= final_val) // handle overshoot
				{
					green_bar.value = final_val;
				}
			}
			else if (final_val < prev_val && white_bar.value > final_val)
			{
				slide_timer += Time.deltaTime;
				white_bar.value = Mathf.Lerp(prev_val, final_val, slide_timer / slide_time_initial);
				if (white_bar.value <= final_val) // handle overshoot
				{
					white_bar.value = final_val;
				}
			}

			if (wait_timer <= 0f)
			{
				wait_timer = 0f;
				slide_timer = 0f;
			}
		}

		// FINAL SLIDE
		else if (final_val > prev_val && white_bar.value < green_bar.value)
		{
			slide_timer += Time.deltaTime;
			white_bar.value = Mathf.Lerp(prev_val, final_val, slide_timer / slide_time_final);
			if (white_bar.value >= final_val) // handle overshoot
			{
				white_bar.value = final_val;
			}
		}
		else if (final_val < prev_val && red_bar.value > white_bar.value)
		{
			slide_timer += Time.deltaTime;
			red_bar.value = Mathf.Lerp(prev_val, final_val, slide_timer / slide_time_final);
			if (red_bar.value <= final_val) // handle overshoot
			{
				red_bar.value = final_val;
			}
		}

		/*test_timer += Time.deltaTime;
		if (test_timer > 4f)
		{
			test_timer = 0f;
			test_direction *= -1f;
			set_target_val(white_bar.value + 0.2f * test_direction);
		}*/
	}
}
