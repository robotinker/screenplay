using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

public class DialogueManager : MonoBehaviour {
	Text blabber_name;
	Text blabber_text;
	Image blabber_name_plate;
	
	Image bg_image;
	public Image fg_image;
	Image portrait_L_image;
	Image portrait_C_image;
	Image portrait_R_image;

	Sprite choice_option_sprite;
	
	string player_speaker_name = "";
	public string player_speaker_pos = "";
	string player_speaker_portrait = "";

	GameObject speech_box;
	GameObject approval_box;
	GameObject interrupt_box;
	GameObject auto_progress_slider;
	GameObject password_box;
	GameObject password_label;

	GameObject choice_box;
	float choice_box_height;
	GameObject secret_choice_box;
	GameObject player_portrait_L;
	GameObject player_portrait_R;
	GameObject bg_left_portraits;
	GameObject bg_right_portraits;
	GameObject tutorial_box;

	GameObject player_ui_block;
	float player_ui_height;
	Vector3 player_ui_start_pos;

	List<GameObject> pause_portraits = new List<GameObject>();

	Vector2 PL_hide;
	Vector2 PL_show;
	Vector2 PR_hide;
	Vector2 PR_show;

	float bg_character_left_max_x;
	float bg_character_right_min_x;

	List<Vector2> CLR_pos_list = new List<Vector2>();

	string choice_mode;
	float fail_timer = 0f;
	float auto_progress_duration = 0f;
	bool timer_running = false;
	bool timing_dialogue = false;
	string tutorial_message = "";

	List<GameObject> player_cursors = new List<GameObject>();
	int player_count; // how many controllers are supported
	bool show_cursor_flag = false;
	
	public List<GameObject> player_guis = new List<GameObject>();
	List<int> player_gui_item_count = new List<int>{0,0,0,0};
	public GameObject game_stat_gui;
	
	public float message_transition_time = 0.5f;

	public FontStyle narration_style;

	public Color character_speech_box_color;
	public Color character_text_color;
	public Color narrator_speech_box_color;
	public Color narrator_text_color;

	public Sprite character_speech_box_image;
	public Sprite narrator_speech_box_image;

	public bool typing_in = false;
	public bool typein_signal = false;
	public float characters_typed = 0f;
	public bool use_type_in = false;
	public float characters_per_second = 30f;
	public string my_line_text = "";
	public float typein_pause = 0.1f;
	public int char_per_typein_noise = 3;

	public List<string> sentence_endings = new List<string>{".","?","!"};
	public List<string> sentence_pauses = new List<string>{",",":","-",";"};
	public float typein_pause_timer = 0f;
	public List<string> open_tags = new List<string>();
	public int latest_sound_index = 0;
	AudioSource typein_emitter;
	public bool play_typein_noise = true;

	public string color_mode = "nar";
	public float white_text_color_threshold = 1.5f;

	public float base_gui_alpha = 0.8f;
	public bool speaker_transition_slide = true;
	public bool new_speaker_slide = false;
	public bool speaker_transition_pulse = true;

	public string font_nar = "";
	public string font_char = "";
	public string font_ui = "";
	public string font_tutorial = "";
	public string font_choice = "";
	public string font_fail = "";

	ScriptManager script_manager;

	List<int> player_choices = new List<int>();
	List<List<int>> player_allocations = new List<List<int>>();
	string current_resource_name = "";
	List<string> my_choices = new List<string>();
	public int column_count = 1;

	List<int> my_participants = new List<int>();
	bool populate_dialogue_flag = false;
	List<string> my_stats_to_show = new List<string>();

	List<int> player_positions = new List<int>();

	public bool showing_recap = false;
	public bool showing_pause_stats = false;

	GameObject my_canvas;

	GameObject spinner_hand;
	GameObject spinner_wedges;
	List<GameObject> wedge_list = new List<GameObject>();
	List<int> wedge_counts = new List<int>();
	string wedge_graphic_name = "wedge20th";

	string badbid_graphic_name = "bad_bid";
	string pip_graphic_name = "bad_bid";
	
	public Dictionary<string, AudioClip> sound_dict = new Dictionary<string, AudioClip>();
	AudioSource SFX_emitter;

	/*public Dictionary<string, List<Sprite>> anim_sprites = new Dictionary<string, List<Sprite>>();
	public Dictionary<string, List<float>> anim_times = new Dictionary<string, List<float>>();*/
	public Dictionary<string, SpriteAnimation> anim_dict = new Dictionary<string, SpriteAnimation>();

	public Dictionary<string, Color> valid_colors = new Dictionary<string, Color>{
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

	public bool bubble_tails = false;
	public bool hide_choices = false;
	public bool randomize_choices = false;
	bool players_are_choosing_characters = false;
	bool line_up_bg_char_flag = false;
	public bool bg_portrait_setup_flag = false;

	public class SpriteAnimation
	{
		List<Sprite> sprites = new List<Sprite>();
		public List<float> times = new List<float>();
		public bool looping = false;
		public int loop_index = 0;

		public SpriteAnimation ()
		{

		}

		public SpriteAnimation (List<Sprite> my_sprites, List<float> my_times, int my_loop_index = -1)
		{
			if (my_loop_index >= 0)
			{
				looping = true;
				loop_index = my_loop_index;
			}
			sprites = my_sprites;
			times = my_times;
		}

		public void apply_to (GameObject target)
		{
			SpriteAnimator my_animator = target.GetComponent<SpriteAnimator>();
			my_animator.set_anim(sprites, times);
			my_animator.set_loop_index(loop_index);
			my_animator.set_looping(looping);
		}
	}

	public class Settings
	{
		public Dictionary<string, string> graphic_names = new Dictionary<string, string>{
			{"windrose", "windrose"},
			{"cursor", "cursor"},
			{"stat box", "default_box"},
			{"tutorial box", "default_box"},
			{"name plate", "default_box"},
			{"character dialogue box", "default_box"},
			{"narrator dialogue box", "default_box"},
			{"interrupt dialogue box", "interrupt_box"},
			{"bad bid token", "bad_bid"},
			{"dialogue progression arrow", "progression arrow 1"},
			{"spinner wedge", "wedge20th"},
			{"spinner hand", "spinner hand"},
			{"choice option backdrop", "default_box"},
			{"choice option box", "default_box"},
			{"password pip", "bad_bid"},
			{"auto progression wheel", "windrose"},
			{"title screen", ""},
		};

		public Dictionary<string, string> sound_names = new Dictionary<string, string>{
			{"allocate", "allocate"},
			{"unallocate", "unallocate"},
			{"move cursor", "move_cursor"},
			{"confirm", "confirm"},
			{"cancel", "cancel2"},
			{"player joining", "player_joining"},
			{"player drop in", "player_joined2"},
			{"player drop out", "player_leaving"},
			{"switch character", "swap_char"},
			{"approve dialogue progression", "bubble_magic"},
			{"progress dialogue", "plink_approve"},
			{"spinner tie break", "spinner"},
			{"save", "bubble_magic"},
			{"type in noise", "boop2"},
			{"title music", ""},
		};

		public float base_alpha = 0.8f;
		public float white_text_threshold = 1.5f;
		public bool nameplate_slide = false;
		public bool new_speaker_slide = false;
		public bool speaker_pulse = true;
		public Color nar_bg_color = Color.white;
		public Color char_bg_color = new Color(0.45f, 0.5f, 1f);
		public FontStyle nar_font_style = FontStyle.Italic;
		public Color tutorial_box_color = Color.white;
		public Color interrupt_font_color = Color.white;

		public bool type_in = false;
		public float characters_per_second = 30f;
		public float typein_pause = 0.1f;
		public int char_per_typein_noise = 3;


		public Settings ()
		{

		}

		public void set_graphic (string new_key, string new_val)
		{
			if (graphic_names.ContainsKey(new_key))
			{
				graphic_names[new_key] = new_val;
			}
			else
			{
				graphic_names.Add (new_key, new_val);
			}
		}

		public void set_sound (string new_key, string new_val)
		{
			if (sound_names.ContainsKey(new_key))
			{
				sound_names[new_key] = new_val;
			}
			else
			{
				sound_names.Add (new_key, new_val);
			}
		}
	}

	public bool game_stat_gui_showing ()
	{
		return game_stat_gui.GetComponent<Image>().color.a > 0f;
	}

	public void apply_settings (Settings new_settings)
	{
		base_gui_alpha = new_settings.base_alpha;
		white_text_color_threshold = new_settings.white_text_threshold;
		speaker_transition_slide = new_settings.nameplate_slide;
		speaker_transition_pulse = new_settings.speaker_pulse;
		new_speaker_slide = new_settings.new_speaker_slide;
		narrator_speech_box_color = new_settings.nar_bg_color;
		if (new_settings.nar_bg_color.r + new_settings.nar_bg_color.g + new_settings.nar_bg_color.b > white_text_color_threshold)
		{
			narrator_text_color = Color.black;
		}
		else
		{
			narrator_text_color = Color.white;
		}
		interrupt_box.GetComponentInChildren<Text>().color = new_settings.interrupt_font_color;
		character_speech_box_color = new_settings.char_bg_color;
		if (new_settings.char_bg_color.r + new_settings.char_bg_color.g + new_settings.char_bg_color.b > white_text_color_threshold)
		{
			character_text_color = Color.black;
		}
		else
		{
			character_text_color = Color.white;
		}
		narration_style = new_settings.nar_font_style;

		tutorial_box.GetComponent<Image>().color = new Color(new_settings.tutorial_box_color.r, new_settings.tutorial_box_color.g, new_settings.tutorial_box_color.b, 0f);

		wedge_graphic_name = new_settings.graphic_names["spinner wedge"];
		badbid_graphic_name = new_settings.graphic_names["bad bid token"];
		pip_graphic_name = new_settings.graphic_names["password pip"];

		List<string> blank_values = new List<string>{"none", "off", ""};
		if (blank_values.Contains(new_settings.graphic_names["title screen"].ToLower()))
		{
			bg_image.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		}
		else
		{
			bg_image.GetComponent<CursorBehavior>().set_image_alpha_target(1f);

			bg_image.sprite = Resources.Load (ScriptManager.Game.current.get_file_path("backgrounds/" + new_settings.graphic_names["title screen"]), typeof(Sprite)) as Sprite;
		}

		if (blank_values.Contains(new_settings.sound_names["title music"].ToLower()))
		{
			script_manager.set_music("off");
		}
		else
		{
			script_manager.set_music(new_settings.sound_names["title music"], true);
		}

		character_speech_box_image = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["character dialogue box"]), typeof(Sprite)) as Sprite;
		narrator_speech_box_image = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["narrator dialogue box"]), typeof(Sprite)) as Sprite;
		//Debug.Log ("Switching narrator speech box to: " + new_settings.graphic_names["narrator dialogue box"]);
		interrupt_box.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["interrupt dialogue box"]), typeof(Sprite)) as Sprite;
		choice_box.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["choice option backdrop"]), typeof(Sprite)) as Sprite;
		secret_choice_box.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["windrose"]), typeof(Sprite)) as Sprite;
		tutorial_box.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["tutorial box"]), typeof(Sprite)) as Sprite;
		blabber_name_plate.sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["name plate"]), typeof(Sprite)) as Sprite;
		spinner_hand.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["spinner hand"]), typeof(Sprite)) as Sprite;
		auto_progress_slider.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["auto progression wheel"]), typeof(Sprite)) as Sprite;

		choice_option_sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["choice option box"]), typeof(Sprite)) as Sprite;

		for (int i = 0; i < player_cursors.Count; i++)
		{
			string side;
			if (i % 2 == 0)
			{
				side = "L";
			}
			else
			{
				side = "R";
			}
			player_cursors[i].GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["cursor"] + side), typeof(Sprite)) as Sprite;
		}

		for (int i= 0; i< player_guis.Count; i++)
		{
			player_guis[i].GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["stat box"]), typeof(Sprite)) as Sprite;
		}

		for (int i= 0; i< approval_box.transform.childCount; i++)
		{
			approval_box.transform.GetChild(i).GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + new_settings.graphic_names["dialogue progression arrow"]), typeof(Sprite)) as Sprite;
		}

		foreach (string key_name in new_settings.sound_names.Keys)
		{
			AudioClip new_sound = Resources.Load (ScriptManager.Game.current.get_file_path("sounds/" + new_settings.sound_names[key_name]), typeof(AudioClip)) as AudioClip;
			if (sound_dict.ContainsKey (key_name))
			{
				sound_dict[key_name] = new_sound;
			}
			else
			{
				sound_dict.Add (key_name, new_sound);
			}
		}

		use_type_in = new_settings.type_in;
		characters_per_second = new_settings.characters_per_second;
		typein_pause = new_settings.typein_pause;
		char_per_typein_noise = new_settings.char_per_typein_noise;
		update_fonts();
		set_color_mode("nar", force_update: true);
	}

	public void load_settings(string game_name)
	{
		script_manager.time_everything = false;
		Settings new_settings = new Settings();

		TextAsset settings_file = Resources.Load (game_name + "/settings") as TextAsset;
		string[] lines = settings_file.text.Split ('\n');

/*		if (!File.Exists(Application.dataPath + "/Resources/" + game_name + "/settings.txt"))
		{
			Debug.Log ("Couldn't find file named Resources/" + game_name + "/settings.txt");
			return;
		}

		string[] lines = File.ReadAllLines (Application.dataPath + "/Resources/" + game_name + "/settings.txt");*/

		List<string> info = new List<string>();
		foreach (string line in lines)
		{
			info.Add (line);
		}
		int story_settings_start = info.IndexOf ("STORY SETTINGS");
		int gui_settings_start = info.IndexOf ("GUI SETTINGS");
		int graphics_start = info.IndexOf ("GRAPHICS");
		int sounds_start = info.IndexOf ("SOUNDS");

		List<string> story_settings = new List<string>{"player count", "save mode", "pause every line", "show story recap", "use speech bubbles", "hide barred choices", "randomize choices"};
		List<string> color_settings = new List<string>{"narrator bg color", "character bg color", "tutorial box color", "interrupt font color"};
		List<string> bool_settings = new List<string>{"speaker pulse", "nameplate slide", "type in", "new speaker slide"};
		List<string> float_settings = new List<string>{"GUI base alpha", "white text threshold", "characters per second", "type in sentence pause"};

		script_manager.pause_all_lines = false;
		script_manager.show_plot_summaries = true;
		script_manager.set_player_limits_from_string("1-4");
		script_manager.save_mode = "overwrite";

		for (int i = story_settings_start + 1; i < gui_settings_start; i++)
		{
			if (info[i] != "")
			{
				string my_name = info[i].Split ('=')[0].Trim ();
				string my_val = info[i].Split ('=')[1].Trim ();
				
				if (story_settings.Contains (my_name))
				{
					switch (my_name)
					{
					case "player count":
						script_manager.set_player_limits_from_string(my_val);
						break;
					case "save mode":
						script_manager.save_mode = my_val;
						break;
					case "pause every line":
						if (my_val == "true")
						{
							script_manager.pause_all_lines = true;
						}
						break;
					case "show story recap":
						if (my_val == "false")
						{
							script_manager.show_plot_summaries = false;
						}
						break;
					case "use speech bubbles":
						if (my_val == "true")
						{
							bubble_tails = true;
							blabber_name_plate.type = Image.Type.Simple;
							blabber_name_plate.preserveAspect = true;
						}
						else
						{
							bubble_tails = false;
							blabber_name_plate.type = Image.Type.Sliced;
							blabber_name_plate.preserveAspect = false;
						}
						break;
					case "randomize choices":
						if (my_val == "true")
						{
							randomize_choices = true;
						}
						break;
					case "hide barred choices":
						if (my_val == "true")
						{
							hide_choices = true;
						}
						else if (my_val == "false")
						{
							hide_choices = false;
						}
						break;
					}
				}
			}
		}

		for (int i = gui_settings_start + 1; i < graphics_start; i++)
		{
			if (info[i] != "")
			{
				string my_name = info[i].Split ('=')[0].Trim ();
				string my_val = info[i].Split ('=')[1].Trim ();

				if (color_settings.Contains (my_name))
				{
					Color new_color = new Color();

					if (valid_colors.ContainsKey (my_val))
					{
						new_color = valid_colors[my_val];
					}
					else if (my_val.Split (',').Length == 3)
					{
						List<float> color_rgb = new List<float>();
						foreach (string color_string in my_val.Split (','))
						{
							float new_val;
							bool is_float = float.TryParse (color_string, out new_val);
							if (is_float)
							{
								if (new_val <= 255f)
								{
									if (new_val <= 1f)
									{
										color_rgb.Add (new_val);
									}
									else
									{
										color_rgb.Add (new_val / 255f);
									}
								}
								else
								{
									new_color = Color.white;
									Debug.Log ("Couldn't parse " + my_name + ": " + my_val);
									break;
								}
							}
							else
							{
								new_color = Color.white;
								Debug.Log ("Couldn't parse " + my_name + ": " + my_val);
								break;
							}
						}
						if (new_color != Color.white)
						{
							new_color = new Color(color_rgb[0], color_rgb[1], color_rgb[2]);
						}
					}
					else
					{
						new_color = Color.white;
						Debug.Log ("Couldn't parse " + my_name + ": " + my_val);
					}

					switch (my_name)
					{
					case "narrator bg color":
						new_settings.nar_bg_color = new_color;
						break;
					case "character bg color":
						new_settings.char_bg_color = new_color;
						break;
					case "tutorial box color":
						new_settings.tutorial_box_color = new_color;
						break;
					case "interrupt font color":
						new_settings.interrupt_font_color = new_color;
						break;
					}
				}
				else if (bool_settings.Contains (my_name))
				{
					bool is_bool = false;
					switch (my_name)
					{
					case "nameplate slide":
						is_bool = bool.TryParse (my_val, out new_settings.nameplate_slide);
						break;
					case "new speaker slide":
						is_bool = bool.TryParse (my_val, out new_settings.new_speaker_slide);
						break;
					case "speaker pulse":
						is_bool = bool.TryParse (my_val, out new_settings.speaker_pulse);
						break;
					case "type in":
						is_bool = bool.TryParse (my_val, out new_settings.type_in);
						break;
					default:
						break;
					}
					if (!is_bool)
					{
						Debug.Log ("Couldn't parse bool for " + my_name + ": " + my_val);
					}

				}
				else if (float_settings.Contains (my_name))
				{
					bool is_float = false;
					switch (my_name)
					{
					case "GUI base alpha":
						is_float = float.TryParse (my_val, out new_settings.base_alpha);
						break;
					case "white text threshold":
						is_float = float.TryParse (my_val, out new_settings.white_text_threshold);
						break;
					case "characters per second":
						is_float = float.TryParse (my_val, out new_settings.characters_per_second);
						break;
					case "type in sentence pause":
						is_float = float.TryParse (my_val, out new_settings.typein_pause);
						break;
					default:
						break;
					}
					if (!is_float)
					{
						Debug.Log ("Couldn't parse float for " + my_name + ": " + my_val);
					}
				}
				else if (my_name == "narrator font style")
				{
					switch (my_val)
					{
					case "italic":
						new_settings.nar_font_style = FontStyle.Italic;
						break;
					case "bold":
						new_settings.nar_font_style = FontStyle.Bold;
						break;
					case "normal":
						new_settings.nar_font_style = FontStyle.Normal;
						break;
					case "bold and italic":
						new_settings.nar_font_style = FontStyle.BoldAndItalic;
						break;
					}
				}
				else if (my_name == "font")
				{

					set_font (my_val);
				}
				else if (my_name.Contains("font") && my_name.Split ()[1] == "font")
				{
					parse_font(my_name.Split ()[0], my_val);
				}

				else if (my_name == "characters per type in noise")
				{
					new_settings.char_per_typein_noise = int.Parse (my_val);
				}
			}
		}
		
		for (int i = graphics_start + 1; i < sounds_start; i++)
		{
			if (info[i] != "")
			{
				string my_name = info[i].Split ('=')[0].Trim ();
				string my_file = info[i].Split ('=')[1].Trim ();
				new_settings.set_graphic(my_name, my_file);
			}
		}
		
		for (int i = sounds_start + 1; i < info.Count; i++)
		{
			if (info[i] != "")
			{
				string my_name = info[i].Split ('=')[0].Trim ();
				string my_file = info[i].Split ('=')[1].Trim ();
				new_settings.set_sound(my_name, my_file);
			}
		}

		apply_settings (new_settings);
	}

	public void parse_font (string kind, string my_val)
	{
		switch (kind)
		{
		case "":
			set_font (my_val);
			break;
		case "UI":
			set_ui_font(my_val);
			break;
		case "character":
			set_char_font(my_val);
			break;
		case "tutorial":
			set_tutorial_font(my_val);
			break;
		case "narrator":
			set_nar_font(my_val);
			break;
		case "failure":
			set_fail_font(my_val);
			break;
		case "choice":
			set_choice_font(my_val);
			break;
		default:
			break;
		}
	}

	public void check_for_anim(string anim_name)
	{
		Sprite first_sprite = Resources.Load (ScriptManager.Game.current.story_name + "/animations/" + anim_name + "/" + anim_name + "0001", typeof(Sprite)) as Sprite;
		if (first_sprite != null && !anim_dict.ContainsKey (anim_name))
		{
			List<Sprite> new_sprites = new List<Sprite>();
			new_sprites.Add (first_sprite);

			bool loading_sprites = true;
			string new_sprite_name = "";
			int next_sprite_i = 0;
			while (loading_sprites)
			{
				next_sprite_i += 1;
				new_sprite_name = anim_name;
				if (next_sprite_i < 10)
				{
					new_sprite_name += "000" + next_sprite_i.ToString ();
				}
				else if (next_sprite_i < 100)
				{
					new_sprite_name += "00" + next_sprite_i.ToString ();
				}
				else if (next_sprite_i < 1000)
				{
					new_sprite_name += "0" + next_sprite_i.ToString ();
				}
				else
				{
					new_sprite_name += next_sprite_i.ToString ();
				}

				Sprite new_sprite = Resources.Load (ScriptManager.Game.current.story_name + "/animations/" + anim_name + "/" + new_sprite_name, typeof(Sprite)) as Sprite;

				if (new_sprite == null)
				{
					loading_sprites = false;
				}
				else
				{
					new_sprites.Add (new_sprite);
				}
			}

			anim_dict.Add (anim_name, new SpriteAnimation(new_sprites, new List<float>{0.1f}));
		}
	}

	// Use this for initialization
	void Start () {
		ScriptManager.Game.current = new ScriptManager.Game("default");
		SpinnerBehavior.onSpinnerStop += OnSpinnerStop;
		ScriptManager.onPromptPassword += OnPromptPassword;
		ScriptManager.onGuessPassword += OnPromptPassword;
		InputDialogue.onPlayersDoneSelectingCharacters += onPlayersDoneChoosingCharacters;
		ScriptManager.onChooseCharacter += onNewPlayerChoosingCharacter;

		my_canvas = GameObject.Find ("Canvas");

		speech_box = GameObject.Find ("SpeechBox");
		speech_box.GetComponent<Image>().sprite = narrator_speech_box_image;
		interrupt_box = GameObject.Find ("InterruptBox");
		auto_progress_slider = GameObject.Find ("AutoProgressWheel");
		password_box = GameObject.Find ("PasswordBox");
		password_label = GameObject.Find ("PasswordLabelBox");
		approval_box = GameObject.Find ("Approvals");
		tutorial_box = GameObject.Find ("TutorialBox");

		player_ui_block = GameObject.Find("PlayerUIs");

		spinner_hand = GameObject.Find ("SpinnerHand");
		spinner_wedges = GameObject.Find("SpinnerWedges");

		blabber_name_plate = GameObject.Find ("NamePlateBox").GetComponent<Image>();
		blabber_name = GameObject.Find ("NamePlate").GetComponent<Text>();
		blabber_text = GameObject.Find ("DialogueBox").GetComponent<Text>();

		bg_image = GameObject.Find ("SceneBG").GetComponent<Image>();
		fg_image = GameObject.Find ("SceneFG").GetComponent<Image>();
		portrait_L_image = GameObject.Find ("PortraitL").GetComponent<Image>();
		portrait_C_image = GameObject.Find ("PortraitC").GetComponent<Image>();
		portrait_R_image = GameObject.Find ("PortraitR").GetComponent<Image>();
		CLR_pos_list.Add (new Vector2(portrait_L_image.transform.position.x, portrait_L_image.transform.position.y));
		CLR_pos_list.Add (new Vector2(portrait_C_image.transform.position.x, portrait_C_image.transform.position.y));
		CLR_pos_list.Add (new Vector2(portrait_R_image.transform.position.x, portrait_R_image.transform.position.y));
		bg_character_left_max_x = CLR_pos_list[0].x + (CLR_pos_list[1].x - CLR_pos_list[0].x) * 2f / 3f;
		bg_character_right_min_x = CLR_pos_list[2].x - (CLR_pos_list[2].x - CLR_pos_list[1].x) * 2f / 3f;
			
		player_portrait_L = GameObject.Find ("PLeftPortrait");
		player_portrait_R = GameObject.Find ("PRightPortrait");
		bg_left_portraits = GameObject.Find("LeftBGPortraits");
		bg_right_portraits = GameObject.Find("RightBGPortraits");

		for (int i = 1; i <= 4; i++)
		{
			pause_portraits.Add(GameObject.Find ("P" + i.ToString () + "PausePortrait"));
		}

		PL_hide = new Vector2(player_portrait_L.transform.position.x, player_portrait_L.transform.position.y);
		PL_show = new Vector2((CLR_pos_list[0].x + CLR_pos_list[1].x) / 2, player_portrait_L.transform.position.y);
		PR_hide = new Vector2(player_portrait_R.transform.position.x, player_portrait_R.transform.position.y);
		PR_show = new Vector2((CLR_pos_list[1].x + CLR_pos_list[2].x) / 2, player_portrait_R.transform.position.y);

		game_stat_gui = GameObject.Find("PauseGameStats");

		choice_box = GameObject.Find ("ChoiceBox");
		secret_choice_box = GameObject.Find ("SecretChoiceBox");

		choice_option_sprite = Resources.Load("default/icons/default_box", typeof(Sprite)) as Sprite;

		script_manager = GameObject.Find ("ScriptHolder").GetComponent<ScriptManager>();
		SFX_emitter = GetComponent<AudioSource>();
		typein_emitter = GameObject.Find ("Typein_SFX_emitter").GetComponent<AudioSource>();

		player_count = script_manager.player_colors.Count;
		script_manager.player_count_max = player_count;

/*		Animator AnimTester = GameObject.Find ("AnimTest").GetComponent<Animator>();
		AnimTester.SetBool ("isActive", true);
		AnimationClip AnimClip = new AnimationClip();
		AnimationClip shadowClip = Resources.Load ("Quick RPG/portraits/shadow_smooth") as AnimationClip;
		//GameObject.Find ("AnimTest").GetComponent<Animator>().Play (int );
		EditorCurveBinding binding = (EditorCurveBinding)curves[i];
		AnimationCurve curve = AnimationUtility.GetEditorCurve(shadowClip, binding);
		AnimationUtility.SetEditorCurve();*/

		for (int i=0; i < player_count; i++)
		{
			player_allocations.Add (new List<int>());
			player_positions.Add (0);

			// GUIPANEL
			GameObject new_ui = Instantiate(Resources.Load ("prefabs/PlayerUI") as GameObject) as GameObject;

			new_ui.transform.SetParent (player_ui_block.transform);
			new_ui.GetComponent<CursorBehavior>().set_image_color_target(script_manager.player_colors[i]);
			player_guis.Add (new_ui);
			hide_ui(i+1);
			image_instant_alpha(0f, new_ui.GetComponent<Image>());

			// CURSOR
			GameObject new_cursor = Instantiate(Resources.Load ("prefabs/PlayerCursor") as GameObject) as GameObject;
			new_cursor.transform.SetParent(GameObject.Find ("Cursors").transform);

			new_cursor.GetComponent<CursorBehavior>().set_image_color_target(script_manager.player_colors[i]);
			if (i % 2 == 1)
			{
				new_cursor.GetComponent<Image>().sprite = Resources.Load ("default/icons/cursorR", typeof(Sprite)) as Sprite;
			}

			player_cursors.Add (new_cursor);

			// APPROVAL ARROW
			GameObject new_approval = Instantiate (Resources.Load ("prefabs/ApprovalArrow") as GameObject) as GameObject;
			new_approval.transform.SetParent (approval_box.transform);
			new_approval.GetComponent<Image>().color = script_manager.player_colors[i];
			image_instant_alpha(0f, new_approval.GetComponent<Image>());
		}

		// GENERAL PURPOSE APPROVAL IN CENTER
		GameObject gen_approval = Instantiate (Resources.Load ("prefabs/ApprovalArrow") as GameObject) as GameObject;
		gen_approval.transform.SetParent (approval_box.transform);
		gen_approval.transform.SetSiblingIndex(player_count / 2);
		gen_approval.GetComponent<Image>().color = Color.black;
		image_instant_alpha(0f, gen_approval.GetComponent<Image>());

		// SET NATIVE SIZES SO APPROVAL ARROWS DON'T MUTATE OVER TIME
		for (int i=0; i < approval_box.transform.childCount; i++)
		{
			approval_box.transform.GetChild (i).GetComponent<CursorBehavior>().wait_to_set_native_size(0.1f);
		}

		script_manager.save_mode = "overwrite";
		load_settings("default");
/*		if (File.Exists(Application.dataPath + "/Resources/default/settings.txt"))
		{
			load_settings ("default");
		}
		else
		{
			Debug.Log ("<color=red>Resources folder must contain a 'default' folder with a 'settings.txt' file!</color>");
			Application.Quit();
		}*/
	}

	public void update_game_stat_ui ()
	{
		if (ScriptManager.Game.current.pause_stats.Count > 0)
		{
			game_stat_gui.GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);
		}

		for (int i=0; i<game_stat_gui.transform.childCount; i++)
		{
			Destroy(game_stat_gui.transform.GetChild (i).gameObject);
		}

		for (int i=0; i < ScriptManager.Game.current.pause_stats.Count; i++)
		{
			//Debug.Log("Looking for stat: " + ScriptManager.Game.current.pause_stats[i]);
			ScriptManager.Stat this_stat = ScriptManager.Game.current.stat_dict[ScriptManager.Game.current.pause_stats[i]];
			GameObject new_stat_banner = Instantiate(Resources.Load ("prefabs/PlayerUIResource" + this_stat.UI_type()) as GameObject) as GameObject;
			new_stat_banner.transform.SetParent (game_stat_gui.transform);

			if (this_stat.UI_type().Substring (0,4) == "Text")
			{
				if (ScriptManager.Game.current.pause_stat_aliases.ContainsKey(this_stat.text))
				{
					new_stat_banner.transform.GetChild(0).GetComponent<Text>().text = ScriptManager.Game.current.pause_stat_aliases[this_stat.text] + ": ";
				}
				else
				{
					new_stat_banner.transform.GetChild(0).GetComponent<Text>().text = this_stat.text + ": ";
				}
			}
			else
			{
				new_stat_banner.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load (this_stat.icon_path, typeof(Sprite)) as Sprite;

			}
			if (this_stat.UI_type ().Substring(4) == "Meter")
			{
				new_stat_banner.transform.GetChild(1).GetComponent<DynamicMeter>().set_val(this_stat.float_val);
			}
			else
			{
				new_stat_banner.transform.GetChild(1).GetComponent<Text>().text = this_stat.get_string();
			}
		}
			
		game_stat_gui.GetComponent<CursorBehavior>().set_image_color_target(narrator_speech_box_color);

		set_ui_font();
	}

	public void hide_game_stats ()
	{
		game_stat_gui.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		hide_all_children(game_stat_gui);

	}

	public void update_player_ui (int player_i, string name, List<string> resources)
	{
		Debug.Log("Updating player UI: " + player_i);
		// Set default UI height if not given
		if (player_ui_height == 0f)
		{
			player_ui_height = player_ui_block.GetComponent<RectTransform>().rect.height;
			player_ui_start_pos = player_ui_block.transform.position;
		}
		
		GameObject this_ui = player_guis[player_i -1];
		player_gui_item_count[player_i - 1] = resources.Count;
		Transform this_stat_panel_transform = this_ui.transform.Find("StatPanel");

		foreach (Transform resource_ribbon in this_stat_panel_transform)
		{
			Destroy(resource_ribbon.gameObject);
		}

		/*for (int i=0; i<this_stat_panel_transform.childCount; i++)
		{
			if (i >= 1)
			{
				Destroy(this_stat_panel_transform.GetChild (i).gameObject);
			}
		}*/

		for (int i=0; i < resources.Count; i++)
		{
			Debug.Log ("Looking for player variable: " + resources[i]);
			ScriptManager.Stat this_stat = ScriptManager.Game.current.players[player_i - 1].stat_dict[resources[i]];
			GameObject new_stat_banner = Instantiate(Resources.Load ("prefabs/PlayerUIResource" + this_stat.UI_type()) as GameObject) as GameObject;
			new_stat_banner.transform.SetParent (this_stat_panel_transform);

			if (this_stat.UI_type().Substring (0,4) == "Text")
			{
				if (ScriptManager.Game.current.pause_player_stat_aliases.ContainsKey(this_stat.text))
				{
					Debug.Log("Alias found!");
					new_stat_banner.transform.GetChild(0).GetComponent<Text>().text = ScriptManager.Game.current.pause_player_stat_aliases[this_stat.text] + ": ";

				}
				else
				{
					new_stat_banner.transform.GetChild(0).GetComponent<Text>().text = this_stat.text + ": ";

				}
			}
			else
			{
				new_stat_banner.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load (this_stat.icon_path, typeof(Sprite)) as Sprite;

			}
			if (this_stat.UI_type ().Substring(4) == "Meter")
			{
				new_stat_banner.transform.GetChild(1).GetComponent<DynamicMeter>().set_val(script_manager.get_player_stat_from_string("player" + player_i.ToString() + "." + resources[i]).float_val);
			}
			else
			{
				new_stat_banner.transform.GetChild(1).GetComponent<Text>().text = script_manager.get_player_stat_from_string("player" + player_i.ToString() + "." + resources[i]).get_string();
			}
		}

		// Resize based on number of stats showing
		int max_stats_showing = resources.Count;
		for (int i=0; i < player_gui_item_count.Count; i++)
		{
			if (player_gui_item_count[i] > max_stats_showing)
			{
				max_stats_showing = player_gui_item_count[i];
			}
		}
		float stat_height_factor = Mathf.Min(max_stats_showing / 3f, 1f);
		float new_ui_height = player_ui_height * (0.25f + 0.75f * stat_height_factor);
		player_ui_block.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, new_ui_height);
		player_ui_block.transform.position = player_ui_start_pos + Vector3.down * (new_ui_height - player_ui_height) / 2f;
		this_ui.transform.Find("PlayerName").GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0.25f * player_ui_height);
		this_stat_panel_transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, player_ui_height * 0.75f * stat_height_factor);
		this_stat_panel_transform.localPosition = Vector3.down * new_ui_height / 16f;

		if (name != "")
		{
			this_ui.GetComponentInChildren<Text>().text = name;
		}
			
		Color new_color = ScriptManager.Game.current.players[player_i - 1].get_color();
		this_ui.GetComponent<CursorBehavior>().set_image_color_target(new_color);
		if (new_color.r + new_color.g + new_color.b > white_text_color_threshold)
		{
			this_ui.GetComponent<CursorBehavior>().set_text_color_target(Color.black);
			foreach (Transform child_transform in this_stat_panel_transform)
			{
				if (child_transform.GetComponent<CursorBehavior>() != null)
				{
					child_transform.GetComponent<CursorBehavior>().set_text_color_target(Color.black);
				}
			}
		}
		else
		{
			this_ui.GetComponent<CursorBehavior>().set_text_color_target(Color.white);
			foreach (Transform child_transform in this_stat_panel_transform)
			{
				if (child_transform.GetComponent<CursorBehavior>() != null)
				{
					child_transform.GetComponent<CursorBehavior>().set_text_color_target(Color.white);
				}
			}
		}
		set_ui_font();

	}

	public void refresh_ui (int player_i, string stat_name)
	{
		int text_i = 1;
		if (my_stats_to_show.Contains(stat_name))
		{
			text_i = my_stats_to_show.IndexOf (stat_name);
		}
		else
		{
			return;
		}

		ScriptManager.Stat this_stat = ScriptManager.Game.current.players[player_i - 1].stat_dict[stat_name];

		if (this_stat.UI_type ().Substring(4) == "Meter")
		{
			player_guis[player_i - 1].transform.GetChild(1).GetChild(text_i).GetChild (1).GetComponent<Slider>().value = script_manager.get_player_stat_from_string("player" + player_i.ToString() + "." + stat_name).float_val;
		}
		else
		{
			player_guis[player_i - 1].transform.GetChild(1).GetChild(text_i).GetChild (1).GetComponent<Text>().text = script_manager.get_player_stat_from_string("player" + player_i.ToString() + "." + stat_name).get_string();
		}
	}

	public void collapse_ui (int player_i)
	{
		if (ScriptManager.Game.current.players[player_i - 1].active)
		{
			update_player_ui(player_i, "", ScriptManager.Game.current.default_stats_to_show);

		}
		else
		{
			Transform target = player_guis[player_i-1].transform;

			target.GetComponentInChildren<Text>().text = ScriptManager.Game.current.players[player_i - 1].name;

			Transform target_stat_panel = target.Find("StatPanel");

			for (int i=0; i < target_stat_panel.childCount; i++)
			{
				Destroy (target_stat_panel.GetChild (i).gameObject);
				player_gui_item_count[player_i - 1] = 0;
			}
		}
	}

	public void collapse_guis ()
	{
		for (int i=1;i<=4;i++)
		{
			collapse_ui(i);
		}
	}

	public void set_interrupt_text (string new_interrupt)
	{
		if (new_interrupt == "")
		{
			//StartCoroutine(Fade_to(speech_box.transform.GetChild (1).GetComponent<Image>(), 0f, message_transition_time));
			//StartCoroutine(Fade_text_to(speech_box.transform.GetChild(1).GetComponentInChildren<Text>(), 0f, message_transition_time));
			interrupt_box.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
			interrupt_box.GetComponent<CursorBehavior>().set_text_alpha_target(0f);
		}
		else
		{
			//StartCoroutine(Fade_to(speech_box.transform.GetChild (1).GetComponent<Image>(), 1f, message_transition_time));
			interrupt_box.GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);
			//speech_box.transform.GetChild(1).GetComponentInChildren<Text>().text = new_interrupt;
			interrupt_box.transform.GetChild(0).GetComponentInChildren<Text>().text = new_interrupt;
			//StartCoroutine(Fade_text_to(speech_box.transform.GetChild(1).GetComponentInChildren<Text>(), 1f, message_transition_time));
			interrupt_box.GetComponent<CursorBehavior>().set_text_alpha_target(1f);
		}
	}

	public void present_hidden_choice (List<string> choices, List<int> participants)
	{
		my_choices = choices;
		choice_mode = ScriptManager.Game.current.current_decision.choice_mode;
		fail_timer = ScriptManager.Game.current.current_decision.fail_timer;
		
		for (int i=0; i < 4; i++)
		{
			if (i < choices.Count)
			{
				secret_choice_box.transform.GetChild (i).GetComponent<Image>().sprite = choice_option_sprite;

				secret_choice_box.transform.GetChild (i).gameObject.GetComponentInChildren<Text>().text = choices[i];
				//StartCoroutine(Fade_text_to(secret_choice_box.transform.GetChild (i).GetComponentInChildren<Text>(), 1f, message_transition_time));
				//StartCoroutine(Fade_to(secret_choice_box.transform.GetChild (i).GetComponentInChildren<Image>(), 1f, message_transition_time));
				secret_choice_box.transform.GetChild (i).GetComponent<CursorBehavior>().set_text_alpha_target(1f);
				secret_choice_box.transform.GetChild (i).GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);
			}
		}

		tutorial_message = "Hold a direction and press <color=green>Confirm</color>. Press <color=red>Cancel</color> to undo choice. ";

		switch (choice_mode.Split (' ')[1])
		{
		case "spinner":
			tutorial_message += "One of your responses will be randomly chosen.";
			break;
		case "vote":
			tutorial_message += "Majority vote wins.";
			break;
		case "poll":
			tutorial_message += "Choices are independant.";
			break;
		}
		show_tutorial_box(tutorial_message);
		
		player_choices.Clear ();
		
		for (int i=1; i <= player_count; i++)
		{
			if (participants.Contains (i))
			{
				player_choices.Add (0);
				show_ui(i);

			}
			else
			{
				player_choices.Add (-1);
			}
		}
		
		if (fail_timer > 0f)
		{
			timer_running = true;
		}
		
		show_secret_choice_box();
	}

	public void show_choice_prices ()
	{
		int choice_count = ScriptManager.Game.current.current_choices.Count;
		for (int i=0; i<choice_count; i++)
		{
			int choice_price = 1;
			int choice_i = script_manager.get_choice_i_in_decision_from_visible_i(i);

			if (ScriptManager.Game.current.current_decision.allocation_costs.Count > choice_i && ScriptManager.Game.current.current_decision.allocation_costs[choice_i] > 1)
			{
				choice_price = ScriptManager.Game.current.current_decision.allocation_costs[choice_i];
			}
			else if (ScriptManager.Game.current.current_decision.allocation_stat_costs.Count > choice_i && ScriptManager.Game.current.current_decision.allocation_stat_costs[choice_i] != "")
			{
				choice_price = int.Parse (script_manager.find_variable (ScriptManager.Game.current.current_decision.allocation_stat_costs[choice_i]));
			}

			if (choice_price != 1)
			{
				Transform target_transform = find_option_from_i (i);
				target_transform.GetComponentInChildren<Text>().text += " <color=red>(" + choice_price.ToString () + ")</color>";
			}

			/*GameObject price_message = Instantiate(Resources.Load ("prefabs/TextPanel") as GameObject) as GameObject;

			price_message.GetComponentInChildren<Text>().text = "Price: " + choice_price.ToString ();
			if (ScriptManager.Game.current.font_choice != "")
			{
				Transform price_message_t = price_message.transform.GetChild (0);
				Text price_message_text = price_message_t.GetComponent<Text>();
				Font new_font = find_font (ScriptManager.Game.current.font_choice);
				price_message_text.font = new_font;
				price_message_t.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, price_message_text.mainTexture);

			}
			price_message.GetComponent<Image>().sprite = choice_option_sprite;
			price_message.transform.SetParent (target_transform.GetChild (0));
			price_message.transform.SetAsFirstSibling();
			price_message.GetComponent<Image>().color = Color.white;
			price_message.GetComponentInChildren<Text>().color = Color.black;*/
		}
	}
	
	public void present_choice (List<string> choice_strings, List<int> participants)
	{
		my_choices = choice_strings;
		my_participants = participants;

		choice_mode = ScriptManager.Game.current.current_decision.choice_mode;
		fail_timer = ScriptManager.Game.current.current_decision.fail_timer;
		if (script_manager.time_everything)
		{
			fail_timer = 15f;
		}

		// CLEAN UP
		for (int i = 0; i < choice_box.transform.childCount; i++)
		{
			Destroy (choice_box.transform.GetChild(i).gameObject);
		}

		populate_dialogue_flag = true;
	}

	public void populate_dialogue_options ()
	{
		// Set base height if not defined
		if (choice_box_height == 0f)
		{
			choice_box_height = choice_box.GetComponent<RectTransform>().rect.height;
		}


		// Calculate column count

		column_count = ScriptManager.Game.current.get_column_count();
		int options_per_column = ScriptManager.Game.current.current_decision.options_per_column;
		int fail_choice_count = ScriptManager.Game.current.current_decision.fail_choices.Count;

		// Create columns
		for (int i = 0; i < column_count; i++)
		{
			GameObject new_column = Instantiate(Resources.Load ("prefabs/DialogueColumn") as GameObject) as GameObject;
			new_column.transform.SetParent (choice_box.transform);
		}

		// Create dialogue options
		for (int i=0; i < my_choices.Count; i++)
		{
			int d_i = script_manager.get_choice_i_in_decision_from_visible_i(i);
			string choice_box_text = "";
			if ((choice_mode == "lottery" || choice_mode == "shop") && script_manager.has_limit_for_choice(ScriptManager.Game.current.current_decision, d_i))
			{
				choice_box_text = my_choices[i] + ", Limit: " + script_manager.get_limit_of_choice(ScriptManager.Game.current.current_decision, d_i);
			}
			else
			{
				choice_box_text = my_choices[i];
			}
			
			GameObject new_d_option = Instantiate(Resources.Load ("prefabs/DialogueOption2") as GameObject) as GameObject;
			new_d_option.transform.GetChild (0).GetComponentInChildren<Text>().text = choice_box_text;
			new_d_option.transform.SetParent(choice_box.transform.GetChild (i / options_per_column));
			new_d_option.GetComponent<Image>().sprite = choice_option_sprite;

			// check for icons
			ScriptManager.Choice this_choice = ScriptManager.Game.current.current_decision.choices[d_i];
			if (this_choice.left_icon_name != "")
			{
				new_d_option.transform.Find("Left").GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + this_choice.left_icon_name), typeof(Sprite)) as Sprite;
			}
			if (this_choice.right_icon_name != "")
			{
				new_d_option.transform.Find("Right").GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + this_choice.right_icon_name), typeof(Sprite)) as Sprite;

			}
		}

		int base_index = my_choices.Count;

		foreach(ScriptManager.Choice option in ScriptManager.Game.current.current_decision.choices)
		{
			if (!ScriptManager.Game.current.current_choices.Contains (option) && !option.hide)
			{
				GameObject locked_option = Instantiate(Resources.Load ("prefabs/DialogueOption2") as GameObject) as GameObject;
				locked_option.GetComponentInChildren<Text>().text = script_manager.plug_in_variables(option.text);
				if (option.req_string != "")
				{
					locked_option.GetComponentInChildren<Text>().text += " (" + option.req_string + ")";
				}
				locked_option.GetComponent<Image>().sprite = choice_option_sprite;
				locked_option.transform.SetParent(choice_box.transform.GetChild (base_index / options_per_column));
				locked_option.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
				locked_option.transform.GetChild (0).GetComponentInChildren<Text>().color = new Color(1f,1f,1f);
				base_index += 1;
			}
		}


		// Set choice box height based on number of options in first column
		float choice_box_height_factor = Mathf.Min(choice_box.transform.GetChild(0).childCount / 5f, 1f);
		choice_box.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, choice_box_height * choice_box_height_factor);

		set_choice_font();
	
		tutorial_message = "";
		
		switch (ScriptManager.Game.current.current_decision.choice_mode)
		{
		case "spinner":
			tutorial_message += "One of your responses will be randomly chosen.";
			break;
		case "vote":
			tutorial_message += "Majority vote wins.";
			break;
		case "shop":
			tutorial_message += "Press <color=green>Confirm</color> to invest in an option, or <color=red>Cancel</color> to take back investment. Press <color=blue>Pause</color> to lock allocations.";
			
			show_choice_prices();
			
			break;
		case "agreement":
			tutorial_message += "Consensus is required.";
			break;
		case "poll":
			tutorial_message += "Choices are independant.";
			break;
		case "first":
			tutorial_message += "First selection is used.";
			break;
		case "lottery":
			tutorial_message += "Press <color=green>Confirm</color> to weigh in an option's favor, or <color=red>Cancel</color> to take back investment. Press <color=blue>Pause</color> to lock allocations.";
			
			show_choice_prices();

			for (int i=0; i<fail_choice_count; i++)
			{
				GameObject new_d_option = Instantiate(Resources.Load ("prefabs/DialogueOption2") as GameObject) as GameObject;
				new_d_option.GetComponentInChildren<Text>().text = ScriptManager.Game.current.current_decision.fail_choices[i].text;
				if (ScriptManager.Game.current.font_fail != "")
				{
					Font new_font = find_font(ScriptManager.Game.current.font_fail);
					Text new_d_text = new_d_option.GetComponentInChildren<Text>();
					new_d_text.font = new_font;

					new_d_option.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, new_d_text.mainTexture);

				}
				new_d_option.transform.SetParent(choice_box.transform.GetChild ((i + base_index) / options_per_column));
				new_d_option.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);
				new_d_option.transform.GetChild (0).GetComponentInChildren<Text>().color = new Color(1f,1f,1f);
				
				int bad_bid_count = 0;
				if (ScriptManager.Game.current.current_decision.fail_bids.Count > i && ScriptManager.Game.current.current_decision.fail_bids[i] > 0)
				{
					bad_bid_count = ScriptManager.Game.current.current_decision.fail_bids[i];
				}
				else if (ScriptManager.Game.current.current_decision.fail_stat_bids.Count > i)
				{
					bad_bid_count = int.Parse (script_manager.find_variable (ScriptManager.Game.current.current_decision.fail_stat_bids[i]));
				}
				
				for (int j=0; j < bad_bid_count; j++)
				{
					GameObject new_icon = Instantiate(Resources.Load("prefabs/AllocationIcon") as GameObject, new Vector3(0f,0f,0f), Quaternion.identity) as GameObject;
					new_icon.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + badbid_graphic_name), typeof(Sprite)) as Sprite;
					new_icon.transform.SetParent (new_d_option.transform.GetChild (1));
					float spacing = 1f / (bad_bid_count + 1);
					int dist_index = (j+1)/2;
					new_icon.GetComponent<CursorBehavior>().set_x_next_frame(new_d_option, dist_index * spacing);
					new_icon.GetComponent<CursorBehavior>().set_y_next_frame(new_d_option, 0.2f);
				}
			}
			break;
		}
		
		// Fill out last column to make option sizes even
		Transform last_column_transform = choice_box.transform.GetChild (choice_box.transform.childCount - 1);
		if (column_count > 1)
		{
			while (last_column_transform.childCount < options_per_column)
			{
				GameObject new_d_option = Instantiate(Resources.Load ("prefabs/DialogueOption2") as GameObject) as GameObject;
				new_d_option.transform.SetParent(last_column_transform);
				new_d_option.GetComponent<CursorBehavior>().set_text_alpha_target(0f);

			}
		}
		
		show_tutorial_box(tutorial_message);
		
		player_choices.Clear ();
		
		for (int i=0; i < player_count; i++)
		{
			player_allocations[i].Clear ();
		}
		
		current_resource_name = ScriptManager.Game.current.current_decision.allocation_currency;

		Debug.Log("Resource for this decision: " + current_resource_name);

		my_stats_to_show.Clear();

		if (ScriptManager.Game.current.current_decision.stats_to_show.Count > 0)
		{
			foreach (string stat_name in ScriptManager.Game.current.current_decision.stats_to_show)
			{
				//Debug.Log ("Adding stat: " + stat_name + " to stat show list");
				my_stats_to_show.Add (stat_name);
			}
		}
		if (!my_stats_to_show.Contains(current_resource_name))
		{
			my_stats_to_show.Add(current_resource_name);
		}

		if (current_resource_name != "" && !my_stats_to_show.Contains (current_resource_name))
		{
			my_stats_to_show.Add (current_resource_name);
		}

		for (int i=1; i <= player_count; i++)
		{
			if (ScriptManager.Game.current.players[i-1].active)
			{
				if (choice_mode == "shop" || choice_mode == "lottery")
				{
					update_player_ui(i,"", my_stats_to_show);
					
					show_ui(i);
				}
				else if (ScriptManager.Game.current.current_decision.stats_to_show.Count > 0)
				{
					update_player_ui(i,"", ScriptManager.Game.current.current_decision.stats_to_show);
				}
				set_ui_font();
			}

			if (my_participants.Contains (i))
			{
				player_choices.Add (0);

			}
			else
			{
				player_choices.Add (-1);
			}
		}
		
		if (fail_timer > 0f)
		{
			timer_running = true;
		}
		
		show_choice_box();
		show_cursor_flag = true;
	}

	public Font find_font(string font_name)
	{
		Font new_font = Resources.Load (ScriptManager.Game.current.story_name + "/fonts/" + font_name, typeof(Font)) as Font;
		if (new_font == null)
		{
			new_font = Resources.Load ("default/fonts/" + font_name, typeof(Font)) as Font;
		}
		if (new_font == null)
		{
			new_font = (Font)Resources.GetBuiltinResource (typeof(Font), "LegacyRuntime.ttf");
		}
		return new_font;
	}

	public void make_spinner (List<int> region_counts)
	{
		SFX_emitter.clip = sound_dict["spinner tie break"];
		SFX_emitter.Play ();

		// set colors
		List<Color> wedge_colors = new List<Color>();
		for (int i=0; i < region_counts.Count; i++)
		{
			if (i < ScriptManager.Game.current.current_choices.Count && ScriptManager.Game.current.current_choices[i].custom_color)
			{
				wedge_colors.Add (script_manager.get_color(ScriptManager.Game.current.current_choices[i].RGBcolor));
			}
			else
			{
				wedge_colors.Add (new Color((region_counts.Count - i) / (region_counts.Count * 1f), i / (region_counts.Count * 1f), 0f));

			}

		}

		//normalize
		wedge_counts.Clear ();
		int total_bids = 0;
		foreach (int number in region_counts)
		{
			total_bids += number;
		}
		for (int i=0; i<region_counts.Count; i++)
		{
			wedge_counts.Add (Mathf.RoundToInt(20f * region_counts[i] / (total_bids * 1f)));
		}
		int wedge_count = 0;
		foreach (int wedges in wedge_counts)
		{
			wedge_count += wedges;
		}
		if (wedge_count > 20)
		{
			bool done = false;
			while (!done)
			{
				int adjusted_index = Random.Range (0, region_counts.Count);
				if (wedge_counts[adjusted_index] >= wedge_count - 20)
				{
					wedge_counts[adjusted_index] -= wedge_count - 20;
					done = true;
				}
			}

		}
		else if (wedge_count < 20)
		{
			int adjusted_index = Random.Range (0, region_counts.Count);
			wedge_counts[adjusted_index] += 20 - wedge_count;
		}

		// REDO or REHASH since not all regions may be used
		List<int> middle_wedges = new List<int>();
		if (wedge_counts[0] > 0)
		{
			middle_wedges.Add (wedge_counts[0] / 2);
		}

		for (int i=1; i<wedge_counts.Count; i++)
		{
			if (wedge_counts[i] > 0)
			{
				middle_wedges.Add (wedge_counts[i-1] + wedge_counts[i] / 2);

			}

			wedge_counts[i] += wedge_counts[i-1];
		}
		wedge_counts.Insert(0, 0);

		float center_x = my_canvas.GetComponent<RectTransform>().rect.width / 2f;
		float center_y = my_canvas.GetComponent<RectTransform>().rect.height / 2f;
		float theta_used = 81f;

		wedge_list.Clear ();
		List<GameObject> middle_wedge_objects = new List<GameObject>();

		for (int i=0; i< 20; i++)
		{
			GameObject new_wedge = Instantiate(Resources.Load ("prefabs/SpinnerWedge") as GameObject) as GameObject;
			new_wedge.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + wedge_graphic_name), typeof(Sprite)) as Sprite;
			new_wedge.transform.SetParent(GameObject.Find ("SpinnerWedges").transform);
			float radius = new_wedge.GetComponent<RectTransform>().rect.width;
			new_wedge.transform.Rotate(new Vector3(0,0,-18f + 360f / 10f + theta_used + 360f / 20f / 2f));
			new_wedge.transform.position = new Vector3 (center_x + radius / 2f * Mathf.Cos (Mathf.Deg2Rad * (theta_used + 360f / 20f / 2f)), center_y + radius / 2f * Mathf.Sin (Mathf.Deg2Rad * (theta_used + 360f / 20f / 2f)), 0f);
			theta_used += 360f / 20f;

			for (int j=0; j < wedge_counts.Count; j++)
			{
				if (i >= wedge_counts[j] && i < wedge_counts[j+1])
				{
					new_wedge.GetComponent<Image>().color = wedge_colors[j];
					if (middle_wedges.Contains (i))
					{
						middle_wedge_objects.Add(new_wedge);
						new_wedge.transform.GetChild(0).rotation = Quaternion.identity;
						if (wedge_colors[j].r + wedge_colors[j].g + wedge_colors[j].b <= white_text_color_threshold)
						{
							new_wedge.GetComponent<CursorBehavior>().set_text_color_target(Color.white);

						}

						Transform new_wedge_t = new_wedge.transform.GetChild (0);
						Text new_wedge_text = new_wedge_t.GetComponent<Text>();
						if (j < ScriptManager.Game.current.current_choices.Count)
						{
							new_wedge_text.text = ScriptManager.Game.current.current_choices[j].text;
							if (font_choice != "")
							{
								Font new_font = find_font (font_choice);
								new_wedge_text.font = new_font;
								new_wedge_t.GetComponent<CanvasRenderer>().SetMaterial (new_font.material, new_wedge_text.mainTexture);
							}
						}
						else
						{
							new_wedge_text.text = ScriptManager.Game.current.current_decision.fail_choices[j - ScriptManager.Game.current.current_choices.Count].text;
							if (font_fail != "")
							{
								Font new_font = find_font (font_fail);
								new_wedge_text.font = new_font;
								new_wedge_t.GetComponent<CanvasRenderer>().SetMaterial (new_font.material, new_wedge_text.mainTexture);
							}
						}

					}
					else
					{
						new_wedge.GetComponentInChildren<Text>().text = "";

					}

					break;
				}
			}
			new_wedge.GetComponent<CursorBehavior>().set_image_alpha_target(1f);
			wedge_list.Add (new_wedge);
		}

		foreach (GameObject wedge in middle_wedge_objects)
		{
			wedge.transform.SetAsLastSibling();
		}

		spinner_hand.GetComponent<CursorBehavior>().set_image_alpha_target(1f);
	}

	public void spin (float force)
	{
		Debug.Log ("Spinning");
		spinner_hand.GetComponent<Rigidbody2D>().AddTorque(force, ForceMode2D.Impulse);
	}

	void OnSpinnerStop (float spinner_rot) 
	{
		SFX_emitter.clip = sound_dict["player drop in"];
		SFX_emitter.Play ();

		int wedge_index = Mathf.FloorToInt((spinner_rot % 360f) / 18);
		if (wedge_index < 0)
		{
			wedge_index += 20;
		}
		Debug.Log ("Stoping on wedge index: " + wedge_index.ToString () + "/" + wedge_list.Count.ToString () + " from angle " + spinner_rot.ToString ());
		wedge_list[wedge_index].GetComponentInChildren<Image>().color = new Color(0,0,0);

		int choice_index = 0;
		for (int j=0; j < wedge_counts.Count; j++)
		{
			if (wedge_index >= wedge_counts[j] && wedge_index < wedge_counts[j+1])
			{
				choice_index = j;
				break;
			}
		}

		StartCoroutine(Wait_to_pick(choice_index, 1f));
		hide_spinner();
	}

	public GameObject get_cursor (int player)
	{
		return player_cursors[player -1];
	}

	public void move_cursor (int player, int amount)
	{
		if (player_choices[player - 1] == 0)
		{
			GameObject cursor = get_cursor(player);

			SFX_emitter.clip = sound_dict["move cursor"];
			SFX_emitter.Play ();

			int new_pos = player_positions[player - 1] + amount;
			int opc = ScriptManager.Game.current.current_decision.options_per_column;
			if (ScriptManager.Game.current.get_column_count() == 1)
			{
				opc = ScriptManager.Game.current.get_choice_count();
			}
			else
			{
				opc = ScriptManager.Game.current.current_decision.options_per_column;
			}

			if (new_pos < 1)
			{
				if (opc <= my_choices.Count)
				{
					int diff = new_pos + opc - my_choices.Count % opc;
					if (diff > 0)
					{
						diff -= opc;
					}
					new_pos = my_choices.Count + diff;
				}
				else if (new_pos == 0)
				{
					new_pos = my_choices.Count;
				}
				else
				{
					new_pos -= amount;
				}
			}
			else if (new_pos > my_choices.Count)
			{
				if (opc <= my_choices.Count)
				{
					new_pos = (new_pos - 1) % opc + 1;

				}
				else if (new_pos == my_choices.Count + 1)
				{
					new_pos = 1;
				}
				else
				{
					new_pos -= amount;

				}
			}

			player_positions[player - 1] = new_pos;

			float y_pos = find_option_from_i(new_pos - 1).position.y;

			// Calculate X position

			float spacing = (choice_box.transform.GetChild(0).GetComponent<RectTransform>().rect.width + cursor.GetComponent<RectTransform>().sizeDelta.x * (2 * ((player-1)/2))) / 2f;
			if (player % 2 == 1)
			{
				spacing *= -1;
			}

			float x_pos = find_option_from_i(new_pos - 1).position.x + spacing;


			cursor.GetComponent<CursorBehavior>().set_translation_target(new Vector2(x_pos, y_pos));
		}
	}

	public void OnPromptPassword ()
	{
		ScriptManager.Game.current.canvas_visibility["password_box"] = true;

		password_box.GetComponent<CursorBehavior>().set_image_alpha_target(1f);
		password_label.GetComponent<CursorBehavior>().set_image_alpha_target(1f);
		password_label.GetComponent<CursorBehavior>().set_text_alpha_target(1f);
/*		Color pbox_color = password_box.GetComponent<Image>().color;
		password_box.GetComponent<Image>().color = new Color(pbox_color.r, pbox_color.g, pbox_color.b, base_gui_alpha);

		Color plabel_color = password_label.GetComponent<Image>().color;
		password_label.GetComponent<Image>().color = new Color(plabel_color.r, plabel_color.g, plabel_color.b, base_gui_alpha);*/
		GameObject placeholder = Instantiate(Resources.Load("prefabs/PasswordPlaceholder")) as GameObject;
		placeholder.transform.SetParent(password_box.transform);
	}

	public void add_password_char ()
	{
		if (password_box.transform.childCount == 1 && password_box.transform.GetChild(0).name == "PasswordPlaceholder(Clone)")
		{
			Destroy (password_box.transform.GetChild(0).gameObject);

		}
		GameObject new_pip = Instantiate (Resources.Load ("prefabs/PasswordPip")) as GameObject;
		new_pip.GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("icons/" + pip_graphic_name), typeof(Sprite)) as Sprite;
		new_pip.transform.SetParent(password_box.transform);

		SFX_emitter.clip = sound_dict["confirm"];
		SFX_emitter.Play ();
	}

	public void remove_password_char ()
	{
		Destroy (password_box.transform.GetChild(0).gameObject);

		SFX_emitter.clip = sound_dict["cancel"];
		SFX_emitter.Play ();

		if (password_box.transform.childCount == 0)
		{
			GameObject placeholder = Instantiate(Resources.Load("prefabs/PasswordPlaceholder")) as GameObject;
			placeholder.transform.SetParent(password_box.transform);
		}
	}

	public void clear_password_pips()
	{
		foreach (Transform childTransform in password_box.transform)
		{
			Destroy (childTransform.gameObject);
		}

	}

	public void hide_password_box ()
	{
		ScriptManager.Game.current.canvas_visibility["password_box"] = false;

		password_box.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		password_label.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		password_label.GetComponent<CursorBehavior>().set_text_alpha_target(0f);
		/*		Color pbox_color = password_box.GetComponent<Image>().color;
		password_box.GetComponent<Image>().color = new Color(pbox_color.r, pbox_color.g, pbox_color.b, 0f);

		Color plabel_color = password_label.GetComponent<Image>().color;
		password_label.GetComponent<Image>().color = new Color(plabel_color.r, plabel_color.g, plabel_color.b, 0f);*/
	}

	public void lock_choice (int player, int choice_i = 0)
	{
		if (choice_i > my_choices.Count || player_choices[player-1] != 0 || (choice_mode.Split (' ')[0] == "hidden" && choice_i == player_choices[player-1]))
		{
			return;
		}
		else if (choice_i > 0)
		{
			player_positions[player - 1] = choice_i;
		}
		int choice = player_positions[player - 1];

		SFX_emitter.clip = sound_dict["confirm"];
		SFX_emitter.Play ();

		if (player_choices[player-1] != -1 && choice > 0 && choice <= my_choices.Count)
		{	
			player_choices[player - 1] = choice;
			SFX_emitter.clip = sound_dict["confirm"];
			SFX_emitter.Play ();

			if (choice_mode == "first")
			{
				commit_choices();
			}
			else if (player_choices.Contains(0) == false)
			{
				if (choice_mode.Split (' ')[0] == "hidden")
				{
					for (int player_i=1;player_i<=4;player_i++)
					{
						if (ScriptManager.Game.current.players[player_i-1].active)
						{
							player_guis[player_i - 1].GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);

						}
					}
				}
				if (choice_mode == "shop" || choice_mode == "lottery")
				{
					commit_allocations();
				}
				else if (choice_mode == "agreement")
				{
					int answer_detected = 0;
					bool matching = true;
					foreach (int answer in player_choices)
					{
						if (answer > 0)
						{
							if (answer_detected == 0)
							{
								answer_detected = answer;
							}
							else if (answer != answer_detected)
							{
								matching = false;
							}
						}
					}
					if (matching)
					{
						commit_choices();
					}
					else
					{
						GameObject cursor = get_cursor(player);

						cursor.GetComponent<Image>().color = color_scale (ScriptManager.Game.current.players[player - 1].get_color(), 0.25f);
						cursor.GetComponent<CursorBehavior>().pulse ("scale", 2f, 0.2f);
						cursor.GetComponent<CursorBehavior>().pulse ("color", 4f, 0.2f);
					}
				}
				else
				{
					commit_choices();
				}
			}
			else
			{
				if (choice_mode.Split (' ')[0] == "hidden")
				{
					player_guis[player - 1].GetComponent<CursorBehavior>().set_image_alpha_target(0.2f);
				}
				else
				{
					GameObject cursor = get_cursor(player);

					cursor.GetComponent<Image>().color = color_scale (ScriptManager.Game.current.players[player - 1].get_color(), 0.25f);
					cursor.GetComponent<CursorBehavior>().pulse ("scale", 2f, 0.2f);
					cursor.GetComponent<CursorBehavior>().pulse ("color", 4f, 0.2f);
				}
			}
		}
	}

	public void unlock_choice (int player)
	{
		if (player_choices[player-1] > 0)
		{
			GameObject cursor = get_cursor(player);
			SFX_emitter.clip = sound_dict["cancel"];
			SFX_emitter.Play ();
			
			player_choices[player - 1] = 0;

			if (ScriptManager.Game.current.current_decision.choice_mode.Split (' ')[0] == "hidden")
			{
				player_guis[player - 1].GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);

			}
			else
			{
				cursor.GetComponent<CursorBehavior>().set_image_color_target(ScriptManager.Game.current.players[player - 1].get_color());
			}
		}
	}

	public Transform find_option_from_i (int i)
	{
		int column = i / ScriptManager.Game.current.current_decision.options_per_column;
		int row = i % ScriptManager.Game.current.current_decision.options_per_column;

		return choice_box.transform.GetChild (column).GetChild (row);
	}

	public void allocate (int player)
	{
		int choice = player_positions[player - 1];

		int d_i = script_manager.get_choice_i_in_decision_from_visible_i(choice - 1);
		
		if ((choice_mode == "shop" || choice_mode == "lottery") && player_choices[player - 1] == 0)
		{
			string stat_key = "player" + player.ToString() + "." + current_resource_name;
			int choice_price = script_manager.get_price_for_choice(ScriptManager.Game.current.current_decision, d_i);

			if (script_manager.get_player_stat_from_string (stat_key).int_val >= choice_price && (script_manager.has_limit_for_choice(ScriptManager.Game.current.current_decision, d_i) == false || script_manager.get_limit_of_choice(ScriptManager.Game.current.current_decision, d_i) > 0))
			{
				SFX_emitter.clip = sound_dict["allocate"];
				SFX_emitter.Play ();

				ScriptManager.Stat this_stat = script_manager.get_player_stat_from_string (stat_key);
				this_stat.int_val -= choice_price;
				player_allocations[player - 1].Add (choice);
				refresh_ui(player, current_resource_name);
				
				GameObject new_icon = Instantiate(Resources.Load("prefabs/AllocationIcon") as GameObject, new Vector3(player_guis[player-1].transform.position.x,player_guis[player-1].transform.position.y,0f), Quaternion.identity) as GameObject;
				new_icon.GetComponent<Image>().sprite = Resources.Load (this_stat.icon_path, typeof(Sprite)) as Sprite;
				new_icon.GetComponent<Image>().color = ScriptManager.Game.current.players[player - 1].get_color (0.4f);
				Transform parent_transform = find_option_from_i(choice - 1);
				Transform side_transform = parent_transform.GetChild (1 + (player - 1) % 2);
				new_icon.transform.SetParent (side_transform);
				float spacing = 0.1f;
				if (player % 2 == 1)
				{
					spacing *= -1;
				}

				new_icon.GetComponent<CursorBehavior>().set_translation_target(new Vector2(0f, find_option_from_i(choice-1).position.y));
				new_icon.GetComponent<CursorBehavior>().set_x_next_frame(parent_transform.gameObject, spacing * side_transform.childCount);

				if (script_manager.has_limit_for_choice(ScriptManager.Game.current.current_decision, d_i) && script_manager.get_limit_of_choice(ScriptManager.Game.current.current_decision, d_i) >= 0)
				{

					script_manager.add_to_limit_of_choice(ScriptManager.Game.current.current_decision, d_i, -1);
					parent_transform.GetChild (0).GetChild (1).GetComponent<Text>().text = parent_transform.GetChild (0).GetChild (1).GetComponent<Text>().text.Split (':')[0] + ": " + script_manager.get_limit_of_choice(ScriptManager.Game.current.current_decision, d_i).ToString();
				}
			}
		}
	}

	public void unallocate (int player)
	{
		if ((choice_mode == "shop" || choice_mode == "lottery") && player_choices[player - 1] == 0)
		{
			int choice = player_positions[player - 1];

			int d_i = script_manager.get_choice_i_in_decision_from_visible_i(choice - 1);
			string stat_key = "player" + player.ToString() + "." + current_resource_name;

			if (player_allocations[player - 1].FindAll(item => item == choice).Count > 0)
			{
				SFX_emitter.clip = sound_dict["unallocate"];
				SFX_emitter.Play ();

				script_manager.get_player_stat_from_string (stat_key).int_val += script_manager.get_price_for_choice(ScriptManager.Game.current.current_decision, d_i);
				player_allocations[player - 1].Remove (choice);
				refresh_ui(player, current_resource_name);
				
				Transform parent_transform = find_option_from_i (choice - 1);
				Transform side_transform = parent_transform.GetChild (1 + (player - 1) % 2);
				GameObject object_removed = side_transform.GetChild (0).gameObject;
				for (int i= side_transform.childCount - 1; i>0; i--)
				{
					Transform child_transform = side_transform.GetChild(i);
					if (child_transform.GetComponent<Image>().color == ScriptManager.Game.current.players[player - 1].get_color (0.4f))
					{
						object_removed = child_transform.gameObject;
						break;
					}
				}

				//GameObject object_removed =	side_transform.GetChild (side_transform.childCount - 1).gameObject;
				object_removed.transform.SetParent(my_canvas.transform);
				object_removed.GetComponent<CursorBehavior>().set_translation_target(new Vector2(player_guis[player-1].transform.position.x, player_guis[player-1].transform.position.y));
				StartCoroutine(Wait_to_destroy (object_removed, message_transition_time));

				if (script_manager.has_limit_for_choice(ScriptManager.Game.current.current_decision, d_i) && script_manager.get_limit_of_choice(ScriptManager.Game.current.current_decision, d_i) >= 0)
				{
					script_manager.add_to_limit_of_choice(ScriptManager.Game.current.current_decision, d_i, 1);
					parent_transform.GetChild (0).GetChild (1).GetComponent<Text>().text = parent_transform.GetChild (0).GetChild (1).GetComponent<Text>().text.Split (':')[0] + ": " + script_manager.get_limit_of_choice(ScriptManager.Game.current.current_decision, d_i).ToString();
				}
			}
		}
	}

	public void commit_choices ()
	{
		timer_running = false;
		hide_choice_box();
		hide_secret_choice_box();
		hide_cursors();
		if (!ScriptManager.Game.current.game_stats_showing)
		{
			hide_game_stats();

		}
		hide_tutorial_box();
		if (ScriptManager.Game.current.current_decision.stats_to_show.Count > 0 || showing_pause_stats)
		{
			showing_pause_stats = false;
			collapse_guis();
		}

		script_manager.declare_answers(player_choices);
	}

	public void commit_allocations ()
	{
		timer_running = false;
		hide_choice_box();
		hide_cursors();
		if (!ScriptManager.Game.current.game_stats_showing)
		{
			hide_game_stats();

		}
		hide_tutorial_box();
		if (ScriptManager.Game.current.current_decision.stats_to_show.Count > 0 || !ScriptManager.Game.current.default_stats_to_show.Contains(ScriptManager.Game.current.current_decision.allocation_currency))
		{
			collapse_guis();
		}

		script_manager.declare_allocations(player_allocations);
	}

	public void set_color_mode (string new_mode, bool force_update = false)
	{
		if (color_mode != new_mode || force_update)
		{
			color_mode = new_mode;

			Color new_color;
			switch (new_mode)
			{
			case "nar":
				new_color = narrator_speech_box_color;
				StartCoroutine(Switch_images(speech_box.GetComponent<Image>(), narrator_speech_box_image, message_transition_time, base_gui_alpha));
				//StartCoroutine(Fade_text_to(speech_box.GetComponentInChildren<Text>(),0f, message_transition_time / 2f));
				speech_box.GetComponent<CursorBehavior>().set_text_alpha_target(0f);
				break;
			case "char":
				new_color = character_speech_box_color;
				if (bubble_tails)
				{
					StartCoroutine(Switch_images(speech_box.GetComponent<Image>(), character_speech_box_image, message_transition_time, 1f));
				}
				else
				{
					StartCoroutine(Switch_images(speech_box.GetComponent<Image>(), character_speech_box_image, message_transition_time, base_gui_alpha));
				}
				break;
			default:
				new_color = narrator_speech_box_color;
				break;
			}

			//Color old_color = speech_box.GetComponent<Image>().color;


			//StartCoroutine(Switch_image_color(speech_box.GetComponent<Image>(), new_color, message_transition_time));
			speech_box.GetComponent<CursorBehavior>().set_image_color_target(new_color);
			speech_box.GetComponent<CursorBehavior>().set_image_alpha_target(new_color.a);

		}
	}

	public void hide_choice_box ()
	{
		ScriptManager.Game.current.canvas_visibility["choice_box"] = false;

		choice_box.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		foreach (Transform column_transform in choice_box.transform)
		{
			column_transform.GetComponent<CursorBehavior>().set_image_alpha_target(0f);

			foreach (Transform option_transform in column_transform)
			{
				option_transform.gameObject.GetComponent<CursorBehavior>().set_text_alpha_target(0f, 0.2f);
				option_transform.gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(0f, 0.2f);
				
				// Remove prices
				if ((ScriptManager.Game.current.current_decision.choice_mode == "shop" || ScriptManager.Game.current.current_decision.choice_mode == "lottery") && option_transform.GetChild (0).childCount > 1)
				{
					option_transform.GetChild (0).GetChild (0).gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(0,0.2f);
					StartCoroutine(Wait_to_destroy (option_transform.GetChild (0).GetChild (0).gameObject, message_transition_time));
				}
				
				// Remove tokens
				for (int i = 1; i <= 2; i++)
				{
					foreach (Transform dead_transform in option_transform.GetChild(i))
					{
						dead_transform.GetComponent<CursorBehavior>().set_image_alpha_target(0,0.2f);
						StartCoroutine(Wait_to_destroy(dead_transform.gameObject, message_transition_time));
					}
				}
			}
		}
	}

	public void show_choice_box ()
	{
		ScriptManager.Game.current.canvas_visibility["choice_box"] = true;

		//StartCoroutine(Fade_to(choice_box.GetComponent<Image>(), 1f, 0.3f));
		choice_box.GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);

		foreach (Transform column_transform in choice_box.transform)
		{
			foreach (Transform option_transform in column_transform)
			{
				if (option_transform.gameObject.activeInHierarchy && option_transform.GetComponentInChildren<Text>().text != "New Text")
				{
					option_transform.gameObject.GetComponent<CursorBehavior>().set_text_alpha_target(1f);
					
					option_transform.gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);
					
				}
			}
		}
		foreach (GameObject cursor in player_cursors)
		{
			//cursor.GetComponent<CursorBehavior>().set_scale_target(new Vector2(choice_box.GetComponent<RectTransform>().rect.height, choice_box.GetComponent<RectTransform>().rect.height) / 200f);
			//cursor.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, choice_box.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().rect.height);


		}
	}

	public void hide_secret_choice_box ()
	{
		ScriptManager.Game.current.canvas_visibility["secret_choice_box"] = false;

		//StartCoroutine(Fade_to(secret_choice_box.GetComponent<Image>(), 0f, 0.3f));
		secret_choice_box.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		secret_choice_box.GetComponent<CursorBehavior>().set_text_alpha_target(0f);

		foreach (Transform child_transform in secret_choice_box.transform)
		{
			//StartCoroutine(Fade_text_to(child_transform.gameObject.GetComponentInChildren<Text>(), 0f, message_transition_time));
			child_transform.gameObject.GetComponent<CursorBehavior>().set_text_alpha_target(0f);

			//StartCoroutine(Fade_to(child_transform.gameObject.GetComponentInChildren<Image>(), 0f, message_transition_time));
			child_transform.gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(0f);

		}
	}

	public void show_secret_choice_box ()
	{
		ScriptManager.Game.current.canvas_visibility["secret_choice_box"] = true;

		//StartCoroutine(Fade_to(secret_choice_box.GetComponent<Image>(), 1f, 0.3f));
		secret_choice_box.GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);

	}

	public void hide_spinner ()
	{
		spinner_hand.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		hide_all_children(spinner_wedges);
		foreach (Transform child_transform in spinner_wedges.transform)
		{
			Wait_to_destroy (child_transform.gameObject, message_transition_time);
		}
	}

	public void hide_ui(int player_i)
	{
		player_guis[player_i-1].GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		player_guis[player_i-1].GetComponent<CursorBehavior>().set_text_alpha_target(0f);
		hide_all_children(player_guis[player_i-1]);
	}

	public void signal_player_change (int player_i, string message)
	{
		GameObject player_ui = player_guis[player_i - 1];
		GameObject new_ui = Instantiate(Resources.Load ("prefabs/PlayerSignal") as GameObject) as GameObject;
		new_ui.transform.SetParent (my_canvas.transform);
		ScriptManager.Game.current.player_alerts_showing[player_i - 1] += 1;

		Text new_ui_text = new_ui.GetComponentInChildren<Text>();
		new_ui_text.text = message;
		new_ui.transform.position = player_ui.transform.position + Vector3.down * player_ui.GetComponent<RectTransform>().rect.height * ScriptManager.Game.current.player_alerts_showing[player_i - 1];
		//new_ui.transform.GetComponent<RectTransform>(). = new Vector2 (player_ui.transform.localScale.x, player_ui.transform.localScale.y);
		if (font_ui != "")
		{
			Font new_font = find_font (font_ui);
			new_ui_text.font = new_font;
			new_ui.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, new_ui_text.mainTexture);
		}
			
		float kill_time = 3f;
		StartCoroutine(Slide_object(new_ui, new_ui.transform.position + Vector3.down * player_ui.GetComponent<RectTransform>().rect.height * 2, kill_time));
		StartCoroutine(Wait_to_fade(new_ui, 0f, kill_time - message_transition_time - 0.02f));
		StartCoroutine(Wait_to_destroy(new_ui, kill_time));
		StartCoroutine(Wait_to_decrement_p_alert(player_i, kill_time));
	}


	public void show_tutorial_box (string message = "", bool pulse = true)
	{
		ScriptManager.Game.current.canvas_visibility["tutorial_box"] = true;

		tutorial_box.GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);
		tutorial_box.GetComponent<CursorBehavior>().set_text_alpha_target(1f);
		if (message.Length > 0)
		{
			tutorial_box.GetComponentInChildren<Text>().text = message;
			if (pulse)
			{
				tutorial_box.GetComponent<CursorBehavior>().pulse("scale", 1.2f, 0.3f);
					
			}
		}
		ScriptManager.Game.current.tutorial_message_showing = message;

	}

	public void hide_tutorial_box ()
	{
		ScriptManager.Game.current.canvas_visibility["tutorial_box"] = false;

		tutorial_box.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		tutorial_box.GetComponent<CursorBehavior>().set_text_alpha_target(0f);
		tutorial_box.GetComponentInChildren<Text>().text = "";
		ScriptManager.Game.current.tutorial_message_showing = "";

	}

	public void image_instant_alpha (float new_alpha, Image target_img)
	{
		target_img.color = new Color (target_img.color.r, target_img.color.g, target_img.color.b, new_alpha);
		foreach (Text child_text in target_img.GetComponentsInChildren<Text>())
		{
			child_text.color = new Color(child_text.color.r, child_text.color.g, child_text.color.b, new_alpha);
		}
	}

	public void hide_uis ()
	{
		ScriptManager.Game.current.canvas_visibility["uis"] = false;

		for (int i = 1; i <= ScriptManager.Game.current.players.Count; i++)
		{
			hide_ui(i);
		}
	}

	public void show_uis ()
	{
		ScriptManager.Game.current.canvas_visibility["uis"] = true;

		for (int i = 1; i <= ScriptManager.Game.current.players.Count; i++)
		{
			if (ScriptManager.Game.current.players[i-1].active)
			{
				show_ui (i);
			}
		}
	}

	public void hide_all_children(GameObject parent)
	{
		foreach (Transform child_transform in parent.transform)
		{
			if (child_transform.GetComponent<CursorBehavior>() != null)
			{
				child_transform.gameObject.GetComponent<CursorBehavior>().set_text_alpha_target(0f);
				child_transform.gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
			}
			else if (child_transform.GetComponent<Image>() != null)
			{
				child_transform.GetComponent<Image>().color = new Color(1f,1f,1f,0f);
			}
			else if (child_transform.GetComponent<Text>() != null)
			{
				child_transform.GetComponent<Text>().color = new Color(1f,1f,1f,0f);
			}
			if (child_transform.childCount > 0)
			{
				hide_all_children(child_transform.gameObject);
			}
		}
/*		foreach (Text blurb in parent.GetComponentsInChildren<Text>())
		{
			StartCoroutine(Fade_text_to(blurb, 0f, duration));
		}
		foreach (Image thing in parent.GetComponentsInChildren<Image>())
		{
			StartCoroutine(Fade_to(thing, 0f, duration));
		}*/
	}

	public void show_all_children(GameObject parent)
	{
		foreach (Transform child_transform in parent.transform)
		{
			if (child_transform.GetComponent<CursorBehavior>() != null)
			{
				child_transform.gameObject.GetComponent<CursorBehavior>().set_text_alpha_target(1f);
				child_transform.gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(1f);
			}
			else if (child_transform.GetComponent<Image>() != null)
			{
				child_transform.GetComponent<Image>().color = Color.white;
			}
			else if (child_transform.GetComponent<Text>() != null)
			{
				child_transform.GetComponent<Text>().color = Color.white;
			}
		}
	}

	public void show_ui(int player_i)
	{
		player_guis[player_i-1].GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);
		player_guis[player_i-1].GetComponent<CursorBehavior>().set_text_alpha_target(1f);
		player_guis[player_i-1].GetComponent<CursorBehavior>().pulse("scale",1.5f,0.3f);
		show_all_children(player_guis[player_i - 1]);
/*		foreach (Text blurb in player_guis[player_i - 1].GetComponentsInChildren<Text>())
		{
			StartCoroutine(Fade_text_to(blurb, 1f, message_transition_time));
		}
		foreach (Image thing in player_guis[player_i - 1].GetComponentsInChildren<Image>())
		{
			StartCoroutine(Fade_to(thing, 1f, message_transition_time));
		}*/
	}

	public void hide_cursors ()
	{
		for (int i=1; i <= player_count; i++)
		{
			if (player_choices[i-1] > 0)
			{
/*				Image target_image = get_cursor(i).GetComponent<Image>();
				target_image.color = new Color(target_image.color.r * 4f, target_image.color.g * 4f, target_image.color.b * 4f);*/
				get_cursor(i).GetComponent<CursorBehavior>().set_image_color_target(ScriptManager.Game.current.players[i-1].get_color());
			}
			//StartCoroutine(Fade_to(get_cursor(i).GetComponent<Image>(), 0f, message_transition_time));
			get_cursor (i).GetComponent<CursorBehavior>().set_image_alpha_target(0f);
			//get_cursor(i).SetActive(false);
		}
	}

	public void show_cursors ()
	{
		for (int i=1; i <= player_count; i++)
		{
			player_positions[i-1] = 0;
			move_cursor(i, 1);
			if (player_choices[i-1] != -1)
			{
				get_cursor (i).GetComponent<CursorBehavior>().set_image_alpha_target(1f);
				get_cursor (i).GetComponent<CursorBehavior>().set_image_color_target(ScriptManager.Game.current.players[i - 1].get_color());
			}
		}
	}

	public void hide_canvas()
	{
		my_canvas.SetActive (false);
	}

	public void show_canvas()
	{
		my_canvas.SetActive(true);
	}

	public string get_speaker_pose ()
	{
		if (player_speaker_name == "")
		{
			return ScriptManager.Game.current.stage_poses[get_speaker_i(ScriptManager.Game.current.current_speaker_pos)];
		}
		else
		{
			return "normal";
		}
	}

	public string get_char_pos (string char_name = "")
	{
		string output = "";
		if (char_name == "")
		{
			output = ScriptManager.Game.current.current_speaker_pos;
		}
		else if (script_manager.is_player_name(char_name))
		{
			int player = script_manager.player_number_from_name(char_name);
			if (player % 2 == 0)
			{
				output = "PR";
			}
			else
			{
				output = "PL";
			}

		}
		else if (ScriptManager.Game.current.bg_actors_left.Contains(char_name))
		{
			output = "BGL" + ScriptManager.Game.current.bg_actors_left.IndexOf(char_name).ToString();
		}
		else if (ScriptManager.Game.current.bg_actors_right.Contains(char_name))
		{
			output = "BGR" + ScriptManager.Game.current.bg_actors_right.IndexOf(char_name).ToString();
		}
		else
		{
			for (int i = 0; i < ScriptManager.Game.current.stage_actors.Count; i ++)
			{
				if (ScriptManager.Game.current.stage_actors[i] == char_name)
				{
					output = get_speaker_side(i);
				}
			}
		}
		return output;
	}

	public string get_speaker_side (int char_i)
	{
		List<string> pos_list = new List<string>();
		pos_list.Add ("L");
		pos_list.Add ("C");
		pos_list.Add ("R");
		return pos_list[char_i];
	}

	public string get_side (string pos)
	{
		if (pos.StartsWith("BG"))
		{
			return pos.Substring(2,1);
		}
		else if (pos.StartsWith("P"))
		{
			return pos.Substring(1,1);
		}
		else if (pos == "L" || pos == "left")
		{
			return "L";
		}
		else if (pos == "R" || pos == "right")
		{
			return "R";
		}
		else if (pos == "C" || pos == "center")
		{
			return "C";
		}
		else
		{
			return "L";
		}
	}

	public int get_speaker_i (string pos)
	{
		int output;
		switch (pos)
		{
		case "L":
			output = 0;
			break;
		case "C":
			output = 1;
			break;
		case "R":
			output = 2;
			break;
		default:
			output = 0;
			break;
		}
		return output;
	}

	public void new_name (string next_name, string pos = "L")
	{
		//Debug.Log("New name at pos: " + pos);
		Color new_text_color = Color.black;
		Color new_bg_color = Color.white;
		bool dont_pulse_flag = false;

		if (bubble_tails)
		{
			blabber_name_plate.GetComponent<CursorBehavior>().set_image_color_target(character_speech_box_color);
		}
		else
		{
			if (ScriptManager.Game.current.char_dict.ContainsKey(next_name))
			{
				new_bg_color = ScriptManager.Game.current.char_dict[next_name].get_color();
				if (new_bg_color.r + new_bg_color.g + new_bg_color.b <= white_text_color_threshold)
				{
					new_text_color = Color.white;
				}
			}
			StartCoroutine(Switch_text(blabber_name, next_name, "C", message_transition_time, new_text_color));
			//StartCoroutine(Switch_image_color(blabber_name_plate, new_bg_color, message_transition_time));
			blabber_name_plate.GetComponent<CursorBehavior>().set_image_color_target(new_bg_color);
		}

		float spacing = (speech_box.GetComponent<RectTransform>().rect.width) / 3f;
		Vector2 new_pos = new Vector2 (blabber_name_plate.transform.position.x, blabber_name_plate.transform.position.y);
		Image image_to_pulse = portrait_L_image;
		if (pos.StartsWith("BG"))
		{
			GameObject img_obj = get_object_from_name(next_name);
			if (img_obj == null)
			{
				dont_pulse_flag = true;
			}
			else
			{
				image_to_pulse = img_obj.GetComponent<Image>();
				if (pos.Substring(2,1) == "R")
				{
					new_pos = new Vector2(speech_box.transform.position.x + spacing, blabber_name.transform.position.y);

				}
				else
				{
					new_pos = new Vector2(speech_box.transform.position.x - spacing, blabber_name_plate.transform.position.y);

				}
			}
		}
		else
		{
			switch (pos)
			{
			case "L":
				new_pos = new Vector2(speech_box.transform.position.x - spacing, blabber_name_plate.transform.position.y);
				image_to_pulse = portrait_L_image;
				break;
			case "C":
				new_pos = new Vector2(speech_box.transform.position.x, blabber_name_plate.transform.position.y);
				image_to_pulse = portrait_C_image;
				break;
			case "R":
				new_pos = new Vector2(speech_box.transform.position.x + spacing, blabber_name.transform.position.y);
				image_to_pulse = portrait_R_image;
				break;
			}
		}


		//Debug.Log("Switching name to: " + next_name + " at position: " + pos);


		//StartCoroutine(Slide_object(blabber_name_plate.gameObject, new_pos, message_transition_time));
		if (speaker_transition_slide)
		{
			blabber_name_plate.GetComponent<CursorBehavior>().set_translation_target(new_pos);
		}
		else
		{
			if (blabber_name_plate.color.a == 0f)
			{
				blabber_name_plate.transform.position = new Vector3(new_pos.x, new_pos.y, 0);
				blabber_name_plate.GetComponent<CursorBehavior>().set_translation_target(new_pos);

			}
			else
			{
				StartCoroutine(Teleport(blabber_name_plate, new_pos, message_transition_time));
			}
		}

		if (!script_manager.is_player_name(next_name) && next_name != "")
		{
			//ScriptManager.Game.current.stage_actors[get_speaker_i (pos)] = next_name;
			if (speaker_transition_pulse && ! dont_pulse_flag)
			{
				image_to_pulse.GetComponent<CursorBehavior>().pulse ("scale", 1.1f, 0.3f);
			}

		}
	}

	public void set_auto_progress_slider_alpha(float new_val)
	{
		//Debug.Log ("Progress slider: " + new_val.ToString ());
		Color my_color = auto_progress_slider.GetComponentInChildren<Image>().color;
		auto_progress_slider.GetComponentInChildren<Image>().color = new Color(my_color.r, my_color.g, my_color.b, new_val);
	
/*		foreach (Transform childTransform in auto_progress_slider.transform)
		{
			foreach (Image childImage in childTransform.GetComponentsInChildren<Image>())
			{
				Color child_color = childImage.color;
				childImage.color = new Color(child_color.r, child_color.g, child_color.b, new_val);
			}
		}*/

	}
	
	public void new_message (string next_message, string pos = "")
	{
		if (script_manager.time_everything)
		{
			timing_dialogue = true;
			if (use_type_in)
			{
				fail_timer = next_message.Length / characters_per_second + 3f;
			}
			else
			{
				fail_timer = next_message.Length / 30f + 3f;
			}
			auto_progress_duration = fail_timer;
			auto_progress_slider.GetComponent<Image>().fillAmount = 1f;

			set_auto_progress_slider_alpha(1f);
		}

		if (player_speaker_pos != "")
		{
			clear_player_speaker();
			//new_name (ScriptManager.Game.current_speaker_name, get_char_pos());
		}

		show_speech_box();
		if (use_type_in && !showing_recap)
		{
			if (typing_in)
			{
				finish_typein();
			}
			if (color_mode == "char")
			{
				StartCoroutine(Switch_text(blabber_text, "", pos, message_transition_time, character_text_color));
				my_line_text = next_message;
				typein_signal = true;
			}
			else
			{
				StartCoroutine(Switch_text(blabber_text, next_message, pos, message_transition_time, character_text_color));
			}
		}
		else
		{
			StartCoroutine(Switch_text(blabber_text, next_message, pos, message_transition_time, character_text_color));
		}
		if (pos != "")
		{
			ScriptManager.Game.current.current_speaker_pos = pos;
			//Debug.Log ("Changing current speaker position to " + pos + ", for message" + next_message);
		}
	}

	public void player_message (string next_name, string next_pos, string next_anim, string next_message)
	{
		if (player_speaker_name != next_name)
		{
			if (player_speaker_name != "")
			{
				hide_player_portrait(player_speaker_pos);
				StartCoroutine(Wait_to_show_player_portrait(message_transition_time));
			}
			else
			{
				show_player_portrait(next_anim, next_pos);
			}
			new_name (next_name, next_pos.Substring (1,1));
		}
		else if (player_speaker_pos != next_pos)
		{
			hide_player_portrait(player_speaker_pos);
			StartCoroutine(Wait_to_show_player_portrait(message_transition_time));
		}
		else if (player_speaker_portrait != next_anim)
		{
			GameObject target_object;
			if (player_speaker_pos == "PL")
			{
				target_object = player_portrait_L;
			}
			else
			{
				target_object = player_portrait_R;
			}
			StartCoroutine(Switch_images(target_object.GetComponent<Image>(), Resources.Load (ScriptManager.Game.current.get_file_path("portraits/" + next_anim), typeof(Sprite)) as Sprite, message_transition_time, 1, next_anim));
		}
		player_speaker_name = next_name;
		player_speaker_pos = next_pos;
		player_speaker_portrait = next_anim;
		if (use_type_in)
		{
			if (typing_in)
			{
				finish_typein();
			}
			StartCoroutine(Switch_text(blabber_text, "", next_pos.Substring (1,1), message_transition_time, Color.white));
			my_line_text = next_message;
			typein_signal = true;
		}
		else
		{
			StartCoroutine(Switch_text(blabber_text, next_message, next_pos.Substring (1,1), message_transition_time, Color.white));
		}
	}

	public void clear_player_speaker ()
	{
		hide_player_portrait(player_speaker_pos);
		player_speaker_name = "";
		player_speaker_pos = "";
		player_speaker_portrait = "";
		script_manager.declare_pause(message_transition_time);
	}

	public void message_approval_of (int player)
	{
		int arrow_index = 0;
		if (player > player_count / 2)
		{
			arrow_index = player;
		}
		else
		{
			arrow_index = player - 1;
		}
		approval_box.transform.GetChild (arrow_index).GetComponent<CursorBehavior>().set_image_alpha_target(0.2f);

		if (arrow_index != player_count / 2)
		{
			approval_box.transform.GetChild (arrow_index).GetComponent<CursorBehavior>().pulse("squish",0.3f, message_transition_time / 2f);
		}
	}

	public void hide_approvals ()
	{
		ScriptManager.Game.current.canvas_visibility["approvals"] = false;

		hide_all_children(approval_box);
	}

	public void show_approvals ()
	{
		ScriptManager.Game.current.canvas_visibility["approvals"] = true;

		if (ScriptManager.Game.current.get_current_line().pause || script_manager.pause_all_lines)
		{
			int arrow_index = 0;
			for (int i = 0; i < player_count; i++)
			{
				if (ScriptManager.Game.current.players[i].active)
				{
					if (i >= player_count / 2)
					{
						arrow_index = i + 1;
					}
					else
					{
						arrow_index = i;
					}
					approval_box.transform.GetChild (arrow_index).GetComponent<CursorBehavior>().set_image_color_target(ScriptManager.Game.current.players[i].get_color());
					
					approval_box.transform.GetChild (arrow_index).GetComponent<CursorBehavior>().set_image_alpha_target(1f);
					approval_box.transform.GetChild (arrow_index).GetComponent<CursorBehavior>().pulse("scale", 1.5f, message_transition_time * 0.8f);
				}
			}
		}

		else
		{
			approval_box.transform.GetChild (player_count / 2).GetComponent<CursorBehavior>().set_image_alpha_target(1f);
			approval_box.transform.GetChild (player_count / 2).GetComponent<CursorBehavior>().pulse ("scale",1.5f, message_transition_time * 0.8f);
		}

	}

	public void add_message (string next_message)
	{
		blabber_text.text += next_message;
	}

	public void clear_bg_portraits()
	{
		while (bg_left_portraits.transform.childCount > 0)
		{
			Transform target_transform = bg_left_portraits.transform.GetChild(0);
			target_transform.SetParent(bg_image.transform);
			target_transform.SetAsFirstSibling();
			target_transform.GetComponent<CursorBehavior>().set_translation_target(PL_hide, destroy_on_arrival:true);
		}
		while (bg_right_portraits.transform.childCount > 0)
		{
			Transform target_transform = bg_right_portraits.transform.GetChild(0);
			target_transform.SetParent(bg_image.transform);
			target_transform.SetAsFirstSibling();
			target_transform.GetComponent<CursorBehavior>().set_translation_target(PR_hide, destroy_on_arrival:true);
		}
	}

	public void set_portrait (string pos, string speaker_pic, string old_pos = "", string player_name = "", string direction = "", string transition = "")
	{
		if (speaker_pic == "" && old_pos != "") // happens when sliding portraits
		{
			if (old_pos.StartsWith("BG"))
			{
				string char_name = "";
				string char_pose = "";
				string char_pos = old_pos.Substring(2,1);
				int char_i = int.Parse(old_pos.Substring(3));
				if (char_pos == "L")
				{
					char_name = ScriptManager.Game.current.bg_actors_left[char_i];
					char_pose = ScriptManager.Game.current.bg_poses_left[char_i];
				}
				else if (char_pos == "R")
				{
					char_name = ScriptManager.Game.current.bg_actors_right[char_i];
					char_pose = ScriptManager.Game.current.bg_poses_right[char_i];
				}
				speaker_pic = script_manager.plug_in_portrait_name (char_name, char_pose, get_side(pos));

			}
			else
			{
				int old_pos_i = get_speaker_i(old_pos);
				speaker_pic = script_manager.plug_in_portrait_name (ScriptManager.Game.current.stage_actors[old_pos_i], ScriptManager.Game.current.stage_poses[old_pos_i], get_side(pos));
			}
		}

		Sprite new_sprite = Resources.Load (ScriptManager.Game.current.get_file_path("portraits/" + speaker_pic), typeof(Sprite)) as Sprite;
		Image old_image;
		Image new_image;

		List<string> left_sliders = new List<string>{"L","C"};

		bool new_side_portrait = false;
		if (pos.StartsWith("BG") && pos.Length == 3)
		{
			new_side_portrait = true;
			if (pos.Substring(2,1) == "R")
			{
				pos += bg_right_portraits.transform.childCount.ToString();
			}
			else
			{
				pos += bg_left_portraits.transform.childCount.ToString();
			}
		}

		GameObject portrait_obj = get_object_from_pos(pos);
		if (pos.StartsWith("BG") && portrait_obj == null)
		{
			//Debug.Log("Creating new background character on " + pos.Substring(2,1));
			GameObject new_obj = Instantiate(Resources.Load ("prefabs/PortraitL") as GameObject) as GameObject;
			new_obj.GetComponent<RectTransform>().sizeDelta = portrait_C_image.GetComponent<RectTransform>().rect.size;
			new_image = new_obj.GetComponent<Image>();
			if (pos.Substring(2,1) == "R")
			{
				new_obj.transform.SetParent(bg_right_portraits.transform);
			}
			else
			{
				new_obj.transform.SetParent(bg_left_portraits.transform);
			}
			new_obj.transform.SetAsFirstSibling();
		}
		else if (portrait_obj == null)
		{
			return;
		}
		else
		{
			new_image = portrait_obj.GetComponent<Image>();
		}

		GameObject old_image_obj = get_object_from_pos(old_pos);
		if (old_image_obj == null)
		{
			old_image = get_object_from_pos("L").GetComponent<Image>();
		}
		else
		{
			old_image = old_image_obj.GetComponent<Image>();
		}

		bool should_slide = (new_speaker_slide && transition != "fade") || transition == "slide";

		//Debug.Log("transition: " + transition + ", should_slide: " + should_slide.ToString());
		//Debug.Log(ScriptManager.Game.current.stage_actors[0] + "," + ScriptManager.Game.current.stage_actors[1] + "," + ScriptManager.Game.current.stage_actors[2]);

		if (should_slide && !showing_recap)
		{
			if (old_pos == "")
			{
				//Debug.Log("Speaker not on screen");
				if (speaker_pic == "transparent") // Exiting character - slide out
				{
					//Debug.Log("Character exiting at pos: " + pos);
					for (int i = 0; i< bg_left_portraits.transform.childCount; i++)
					{
						if (bg_left_portraits.transform.GetChild(i) == portrait_obj.transform)
						{
							break;
						}
					}
					bool destroy_me = pos.StartsWith("BG"); // if sliding out a background image, unparent and destroy on arrival for cleanup
					if (destroy_me)
					{
						new_image.transform.SetParent(bg_image.transform);
						new_image.transform.SetAsFirstSibling();
					}
					else
					{
						StartCoroutine(Wait_to_switch_images(new_image, new_sprite, message_transition_time, 0f));

					}

					if (direction == "left" || ((left_sliders.Contains (pos) || pos.StartsWith("BGL")) && direction != "right"))
					{
						new_image.gameObject.GetComponent<CursorBehavior>().set_translation_target(PL_hide, destroy_on_arrival:destroy_me);
					}
					else
					{
						new_image.gameObject.GetComponent<CursorBehavior>().set_translation_target(PR_hide, destroy_on_arrival:destroy_me);
					}
				}
				else
				{
					//Debug.Log("pos: " + pos + ", object found: " + (get_object_from_pos(pos) != null).ToString());
					if ((pos.StartsWith("BG") && !new_side_portrait) || (!pos.StartsWith("BG") && ScriptManager.Game.current.stage_actors[get_speaker_i(pos)] != "")) // Character needs to slide out first
					{
						if (left_sliders.Contains (pos) || pos.StartsWith("BGL"))
						{
							new_image.gameObject.GetComponent<CursorBehavior>().set_translation_target(PL_hide);
						}
						else
						{
							new_image.gameObject.GetComponent<CursorBehavior>().set_translation_target(PR_hide);
						}
						StartCoroutine(Wait_to_set_translation_target(new_image.gameObject, CLR_pos_list[get_speaker_i(pos)], message_transition_time / 2f));
						StartCoroutine(Wait_to_switch_images(new_image, new_sprite, message_transition_time / 2f, 0f, speaker_pic));
					}
					else // New character - slide in
					{
						if (left_sliders.Contains (pos) || pos.StartsWith("BGL"))
						{
							new_image.transform.position = PL_hide;
						}
						else
						{
							new_image.transform.position = PR_hide;
						}
						if (!pos.StartsWith("BG"))
						{
							new_image.gameObject.GetComponent<CursorBehavior>().set_translation_target(CLR_pos_list[get_speaker_i(pos)]);
						}
						StartCoroutine(Wait_to_switch_images(new_image, new_sprite, message_transition_time, 0f, speaker_pic));

					}

				}
			}
			else if (pos == old_pos)
			{
				Debug.Log("Position not changed. Switching images for pose");
				StartCoroutine(Switch_images(new_image, new_sprite, message_transition_time, 1, speaker_pic));

			}
			else // Character moved around scene - set position and slide to new
			{
				new_image.sprite = old_image.sprite;
				old_image.GetComponent<SpriteAnimator>().stop ();
				new_image.transform.position = old_image.transform.position;
				if (old_pos.StartsWith("BG"))
				{
					Destroy(old_image_obj);
				}
				else
				{
					old_image.sprite = Resources.Load (ScriptManager.Game.current.get_file_path("portraits/transparent"), typeof(Sprite)) as Sprite;
				}
				if (!pos.StartsWith("BG"))
				{
					new_image.gameObject.GetComponent<CursorBehavior>().set_translation_target(CLR_pos_list[get_speaker_i(pos)]);

				}
				if (new_image.sprite != new_sprite)
				{
					StartCoroutine(Switch_images(new_image, new_sprite, message_transition_time, 1, speaker_pic));
				}
			}
		}
		else
		{
			if (old_pos != "")
			{
				if (old_pos.StartsWith("BG"))
				{
					set_portrait(pos, "transparent");
				}
				else
				{
					StartCoroutine(Switch_images(old_image, Resources.Load (ScriptManager.Game.current.get_file_path("portraits/transparent"), typeof(Sprite)) as Sprite, message_transition_time, 1, speaker_pic));
				}
			}
			StartCoroutine(Switch_images(new_image, new_sprite, message_transition_time, 1, speaker_pic));

			new_image.gameObject.GetComponent<CursorBehavior>().set_translation_target(CLR_pos_list[get_speaker_i(pos)]);
			new_image.gameObject.transform.position = CLR_pos_list[get_speaker_i(pos)];
		}
		line_up_bg_char_flag = true;
		//line_up_bg_portraits();
	}

	void line_up_bg_portraits ()
	{

		for (int i = 0; i < bg_left_portraits.transform.childCount; i++)
		{
			bg_left_portraits.transform.GetChild(i).GetComponent<CursorBehavior>().set_translation_target(bg_portrait_coordinates("L", bg_left_portraits.transform.childCount - (i+1)));
		}
		for (int i = 0; i < bg_right_portraits.transform.childCount; i++)
		{
			bg_right_portraits.transform.GetChild(i).GetComponent<CursorBehavior>().set_translation_target(bg_portrait_coordinates("R", bg_right_portraits.transform.childCount - (i+1)));
		}
	}

	public void clear_name ()
	{
		blabber_name.text = "";
		//ScriptManager.Game.current.stage_actors[get_speaker_i(ScriptManager.Game.current.current_speaker_pos)] = "";
	}

	public void clear_message()
	{
		blabber_text.text = "";
	}

	public void clear_speaker (string char_name, string direction = "", string transition_type = "")
	{
		string target_pos = get_char_pos(char_name);
		set_portrait(target_pos, "transparent", direction: direction, transition: transition_type);

		// bookkeeping
		if (target_pos.StartsWith("BG"))
		{
			if (target_pos.Substring(2,1) == "R")
			{
				int bg_index = int.Parse(target_pos.Substring(3));
				ScriptManager.Game.current.bg_actors_right.RemoveAt(bg_index);
			}
			else if (target_pos.Substring(2,1) == "L")
			{
				int bg_index = int.Parse(target_pos.Substring(3));
				ScriptManager.Game.current.bg_actors_left.RemoveAt(bg_index);
			}
		}

		else
		{
			int speaker_i = get_speaker_i(target_pos);
			ScriptManager.Game.current.stage_actors[speaker_i] = "";
		}
	}

	public Vector2 bg_portrait_coordinates (string side, int rank)
	{
		if (side == "R")
		{
			float x_diff = (CLR_pos_list[2].x - bg_character_right_min_x) / bg_right_portraits.transform.childCount;
			return new Vector2(CLR_pos_list[2].x - (rank +1) * x_diff, CLR_pos_list[2].y + (rank +1) * x_diff / 3f);
		}
		else
		{
			float x_diff = (bg_character_left_max_x - CLR_pos_list[0].x) / bg_left_portraits.transform.childCount;
			return new Vector2(CLR_pos_list[0].x + (rank +1) * x_diff, CLR_pos_list[0].y + (rank +1) * x_diff / 3f);	
		}
	}

	public void clear_all_speakers (string transition_type = "")
	{
		set_portrait("L", "transparent", transition: transition_type);
		set_portrait("C", "transparent", transition: transition_type);
		set_portrait("R", "transparent", transition: transition_type);

		ScriptManager.Game.current.stage_actors = new List<string>{"","",""};
		hide_player_portrait("PL");
		hide_player_portrait("PR");

		for (int i = 0; i < ScriptManager.Game.current.bg_actors_left.Count; i++)
		{
			set_portrait("BGL0", "transparent");
		}
		ScriptManager.Game.current.bg_actors_left.Clear();
		for (int r_i = 0; r_i < ScriptManager.Game.current.bg_actors_right.Count; r_i++)
		{
			set_portrait("BGR0", "transparent");

		}
		ScriptManager.Game.current.bg_actors_right.Clear();
	}

	public void hide_all_speakers()
	{
		set_portrait("L", "transparent");
		set_portrait("C", "transparent");
		set_portrait("R", "transparent");

		for (int i = 0; i < ScriptManager.Game.current.bg_actors_left.Count; i++)
		{
			set_portrait("BGL0", "transparent");
		}
		for (int r_i = 0; r_i < ScriptManager.Game.current.bg_actors_right.Count; r_i++)
		{
			set_portrait("BGR0", "transparent");
		}
	}

	public void clear_all()
	{
		clear_name ();
		clear_message();
		clear_all_speakers();
	}

	public GameObject get_object_from_name  (string my_name)
	{
		return get_object_from_pos(get_char_pos(my_name));
	}

	public GameObject get_object_from_pos (string pos)
	{
		GameObject target = null;

		switch (pos)
		{
		case "L":
			target = portrait_L_image.gameObject;
			break;
		case "C":
			target = portrait_C_image.gameObject;
			break;
		case "R":
			target = portrait_R_image.gameObject;
			break;
		case "PL":
			target = player_portrait_L;
			break;
		case "PR":
			target = player_portrait_R;
			break;
		}

		if (pos.StartsWith("BGL"))
		{
			if (pos.Length == 3 || bg_left_portraits.transform.childCount < int.Parse(pos.Substring(3)) + 1)
			{
				target = null;
			}
			else
			{
				int depth = int.Parse(pos.Substring(3));
				target = bg_left_portraits.transform.GetChild(bg_left_portraits.transform.childCount - 1 - depth).gameObject;
			}
		}
		else if (pos.StartsWith("BGR"))
		{
			if (pos.Length == 3 || bg_right_portraits.transform.childCount < int.Parse(pos.Substring(3)) + 1)
			{
				target = null;
			}
			else
			{
				int depth = int.Parse(pos.Substring(3));
				target = bg_right_portraits.transform.GetChild(bg_right_portraits.transform.childCount - 1 - depth).gameObject;
			}
		}
		return target;
	}

	public void blink_speaker (string char_name, float intensity = 0.1f, float duration = 0.5f, float gain = 0.05f)
	{
		GameObject target = get_object_from_pos(get_char_pos(char_name));

		target.GetComponent<CursorBehavior>().pulse ("alpha", intensity, duration, gain);
	}

	public void throb_speaker (string char_name, float intensity = 1.1f, float duration = 0.3f, float gain = 0.05f)
	{
		GameObject target = get_object_from_pos(get_char_pos(char_name));
		
		target.GetComponent<CursorBehavior>().pulse ("scale", intensity, duration, gain);
	}

	public void darken_speaker (string char_name, float intensity = 0.1f, float duration = 0.5f, float gain = 0.05f)
	{
		GameObject target = get_object_from_pos(get_char_pos(char_name));
		
		target.GetComponent<CursorBehavior>().pulse ("color", intensity, duration, gain);
	}

	public void blush_speaker (string char_name, float intensity = 0.5f, float duration = 0.5f, float gain = 0.05f, string color = "red")
	{
		GameObject target = get_object_from_pos(get_char_pos(char_name));
		
		target.GetComponent<CursorBehavior>().pulse ("tint", intensity, duration, gain, color);
	}

	public void shake_speaker (string char_name, float intensity = 20f, float decay = 0.95f)
	{
		GameObject target = get_object_from_pos(get_char_pos(char_name));
		
		StartCoroutine(Shake_thing(target, intensity, decay));
	}
	
	public void squish_speaker (string char_name, float intensity = 0.7f, float duration = 0.3f, float gain = 0.1f)
	{
		GameObject target = get_object_from_pos(get_char_pos(char_name));
		
		target.GetComponent<CursorBehavior>().pulse ("squish", intensity, duration, gain);
	}

	public bool speech_box_is_showing ()
	{
		if (speech_box.GetComponent<CursorBehavior>().get_image_color_target().a > 0f)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public void hide_speech_box()
	{
		ScriptManager.Game.current.canvas_visibility["speech_box"] = false;

		speech_box.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		blabber_name_plate.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
		hide_approvals();

		if (script_manager.time_everything)
		{
			set_auto_progress_slider_alpha(0f);

		}

		blabber_text.text = "";
		blabber_name.text = "";
	}

	public void show_speech_box()
	{
		ScriptManager.Game.current.canvas_visibility["speech_box"] = true;
		switch (color_mode)
		{
		case "nar":
			speech_box.GetComponent<CursorBehavior>().set_image_color_target(narrator_speech_box_color);
			blabber_name_plate.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
			speech_box.GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);

			break;
		case "char":
			//speech_box.GetComponent<Image>().color = character_speech_box_color;
			speech_box.GetComponent<CursorBehavior>().set_image_color_target(character_speech_box_color);

			if (bubble_tails)
			{
				speech_box.GetComponent<CursorBehavior>().set_image_alpha_target(1f);
			}
			else
			{
				speech_box.GetComponent<CursorBehavior>().set_image_alpha_target(base_gui_alpha);
			}

			break;
		}
	}

	public void show_player_portrait (string img_name, string pos = "PL")
	{
		ScriptManager.Game.current.canvas_visibility["player_portraits"] = true;

		Sprite new_img = Resources.Load (ScriptManager.Game.current.get_file_path("portraits/" + img_name), typeof(Sprite)) as Sprite;
		GameObject target_object;
		Vector3 new_pos = new Vector2();

		player_speaker_pos = pos;

		switch (pos)
		{
		case "PL":
			target_object = player_portrait_L;
			new_pos = PL_show;
			break;
		case "PR":
			target_object = player_portrait_R;
			new_pos = PR_show;
			break;
		default:
			target_object = player_portrait_L;
			new_pos = PL_show;
			break;
		}
		StartCoroutine(Switch_images(target_object.GetComponent<Image>(), new_img, message_transition_time, 1, img_name));
		//StartCoroutine(Slide_object(target_object, new_pos, message_transition_time));
		target_object.GetComponent<CursorBehavior>().set_translation_target(new_pos);
	}

	public void show_pause_portrait (int portrait_i, string sprite_path, string anim_check_name = "")
	{
		pause_portraits[portrait_i].GetComponent<SpriteAnimator>().stop ();

		if (anim_check_name != "" && anim_dict.ContainsKey(anim_check_name))
		{
			anim_dict[anim_check_name].apply_to(pause_portraits[portrait_i]);
			pause_portraits[portrait_i].GetComponent<SpriteAnimator>().play ();
		}
		else
		{
			pause_portraits[portrait_i].GetComponent<Image>().sprite = Resources.Load (sprite_path, typeof(Sprite)) as Sprite;
		}
		pause_portraits[portrait_i].GetComponent<CursorBehavior>().set_image_alpha_target(1f);
	}

	public void hide_pause_portrait (int portrait_i)
	{

		pause_portraits[portrait_i].GetComponent<CursorBehavior>().set_image_alpha_target(0f);
	}

	public void set_pause_portrait_visibility (bool new_val)
	{
		ScriptManager.Game.current.canvas_visibility["pause_portraits"] = new_val;

		for (int i=0; i < 4; i++)
		{
			if (new_val)
			{
				ScriptManager.Character this_player = ScriptManager.Game.current.players[i];
				if (ScriptManager.Game.current.player_names.Contains(this_player.name))
				{
					pause_portraits[i].GetComponent<Image>().sprite = Resources.Load (ScriptManager.Game.current.get_file_path("portraits/" + this_player.portraits["normal"]), typeof(Sprite)) as Sprite;
					if (this_player.active)
					{
						pause_portraits[i].GetComponent<CursorBehavior>().set_image_alpha_target(1f);
					}
					else
					{
						pause_portraits[i].GetComponent<CursorBehavior>().set_image_alpha_target(0.5f);
					}
				}
			}
			else
			{
				pause_portraits[i].GetComponent<CursorBehavior>().set_image_alpha_target(0f);
			}
		}
	}

	public void show_pause_stats ()
	{
		for (int i=1;i<=4;i++)
		{
			ScriptManager.Character this_player = ScriptManager.Game.current.players[i-1];
			if (this_player.active)
			{
				update_player_ui(i, this_player.name, ScriptManager.Game.current.pause_player_stats);
			}
		}
		update_game_stat_ui();
		set_ui_font();
		showing_pause_stats = true;
	}

	public void hide_player_portrait (string pos = "PL")
	{
		ScriptManager.Game.current.canvas_visibility["player_portraits"] = false;

		Sprite new_img = Resources.Load (ScriptManager.Game.current.get_file_path("portraits/transparent"), typeof(Sprite)) as Sprite;
		GameObject target_object;
		Vector3 new_pos = new Vector3();
		
		switch (pos)
		{
		case "PL":
			target_object = player_portrait_L;
			new_pos = PL_hide;
			break;
		case "PR":
			target_object = player_portrait_R;
			new_pos = PR_hide;
			break;
		default:
			target_object = player_portrait_L;
			new_pos = PL_hide;
			break;
		}
		StartCoroutine(Switch_images(target_object.GetComponent<Image>(), new_img, message_transition_time));
		target_object.GetComponent<CursorBehavior>().set_translation_target(new_pos);

	}

	public void set_background (string bg_name, bool clear_stuff = true)
	{
		ScriptManager.Game.current.canvas_visibility["bg"] = bg_name == "transparent";
		if (clear_stuff)
		{
			clear_all_speakers();
			hide_speech_box();
		}
		check_for_anim(bg_name);
		StartCoroutine(Switch_images(bg_image, Resources.Load (ScriptManager.Game.current.get_file_path("backgrounds/" + bg_name), typeof(Sprite)) as Sprite, message_transition_time, 1, bg_name));
		//bg_image.sprite = Resources.Load ("backgrounds/" + bg_name, typeof(Sprite)) as Sprite;
	}

	public Color color_scale (Color start_color, float scale)
	{
		return new Color(start_color.r * scale, start_color.g * scale, start_color.b * scale);
	}

	public void effect_flash (string color_name = "white", float duration = 0.6f, float attack = 0.5f)
	{
		Color flash_color = script_manager.find_color(color_name);
		/*if (valid_colors.ContainsKey (color_name))
		{
			flash_color = valid_colors[color_name];
		}
		else
		{
			flash_color = Color.white;
		}*/
		StartCoroutine(Switch_images(fg_image, Resources.Load (ScriptManager.Game.current.get_file_path("foregrounds/white_flash"), typeof(Sprite)) as Sprite, duration * attack));
		fg_image.GetComponent<CursorBehavior>().set_image_color_target(flash_color, 1f);
		StartCoroutine(Wait_to_switch_images(fg_image, Resources.Load (ScriptManager.Game.current.get_file_path("backgrounds/transparent"), typeof(Sprite)) as Sprite, duration * attack, duration * (1f - attack)));
	}

	public void effect_shake (float shake_intensity = 30f, float decay = 0.95f)
	{
		StartCoroutine(Shake_thing(bg_image.gameObject, shake_intensity, decay));
		StartCoroutine(Shake_thing(portrait_C_image.gameObject, shake_intensity, decay));
		StartCoroutine(Shake_thing(portrait_L_image.gameObject, shake_intensity, decay));
		StartCoroutine(Shake_thing(portrait_R_image.gameObject, shake_intensity, decay));
	}

	public void onNewPlayerChoosingCharacter (int new_player_i)
	{
		players_are_choosing_characters = true;
	}

	public void onPlayersDoneChoosingCharacters ()
	{
		players_are_choosing_characters = false;
	}

	IEnumerator Wait_to_set_translation_target (GameObject target, Vector2 translation_target, float delay)
	{
		float timer = 0f;
		while (timer < delay)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;

			}
			yield return null;
		}
		target.GetComponent<CursorBehavior>().set_translation_target(translation_target);
	}

	IEnumerator Wait_to_switch_images (Image target_img, Sprite target_sprite, float delay, float fade_in_time = 0.5f, string anim_check = "")
	{
		float timer = 0f;
		while (timer < delay)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}
		StartCoroutine(Switch_images(target_img, target_sprite, fade_in_time, 1, anim_check));
	}

	IEnumerator Wait_to_fade (GameObject target, float target_alpha, float delay)
	{
		float timer = 0f;
		while (timer < delay)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}

		target.GetComponent<CursorBehavior>().set_image_alpha_target(0f,0f,message_transition_time);
		target.GetComponent<CursorBehavior>().set_text_alpha_target(0f);
		/*StartCoroutine(Fade_to(target.GetComponent<Image>(), target_alpha, message_transition_time));
		foreach (Text blurb in target.GetComponentsInChildren<Text>())
		{
			Fade_text_to(blurb, target_alpha, message_transition_time);
		}*/
	}

	IEnumerator Wait_to_decrement_p_alert (int player, float delay)
	{
		float timer = 0f;
		while (timer < delay)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}
		
		ScriptManager.Game.current.player_alerts_showing[player - 1] -= 1;
	}

	IEnumerator Shake_thing (GameObject thing, float amount, float decay)
	{
		Vector3 start_pos = thing.transform.position;
		while (amount > 0.01f)
		{
			if (ScriptManager.Game.current.running)
			{
				thing.transform.position = start_pos;
				float my_angle = Random.Range (0, 360) * Mathf.Deg2Rad;
				thing.transform.Translate (amount * new Vector3(Mathf.Cos (my_angle), Mathf.Sin (my_angle),0f));
				amount *= decay;
			}

			yield return null;
		}
		thing.transform.position = start_pos;

	}

	IEnumerator Wait_to_destroy (GameObject target, float delay)
	{
		float timer = 0f;
		while (timer < delay)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}
		Destroy(target);
	}

	IEnumerator Wait_to_show_player_portrait (float delay)
	{
		float timer = 0f;
		while (timer < delay)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}
		show_player_portrait(player_speaker_portrait, player_speaker_pos);
	}

	IEnumerator Wait_to_pick (int choice_index, float delay)
	{
		float timer = 0f;
		while (timer < delay)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}

		if (choice_index >= ScriptManager.Game.current.current_choices.Count)
		{
			script_manager.pick(ScriptManager.Game.current.current_decision.fail_choices[choice_index - ScriptManager.Game.current.current_choices.Count]);
		}
		else
		{
			script_manager.pick(ScriptManager.Game.current.current_choices[choice_index]);
		}
	}

	IEnumerator Fade_to(Image image, float alpha, float duration)
	{
		Color old_color = image.color;
		float timer = 0f;
		while (timer < duration)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				image.color = new Color(old_color.r, old_color.g, old_color.b, old_color.a + timer / duration * (alpha - old_color.a));

			}
			yield return null;
		}
		image.color = new Color(old_color.r, old_color.g, old_color.b, alpha);
	}

	IEnumerator Brightness_shift(Image image, float color_percent, float duration)
	{
		Color old_color = image.color;
		float timer = 0f;
		while (timer < duration)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				image.color = new Color(old_color.r * (1 + (color_percent - 1) * timer / duration), old_color.g * (1 + (color_percent - 1) * timer / duration), old_color.b * (1 + (color_percent - 1) * timer / duration));

			}
			yield return null;
		}
		image.color = new Color(old_color.r * color_percent, old_color.g * color_percent, old_color.b * color_percent);
	}

	IEnumerator Fade_text_to(Text text, float alpha, float duration)
	{
		Color old_color = text.color;
		float timer = 0f;
		while (timer < duration)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				text.color = new Color(old_color.r, old_color.g, old_color.b, old_color.a + timer / duration * (alpha - old_color.a));

			}
			yield return null;
		}
		text.color = new Color(old_color.r, old_color.g, old_color.b, alpha);
	}

	IEnumerator Slide_object (GameObject target_object, Vector3 new_pos, float duration)
	{
		float timer = 0f;
		Vector3 start_pos = target_object.transform.position;

		while (timer < duration)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				target_object.transform.position = Vector3.Lerp (start_pos, new_pos, timer / duration);

			}
			yield return null;
		}
	}

	IEnumerator Switch_images (Image from_image, Sprite to_image, float duration, float final_alpha = 1f, string anim_name_check = "")
	{
		float timer = 0f;
		//StartCoroutine(Fade_to(from_image, 0f, duration / 2f));
		from_image.gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(0f);

		while (timer < duration / 2f)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}

		if (anim_name_check != "" && anim_dict.ContainsKey(anim_name_check))
		{
			anim_dict[anim_name_check].apply_to(from_image.gameObject);
			from_image.gameObject.GetComponent<SpriteAnimator>().play();

		}
		else
		{
			SpriteAnimator test_animator = from_image.gameObject.GetComponent<SpriteAnimator>();
			if (test_animator != null)
			{
				test_animator.stop();
			}
			from_image.sprite = to_image;
		}
		//from_image.color = new Color(from_image.color.r, from_image.color.g, from_image.color.b, 0f);
		//StartCoroutine(Fade_to(from_image, 1f, duration / 2f));
		from_image.gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(final_alpha);

	}

	IEnumerator Switch_image_color (Image from_image, Color to_color, float duration)
	{
		float timer = 0f;
		//StartCoroutine(Fade_to(from_image, 0f, duration / 2f));
		from_image.gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(0f);

		
		while (timer < duration / 2f)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}
		
		from_image.color = to_color;
		from_image.color = new Color(from_image.color.r, from_image.color.g, from_image.color.b, 0f);
		//StartCoroutine(Fade_to(from_image, 1f, duration / 2f));
		from_image.gameObject.GetComponent<CursorBehavior>().set_image_alpha_target(to_color.a);

	}

	IEnumerator Teleport (Image subject, Vector2 new_pos, float duration)
	{
		float timer = 0f;
		subject.GetComponent<CursorBehavior>().set_image_alpha_target(0f, 0.5f);
		subject.GetComponent<CursorBehavior>().set_text_alpha_target(0f, 0.5f);
		float prev_alpha = subject.color.a;

		while (timer < duration / 2f)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}

		subject.transform.position = new Vector3(new_pos.x, new_pos.y);
		subject.GetComponent<CursorBehavior>().set_translation_target(new_pos);
		subject.GetComponent<CursorBehavior>().set_image_alpha_target(prev_alpha);
		subject.GetComponent<CursorBehavior>().set_text_alpha_target(1f);
	}

	IEnumerator Switch_text (Text from_text, string next_message, string alignment, float duration, Color new_color)
	{
		float timer = 0f;
		//StartCoroutine(Fade_text_to(from_text, 0f, duration / 2f));
		from_text.transform.parent.gameObject.GetComponent<CursorBehavior>().set_text_alpha_target(0f);

		while (timer < duration / 2f)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}

		from_text.text = next_message;

		switch (color_mode)
		{
		case "nar":
			if (from_text.transform.parent.GetComponent<CursorBehavior>() != null)
			{
				from_text.transform.parent.GetComponent<CursorBehavior>().set_text_color_target(narrator_text_color);
			}
			else 
			{
				from_text.color = new Color(narrator_text_color.r, narrator_text_color.g, narrator_text_color.b, from_text.color.a);

			}
			from_text.alignment = TextAnchor.UpperCenter;
			from_text.fontStyle = narration_style;
			//StartCoroutine(Fade_to(blabber_name_plate.GetComponent<Image>(), 0f, 0.4f));
			blabber_name_plate.GetComponent<CursorBehavior>().set_image_alpha_target(0f);
			//StartCoroutine(Fade_text_to(blabber_name, 0f, 0.4f));
			blabber_name_plate.gameObject.GetComponent<CursorBehavior>().set_text_alpha_target(0f);
			set_nar_font();
			break;
		case "char":
			if (from_text.transform.parent.GetComponent<CursorBehavior>() != null)
			{
				from_text.transform.parent.GetComponent<CursorBehavior>().set_text_color_target(new_color);
			}
			else 
			{
				from_text.color = new Color(new_color.r, new_color.g, new_color.b, from_text.color.a);

			}
			//StartCoroutine(Fade_to(blabber_name_plate.GetComponent<Image>(), 1f, 0.4f));
			blabber_name_plate.GetComponent<CursorBehavior>().set_image_alpha_target(1f);
			blabber_name_plate.gameObject.GetComponent<CursorBehavior>().set_text_alpha_target(1f);
			
			from_text.fontStyle = FontStyle.Normal;
			
			switch (alignment)
			{
			case "L":
				from_text.alignment = TextAnchor.UpperLeft;
				break;
			case "C":
				from_text.alignment = TextAnchor.UpperCenter;
				break;
			case "R":
				from_text.alignment = TextAnchor.UpperRight;
				break;
			}
			if (use_type_in && typein_signal)
			{
				typein_signal = false;
				begin_typein();
			}
			set_char_font();

			break;
		}

		//StartCoroutine(Fade_text_to(from_text, 1f, duration / 2f));
		from_text.transform.parent.gameObject.GetComponent<CursorBehavior>().set_text_alpha_target(1f);

		
	}

	public void finish_typein()
	{
		typing_in = false;
		characters_typed = my_line_text.Length;
		blabber_text.text = my_line_text;
		show_approvals();
	}

	public void update_dialogue_box()
	{
		string open_tag_string = "";
		string close_tag_string = "";
		foreach (string tag in open_tags)
		{
			if (tag.StartsWith("color"))
			{
				open_tag_string += "<color=#0000>";

				close_tag_string = close_tag_string.Insert(0, "</color>");
			}
			else
			{
				open_tag_string += "<" + tag + ">";

				close_tag_string = close_tag_string.Insert(0, "</" + tag + ">");
			}
		}

		string first_string = my_line_text.Substring (0, Mathf.FloorToInt(characters_typed));
		string second_string = supress_color_tags(my_line_text.Substring (Mathf.FloorToInt(characters_typed)));
		if (first_string.EndsWith ("<"))
		{
			blabber_text.text = my_line_text.Substring (0, Mathf.FloorToInt(characters_typed) - 1) + close_tag_string + "<color=#0000>" + open_tag_string + supress_color_tags(my_line_text.Substring (Mathf.FloorToInt(characters_typed - 1))) + "</color>";
		}
		else
		{
			blabber_text.text = first_string + close_tag_string + "<color=#0000>" + open_tag_string + second_string + "</color>";
		}
	}

	public string supress_color_tags(string phrase)
	{
		string output_string = phrase;
		string pattern = @"(^.*<color=)([a-z]+|#[\da-z]+)(>.*)";
		string pattern2 = @"(^.*<color=)( ___ )(>.*)";
		while (Regex.IsMatch (output_string, pattern))
		{
			output_string = Regex.Replace (output_string, pattern, "$1 ___ $3");
		}
		while (Regex.IsMatch (output_string, pattern2))
		{
			output_string = Regex.Replace (output_string, pattern2, "$1#0000$3");
		}
		return output_string;
	}

	public void begin_typein()
	{
		typing_in = true;
		//my_line_text = script_manager.plug_in_variables( ScriptManager.Game.current.get_current_line().text);
		characters_typed = 0f;
		typein_pause_timer = 0f;
		latest_sound_index = 0;
		open_tags.Clear();
	}

	public void set_typein_pitch (float new_val)
	{
		typein_emitter.pitch = new_val;
		play_typein_noise = true;
	}

	public void set_ui_font (string font_name = "")
	{
		if (font_name == "")
		{
			if (ScriptManager.Game.current.font_ui != "")
			{
				font_name = ScriptManager.Game.current.font_ui;
			}
		}
		Font new_font = find_font(font_name);

		foreach (GameObject player_gui in player_guis)
		{
			Transform name_t = player_gui.transform.GetChild (0);
			Text name_text = name_t.GetComponent<Text>();
			name_text.font = new_font;
			name_t.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, name_text.mainTexture);

			foreach (Transform label_t in player_gui.transform)
			{
				foreach (Text child_text in label_t.GetComponentsInChildren<Text>())
				{
					child_text.font = new_font;
					child_text.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, child_text.mainTexture);
				}
			}
		}
		ScriptManager.Game.current.font_ui = font_name;
		font_ui = font_name;
	}

	public void set_tutorial_font (string font_name = "")
	{
		if (font_name == "")
		{
			if (ScriptManager.Game.current.font_ui != "")
			{
				font_name = ScriptManager.Game.current.font_ui;
			}
		}

		Transform tut_t = tutorial_box.transform.GetChild (0);
		Text tut_text = tut_t.GetComponent<Text>();
		Font new_font = find_font(font_name);

		tut_text.font = new_font;
		tut_t.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, tut_text.mainTexture);

		Transform password_label_t = password_label.transform.GetChild (0);
		Text password_label_text = password_label.GetComponentInChildren<Text>();
		password_label_text.font = new_font;
		password_label_t.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, password_label_text.mainTexture);

		ScriptManager.Game.current.font_tutorial = font_name;
		font_tutorial = font_name;

	}

	public void set_choice_font (string font_name = "")
	{
		if (font_name == "")
		{
			if (ScriptManager.Game.current.font_ui != "")
			{
				font_name = ScriptManager.Game.current.font_ui;
			}
		}
		Font new_font = find_font(font_name);

		foreach (Transform column_t in choice_box.transform)
		{
			foreach (Transform option_t in column_t)
			{
				foreach (Text child_text in option_t.GetChild (0).GetComponentsInChildren<Text>())
				{

					child_text.font = new_font;
					child_text.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, child_text.mainTexture);

				}
			}
		}
		foreach (Transform choice_panel_t in secret_choice_box.transform)
		{
			Text child_text = choice_panel_t.GetChild (0).GetComponent<Text>();
			child_text.font = new_font;
			child_text.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, child_text.mainTexture);
		}
		ScriptManager.Game.current.font_choice = font_name;
		font_choice = font_name;

	}

	public void set_fail_font (string font_name = "")
	{
		if (font_name == "")
		{
			if (ScriptManager.Game.current.font_ui != "")
			{
				font_name = ScriptManager.Game.current.font_ui;
			}
		}

		ScriptManager.Game.current.font_fail = font_name;
		font_fail = font_name;

	}

	public void set_nar_font (string font_name = "")
	{
		if (font_name == "")
		{
			if (ScriptManager.Game.current.font_ui != "")
			{
				font_name = ScriptManager.Game.current.font_nar;
			}
		}
		Font new_font = find_font(font_name);

		if (color_mode == "nar")
		{
			blabber_text.font = new_font;
			blabber_text.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, blabber_text.mainTexture);

		}
		ScriptManager.Game.current.font_nar = font_name;
		font_nar = font_name;

	}

	public void set_char_font (string font_name = "")
	{
		if (font_name == "")
		{
			if (ScriptManager.Game.current.font_ui != "")
			{
				font_name = ScriptManager.Game.current.font_ui;
			}
		}
		Font new_font = find_font(font_name);

		if (color_mode == "char")
		{
			blabber_text.font = new_font;
			blabber_text.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, blabber_text.mainTexture);
		}
		blabber_name.font = new_font;
		blabber_name.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, blabber_name.mainTexture);

		Text interrupt_box_text = interrupt_box.GetComponentInChildren<Text>();
		interrupt_box_text.font = new_font;
		interrupt_box_text.GetComponent<CanvasRenderer>().SetMaterial(new_font.material, interrupt_box_text.mainTexture);

		ScriptManager.Game.current.font_char = font_name;
		font_char = font_name;

	}

	public void set_font (string font_name = "")
	{
		set_ui_font (font_name);
		set_tutorial_font (font_name);
		set_choice_font (font_name);
		set_fail_font (font_name);
		set_nar_font (font_name);
		set_char_font (font_name);

	}

	public void update_fonts ()
	{
		set_ui_font ();
		set_char_font();
		set_choice_font();
		set_nar_font();
		set_tutorial_font();
		set_fail_font();
	}

	public void handle_typein()
	{
		if (typein_pause_timer > 0f)
		{

			typein_pause_timer -= Time.deltaTime;
		}
		else
		{
			characters_typed += Time.deltaTime * characters_per_second;

			int latest_character_index = Mathf.FloorToInt(characters_typed);
			if (latest_character_index / char_per_typein_noise > latest_sound_index)
			{
				latest_sound_index = latest_character_index / char_per_typein_noise;
				if (play_typein_noise)
				{
					typein_emitter.clip = sound_dict["type in noise"];
					typein_emitter.Play ();
				}
			}

			if (latest_character_index > 0)
			{
				string added_character = my_line_text.Substring(latest_character_index - 1,1);
				
				if (added_character == "<" && my_line_text.Substring (latest_character_index).Contains (">"))
				{
					int starting_index = latest_character_index;
					while (added_character != ">")
					{
						characters_typed += 1f;
						latest_character_index += 1;
						added_character = my_line_text.Substring(latest_character_index - 1,1);
						
					}
					int ending_index = latest_character_index;

					string tag_name = my_line_text.Substring(starting_index, ending_index - starting_index - 1);

					if (tag_name.StartsWith("/"))
					{
						open_tags.RemoveAt(open_tags.Count - 1);
					}
					else
					{
						open_tags.Add (tag_name);
					}

					characters_typed += 1;
				}
				if (sentence_endings.Contains(added_character))
				{
					typein_pause_timer = typein_pause;
				}
				else if (sentence_pauses.Contains (added_character))
				{
					typein_pause_timer = typein_pause / 2f;
				}
			}
			
			if (characters_typed >= my_line_text.Length)
			{
				finish_typein();
			}
			else
			{
				update_dialogue_box();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (ScriptManager.Game.current.running)
		{
			if (show_cursor_flag)
			{
				show_cursor_flag = false;
				show_cursors();
			}
			
			if (populate_dialogue_flag)
			{
				populate_dialogue_flag = false;
				populate_dialogue_options();
			}

			if (bg_portrait_setup_flag)
			{
				bg_portrait_setup_flag = false;
				for (int i = 0; i < ScriptManager.Game.current.bg_actors_left.Count; i++)
				{
					set_portrait("BGL", script_manager.plug_in_portrait_name(ScriptManager.Game.current.bg_actors_left[i], ScriptManager.Game.current.bg_poses_left[i]));
				}
				for (int i = 0; i < ScriptManager.Game.current.bg_actors_right.Count; i++)
				{
					set_portrait("BGR", script_manager.plug_in_portrait_name(ScriptManager.Game.current.bg_actors_right[i], ScriptManager.Game.current.bg_poses_right[i]));
				}
			}

			if (line_up_bg_char_flag)
			{
				line_up_bg_char_flag = false;
				line_up_bg_portraits();
			}
			
			if (typing_in)
			{
				handle_typein();
			}
			
			if (timer_running)
			{
				fail_timer -= Time.deltaTime;
				if (fail_timer <= 0f)
				{
					List<string> wait_modes = new List<string>{"spinner", "hidden spinner", "lottery"};
					List<string> first_option_default_modes = new List<string>{"poll", "hidden poll"};
					List<string> allocate_modes = new List<string>{"shop", "lottery"};
					
					if (wait_modes.Contains (choice_mode))
					{
						script_manager.done_choosing();
					}
					
					if (allocate_modes.Contains (choice_mode))
					{
						commit_allocations();
					}
					
					else if (first_option_default_modes.Contains (choice_mode))
					{
						for (int i=0; i < player_choices.Count; i++)
						{
							if (player_choices[i] == 0)
							{
								player_choices[i] = 1;
							}
						}
						commit_choices();
					}
					
					else if (ScriptManager.Game.current.current_decision.fail_choices.Count >= 1)
					{
						timer_running = false;
						hide_choice_box();
						hide_secret_choice_box();
						hide_cursors();
						hide_tutorial_box();
						script_manager.pick (ScriptManager.Game.current.current_decision.fail_choices[0]);
					}
					
					else
					{
						if (player_choices.FindAll(item => item > 0).Count == 0)
						{
							player_choices[0] = 1; // Will default to first option if timer is up and no player has voted
						}
						commit_choices();
					}
				}
				else
				{
					string timer_text = fail_timer.ToString();
					if (timer_text.Length > 4)
					{
						timer_text = timer_text.Substring (0,4);
					}
					show_tutorial_box(tutorial_message + " (" + timer_text + ")", false);
				}
			}
			else if (script_manager.time_everything && timing_dialogue && fail_timer > 0f && player_count > 0 && !players_are_choosing_characters)
			{
				
				fail_timer -= Time.deltaTime;
				auto_progress_slider.GetComponent<Image>().fillAmount = 1f - fail_timer / auto_progress_duration;
				if (fail_timer <= 0f)
				{
					timing_dialogue = false;
					script_manager.progress ();
				}
			}
		}

	}
}
