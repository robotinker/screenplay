using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SpriteAnimator : MonoBehaviour {

	List<Sprite> sprite_list = new List<Sprite>();
	List<float> frame_durations = new List<float>();
	Image my_image;

	float timer = 0f;
	int current_frame = 0;
	bool is_active = false;
	int loop_index = 0;
	bool looping = false;
	
	// Use this for initialization
	void Start () {
		my_image = gameObject.GetComponent<Image>();
	}

	public void set_anim (List<Sprite> my_sprites, List<float> my_times)
	{
		sprite_list = my_sprites;
		frame_durations = my_times;
		while (frame_durations.Count < sprite_list.Count)
		{
			frame_durations.Add (frame_durations[frame_durations.Count - 1]);
		}
	}

	public void set_loop_index(int new_val)
	{
		loop_index = new_val;
	}

	public void set_looping (bool new_val)
	{
		looping = new_val;
	}

	public void play()
	{
		is_active = true;
		timer = 0f;
		current_frame = 0;
		my_image.sprite = sprite_list[0];
	}

	public void stop()
	{
		is_active = false;
	}

	public void resume()
	{
		is_active = true;
	}

	// Update is called once per frame
	void Update () {
		if (is_active && ScriptManager.Game.current.running)
		{
			timer += Time.deltaTime;

			if (timer >= frame_durations[current_frame]) // SWITCH FRAME
			{
				timer -= frame_durations[current_frame];

				if (sprite_list.Count > current_frame + 1)
				{
					current_frame += 1;
					my_image.sprite = sprite_list[current_frame];
				}
				else if (looping)
				{
					current_frame = loop_index;
					my_image.sprite = sprite_list[current_frame];

				}
				else // No more frames and not looping
				{
					is_active = false;
				}
			}

		}
	}
}
