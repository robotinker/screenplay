using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputDialogue : MonoBehaviour {

	DialogueManager dialogue_manager;
	ScriptManager script_manager;
	CutsceneManager cutscene_manager;

	//float double_tap_length = 0.1f; // if button pressed twice in this amount of time, counted as double tap (used for skipping dialogue)
	public float axis_threshold = 0.5f;

	// button_down array is Up, Down, Left, Right, Cancel, Confirm
	List<List<float>> button_down = new List<List<float>>{new List<float>{0f,0f,0f,0f,0f,0f,0f},new List<float>{0f,0f,0f,0f,0f,0f,0f},new List<float>{0f,0f,0f,0f,0f,0f,0f},new List<float>{0f,0f,0f,0f,0f,0f,0f}};

	bool can_progress = true;
	bool should_progress = false;
	bool paused = false;
	bool important_line;
	bool in_pause_menu = true;
	bool setting_password = false;
	bool confirming_password = false;
	bool guessing_password = false;
	//float pause_timer = 0f;
	List<bool> progression_approved = new List<bool>{false, false, false, false};
	public bool choosing = false;

	bool externally_paused = false;

	List<int> players_selecting_character = new List<int>();

	public bool can_toggle_activity = false;
	List<float> toggle_activity_timer = new List<float>{1f,1f,1f,1f};
	public float activity_toggle_delay = 1f;

	AudioSource SFX_emitter;

	public bool skip_cutscene_flag = false;

	List<int> temp_code = new List<int>();
	List<int> temp_code2 = new List<int>();

	public delegate void noPlayersChoosing();
	public static event noPlayersChoosing onPlayersDoneSelectingCharacters;
	
	// Use this for initialization
	void Start () {
		dialogue_manager = GameObject.Find ("ScriptHolder").GetComponent<DialogueManager>();
		script_manager = GameObject.Find ("ScriptHolder").GetComponent<ScriptManager>();
		cutscene_manager = GameObject.Find ("ScriptHolder").GetComponent<CutsceneManager>();
		SFX_emitter = GetComponent<AudioSource>();

		ScriptManager.onScriptReady += OnScriptReady;
		ScriptManager.onPlayerChoosing += OnPlayerChoosing;
		ScriptManager.onPauseInput += OnPauseInput;
		ScriptManager.onChooseCharacter += OnChooseCharacter;
		ScriptManager.onCharacterLock += OnCharacterLock;
		ScriptManager.onImportanceSet += OnImportanceSet;
		ScriptManager.onGameLoaded += OnGameLoaded;
		ScriptManager.onCanPause += OnCanPause;
		ScriptManager.onPromptPassword += OnPromptPassword;
		ScriptManager.onGuessPassword += OnGuessPassword;
		clear_approvals();
	}

	void OnChooseCharacter(int player_i)
	{
		players_selecting_character.Add(player_i);
		can_progress = false;
	}

	void check_timers ()
	{
		for (int i=0; i<4; i++)
		{
			if (toggle_activity_timer[i] > 0f)
			{
				toggle_activity_timer[i] -= Time.deltaTime;
			}
		}
	}

	void OnScriptReady ()
	{
		if (!choosing)
		{
			if (paused)
			{
				should_progress = true;
			}
			else
			{
				can_progress = true;

			}
		}
	}

	void OnCanPause ()
	{
		in_pause_menu = false;
	}

	void OnGameLoaded ()
	{
		can_toggle_activity = true;
		in_pause_menu = false;
	}

	void OnCharacterLock (bool new_state)
	{
		ScriptManager.Game.current.can_select_character = !new_state;
	}

	void OnImportanceSet (bool new_val)
	{
		important_line = new_val;
		clear_approvals();
	}

	void OnPlayerChoosing (bool new_state)
	{
		StartCoroutine(wait_to_set_choosing(new_state, 0.5f));

		StartCoroutine(wait_to_allow_progression(0.6f));

		can_progress = false;
	}

	void OnPauseInput (float pause_time)
	{
		StartCoroutine(wait_to_allow_progression(pause_time));
	}

	public void clear_approvals ()
	{
		progression_approved = new List<bool>{false, false, false, false};
		for (int i = 0; i < 4; i++)
		{
			if (!ScriptManager.Game.current.players[i].active)
			{
				progression_approved[i] = true;
			}
		}
	}

	IEnumerator wait_to_set_choosing (bool new_state, float delay)
	{
		float timer = 0f;
		if (new_state == false)
		{
			choosing = new_state;
		}

		while (timer < delay)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
			}
			yield return null;
		}

		if (new_state == true)
		{
			choosing = new_state;
		}
		can_progress = !choosing;
		if (paused && can_progress)
		{
			can_progress = false;
			should_progress = true;
		}
	}

	IEnumerator wait_to_allow_progression (float pause_time, string default_state = "")
	{
		for (int i = 0; i < toggle_activity_timer.Count; i++)
		{
			if (players_selecting_character.Contains (i + 1))
			{
				toggle_activity_timer[i] = pause_time + 0.1f;
			}
		}

		float timer = 0f;
		should_progress = false;
		paused = true;
		can_progress = false;
		while (timer < pause_time)
		{
			if (ScriptManager.Game.current.running)
			{
				timer += Time.deltaTime;
				if (can_progress)
				{
					should_progress = true;
				}
				can_progress = false;
				if (skip_cutscene_flag)
				{
					skip_cutscene_flag = false;
					timer = pause_time;
				}			
			}

			yield return null;
		}
		can_progress = should_progress;
		if (default_state != "")
		{
			can_progress = bool.Parse (default_state);
		}
		paused = false;
	}

	void new_approval(int player_i)
	{
		//Debug.Log("New approval for player " + player_i.ToString() + ", whose current approval is: " + progression_approved[player_i-1].ToString());
		//Debug.Log("Approval status is: " + progression_approved[0].ToString() + ", " + progression_approved[1].ToString() + ", " + progression_approved[2].ToString() + ", " + progression_approved[3].ToString());
		if (progression_approved[player_i-1] == false)
		{
			SFX_emitter.clip = dialogue_manager.sound_dict["approve dialogue progression"];
			SFX_emitter.Play ();
			progression_approved[player_i-1] = true;
			dialogue_manager.message_approval_of(player_i);

		}
		if (progression_approved.Contains(false) == false)
		{
			StartCoroutine(wait_to_allow_progression(0.2f));
			progress_dialogue();
		}
	}

	void progress_dialogue()
	{
		dialogue_manager.hide_approvals();
		
		script_manager.progress();
		clear_approvals();
		can_progress = false;
		SFX_emitter.clip = dialogue_manager.sound_dict["progress dialogue"];
		SFX_emitter.Play ();

		skip_cutscene_flag = true;
		cutscene_manager.clear_images();
	}

	void try_set_activity (int player_i, bool new_val)
	{
		Debug.Log("Trying to set activity");
		if (can_toggle_activity && toggle_activity_timer[player_i-1] <= 0f)
		{
			toggle_activity_timer[player_i-1] = activity_toggle_delay;

			if (new_val)
			{
				//Debug.Log ("drop in");
				List<ScriptManager.Character> committed_characters = ScriptManager.Game.current.players.FindAll (item => ScriptManager.Game.current.player_names.Contains(item.name));
/*				Debug.Log ("Committed characters: " + committed_characters.Count.ToString());
				Debug.Log ("Available characters: " + ScriptManager.Game.current.p_chars_available.Count.ToString());*/
				if (ScriptManager.Game.current.stat_dict["_player_count"].int_val < script_manager.player_count_max)
				{
					Debug.Log("Player characters available to choose from: " + ScriptManager.Game.current.p_chars_available.Count);
					foreach (string char_name in ScriptManager.Game.current.p_chars_available)
					{
						Debug.Log(char_name);
					}
					if (ScriptManager.Game.current.p_chars_available.Count == 0)
					{
						script_manager.reactivate_p_char(player_i);
						SFX_emitter.clip = dialogue_manager.sound_dict["player drop in"];
						SFX_emitter.Play ();
						
						can_progress = true;
					}
					else if (ScriptManager.Game.current.can_select_character || (committed_characters.Count < script_manager.player_count_max && !committed_characters.Contains (ScriptManager.Game.current.players[player_i - 1])))
					{
						script_manager.drop_in_player(player_i);
						
						players_selecting_character.Add(player_i);
						SFX_emitter.clip = dialogue_manager.sound_dict["player joining"];
						SFX_emitter.Play ();

						can_progress = false;
					}
					else if (committed_characters.Contains (ScriptManager.Game.current.players[player_i - 1]))
					{
						script_manager.reactivate_p_char(player_i);
						SFX_emitter.clip = dialogue_manager.sound_dict["player drop in"];
						SFX_emitter.Play ();

						can_progress = false;
					}
				}
			}
			else
			{
				Debug.Log ("drop out");
				script_manager.drop_out_player(player_i);
				if (players_selecting_character.Contains (player_i))
				{
					players_selecting_character.Remove (player_i);

				}

				if (important_line || script_manager.pause_all_lines)
				{
					new_approval(player_i);
				}

				if (ScriptManager.Game.current.players[player_i - 1].name.StartsWith ("Player"))
				{
					can_progress = true;
				}
				else
				{
					can_progress = false;

				}

/*				if (ScriptManager.Game.current.can_select_character)
				{
					ScriptManager.Game.current.players[player_i - 1].name = "Player " + player_i.ToString ();

				}
				else
				{
					ScriptManager.Game.current.p_chars_available.Remove (ScriptManager.Game.current.players[player_i -1].name);
				}*/

				SFX_emitter.clip = dialogue_manager.sound_dict["player drop out"];
				SFX_emitter.Play ();

			}
		}
	}

	void OnPromptPassword ()
	{
		setting_password = true;
		confirming_password = false;
		guessing_password = false;
		temp_code.Clear ();
		temp_code2.Clear ();

		toggle_activity_timer[script_manager.command_player] = 0.2f;
	}

	void OnGuessPassword ()
	{
		setting_password = false;
		confirming_password = false;
		guessing_password = true;
		temp_code.Clear ();

		toggle_activity_timer[script_manager.command_player] = 0.2f;
	}

	string list_to_string (List<int> my_list)
	{
		string outstring = "";
		foreach (int num in my_list)
		{
			outstring += num.ToString ();
		}
		return outstring;
	}

	public void set_pause_state (bool new_state)
	{
		externally_paused = new_state;
	}

	void check_input ()
	{
		if (Input.GetButtonDown ("Hotkey1"))
		{
			if (toggle_activity_timer[0] <= 0)
			{
				toggle_activity_timer[0] = 0.2f;
				externally_paused = !externally_paused;
				script_manager.set_running (!externally_paused);
			}
			//Debug.Log ("choosing: " + choosing.ToString() + ", paused: " + paused.ToString () + ", players selecting characters: " + players_selecting_character.Count.ToString () + ", can_progress: " + can_progress.ToString());
		}
		if (Input.GetButtonDown("Exit"))
		{
			Application.Quit ();
		}

		if (externally_paused)
		{

		}
		else if (setting_password || guessing_password)
		{
			if (toggle_activity_timer[script_manager.command_player] <= 0)
			{
				string player_prefix = "P" + (script_manager.command_player + 1).ToString();
				
				if (Input.GetButtonDown(player_prefix + "_confirm"))
				{
					int direction = 0;
					if (Input.GetAxis(player_prefix + "_horizontal") < (axis_threshold * -1) || Input.GetAxis(player_prefix + "_horizontal_stick") < (axis_threshold * -1))
					{
						direction = 1;
					}
					else if (Input.GetAxis(player_prefix + "_horizontal") > axis_threshold || Input.GetAxis(player_prefix + "_horizontal_stick") > axis_threshold)
					{
						direction = 2;
					}
					else if (Input.GetAxis(player_prefix + "_vertical") > axis_threshold || Input.GetAxis(player_prefix + "_vertical_stick") > axis_threshold)
					{
						direction = 3;
					}
					else if (Input.GetAxis(player_prefix + "_vertical") < (axis_threshold * -1) || Input.GetAxis(player_prefix + "_vertical_stick") < (axis_threshold * -1))
					{
						direction = 4;
					}
					if (confirming_password)
					{
						temp_code2.Add (direction);
					}
					else
					{
						temp_code.Add (direction);
					}
					dialogue_manager.add_password_char();

					toggle_activity_timer[script_manager.command_player] = 0.2f;
				}
				else if (Input.GetButtonDown(player_prefix + "_cancel"))
				{
					if (confirming_password && temp_code2.Count > 0)
					{
						temp_code2.RemoveAt (temp_code2.Count - 1);
						dialogue_manager.remove_password_char();
						
						toggle_activity_timer[script_manager.command_player] = 0.2f;
					}
					else if (temp_code.Count > 0)
					{
						temp_code.RemoveAt (temp_code.Count - 1);
						dialogue_manager.remove_password_char();
						
						toggle_activity_timer[script_manager.command_player] = 0.2f;
					}


				}
				else if (Input.GetButtonDown(player_prefix + "_pause"))
				{
					if (confirming_password)
					{
						setting_password = false;
						confirming_password = false;
						script_manager.try_lock_password(list_to_string(temp_code), list_to_string(temp_code2));
					}
					else if (setting_password)
					{
						confirming_password = true;
						dialogue_manager.show_tutorial_box("Confirm password.");
					}
					else if (guessing_password)
					{
						guessing_password = false;
						setting_password = false;
						confirming_password = false;
						script_manager.try_password (list_to_string(temp_code));
					}
					dialogue_manager.clear_password_pips();
					toggle_activity_timer[script_manager.command_player] = 0.2f;

				}
			}

		}
		else
		{
			if (!in_pause_menu && can_progress)
			{
				for (int i=1; i<=4; i++)
				{
					if (Input.GetButtonDown("P" + i.ToString() + "_pause") && ScriptManager.Game.current.players[i-1].active && toggle_activity_timer[i-1] <= 0)
					{
						if (Input.GetAxis("P" + i.ToString() + "_cancel") > axis_threshold) // Force cooperation with Cancel + Pause
						{
							toggle_activity_timer[i-1] = 1f;
							if (!script_manager.pause_all_lines)
							{
								script_manager.pause_all_lines = true;
								dialogue_manager.show_tutorial_box("Forced cooperation activated");
							}
							else if (!script_manager.time_everything)
							{
								script_manager.time_everything = true;
								dialogue_manager.show_tutorial_box("Auto-progression activated");
								
							}
						}
						else // pause the game
						{
							in_pause_menu = true;
							script_manager.pause_menu ();
							break;
						}
						
					}
				}
			}
			
			if (choosing && !paused)
			{
				string choice_mode = ScriptManager.Game.current.current_decision.choice_mode;
				if (choice_mode.Split (' ')[0] == "hidden") // 1: Left, 2: Right, 3: Up, 4: Down
				{
					for (int i=1; i<=4; i++)
					{
						if (Input.GetButtonDown("P" + i.ToString() + "_confirm"))
						{
							if ((Input.GetAxis("P" + i.ToString() + "_horizontal") < (axis_threshold * -1) || Input.GetAxis("P" + i.ToString() + "_horizontal_stick") < (axis_threshold * -1)) && ScriptManager.Game.current.current_decision.choices.Count > 0)
							{
								dialogue_manager.lock_choice (i, 1);
							}
							else if ((Input.GetAxis("P" + i.ToString() + "_horizontal") > axis_threshold || Input.GetAxis("P" + i.ToString() + "_horizontal_stick") > axis_threshold) && ScriptManager.Game.current.current_decision.choices.Count > 1)
							{
								dialogue_manager.lock_choice (i, 2);
							}
							else if ((Input.GetAxis("P" + i.ToString() + "_vertical") > axis_threshold || Input.GetAxis("P" + i.ToString() + "_vertical_stick") > axis_threshold) && ScriptManager.Game.current.current_decision.choices.Count > 2)
							{
								dialogue_manager.lock_choice (i, 3);
							}
							else if ((Input.GetAxis("P" + i.ToString() + "_vertical") < (axis_threshold * -1) || Input.GetAxis("P" + i.ToString() + "_vertical_stick") < (axis_threshold * -1)) && ScriptManager.Game.current.current_decision.choices.Count > 3)
							{
								dialogue_manager.lock_choice (i, 4);
							}
						}
						else if (Input.GetButtonDown("P" + i.ToString() + "_cancel"))
						{
							dialogue_manager.unlock_choice(i);
						}
					}
				}
				else
				{
					int options_per_column = 1;
					if (dialogue_manager.column_count == 1)
					{
						options_per_column = ScriptManager.Game.current.current_choices.Count;
					}
					else
					{
						options_per_column = ScriptManager.Game.current.current_decision.options_per_column;
					}
					for (int i = 1; i <= 4; i++)
					{
						if ((Input.GetAxis("P" + i.ToString() + "_vertical") > axis_threshold) || (Input.GetAxis("P" + i.ToString() + "_vertical_stick") > axis_threshold))
						{
							if (button_down[i-1][0] == 0)
							{
								dialogue_manager.move_cursor(i, -1);
							}
							button_down[i-1][0] += Time.deltaTime;
						}
						else if ((Input.GetAxis("P" + i.ToString() + "_vertical") < (axis_threshold * -1)) || (Input.GetAxis("P" + i.ToString() + "_vertical_stick") < (axis_threshold * -1)))
						{
							if (button_down[i-1][1] == 0)
							{
								dialogue_manager.move_cursor(i, 1);;
							}
							button_down[i-1][1] += Time.deltaTime;
						}
						else if (Input.GetAxis("P" + i.ToString() + "_horizontal") > axis_threshold || Input.GetAxis("P" + i.ToString() + "_horizontal_stick") > axis_threshold)
						{
							if (button_down[i-1][2] == 0)
							{
								dialogue_manager.move_cursor(i, options_per_column);;
							}
							button_down[i-1][2] += Time.deltaTime;					
						}
						else if (Input.GetAxis("P" + i.ToString() + "_horizontal") < (axis_threshold * -1) || Input.GetAxis("P" + i.ToString() + "_horizontal_stick") < (axis_threshold * -1))
						{
							if (button_down[i-1][3] == 0)
							{
								dialogue_manager.move_cursor(i, options_per_column * -1);;
							}
							button_down[i-1][3] += Time.deltaTime;					}
						
						else
						{
							button_down[i-1][0] = 0f;
							button_down[i-1][1] = 0f;
							button_down[i-1][2] = 0f;
							button_down[i-1][3] = 0f;
						}
						
						if (Input.GetButtonDown("P" + i.ToString() + "_cancel"))
						{
							if (button_down[i-1][4] == 0)
							{
								if (choice_mode == "shop" || choice_mode == "lottery")
								{
									dialogue_manager.unallocate(i);
								}
								else
								{
									dialogue_manager.unlock_choice(i);
								}
							}
							button_down[i-1][4] += Time.deltaTime;
						}
						else {button_down[i-1][4] = 0f;}
						
						if (Input.GetButtonDown("P" + i.ToString() + "_confirm"))
						{
							if (button_down[i-1][5] == 0)
							{
								if (choice_mode == "shop" || choice_mode == "lottery")
								{
									dialogue_manager.allocate(i);
								}
								else
								{
									dialogue_manager.lock_choice(i);
								}
							}						
							button_down[i-1][5] += Time.deltaTime;
						}
						else {button_down[i-1][5] = 0f;}
						
						if ((choice_mode == "shop" || choice_mode == "lottery") && Input.GetButtonDown("P" + i.ToString() + "_pause"))
						{
							if (button_down[i-1][6] == 0)
							{
								dialogue_manager.lock_choice(i);
							}						
							button_down[i-1][6] += Time.deltaTime;
						}
						else {button_down[i-1][6] = 0f;}
					}
				}
			}
			
			else if (players_selecting_character.Count > 0)
			{
				for (int i=1; i <= 4; i++)
				{
					if (players_selecting_character.Contains(i) && toggle_activity_timer[i-1] <= 0f)
					{
						int player_selecting_character = i;
						
						if ((Input.GetAxis("P" + player_selecting_character.ToString() + "_horizontal") > axis_threshold) || (Input.GetAxis("P" + player_selecting_character.ToString() + "_horizontal_stick") > axis_threshold))
						{
							if (button_down[player_selecting_character-1][2] == 0)
							{
								// Next character displayed
								script_manager.swap_p_char(1, player_selecting_character);
								SFX_emitter.clip = dialogue_manager.sound_dict["switch character"];
								SFX_emitter.Play ();
							}
							button_down[player_selecting_character-1][2] += Time.deltaTime;
						}
						else if ((Input.GetAxis("P" + player_selecting_character.ToString() + "_horizontal") < (axis_threshold * -1)) || (Input.GetAxis("P" + player_selecting_character.ToString() + "_horizontal_stick") < (axis_threshold * -1)))
						{
							if (button_down[player_selecting_character-1][3] == 0)
							{
								// Previous character displayed
								script_manager.swap_p_char(-1, player_selecting_character);
								SFX_emitter.clip = dialogue_manager.sound_dict["switch character"];
								SFX_emitter.Play ();
							}
							button_down[player_selecting_character-1][3] += Time.deltaTime;
						}
						else 
						{
							button_down[player_selecting_character-1][2] = 0f;
							button_down[player_selecting_character-1][3] = 0f;
						}
						
						if (Input.GetButtonDown("P" + player_selecting_character.ToString() + "_confirm"))
						{
							// Select character
							script_manager.select_p_char(player_selecting_character);
							players_selecting_character.Remove (player_selecting_character);
							SFX_emitter.clip = dialogue_manager.sound_dict["player drop in"];
							SFX_emitter.Play ();


							if (players_selecting_character.Count == 0 && onPlayersDoneSelectingCharacters != null)
							{
								Debug.Log("Done selecting characters");
								onPlayersDoneSelectingCharacters();
							}
							break;
						}
						else if (Input.GetButtonDown("P" + i.ToString () + "_cancel"))
						{
							try_set_activity(i, false);
							break;
						}
					}
					else if (!ScriptManager.Game.current.players[i-1].active && Input.GetButtonDown("P" + i.ToString () + "_confirm") || Input.GetButtonDown("P" + i.ToString () + "_cancel") || Input.GetButtonDown("P" + i.ToString () + "_pause"))
					{
						try_set_activity(i, true);
						break;
					}
				}
			}
			
			else if (cutscene_manager.cutscene_playing())
			{
				if (cutscene_manager.can_skip())
				{
					if (ScriptManager.Game.current.players.FindAll (item => item.active == true).Count == 0)
					{
						for (int i=1; i<=4; i++)
						{
							if (Input.GetButtonDown("P" + i.ToString () + "_confirm"))
							{
								progress_dialogue();
							}
						}
					}
					else
					{
						for (int i=1; i<=4; i++)
						{
							if (Input.GetButtonDown("P" + i.ToString () + "_confirm") && ScriptManager.Game.current.players[i-1].active)
							{
								new_approval(i);
							}
						}
					}
				}
				
			}
			
			else if (can_progress)
			{
				bool minimum_players_reached = ScriptManager.Game.current.stat_dict["_player_count"].int_val >= script_manager.player_count_min || ScriptManager.Game.current.stat_dict["_player_count"].int_val == 0;
				for (int i=1; i <= 4; i++)
				{
					if (ScriptManager.Game.current.players[i-1].active)
					{
						
						if (Input.GetButtonDown("P" + i.ToString () + "_confirm"))
						{
							if (Input.GetAxis("P" + i.ToString () + "_cancel") > axis_threshold)
							{
								
								try_set_activity(i, false);
								break;
							}
							
							else if (minimum_players_reached)
							{
								if (dialogue_manager.use_type_in && dialogue_manager.typing_in)
								{
									dialogue_manager.finish_typein();
									StartCoroutine(wait_to_allow_progression(0.2f, "true"));
									break;
								}
								else if (important_line)
								{
									new_approval(i);
									//Debug.Log ("Line is important: signaling new approval");
								}
								else if (script_manager.safe_to_progress())
								{
									SFX_emitter.clip = dialogue_manager.sound_dict["progress dialogue"];
									SFX_emitter.Play ();
									script_manager.progress();
									clear_approvals();
									can_progress = false;
									break;
								}
							}

						}
						else if (Input.GetButtonDown("P" + i.ToString () + "_cancel"))
						{
							script_manager.interrupt (i);
							break;
						}
					}
					else if (Input.GetButtonDown("P" + i.ToString () + "_cancel") || Input.GetButtonDown("P" + i.ToString () + "_confirm") || Input.GetButtonDown("P" + i.ToString () + "_pause"))
					{
						try_set_activity(i, true);
						break;
					}
				}
				
			}
		}

	}

	// Update is called once per frame
	void Update () {
		check_timers();
		check_input();
	}
}
