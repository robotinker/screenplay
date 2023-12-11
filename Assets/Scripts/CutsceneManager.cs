using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CutsceneManager : MonoBehaviour {

	GameObject my_canvas;
	List<CutScene> playing_scenes = new List<CutScene>();
	float screen_width;
	float screen_height;

	public static Dictionary<string, GameObject> actor_dict = new Dictionary<string, GameObject>();
	
	AudioSource SFX_emitter;

	public delegate void cutscenes_done ();
	public static event cutscenes_done onCutscenesDone;
	
	[System.Serializable]

	public class Direction
	{
		public Dictionary<string, string> arg_dict = new Dictionary<string, string>();
		public string command = "";

		public Direction (string my_command)
		{
			command = my_command;
		}
	}

	[System.Serializable]

	public class TimePoint
	{
		public float time;
		public List<Direction> directions = new List<Direction>();

		public TimePoint (float my_time)
		{
			time = my_time;
		}

		public void execute(Dictionary<string, string> image_dict, GameObject my_canvas, float screen_width, float screen_height, AudioSource SFX_emitter, List<string> no_stretch_list)
		{
			foreach (Direction this_direction in directions)
			{
				//Debug.Log (this_direction.command);
				if (this_direction.command.Contains (":"))
				{
					if (this_direction.command.Split (':')[0].Trim () == "sound")
					{
						SFX_emitter.clip = Resources.Load (ScriptManager.Game.current.get_file_path("sounds/" + this_direction.command.Split (':')[1].Trim ()), typeof(AudioClip)) as AudioClip;
						SFX_emitter.Play ();
					}
					else if (this_direction.command.Split (':')[0].Trim () == "text")
					{
						string phrase = this_direction.command.Split (':')[1].Trim ();
						DialogueManager dialogue_manager = GameObject.Find ("ScriptHolder").GetComponent<DialogueManager>();
						dialogue_manager.set_color_mode("nar");
						dialogue_manager.new_message(phrase);
					}
				}
				else if (this_direction.command == "clear text")
				{
					DialogueManager dialogue_manager = GameObject.Find ("ScriptHolder").GetComponent<DialogueManager>();
					dialogue_manager.hide_speech_box();
				}
				else if (this_direction.command.StartsWith ("fade in "))
				{
					string actor_key = this_direction.command.Substring (8);
					GameObject new_actor = Instantiate(Resources.Load ("prefabs/CutSceneLayer") as GameObject) as GameObject;
					CutsceneManager.actor_dict.Add(actor_key, new_actor);

					if (ScriptManager.Game.current.is_anim(image_dict[actor_key]))
					{
						ScriptManager.Game.current.get_anim(image_dict[actor_key]).apply_to(new_actor);
						new_actor.GetComponent<SpriteAnimator>().play ();
					}
					new_actor.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("cutscene images/" + image_dict[actor_key]), typeof(Sprite)) as Sprite;

					if (no_stretch_list.Contains (actor_key))
					{
						new_actor.GetComponent<Image>().preserveAspect = true;
					}

					new_actor.transform.SetParent (my_canvas.transform);
					new_actor.GetComponent<RectTransform>().offsetMax = new Vector2(0,0);

					new_actor.GetComponent<Image>().color = new Color(1f,1f,1f,0f);
					float fade_time = 0.5f;
					if (this_direction.arg_dict.ContainsKey("duration"))
					{
						fade_time = float.Parse (this_direction.arg_dict["duration"]);
					}
					new_actor.GetComponent<CursorBehavior>().set_image_alpha_target(1f, new_timer: fade_time);
				}
				else if (this_direction.command.StartsWith ("fade out "))
				{
					string actor_key = this_direction.command.Substring (9);

					if (CutsceneManager.actor_dict.ContainsKey(actor_key))
					{
						GameObject this_actor = CutsceneManager.actor_dict[actor_key];
						float fade_time = 1f;
						if (this_direction.arg_dict.ContainsKey("duration"))
						{
							fade_time = float.Parse (this_direction.arg_dict["duration"]);
						}
						this_actor.GetComponent<CursorBehavior>().set_image_alpha_target(0f, new_timer: fade_time);
					}
					else
					{
						Debug.Log ("Actor not found for fading out: " + actor_key);

					}
				}
				else if (this_direction.command.StartsWith ("pan "))
				{
					string actor_key = this_direction.command.Substring (4);
					if (CutsceneManager.actor_dict.ContainsKey(actor_key))
					{
						if (this_direction.arg_dict.ContainsKey ("point"))
						{
							GameObject this_actor = CutsceneManager.actor_dict[actor_key];
							float effect_time = 1f;
							if (this_direction.arg_dict.ContainsKey("duration"))
							{
								effect_time = float.Parse (this_direction.arg_dict["duration"]);
							}
							string coordinates = this_direction.arg_dict["point"].Substring(1, this_direction.arg_dict["point"].Length - 2);
							float pan_x = float.Parse(coordinates.Split(';')[0].Trim());
							float pan_y = float.Parse(coordinates.Split(';')[1].Trim());
							this_actor.GetComponent<CursorBehavior>().set_translation_target(new Vector2(pan_x * screen_width, pan_y * screen_height), new_timer: effect_time);
						}
						else
						{
							Debug.Log ("Pan command requires argument: point");
						}
					}
					else
					{
						Debug.Log ("Actor not found for panning: " + actor_key);
					}
				}
				else if (this_direction.command.StartsWith ("rotate "))
				{
					string actor_key = this_direction.command.Substring (7);
					if (CutsceneManager.actor_dict.ContainsKey(actor_key))
					{
						if (this_direction.arg_dict.ContainsKey ("amount"))
						{
							GameObject this_actor = CutsceneManager.actor_dict[actor_key];
							float effect_time = 1f;
							if (this_direction.arg_dict.ContainsKey("duration"))
							{
								effect_time = float.Parse (this_direction.arg_dict["duration"]);
							}
							this_actor.GetComponent<CursorBehavior>().set_rotation_target(float.Parse (this_direction.arg_dict["amount"]), 0f, effect_time);
						}
						else
						{
							Debug.Log ("Rotate command needs argument: amount");
						}
					}
					else
					{
						Debug.Log ("Actor not found for rotation: " + actor_key);
					}
				}
				else if (this_direction.command.StartsWith ("zoom "))
				{
					string actor_key = this_direction.command.Substring (5);

					if (CutsceneManager.actor_dict.ContainsKey(actor_key))
					{
						if (this_direction.arg_dict.ContainsKey ("amount"))
						{
							GameObject this_actor = CutsceneManager.actor_dict[actor_key];
							float effect_time = 1f;
							if (this_direction.arg_dict.ContainsKey("duration"))
							{
								effect_time = float.Parse (this_direction.arg_dict["duration"]);
							}
							if (this_direction.arg_dict.ContainsKey ("point"))
							{
								float pan_x = float.Parse(this_direction.arg_dict["point"].Split(';')[0].Substring (1).Trim());
								string y_string = this_direction.arg_dict["point"].Split(';')[1].Trim();
								float pan_y = float.Parse(y_string.Substring (0, y_string.Length - 1));
								this_actor.GetComponent<CursorBehavior>().set_translation_target(new Vector2(pan_x * screen_width, pan_y * screen_height), new_timer: effect_time);
							}
							float zoom_amount = float.Parse (this_direction.arg_dict["amount"]);
							this_actor.GetComponent<CursorBehavior>().set_scale_target(new Vector2(zoom_amount, zoom_amount), new_timer: effect_time);
						}
						else
						{
							Debug.Log ("Zoom command needs argument: amount");
						}
					}
					else
					{
						Debug.Log ("Actor not found for zooming: " + actor_key);
					}
				}

			}
		}
	}

	[System.Serializable]

	public class CutScene
	{
		public bool running = false;
		public float timer = 0f;
		int next_i = 0;
		public List<TimePoint> timepoints = new List<TimePoint>();
		public Dictionary<string, string> image_dict = new Dictionary<string, string>();
		public List<string> no_stretch_list = new List<string>();
		public bool skippable = true;

		public CutScene ()
		{

		}

		public CutScene (List<TimePoint> my_timepoints, Dictionary<string, string> my_image_dict)
		{
			timepoints = my_timepoints;
			image_dict = my_image_dict;
		}

		public void update (GameObject my_canvas, float screen_width, float screen_height, AudioSource SFX_emitter)
		{
			if (running && timepoints.Count > 0)
			{
				timer += Time.deltaTime;
				if (timer >= timepoints[next_i].time)
				{
					timepoints[next_i].execute(image_dict, my_canvas, screen_width, screen_height, SFX_emitter, no_stretch_list);
					next_i ++;
					if (next_i >= timepoints.Count)
					{
						running = false;
					}
				}
			}
		}

		public void play ()
		{
			running = true;
			timer = 0f;
		}

		public float get_duration()
		{
			if (timepoints.Count > 0)
			{
				float output = timepoints[timepoints.Count - 1].time;
				float max_additional_time = 0f;
				foreach (Direction direction in timepoints[timepoints.Count - 1].directions)
				{
					if (direction.arg_dict.ContainsKey ("duration"))
					{
						float duration =  float.Parse(direction.arg_dict["duration"]);
						if (duration > max_additional_time)
						{
							max_additional_time = duration;
						}
					}
				}
				if (max_additional_time == 0f)
				{
					max_additional_time = 1f;
				}
				return output + max_additional_time;
			}
			else
			{
				return 0f;
			}
		}

		public void add_cleanup ()
		{
			TimePoint cleanup_time = new TimePoint(get_duration());
			foreach (string img_key in image_dict.Keys)
			{
				cleanup_time.directions.Add (new Direction("fade out " + img_key));
			}
			timepoints.Add(cleanup_time);
		}
	}

	public void add_new(string cs_name)
	{
		if (ScriptManager.Game.current.cutscene_dict.ContainsKey(cs_name))
		{
			ScriptManager.Game.current.cutscene_dict[cs_name] = new CutScene();
		}
		else
		{
			ScriptManager.Game.current.cutscene_dict.Add (cs_name, new CutScene());
		}
	}

	public void clear_images()
	{
		playing_scenes.Clear();
		foreach(Transform image_layer in my_canvas.transform)
		{
			image_layer.GetComponent<CursorBehavior>().set_image_alpha_target(0f, new_timer: 1f);
			StartCoroutine(wait_to_destroy(image_layer.gameObject, 1f));
		}
	}

	// Use this for initialization
	void Start () 
	{
		my_canvas = GameObject.Find ("CutsceneImages");
		screen_width = my_canvas.GetComponent<RectTransform>().rect.width;
		screen_height = my_canvas.GetComponent<RectTransform>().rect.height;
		SFX_emitter = GetComponent<AudioSource>();
	}

	IEnumerator wait_to_destroy (GameObject target, float delay_time)
	{
		float timer = 0f;
		while (timer < delay_time)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;

			}
			yield return null;
		}
		Destroy (target);
	}

	public void play_cutscene (string scene_name)
	{
		playing_scenes.Add (ScriptManager.Game.current.cutscene_dict[scene_name]);
		ScriptManager.Game.current.cutscene_dict[scene_name].play();
	}

	public bool cutscene_playing ()
	{
		return playing_scenes.Count > 0;
	}

	public bool can_skip ()
	{
		return playing_scenes.FindAll (item => item.timer < 1f || item.skippable == false).Count == 0;
	}

	// Update is called once per frame
	void Update () {
		if (ScriptManager.Game.current.running)
		{
			List<CutScene> kill_list = new List<CutScene>();
			foreach (CutScene scene in playing_scenes)
			{
				if (scene.running)
				{
					scene.update(my_canvas, screen_width, screen_height, SFX_emitter);
					
				}
				else
				{
					kill_list.Add (scene);
				}
			}
			foreach (CutScene finished_scene in kill_list)
			{
				playing_scenes.Remove (finished_scene);
				
			}
			if (kill_list.Count > 0 && playing_scenes.Count == 0)
			{
				onCutscenesDone();
			}
		}

	}
}
