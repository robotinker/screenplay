using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
//using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

public class ScriptManager : MonoBehaviour {
	
	DialogueManager dialogue_manager;
	CutsceneManager cutscene_manager;
	
	Regex is_player_stat = new Regex(@"^\$player(\d+)\.(.+)");
	Regex fun_2_int_var = new Regex(@"^.*\((.+),(.+)\)");

	//public List<bool> active_players = new List<bool>();
	public List<Color> player_colors;
	List<int> p_char_i = new List<int>();
	public int player_count_min = 1; // how many players are needed by this story
	public int player_count_max = 4; // how many players are supported by this story

	public string save_mode = "overwrite";
	public bool show_plot_summaries = true;
	public bool pause_all_lines = false;
	public bool time_everything = false;

	public int command_player = 0;
	public int savegame_index = 0;

	List<char> string_ender_list = new List<char>{'^','*','/','+','-','<','>','=','!','&','|'};
	List<char> variable_ender_list = new List<char>{' ',',','!','?',')'};
	List<char> num_char_list = new List<char>{'1','2','3','4','5','6','7','8','9','0','.'};
	
	List<string> command_assign_words = new List<string>{
		"settings",
		"chapter",
		"scene",
		"background",
		"music",
		"sound",
		"tutorial message",
		"font",
		"UI font",
		"character font",
		"narrator font",
		"tutorial font",
		"choice font",
		"failure font",
		"_commander",
		"decision",
		"branch",
		"cutscene",
		"load_story",
		"new_game",
		"character available",
		"character unavailable",
		"show player stats",
		"show game stats",
		"show player pause stats",
	};
	
	List<string> character_effects = new List<string>{
		"blushes",
		"squishes",
		"shakes",
		"blinks",
		"darkens",
		"throbs",
	};
	
	public delegate void script_ready ();
	public static event script_ready onScriptReady;
	
	public delegate void game_loaded ();
	public static event game_loaded onGameLoaded;
	
	public delegate void can_pause ();
	public static event can_pause onCanPause;
	
	public delegate void player_choosing (bool new_state);
	public static event player_choosing onPlayerChoosing;
	
	public delegate void pause_input (float pause_time);
	public static event pause_input onPauseInput;
	
	public delegate void set_character_selection_lock (bool new_state);
	public static event set_character_selection_lock onCharacterLock;

	public delegate void player_selecting_character (int player_i);
	public static event player_selecting_character onChooseCharacter;

	public delegate void important_line (bool new_val);
	public static event important_line onImportanceSet;
	
	public delegate void password_prompt ();
	public static event password_prompt onPromptPassword;
	
	public delegate void password_guess ();
	public static event password_guess onGuessPassword;
	
	public string password_mode = "";
	
	AudioSource SFX_emitter;
	AudioSource Music_emitter;
	
	[System.Serializable]
	
	public class Game
	{
		public static Game current;
		public string story_name;
		public string save_mode = "overwrite";
		
		public string chapter_name = "Once upon a time...";
		
		public Dictionary<string, Character> char_dict = new Dictionary<string, Character>();
		public List<string> cap_chars = new List<string>();
		public List<string> p_chars_available = new List<string>();
		public List<Character> players = new List<Character>();
		public List<string> player_names = new List<string>();
		public List<int> player_alerts_showing = new List<int>{0,0,0,0};
		
		public bool can_select_character = true;
		
		public Dictionary<string, Stat> stat_dict = new Dictionary<string, Stat>();
		public List<Stat> default_stats = new List<Stat>();
		public List<string> default_stats_to_show = new List<string>();

		public List<string> cap_stats = new List<string>();
		public List<string> char_select_stats = new List<string>();
		public List<string> pause_stats = new List<string>();
		public Dictionary<string,string> pause_stat_aliases = new Dictionary<string, string>();
		public List<string> pause_player_stats = new List<string>();
		public Dictionary<string,string> pause_player_stat_aliases = new Dictionary<string, string>();

		public Dictionary<string, Decision> decision_dict = new Dictionary<string, Decision>();
		public Decision current_decision;
		
		public Dictionary<string, Choice> choice_dict = new Dictionary<string, Choice>();
		public List<Choice> current_choices = new List<Choice>(); // stores qualifying choices of Game.current.current_decision
		
		public bool interrupting;
		public Choice interrupt_choice;
		
		public List<Line> script = new List<Line>();
		public int script_pos = -1;
		public string current_speaker_name = "";
		public string current_speaker_pos = "C";
		
		public Dictionary<string, CutsceneManager.CutScene> cutscene_dict = new Dictionary<string, CutsceneManager.CutScene>();
		
		public string music_playing = "";
		public string background_showing = "transparent";
		public string tutorial_message_showing = "";
		public bool game_stats_showing = false;
		public List<string> stage_actors = new List<string>{"","",""};
		public List<string> stage_poses = new List<string>{"normal","normal","normal"};
		public List<string> bg_actors_left = new List<string>();
		public List<string> bg_actors_right = new List<string>();
		public List<string> bg_poses_left = new List<string>();
		public List<string> bg_poses_right = new List<string>();

		public List<Line> plot_summary = new List<Line>();
		public int ID = Random.Range (0, 1000);
		
		public bool show_plot_summaries = true;
		public bool pause_all_lines = false;
		
		public string font_nar = "";
		public string font_char = "";
		public string font_ui = "";
		public string font_tutorial = "";
		public string font_choice = "";
		public string font_fail = "";
		
		public bool running = true;
		
		public Dictionary<string, bool> canvas_visibility = new Dictionary<string, bool>{
			{"uis", false},
			{"speech_box", false},
			{"player_portraits", false},
			{"approvals", false},
			{"pause_portraits", false},
			{"tutorial_box", false},
			{"secret_choice_box", false},
			{"choice_box", false},
			{"bg", false},
			{"password_box", false},
		};
		
		public string password = "";
		
		public Game (string my_name)
		{
			story_name = my_name;
			
			Stat this_player = new Stat("Player Index", "_this_player", "int", 1);
			stat_dict["_this_player"] = this_player;
			
			stat_dict["_player_count"] = new Stat("Player Count", "_player_count", "int");

			initialize_decision_dict();
		}

		public void grab_pause_stat_and_alias (string my_stat_string)
		{
			if (my_stat_string.Contains("(") && my_stat_string.Contains(")"))
			{
				string stat_string = my_stat_string.Substring(0,my_stat_string.IndexOf("(")).Trim();
				int start_index = my_stat_string.IndexOf("(") + 1;
				int end_index = my_stat_string.IndexOf(")");
				string alias_string = my_stat_string.Substring(start_index, end_index - start_index);
				Debug.Log("logging stat: " + stat_string + " with alias: " + alias_string);
				if (stat_dict.ContainsKey(stat_string))
				{
					pause_stats.Add(stat_string);
					pause_stat_aliases.Add(stat_string, alias_string);
				}
			}
			else if (stat_dict.ContainsKey(my_stat_string))
			{
				pause_stats.Add(my_stat_string);
			}
		}

		public void grab_player_pause_stat_and_alias (string my_stat_string)
		{
			if (my_stat_string.Contains("(") && my_stat_string.Contains(")"))
			{
				string stat_string = my_stat_string.Substring(0,my_stat_string.IndexOf("(")).Trim();
				int start_index = my_stat_string.IndexOf("(") + 1;
				int end_index = my_stat_string.IndexOf(")");
				string alias_string = my_stat_string.Substring(start_index, end_index - start_index);
				Debug.Log("logging stat: " + stat_string + " with alias: " + alias_string);

				if (has_default_player_stat(stat_string) || (players.Count > 0 && players[stat_dict["_this_player"].int_val].stat_dict.ContainsKey(stat_string)))
				{
					pause_player_stats.Add(stat_string);
					pause_player_stat_aliases.Add(stat_string, alias_string);
				}
			}
			else if (has_default_player_stat(my_stat_string) || (players.Count > 0 && players[stat_dict["_this_player"].int_val].stat_dict.ContainsKey(my_stat_string)))
			{
				pause_player_stats.Add(my_stat_string);
			}
		}

		public void grab_player_default_stat_and_alias (string my_stat_string)
		{
			Debug.Log("Received stat string: " + my_stat_string);
			if (my_stat_string.Contains("(") && my_stat_string.Contains(")"))
			{
				string stat_string = my_stat_string.Substring(0,my_stat_string.IndexOf("(")).Trim();
				int start_index = my_stat_string.IndexOf("(") + 1;
				int end_index = my_stat_string.IndexOf(")");
				string alias_string = my_stat_string.Substring(start_index, end_index - start_index);

				if (has_default_player_stat(stat_string) || (players.Count > 0 && players[stat_dict["_this_player"].int_val].stat_dict.ContainsKey(stat_string)))
				{
					default_stats_to_show.Add(stat_string);
					pause_player_stat_aliases.Add(stat_string, alias_string);
				}
			}
			else if (has_default_player_stat(my_stat_string) || (players.Count > 0 && players[stat_dict["_this_player"].int_val].stat_dict.ContainsKey(my_stat_string)))
			{
				default_stats_to_show.Add(my_stat_string);

			}
		}

		public void grab_char_select_stat_and_alias (string my_stat_string)
		{
			if (my_stat_string.Contains("(") && my_stat_string.Contains(")"))
			{
				string stat_string = my_stat_string.Substring(0,my_stat_string.IndexOf("(")).Trim();
				int start_index = my_stat_string.IndexOf("(") + 1;
				int end_index = my_stat_string.IndexOf(")");
				string alias_string = my_stat_string.Substring(start_index, end_index - start_index);
				Debug.Log("logging stat: " + stat_string + " with alias: " + alias_string);

				if (has_default_player_stat(stat_string) || (players.Count > 0 && players[stat_dict["_this_player"].int_val].stat_dict.ContainsKey(stat_string)))
				{
					char_select_stats.Add(stat_string);
					pause_player_stat_aliases.Add(stat_string, alias_string);
				}
			}
			else if (has_default_player_stat(my_stat_string) || (players.Count > 0 && players[stat_dict["_this_player"].int_val].stat_dict.ContainsKey(my_stat_string)))
			{
				char_select_stats.Add(my_stat_string);
			}
		}
		
		public Game make_copy ()
		{
			Game copy = new Game(story_name);
			copy.background_showing = background_showing;
			copy.tutorial_message_showing = tutorial_message_showing;
			copy.can_select_character = can_select_character;
			copy.char_select_stats = char_select_stats;
			copy.choice_dict = choice_dict;
			copy.current_decision = current_decision;
			copy.current_speaker_name = current_speaker_name;
			copy.current_speaker_pos = current_speaker_pos;
			copy.default_stats = default_stats;
			copy.default_stats_to_show = default_stats_to_show;
			copy.update_cap_stats();
			copy.interrupt_choice = interrupt_choice;
			copy.interrupting = interrupting;
			copy.music_playing = music_playing;
			copy.chapter_name = chapter_name;
			copy.script_pos = script_pos;
			copy.story_name = story_name;
			copy.cutscene_dict = cutscene_dict;
			copy.pause_stats = pause_stats;
			copy.pause_stat_aliases = pause_stat_aliases;
			copy.game_stats_showing = game_stats_showing;
			copy.pause_player_stats = pause_player_stats;
			copy.pause_player_stat_aliases = pause_player_stat_aliases;
			copy.pause_all_lines = pause_all_lines;
			copy.show_plot_summaries = show_plot_summaries;
			
			copy.font_ui = font_ui;
			copy.font_tutorial = font_tutorial;
			copy.font_char = font_char;
			copy.font_nar = font_nar;
			copy.font_choice = font_choice;
			copy.font_fail = font_fail;
			
			copy.password = password;
			copy.running = running;
			
			foreach (string name in p_chars_available)
			{
				copy.p_chars_available.Add (name);
			}
			foreach (string name in player_names)
			{
				copy.player_names.Add (name);
			}
			foreach (string stat_key in stat_dict.Keys)
			{
				if (copy.stat_dict.ContainsKey(stat_key))
				{
					copy.stat_dict[stat_key] = stat_dict[stat_key].make_copy ();
				}
				else
				{
					copy.stat_dict.Add (stat_key, stat_dict[stat_key].make_copy());
				}
			}
			foreach (Character dude in players)
			{
				copy.players.Add (dude.make_copy ());
			}
			foreach (Line blurb in plot_summary)
			{
				copy.plot_summary.Add (blurb);
			}
			foreach (string decision_key in decision_dict.Keys)
			{
				if (copy.decision_dict.ContainsKey (decision_key))
				{
					copy.decision_dict[decision_key] = decision_dict[decision_key].make_copy();
				}
				else
				{
					copy.decision_dict.Add (decision_key, decision_dict[decision_key].make_copy());
				}
			}
			foreach (Choice this_choice in current_choices)
			{
				copy.current_choices.Add (this_choice);
			}
			foreach (string rando_key in char_dict.Keys)
			{
				copy.char_dict.Add (rando_key, char_dict[rando_key].make_copy());
			}
			copy.update_cap_chars();
			
			foreach (Line this_line in script)
			{
				copy.script.Add (this_line);
			}
			
			copy.stage_poses.Clear();
			copy.stage_actors.Clear ();
			
			for (int i=0; i< 3; i++)
			{
				copy.stage_poses.Add (stage_poses[i]);
				copy.stage_actors.Add (stage_actors[i]);
			}

			copy.bg_poses_left.Clear();
			copy.bg_actors_left.Clear();
			for (int i=0; i< bg_actors_left.Count; i++)
			{
				copy.bg_actors_left.Add (bg_actors_left[i]);
				copy.bg_poses_left.Add (bg_poses_left[i]);
			}

			copy.bg_poses_right.Clear();
			copy.bg_actors_right.Clear();
			for (int i=0; i< bg_actors_right.Count; i++)
			{
				copy.bg_actors_right.Add (bg_actors_right[i]);
				copy.bg_poses_right.Add (bg_poses_right[i]);
			}

			copy.ID = ID;
			
			return copy;
		}
		
		public bool is_anim (string anim_name)
		{
			return GameObject.Find ("ScriptHolder").GetComponent<DialogueManager>().anim_dict.ContainsKey (anim_name);
		}
		
		public Stat get_default_player_stat (string stat_name)
		{
			foreach (Stat this_stat in default_stats)
			{
				if (this_stat.text == stat_name)
				{
					return this_stat;
				}
			}
			return null;
		}
		
		public bool default_player_stat_is_int (string stat_name)
		{
			Stat my_stat = get_default_player_stat(stat_name);
			return my_stat.var_type == "int";
		}
		
		public bool has_default_player_stat (string stat_name)
		{
			Stat my_stat = get_default_player_stat(stat_name);
			return my_stat != null;		
		}
		
		public DialogueManager.SpriteAnimation get_anim (string anim_name)
		{
			return GameObject.Find ("ScriptHolder").GetComponent<DialogueManager>().anim_dict[anim_name];
			
		}

		public void update_cap_stats ()
		{
			cap_stats.Clear ();
			foreach (Stat this_stat in default_stats)
			{
				cap_stats.Add (this_stat.text.ToUpper ());
			}
		}

		public void update_cap_chars () 
		{
			cap_chars.Clear ();
			foreach (string name in char_dict.Keys)
			{
				cap_chars.Add (name.ToUpper ());
			}
		}

		public string get_stat_name_from_caps (string caps)
		{
			string output = "";
			foreach (Stat this_stat in default_stats)
			{
				string normal_name = this_stat.text;
				if (normal_name.ToUpper () == caps)
				{
					output = normal_name;
				}
			}
			if (output == "")
			{
				output = "NAME NOT FOUND";
				Debug.Log("Stat name (" + caps + ") not found in stats file");
			}
			return output;
		}

		public string get_char_name_from_caps (string caps)
		{
			string output = "";
			foreach (string normal_name in char_dict.Keys)
			{
				if (normal_name.ToUpper () == caps)
				{
					output = normal_name;
				}
			}
			if (output == "")
			{
				output = "NAME NOT FOUND";
				Debug.Log("Speaker name (" + caps + ") not found in stats file");
			}
			return output;
		}
		
		public string get_save_label ()
		{	
			return ID.ToString () + ": " + chapter_name;
		}
		
		public int get_column_count ()
		{
			int option_count = get_choice_count ();
			int options_per_column = current_decision.options_per_column;
			int column_count = (option_count - 1) / options_per_column + 1;
			
			return column_count;
		}
		
		public int get_choice_count ()
		{
			int option_count = current_choices.Count;
			int fail_choice_count = current_decision.fail_choices.Count;
			if (current_decision.choice_mode == "lottery")
			{
				option_count += fail_choice_count;
			}

			foreach (Choice option in current_decision.choices)
			{
				if (!current_choices.Contains (option) && !option.hide)
				{
					option_count += 1;
				}
			}
			return option_count;
		}
		
		public void check_for_icon_images()
		{
			foreach (Stat this_stat in current.default_stats)
			{
				this_stat.check_for_icon_sprite();
			}
			
			foreach (Character this_character in char_dict.Values)
			{
				foreach (Stat this_stat in this_character.stat_dict.Values)
				{
					this_stat.check_for_icon_sprite();
				}
			}
		}

		public void initialize_decision_dict ()
		{
			decision_dict.Clear();
			decision_dict.Add(
				"commander_choice", new Decision(new List<Choice>{
					new Choice("Player 1", new List<Line>{
						new Line("_commander = 0"), 
					}, new List<string>{"$player1.active == true",}, new List<string>()),
					new Choice("Player 2", new List<Line>{
						new Line("_commander = 1"), 
					}, new List<string>{"$player2.active == true",}, new List<string>()),
					new Choice("Player 3", new List<Line>{
						new Line("_commander = 2"), 
					}, new List<string>{"$player3.active == true",}, new List<string>()),
					new Choice("Player 4", new List<Line>{
						new Line("_commander = 3")
					}, new List<string>{"$player4.active == true",}, new List<string>()),
				}, new List<string>(), "", 0f, new List<Choice>(), new List<int>(), "vote", "", new List<int>(), new List<string>(), new List<string>()));
			decision_dict.Add(
				"new/load/back", new Decision(new List<Choice>{
					new Choice("New Game", new List<Line>{
						new Line("new_game: " + story_name), 
					}, new List<string>(), new List<string>()),
					new Choice("Continue", new List<Line>{
						new Line("load_game_list"), 
					}, new List<string>(), new List<string>()),
					new Choice("Delete", new List<Line>{
						new Line("delete_game_list"), 
					}, new List<string>(), new List<string>()),
					new Choice("Back", new List<Line>{
						new Line("settings: default"),
						new Line("decision: game_select"),
					}, new List<string>(), new List<string>()),
				}, new List<string>(), "", 0f, new List<Choice>(), new List<int>(), "first", "", new List<int>(), new List<string>(), new List<string>()));
			decision_dict.Add (
				"load_game_list", new Decision(
					new List<Choice>(), new List<string>(), "", 0f, new List<Choice>(), new List<int>(), "first", "", new List<int>(), new List<string>(), new List<string>()));
			decision_dict.Add (
				"delete_game_list", new Decision(
					new List<Choice>(), new List<string>(), "", 0f, new List<Choice>(), new List<int>(), "first", "", new List<int>(), new List<string>(), new List<string>()));
			decision_dict.Add(
				"pause_menu", new Decision(new List<Choice>{
					new Choice("Save", new List<Line>{
						new Line("save_game"), 
					}, new List<string>(), new List<string>()),
					new Choice("Load", new List<Line>{
						new Line("load_game_list"), 
					}, new List<string>(), new List<string>()),
					new Choice("Story Select", new List<Line>{
						new Line("clear"),
						new Line("clear scene"),
						new Line("music: off"),
						new Line("hide_GUIs"),
						new Line("settings: default"),
						new Line("decision: game_select"),
					}, new List<string>(), new List<string>()),
					new Choice("Back", new List<Line>{
						new Line("unpause"),
					}, new List<string>(), new List<string>()),
				}, new List<string>(), "", 0f, new List<Choice>(), new List<int>(), "vote", "", new List<int>(), new List<string>(), new List<string>()));


			List<string> blank = new List<string>();
			List<Choice> blank_choices = new List<Choice>();
			List<int> blank_int = new List<int>();

			List<Choice> game_choices = new List<Choice>();
			//string[] game_list = File.ReadAllLines(Application.dataPath + "/Resources/default/game_list.txt");
			TextAsset game_list_file = Resources.Load ("default/game_list") as TextAsset;
			string[] game_list = game_list_file.text.Split ('\n');
			foreach (string my_story in game_list)
			{
				if (my_story != "")
				{
					game_choices.Add (new Choice(my_story, new List<Line>{new Line("load_story: " + my_story)}, blank, blank));

				}
			}
			decision_dict.Add ("game_select", new Decision(game_choices, blank, "", 0f, blank_choices, blank_int, "first", "", blank_int, blank, blank));
		}

		public void update_pause_menu_options ()
		{
			if (show_plot_summaries && decision_dict["pause_menu"].choices.Count == 4)
			{
				decision_dict["pause_menu"].choices.Insert(0, 
					new Choice("Story recap", new List<Line>{
						new Line("unpause"),
						new Line("story_recap"), 
					}, new List<string>(), new List<string>())
				);
			}
		}
		
		public string get_file_path (string filename)
		{
			string domain = current.story_name;
			
			Sprite test_sprite = Resources.Load (domain + "/" + filename, typeof(Sprite)) as Sprite;
			AudioClip test_sound = Resources.Load (domain + "/" + filename, typeof(AudioClip)) as AudioClip;
			if (test_sprite != null || test_sound != null)
			{
				return domain + "/" + filename;
			}
			else
			{
				test_sprite = Resources.Load ("default/" + filename, typeof(Sprite)) as Sprite;
				test_sound = Resources.Load ("default/" + filename, typeof(AudioClip)) as AudioClip;
				
				if (test_sprite != null || test_sound != null)
				{
					return "default/" + filename;
				}
				else
				{
					Debug.Log ("FILE NOT FOUND: " + filename);
					return "";
				}
			}
			
		}
		
		public Line get_current_line ()
		{
			return script[script_pos];
		}
	}
	
	public static class SaveLoad {
		
		public static List<Game> savedGames = new List<Game>();
		
		public static void Save() {
			if (Game.current.save_mode == "overwrite")
			{
				savedGames.RemoveAll(item => item.ID == Game.current.ID);
			}
			savedGames.Add(Game.current.make_copy());
			
			write_out ();
		}
		
		public static void write_out()
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Create (Application.persistentDataPath + "/" + Game.current.story_name + "_savedGames.gd");
			bf.Serialize(file, SaveLoad.savedGames);
			file.Close();
		}
		
		public static void Load(string story_name) {
			if(File.Exists(Application.persistentDataPath + "/" + story_name + "_savedGames.gd")) {
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(Application.persistentDataPath + "/" + story_name + "_savedGames.gd", FileMode.Open);
				SaveLoad.savedGames = (List<Game>)bf.Deserialize(file);
				file.Close();
			}
			else
			{
				SaveLoad.savedGames = new List<Game>();
			}
		}
		
		public static void LoadGame (int game_index)
		{
			Game.current = savedGames[game_index];
			for (int i = 1; i <= 4; i++)
			{
				if (Game.current.players[i-1].active)
				{
					silent_drop_out_player (i);
				}
			}
		}
		
		public static bool try_password (int game_index, string password_guess)
		{
			return savedGames[game_index].password == password_guess;
		}
		
		public static void DeleteGame (int game_index)
		{
			savedGames.RemoveAt (game_index);
			write_out();
		}
		
		public static void silent_drop_out_player (int player_i)
		{
			Game.current.players[player_i-1].active = false;
			
			Game.current.stat_dict["_player_count"].int_val -= 1;
			if (Game.current.can_select_character && !Game.current.players[player_i-1].name.StartsWith ("Player"))
			{
				Game.current.p_chars_available.Add (Game.current.players[player_i - 1].name);
				
			}
		}
	}
	
	[System.Serializable]
	
	public class Line
	{
		public string text = "";
		public string speaker = "";
		public string position = "";
		public string pose = "";
		public string interrupt_message = "";
		public string interrupt_label = "";
		public string interrupt_type = "";
		public string sound = "";
		public bool bg_portrait = false;
		public bool important = false;
		public bool pause = false;
		public Dictionary<string, string> arg_dict = new Dictionary<string, string>();
		
		public Line (string my_text)
		{
			text = my_text;
			if (my_text.Contains (":"))
			{
				arg_dict.Add ("_first_key", my_text.Split (':')[0].Trim ());
				arg_dict.Add (arg_dict["_first_key"], my_text.Split (':')[1].Trim ());
				//Debug.Log ("New line with _first_key: " + arg_dict["_first_key"] + ", " + arg_dict["_first_key"] + ": " + arg_dict[arg_dict["_first_key"]]);
			}
			else
			{
				arg_dict.Add("_first_key", my_text.Split(',')[0]);
			}
			if (Game.current != null && Game.current.pause_all_lines)
			{
				pause = true;
			}
		}
		
		public Line make_copy()
		{
			Line new_line = new Line(text);
			new_line.speaker = speaker;
			new_line.position = position;
			new_line.pose = pose;
			new_line.interrupt_message = interrupt_message;
			new_line.interrupt_type = interrupt_type;
			new_line.interrupt_label = interrupt_label;
			new_line.important = important;
			new_line.sound = sound;
			new_line.bg_portrait = bg_portrait;
			foreach (string keyname in arg_dict.Keys)
			{
				new_line.arg_dict.Add (keyname, arg_dict[keyname]);
			}
			
			return new_line;
		}
	}
	
	[System.Serializable]
	
	public class Stat
	{
		public string text;
		public string slug;
		public int int_val;
		public float float_val;
		public string string_val;
		public bool bool_val;
		
		public string var_type;
		public string icon_path = "";
		
		public Stat (string my_text, string my_slug, string my_type, int my_int = 0, float my_float = 0f, string my_string = "", bool my_bool = false)
		{
			text = my_text;
			slug = my_slug;
			var_type = my_type;
			int_val = my_int;
			float_val = my_float;
			string_val = my_string;
			bool_val = my_bool;
		}
		
		public Stat make_copy ()
		{
			Stat new_stat = new Stat(text, slug, var_type, int_val, float_val, string_val, bool_val);
			new_stat.icon_path = icon_path;
			return new_stat;
		}
		
		public void check_for_icon_sprite ()
		{
			Sprite test_sprite = Resources.Load (Game.current.story_name + "/icons/resource_" + slug, typeof(Sprite)) as Sprite;
			
			if (test_sprite != null)
			{
				//Debug.Log ("Found resource icon: " + Game.current.story_name + "/icons/resource_" + slug);
				
				icon_path = Game.current.story_name + "/icons/resource_" + slug;
			}
			else
			{
				test_sprite = Resources.Load ("default/icons/resource_" + slug, typeof(Sprite)) as Sprite;
				
				if (test_sprite != null)
				{
					//Debug.Log ("Found resource icon: default/icons/resource_" + slug);
					icon_path = "default/icons/resource_" + slug;
				}
			}
		}
		
		public string get_string ()
		{
			switch (var_type)
			{
			case "string":
				return string_val;
			case "int":
				return int_val.ToString();
			case "float":
				return float_val.ToString();
			case "bool":
				if (bool_val)
				{
					return "1";
				}
				else
				{
					return "0";
				}

			default:
				return string_val;
			}
		}
		
		public string UI_type()
		{
			if (var_type == "float" && float_val <= 1f && float_val >= 0f)
			{
				if (icon_path == "")
				{
					return "TextMeter";
				}
				else
				{
					return "IconMeter";
				}
			}
			else
			{
				if (icon_path == "")
				{
					return "TextText";
				}
				else
				{
					return "IconText";
				}
			}
		}
	}
	
	[System.Serializable]
	
	public class Choice
	{
		public string text = "";
		public List<Line> line_bundle = new List<Line>();
		public List<string> conditions = new List<string>();
		public List<string> changes = new List<string>();
		public bool hide = false;
		public float[] RGBcolor = new float[3]{0f,0f,0f};
		//public Color color = Color.white;
		public bool custom_color = false;
		public string req_string = "";
		public string left_icon_name = "";
		public string right_icon_name = "";
		
		public Choice()
		{
			
		}
		
		public Choice (string my_text, List<Line> my_lines, List<string> my_cond, List<string> my_changes)
		{
			text = my_text;
			line_bundle = my_lines;
			conditions = my_cond;
			changes = my_changes;
		}
	}
	
	[System.Serializable]
	
	public class Decision
	{
		public string name = "";
		public List<Choice> choices = new List<Choice>();
		public List<Choice> fail_choices = new List<Choice>();
		public List<int> fail_bids = new List<int>();
		public List<string> fail_stat_bids = new List<string>();
		public List<string> participant_conditions = new List<string>();
		public string exclusion_text = "";
		public string choice_mode = "vote"; 
		/* branch: select first available choice, 
		 * vote, 
		 * agreement, 
		 * spinner: random result based on vote, 
		 * shop: players spend a resource to execute multiple actions, 
		 * lottery: players spend resource to vote multiple times (+spinner), 
		 * poll: execute changes per player just like allocation
		 * first: first player to press Confirm chooses
		 */
		public string allocation_currency = "money";
		public List<string> stats_to_show = new List<string>();
		public float fail_timer = 0f;
		public List<int> allocation_limits = new List<int>();
		public List<string> allocation_stat_limits = new List<string>();
		public List<int> allocation_costs = new List<int>();
		public List<string> allocation_stat_costs = new List<string>();
		public int options_per_column = 5;
		public bool randomize_order = false;
		
		public Decision ()
		{
			
		}
		
		public Decision (List<Choice> my_choices, List<string> my_reqs, string exclusion_explanation, float my_timer, List<Choice> my_fails, List<int> my_fail_bids, string my_mode, string my_currency, List<int> my_limits, List<string> my_stat_limits, List<string> my_stats_to_show)
		{
			choices = my_choices;
			participant_conditions = my_reqs; // for each one, stat_dict is consulted using "p1_" + string and is_true()
			exclusion_text = exclusion_explanation;
			
			fail_timer = my_timer;
			fail_choices = my_fails;
			fail_bids = my_fail_bids;
			choice_mode = my_mode;
			allocation_currency = my_currency;
			allocation_limits = my_limits;
			allocation_stat_limits = my_stat_limits;
			stats_to_show = my_stats_to_show;
		}
		
		public Decision make_copy ()
		{
			List<int> new_limits = new List<int>();
			foreach (int limit in allocation_limits)
			{
				new_limits.Add (limit);
			}
			
			List<int> new_costs = new List<int>();
			foreach (int cost in allocation_costs)
			{
				new_costs.Add (cost);
			}
			
			List<string> new_show_list = new List<string>();
			foreach (string stat_name in stats_to_show)
			{
				new_show_list.Add (stat_name);
			}
			
			Decision new_decision = new Decision(choices, participant_conditions, exclusion_text, fail_timer, fail_choices, fail_bids, choice_mode, allocation_currency, new_limits, allocation_stat_limits, new_show_list);
			new_decision.name = name;
			new_decision.allocation_costs = new_costs;
			new_decision.options_per_column = options_per_column;
			
			return new_decision;
		}
	}
	
	public bool has_limit_for_choice (Decision this_decision, int choice_i)
	{
		return (this_decision.allocation_limits.Count > choice_i && this_decision.allocation_limits[choice_i] > -1) || (this_decision.allocation_stat_limits.Count > choice_i && this_decision.allocation_stat_limits[choice_i] != "" && Game.current.stat_dict.ContainsKey(this_decision.allocation_stat_limits[choice_i]) && Game.current.stat_dict[this_decision.allocation_stat_limits[choice_i]].int_val > -1);
	}
	
	public int get_limit_of_choice (Decision this_decision, int choice_i) 
	{
		if (choice_i < this_decision.allocation_stat_limits.Count && this_decision.allocation_stat_limits[choice_i] != "")
		{
			if (Game.current.stat_dict.ContainsKey(this_decision.allocation_stat_limits[choice_i]))
			{
				return Game.current.stat_dict[this_decision.allocation_stat_limits[choice_i]].int_val;
			}
			else
			{
				Debug.Log ("Couldn't find stat named: " + this_decision.allocation_stat_limits[choice_i] + " for allocation limiting");
				return 0;
			}
		}
		else if (choice_i < this_decision.allocation_limits.Count)
		{
			return this_decision.allocation_limits[choice_i];
		}
		else
		{
			return 0;
		}
	}
	
	public int get_price_for_choice (Decision this_decision, int choice_i) 
	{
		if (choice_i < this_decision.allocation_stat_costs.Count && this_decision.allocation_stat_costs[choice_i] != "")
		{
			if (Game.current.stat_dict.ContainsKey(this_decision.allocation_stat_costs[choice_i]))
			{
				return Game.current.stat_dict[this_decision.allocation_stat_costs[choice_i]].int_val;
			}
			else
			{
				Debug.Log ("Couldn't find stat named: " + this_decision.allocation_stat_costs[choice_i] + " for allocation pricing");
				return 1;
			}
		}
		else if (choice_i < this_decision.allocation_costs.Count)
		{
			return this_decision.allocation_costs[choice_i];
		}
		else
		{
			return 1;
		}
	}
	
	public void add_to_limit_of_choice (Decision this_decision, int choice_i, int add_val) 
	{
		if (choice_i < this_decision.allocation_stat_limits.Count && this_decision.allocation_stat_limits[choice_i] != "")
		{
			if (Game.current.stat_dict.ContainsKey(this_decision.allocation_stat_limits[choice_i]))
			{
				Game.current.stat_dict[this_decision.allocation_stat_limits[choice_i]].int_val += add_val;
			}
			else
			{
				Debug.Log ("Couldn't find stat named: " + this_decision.allocation_stat_limits[choice_i] + " for allocation limiting");
			}
		}
		else if (choice_i < this_decision.allocation_limits.Count)
		{
			this_decision.allocation_limits[choice_i] += add_val;
		}
	}

	public void set_player_limits_from_string (string my_val)
	{
		if (my_val.Contains("-"))
		{
			string first_val = my_val.Split('-')[0].Trim();
			string second_val = my_val.Split('-')[1].Trim();
			int.TryParse(first_val, out player_count_min);
			int.TryParse(second_val, out player_count_max);
		}
		else if (int.TryParse (my_val, out player_count_min))
		{
			player_count_max = player_count_min;
		}
	}
	
	[System.Serializable]
	
	public class Character
	{
		public string name;

		public float[] RGBcolor;
		public Dictionary<string,string> portraits = new Dictionary<string, string>();
		public Dictionary<string, Stat> stat_dict = new Dictionary<string, Stat>();
		public bool active = false;
		public string greeting = "";
		public string pose_for_player = "";
		public float voice_pitch = 1f;
		
		public Character(string my_name,  Color my_color, Dictionary<string,string> my_portraits, Dictionary<string, Stat> my_stats, string my_greeting = "")
		{
			name = my_name;
			set_color (my_color);
			//portraits = my_portraits;
			//stat_dict = my_stats;
			foreach (string portrait_name in my_portraits.Keys)
			{
				portraits.Add (portrait_name, my_portraits[portrait_name]);
			}
			foreach (string stat_key in my_stats.Keys)
			{
				stat_dict.Add (stat_key, my_stats[stat_key]);
			}
			greeting = my_greeting;
		}
		
		public Character make_copy ()
		{
			Dictionary<string, Stat> stat_dict_copy = new Dictionary<string, Stat>();
			foreach (string key in stat_dict.Keys)
			{
				stat_dict_copy[key] = stat_dict[key].make_copy ();
			}
			Character new_character = new Character(name, get_color (), portraits, stat_dict_copy, greeting);
			new_character.active = active;
			return new_character;
		}
		
		public Color get_color (float amount = 1f)
		{
			if (RGBcolor == null)
			{
				return Color.gray;
			}
			else
			{
				List<float> tintRGB = new List<float>();
				foreach (float rgb_val in RGBcolor)
				{
					tintRGB.Add (1f - amount * (1f - rgb_val));
				}
				return new Color(tintRGB[0], tintRGB[1], tintRGB[2]);
			}
		}
		
		public void set_color (Color new_color)
		{
			RGBcolor = new float[3]{new_color.r, new_color.g, new_color.b};
		}
	}
	
	public void pick(Choice my_choice)
	{
		if (Game.current.script_pos < Game.current.script.Count - 1)
		{
			Game.current.script.InsertRange(Game.current.script_pos + 1, my_choice.line_bundle);
		}
		else
		{
			Game.current.script.AddRange(my_choice.line_bundle);
		}
		
		onPlayerChoosing(false);
		progress();
	}
	
	public void pause_menu ()
	{
		Game.current.script.Insert (Game.current.script_pos, new Line("decision: pause_menu"));
		Game.current.script_pos -= 1;
		progress ();
		
		//dialogue_manager.set_pause_portrait_visibility(true);
	}
	
	public void trim_script ()
	{
		Game.current.script = Game.current.script.GetRange(0, Game.current.script_pos + 1);
	}
	
	public void interrupt (int player)
	{
		if (Game.current.players[player-1].active)
		{
			Line this_line = Game.current.script[Game.current.script_pos];
			if (this_line.interrupt_label != "" && Game.current.choice_dict.ContainsKey(this_line.interrupt_label))
			{
				SFX_emitter.clip = dialogue_manager.sound_dict["cancel"];
				SFX_emitter.Play ();
				
				Game.current.interrupting = true;
				if (this_line.interrupt_type == "divert")
				{
					trim_script();
				}

				Debug.Log ("Inserting lines from interrupt with player = " + player.ToString ());
				insert_lines(player, Game.current.interrupt_choice.line_bundle);
				progress ();
			}
		}
	}

	public float paren_string_to_float (string paren_string)
	{
		float result = 0f;
		string working_string = paren_string;
		// Make replacements for float math
		working_string = working_string.Replace("true", "1");
		working_string = working_string.Replace("false", "0");

		int loop_count = 0;
		while (working_string.Contains (")"))
		{
			loop_count += 1;
			if (loop_count > 10)
			{
				Debug.Log("Exiting parenthetical loop. Ended up with working string: " + working_string);
				break;
			}

			int end_index = working_string.IndexOf(')');
			string first_string_part = working_string.Substring (0, end_index);
			int start_index = first_string_part.LastIndexOf('(');

			float reduced_num = string_to_float(working_string.Substring (start_index + 1, end_index - start_index - 1));

			if (working_string.Length > end_index + 1)
			{
				working_string = working_string.Substring (0, start_index) + reduced_num.ToString () + working_string.Substring(end_index + 1);
			}
			else
			{
				working_string = working_string.Substring (0, start_index) + reduced_num.ToString();
			}
				
		}

		result = string_to_float(working_string);

		return result;
	}

	public float string_to_float (string in_string)
	{
		// NEEDS TO DETECT STRINGS FOR COMPARISON AS WELL
		string working_string = in_string;

		string[] ops = new string[]{"^", "*", "/", "+", "- ", "<=", ">=", "<", ">", "==", "!=", "&&", "||"};

		foreach (string op in ops)
		{
			int loop_count = 0;
			while (working_string.Contains (op))
			{
				loop_count += 1;
				if (loop_count > 10)
				{
					Debug.Log("Exiting operation loop for " + op + ". Ended up with working string: " + working_string);
					break;
				}
				int op_index = working_string.IndexOf (op);
				int front_cursor_index = op_index;
				int back_cursor_index = op_index + op.Length;

				int front_loop_count = 0;
				while (front_cursor_index > 0)
				{
					front_loop_count += 1;
					if (front_loop_count > 100)
					{
						Debug.Log("Exiting front index loop. Ended up with front index: " + front_cursor_index.ToString());
						break;
					}
					char next_front_char = working_string[front_cursor_index - 1];
					//Debug.Log ("Next front character is: " + next_front_char);
					if (!string_ender_list.Contains(next_front_char))
					{
						front_cursor_index -= 1;
						//Debug.Log ("Valid");
					}
					else if (next_front_char == '-' && num_char_list.Contains(working_string[front_cursor_index]))
					{
						front_cursor_index -= 1;
						//Debug.Log ("Negative number");
					}
					else
					{
						//Debug.Log ("Breaking");
						break;
					}
				}

				int back_loop_count = 0;
				while (back_cursor_index < working_string.Length && !string_ender_list.Contains(working_string[back_cursor_index]))
				{
					back_loop_count += 1;
					if (back_loop_count > 100)
					{
						Debug.Log("Exiting back index loop. Ended up with back index: " + back_cursor_index.ToString());
						break;
					}
					back_cursor_index += 1;
				}

				string front_num_string = working_string.Substring (front_cursor_index, op_index - front_cursor_index).Trim ();
				string back_num_string = working_string.Substring(op_index + op.Length, back_cursor_index - (op_index + op.Length)).Trim ();
				//Debug.Log ("Calculating phrase: " + front_num_string + "|" + op + "|" + back_num_string);
				float result_num = rev_pol_list_to_num(new List<string>{front_num_string, back_num_string, op.Trim ()});

				working_string = working_string.Substring (0, front_cursor_index) + " " + result_num.ToString () + " " + working_string.Substring(back_cursor_index);
			}
		}

		float output_val = 0f;
		float.TryParse(working_string, out output_val);
		return output_val;
	}

	public float rev_pol_list_to_num (List<string> rev_pol_list)
	{
		List<string> valid_ops = new List<string>{"+", "-", "/", "*", "^", "||", "&&", "<", ">", "<=", ">=", "==", "!="};
		List<float> val_list = new List<float>();
		List<string> string_val_list = new List<string>();

		// Validate operations
		foreach (string word in rev_pol_list)
		{
			float float_val = 0f;
			if (float.TryParse (word, out float_val))
			{
				val_list.Add (float_val);
			}
			else if (valid_ops.Contains (word))
			{
				if (val_list.Count > 1)
				{

					float val2 = val_list[val_list.Count - 1];
					val_list.RemoveAt(val_list.Count - 1);
					
					float val1 = val_list[val_list.Count - 1];
					val_list.RemoveAt(val_list.Count - 1);

					string s_val1 = "";
					string s_val2 = "";
					bool using_strings = false;

					if (val2 == val1 && val1 == 12345f)
					{
						using_strings = true;

						s_val2 = string_val_list[string_val_list.Count - 1];
						string_val_list.RemoveAt(string_val_list.Count - 1);

						s_val1 = string_val_list[string_val_list.Count - 1];
						string_val_list.RemoveAt(string_val_list.Count - 1);
					}

					float new_val = 1f;
					switch (word)
					{
					case "||":
						if (val1 == 0f && val2 == 0f)
						{
							new_val = 0f;
						}
						else
						{
							new_val = 1f;
						}
						break;
					case "&&":
						if (val1 == 0f || val2 == 0f)
						{
							new_val = 0f;
						}
						else
						{
							new_val = 1f;
						}						
						break;
					case "!=":
						if ((using_strings && s_val1 == s_val2) || (!using_strings && val1 == val2))
						{
							new_val = 0f;
						}
						else
						{
							new_val = 1f;
						}						
						break;
					case "==":
						if ((using_strings && s_val1 == s_val2) || (!using_strings && val1 == val2))
						{
							new_val = 1f;
						}
						else
						{
							new_val = 0f;
						}						
						break;
					case ">=":
						if (val1 >= val2)
						{
							new_val = 1f;
						}
						else
						{
							new_val = 0f;
						}						
						break;
					case ">":
						if (val1 > val2)
						{
							new_val = 1f;
						}
						else
						{
							new_val = 0f;
						}			
						break;
					case "<=":
						if (val1 <= val2)
						{
							new_val = 1f;
						}
						else
						{
							new_val = 0f;
						}						
						break;
					case "<":
						if (val1 < val2)
						{
							new_val = 1f;
						}
						else
						{
							new_val = 0f;
						}						
						break;
					case "+":
						new_val = val1 + val2;
						break;
					case "-":
						new_val = val1 - val2;
						break;
					case "*":
						new_val = val1 * val2;
						break;
					case "/":
						new_val = val1 / val2;
						break;
					case "^":
						new_val = Mathf.Pow (val1, val2);
						break;
					}
					val_list.Add (new_val);
				}
			}
			else
			{
				string_val_list.Add (word);
				val_list.Add (12345f); // maintain parallel list of entries in val_list so we know when to look for a string
			}
		}
		return val_list[0];

	}
	
	public string plug_in_player_i (string statement)
	{
		if (statement.StartsWith ("$active_player"))
		{
			return "$player" + Game.current.stat_dict["_this_player"].int_val.ToString() + statement.Substring (14);
		}
		else
		{
			return statement;
		}
	}
	
	void make_change(string my_change)
	{
		string[] split_text = my_change.Trim ().Split (' ');
		string my_stat_key = plug_in_player_i(split_text[0].Trim ());
		
		if (my_change.Substring (0,8) == "$players")
		{
			Debug.Log ("changing stat for all players: " + my_change.Substring (9));
			int old_active_player = Game.current.stat_dict["_this_player"].int_val;
			string this_change = "$active_player" + my_change.Substring (8);
			for (int i=1; i<=4; i++)
			{
				if (Game.current.players[i-1].active)
				{
					Game.current.stat_dict["_this_player"].int_val = i;
					make_change(this_change);
				}
			}
			Game.current.stat_dict["_this_player"].int_val = old_active_player;
			return;
		}
		else if (my_change.StartsWith ("$active_player.signal"))
		{
			dialogue_manager.signal_player_change(Game.current.stat_dict["_this_player"].int_val, my_change.Split ('=')[1]);
			return;
		}


/*		Debug.Log ("Making change: " + my_change);
		Debug.Log ("Stat key: " + my_stat_key);*/
		Stat my_stat;
		string stripped_stat_key = my_stat_key.Substring (1);

		if (is_player_stat.IsMatch (my_stat_key))
		{
			my_stat = get_player_stat_from_string(stripped_stat_key, my_change);
		}
		else if (Game.current.stat_dict.ContainsKey(stripped_stat_key))
		{
			my_stat = Game.current.stat_dict[stripped_stat_key];
		}
		else if (split_text.Length > 2 && split_text[1] == "=")
		{
			string new_val = check_for_functions(plug_in_variables(my_change.Substring (my_change.IndexOf ('=') + 1))).Trim ();

			if (new_val.Contains ("+") || new_val.Contains ("-") || new_val.Contains ("/") || new_val.Contains ("*") || new_val.Contains ("^"))
			{
				//return;
				new_val = paren_string_to_float(new_val).ToString ();
			}

			Stat new_stat = parse_stat (stripped_stat_key, new_val);
			Game.current.stat_dict.Add (stripped_stat_key, new_stat);
			Debug.Log ("Creating stat named " + stripped_stat_key + " as a " + new_stat.var_type + " with value " + new_stat.get_string());
			return;
		}
		else
		{
			Debug.Log ("Couldn't find stat key: " + stripped_stat_key);
			return;
		}
		
		List<string> op_list = new List<string>();
		op_list.Add ("=");
		op_list.Add ("toggle");
		op_list.Add ("+=");
		op_list.Add ("-=");
		op_list.Add ("*=");
		op_list.Add ("/=");
		
		op_list.Add ("+%"); // used for tangential addition toward 1.0f or 100 (for int)
		op_list.Add ("-%"); // used for tangential subtraction toward 1.0f or 0 (for int)
		
		string my_op = split_text[1]; // Operators can be "==" ">" ">=" "<" "<=" (bool only: toggle)
		string val_string = "";
		if (split_text.Length > 2)
		{
			val_string = my_change.Substring (split_text[0].Length + split_text[1].Length + 2);
			val_string = plug_in_variables(val_string);
			// At this point the val_string should be just numbers and operators
			if (my_stat.var_type == "float")
			{
				val_string = paren_string_to_float(check_for_functions(val_string)).ToString ();
			}
			else if (my_stat.var_type == "int")
			{
				val_string = Mathf.RoundToInt(paren_string_to_float(check_for_functions(val_string))).ToString ();
			}
		}
		
		if (my_stat.var_type == "string")
		{
			if (my_op == "=")
			{
				my_stat.string_val = val_string;
			}
			else if (my_op == "+=")
			{
				my_stat.string_val += val_string;
			}
			else
			{
				Debug.Log ("strings may only be compared for equality (" + my_change + ")");
			}
			return;
		}
		else if (my_stat.var_type == "bool")
		{
			if (my_op == "=")
			{
				//my_stat.bool_val = bool.Parse(val_string);
				if (val_string == "0" || val_string == "false")
				{
					my_stat.bool_val = false;
				}
				else
				{
					my_stat.bool_val = true;
				}
			}
			else if (my_op == "toggle")
			{
				bool this_val = my_stat.bool_val;
				my_stat.bool_val = !this_val;
			}
			else
			{
				Debug.Log ("bools may only be assigned via '=' (" + my_change + ")");
			}
			return;
		}
		else if (op_list.Contains (my_op) == false)
		{
			Debug.Log ("Couldn't compute operator: " + my_op + " in (" + my_change + ")");
			return;
		}
		
		switch (my_stat.var_type)
		{
		case "int":
			int prev_val = my_stat.int_val;
			int test_val = int.Parse (val_string);
			
			switch (my_op)
			{
			case "=":
				my_stat.int_val = test_val;
				break;
			case "+=":
				my_stat.int_val += test_val;
				break;
			case "-=":
				my_stat.int_val -= test_val;
				break;
			case "*=":
				my_stat.int_val *= test_val;
				break;
			case "/=":
				my_stat.int_val /= test_val;
				break;
			case "+%":
				my_stat.int_val += test_val * (100 - my_stat.int_val) / 100;
				break;
			case "-%":
				my_stat.int_val -= my_stat.int_val * test_val / 100;
				break;
			}

			//Debug.Log("Stat key used for change: " + my_stat_key);
			if (is_player_stat.IsMatch (my_stat_key))
			{
				if (Game.current.default_stats_to_show.Contains(my_stat.text))
				{
					int player_i = 1;
					int stat_i = Game.current.default_stats_to_show.IndexOf(my_stat.text) + 1; // +1 because of name text field;
					if (int.TryParse(my_stat_key.Substring(7,1), out player_i))
					{
						dialogue_manager.player_guis[player_i - 1].transform.GetChild(1).GetChild(stat_i).GetComponentInChildren<DynamicIntegerText>().set_target_val(my_stat.int_val);
					}
				}
				else
				{
					int diff = my_stat.int_val - prev_val;
					Match match = is_player_stat.Match (my_stat_key);
					int player = int.Parse(match.Groups[1].Value);

					//Debug.Log("It's a player stat with diff: " + diff.ToString());

					string change_message = "";
					if (diff >= 0)
					{
						change_message = match.Groups[2].Value + " <color=green>+" + diff.ToString () + "</color>";
					}
					else
					{
						change_message = match.Groups[2].Value + " <color=red>" + diff.ToString () + "</color>";
					}
					dialogue_manager.signal_player_change(player, change_message);
				}

			}
			else if (dialogue_manager.game_stat_gui_showing() && Game.current.pause_stats.Contains(my_stat_key))
			{
				int game_stat_index = Game.current.pause_stats.IndexOf(my_stat_key);
				dialogue_manager.game_stat_gui.transform.GetChild(game_stat_index).GetComponentInChildren<DynamicIntegerText>().set_target_val(my_stat.int_val);
			}
			break;
		case "float":
			float ftest_val = float.Parse(val_string);
			
			switch (my_op)
			{
			case "=":
				my_stat.float_val = ftest_val;
				break;
			case "+=":
				my_stat.float_val += ftest_val;
				break;
			case "-=":
				my_stat.float_val -= ftest_val;
				break;
			case "*=":
				my_stat.float_val *= ftest_val;
				break;
			case "/=":
				my_stat.float_val /= ftest_val;
				break;
			case "+%":
				my_stat.float_val += ftest_val / 100f * (1f - my_stat.float_val);
				break;
			case "-%":
				my_stat.float_val -= my_stat.float_val / 100f * ftest_val;
				break;
			}

			Debug.Log("game_stat_gui_showing: " + dialogue_manager.game_stat_gui_showing().ToString() + ", looking for: " + my_stat_key + ", " + Game.current.pause_stats.Contains(my_stat_key).ToString());

			if (is_player_stat.IsMatch (my_stat_key))
			{
				if (Game.current.default_stats_to_show.Contains(my_stat.text))
				{
					int player_i = 1;
					int stat_i = Game.current.default_stats_to_show.IndexOf(my_stat.text); // +1 because of name text field;
					if (int.TryParse(my_stat_key.Substring(7,1), out player_i))
					{
						Debug.Log(dialogue_manager.player_guis.Count);
						dialogue_manager.player_guis[player_i - 1].transform.GetChild(1).GetChild(stat_i).GetComponentInChildren<DynamicMeter>().set_target_val(my_stat.float_val);
					}
				}
			}
			else if (dialogue_manager.game_stat_gui_showing() && Game.current.pause_stats.Contains(my_stat.text))
			{
				int game_stat_index = Game.current.pause_stats.IndexOf(my_stat.text);
				Debug.Log("making change");
				dialogue_manager.game_stat_gui.transform.GetChild(game_stat_index).GetComponentInChildren<DynamicMeter>().set_target_val(my_stat.float_val);
			}


			break;
		}
	}
	
	bool is_true(string my_condition)
	{
		float result = paren_string_to_float(plug_in_variables(plug_in_player_i(my_condition)));
		return result != 0f;	
	}
	
	bool all_true (List<string> conditions)
	{
		bool output = true;
		foreach (string cond in conditions)
		{
			if (is_true(cond) == false)
			{
				output = false;
				break;
			}
		}
		return output;
	}
	
	bool one_true (List<string> conditions)
	{
		bool output = false;
		if (conditions.Count == 0)
		{
			return true;
		}
		else
		{
			foreach (string cond in conditions)
			{
				if (is_true(cond))
				{
					return true;
				}
			}
			return output;
		}
	}
	
	public Stat get_player_stat_from_string (string phrase, string assignment_phrase = "")
	{
		//Debug.Log ("Getting player stat: " + phrase);
		if (phrase.StartsWith ("$"))
		{
			phrase = phrase.Substring (1);
		}
		string[] score_split = phrase.Split ('.');
		int player_i = int.Parse(score_split[0].Substring(6));
		//Debug.Log ("player_i: " + score_split[0].Substring(6) + ", " + phrase);
		if (Game.current.players[player_i - 1].stat_dict.ContainsKey(score_split[1]))
		{
			return Game.current.players[player_i - 1].stat_dict[score_split[1]];
			
		}
		else if (score_split[1] == "name")
		{
			//Debug.Log ("Returning a name: " + Game.current.players[player_i - 1].name);
			return new Stat("","","string",0,0f, Game.current.players[player_i - 1].name);
		}
		else if (score_split[1] == "active")
		{
			return new Stat("","","bool",0,0f,"",Game.current.players[player_i - 1].active);
		}
		else
		{
			Debug.Log ("Couldn't find stat named " + score_split[1].ToString() + " in stat dictionary of player " + player_i.ToString ());
			if (assignment_phrase != "" && assignment_phrase.Split (' ').Length > 2 && assignment_phrase.Split (' ')[1] == "=")
			{
				Stat new_stat = parse_stat(score_split[1], assignment_phrase.Split (' ')[2]);
				Game.current.players[player_i - 1].stat_dict.Add (score_split[1], new_stat);
				Debug.Log ("Creating player" + player_i.ToString () + " stat named " + score_split[1].ToString() + " as a " + new_stat.var_type + " with value " + new_stat.get_string());
				
				return new_stat;
			}
			else
			{
				return null;
			}
		}
	}
	
	public string find_variable (string word)
	{
		List<string> special_functions = new List<string>{"$pMax", "$pMin", "$Max", "$Min", "$Avg", "$Sum"};
		string next_output = "";
		
		string key_word = plug_in_player_i(word);
		//Debug.Log ("Got key_word: " + key_word + " from word: " + word);
		
		if (key_word.StartsWith ("random"))
		{
			return check_for_functions(word);
		}
		else if (is_player_stat.IsMatch (key_word))
		{
			return get_player_stat_from_string (key_word).get_string();
		}
		else if (key_word.Contains (".") && special_functions.Contains(key_word.Split ('.')[0]))
		{
			string func_name = key_word.Split ('.')[0];
			string stat_name = key_word.Substring (func_name.Length + 1);
			List<Character> active_players = new List<Character>();
			foreach (Character player in Game.current.players)
			{
				if (player.active)
				{
					active_players.Add (player);
				}
			}
			string var_type = active_players[0].stat_dict[stat_name].var_type;
			
			if (Game.current.players[0].stat_dict.ContainsKey (stat_name))
			{
				switch (func_name)
				{
				case "$Max":
					int max_i = 0;
					switch (var_type)
					{
					case "int":
						next_output = active_players[max_i].stat_dict[stat_name].int_val.ToString ();
						break;
					case "float":
						next_output = active_players[max_i].stat_dict[stat_name].float_val.ToString ();
						break;
					}
					
					for (int j = 0; j < active_players.Count; j++)
					{
						switch (var_type)
						{
						case "int":
							if (active_players[j].stat_dict[stat_name].int_val > active_players[max_i].stat_dict[stat_name].int_val)
							{
								max_i = j;
								next_output = active_players[j].stat_dict[stat_name].int_val.ToString ();
							}
							break;
						case "float":
							if (active_players[j].stat_dict[stat_name].float_val > active_players[max_i].stat_dict[stat_name].float_val)
							{
								max_i = j;
								next_output = active_players[j].stat_dict[stat_name].float_val.ToString ();
							}
							break;
						}
					}
					return next_output;
				case "$pMax":
					int pmax_i = 0;
					next_output = active_players[pmax_i].name;
					
					for (int j = 0; j < active_players.Count; j++)
					{
						switch (var_type)
						{
						case "int":
							if (active_players[j].stat_dict[stat_name].int_val > active_players[pmax_i].stat_dict[stat_name].int_val)
							{
								pmax_i = j;
								next_output = active_players[j].name;
							}
							break;
						case "float":
							if (active_players[j].stat_dict[stat_name].float_val > active_players[pmax_i].stat_dict[stat_name].float_val)
							{
								pmax_i = j;
								next_output = active_players[j].name;
							}
							break;
						}
					}
					return next_output;
				case "$Min":
					int min_i = 0;
					switch (var_type)
					{
					case "int":
						next_output = active_players[min_i].stat_dict[stat_name].int_val.ToString ();
						break;
					case "float":
						next_output = active_players[min_i].stat_dict[stat_name].float_val.ToString ();
						break;
					}
					
					for (int j = 0; j < active_players.Count; j++)
					{
						switch (var_type)
						{
						case "int":
							if (active_players[j].stat_dict[stat_name].int_val < active_players[min_i].stat_dict[stat_name].int_val)
							{
								min_i = j;
								next_output = active_players[j].stat_dict[stat_name].int_val.ToString ();
							}
							break;
						case "float":
							if (active_players[j].stat_dict[stat_name].float_val < active_players[min_i].stat_dict[stat_name].float_val)
							{
								min_i = j;
								next_output = active_players[j].stat_dict[stat_name].float_val.ToString ();
							}
							break;
						}
					}
					return next_output;
				case "$pMin":
					int pmin_i = 0;
					next_output = active_players[pmin_i].name;
					for (int j = 0; j < active_players.Count; j++)
					{
						switch (var_type)
						{
						case "int":
							if (active_players[j].stat_dict[stat_name].int_val < active_players[pmin_i].stat_dict[stat_name].int_val)
							{
								pmin_i = j;
								next_output = active_players[j].name;
							}
							break;
						case "float":
							if (active_players[j].stat_dict[stat_name].float_val < active_players[pmin_i].stat_dict[stat_name].float_val)
							{
								pmin_i = j;
								next_output = active_players[j].name;
							}
							break;
						}
					}
					return next_output;
				case "$Avg":
					int int_sum = 0;
					float float_sum = 0f;
					for (int j = 0; j < active_players.Count; j++)
					{
						switch (var_type)
						{
						case "int":
							int_sum += active_players[j].stat_dict[stat_name].int_val;
							break;
						case "float":
							float_sum += active_players[j].stat_dict[stat_name].float_val;
							break;
						}
					}
					switch (var_type)
					{
					case "int":
						return Mathf.RoundToInt(int_sum / (1f * active_players.Count)).ToString();
					case "float":
						return (float_sum / (1f * active_players.Count)).ToString();
					default:
						return word;
					}
				case "$Sum":
					int Int_sum = 0;
					float Float_sum = 0f;
					for (int j = 0; j < active_players.Count; j++)
					{
						switch (var_type)
						{
						case "int":
							Int_sum += active_players[j].stat_dict[stat_name].int_val;
							break;
						case "float":
							Float_sum += active_players[j].stat_dict[stat_name].float_val;
							break;
						}
					}
					switch (var_type)
					{
					case "int":
						return Int_sum.ToString();
					case "float":
						return Float_sum.ToString();
					default:
						return word;
					}
				default:
					return word;
				}
			}
			else
			{
				Debug.Log ("Couldn't find stat " + stat_name + " in player stat_dict");
				return word;
			}
			
		}
		else if (Game.current.stat_dict.ContainsKey(key_word.Substring (1)))
		{
			return Game.current.stat_dict[key_word.Substring (1)].get_string();
		}
		else
		{
			return word;
		}
	}
	public string plug_in_variables (string my_line)
	{
		string[] split_line = check_for_functions(my_line).Split ('$');
		string output = "";
		int start_index = 1;
/*		if (my_line.StartsWith("$"))
		{
			start_index = 0;
		}
		else
		{
			output = split_line[0];
		}*/

		output = split_line[0];

		
		for (int i=start_index; i< split_line.Length; i++)
		{
			if (output.Length > 0 && output[output.Length - 1] == '\\')
			{
				output = output.Substring (0, output.Length - 1) + "$" + split_line[i];
			}
			
			else
			{
				int back_cursor_index = 0;
				while (back_cursor_index < split_line[i].Length && !string_ender_list.Contains(split_line[i][back_cursor_index]) && !variable_ender_list.Contains(split_line[i][back_cursor_index]))
				{
					back_cursor_index += 1;
				}

				string key_word = split_line[i].Substring(0, back_cursor_index);
				while (key_word.EndsWith("."))
				{
					key_word = key_word.Substring(0, key_word.Length - 1);
				}
				int key_word_length = key_word.Length;
				output += find_variable ("$" + key_word);
				
				if (split_line[i].Length > key_word_length)
				{
					output += split_line[i].Substring (key_word_length);
				}
			}
		}
		return output;
	}
	
	public void done_choosing()
	{
		onPlayerChoosing(false);
	}

	public string check_for_functions (string phrase)
	{
		string output = "";
		string iterable = phrase.Replace ("random", "@");
		string[] split_list = iterable.Split ('@');
		for (int i=0; i < split_list.Length; i++)
		{
			if (i > 0)
			{
				string trim_word = split_list[i].TrimStart ();
				if (trim_word.StartsWith ("(") && trim_word.Contains(")"))
				{
					int end_index = trim_word.IndexOf (')');
					string match_phrase = trim_word.Substring (0, trim_word.IndexOf (')') + 1);
					//Debug.Log ("Match phrase: " + match_phrase);
					Match match = fun_2_int_var.Match(match_phrase);
					
					if (match.Success)
					{
						int v_min;
						int v_max;
						if (int.TryParse(find_variable(match.Groups[1].Value), out v_min) && int.TryParse (find_variable(match.Groups[2].Value), out v_max))
						{
							int random_number = Random.Range (v_min, v_max);
							output += random_number.ToString();
						}
						else
						{
							string[] possibility_list = trim_word.Substring(1, trim_word.IndexOf(")") - 1).Split(',');
							output += possibility_list[Random.Range(0, possibility_list.Length)].Trim();
						}

						// Preserve rest of string, like if someone types in "($attack_roll = random(1,$player_attack) + $player_attack_bonus)"
						if (trim_word.Length > end_index + 1)
						{
							output += trim_word.Substring (end_index + 1);
						}
					}

					else
					{
						output += "random" + split_list[i];
					}
				}
			}

			else
			{
				output += split_list[i];
			}
		}

		//Debug.Log ("check_for_functions() received [" + phrase + "] and returned [" + output + "]");
		return output;
	}

	Stat parse_stat (string resource_name, string stat_val)
	{
		int int_val;
		float float_val;
		bool bool_val;
		bool is_int = int.TryParse(stat_val, out int_val);
		bool is_float = float.TryParse(stat_val, out float_val);
		bool is_bool = bool.TryParse(stat_val, out bool_val);
		//Debug.Log (stat_val);
		if (stat_val.StartsWith ("random"))
		{
			return new Stat(resource_name, resource_name, "int", int.Parse(check_for_functions (stat_val)));
		}
		else if (is_int)
		{
			return new Stat(resource_name, resource_name, "int", int_val);
		}
		else if (is_float)
		{
			return new Stat(resource_name, resource_name, "float",0, float_val);
		}
		else if (is_bool)
		{
			return new Stat(resource_name, resource_name, "bool", 0, 0f,"", bool_val);
		}
		else
		{
			return new Stat(resource_name, resource_name, "string", 0,0f, stat_val);
		}
	}
	
	void load_story (string story_name)
	{
		//Debug.Log ("Loading story " + story_name);
		Game.current = new Game(story_name);
		dialogue_manager.load_settings (story_name);
		load_stats (Game.current.story_name);
	}
	
	void grab_font_vals()
	{
		Game.current.font_fail = dialogue_manager.font_fail;
		Game.current.font_ui = dialogue_manager.font_ui;
		Game.current.font_tutorial = dialogue_manager.font_tutorial;
		Game.current.font_nar = dialogue_manager.font_nar;
		Game.current.font_choice = dialogue_manager.font_choice;
		Game.current.font_char = dialogue_manager.font_char;
	}
	
	void new_game (string story_name)
	{
		Game.current = new Game(story_name);
		grab_font_vals();
		bool unique_ID = false;
		while (!unique_ID)
		{
			Game.current.ID += 1;
			unique_ID = true;
			foreach (Game saved_game in SaveLoad.savedGames)
			{
				if (saved_game.ID == Game.current.ID)
				{
					unique_ID = false;
					break;
				}
			}
		}
		load_stats (Game.current.story_name);
		Game.current.update_cap_stats();
		Game.current.update_cap_chars();

		Game.current.save_mode = save_mode;
		Game.current.show_plot_summaries = show_plot_summaries;
		Game.current.pause_all_lines = pause_all_lines;
		
		load_script (Game.current.story_name);
		Game.current.update_pause_menu_options();
		dialogue_manager.hide_uis();
		dialogue_manager.set_pause_portrait_visibility(false);
		onGameLoaded();
	}
	
	void delete_game (int game_index = -1)
	{
		if (game_index != -1)
		{
			SaveLoad.DeleteGame (game_index);
		}
	}
	
	void load_game (int game_index = -1)
	{
		//dialogue_manager.hide_all_speakers();
		
		if (game_index != -1)
		{
			SaveLoad.LoadGame (game_index);
		}
		
		// Start music if it was playing
		if (Game.current.music_playing != "")
		{
			Music_emitter.clip = Resources.Load (Game.current.get_file_path("music/" + Game.current.music_playing), typeof(AudioClip)) as AudioClip;
			Music_emitter.Play ();
		}
		
		// Set background if there was one
		if (Game.current.background_showing != "")
		{
			dialogue_manager.set_background(Game.current.background_showing, false);
		}
		
		// Set tutorial message if there was one
		if (Game.current.tutorial_message_showing != "")
		{
			dialogue_manager.show_tutorial_box(Game.current.tutorial_message_showing);
		}

		// Show pause stats if they were showing
		if (Game.current.game_stats_showing)
		{
			dialogue_manager.update_game_stat_ui();
		}
		else
		{
			dialogue_manager.hide_game_stats();
		}

		// Set up actors as they were
		for (int i = 0; i < 3; i++)
		{
			string my_pos = dialogue_manager.get_speaker_side(i);
			
			if (Game.current.stage_actors[i] == "")
			{
				dialogue_manager.set_portrait(my_pos, "transparent");
				
			}
			else
			{
				dialogue_manager.set_portrait(my_pos, plug_in_portrait_name(Game.current.stage_actors[i], Game.current.stage_poses[i]));
			}
		}

		// Set up background actors
		dialogue_manager.clear_bg_portraits();
/*		for (int i = 0; i < Game.current.bg_actors_left.Count; i++)
		{
			dialogue_manager.set_portrait("BGL", plug_in_portrait_name(Game.current.bg_actors_left[i], Game.current.bg_poses_left[i]));
		}

		for (int i = 0; i < Game.current.bg_actors_right.Count; i++)
		{
			dialogue_manager.set_portrait("BGR", plug_in_portrait_name(Game.current.bg_actors_right[i], Game.current.bg_poses_right[i]));
		}*/

		dialogue_manager.bg_portrait_setup_flag = true;
		
		dialogue_manager.update_fonts();
		
		dialogue_manager.new_name(Game.current.current_speaker_name, Game.current.current_speaker_pos);

		dialogue_manager.collapse_guis();
		dialogue_manager.hide_uis();
		
		dialogue_manager.set_pause_portrait_visibility(false);
		
		onGameLoaded();
	}
	
	public Color find_color (string target_color)
	{
		Color new_char_color = Color.white;
		if (dialogue_manager.valid_colors.ContainsKey (target_color))
		{
			new_char_color = dialogue_manager.valid_colors[target_color];
		}
		else if (target_color.Split (',').Length == 3)
		{
			List<float> color_rgb = new List<float>();
			foreach (string color_string in target_color.Split (','))
			{
				new_char_color = Color.black;
				float new_val;
				bool is_float = float.TryParse (color_string.Trim (), out new_val);
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
						new_char_color = Color.white;
						Debug.Log ("Couldn't parse character color: " + target_color);
						break;
					}
				}
				else
				{
					new_char_color = Color.white;
					Debug.Log ("Couldn't parse character color: " + target_color);
					break;
				}
			}
			if (new_char_color != Color.white)
			{
				new_char_color = new Color(color_rgb[0], color_rgb[1], color_rgb[2]);
			}
		}
		else
		{
			new_char_color = Color.white;
			Debug.Log ("Couldn't parse character color: " + target_color);
		}
		
		return new_char_color;
	}

	public int indent_level_of_string (string phrase)
	{
		int level = 1;
		string[] starting_tab_check_list = phrase.TrimEnd().Split('\t');
		foreach (string word in starting_tab_check_list)
		{
			if (word.Length > 0)
			{
				break;
			}
			else
			{
				level += 1;
			}
		}
		return level;
	}
	
	void load_stats (string story_name)
	{
		Game.current.p_chars_available.Clear ();
		Game.current.players.Clear ();
		
		List<string> stat_info = new List<string>();
		
		/*		if (!File.Exists(Application.dataPath + "/Resources/" + story_name + "/stats.txt"))
		{
			Debug.Log ("Couldn't find file named " + story_name + "/stats.txt in content folder");
			return;
		}
*/
		TextAsset stat_file = Resources.Load (Game.current.story_name + "/stats") as TextAsset;
		string[] lines = stat_file.text.Split ('\n');
		//string[] lines = File.ReadAllLines (Application.dataPath + "/Resources/" + story_name + "/stats.txt");
		
		foreach (string line in lines)
		{
			stat_info.Add (line.Replace("    ", "\t"));
		}
		//Find playable character list
		int stat_start = stat_info.IndexOf ("STATS");
		int default_stat_start = stat_info.IndexOf ("DEFAULT PLAYER STATS");
		int selection_stat_start = stat_info.IndexOf ("STATS VISIBLE DURING CHARACTER SELECTION");
		int pause_stat_start = stat_info.IndexOf ("GAME STATS VISIBLE ON PAUSE SCREEN");
		int pause_player_stat_start = stat_info.IndexOf ("PLAYER STATS VISIBLE ON PAUSE SCREEN");
		int playable_character_name_start = stat_info.IndexOf ("PLAYABLE CHARACTERS");
		int character_stat_start = stat_info.IndexOf ("CHARACTERS");
		
		Game.current.stat_dict.Clear ();
		Game.current.stat_dict["_this_player"] = new Stat("Player Index", "_this_player", "int");
		Game.current.stat_dict["_player_count"] = new Stat("Player Count", "_player_count", "int");
		for (int i = stat_start + 1; i < default_stat_start; i++)
		{
			if (stat_info[i] != "")
			{
				string stat_name = stat_info[i].Split ('=')[0].Trim ();
				string stat_val = stat_info[i].Split ('=')[1].Trim ();
				Game.current.stat_dict.Add (stat_name, parse_stat (stat_name, stat_val));
			}
		}
		
		Game.current.default_stats.Clear ();
		for (int i = default_stat_start + 1; i < selection_stat_start; i++)
		{
			if (stat_info[i] != "")
			{
				string resource = stat_info[i].Trim ();
				string resource_name = "";
				string[] split_text = resource.Split ('=');
				
				Stat new_stat;
				if (split_text.Length > 1)
				{
					resource_name = split_text[0].Trim ();
					new_stat = parse_stat(split_text[0].Trim (), split_text[1].Trim ());
					
				}
				else
				{
					resource_name = resource;
					new_stat = new Stat(resource_name, resource_name, "int", 50);
					
				}
				
				Game.current.default_stats.Add (new_stat);
			}
		}
		
		Game.current.char_select_stats.Clear ();
		Game.current.pause_player_stat_aliases.Clear();
		for (int i = selection_stat_start + 1; i < pause_stat_start; i++)
		{
			if (stat_info[i] != "")
			{
				Game.current.grab_char_select_stat_and_alias (stat_info[i].Trim ());
			}
		}

		Game.current.pause_stats.Clear ();
		Game.current.pause_stat_aliases.Clear();
		for (int i = pause_stat_start + 1; i < pause_player_stat_start; i++)
		{
			if (stat_info[i] != "")
			{
				Game.current.grab_pause_stat_and_alias (stat_info[i].Trim ());
			}
		}

		Game.current.pause_player_stats.Clear ();
		for (int i = pause_player_stat_start + 1; i < playable_character_name_start; i++)
		{
			if (stat_info[i] != "")
			{
				Game.current.grab_player_pause_stat_and_alias (stat_info[i].Trim ());
			}
		}
		
		Game.current.p_chars_available.Clear ();
		for (int i = playable_character_name_start + 1; i < character_stat_start; i++)
		{
			if (stat_info[i] != "")
			{
				Game.current.player_names.Add (stat_info[i].Trim ());
			}
		}
		
		foreach (string name in Game.current.player_names)
		{
			if (!name.StartsWith ("Player"))
			{
				Game.current.p_chars_available.Add (name);

			}
		}
		
		Game.current.char_dict.Clear ();
		string context = "none";
		string new_char_name = "";
		string new_char_greeting = "";
		Dictionary<string, Stat> new_char_stats = new Dictionary<string, Stat>();
		Color new_char_color = new Color();
		Dictionary<string, string> new_char_poses = new Dictionary<string, string>();
		float new_char_pitch = 1f;
		string most_recent_anim_name = "";

		for (int i = character_stat_start + 1; i <= stat_info.Count; i++)
		{
			if (i == stat_info.Count)
			{
				Game.current.char_dict.Add (new_char_name, new Character(new_char_name, new_char_color, new_char_poses, new_char_stats, new_char_greeting));
			}
			else if (stat_info[i] != "")
			{
				int level = indent_level_of_string(stat_info[i]);
				
				switch (level)
				{
				case 1:
					
					if (stat_info[i].Trim () != "")
					{
						if (context != "none")
						{
							Character new_character = new Character(new_char_name, new_char_color, new_char_poses, new_char_stats, new_char_greeting);
							Game.current.char_dict.Add (new_char_name, new_character);
							
							new_character.voice_pitch = new_char_pitch;

							new_char_pitch = 1f;
							new_char_poses.Clear ();
							new_char_stats.Clear ();
						}
						new_char_name = stat_info[i].Trim ();
					}
					break;
				case 2:
					if (stat_info[i].Trim() == "Stats")
					{
						context = "Stats";
					}
					else if (stat_info[i].Trim () == "Poses")
					{
						context = "Poses";
					}
					else if (stat_info[i].Split ('=')[0].Trim () == "Color")
					{
						string target_color = stat_info[i].Split ('=')[1].Trim ();
						new_char_color = find_color(target_color);
					}
					else if (stat_info[i].Split ('=')[0].Trim () == "Entry phrase")
					{
						new_char_greeting = stat_info[i].Split ('=')[1].Trim ();
					}
					else if (stat_info[i].Split ('=')[0].Trim () == "Vocal pitch")
					{
						new_char_pitch = float.Parse (stat_info[i].Split ('=')[1].Trim ());
					}
					break;
				case 3:
					switch (context)
					{
					case "Poses":
						string image_name = stat_info[i].Split ('=')[1].Trim ();
						new_char_poses.Add (stat_info[i].Split ('=')[0].Trim (), image_name);
						dialogue_manager.check_for_anim (image_name);
						most_recent_anim_name = image_name;
						
						break;
					case "Stats":
						string resource_name = stat_info[i].Split ('=')[0].Trim ();
						string stat_val = stat_info[i].Split ('=')[1].Trim ();
						new_char_stats.Add (resource_name, parse_stat(resource_name, stat_val));
						break;
					}
					break;
				case 4:
					switch (context)
					{
					case "Poses":
						if (stat_info[i].Contains ("="))
						{
							string key_name = stat_info[i].Split ('=')[0].Trim ();
							string val_name = stat_info[i].Split ('=')[1].Trim ();
							
							switch (key_name)
							{
							case "loop_frame":
								int loop_frame;
								bool is_int = int.TryParse (val_name, out loop_frame);
								if (is_int)
								{
									dialogue_manager.anim_dict[most_recent_anim_name].loop_index = int.Parse (val_name);
									dialogue_manager.anim_dict[most_recent_anim_name].looping = true;
								}
								break;
							case "times":
								List<float> new_times = new List<float>();
								string[] split_times = val_name.Split (',');
								foreach (string word in split_times)
								{
									float this_duration;
									bool is_float = float.TryParse(word.Trim (), out this_duration);
									if (is_float)
									{
										new_times.Add (this_duration);
									}
								}
								dialogue_manager.anim_dict[most_recent_anim_name].times = new_times;
								break;
							}
						}
						break;
					}
					break;
				}
			}
		}
		
		// load default stats for players
		for (int i=0; i < player_colors.Count; i++)
		{
			Dictionary<string, Stat> p_stat_dict = new Dictionary<string, Stat>();
			foreach (Stat resource in Game.current.default_stats)
			{	
				p_stat_dict.Add (resource.slug, resource.make_copy ());
			}
			Game.current.players.Add (new Character("Player " + (i+1).ToString (), player_colors[i], new Dictionary<string, string>(), p_stat_dict));
		}
		
		Game.current.check_for_icon_images();
	}
	
	public float[] get_RGB(Color my_color)
	{
		return new float[3]{my_color.r, my_color.g, my_color.b};
	}
	
	public Color get_color(float[] RGB_vals)
	{
		return new Color(RGB_vals[0], RGB_vals[1], RGB_vals[2]);
	}

	public string get_first_matching_actor_or_variable_from_line (string line_text)
	{
		string match = get_first_matching_actor_name_from_line(line_text);
		if (match != "")
		{
			return match;
		}
		else if (line_text.Contains("$active_player"))
		{
			return "$active_player.name";
		}
		else if (line_text.Contains ("$"))
		{
			string stat_name = line_text.Split ('$')[1].Split ()[0];
			bool is_stat = Game.current.stat_dict.ContainsKey (stat_name) && Game.current.stat_dict[stat_name].var_type == "string";
			bool is_player_function_stat = (stat_name.StartsWith("pMax.") || stat_name.StartsWith ("pMin.")) && Game.current.has_default_player_stat(stat_name.Split('.')[1]);

			if (is_stat || is_player_function_stat)
			{
				return "$" + stat_name;
			}
			else
			{
				return "";
			}
		}
		else
		{
			return "";
		}
	}

	public string get_first_matching_actor_name_from_line (string line_text)
	{
		foreach (string char_name in Game.current.char_dict.Keys)
		{
			if (line_text.Contains (char_name))
			{
				return char_name;
			}
		}
		return "";
	}

	public string get_first_position_from_line (string line_text)
	{
		if (line_text.Contains (" left"))
		{
			return "L";
		}
		else if (line_text.Contains (" right"))
		{
			return "R";
		}
		else if (line_text.Contains (" center"))
		{
			return "C";
		}
		else
		{
			return "";
		}
	}

	Dictionary<string,string> comma_separated_assignments_to_dict (string line_text, string assignment_symbol = ":")
	{
		Dictionary<string, string> arg_dict = new Dictionary<string, string>();
		arg_dict.Add ("_no_assignment", "");
		arg_dict.Add ("_first_key", "");
		
		bool first_identified = false;
		
		foreach (string arg_string in line_text.Split (','))
		{
			if (!first_identified)
			{
				first_identified = true;
				
				if (arg_string.Contains (":"))
				{
					int colon_index = arg_string.IndexOf (assignment_symbol);
					string key = arg_string.Substring (0, colon_index).Trim ();
					arg_dict["_first_key"] = key;
				}
				else
				{
					arg_dict["_first_key"] = line_text.Split(',')[0];
				}
			}

			if (arg_string.Contains (":"))
			{
				int colon_index = arg_string.IndexOf (assignment_symbol);
				string key = arg_string.Substring (0, colon_index).Trim ();
				string value = arg_string.Substring (colon_index + 1).Trim ();
				arg_dict.Add (key, value);
			}
			else if (arg_dict["_no_assignment"].Length == 0)
			{
				arg_dict["_no_assignment"] = arg_string;
	        }
	        else
	        {
				arg_dict["_no_assignment"] += ", " + arg_string;
			}
		}
		return arg_dict;
	}

	void load_script (string story_name)
	{
		// CLEAR PREVIOUS VALUES
		Game.current.initialize_decision_dict();
		Game.current.cutscene_dict.Clear();
		Game.current.choice_dict.Clear();

		// LOAD SCRIPT

		TextAsset script_file = Resources.Load (Game.current.story_name + "/script") as TextAsset;
		string[] script_lines = script_file.text.Split ('\n');

		Line most_recent_line = new Line("");
		List<string> line_attributes = new List<string>{
			"interrupt",
			"voice over",
		};
		List<string> line_commands = new List<string>{
			"important",
			"pause",
		};

		Decision most_recent_decision = new Decision();
		List<string> decision_attributes = new List<string>{
			"type",
			"currency",
			"timer",
			"exclusion message",
			"wrap count",
			"player test",
			"show stats",
			"random order",
		};
		List<string> valid_decision_types = new List<string>{
			"per player",
			"first",
			"vote",
			"hidden vote",
			"agreement",
			"spinner",
			"hidden spinner",
			"poll",
			"hidden poll",
			"shop",
			"lottery",
		};

		Choice most_recent_choice = new Choice();
		List<string> choice_attributes = new List<string>{
			"type",
			"bad tickets",
			"limit",
			"price",
			"hide",
			"color",
			"show if",
			"per vote",
			"requires",
			"left icon",
			"right icon",
		};

		string most_recent_speaker_name = "Narrator";
		string most_recent_cs_key = "";
		CutsceneManager.Direction most_recent_direction = new CutsceneManager.Direction("");
		CutsceneManager.TimePoint most_recent_timepoint = new CutsceneManager.TimePoint(0f);

		List<string> cutscene_attributes = new List<string>{
			"skippable",
		};
		List<string> cutscene_commands = new List<string>{
			"pan",
			"zoom",
			"fade in",
			"fade out",
			"clear",
			"rotate",
		};
		List<string> timepoint_commands = new List<string>{
			"sound",
			"text",
		};

		/*List<string> change_bundle_choice_modes = new List<string>{
			"shop",
		};*/

		List<string> p_max_comparison_words = new List<string>{
			"MOST",
			"GREATEST",
			"HIGHEST",
		};

		List<string> p_min_comparison_words = new List<string>{
			"FEWEST",
			"LEAST",
			"LOWEST",
		};

		List<string> contexts = new List<string>();
		List<int> context_levels = new List<int>();
		List<string> conditional_branch_nest = new List<string>();

		int level = 0;

		foreach (string my_line in script_lines)
		{
			// shave off any comments
			string line_text = Regex.Split (my_line, "//")[0].TrimEnd ();

            // replace 4 spaces with tabs
            line_text = line_text.Replace("    ", "\t");
				         
	        if (line_text != "") // if anything remains...
        	{
				level = indent_level_of_string (line_text); // count tab spaces to determine indent level
				line_text = line_text.Trim (); // then get rid of tab spaces
				
				// "contexts" are stored in a stack, so pop off of that stack until the levels match
				// contexts tell you what you're indented under, like a line or a cutscene time point

				while ((context_levels.Count > 1 || conditional_branch_nest.Count > 0) && level <= context_levels[context_levels.Count - 1])
				{
					if (level == context_levels[context_levels.Count - 1] && (line_text.StartsWith("(else:") || line_text.StartsWith("(else if:")))
					{
						break;
					}
					if (contexts [contexts.Count - 1] == "choice" && most_recent_decision.name.StartsWith("_conditional"))
					{
						Debug.Log("Removing context for conditional: " + conditional_branch_nest[conditional_branch_nest.Count - 1]);
						conditional_branch_nest.RemoveAt(conditional_branch_nest.Count - 1);
						if (conditional_branch_nest.Count > 0)
						{
							Debug.Log("Falling into conditional: " + conditional_branch_nest[conditional_branch_nest.Count - 1]);

							most_recent_decision = Game.current.decision_dict[conditional_branch_nest[conditional_branch_nest.Count - 1]];
							most_recent_choice = most_recent_decision.choices[most_recent_decision.choices.Count - 1];
						}
					}
					contexts.RemoveAt (contexts.Count - 1);
					context_levels.RemoveAt (context_levels.Count - 1);
				}
				
				// ----- FIRST, LOOK FOR CONTEXT RESETTERS: SCENES, DECISIONS/BRANCHES, AND CUTSCENES -----
				
				// lines that start with "---" are scenes
				if (level == 1 && line_text.StartsWith ("---") && line_text.EndsWith ("---") && line_text.Length > 6)
				{
					// reset contexts
					contexts = new List<string>{"scene"};
					context_levels = new List<int>{0};
					
					// parse label
					int start_index = 3;
					int end_index = line_text.Length - 3;
					string scene_key = line_text.Substring (start_index, end_index - start_index).Trim ();
					//Debug.Log ("New scene: " + scene_key);
					
					// store most recent objects
					Game.current.choice_dict.Add (scene_key, new Choice());
					most_recent_choice = Game.current.choice_dict[scene_key];
					most_recent_choice.text = scene_key;
				}
				
				// lines that start with "[[" are branches or decisions
				else if (level == 1 && line_text.StartsWith ("[[") && line_text.Contains ("]]"))
				{
					// reset contexts
					contexts = new List<string>{"decision"};
					context_levels = new List<int>{0};
					
					// parse label
					int start_index = line_text.IndexOf ('[') + 2;
					int end_index = line_text.IndexOf (']');
					string decision_key = line_text.Substring (start_index, end_index - start_index).Trim ();
                    //Debug.Log ("New decision/branch: " + decision_key);

                    // store most recent objects
                    Debug.LogFormat("Storing most recent decision as: {0}", decision_key);
					Game.current.decision_dict.Add (decision_key, new Decision());
					most_recent_decision = Game.current.decision_dict[decision_key];
					most_recent_decision.name = decision_key;
					most_recent_decision.randomize_order = dialogue_manager.randomize_choices;
					
					// check for arguments for decisions/branches
					if (!line_text.EndsWith ("]"))
					{
						string extra_text = line_text.Substring (end_index + 2).Trim ();
						if (extra_text.EndsWith (")") && extra_text.StartsWith ("("))
						{
							string arg_string = extra_text.Substring (1, extra_text.Length - 2);
							Dictionary<string,string> arg_dict = comma_separated_assignments_to_dict(arg_string);
							
							foreach (string key in arg_dict.Keys)
							{
								if (decision_attributes.Contains (key))
								{
									string val_text = arg_dict[key];
									switch (key)
									{
									case "player test":
										most_recent_decision.participant_conditions.Add (val_text);
										break;
									case "random order":
										if (val_text == "true")
										{
											most_recent_decision.randomize_order = true;

										}
										else if (val_text == "false")
										{
											most_recent_decision.randomize_order = false;
										}
										break;
									case "show stats":
										foreach (string stat_name in val_text.Split(';'))
										{
											string verified_stat_name = stat_name.Trim ();
											if (Game.current.has_default_player_stat(verified_stat_name))
											{
												most_recent_decision.stats_to_show.Add (verified_stat_name);
											}
											else
											{
												Debug.Log("Stat (" + verified_stat_name + ") to display as part of decision (" + decision_key + ") not recognized as default player stat.");
											}
										}
										break;
									case "type":
										if (valid_decision_types.Contains (val_text))
										{
											most_recent_decision.choice_mode = val_text;
										}
										else
										{
											Debug.Log("Decision/Branch type not recognized for " + decision_key + ": " + val_text + ". Defaulting to 'vote'");
											most_recent_decision.choice_mode = "vote";
										}
										break;
									case "currency":
										if (Game.current.has_default_player_stat(val_text))
										{
											if (Game.current.default_player_stat_is_int(val_text))
											{
												//Debug.Log("Currency (" + val_text + ") for Decision/Branch (" + decision_key + ") has been assigned.");

												most_recent_decision.allocation_currency = val_text;
												
											}
											else
											{
												Debug.Log("Currency (" + val_text + ") for Decision/Branch (" + decision_key + ") must be an integer stat.");
											}
										}
										else
										{
											Debug.Log("Default player stat (" + val_text + ") not found. Currency for Decision/Branch (" + decision_key + ") must be a default player stat.");
										}
										break;
									case "exclusion message":
										most_recent_decision.exclusion_text = val_text;
										break;
									case "timer":
										float float_test;
										if (float.TryParse(val_text, out float_test))
										{
											most_recent_decision.fail_timer = float_test;
										}
										else
										{
											Debug.Log("Argument for [timer] in Decision/Branch (" + decision_key + ") must be a float (e.g. 1.2)");
										}
										break;
									case "wrap count":
										int int_test;
										if (int.TryParse(val_text, out int_test))
										{
											most_recent_decision.options_per_column = int_test;
										}
										else
										{
											Debug.Log("Argument for [wrap count] in Decision/Branch (" + decision_key + ") must be an int (e.g. 6)");
										}
										break;
									}
								}
							}
						}
					}
				}
					
				// lines starting with "<<" are cutscenes
				else if (level == 1 && line_text.StartsWith("<<") && line_text.Contains (">>"))
				{
					// reset contexts
					contexts = new List<string>{"cutscene"};
					context_levels = new List<int>{0};
					
					// parse label
					int bracket_index = line_text.IndexOf ('>');
					most_recent_cs_key = line_text.Substring (2, bracket_index - 2).Trim ();
					//Debug.Log ("New cutscene: " + most_recent_cs_key);
					
					// store most recent objects
					cutscene_manager.add_new(most_recent_cs_key);
					
					// check for optional arguments
					string extra_text = line_text.Substring (bracket_index + 2).Trim ();
					if (extra_text.EndsWith (")") && extra_text.StartsWith ("("))
					{
						string arg_string = extra_text.Substring (1, line_text.Length - 2);
						Dictionary<string,string> arg_dict = comma_separated_assignments_to_dict(arg_string);
						
						foreach (string key in arg_dict.Keys)
						{
							if (cutscene_attributes.Contains (key))
							{
								switch (key)
								{
								case "skippable":
									if (!bool.TryParse(arg_dict[key], out Game.current.cutscene_dict[most_recent_cs_key].skippable))
									{
										Debug.Log("Value for [skippable] on cutscene (" + most_recent_cs_key + ") must be either 'true' or 'false'");
									}
									break;
								}
							}
						}
					}
				}
				
				// ----- THEN, SORT THINGS OUT BY CONTEXT -----
				
				// Contexts are super weird exceptions, so we'll handle those first
				else if (contexts.Count > 0 && contexts[0] == "cutscene")
				{
					Dictionary<string,string> arg_dict = comma_separated_assignments_to_dict(line_text);
					if (level == 1)
					{
						switch (arg_dict["_first_key"])
						{
						case "layer":
							if (arg_dict.ContainsKey ("image"))
							{
								Game.current.cutscene_dict[most_recent_cs_key].image_dict.Add (arg_dict["layer"], arg_dict["image"]);
								if (arg_dict.ContainsKey ("stretch") && arg_dict["stretch"] == "false")
								{
									Game.current.cutscene_dict[most_recent_cs_key].no_stretch_list.Add(most_recent_cs_key);
								}
							}
							else
							{
								Debug.Log("Layer (" + arg_dict["layer"] + ") requires an [image] argument.");
							}
							
							break;
						case "timepoint":
							float time_point_float;
							if (float.TryParse (arg_dict["timepoint"], out time_point_float))
							{
								most_recent_timepoint = new CutsceneManager.TimePoint(time_point_float);
								Game.current.cutscene_dict[most_recent_cs_key].timepoints.Add (most_recent_timepoint);
								//Debug.Log ("adding timepoint " + time_point_float.ToString () + " to scene " + most_recent_cs_key);
							}
							else
							{
								Debug.Log("Invalid timepoint (" + arg_dict["timepoint"] + ") in cutscene (" + most_recent_cs_key + ")");
							}
							
							break;
						default:
							break;
						}
					}
					else if (level == 2)
					{
						if (timepoint_commands.Contains(arg_dict["_first_key"]))
						{
							most_recent_timepoint.directions.Add (new CutsceneManager.Direction(line_text));
						}
						else if (cutscene_commands.Contains (arg_dict["_first_key"]))
						{
							most_recent_direction = new CutsceneManager.Direction(arg_dict["_first_key"] + " " + arg_dict[arg_dict["_first_key"]]);
							most_recent_timepoint.directions.Add (most_recent_direction);
							most_recent_direction.arg_dict = arg_dict;
						}
					}
				}
				
				// lines in the branch or decision context are choice names if not indented
				else if (contexts.Count > 0 && contexts[0] == "decision" && level == 1)
				{
					// parse label
					string choice_key = line_text;
					string extra_text = "";
					
					if (line_text.Contains ("(") && line_text.EndsWith (")"))
					{
						int paren_index = line_text.IndexOf ('(');
						choice_key = line_text.Substring (0, paren_index).TrimEnd();
						extra_text = line_text.Substring(paren_index + 1, line_text.Length - (paren_index + 2)).Trim();
					}
					
					//Debug.Log ("New decision/branch: " + choice_key);
					
					// store most recent objects
					most_recent_decision.choices.Add (new Choice());
					most_recent_choice = most_recent_decision.choices[most_recent_decision.choices.Count - 1];
					most_recent_choice.text = choice_key;
					most_recent_choice.hide = dialogue_manager.hide_choices;

					//Debug.Log ("Adding choice to decision (" + most_recent_decision.name + "): " + most_recent_choice.text);
					
					// check for arguments for decisions/branches
					Dictionary<string,string> arg_dict = comma_separated_assignments_to_dict(extra_text);
					
					foreach (string key in arg_dict.Keys)
					{
						if (choice_attributes.Contains (key))
						{
							string val_text = arg_dict[key];
							switch (key)
							{
							case "show if":
								most_recent_choice.conditions.Add (val_text);
								//Debug.Log ("Adding condition (" + val_text + ") to choice :" + most_recent_choice.text);
								break;
							case "per vote":
								foreach (string change in val_text.Split (';'))
								{
									most_recent_choice.changes.Add (change.Trim());
								}
								break;
							case "type":
								if (val_text == "failure" || val_text == "fail")
								{
									most_recent_decision.fail_choices.Add (most_recent_decision.choices[most_recent_decision.choices.Count - 1]);
									most_recent_decision.choices.RemoveAt (most_recent_decision.choices.Count - 1);
								}
								else
								{
									Debug.Log("Ignoring [type] argument for choice (" + choice_key + ") of decision (" + most_recent_decision.name + ") because the only ones recognized are 'fail' and 'failure'");
								}
								break;
							case "limit":
								int limit_int = 0;
								bool is_int = int.TryParse(val_text, out limit_int);
								if (is_int)
								{
									while (most_recent_decision.allocation_limits.Count < most_recent_decision.choices.Count - 1)
									{
										most_recent_decision.allocation_limits.Add (-1);
									}
									most_recent_decision.allocation_limits.Add (limit_int);
								}
								else // limit might be a variable name
								{
									if (Game.current.stat_dict.ContainsKey(val_text))
									{
										while (most_recent_decision.allocation_stat_limits.Count < most_recent_decision.choices.Count - 1)
										{
											most_recent_decision.allocation_stat_limits.Add ("");
										}
										most_recent_decision.allocation_stat_limits.Add (val_text);
									}
									else // text for stat name not found
									{
										Debug.Log("Limit provided for choice (" + choice_key + ") provided as stat name, but stat (" + val_text + ") is not found in stat file.");
									}
								}
								break;
							case "bad tickets":
								int bid_int = 0;
								bool bid_is_int = int.TryParse(val_text, out bid_int);
								if (bid_is_int)
								{
									while (most_recent_decision.fail_bids.Count < most_recent_decision.fail_choices.Count - 1)
									{
										most_recent_decision.fail_bids.Add (0);
									}
									most_recent_decision.fail_bids.Add (bid_int);
								}
								else
								{
									if (Game.current.stat_dict.ContainsKey(val_text))
									{
										while (most_recent_decision.fail_stat_bids.Count < most_recent_decision.fail_choices.Count - 1)
										{
											most_recent_decision.fail_stat_bids.Add ("");
										}
										most_recent_decision.fail_stat_bids.Add (val_text);
									}
									else // text for stat name not found
									{
										Debug.Log("Agument [bad tickets] provided for choice (" + choice_key + ") provided as stat name, but stat (" + val_text + ") is not found in stat file.");
									}
								}
								break;
							case "left icon":
								most_recent_choice.left_icon_name = val_text;
								break;
							case "right icon":
								most_recent_choice.right_icon_name = val_text;
								break;
							case "price":
								int price_int = 1;
								bool price_is_int = int.TryParse(val_text, out price_int);
								if (price_is_int)
								{
									while (most_recent_decision.allocation_costs.Count < most_recent_decision.choices.Count - 1)
									{
										most_recent_decision.allocation_costs.Add (1);
									}
									most_recent_decision.allocation_costs.Add (price_int);
								}
								else
								{
									if (Game.current.stat_dict.ContainsKey(val_text))
									{
										while (most_recent_decision.allocation_stat_costs.Count < most_recent_decision.choices.Count - 1)
										{
											most_recent_decision.allocation_stat_costs.Add ("");
										}
										most_recent_decision.allocation_stat_costs.Add (val_text);
									}
									else // text for stat name not found
									{
										Debug.Log("Limit provided for choice (" + choice_key + ") provided as stat name, but stat (" + val_text + ") is not found in stat file.");
									}
								}
								break;
							case "color":
								most_recent_choice.RGBcolor = get_RGB(find_color(val_text));
								most_recent_choice.custom_color = true;
								break;
							case "hide":
								bool hide_me = false;
								bool val_is_bool = bool.TryParse (val_text, out hide_me);
								if (val_is_bool)
								{
									most_recent_choice.hide = hide_me;
								}
								break;
							case "requires":
								most_recent_choice.req_string = val_text;
								break;
							}
						}
					}
				}
				
				// If not in a cutscene, similar dialogue syntax is expected
				
				// lines that start with "(" are commands
				else if (line_text.StartsWith("(") && line_text.EndsWith (")"))
				{
					// command text stored up to first comma. The arguments after the comma are stored as args
					Line this_line = new Line(line_text.Substring(1, line_text.Length - 2).Trim ());
					this_line.arg_dict = comma_separated_assignments_to_dict(line_text.Substring(1, line_text.Length - 2));
					string first_key = this_line.arg_dict["_first_key"];

					// line commands and line attributes modify the most recent lines, so take care of them first
					if (line_commands.Contains(this_line.text))
					{
						switch (this_line.text)
						{
						case "pause":
							most_recent_line.pause = true;
							break;
						case "important":
							most_recent_line.important = true;
							break;
						}
					}
					else if (line_attributes.Contains (first_key))
					{
						switch (first_key)
						{
						case "voice over":
							most_recent_line.sound = this_line.arg_dict["voice over"];
							//Debug.Log("Assigning sound: " + this_line.arg_dict["voice over"] + " to line: " + this_line.text);
							break;
						case "interrupt":
							//Debug.Log ("Interrupt detected. Adding to line: " + most_recent_line.text);
							if (this_line.arg_dict.ContainsKey("scene"))
							{
								most_recent_line.interrupt_message = this_line.arg_dict["interrupt"];
								most_recent_line.interrupt_label = this_line.arg_dict["scene"];
								if (this_line.arg_dict.ContainsKey ("type"))
								{
									most_recent_line.interrupt_type = this_line.arg_dict["type"];
								}
							}
							else
							{
								Debug.Log("Interrupt (" + this_line.arg_dict["interrupt"] + ") needs a goto (scene: blah) on the following line: " + this_line.text);
							}
							
							break;
						default:
							break;
						}
					}
					// Look for adjustments to position or pose (enter, moves, looks) (exit just stored and handled during interpretation)
					else if (first_key.ToLower ().StartsWith ("enter") || first_key.Contains (" looking ") || first_key.Contains (" looks ") || first_key.Contains (" moves "))
					{
						// First, find the actor
						string actor_name = get_first_matching_actor_or_variable_from_line(first_key);

						// Then, figure out where they're going
						string position = "";
						string pose_name = "";

						if (first_key.Contains (" looks ") || first_key.Contains(" looking "))
						{
							string[] split_phrase = first_key.Split ();
							int looks_i = 0;
							for (int index = 0; index < split_phrase.Length; index ++)
							{
								if (split_phrase[index] == "looks" || split_phrase[index] == "looking")
								{
									looks_i = index;
									break;
								}
							}
							
							if (split_phrase.Length > looks_i + 1)
							{
								pose_name = split_phrase[looks_i + 1];
							}
						}

						if (first_key.ToLower ().StartsWith ("enter") || first_key.Contains (" moves "))
						{
							position = get_first_position_from_line(first_key);

							string first_dest = "";
							if (first_key.Contains (" from left"))
							{
								first_dest = "left";
							}
							else if (first_key.Contains (" from right"))
							{
								first_dest = "right";
							}

							if (first_dest != "")
							{
								Line pre_line = new Line("");
								pre_line.speaker = actor_name;
								pre_line.position = first_dest;
								pre_line.pose = pose_name;
								pre_line.arg_dict["wait time"] = "0.5";
								if (this_line.arg_dict.ContainsKey ("transition"))
								{
									pre_line.arg_dict["transition"] = this_line.arg_dict["transition"];
								}

								if (contexts.Count > 0)
								{
									most_recent_choice.line_bundle.Add (pre_line);
								}
								else 
								{
									Game.current.script.Add (pre_line);
								}
							}
						}

						Line new_line = new Line("");
						new_line.position = position;
						new_line.speaker = actor_name;
						new_line.pose = pose_name;

						if (first_key.Contains("background"))
						{
							//Debug.Log("Background line detected");
							new_line.position = "BG" + new_line.position;
						}
						if (this_line.arg_dict.ContainsKey ("wait time"))
						{
							new_line.arg_dict["wait time"] = this_line.arg_dict["wait time"];
						}
						if (this_line.arg_dict.ContainsKey ("transition"))
						{
							new_line.arg_dict["transition"] = this_line.arg_dict["transition"];
						}
						
						if (contexts.Count > 0)
						{
							most_recent_choice.line_bundle.Add (new_line);
						}
						else 
						{
							Game.current.script.Add (new_line);
						}
					}
					
					else if (first_key ==  "if")
					{
						// IF BLOCKS TREATED AS AUTO-NAMED BRANCHES
						int conditional_index = 0;
						string decision_key = "_conditional_0";
						while (Game.current.decision_dict.ContainsKey(decision_key))
						{
							conditional_index += 1;
							decision_key = "_conditional_" + conditional_index.ToString();
						}

						if (contexts.Count > 0)
						{
							most_recent_choice.line_bundle.Add (new Line("branch: " + decision_key));
						}
						else 
						{
							Game.current.script.Add (new Line("branch: " + decision_key));
						}

						conditional_branch_nest.Add(decision_key);
						Game.current.decision_dict.Add (decision_key, new Decision());
						most_recent_decision = Game.current.decision_dict[decision_key];
						most_recent_decision.name = decision_key;

						// reset contexts
						contexts.Add("choice");
						context_levels.Add(level);

						// store most recent objects
						most_recent_decision.choices.Add (new Choice());
						most_recent_choice = most_recent_decision.choices[most_recent_decision.choices.Count - 1];
						most_recent_choice.conditions.Add(this_line.arg_dict[first_key]);

						//Debug.Log("Creating new conditional called (" + decision_key + ") with first choice's conditionals as: " + this_line.arg_dict[first_key]);
					}
					else if (first_key == "else if")
					{
						most_recent_decision.choices.Add (new Choice());
						most_recent_choice = most_recent_decision.choices[most_recent_decision.choices.Count - 1];
						most_recent_choice.conditions.Add(this_line.arg_dict[first_key]);

						//Debug.Log("Adding to conditional called (" + most_recent_decision.name + ") with conditionals as: " + this_line.arg_dict[first_key]);

					}
					else if (first_key == "else" || line_text == "else")
					{
						most_recent_decision.choices.Add (new Choice());
						most_recent_choice = most_recent_decision.choices[most_recent_decision.choices.Count - 1];
						//Debug.Log("Adding default condition to conditional called (" + most_recent_decision.name + ")");

					}

					// if not line command, line attribute, conditional, or move/pose commands, then just store and interpret in real time
					else if (contexts.Count > 0)
					{
						most_recent_choice.line_bundle.Add (this_line);
					}
					else 
					{
						Game.current.script.Add (this_line);
					}
				}
				
				// lines that are all-caps names of characters change the speaker
				else if (line_text == "NARRATOR")
				{
					most_recent_speaker_name = "Narrator";
				}
				else if (line_text == "ACTIVE PLAYER")
				{
					most_recent_speaker_name = "$active_player.name";
				}
				else if (line_text.StartsWith ("PLAYER WITH") && line_text.Split().Length > 2 && (p_min_comparison_words.Contains(line_text.Split()[2]) || p_max_comparison_words.Contains(line_text.Split()[2])))
				{
					string comparative_word = line_text.Split()[2];
					int substring_start = 12 + comparative_word.Length;
					string cap_stat = line_text.Substring(substring_start).Trim();
					string code_start = "";
					if (p_max_comparison_words.Contains(comparative_word))
					{
						code_start = "$pMax.";
					}
					else
					{
						code_start = "pMin.";
					}

					if (Game.current.cap_stats.Contains (cap_stat))
					{
						most_recent_speaker_name = code_start + Game.current.get_stat_name_from_caps(cap_stat);
					}
					else
					{
						Debug.Log ("Stat '" + cap_stat + "' not found to assign speaker: " + line_text);
					}
				}
				else if (line_text.StartsWith("$") && Game.current.stat_dict.ContainsKey(line_text.Substring (1)))
				{
					most_recent_speaker_name = line_text;
				}

				else if (Game.current.cap_chars.Contains (line_text))
				{
					most_recent_speaker_name = Game.current.get_char_name_from_caps(line_text);
				}
				
				// Plain lines other than those above are spoken dialogue
				else
				{
					Line this_line = new Line(line_text);
					this_line.speaker = most_recent_speaker_name;
					
					if (contexts.Count > 0)
					{
						most_recent_choice.line_bundle.Add (this_line);
					}
					else 
					{
						Game.current.script.Add (this_line);
					}
					most_recent_line = this_line;
				}
        	}
        }
    }
				         
				         // Use this for initialization
    void Start ()
    {
		Game.current = new Game("default");
		
		dialogue_manager = GameObject.Find ("ScriptHolder").GetComponent<DialogueManager>();
		cutscene_manager = GameObject.Find ("ScriptHolder").GetComponent<CutsceneManager>();
		
		CutsceneManager.onCutscenesDone += progress;
		
		SFX_emitter = GetComponent<AudioSource>();
		Music_emitter = GameObject.Find ("BGMusic_holder").GetComponent<AudioSource>();
		
		
		for (int i=0; i < player_colors.Count; i++)
		{
			Character new_char = new Character("Player " + (i+1).ToString (), player_colors[i], new Dictionary<string, string>(), new Dictionary<string, Stat>());
			new_char.active = true;
			Game.current.players.Add (new_char);
			p_char_i.Add (-1);
		}
		
		Game.current.script.Add (new Line("Welcome to ScreenPlay, a multiplayer visual novel engine! Select a game."));
		Game.current.script.Add (new Line("decision: game_select"));

		// Set all players' activity to false
		
		progress ();
    }
				         
    IEnumerator wait_for_input(float delay_time)
 	{
		float timer = 0f;
		while (timer < delay_time)
		{
			if (Game.current.running)
			{
				timer += Time.deltaTime;
			}
			yield return null;
		}
		onScriptReady();
    }
	         
    IEnumerator wait_to_progress(float delay_time)
    {
		float timer = 0f;
		while (timer < delay_time)
		{
			if (Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}
		progress ();
    }
	         
    IEnumerator wait_to_load(float delay_time, Game game_to_load)
    {
		float timer = 0f;
		while (timer < delay_time)
		{
			if (Game.current.running)
			{
				timer += Time.deltaTime;
				
			}
			yield return null;
		}
		Game.current = game_to_load;
		load_game();
		dialogue_manager.show_uis ();
		dialogue_manager.showing_recap = false;
		progress ();
    }
	         
    IEnumerator wait_to_check_for_active_players(float delay_time)
    {
		float timer = 0f;
		while (timer < delay_time)
		{
			if (Game.current.running)
			{
				timer += Time.deltaTime;
			}
			yield return null;
		}
		check_for_active_players ();
    }
	         
    public int get_choice_i_in_decision_from_visible_i (int visible_choice_i)
    {
		for (int i=0; i < Game.current.current_decision.choices.Count; i++)
		{
			if (Game.current.current_choices[visible_choice_i] == Game.current.current_decision.choices[i])
			{
				return i;
			}
		}
		Debug.Log("<color=red>ERROR: Couldn't find visible choice index (" + visible_choice_i.ToString() + ") in current_decision.choices, which has " + Game.current.current_decision.choices.Count.ToString() + " elements.</color>");
		return 0;
    }
	         
    public void declare_allocations (List<List<int>> allocations) // allocation decisions don't progress script (nothing gets appended), so choices of this type must happen mid-script. If needing to make choices based on allocation, use "branch" after allocation decision
    {
		switch (Game.current.current_decision.choice_mode)
		{
		case "shop":
			onPlayerChoosing(false);
			
			int old_active_player = Game.current.stat_dict["_this_player"].int_val;
			for (int i=0; i<4; i++)
			{
				
				foreach (int choice_i in allocations[i])
				{
					foreach (string change in Game.current.current_choices[choice_i - 1].changes)
					{
						if (change.Substring (0,7) == "players")
						{
							string this_change = "$active_player" + change.Substring (7);
							for (int j=1; j<=4; j++)
							{
								if (Game.current.players[j-1].active)
								{
									Game.current.stat_dict["_this_player"].int_val = j;
									make_change(this_change);
								}
							}
						}
						else
						{
							Game.current.stat_dict["_this_player"].int_val = i+1;
							make_change(change);
						}
					}
				}
			}
			Game.current.stat_dict["_this_player"].int_val = old_active_player;
			
			progress();
			
			break;
		case "lottery":
			List<int> spinner_list = new List<int>();
			
			for (int i=0; i < Game.current.current_decision.choices.Count; i++)
			{
				spinner_list.Add (0);			
				for (int j=0; j < allocations.Count; j++)
				{
					spinner_list[i] += allocations[j].FindAll(item => item == i+1).Count;
				}
			}
			for (int i=0; i < Game.current.current_decision.fail_choices.Count; i++)
			{
				spinner_list.Add (Game.current.current_decision.fail_bids[i]);
			}
			
			dialogue_manager.make_spinner(spinner_list);
			dialogue_manager.spin (Random.Range(90f,110f));
			onPauseInput(7f);
			
			break;
		}
		
    }
	         
    public void declare_answers (List<int> answer_list)
    {
		int my_choice = 0;
		
		bool ready_to_choose = true;

		int old_active_player = Game.current.stat_dict["_this_player"].int_val;
		
		for (int i=0; i<4; i++)
		{
			if (answer_list[i] > 0)
			{
				foreach (string change in Game.current.current_choices[answer_list[i] - 1].changes)
				{
					
					if (change.Substring (0,7) == "players")
					{
						string this_change = "$active_player" + change.Substring (7);
						for (int j=1; j<=4; j++)
						{
							if (Game.current.players[j-1].active)
							{
								Game.current.stat_dict["_this_player"].int_val = j;
								make_change(this_change);
							}
						}
					}
					else
					{
						Game.current.stat_dict["_this_player"].int_val = i+1;
						Debug.Log ("making change for player " + (i+1).ToString () + ": " + change);
						make_change(change);
					}
				}
			}
		}
		Game.current.stat_dict["_this_player"].int_val = old_active_player;
		
		
		if (Game.current.current_decision.choice_mode == "vote" || Game.current.current_decision.choice_mode == "hidden vote")
		{
			List<int> count_list = new List<int>();
			for (int i=1; i<=Game.current.current_choices.Count; i++)
			{
				count_list.Add (answer_list.FindAll(item => item == i).Count);
				
			}
			
			int count_max = 0;
			for (int i=0; i<count_list.Count; i++)
			{
				if (count_list[i] > count_max)
				{
					count_max = count_list[i];
				}
			}
			if (count_list.FindAll (item => item == count_max).Count > 1)
			{
				ready_to_choose = false;
				List<int> spinner_list = new List<int>();
				
				for (int i=0; i<count_list.Count; i++)
				{
					if (count_list[i] == count_max)
					{
						spinner_list.Add (1);
					}
					else
					{
						spinner_list.Add (0);
					}
				}
				dialogue_manager.make_spinner(spinner_list);
				dialogue_manager.spin (Random.Range(90f,110f));
			}
			else
			{
				my_choice = count_list.FindIndex(item => item == count_max);
			}
		}
		else if (Game.current.current_decision.choice_mode == "first")
		{
			my_choice = answer_list.Find (item => item > 0) -1;
			command_player = answer_list.IndexOf (my_choice + 1);
		}
		else if (Game.current.current_decision.choice_mode == "agreement")
		{
			my_choice = answer_list.Find (item => item > 0) -1;
		}
		else if (Game.current.current_decision.choice_mode == "poll" || Game.current.current_decision.choice_mode == "hidden poll")
		{
			onPlayerChoosing(false);
			ready_to_choose = false;
			
			for (int i=3; i>=0; i--)
			{
				if (answer_list[i] > 0)
				{
					insert_lines(i+1, Game.current.current_choices[answer_list[i] - 1].line_bundle);
				}
			}
			progress();
		}
		else if (Game.current.current_decision.choice_mode == "spinner" || Game.current.current_decision.choice_mode == "hidden spinner")
		{
			List<int> stripped_choices = answer_list.FindAll(item => item > 0);
			for (int i=0; i<stripped_choices.Count; i++)
			{
				for (int j=0; j<i; j++)
				{
					if (stripped_choices[j] != stripped_choices[i])
					{
						ready_to_choose = false;
						break;
					}
				}
			}
			if (ready_to_choose)
			{
				my_choice = stripped_choices[0] -1;
			}
			else
			{
				List<int> spinner_list = new List<int>();
				for (int i=0; i < Game.current.current_decision.choices.Count; i++)
				{
					spinner_list.Add(stripped_choices.FindAll (item => item == i+1).Count);
				}
				dialogue_manager.make_spinner(spinner_list);
				dialogue_manager.spin (Random.Range(90f, 110f));
			}
		}
		
		if (ready_to_choose)
		{
			pick (Game.current.current_choices[my_choice]);
		}
		else if (Game.current.current_decision.choice_mode != "poll" && Game.current.current_decision.choice_mode != "hidden poll")
		{
			onPauseInput(7f);
		}
    }
	         
    void process_branch (Decision this_decision)
    {
		if (this_decision.choice_mode == "per player")
		{
			int old_active_player = Game.current.stat_dict["_this_player"].int_val;
			for (int i=3; i>=0; i--)
			{
				if (Game.current.players[i].active)
				{
					foreach (Choice choice in this_decision.choices)
					{
						Game.current.stat_dict["_this_player"].int_val = i+1;
						
						if (all_true(choice.conditions))
						{
							//Debug.Log ("Going with choice that has " + choice.conditions.Count.ToString () + " conditions");
							Game.current.stat_dict["_this_player"].int_val = old_active_player;
							
							insert_lines(i+1, choice.line_bundle);
							break;
						}
					}
				}
			}
			
			progress ();
		}
		else
		{
			bool choice_found = false;
			foreach (Choice choice in this_decision.choices)
			{
				if (all_true(choice.conditions))
				{
					pick (choice);
					choice_found = true;
					break;
				}
			}
			if (!choice_found)
			{
				progress();
			}
		}
    }
	         
    public string plug_in_portrait_name (string my_name, string my_pose, string pos = "")
    {
		string portrait_name = my_pose;
		string alt_portrait_name = "";
		if (pos != "")
		{
			alt_portrait_name = my_pose + "_" + pos;
		}

        Debug.Log("Alt_portrait_name: " + alt_portrait_name);
		
		if (Game.current.char_dict.ContainsKey(my_name) && Game.current.char_dict[my_name].portraits.ContainsKey(alt_portrait_name))
		{
			portrait_name = Game.current.char_dict[my_name].portraits[alt_portrait_name];
		}
		else if (Game.current.char_dict.ContainsKey(my_name) && Game.current.char_dict[my_name].portraits.ContainsKey(my_pose))
		{
			portrait_name = Game.current.char_dict[my_name].portraits[my_pose];
		}
		else
		{
			if (!Game.current.char_dict.ContainsKey(my_name))
			{
				Debug.Log ("Couldn't find character: " + my_name + " in Game.current.char_dict. Using portrait named: " + my_pose);
				dialogue_manager.set_typein_pitch(1f);
			}
			else if (!Game.current.char_dict[my_name].portraits.ContainsKey(my_pose))
			{
				Debug.Log ("Couldn't find pose: " + my_pose + " in portrait dictionary of Character: " + my_name);
			}
		}
		return portrait_name;
    }
	         
    public void drop_in_player(int player_i)
    {
		//active_players[player_i-1] = true;
		Game.current.stat_dict["_player_count"].int_val += 1;
		dialogue_manager.show_ui(player_i);
		dialogue_manager.collapse_ui(player_i);
		
		if (Game.current.script[Game.current.script_pos].text != "Player joining...")
		{
			Line joining_line = new Line("Player joining...");
			joining_line.speaker = "Narrator";
			if (Game.current.get_current_line().text == "character_select")
			{
				Game.current.script.Insert (Game.current.script_pos + 1, joining_line);
			}
			else
			{
				Game.current.script.Insert (Game.current.script_pos, joining_line);
				Game.current.script_pos -= 1;
			}

			progress ();
		}
		
		onPlayerChoosing(false);
		
		p_char_i[player_i -1] = -1;
		swap_p_char(1, player_i);
		
		//dialogue_manager.hide_tutorial_box();
		check_for_active_players();
		dialogue_manager.set_ui_font();
		
		onPauseInput(0.5f);
    }
	         
    public int next_available_char_i (int curr_i)
    {
		for (int i = curr_i; i < Game.current.p_chars_available.Count; i++)
		{
			if (!p_char_i.Contains (i))
			{
				return i;
			}
		}
		for (int i = 0; i < curr_i; i++)
		{
			if (!p_char_i.Contains (i))
			{
				return i;
			}
		}
		return curr_i;
    }
	         
    public int prev_available_char_i (int curr_i)
    {
		for (int i = curr_i - 1; i >= 0; i--)
		{
			if (!p_char_i.Contains (i))
			{
				return i;
			}
		}
		for (int i = Game.current.p_chars_available.Count - 1; i > curr_i; i--)
		{
			if (!p_char_i.Contains (i))
			{
				return i;
			}
		}
		
		return curr_i;
    }
     
    public void swap_p_char(int step, int player_i)
    {
		if (step > 0)
		{
			p_char_i[player_i - 1] = next_available_char_i(p_char_i[player_i - 1]);
		}
		else if (step < 0)
		{
			p_char_i[player_i - 1] = prev_available_char_i(p_char_i[player_i - 1]);
		}
		
		Character next_char = Game.current.char_dict[Game.current.p_chars_available[p_char_i[player_i -1]]];
		Game.current.players[player_i - 1].portraits = next_char.portraits;
		Game.current.players[player_i - 1].name = next_char.name;
		Game.current.players[player_i - 1].set_color(next_char.get_color());
		Game.current.players[player_i - 1].greeting = next_char.greeting;
		//players[player_i - 1].stat_dict = new Dictionary<string, Stat>();
		
		// SHOW PAUSE PORTRAIT
		dialogue_manager.show_pause_portrait(player_i - 1, Game.current.get_file_path("portraits/" + next_char.portraits["normal"]), next_char.portraits["normal"]);
		
		foreach (Stat default_stat in Game.current.default_stats)
		{
			Game.current.players[player_i - 1].stat_dict[default_stat.slug] = default_stat.make_copy();
		}
		
		foreach (string stat_name in Game.current.char_dict[Game.current.p_chars_available[p_char_i[player_i -1]]].stat_dict.Keys)
		{
			Game.current.players[player_i - 1].stat_dict[stat_name] = Game.current.char_dict[Game.current.p_chars_available[p_char_i[player_i -1]]].stat_dict[stat_name];
		}
		dialogue_manager.update_player_ui (player_i, Game.current.p_chars_available[p_char_i[player_i -1]], Game.current.char_select_stats);
    }
     
    public void select_p_char (int player_i)
    {
		Game.current.players[player_i-1].active = true;
		//Game.current.stat_dict["_this_player"].int_val = player_i;
		
		dialogue_manager.collapse_ui(player_i);
		//dialogue_manager.clear_player_speaker();
		
		Game.current.p_chars_available.Remove(Game.current.p_chars_available[p_char_i[player_i -1]]);
		for (int i = 0; i < p_char_i.Count; i++)
		{
			if (p_char_i[i] > p_char_i[player_i - 1])
			{
				p_char_i[i] -= 1; // Shifts current positon of player character selectors down 1 since someone was removed from the available list
			}
		}
		p_char_i[player_i - 1] = -1;
		dialogue_manager.hide_pause_portrait(player_i - 1);
		
		Line greeting_line = new Line(Game.current.players[player_i - 1].greeting);
		greeting_line.speaker = Game.current.players[player_i - 1].name;
		Game.current.script.Insert (Game.current.script_pos, greeting_line);
		Game.current.script_pos -= 1;
		
		//progress ();
		// PLAY THROUGH GREETING LINES OF ALL NEWLY-SELECTED CHARACTERS
		progress ();
		onPauseInput(2f);
		StartCoroutine(wait_to_progress(1.5f));
		if (Mathf.Max (p_char_i.ToArray()) == -1)
		{
			StartCoroutine(wait_to_progress(1.51f));
		}
    }
     
    public void reactivate_p_char (int player_i)
    {
		//Debug.Log ("reactivating player");
		Game.current.stat_dict["_player_count"].int_val += 1;
		Game.current.players[player_i-1].active = true;
		dialogue_manager.update_player_ui (player_i, Game.current.players[player_i - 1].name, new List<string>());
		dialogue_manager.show_ui(player_i);

		//dialogue_manager.hide_tutorial_box();
		check_for_active_players();

		dialogue_manager.collapse_ui(player_i);
		//p_chars_available.Remove(players[player_i - 1].name);
		dialogue_manager.clear_player_speaker();
		
		if (Game.current.player_names.Contains (Game.current.players[player_i - 1].name))
		{
			Line greeting_line = new Line(Game.current.players[player_i - 1].greeting);
			greeting_line.speaker = Game.current.players[player_i - 1].name;
			insert_lines_and_retrace(player_i, new List<Line>{greeting_line,});
			
			onPauseInput(2f);
			progress ();
			 
			StartCoroutine(wait_to_progress(1.5f));
		}
		//Game.current.script_pos -= 1;
		
    }
     
    public void drop_out_player(int player_i)
    {
		Game.current.stat_dict["_player_count"].int_val -= 1;
		dialogue_manager.collapse_ui(player_i);
		dialogue_manager.hide_ui(player_i);
		
		if (Game.current.players[player_i-1].active && !Game.current.players[player_i-1].name.StartsWith ("Player"))
		{
			if (Game.current.can_select_character)
			{
				Game.current.p_chars_available.Add (Game.current.players[player_i - 1].name);
			}
			
			Line leaving_line = new Line("I have to go!");
			leaving_line.speaker = Game.current.players[player_i -1].name;
			
			insert_lines_and_retrace(player_i, new List<Line>{leaving_line,});
			
			progress ();
			onPauseInput(2f);
			StartCoroutine(wait_to_progress(1.5f));
		}
		else
		{
			if (!Game.current.players[player_i-1].name.StartsWith ("Player"))
			{
				dialogue_manager.hide_pause_portrait(player_i - 1);
				Game.current.players[player_i - 1].name = "Player " + player_i.ToString ();
				
			}
			p_char_i[player_i - 1] = -1;
			
			onPlayerChoosing(false);
		}
		
		Game.current.players[player_i-1].active = false;
		
		check_for_active_players();
    }
     
    public void insert_lines_and_retrace (int active_p, List<Line> line_bundle)
    {
		int old_active_player = Game.current.stat_dict["_this_player"].int_val;
		
		Game.current.script.Insert (Game.current.script_pos, new Line("$_this_player = " + old_active_player.ToString ()));
		Game.current.script.InsertRange (Game.current.script_pos, line_bundle);
		Game.current.script.Insert (Game.current.script_pos, new Line("$_this_player = " + active_p.ToString ()));
		Game.current.script_pos -= 1;
		
    }
     
    public void insert_lines (int active_p, List<Line> line_bundle)
    {
		int old_active_player = Game.current.stat_dict["_this_player"].int_val;
		
		if (Game.current.script_pos < Game.current.script.Count - 1)
		{
			Game.current.script.Insert (Game.current.script_pos + 1, new Line("$_this_player = " + old_active_player.ToString ()));
			Game.current.script.InsertRange (Game.current.script_pos + 1, line_bundle);
			Game.current.script.Insert (Game.current.script_pos + 1, new Line("$_this_player = " + active_p.ToString ()));
		}
		else
		{
			Game.current.script.Add (new Line("$_this_player = " + active_p.ToString ()));
			Game.current.script.AddRange (line_bundle);
			Game.current.script.Add (new Line("$_this_player = " + old_active_player.ToString ()));
		}
		
    }
     
    public bool is_player_name(string test_phrase)
    {
		return Game.current.player_names.Contains (test_phrase);
    }
     
    public int player_number_from_name(string test_phrase)
    {
		List<string> current_player_names = new List<string>();
		foreach (Character my_player in Game.current.players)
		{
			current_player_names.Add (my_player.name);
		}
		
		if (current_player_names.Contains (test_phrase))
		{
			return current_player_names.IndexOf (test_phrase) + 1;
			
		}
		else
		{
			return 0;
		}
    }
     
    public void declare_pause (float duration)
    {
		onPauseInput(duration);
    }
     
    public bool safe_to_progress()
    {
		return Game.current.script.Count > Game.current.script_pos + 1;
    }
     
    public void process_dialogue_extras (Line this_line)
    {
		if (this_line.arg_dict.ContainsKey("bg"))
		{
			string new_scene_name = this_line.arg_dict["bg"];
			dialogue_manager.set_background(new_scene_name);
			Game.current.background_showing = new_scene_name;
		}

		string interpreted_name = plug_in_variables(plug_in_player_i(this_line.speaker));
		if (Game.current.char_dict.ContainsKey (interpreted_name))
		{
			dialogue_manager.set_typein_pitch (Game.current.char_dict[interpreted_name].voice_pitch);
		}
		
		if (this_line.sound != "")
		{
			Debug.Log ("Playing line sound");
			SFX_emitter.clip = Resources.Load (Game.current.get_file_path("sounds/" + this_line.sound.Trim ()), typeof(AudioClip)) as AudioClip;
			SFX_emitter.Play ();
			dialogue_manager.play_typein_noise = false;
		}
		
		if (this_line.speaker == "Narrator")
		{
			dialogue_manager.set_color_mode("nar");
		}
		else
		{
			dialogue_manager.set_color_mode("char");
		}
		
		if (!dialogue_manager.speech_box_is_showing())
		{
			dialogue_manager.show_speech_box();
		}
		
		if (pause_all_lines || this_line.pause) // line is important
		{
			onImportanceSet(true);
		}
		else
		{
			onImportanceSet(false);
		}
		
		dialogue_manager.hide_approvals();
		
		if (!(dialogue_manager.use_type_in && dialogue_manager.color_mode == "char"))
		{
			dialogue_manager.show_approvals();
		}
		
		dialogue_manager.set_interrupt_text(this_line.interrupt_message);
		
		if (this_line.interrupt_label != "")
		{
			//Debug.Log ("Attempting to display interrupt with scene goto: " + this_line.interrupt_label);
			Game.current.interrupt_choice = Game.current.choice_dict[this_line.interrupt_label];
		}
    }
     
    public void register_plot_point (Line line_to_register)
    {
		Line this_line = new Line(line_to_register.text);
		Game.current.plot_summary.Add (this_line);
		this_line.arg_dict["bg"] = Game.current.background_showing;
		this_line.pose = dialogue_manager.get_speaker_pose();
		this_line.position = dialogue_manager.get_char_pos();
		this_line.sound = line_to_register.sound;
		
		if (this_line.speaker == "")
		{
			if (Game.current.current_speaker_name == "")
			{
				this_line.speaker = "Narrator";
			}
			else
			{
				this_line.speaker = Game.current.current_speaker_name;
			}
		}
		else
		{
			this_line.speaker = line_to_register.speaker;
		}
		
    }
     
    public void show_plot_summary ()
    {
		if (Game.current.show_plot_summaries)
		{
			float time_per_line = 3f;
			
			dialogue_manager.show_tutorial_box("Previously...");
			
			if (Game.current.plot_summary.Count == 0)
			{
				Line new_line = new Line("The story begins...");
				new_line.speaker = "Narrator";
				Game.current.script.Insert (Game.current.script_pos + 1, new_line);
				onPauseInput(time_per_line);
				progress ();
				StartCoroutine(wait_to_progress(time_per_line));
				StartCoroutine(wait_to_check_for_active_players(time_per_line));
			}
			else
			{
				dialogue_manager.showing_recap = true;
				Game pre_summary_game = Game.current.make_copy();
				
				dialogue_manager.clear_all();
				
				int lines_to_show = Game.current.plot_summary.Count;
				if (lines_to_show > 5)
				{
					lines_to_show = 5;
				}
				
				onPauseInput(lines_to_show * time_per_line);
				
				Game.current.script.InsertRange(Game.current.script_pos + 1, Game.current.plot_summary);
				
				for (int i=1; i <= lines_to_show; i++)
				{
					StartCoroutine(wait_to_progress(time_per_line * (i-1)));
				}
				
				StartCoroutine(wait_to_load(time_per_line * lines_to_show, pre_summary_game));
				StartCoroutine(wait_to_check_for_active_players(time_per_line * lines_to_show));
			}
		}
		else
		{
			progress();
		}
    }
     
    public void check_for_active_players()
    {
		if (Game.current.players.FindAll(item => item.active == true).Count + p_char_i.FindAll(item => item >= 0).Count < player_count_min)
		{
			string join_message = "Press any button to join";
			if (player_count_min > 1)
			{
				join_message += ". At least " + player_count_min.ToString() + " required.";
			}
			dialogue_manager.show_tutorial_box(join_message);
		}
		else
		{
			dialogue_manager.hide_tutorial_box();
		}
    }
     
    public void populate_save_files (string menu_type)
    {
		Game.current.decision_dict[menu_type + "_game_list"].choices.Clear ();
		
		for (int i = 0; i < SaveLoad.savedGames.Count; i++)
		{
			Game saved_game = SaveLoad.savedGames[i];
			Game.current.decision_dict[menu_type + "_game_list"].choices.Add (new Choice(saved_game.get_save_label (), new List<Line>{new Line(menu_type + "_game " + i.ToString ())}, new List<string>(), new List<string>()));
		}
		
		if (Game.current.current_decision == Game.current.decision_dict["pause_menu"])
		{
			Game.current.decision_dict[menu_type + "_game_list"].choices.Add (new Choice("Back", new List<Line>{new Line("decision: pause_menu")}, new List<string>(), new List<string>()));
		}
		else
		{
			Game.current.decision_dict[menu_type + "_game_list"].choices.Add (new Choice("Back", new List<Line>{new Line("decision: new/load/back")}, new List<string>(), new List<string>()));
		}
    }
     
    void delay_for_effect (Line this_line)
    {
		float wait_time = 1f;
		if (this_line.arg_dict.ContainsKey("wait time"))
		{
			wait_time = float.Parse(this_line.arg_dict["wait time"]);
		}
		else if (this_line.arg_dict.ContainsKey("duration"))
		{
			wait_time = float.Parse(this_line.arg_dict["duration"]);
		}
		if (wait_time > 0f)
		{
			declare_pause(wait_time);
		}
		StartCoroutine(wait_to_progress(wait_time));
    }
     
    public string stage_position_word_to_abbrev(string word)
    {
		switch (word)
		{
		case "left":
			return "L";
		case "right":
			return "R";
		case "center":
			return "C";
		case "L":
			return "L";
		case "R":
			return "R";
		case "C":
			return "C";
		}
		if (word.StartsWith("BG"))
		{
			return word;
		}
		else
		{
			return "L";
		}
    }

	public void start_character_selection(int player_i)
	{
		Game.current.players[player_i-1].active = false;

		Game.current.stat_dict["_player_count"].int_val -= 1;
		if (Game.current.can_select_character && !Game.current.players[player_i-1].name.StartsWith("Player"))
		{
			Game.current.p_chars_available.Add (Game.current.players[player_i - 1].name);

		}

		drop_in_player(player_i);

		onChooseCharacter(player_i);
	}
     
    public void try_lock_password (string code1, string code2)
    {
		if (code1 == code2)
		{
			dialogue_manager.hide_tutorial_box();
			Game.current.password = code1;
			dialogue_manager.hide_password_box();
			commit_save ();
			
			Line confirmation_info = new Line("Save successful. Your game ID is <color=red><b>" + Game.current.ID.ToString () + "</b></color>.");
			confirmation_info.speaker = "Narrator";
			Game.current.script.Insert (Game.current.script_pos + 1, confirmation_info);
			progress ();
		}
		
		else
		{
			dialogue_manager.show_tutorial_box("Passwords did not match. Enter a password.");
			Game.current.script.Insert (Game.current.script_pos + 1, new Line("set_password"));
			progress ();
		}
    }
     
    public void try_password (string code)
    {
		if (password_mode == "load")
		{
			if (SaveLoad.try_password(savegame_index, code))
			{
				Game.current.script.Insert (Game.current.script_pos + 1, new Line("unlock_game"));
				progress ();
			}
			else
			{
				Game.current.current_decision = Game.current.decision_dict["pause_menu"];
				//populate_save_files("load");
				
				Game.current.script.Insert (Game.current.script_pos + 1, new Line("decision: load_game_list"));
				progress ();
			}
		}
		else if (password_mode == "delete")
		{
			if (SaveLoad.try_password(savegame_index, code))
			{
				Game.current.script.Insert (Game.current.script_pos + 1, new Line("delete_current_game"));
				progress ();
			}
			else
			{
				//Game.current.current_decision = Game.current.decision_dict["pause_menu"];
				//populate_save_files("delete");
				
				Game.current.script.Insert (Game.current.script_pos + 1, new Line("decision: delete_game_list"));
				progress ();
			}
		}
		
		dialogue_manager.hide_password_box();
    }
     
    public void commit_save ()
    {
		SaveLoad.Save ();
		SFX_emitter.clip = dialogue_manager.sound_dict["save"];
		SFX_emitter.Play ();
		onCanPause();
		dialogue_manager.set_pause_portrait_visibility(false);
		
		progress ();
    }
     
    public void set_running (bool new_val)
    {
		Game.current.running = new_val;
    }

	public void set_music (string song_name, bool play_once = false)
	{
		if (Music_emitter != null)
		{
			if (song_name == "none" || song_name == "off")
			{
				Music_emitter.Stop();
				Game.current.music_playing = "";

			}
			else 
			{
				Music_emitter.clip = Resources.Load (Game.current.get_file_path("music/" + song_name), typeof(AudioClip)) as AudioClip;
				Music_emitter.Play ();
				Game.current.music_playing = song_name;
				if (play_once)
				{
					Music_emitter.loop = false;
				}
				else
				{
					Music_emitter.loop = true;
				}
			}
		}

	}

	void update_or_add_bg_portrait_records (string target_pos, string interpreted_name, string pic_name)
	{
		if (target_pos.StartsWith("BGL"))
		{
			int bg_i = 0;
			if (target_pos.Length > 3 && int.Parse(target_pos.Substring(3)) < Game.current.bg_actors_left.Count)
			{
				bg_i = int.Parse(target_pos.Substring(3));
			}
			else
			{
				bg_i = Game.current.bg_actors_left.Count;
				Game.current.bg_actors_left.Add("");
				Game.current.bg_poses_left.Add("");
			}
			Game.current.bg_poses_left[bg_i] = pic_name;
			Game.current.bg_actors_left[bg_i] = interpreted_name;
		}
		else if (target_pos.StartsWith("BGR"))
		{
			int bg_i = 0;
			if (target_pos.Length > 3 && int.Parse(target_pos.Substring(3)) < Game.current.bg_actors_right.Count)
			{
				bg_i = int.Parse(target_pos.Substring(3));
			}
			else
			{
				bg_i = Game.current.bg_actors_right.Count;
				Game.current.bg_actors_right.Add("");
				Game.current.bg_poses_right.Add("");
			}
			Game.current.bg_poses_right[bg_i] = pic_name;
			Game.current.bg_actors_right[bg_i] = interpreted_name;
		}
		else
		{
			int pos_i = dialogue_manager.get_speaker_i(target_pos);

			Game.current.stage_poses[pos_i] = pic_name;
			Game.current.stage_actors[pos_i] = interpreted_name;
		}
	}
     
    public void progress()
    {
		Game.current.script_pos ++;
		if (Game.current.script_pos < Game.current.script.Count)
		{
			Line this_line = Game.current.script[Game.current.script_pos];
			
			Debug.Log (Game.current.script_pos.ToString() + "/" + Game.current.script.Count.ToString () + " " + this_line.speaker + ", " + this_line.position + ", " + this_line.pose + ": " + this_line.text);
			
			if (this_line.speaker != "") // someone must be talking
			{
				process_dialogue_extras(this_line);
				
				string interpreted_name = plug_in_variables(plug_in_player_i(this_line.speaker));
				string target_pos = stage_position_word_to_abbrev(this_line.position);
				string old_pos = dialogue_manager.get_char_pos(interpreted_name);
				//Debug.Log("Interpreted name is: " + interpreted_name + " at position: " + old_pos);

				if (this_line.position == "")
				{
					target_pos = old_pos;
					if (target_pos == "")
					{
						target_pos = "L";
					}
				}
				
				string my_pose = "";
				
				if (interpreted_name != "Narrator")
				{
					if (this_line.pose == "")
					{
						if (is_player_name (interpreted_name))
						{
							my_pose = Game.current.players[player_number_from_name(interpreted_name)].pose_for_player;
							if (my_pose == "")
							{
								my_pose = "normal";
							}
						}
						else if (dialogue_manager.get_char_pos(interpreted_name) == "")
						{
							my_pose = "normal";
						}
					}
					else
					{
						my_pose = this_line.pose;
						if (is_player_name(interpreted_name))
						{
							Game.current.players[player_number_from_name(interpreted_name)].pose_for_player = my_pose;
						}
					}
					
					if (interpreted_name != Game.current.current_speaker_name || target_pos != old_pos)
					{
						dialogue_manager.new_name (interpreted_name, target_pos);
					}
				}
				
				if (is_player_name (interpreted_name))
				{
					dialogue_manager.player_message (interpreted_name,target_pos, plug_in_portrait_name(interpreted_name, my_pose, target_pos.Substring (1,1)), plug_in_variables(this_line.text));
					onPauseInput(dialogue_manager.message_transition_time * 2);
					Game.current.current_speaker_name = interpreted_name;
				}
				else
				{
					int pos_i = dialogue_manager.get_speaker_i(target_pos);
					string transition_type = "";
					if (this_line.arg_dict.ContainsKey("transition"))
					{
						transition_type = this_line.arg_dict["transition"];
					}

					if (dialogue_manager.get_char_pos(interpreted_name) != "" && dialogue_manager.get_char_pos(interpreted_name) != target_pos) // character sliding
					{
						string pic_name = "normal";
						if (my_pose != "")
						{
							pic_name = plug_in_portrait_name(interpreted_name, my_pose, target_pos);
							dialogue_manager.set_portrait(target_pos, pic_name, old_pos, interpreted_name, transition: transition_type);
						}
						else
						{
							dialogue_manager.set_portrait(target_pos, "", old_pos, interpreted_name, transition: transition_type);
						}

						// bookkeeping: Remove old
						if (dialogue_manager.get_char_pos(interpreted_name).StartsWith("BG"))
						{
							if (dialogue_manager.get_char_pos(interpreted_name).Substring(2,1) == "L")
							{
								int removal_index = Game.current.bg_actors_left.IndexOf(interpreted_name);
								Game.current.bg_actors_left.RemoveAt(removal_index);
								Game.current.bg_poses_left.RemoveAt(removal_index);
							}
							else if (dialogue_manager.get_char_pos(interpreted_name).Substring(2,1) == "R")
							{
								int removal_index = Game.current.bg_actors_right.IndexOf(interpreted_name);
								Game.current.bg_actors_right.RemoveAt(removal_index);
								Game.current.bg_poses_right.RemoveAt(removal_index);
							}
						}
						else
						{
							Game.current.stage_actors[dialogue_manager.get_speaker_i(old_pos)] = "";
						}

						// bookkeeping: Add new
                        update_or_add_bg_portrait_records(target_pos, interpreted_name, my_pose == "" ? "normal" : my_pose);
					}
					else if (my_pose != "")
					{
						string pic_name = plug_in_portrait_name(interpreted_name, my_pose, target_pos);
						dialogue_manager.set_portrait(target_pos, pic_name, old_pos, transition: transition_type);

						// bookkeeping
                        update_or_add_bg_portrait_records(target_pos, interpreted_name, my_pose == "" ? "normal" : my_pose);
					}
					
					if (interpreted_name != "" && interpreted_name != "Narrator")
					{
						if (!target_pos.StartsWith("BG"))
						{
							Game.current.stage_actors[pos_i] = interpreted_name;

						}
						Game.current.current_speaker_name = interpreted_name;
					}
					else
					{
						Game.current.current_speaker_name = "";
					}
					
					dialogue_manager.new_message (plug_in_variables(this_line.text), target_pos);
				}
				
				if (this_line.important)
				{
					register_plot_point(this_line);
				}

				float wait_time;
				if (this_line.arg_dict.ContainsKey ("wait time") && float.TryParse(this_line.arg_dict["wait time"], out wait_time))
				{
					onPauseInput(wait_time + 0.1f);
					StartCoroutine(wait_to_progress(wait_time));
				}
				else if (this_line.text.Trim () == "")
				{
					progress ();
				}
				else
				{
					StartCoroutine(wait_for_input(0.5f));
					
				}
			}
			else // must be a command: choice CHOICE_NAME, hide, show, clear, clear CHARACTER_NAME, scene BG_IMAGE_NAME
			{
				dialogue_manager.set_interrupt_text("");

				if (this_line.text.Split (',')[0].Contains (":"))
				{
					string var_name = this_line.arg_dict["_first_key"];
					string val_string = this_line.arg_dict[var_name];

					//Debug.Log ("var_name: (" + var_name + ") val_string: " + val_string);
					if (command_assign_words.Contains(var_name))
					{
						if (var_name.Contains("font") && var_name.Split()[1] == "font")
						{
							dialogue_manager.parse_font(var_name.Split ()[0], val_string);
							progress ();
						}
						else
						{
							switch (var_name)
							{
							case "character available":
								if (!Game.current.player_names.Contains(val_string))
								{
									Game.current.player_names.Add(val_string);
								}
								if (Game.current.char_dict.ContainsKey(val_string) && !Game.current.p_chars_available.Contains(val_string))
								{
									Game.current.p_chars_available.Add(val_string);
								}
								progress();
								break;
							case "character unavailable":
								if (Game.current.p_chars_available.Contains(val_string))
								{
									Game.current.p_chars_available.Remove(val_string);
								}
								progress();

								break;
							case "show player stats":
								Game.current.default_stats_to_show.Clear();

								if (val_string.Trim() != "")
								{
									foreach (string stat_name in val_string.Split(';'))
									{
										Game.current.grab_player_default_stat_and_alias(stat_name.Trim());
									}

								}
								dialogue_manager.collapse_guis();
								progress();
								break;
							case "show player pause stats":
								Game.current.pause_player_stats.Clear();

								if (val_string.Trim() != "")
								{
									foreach (string stat_name in val_string.Split(';'))
									{
										Game.current.grab_player_pause_stat_and_alias(stat_name.Trim());
									}

								}
								progress();
								break;
							case "show game stats":

								if (val_string.Trim() != "")
								{
									Game.current.pause_stats.Clear();
									Game.current.pause_stat_aliases.Clear();

									foreach (string stat_name in val_string.Split(';'))
									{
										Game.current.grab_pause_stat_and_alias(stat_name.Trim());
									}

								}
								dialogue_manager.update_game_stat_ui();
								Game.current.game_stats_showing = true;
								progress();
								break;
							case "_commander":
								int.TryParse(val_string, out command_player);
								
								progress ();
								break;
							case "font":
								dialogue_manager.parse_font("", val_string);
								progress ();
								break;
							case "settings":
								string game_name = find_variable(val_string);
								dialogue_manager.load_settings(game_name);
								Debug.Log ("loading settings for game: " + game_name);
								
								progress ();
								break;
							case "chapter":
								string new_chapter_name = find_variable(val_string);
								Game.current.chapter_name = new_chapter_name;
								
								progress ();
								break;
							case "tutorial message":
								string message = find_variable(val_string);
								dialogue_manager.show_tutorial_box(message);
								
								progress();
								break;
							case "background":
								string new_scene_name = find_variable(val_string);
								//Debug.Log ("switching background to: " + new_scene_name);
								dialogue_manager.set_background(new_scene_name);
								Game.current.background_showing = new_scene_name;
								
								onPauseInput(0.75f);
								StartCoroutine(wait_to_progress(0.5f));
								break;
							case "music":
								set_music(val_string);

								progress ();
								break;
							case "sound":
								//Debug.Log ("Playing sound from command");
								SFX_emitter.clip = Resources.Load (Game.current.get_file_path("sounds/" + val_string), typeof(AudioClip)) as AudioClip;
								SFX_emitter.Play ();
								if (this_line.arg_dict.ContainsKey ("wait time"))
								{
									StartCoroutine(wait_to_progress (float.Parse (this_line.arg_dict["wait time"])));
								}
								else
								{
									StartCoroutine(wait_to_progress (SFX_emitter.clip.length));

									//progress ();
								}
								break;
							case "scene":
								string choice_key = find_variable(val_string);
								
								if (Game.current.choice_dict.ContainsKey (choice_key))
								{
									pick (Game.current.choice_dict[choice_key]);
								}
								else
								{
									Debug.Log ("GOTO key: " + choice_key + " NOT FOUND in choice_dict");
								}
								break;
							case "branch":
								string branch_name = find_variable(val_string);
								
								process_branch(Game.current.decision_dict[branch_name]);
								break;
							case "decision": // Make Line class and check Line.text instead. Spoken lines will have Line.important, etc. while decisions/branches/sections will just have their type + label
								string decision_name = find_variable(val_string).Trim ();
								//Debug.Log ("Looking for decision:" + decision_name);
								if (Game.current.decision_dict.ContainsKey(decision_name))
								{
									if (decision_name == "pause_menu")
									{
										dialogue_manager.show_pause_stats();
									}
									
									Game.current.current_decision = Game.current.decision_dict[decision_name];
									
									Game.current.current_choices.Clear(); // loading up available options
									List<string> choice_string_list = new List<string>(); // loading up text of available options
									foreach (Choice choice in Game.current.decision_dict[decision_name].choices)
									{
										if (all_true(choice.conditions))
										{
											choice_string_list.Add (find_variable(choice.text));
											Game.current.current_choices.Add (choice);
										}
									}

									// Shuffle choices, if necessary
									if (Game.current.current_decision.randomize_order)
									{
										List<string> new_choice_string_list = new List<string>();
										List<Choice> new_choice_list = new List<Choice>();
										while (choice_string_list.Count > 0)
										{
											int random_index = Random.Range(0,choice_string_list.Count);

											string new_choice = choice_string_list[random_index];
											new_choice_string_list.Add(new_choice);

											choice_string_list.RemoveAt(random_index);

											new_choice_list.Add(Game.current.current_choices[random_index]);
											Game.current.current_choices.RemoveAt(random_index);
										}
										choice_string_list = new_choice_string_list;

										Game.current.current_choices = new_choice_list;

									}

									List<int> test_participants = new List<int>();
									bool explanation_given = false;
									for (int i=1; i<=4; i++)
									{
										if (Game.current.players[i-1].active)
										{
											List<string> player_tests = new List<string>();
											foreach (string test in Game.current.current_decision.participant_conditions)
											{
												player_tests.Add ("$player" + i.ToString() + "." + test); // append "p1" to the beginning of each variable if i=1, etc.
											}
											if (all_true(player_tests))
											{
												test_participants.Add (i);
											}
											else if (!explanation_given && Game.current.current_decision.exclusion_text != "")
											{
												explanation_given = true;
												dialogue_manager.new_message(Game.current.current_decision.exclusion_text);
												Debug.Log("providing explanation: " + Game.current.current_decision.exclusion_text);
											}
										}
									}
									if (test_participants.Count > 0)
									{
										if (Game.current.current_decision.choice_mode.Split (' ')[0] == "hidden")
										{
											dialogue_manager.present_hidden_choice(choice_string_list, test_participants);
										}
										else
										{
											dialogue_manager.present_choice(choice_string_list, test_participants);
										}
										onPlayerChoosing(true);
									}
									else
									{
										process_branch(Game.current.current_decision);
									}
								}
								else
								{
									Debug.Log ("Decision key: " + decision_name + " NOT FOUND in Game.current.decision_dict");
/*									foreach (string decision_key in Game.current.decision_dict.Keys)
									{
										Debug.Log ("Decision: " + decision_key);
									}*/
								}
								break;
							case "cutscene":
								if (Game.current.cutscene_dict.ContainsKey (val_string))
								{
									cutscene_manager.play_cutscene(val_string);
									onImportanceSet(true);
									
									//Debug.Log ("Applying narrator box graphic to dialogue box");
									dialogue_manager.hide_speech_box();
									dialogue_manager.set_color_mode("nar");
									
									declare_pause(Game.current.cutscene_dict[val_string].get_duration());
									//StartCoroutine(wait_to_progress(Game.current.cutscene_dict[scene_name].get_duration()));
								}
								else
								{
									Debug.Log ("Couldn't find cutscene named: " + val_string);
									progress ();
								}
								
								break;
							case "load_story":
								load_story (val_string);
								Game.current.players[command_player].active = true;
								SaveLoad.Load (Game.current.story_name);
								
								Game.current.script.Add (new Line("decision: new/load/back"));
								progress();
								dialogue_manager.hide_speech_box();

								break;
							case "new_game":
								new_game (val_string);
								Game.current.script.Insert (Game.current.script_pos + 1, new Line("deactivate all players"));
								
								progress();
								break;
							default:
								Debug.Log ("Couldn't parse command (" + this_line.text + ")");
								progress();
								break;
							}
						}
					}
					else
					{
						Debug.Log ("Couldn't parse command (" + this_line.text + ")");
						progress();
					}
				}
				else if (this_line.text.Contains ("=") || this_line.text.Contains ("+%") || this_line.text.Contains ("-%"))
				{
					make_change(this_line.text.Trim ());
					progress ();
				}
				else
				{
					string[] split_text = this_line.arg_dict["_first_key"].Split (' ');

					//Debug.Log("First word in _first_key: " + split_text[0] + ", last word in _first_key: " + split_text[split_text.Length-1]);

					if (this_line.arg_dict["_first_key"].EndsWith (" plays"))
					{
						string music_name = this_line.text.Substring (0, this_line.text.Length - 6);
						set_music (music_name);
						progress ();
					}
					else if (character_effects.Contains (split_text[split_text.Length - 1].Trim ()) && split_text[0] != "screen")
					{
						string target = this_line.arg_dict["_first_key"].Substring (0, this_line.arg_dict["_first_key"].Length - split_text[split_text.Length - 1].Length).Trim ();
						Debug.Log("Character effect detected: " + split_text[split_text.Length - 1].Trim () + ", with target: " + target);
						switch (split_text[split_text.Length - 1].Trim ())
						{
						case "blinks":
							float blink_intensity = 0.1f;
							float blink_duration = 0.5f;
							float blink_gain = 0.05f;
							
							if (this_line.arg_dict.ContainsKey("intensity"))
							{
								blink_intensity = Mathf.Clamp01(float.Parse(this_line.arg_dict["intensity"]));
							}
							if (this_line.arg_dict.ContainsKey("duration"))
							{
								blink_duration = float.Parse(this_line.arg_dict["duration"]);
							}
							if (this_line.arg_dict.ContainsKey("gain"))
							{
								blink_gain = Mathf.Clamp01(float.Parse(this_line.arg_dict["gain"]));
							}
							
							dialogue_manager.blink_speaker(find_variable(target), blink_intensity, blink_duration, blink_gain);
							delay_for_effect(this_line);
							break;
						case "darkens":
							float dark_intensity = 0.1f;
							float dark_duration = 0.5f;
							float dark_gain = 0.05f;
							
							if (this_line.arg_dict.ContainsKey("intensity"))
							{
								dark_intensity = Mathf.Clamp01(float.Parse(this_line.arg_dict["intensity"]));
							}
							if (this_line.arg_dict.ContainsKey("duration"))
							{
								dark_duration = float.Parse(this_line.arg_dict["duration"]);
							}
							if (this_line.arg_dict.ContainsKey("gain"))
							{
								dark_gain = Mathf.Clamp01(float.Parse(this_line.arg_dict["gain"]));
							}
							
							dialogue_manager.darken_speaker(find_variable(target), dark_intensity, dark_duration, dark_gain);
							delay_for_effect(this_line);
							break;
						case "blushes":
							float blush_intensity = 0.5f;
							float blush_duration = 0.5f;
							float blush_gain = 0.05f;
							string blush_color = "red";
							
							if (this_line.arg_dict.ContainsKey("intensity"))
							{
								blush_intensity = Mathf.Clamp01(float.Parse(this_line.arg_dict["intensity"]));
							}
							if (this_line.arg_dict.ContainsKey("duration"))
							{
								blush_duration = float.Parse(this_line.arg_dict["duration"]);
							}
							if (this_line.arg_dict.ContainsKey("gain"))
							{
								blush_gain = Mathf.Clamp01(float.Parse(this_line.arg_dict["gain"]));
							}
							if (this_line.arg_dict.ContainsKey("color"))
							{
								blush_color = this_line.arg_dict["color"];
							}
							
							dialogue_manager.blush_speaker(find_variable(target), blush_intensity, blush_duration, blush_gain, blush_color);
							delay_for_effect(this_line);
							break;
						case "shakes":
							float my_intensity = 20f;
							float my_decay = 0.95f;
							
							if (this_line.arg_dict.ContainsKey("intensity"))
							{
								my_intensity = float.Parse(this_line.arg_dict["intensity"]);
							}
							if (this_line.arg_dict.ContainsKey("decay"))
							{
								my_decay = Mathf.Clamp01(float.Parse(this_line.arg_dict["decay"]));
							}
							Debug.Log("shaking with intensity: " + my_intensity.ToString() + ", decay: " + my_decay.ToString());
							dialogue_manager.shake_speaker(find_variable(target), my_intensity, my_decay);
							delay_for_effect(this_line);
							break;
						case "squishes":
							float squish_intensity = 0.7f;
							float squish_duration = 0.3f;
							float squish_gain = 0.1f;
							
							if (this_line.arg_dict.ContainsKey("intensity"))
							{
								squish_intensity = Mathf.Clamp01(float.Parse(this_line.arg_dict["intensity"]));
							}
							if (this_line.arg_dict.ContainsKey("duration"))
							{
								squish_duration = float.Parse(this_line.arg_dict["duration"]);
							}
							if (this_line.arg_dict.ContainsKey("gain"))
							{
								squish_gain = Mathf.Clamp01(float.Parse(this_line.arg_dict["gain"]));
							}
							
							dialogue_manager.squish_speaker(find_variable(target), squish_intensity, squish_duration, squish_gain);
							delay_for_effect(this_line);
							break;
						case "throbs":
							float throb_intensity = 1.1f;
							float throb_duration = 0.3f;
							float throb_gain = 0.05f;
							
							if (this_line.arg_dict.ContainsKey("intensity"))
							{
								throb_intensity = Mathf.Clamp(float.Parse(this_line.arg_dict["intensity"]), 0f, 100f);
							}
							if (this_line.arg_dict.ContainsKey("duration"))
							{
								throb_duration = float.Parse(this_line.arg_dict["duration"]);
							}
							if (this_line.arg_dict.ContainsKey("gain"))
							{
								throb_gain = Mathf.Clamp01(float.Parse(this_line.arg_dict["gain"]));
							}

							Debug.Log("throbbing with intensity: " + throb_intensity.ToString() + ", duration: " + throb_duration.ToString() + ", gain: " + throb_gain.ToString());

							dialogue_manager.throb_speaker(find_variable(target), throb_intensity, throb_duration, throb_gain);
							delay_for_effect(this_line);
							break;
						}
					}
					else
					{
						switch (split_text[0].Trim ().ToLower())
						{
						case "character_select":
							for (int i=0; i < Game.current.players.Count; i++)
							{
								if (Game.current.players[i].active)
								{
									start_character_selection(i + 1);
								}
							}
							break;
						case "readin_report":
							Debug.Log ("Scene count: " + Game.current.choice_dict.Keys.Count.ToString());
							foreach(string scene_key in Game.current.choice_dict.Keys)
							{
								Debug.Log (scene_key);
							}
							Debug.Log ("Decision count: " + Game.current.decision_dict.Keys.Count.ToString());
							foreach(string decision_key in Game.current.decision_dict.Keys)
							{
								Debug.Log (decision_key);
							}
							Debug.Log ("Cutscene count: " + Game.current.cutscene_dict.Keys.Count.ToString());
							foreach(string cs_key in Game.current.cutscene_dict.Keys)
							{
								Debug.Log (cs_key);
							}
							progress ();
							break;
						case "story_recap":
							show_plot_summary();
							break;
						case "save_game":
							Game.current.script.Insert (Game.current.script_pos + 1, new Line("set_password"));
							
							Game.current.script.Insert (Game.current.script_pos + 1, new Line("decision: commander_choice"));
							
							Line prompt_line = new Line("Who will set the password?");
							prompt_line.speaker = "Narrator";
							Game.current.script.Insert (Game.current.script_pos + 1, prompt_line);
							
							progress ();
							progress ();
							
							break;
						case "set_password":
							dialogue_manager.show_tutorial_box("Player " + (command_player + 1).ToString() + ", set password using direction + <color=green>Confirm</color>. Press <color=blue>Pause</color> when done.");
							onPromptPassword();
							
							break;
						case "unpause":
							onCanPause();
							dialogue_manager.set_pause_portrait_visibility(false);
							
							progress ();
							break;
						case "screen":
							/*string[] effect_desc = this_line.text.Substring (7).Split ();
							string effect_name = effect_desc[0];*/
							switch (split_text[split_text.Length -1])
							{
							case "flashes":
								float duration = 0.6f;
								float attack = 0.5f;
								string color = "white";
								
								if (this_line.arg_dict.ContainsKey("color"))
								{
									color = plug_in_variables(this_line.arg_dict["color"]).Trim();
								}
								if (this_line.arg_dict.ContainsKey("attack"))
								{
									attack = Mathf.Clamp01(float.Parse(this_line.arg_dict["attack"]));
									
								}
								if (this_line.arg_dict.ContainsKey("duration"))
								{
									duration = float.Parse(this_line.arg_dict["duration"]);
								}
								
								dialogue_manager.effect_flash (color, duration, attack);
								break;
							case "shakes":
								float intensity = 30f;
								float decay = 0.95f;
								
								if (this_line.arg_dict.ContainsKey("intensity"))
								{
									intensity = float.Parse(this_line.arg_dict["intensity"]);
								}
								if (this_line.arg_dict.ContainsKey("decay"))
								{
									decay = Mathf.Clamp(float.Parse(this_line.arg_dict["decay"]), 0.1f, 1f);
								}

								Debug.Log("Shaking screen with intensity: " + intensity.ToString() + ", decay: " + decay.ToString());

								dialogue_manager.effect_shake (intensity, decay);
								break;
							}
							
							delay_for_effect (this_line);
							break;
						case "load_game":
							savegame_index = int.Parse(this_line.text.Substring(10).Trim ());
							dialogue_manager.show_tutorial_box("Player " + (command_player + 1).ToString() + ", guess password using direction + <color=green>Confirm</color>. Press <color=blue>Pause</color> when done.");
							password_mode = "load";
							onGuessPassword();
							
							break;
						case "unlock_game":
							load_game (savegame_index);
							//Debug.Log ("Character lock status: " + (!Game.current.can_select_character).ToString ());
							Game.current.script.Insert (Game.current.script_pos + 1, new Line("deactivate all players"));
							//Game.current.script.Insert (Game.current.script_pos + 1, new Line("story_recap"));
							
							progress();
							break;
						case "delete_game":
							savegame_index = int.Parse(this_line.text.Substring(12).Trim ());
					
							dialogue_manager.show_tutorial_box("Player " + (command_player + 1).ToString() + ", enter password to delete game. Press <color=blue>Pause</color> when done.");
							password_mode = "delete";
							onGuessPassword();
							break;
						case "delete_current_game":
							delete_game (savegame_index);
							populate_save_files("delete");
							Game.current.script.Insert (Game.current.script_pos + 1, new Line("delete_game_list"));
							progress();
							break;
						case "hide_dialogue":
							dialogue_manager.hide_speech_box();
							onPauseInput(0.6f);
							StartCoroutine(wait_to_progress(0.5f));
							break;
						case "hide_guis":
							dialogue_manager.hide_uis();
							progress ();
							break;
						case "hide":
							if (this_line.text == "hide player stats")
							{
								Game.current.default_stats_to_show.Clear();
								dialogue_manager.collapse_guis();
							}

							else if (this_line.text == "hide game stats")
							{
								//Game.current.pause_stats.Clear();
								dialogue_manager.hide_game_stats();
								Game.current.game_stats_showing = false;
							}
							progress();
							break;
						case "show":
							if (this_line.text == "show game stats")
							{
								dialogue_manager.update_game_stat_ui();
							}
							progress();
							break;
						case "clear":
							string transition_type = "";
							if (this_line.arg_dict.ContainsKey("transition"))
							{
								transition_type = this_line.arg_dict["transition"];
							}

							if (split_text.Length == 1)
							{
								dialogue_manager.clear_all_speakers(transition_type);
							}
							else if (split_text[1] == "cutscene")
							{
								cutscene_manager.clear_images();
							}
							else if (this_line.text.Substring (6).Trim () == "tutorial message")
							{
								dialogue_manager.hide_tutorial_box();
							}
							else if (split_text[1] == "scene")
							{
								dialogue_manager.set_background("transparent");
							}
							else
							{
								dialogue_manager.clear_speaker(find_variable(this_line.text.Substring(6).Trim ()), transition_type: transition_type);
							}
							onPauseInput(0.6f);
							StartCoroutine(wait_to_progress(0.5f));
							dialogue_manager.set_pause_portrait_visibility(false);
							
							break;
						case "curtains":
							dialogue_manager.set_background("transparent");
							onPauseInput(0.6f);
							StartCoroutine(wait_to_progress(0.5f));
							dialogue_manager.set_pause_portrait_visibility(false);
							break;
						case "exit":
							Debug.Log("first_key: " + this_line.arg_dict["_first_key"]);
							string target = this_line.arg_dict["_first_key"].Substring(5).Trim ();
							string transition = "";
							if (this_line.arg_dict.ContainsKey("transition"))
							{
								transition = this_line.arg_dict["transition"];
							}

							if (target == "all")
							{
								dialogue_manager.clear_all_speakers(transition);
							}
							else
							{
								string direction = "";
								if (this_line.arg_dict["_first_key"].EndsWith (" left"))
								{
									direction = "left";
									target = target.Substring (0, target.Length - 4).TrimEnd ();
								}
								else if (this_line.arg_dict["_first_key"].EndsWith (" right"))
								{
									direction = "right";
									target = target.Substring (0, target.Length - 5).TrimEnd ();
								}
								dialogue_manager.clear_speaker(find_variable(target), direction, transition);
							}
							onPauseInput(0.6f);
							if (this_line.arg_dict.ContainsKey("wait time"))
							{
								StartCoroutine(wait_to_progress(float.Parse(this_line.arg_dict["wait time"])));

							}
							else
							{
								StartCoroutine(wait_to_progress(0.5f));

							}
							dialogue_manager.set_pause_portrait_visibility(false);
							
							break;
						case "lock":
							if (this_line.text == "lock characters")
							{
								onCharacterLock(true);
							}
							progress ();
							break;
						case "unlock":
							if (this_line.text == "unlock characters")
							{
								onCharacterLock(false);
							}
							break;
						case "deactivate":
							if (Game.current.script[Game.current.script_pos].text.Trim () == "deactivate all players")
							{
								for (int i=0; i < player_colors.Count; i++)
								{
									Game.current.players[i].active = false;
								}
								check_for_active_players();
							}
							progress ();
							break;
						case "load_game_list":
							populate_save_files("load");
							
							Game.current.script.Insert (Game.current.script_pos + 1, new Line("decision: load_game_list"));
							progress();
							break;
						case "delete_game_list":
							populate_save_files("delete");
							
							Game.current.script.Insert (Game.current.script_pos + 1, new Line("decision: delete_game_list"));
							progress();
							break;
						default:
							//process_dialogue_extras(this_line);
							dialogue_manager.show_approvals();
							
							string target_pos = dialogue_manager.get_char_pos();
							//Debug.Log("Default message: " + plug_in_variables(Game.current.script[Game.current.script_pos].text));
							dialogue_manager.new_message(plug_in_variables(Game.current.script[Game.current.script_pos].text), target_pos);
							
							if (this_line.important)
							{
								register_plot_point(this_line);
							}
							
							StartCoroutine(wait_for_input(0.5f));
							
							break;
						}
					}
					
				}
			}
		}
		else
		{
			Debug.Log ("End of script");
		}
    }
     
    // Update is called once per frame
    void Update () 
	{
		
    }
}
