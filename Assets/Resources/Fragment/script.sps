music = opening
scene = space

Genie:
	position = center
Narrator: A.I - Genie.
Narrator: Yours.
Narrator: Won't snitch.

exit Genie
Giving her skin.
Company evil enough to use.
What for?
decision motive
Biding.
Time.
chapter = Infiltration
decision entry_method

Decision motive
	type = passthrough
	Company collapse
		active_player.rage += 1
	Covert gains
		active_player.stealth += 1
	Her freedom
		active_player.charm += 1

Decision entry_method
	Charm insider
		per vote
			active_player.charm += 1
		music = main
		Drinks. Laughter. Touch. A drive.
		scene = city night

		Heat.
		Breath.
		Genie:
			position = left
		Narrator: Forged dismissal notice
		Genie: You're welcome.
		exit Genie
		Narrator: Before sunrise.
		goto mainframe
	Breach by night
		per vote
			active_player.stealth += 1
		music = main
		curtains
		scene = city night

		Narrator: Darkness. Twist. Click.
		Beeping.
		Genie:
			position = left
		Narrator: No beeping
		Genie: You're welcome.
		exit Genie
		goto mainframe

Section mainframe
	chapter = Investigation
	Narrator: Wires. Data wash. Euphoria.
	Secrets... 
	Millionaires. Human minds awaiting robotic bodies.
	Reversal?
	screen flashes
	Shuffling. A light.
	screen shakes
	Shouting and orders
		duration = 0.2
	chapter = Discovered
	decision guard_problems

Decision guard_problems
	Neutralize
		per vote
			active_player.rage += 1
		Narrator: Impact. Bodies entangled.
		A gasp.
		Stillness.
		Genie:
			position = left
		Narrator: Locks disengaged.
		Genie: You're welcome.
		exit Genie
		goto door
	Impersonate
		per vote
			active_player.charm += 1
		branch impersonation_test
		goto door

Branch impersonation_test
	Success
		show if
			Avg.charm > 1
		Raised brow...
		Ease.
		Back. A map. Hall.
	Failure
		Inquiry. Panic. Pursuit.
		Labyrinth. Heart pounding. Silence.
		Genie:
			position = left
		Narrator: Locks disengaged.
		Genie: You're welcome.
		exit Genie
		chased = 1

Section door
	chapter = Dawning
	Narrator: Floating bodies.
	Terrifying. Perfect.
	branch chase_test
	decision choose_body

Branch chase_test
	chased
		show if
			chased == 1
		screen flashes
		Guard again.
		Unseen.
	not chased
		Healthy subject. Mindless.
		screen flashes
		Lights on. No time!

Decision choose_body
	Push
		show if
			chased == 1
		hide = true
		per vote
			active_player.rage += 1
		screen shakes
		Yelp. Wisps.
		Vacancy.
		goto manifest
	Step in
		Genie:
			position = center
			pose = shocked
		Narrator: Final program
		Genie: Thank you...
		Narrator: Chill. Drift.
		Narrator: Envelopment.
		goto be_free
	Manifest Genie
		goto manifest

Section manifest
	chapter = Awakening
	Genie:
		position = center
		pose = final
	Narrator: Gurgling. Consciousness.
	curtains
	Narrator: Window. Diving. Rest.
	scene = space
	Orders...
	death_toll = 0
	decision orders

Decision orders
	Strike
		goto strike_description
	Exploit
		Narrator: Steady money stream. All you need.
		branch leach_test
		goto game_over
	Be free
		goto be_free

Section strike_description
	chapter = Burning
	branch strike_test
	Corporate collapse. Uncaught.
	death_toll += 1
	Narrator: $death_toll corporations decimated.
	Orders?
	decision continue_strike

Decision continue_strike
	More
		per vote
			active_player.rage += 1
		goto strike_description
	Be free
		goto be_free

Branch strike_test
	Devastation
		show if
			Avg.rage > 2
		Narrator: Devastation.
	Default
		Narrator: Fire and explosions. 

Branch leach_test
	success
		show if
			Avg.stealth > 1
		You are free.
	failure
		Genie caught.
		Still untraceable.

Section be_free
	curtains
	scene = space
	Genie:
		position = center
		pose = final
	Narrator: The genie is free.
	branch rage_test
	curtains
	goto game_over

Branch rage_test
	Suicide
		show if
			Avg.rage > 2
		music = bad
		exit Genie
		Narrator: A fire that burns itself out.
		Narrator: Curtains.
	Terror
		show if
			Avg.rage > 1
		music = bad
		Narrator: More follow. Terror is wrought.
	Love
		music = good
		Narrator: She wanders and loves and lives.
		She is real.

Section game_over
	clear
	clear scene
	Narrator: Portrait art: IICharacter Alpha and KH Mix
	Narrator: Backgrounds: opengameart.org's Scribe and ansimuz
	Narrator: Music: opengameart.org's Zander Noriega, oyvind, VividReality, and BossLevelVGM
	music = off
	hide_GUIs
	curtains
	decision game_select