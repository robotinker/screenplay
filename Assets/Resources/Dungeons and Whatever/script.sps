Narrator: Hey, adventurers. Go adventure.
goto town

Section town
	chapter = Town
	scene = town
	music = town_music
	Narrator: The bustling town square. 
	Narrator: People are all over doing town things.
		lock characters
	decision embark

Decision embark
	wrap count = 2
	Dungeon
		goto dungeon_entrance
	Shop
		Narrator: This shops sells weapons and armor. This is your kind of shop.
		You could probably loiter here way longer than would be polite and the proprietors wouldn't kick you out because you're like part of the experience.
		When you buy something the cashier, I mean the storekeeper or whatever, won't even say anything. They'll just quielty nod approvingly.
		branch shop_loop
		goto town
	Inn
		scene = inn
		Narrator: A rustic inn with beds made of hay probably.
		They're charging one gold piece for these hay beds. That seems fair.
		branch hurt_calculator
		branch drained_calculator
		branch rest_check
		goto town

Branch shop_loop
	type = per player
	Default
		Narrator: What will $active_player.name buy?
		decision shop

Branch hurt_calculator
	type = per player
	Default
		active_player.hurt = active_player.MaxHP
		active_player.hurt -= active_player.HP

Branch drained_calculator
	type = per player
	Default
		active_player.drained = active_player.MaxSP
		active_player.drained -= active_player.SP

Section dungeon_entrance
	scene = dungeon_entrance
	music = town_music
	branch update_player_HP_message
	branch update_player_SP_message
	PDMG_boost = 0
	PGuard = false
	PAC_boost = 0
	PDMG_boost_timer = 0
	PGuard_timer = 0
	PAC_boost_timer = 0
	Narrator: You're at the entrance of a pretty dank dungeon. I guess there's a staircase leading down. In the middle of a field. I dunno, how would you build a dungeon?
	branch strong_test

Branch strong_test
	strong enough
		show if
			Avg.LVL >= 4
		Narrator: You're ready now. It's time to fight the boss.
		Narrator: Make sure to get some rest and buy some stuff.
		decision dungeon_choice
	grinding
		decision dungeon_choice

Decision dungeon_choice
	Fight Boss
		show if
			Avg.LVL >= 4
		goto boss_init
	Explore
		goto explore
	Back to town
		goto town

Section explore
	scene = dungeon
	chapter = Exploring the Dungeon
	Narrator: You explore that dungeon because that's what you do. 
	That's your thing, what makes you great. You're the exploriest.
	monster_roll = random(1,100)
	music = dungeon_music
	branch find_monster
	goto battle

Branch find_monster
	spirit
		show if
			monster_roll <= 50
			Avg.LVL <= 2
		monster = spirit
		Spirit:
			position = center
			sound = growl_high
		mon_HP = 12
		mon_HP *= player_count
		mon_AC = 6
		mon_DMG = 4
		Narrator: You encounter a spooky spirit!

	floating eye
		show if
			monster_roll <= 20
		monster = floating eye
		Floating Eye:
			position = center
			sound = growl_low
		mon_HP = 16
		mon_HP *= player_count
		mon_AC = 10
		mon_DMG = 8
		Narrator: GAH! What is <i>that</i>???

	tooth worm
		show if
			monster_roll <= 50
		monster = tooth worm
		Tooth Worm:
			position = center
			sound = growl_high
		mon_HP = 14
		mon_HP *= player_count
		mon_AC = 8
		mon_DMG = 6
		Narrator: Gah! What is that?

	bat
		show if
			Avg.LVL <= 2
		monster = bat
		Bat:
			position = center
			sound = growl_low
		mon_HP = 8
		mon_HP *= player_count
		mon_AC = 5
		mon_DMG = 3
		Narrator: You encounter an aggressive bat!

	iron beetle
		monster = iron beetle
		Iron Beetle:
			position = center
		mon_HP = 12
		mon_HP *= player_count
		mon_AC = 7
		mon_DMG = 5
		Narrator: A beetle gets up in your grill OUT OF NOWHERE!


Section battle
	Narrator: You are fighting a $monster . It's pretty intense I guess.
	branch mon_description
	branch player_action_loop
	branch run_check

Branch mon_description
	Spirit
		show if
			monster == spirit
		His wispy tentacles are kinda freakin' you out but you're keeping it together.
	Bat
		show if
			monster == bat
		This bat just really doesn't want you to be alive any more. What's with that?
	Floating Eye
		show if
			monster == floating eye
		Without an eyelid, how does this thing keep from drying out? And what's with the teeth? There's not even a mouth.
	Tooth Worm
		show if
			monster == tooth worm
		You kind of want to ride it, but it doesn't seem game. Doesn't seem like the sort of thing you'd want to domesticate, either.
	Iron Beetle
		show if
			monster == iron beetle
		It's a beetle, but big and made of iron I guess?		

Branch run_check
	Run
		show if
			Max.agro == 0
		Narrator: You escape!
		goto town
	Stay
		branch agro_reset
		branch battle_loop_check

Branch battle_loop_check
	Final boss
		show if
			monster == Fear Itself
		goto boss_battle
	Default
		goto battle

Decision battle_orders
	show stats
		Health
		Magic
	player test
		name == $active_player.name
	Attack
		active_player.agro = 1
		active_player.name: Take this!
		attack_roll = random(1,20)
		attack_roll += active_player.attack
		attack_roll += active_player.WP_ATT
		attack_damage = active_player.damage
		attack_damage += random(-2, 2)
		Narrator: You swing your $active_player.weapon at the monster!
		branch hit_check
	Heal Self
		show if
			active_player.SP > 0
			active_player.Class == Paladin
		hide = true
		heal_amount = 5
		heal_bonus = active_player.LVL
		heal_bonus *= 3
		heal_amount += heal_bonus
		active_player.hurt = active_player.MaxHP
		active_player.hurt -= active_player.HP
		branch heal_cap
		active_player.agro = 1
		active_player.SP -= 1
	Guard (1 round)
		show if
			active_player.SP > 0
			active_player.Class == Paladin
			player_count > 1
		hide = true
		PGuard = true
		branch guard_alert
		PGuard_timer = player_count
		PGuard_timer -= 1
		active_player.name: I will defend!
		active_player.agro = 1
		active_player.SP -= 1

	Sure strike
		show if
			active_player.SP > 0
			active_player.Class == Fighter
		hide = true
		active_player.agro = 1
		active_player.name: Aaaahhh!
		attack_roll = 100
		attack_damage = active_player.damage
		attack_damage += random(-2, 2)
		active_player.SP -= 1
		Narrator: The strike from your $active_player.weapon is 100% accurate!
		branch hit_check
	Battlecry
		show if
			active_player.SP > 0
			active_player.Class == Fighter
			player_count > 1
		hide = true
		branch SP_boost_others
		active_player.name: To battle!
		active_player.SP -= 1
		active_player.agro = 1
	Trip
		show if
			active_player.SP > 0
			active_player.Class == Thief
		hide = true
		active_player.name: Watch your step!
		Narrator: Enemy dodge decreased!
		mon_AC -= 2
		active_player.agro = 1
		active_player.SP -= 1
	Poison (1 round)
		show if
			active_player.SP > 0
			active_player.Class == Thief
			player_count > 0
		hide = true
		active_player.name: Looking a little haggard there, chummy.
		Narrator: Enemy defense decreased!
		PDMG_boost = 2
		PDMG_boost_timer = player_count
		active_player.agro = 1
		active_player.SP -= 1
	Study
		show if
			active_player.SP > 0
			active_player.Class == Hunter
		hide = true
		active_player.name: The enemy has $mon_HP health remaining.
		active_player.agro = 1
		active_player.SP -= 1
	Calm
		show if
			active_player.SP > 0
			active_player.Class == Hunter
		hide = true
		Narrator: $active_player.name tries to calm the enemy - enemy damage decreased.
		mon_DMG -= 1
		active_player.agro = 1
		active_player.SP -= 1
	Fireball
		show if
			active_player.SP > 0
			active_player.Class == Mage
			active_player.weapon == Book of Fire
		hide = true
		screen flashes
			color = red
			wait time = 0
		active_player.name: Burn!
			sound = magic1
		attack_roll = 50
		attack_damage = 15
		active_player.agro = 1
		active_player.SP -= 1
		branch hit_check
	Healing Light
		show if
			active_player.SP > 0
			active_player.Class == Mage
		hide = true
		heal_amount = 12
		heal_bonus = active_player.LVL
		heal_bonus *= 8
		heal_amount += heal_bonus
		heal_amount /= player_count
		branch heal_all_players
		screen flashes
			duration = 1.5
			wait time = 0
		active_player.name: Mend.
			sound = magic2
		active_player.agro = 1
		active_player.SP -= 1
	Blind (1 round)
		show if
			active_player.SP > 0
			active_player.Class == Mage
			player_count > 1
		hide = true
		screen flashes
			color = blue
			wait time = 0
		active_player.name: The light forbids your aggresion!
			sound = magic1
		players.signal = Dodge up!
		Narrator: Dodge increased!
		PAC_boost = 2
		PAC_boost_timer = player_count
		active_player.agro = 1
		active_player.SP -= 1
	Hide
		active_player.agro = 0

Branch SP_boost_others
	type = per player
	Boosted
		show if
			active_player.Class != Fighter
		active_player.SP += 1
	Default	

Branch heal_all_players
	type = per player
	Healed
		show if
			active_player.HP > 0
		active_player.hurt = active_player.MaxHP
		active_player.hurt -= active_player.HP
		branch heal_cap
	Default

Branch heal_cap
	Would overheal
		show if
			active_player.hurt < heal_amount
		active_player.HP += active_player.hurt
	Default
		active_player.HP += heal_amount

Branch overheal_check
	type = per player
	Capped
		show if
			active_player.HP > active_player.MaxHP
		active_player.HP = active_player.MaxHP
	Default

Branch hit_check
	Hit
		show if
			attack_roll >= mon_AC
		attack_damage += PDMG_boost
		attack_damage += active_player.WP_DMG
		mon_HP -= attack_damage
		branch shake_monster
		Narrator: You hit! The monster takes $attack_damage damage.
			sound = player_attack
		branch dead_mon_check
	Miss
		Narrator: You miss.
			sound = miss
		goto counterattack

Branch shake_monster
	Bat
		show if
			monster == bat
		Bat shakes
			wait time = 0
	Spirit
		show if
			monster == spirit
		Spirit shakes
			wait time = 0
	Floating Eye
		show if
			monster == floating eye
		Floating Eye shakes
			wait time = 0
	Tooth Worm
		show if
			monster == tooth worm
		Tooth Worm shakes
			wait time = 0
	Iron Beetle
		show if
			monster == iron beetle
		Iron Beetle shakes
			wait time = 0
	Default
		screen flashes
			color = red
			wait time = 0

Branch dead_mon_check
	dead
		show if
			mon_HP <= 0
		branch dead_mon_desc
		branch rewards
		goto lvlup_checker
		goto dungeon_entrance
	alive
		goto counterattack

Section counterattack
	branch mon_attack_desc
	mon_hit = random(1,20)
	total_AC = active_player.AC
	total_AC += active_player.ARM_AC
	total_AC += PAC_boost
	branch mon_hit_check

Branch dead_mon_desc
	Final boss
		show if
			monster == Fear Itself
		exit Fear Itself
		screen shakes
			duration = 2
		chapter = Victory
		Narrator: You beat Fear Itself!
		music = victory_music
		You feel like you could do anything. The dishes for instance, you feel like right when you get home you're going to do the dishes. 
		Although you have the nagging feeling that you're almost not consciously aware of that when you actually get home that dish pile is going to look pretty big and you might not do them. 
		But like pretty much anything. 
		Right now you feel great and it feels good thinking that you might do the dishes, if not right away, pretty soon.
		Yeah.
		goto game_over
	Other
		branch mon_exit
		Narrator: You defeated the $monster .
			sound = fanfare

Branch mon_exit
	Spirit
		show if
			monster == spirit
		exit Spirit
	Bat
		show if
			monster == bat
		exit Bat
	Floating Eye
		show if
			monster == floating Eye
		exit Floating Eye
	Tooth Worm
		show if
			monster == tooth worm
		exit Tooth Worm
	Iron Beetle
		show if
			monster == iron beetle
		exit Iron Beetle

Branch mon_attack_desc
	Final boss
		show if
			monster == Fear Itself
		Narrator: Fear Itself attacks!
	Other
		Narrator: The $monster counterattacks!

Branch mon_hit_check
	hit
		show if
			mon_hit > total_AC
		branch guard_check
		branch update_player_HP_message
	miss
		branch mon_miss_desc

Branch guard_check
	Defender
		show if
			PGuard == true
		defender = false
		branch guard_dmg_loop
		branch no_guard_check
	Default
		active_player.HP -= mon_DMG
		screen shakes
			wait time = 0
		branch mon_hit_desc
		branch dead_player_check

Branch guard_dmg_loop
	type = per player
	Defender
		show if
			active_player.Class == Paladin
			active_player.HP > 0
		defender = true
		active_player.HP -= mon_DMG
		screen shakes
			wait time = 0
		branch mon_hit_desc
		branch dead_player_check
	Default

Branch no_guard_check
	No guard
		show if
			defender == false
		PGuard = false
		branch guard_check
	Default

Branch update_player_HP_message
	type = per player
	Default
		active_player.Health = active_player.HP
		active_player.Health += /
		active_player.Health += active_player.MaxHP

Branch update_player_SP_message
	type = per player
	Default
		active_player.Magic = active_player.SP
		active_player.Magic += /
		active_player.Magic += active_player.MaxSP

Branch mon_hit_desc
	Final boss
		show if
			monster == Fear Itself
		Narrator: It hits $active_player.name for a devastating $mon_DMG damage! $active_player.name has $active_player.HP HP left.
			sound = boss_attack
	Other
		Narrator: You take $mon_DMG damage! $active_player.name has $active_player.HP HP left.
			sound = monster_attack

Branch mon_miss_desc
	Final boss
		show if
			monster == Fear Itself
		Narrator: Its fearsome attack misses.
			sound = miss
	Other
		Narrator: It misses.
			sound = miss

Branch dead_player_check
	all_dead
		show if
			Max.HP <= 0
		music = off
		Narrator: You died.
		goto game_over
	dead
		show if
			active_player.HP <= 0
		active_player.signal = KO'd
		Narrator: $active_player.name has been knocked out!
	Default

Branch rewards
	Spirit
		show if
			monster == spirit
		exp_reward = 3
		gold_reward = 25
	Bat
		show if
			monster == bat
		exp_reward = 2
		gold_reward = 15
	Floating Eye
		show if
			monster == floating eye
		exp_reward = 3
		gold_reward = 30
	Tooth Worm
		show if
			monster == tooth worm
		exp_reward = 3
		gold_reward = 25
	Iron Beetle
		show if
			monster == iron beetle
		exp_reward = 2
		gold_reward = 20
	Default

Section lvlup_checker
	branch reward_check
	branch lvlup_check

Branch reward_check
	type = per player
	Get stuff
		show if
			active_player.HP > 0
			active_player.active
		active_player.EXP += exp_reward
		active_player.gold += gold_reward
	Default

Branch lvlup_check
	type = per player
	Level up!
		show if
			active_player.active
			active_player.EXP >= 5
		active_player.LVL += 1
		active_player.name: Level up!
			sound = lvl_up
		active_player.EXP -= 5
		active_player.MaxHP += 2
		active_player.MaxSP += 1
		active_player.attack += 1
		active_player.damage += 1
		active_player.AC += 1
		Narrator: $active_player.name is now level $active_player.LVL !
	Default

Section boss_init
	scene = dungeon
	music = boss_music
	chapter = The Final Battle
	Fear Itself:
		position = center
	mon_HP = 22
	mon_HP *= player_count
	mon_AC = 12
	mon_DMG = 10
	monster = Fear Itself
	goto boss_battle

Section boss_battle
	Narrator: You are fighting the boss. The boss is Fear Itself.
	You are fighting Fear Itself with swords because that's how you roll.
	branch player_action_loop
	branch run_check

Branch player_action_loop
	type = per player
	Alive
		show if
			active_player.HP > 0
		Narrator: What will $active_player.name do?
		branch update_player_HP_message
		branch update_player_SP_message
		goto update_timers
		decision battle_orders
	KOd

Section update_timers
	branch update_PAC_boost
	branch update_PGuard
	branch update_PDMG_boost

Branch update_PAC_boost
	Tick
		show if
			PAC_boost_timer > 0
		PAC_boost_timer -= 1
	Done
		PAC_boost = 0

Branch update_PDMG_boost
	Tick
		show if
			PDMG_boost_timer > 0
		PDMG_boost_timer -= 1
	Done
		PDMG_boost = 0

Branch update_PGuard
	Tick
		show if
			PGuard_timer > 0
		PGuard_timer -= 1
	Done
		PGuard = false

Branch guard_alert
	type = per player
	Guarded
		show if
			active_player.Class != Paladin
		active_player.signal = Guarded!
	Default

Branch agro_reset
	type = per player
	Dead
		show if
			active_player.HP <= 0
		active_player.agro = 0
	Default

Decision shop
	type = allocate
	currency = gold
	player test
		name == $active_player.name
	Book of Fire (SP +2, Fireball)
		show if
			active_player.Class == Mage
			active_player.weapon != Book of Fire
		hide = true
		price = 50
		active_player.SP += 2
		active_player.MaxSP += 2
		branch update_player_SP_message
		active_player.weapon = Book of Fire
	Short Sword (Damage +1, Attack +1)
		show if
			active_player.weapon != short sword
			active_player.weapon != rapier
			active_player.weapon != broadsword
		price = 20
		active_player.weapon = short sword
		active_player.WP_DMG = 1
		active_player.WP_ATT = 1
	Rapier (Damage +2, Attack +2)
		show if
			active_player.weapon != rapier
			active_player.weapon != broadsword
		price = 40
		active_player.weapon = rapier
		active_player.WP_DMG = 2
		active_player.WP_ATT = 2
	Broadsword (Damage +4, Attack +2)
		show if
			active_player.weapon != broadsword
		price = 60
		active_player.weapon = broadsword
		active_player.WP_DMG = 4
		active_player.WP_ATT = 2
	Leather Armor (AC +2)
		show if
			active_player.armor == none
		price = 25
		active_player.armor = leather
		active_player.ARM_AC = 2
	Chainmail (AC +3)
		show if
			active_player.armor != chainmail
			active_player.armor != platemail
		price = 35
		active_player.armor = chainmail
		active_player.ARM_AC = 3
	Platemail (AC +5)
		show if
			active_player.armor != platemail
		price = 50
		active_player.armor = platemail
		active_player.ARM_AC = 5

Branch rest_check
	no need
		show if
			Max.hurt == 0
			Max.drained == 0
		Narrator: You feel totally rested, so I guess there isn't anything to see here...
	possible rest
		sleepy_message_seen = false
		decision sleep_at_inn

Decision sleep_at_inn
	type = passthrough
	show stats
		Health
		Magic
		gold
	Rest
		branch inn_money_check
	Don't rest

Branch inn_money_check
	Broke
		show if
			active_player.gold <= 0
		active_player.name: Any chance you could just <i>give</i> me a bed? You know I'm good for it.
		Inn Keeper: Get outta here! Come back when you have money, kid!
			position = center
		active_player.name: Aww...
	Afford
		branch sleepy_explanation
		active_player.gold -= 1
		active_player.HP = active_player.MaxHP
		active_player.SP = active_player.MaxSP
		branch update_player_HP_message
		branch update_player_SP_message

Branch sleepy_explanation
	First sleeper
		show if
			sleepy_message_seen = false
		sleepy_message_seen = true
		active_player.name: Time to hit the hay!
		Narrator: You get some sleep on a hay bed which is probably comfy if you've never known better.
		Narrator: Oh also, your health is restored.
		active_player.name: I feel great!
	Not first
		active_player.name: I feel great!

Section game_over
	clear
	clear scene
	Narrator: Hero portraits: Justin Nichol and the Flare Project
	Narrator: Enemy portraits: opengameart.org's Balmer and Redshrike
	Narrator: Backgrounds: opengameart.org's Ecrivain, Nila122, Jordan Trudgett, JAP
	Narrator: Icons: opengameart.org's Ails
	Narrator: Music: Britt Brady, Andrew Dent, and Matthew Pablo
	Narrator: SFX: freesound.org's Timbre and RICHERlandTV
	music = off
	hide_GUIs
	curtains
	decision game_select