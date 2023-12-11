using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CursorBehavior : MonoBehaviour {

	public bool scaling_active = true;
	bool scale_homing = true;
	bool scale_set = false;
	public float scale_gain = 0.1f;
	Vector2 scale_target = new Vector2();
	Vector2 prev_scale = new Vector2();
	bool scale_linear = false;
	float scale_timer = 0f;
	float scale_duration = 0f;

	public bool translation_active = true;
	bool translation_homing = true;
	bool translation_set = false;
	public float translation_gain = 0.1f;
	Vector2 translation_target = new Vector2();
	Vector2 prev_pos = new Vector2();
	bool translation_linear = false;
	float translation_timer = 0f;
	float translation_duration = 0f;
	bool destroy_me_on_arrival = false;

	public bool rotation_active = true;
	bool rotation_homing = true;
	bool rotation_set = false;
	public float rotation_gain = 0.1f;
	float rotation_target = 0f;
	float prev_rot = 0f;
	bool rotation_linear = false;
	float rotation_timer = 0f;
	float rotation_duration = 0f;

	GameObject target_object;
	bool update_x_pos = false;
	bool update_y_pos = false;
	float y_offset = 0f;
	float x_offset = 0f;

	public bool text_color_shift_active = true;
	bool text_color_homing = true;
	bool text_color_set = false;
	Color target_text_color = Color.black;
	public float text_gain = 0.1f;
	Text[] text_children;

	public bool image_color_shift_active = true;
	bool image_color_homing = true;
	bool image_color_set = false;
	Color target_image_color = Color.white;
	public float image_gain = 0.1f;
	Image my_image;

	public bool image_alpha_shift_active = true;
	bool image_alpha_homing = true;
	bool image_alpha_set = false;
	float target_image_alpha = 1f;
	public float image_alpha_gain = 0.1f;
	bool fade_linear = false;
	float fade_timer = 0f;
	float fade_duration = 0f;
	float prev_alpha = 0f;

	public bool text_alpha_shift_active = true;
	bool text_alpha_homing = true;
	bool text_alpha_set = false;
	float target_text_alpha = 1f;
	public float text_alpha_gain = 0.1f;

	bool scale_pulsing = false;
	bool color_pulsing = false;
	bool alpha_pulsing = false;

	bool use_native_size = false;
	Vector2 native_size;

	ScriptManager script_manager;

/*	Dictionary<string, Color> valid_colors = new Dictionary<string, Color>{
		{"black", Color.black},
		{"blue", Color.blue},
		{"cyan", Color.cyan},
		{"gray", Color.gray},
		{"green", Color.green},
		{"grey", Color.grey},
		{"magenta", Color.magenta},
		{"red", Color.red},
		{"white", Color.white},
		{"yellow", Color.yellow},
	};
*/
	public void wait_to_set_native_size (float delay)
	{
		StartCoroutine(SetNativeSizeAfter(delay));
	}

	public void set_native_size (Vector2 new_size)
	{
		native_size = new_size;
		use_native_size = true;
	}

	// Use this for initialization
	void Start () {
		script_manager = GameObject.Find ("ScriptHolder").GetComponent<ScriptManager>();

		my_image = GetComponent<Image>();
		text_children = GetComponentsInChildren<Text>();

		if (!scale_set)
		{
			scale_target = new Vector2(transform.localScale.x, transform.localScale.y);
		}

		if (!translation_set)
		{
			translation_target = new Vector2 (transform.position.x, transform.position.y);

		}

		if (!rotation_set)
		{
			rotation_target = transform.rotation.eulerAngles.z;
		}

		if (!image_color_set)
		{
			target_image_color = my_image.color;

		}

		if (!image_alpha_set)
		{
			target_image_alpha = my_image.color.a;

		}
		if (transform.childCount > 0 && transform.GetChild (0).GetComponent<Text>() != null)
		{
			if (!text_color_set)
			{
				target_text_color = transform.GetChild (0).GetComponent<Text>().color;
			}
			if (!text_alpha_set)
			{
				target_text_alpha = transform.GetChild (0).GetComponent<Text>().color.a;
			}
		}
	}

	public void set_image_color_target (Color new_target, float new_gain = 0.1f)
	{
		target_image_color = new_target;
		image_color_set = true;
		image_color_homing = true;
		image_gain = new_gain;
	}

	public Color get_image_color_target ()
	{
		return target_image_color;
	}

	public void set_text_color_target (Color new_target, float new_gain = 0.1f)
	{
		target_text_color = new_target;
		text_color_set = true;
		text_color_homing = true;
		text_gain = new_gain;
	}

	public void set_text_alpha_target (float new_target, float new_gain = 0.1f)
	{
		target_text_alpha = new_target;
		text_alpha_set = true;
		text_alpha_homing = true;
		text_alpha_gain = new_gain;
		if (new_target == 0f)
		{
			strip_color_tags_from_text();
		}
	}

	public void set_image_alpha_target (float new_target, float new_gain = 0.1f, float new_timer = 0f)
	{
		target_image_alpha = new_target;
		image_alpha_set = true;
		image_alpha_homing = true;
		image_alpha_gain = new_gain;
		if (new_timer > 0f)
		{
			fade_linear = true;
			fade_timer = 0f;
			fade_duration = new_timer;
			my_image = GetComponent<Image>();
			prev_alpha = my_image.color.a;
			//Debug.Log ("Linear fade to " + target_image_alpha.ToString() + " over " + fade_duration.ToString () + " seconds");
		}
	}

	public void set_scale_target (Vector2 new_target, float new_gain = 0.1f, float new_timer = 0f)
	{
		scale_target = new_target;
		scale_set = true;
		scale_homing = true;
		scale_gain = new_gain;
		if (new_timer > 0f)
		{
			scale_linear = true;
			scale_timer = 0f;
			scale_duration = new_timer;
			prev_scale = new Vector2 (transform.localScale.x, transform.localScale.y);
		}
	}

	public void set_translation_target (Vector2 new_target, float new_gain = 0.1f, float new_timer = 0f, bool destroy_on_arrival = false)
	{
		translation_target = new_target;
		translation_set = true;
		translation_homing = true;
		translation_gain = new_gain;
		destroy_me_on_arrival = destroy_on_arrival;
		if (new_timer > 0f)
		{
			translation_linear = true;
			translation_timer = 0f;
			translation_duration = new_timer;
			prev_pos = new Vector2 (transform.position.x, transform.position.y);
		}
	}

	public void set_rotation_target (float new_target, float new_gain = 0.1f, float new_timer = 0f)
	{
		rotation_target = new_target;
		rotation_set = true;
		rotation_homing = true;
		rotation_gain = new_gain;
		if (new_timer > 0f)
		{
			rotation_linear = true;
			rotation_timer = 0f;
			rotation_duration = new_timer;
			prev_rot = transform.rotation.eulerAngles.z;
		}
	}
	
	public void set_x_next_frame (GameObject my_target, float offset = 0f)
	{
		x_offset = offset;
		target_object = my_target;
		update_x_pos = true;
	}

	public void set_y_next_frame (GameObject my_target, float offset = 0f)
	{
		y_offset = offset;
		target_object = my_target;
		update_y_pos = true;
	}

	public void pulse(string pulse_type, float intensity, float duration, float new_gain = 0.1f, string new_color = "black")
	{
		bool okay_to_proceed = true;

		switch (pulse_type)
		{
		case "scale":
			okay_to_proceed = !scale_pulsing;
			break;
		case "squish":
			okay_to_proceed = !scale_pulsing;
			break;
		case "color":
			okay_to_proceed = !color_pulsing;
			break;
		case "tint":
			okay_to_proceed = !color_pulsing;
			break;
		case "alpha":
			okay_to_proceed = !alpha_pulsing;
			break;
		}

		if (okay_to_proceed)
		{
			StartCoroutine (Pulse(pulse_type, intensity, duration, new_gain, new_color));
		}
	}

	IEnumerator SetNativeSizeAfter (float delay)
	{
		float timer = 0f;
		while (timer < delay)
		{
			timer += Time.deltaTime;
			yield return null;
		}

		set_native_size(new Vector2(transform.localScale.x, transform.localScale.y));
	}

	IEnumerator Pulse (string pulse_type, float intensity, float duration, float new_gain = 0.1f, string new_color = "black")
	{
		Vector2 prev_scale = new Vector2(transform.localScale.x, transform.localScale.y);
		Color prev_color = Color.white;
		if (my_image != null)
		{
			prev_color = my_image.color;
		}

		float prev_gain = 0.1f;

		switch (pulse_type)
		{
		case "squish":
			scale_pulsing = true;
			set_scale_target(new Vector2(prev_scale.x * (intensity / 0.5f), prev_scale.y / (intensity / 0.5f)), new_gain, duration / 2f);
			prev_gain = scale_gain;
			break;
		case "scale":
			scale_pulsing = true;
			set_scale_target(prev_scale * intensity, new_gain, duration / 2f);
			prev_gain = scale_gain;
			break;
		case "color":
			color_pulsing = true;
			set_image_color_target(new Color(prev_color.r * intensity, prev_color.g * intensity, prev_color.b * intensity, prev_color.a), new_gain);
			prev_gain = image_gain;
			break;
		case "tint":
			color_pulsing = true;
			Color specified_color = script_manager.find_color (new_color);

			/*Color specified_color = Color.black;
			string[] split_color = new_color.Split (',');
			if (valid_colors.ContainsKey(new_color))
			{
				specified_color = valid_colors[new_color];
			}
			else if (split_color.Length == 3)
			{
				specified_color = new Color(float.Parse(split_color[0]), float.Parse (split_color[1]), float.Parse (split_color[2]));
			}
			else
			{
				specified_color = Color.black;
			}*/

			Color dest_color = new Color(prev_color.r + (specified_color.r - prev_color.r) * intensity, prev_color.g + (specified_color.g - prev_color.g) * intensity, prev_color.b + (specified_color.b - prev_color.b) * intensity, prev_color.a);
			set_image_color_target(dest_color, new_gain);
			prev_gain = image_gain;
			break;
		case "alpha":
			alpha_pulsing = true;
			set_image_alpha_target(prev_color.a * intensity, new_gain);
			prev_gain = image_alpha_gain;
			break;
		}

		float timer = 0f;
		while (timer < duration / 2f)
		{
			timer += Time.deltaTime;
			yield return null;
		}

		switch (pulse_type)
		{
		case "scale":
			if (use_native_size)
			{
				set_scale_target(native_size, new_gain, duration / 2f);
			}
			else
			{
				set_scale_target(prev_scale, new_gain, duration / 2f);
			}
			break;
		case "squish":
			if (use_native_size)
			{
				set_scale_target(native_size, new_gain, duration / 2f);
			}
			else
			{
				set_scale_target(prev_scale, new_gain, duration / 2f);
			}			break;
		case "color":
			set_image_color_target(new Color(prev_color.r, prev_color.g, prev_color.b, my_image.color.a));
			break;
		case "tint":
			set_image_color_target(new Color(prev_color.r, prev_color.g, prev_color.b, my_image.color.a));
			break;
		case "alpha":
			set_image_alpha_target(prev_color.a);
			break;
		}

		while (timer < duration / 2f)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		
		switch (pulse_type)
		{
		case "scale":
			scale_pulsing = false;
			scale_gain = prev_gain;
			break;
		case "squish":
			scale_pulsing = false;
			scale_gain = prev_gain;
			break;
		case "color":
			color_pulsing = false;
			image_gain = prev_gain;
			break;
		case "tint":
			color_pulsing = false;
			image_gain = prev_gain;
			break;
		case "alpha":
			alpha_pulsing = false;
			image_alpha_gain = prev_gain;
			break;
		}
	}

	void strip_color_tags_from_text ()
	{
		foreach (Text text_item in GetComponentsInChildren<Text>())
		{
			while (text_item.text.Contains("</color>"))
			{
				int start_index = text_item.text.IndexOf("</color>");
				int end_index = start_index + 7;
				if (text_item.text.Length > end_index)
				{
					text_item.text = text_item.text.Substring(0, start_index) + text_item.text.Substring(end_index + 1);

				}
				else
				{
					text_item.text = text_item.text.Substring(0, start_index);
				}
			}

			while (text_item.text.Contains("<color"))
			{
				int start_index = text_item.text.IndexOf("<color");
				int end_index = start_index;
				while (end_index < text_item.text.Length && text_item.text[end_index] != '>')
				{
					end_index += 1;
				}

				if (text_item.text.Length > end_index)
				{
					text_item.text = text_item.text.Substring(0, start_index) + text_item.text.Substring(end_index + 1);

				}
				else
				{
					text_item.text = text_item.text.Substring(0, start_index);
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (ScriptManager.Game.current.running)
		{
			if (translation_active && translation_homing && (transform.position.x != translation_target.x || transform.position.y != translation_target.y))
			{
				if (update_x_pos)
				{
					update_x_pos = false;
					translation_target = new Vector2 (target_object.transform.position.x + target_object.GetComponent<RectTransform>().rect.width * x_offset, translation_target.y);
				}
				
				if (update_y_pos)
				{
					update_y_pos = false;
					translation_target = new Vector2 (translation_target.x, target_object.transform.position.y + target_object.GetComponent<RectTransform>().rect.height * y_offset);
				}
				
				if (Mathf.Abs(translation_target.x - transform.position.x) < 0.01f && Mathf.Abs(translation_target.y - transform.position.y) < 0.01f || (translation_linear && translation_timer >= translation_duration))
				{
					transform.position = new Vector3(translation_target.x, translation_target.y, 0f);
					translation_homing = false;
					if (destroy_me_on_arrival)
					{
						Destroy(gameObject);
					}

				}
				else
				{
					if (translation_linear)
					{
						translation_timer += Time.deltaTime;
						transform.position = new Vector3(prev_pos.x, prev_pos.y) + new Vector3(translation_target.x - prev_pos.x, translation_target.y - prev_pos.y, 0f) * translation_timer / translation_duration;
						
					}
					else
					{
						transform.Translate (new Vector3(translation_target.x - transform.position.x, translation_target.y - transform.position.y, 0f) * translation_gain);
						
					}
				}
			}
			
			if (rotation_active && rotation_homing && transform.rotation.eulerAngles.z != rotation_target)
			{
				if (Mathf.Abs(rotation_target - transform.rotation.eulerAngles.z) < 0.01f || (rotation_linear && rotation_timer >= rotation_duration))
				{
					transform.rotation = Quaternion.Euler(Vector3.forward * rotation_target);
					rotation_homing = false;
				}
				else
				{
					if (rotation_linear)
					{
						rotation_timer += Time.deltaTime;
						transform.rotation = Quaternion.Euler (Vector3.forward * (rotation_target - prev_rot) * rotation_timer / rotation_duration);
						
					}
					else
					{
						transform.Rotate (new Vector3(0f,0f, (rotation_target - transform.rotation.eulerAngles.z) * translation_gain));
						
					}
				}
			}
			
			if (scaling_active && scale_homing)
			{
				if (Mathf.Abs (transform.localScale.x - scale_target.x) < 0.01f && Mathf.Abs (transform.localScale.y - scale_target.y) < 0.01f || (scale_linear && scale_timer >= scale_duration))
				{
					//my_image.rectTransform.rect.Set(my_image.rectTransform.rect.xMin, my_image.rectTransform.rect.yMax, scale_target.x, scale_target.y);
					transform.localScale = new Vector3(scale_target.x, scale_target.y, 1f);
					scale_homing = false;
				}
				else
				{
					if (scale_linear)
					{
						scale_timer += Time.deltaTime;
						transform.localScale = new Vector3(prev_scale.x, prev_scale.y) + new Vector3(scale_target.x - prev_scale.x, scale_target.y - prev_scale.y, 0f) * scale_timer / scale_duration;
						
					}
					else
					{
						transform.localScale = new Vector3(transform.localScale.x + (scale_target.x - transform.localScale.x) * scale_gain, transform.localScale.y + (scale_target.y - transform.localScale.y) * scale_gain, 1f);
						
					}
				}
			}
			
			if (image_alpha_shift_active && image_alpha_homing)
			{
				if (Mathf.Abs (target_image_alpha - my_image.color.a) < 0.01f || (fade_linear && fade_timer >= fade_duration))
				{
					my_image.color = new Color(my_image.color.r, my_image.color.g, my_image.color.b, target_image_alpha);
					image_alpha_homing = false;
				}
				else
				{
					if (fade_linear)
					{
						fade_timer += Time.deltaTime;
						my_image.color = new Color(my_image.color.r, my_image.color.g, my_image.color.b, prev_alpha + (target_image_alpha - prev_alpha) * fade_timer / fade_duration);
						
					}
					else
					{
						my_image.color = new Color(my_image.color.r, my_image.color.g, my_image.color.b, my_image.color.a + (target_image_alpha - my_image.color.a) * image_alpha_gain);
					}
				}
			}
			
			if (text_alpha_shift_active && text_alpha_homing)
			{
				bool should_home = false;
				foreach (Text text_item in text_children)
				{
					if (Mathf.Abs (target_text_alpha - text_item.color.a) < 0.01f)
					{
						text_item.color = new Color(text_item.color.r, text_item.color.g, text_item.color.b, target_text_alpha);
						
					}
					else
					{
						text_item.color = new Color(text_item.color.r, text_item.color.g, text_item.color.b, text_item.color.a + (target_text_alpha - text_item.color.a) * text_alpha_gain);
						should_home = true;
					}
				}
				text_alpha_homing = should_home;
			}
			
			if (image_color_shift_active && image_color_homing)
			{
				if (Mathf.Abs (target_image_color.r - my_image.color.r) < 0.01f && Mathf.Abs (target_image_color.g - my_image.color.g) < 0.01f && Mathf.Abs (target_image_color.b - my_image.color.b) < 0.01f)
				{
					my_image.color = new Color(target_image_color.r, 
					                           target_image_color.g, 
					                           target_image_color.b,
					                           my_image.color.a
					                           );
					image_color_homing = false;
				}
				else
				{
					my_image.color = new Color(my_image.color.r + (target_image_color.r - my_image.color.r) * image_gain, 
					                           my_image.color.g + (target_image_color.g - my_image.color.g) * image_gain, 
					                           my_image.color.b + (target_image_color.b - my_image.color.b) * image_gain,
					                           my_image.color.a
					                           );
				}
				
			}
			
			if (text_color_shift_active && text_color_homing)
			{
				bool should_home = false;
				foreach (Text text_item in text_children)
				{
					if (Mathf.Abs (target_text_color.r - text_item.color.r) < 0.01f && Mathf.Abs (target_text_color.g - text_item.color.g) < 0.01f && Mathf.Abs (target_text_color.b - text_item.color.b) < 0.01f)
					{
						text_item.color = new Color(target_text_color.r, 
						                            target_text_color.g, 
						                            target_text_color.b,
						                            text_item.color.a
						                            );	
					}
					else
					{
						text_item.color = new Color(text_item.color.r + (target_text_color.r - text_item.color.r) * text_gain, 
						                            text_item.color.g + (target_text_color.g - text_item.color.g) * text_gain, 
						                            text_item.color.b + (target_text_color.b - text_item.color.b) * text_gain,
						                            text_item.color.a
						                            );	
						should_home = true;
					}
					
				}
				text_color_homing = should_home;
			}
		}
		}

}
