Cutscene opening
	street
		image = london1
		stretch = true
	bay
		image = town_bay
	mist
		image = mist
	mist2
		image = mist
	mist3
		image = mist

	0
		fade in street
			duration = 0.2
		fade in bay
		fade in mist
		fade in mist2
		fade in mist3
		zoom mist
			amount = 0.5
			point = 0.3, 0.1
			duration = 0.01
		zoom mist2
			amount = 0.7
			point = 0.8, 0.1
			duration = 0.01
		zoom mist3
			amount = 0.5
			point = 1.3, 0.1
			duration = 0.01
		zoom bay
			amount = 1.5
			point = 0.4, 0.4
			duration = 7
		text = Between a forest and a bay stood Evanbrook, home of the noble who stood on the poor.
	0.5
		zoom mist
			amount = 0.6
			duration = 9
			point = 0, 0.1
		zoom mist2
			amount = 0.8
			duration = 9
			point = 0.5, 0.1
		zoom mist3
			amount = 0.6
			duration = 9
			point = 1, 0.1
	5
		clear text
	7
		fade out bay
		text = The rain of Evenbrook is harsh tonight...

	9
		zoom street
			amount = 3
			point = -0.4, 0.7
			duration = 7
		zoom mist
			amount = 0.8
			duration = 7
			point = -0.2, -0.2
		zoom mist2
			amount = 1
			duration = 7
			point = 0.3, -0.2
		zoom mist3
			amount = 0.8
			duration = 7
			point = 0.8, -0.2
	11
		clear text
	13
		text = And within these walls the urchins of Evanbrook seek refuge from the bitter cold...
	16
		pan mist
			duration = 1.5
			point = -0.24, -0.2
		pan mist2
			duration = 1.5
			point = 0.26, -0.2
		pan mist3
			duration = 1.5
			point = 0.76, -0.2
	17
		clear text

music = chill piano
cutscene opening
clear cutscene
scene = dark room


Josephine: Who's there?
	position = left
Josephine: Oh yeah? So $player_count of you, huh?
	pause
	position = center
Josephine: Great. The ritual is about to begin. Come this way...
	position = right
	lock characters
exit Josephine

screen flashes
Narrator: Your inner psychic powers are awakened
	important

chapter = A day in the life
scene = Darmstadt_in_1900
Narrator: And so another day begins...
players.signal = Hungry
Nora: I'm so hungry...
	position = center
	pose = sad
Hester: We need money for food.
	position = right
Harland: Money is a means to and end, my friends. In this case, food.
	position = left
Harland: Look over there...
Narrator: Harland points to a street vendor selling cured meats. He appears to be fumbling with the meat hooks.
Narrator: The smell of ham and sausages makes your stomach rumble in response.
Harland: It would be so easy to swipe a rope of sausages from his cart. 
Harland: He's moving slow this morning. Even if he catches us he couldn't really <i>catch</i> us!
Nora: Well sure, but I bet we can find honest work instead.
	pose = default
Nora: Look, there's a group of travellers - I bet they'd like someone to show them around for a small fee!
Stephen: Why look for patrons when they can come to you?
	position = right
Stephen: I propose that we put on a show right next to the tavern.
Stephen: The swillers inside will be delighted to hear us croon and battle with wooden swords, I'm sure!
Narrator: What do you think?
decision fund_raising

Decision fund_raising
	type = vote
	Steal from the meat stand
		per vote
			active_player.cunning +% 10
		exit all
		Narrator: You inquire with the meat monger about how his business is going. He looks suspicious...
		Narrator: Harland asks if he has any rashers of bacon with him. The man tottles over to a box behind him and leans in.
		Narrator: Harland's hands are quick and precise. He snatches dried meats off of the racks and thrusts them into your pockets.
			important
		Narrator: You skurry into the shadows as the merchant notices his missing goods.
		Narrator: He shouts and wails. As you watch from the shadows, a croud gathers around. 
		The merchant has a look of betrayal and disgust on his face. He sits on his stool, dejected...
		But at least you have food!
		players.energy += 3
		steal_path = true
		Narrator: As the day wears on, you take to begging near a deli.
		Narrator: A man casts a bar of chocolate at your feet, which might have well had been made of gold.
		character_count = 4
		character_count += player_count
		Narrator: The bar is scored into $character_count pieces - just enough to share.
		branch stolen_chocolate
		Narrator: Hester is watching people closely.
		goto psy_intro
	Ask travellers if they'd like a tour
		per vote
			active_player.bravery +% 10
		Harland: Do what you please.
		exit Harland
		Nora: We will!
		exit all
		Narrator: Nora greets the travellers warmly, who are delighted to see your merry band of urchins.
			important
		Narrator: You show them about the city, which amuses them greatly.
		players.money += 2

		Narrator: The travellers give you 2 gold each before going on their way.
		goto shop_intro
	Put on a show outside the tavern
		per vote
			active_player.charm +% 10
		Harland: Do what you please.
		exit Harland
		Stephen: Harland...
		Stephen: Well, he was never a very good actor anyway. Let's go.
		exit all
		Narrator: Stephen leads you in a little dance just outside the Slobbering Sea Hawk.
			important
		players.money += 2

		Onlookers are amused, and after a deal of crooning on Stephen's behalf coins begin to litter the street.
		goto shop_intro

Section shop_intro
	steal_path = false
	Narrator: Down the road you enter a bakery that sells to the poor at a discount.
	It serves you well to have a good relationship with this particular merchant, so you keep your hands to yourselves.
	Helen: Hello children! I hope you're staying out of trouble...
		position = right
	pMax.cunning: Of course we are!
	Helen: What can I get you today?
		pose = happy
	decision food_shop
	Narrator: The shop keeper hands you the food over the counter and you scurry off to a corner to scarf it down.
	Harland: Hey, there you are!
		position = left
	pMax.charm: Hey, Harland.
	Harland: Guys, look what I scored!
		important
	character_count = 4
	character_count += player_count
	Narrator: Harland holds out a bar of chocolate scored into $character_count pieces - just enough to share.
	branch stolen_chocolate
	Narrator: Hester watches eagerly out the shop window as you eat.
	goto psy_intro

Branch stolen_chocolate
	Share
		show if
			player_count == 1
		Stephen: Magnificent! Let's have it!
			position = center
		players.energy += 1
	Fight
		Narrator: It's been <i>so long</i> since you've even <b>seen</b> a piece of chocolate.
		Narrator: You <i>could</i> share it, but... Your mouth is watering uncontrollably.
		total_agro = 0
		branch dilemma_init
		decision prisoners_dilemma
		branch dilemma_result

Branch dilemma_init
	type = per player
	Default
		active_player.agro = 0.0

Decision prisoners_dilemma
	type = hidden passthrough
	Share
		per vote
			active_player.charm +% 10
	Steal it!
		per vote
			active_player.will +% 10
			active_player.charm -% 10
		active_player.agro = 1
		total_agro += 1

Branch dilemma_result
	Share
		show if
			Max.agro == 0
		Narrator: Despite how fiercely your stomach protests, you manage to control yourselves.
		pMax.charm: Let's share.
		Harland: Well yeah, we're family - we look out for each other!
		players.energy += 1
	Steal
		show if
			total_agro == 1
		pMax.agro: Mine!
		Narrator: $pMax.agro swipes the chocolate from Harland's hands and licks it all over.
		pMax.agro: Mine...
		branch thief_award
		Nora:
			position = center
			pose = sad
		Stephen:
			position = right
		Harland: ...
			position = left
		branch disappointed_desc
		exit all
	Fail
		branch frenzy
		branch fight_desc

Branch disappointed_desc
	steal path
		show if
			steal_path == true
		Narrator: The others look on in amazement, but life is tough in the streets. They return to begging.

	Not steal path
		Narrator: The others look on in amazement, but at least they have what they've bought...

Branch fight_desc
	steal path
		show if
			steal_path == true
		Narrator: An argument breaks out over the precious candy bar, making quite a scene. While you fight amongst yourselves, bigger kid steps in.
		Josephine: I'll be taking this.
			position = center
		exit Josephine
		Narrator: You go back to begging, feeling slightly ashamed.
	Default
		Narrator: An argument breaks out over the precious candy bar, making quite a scene. Helen steps in to break up the fight.
		Helen: This is no way for children to behave!
		Narrator: She takes the candy away.
		Helen: Ill-gotten, no doubt. It'd rot your teeth out anyhow...
		Narrator: You return quietly to the food you've purchased.

Branch frenzy
	type = per player
	Stealing
		show if
			active_player.agro == 1
		active_player.name: Mine!
	Default

Branch thief_award
	type = per_player
	Award
		show if
			active_player.name == pMax.agro
		active_player.energy += character_count
	Default

Decision food_shop
	type = allocate
	currency = money
	Bread for you (+1 energy)
		active_player.energy += 1
	Bread for everyone (+1 energy for all)
		price = 2
		players.energy += 1
	Soup to share (+2 energy for all)
		price = 4
		players.energy += 2

Section psy_intro
	Hester:
		position = right
	Narrator: She leans in, fixated on something...

	Hester squishes
		amount = 0.55
		wait time = 0
	Hester: There!
	pMax.cunning: What is it?
	Lorena:
		position = center
	Hester: A starry-eyed traveller. She's new to town - you can tell.
		pause
	Hester: Just look at how she's stumbling around. Looks like she has some money to throw around too.
	Hester shakes
		wait time = 0
		gain = 0.1
	Hester: Here she comes!
	Lorena: Oh, what a lovely little bakery! 
	Helen: Thank you, ma'am! We offer soups and sandwiches, too!
		position = left
		pose = happy
	Lorena: How very novel. And who are these...?
	Helen: Oh, the kids? Charity case, ma'am. These children have no homes so we cut them a break on food pricing.
	Lorena: No homes!? The poor dears...
	Hester darkens
		wait time = 0
	Hester shakes
		gain = 0.1
	Hester: That's right, ma'am. No parents, no food, no nothin'. *cough* *cough*
		interrupt goto = cough_acting
		interrupt message = That's right!
	Lorena: I wish I could help, but I don't have any spare change.
	Hester: Are you sure?
		pose = psy
	Lorena: I... (Hester is using her psychic powers to influence Lorena. Use energy to help her out!)
	decision psy_battle_Lorena
	exit all
	Narrator: Your merry band of urchins skurries out of the shop and into a nearby alley.
	Hester: So <i>that's</i> what it's like to touch another mind...
		position = center
	Nora: It's like playing tug-of-war...
		position = right
	Stephen: It's tiring...
		position = left
	Harland: Can't wait to see what kind of mischief we can make with this!
		position = center
	goto escape_scene
	curtains

Section cough_acting
	active_player.signal = Cunning up!
	active_player.cunning +% 10
	active_player.name: And these fevers all the time... Poor Hester's been getting chills, too!
	Hester squishes
		wait time = 0
		amount = 0.6
	Hester: What are we gonna do...?

Decision psy_battle_Lorena
	type = bid
	currency = energy
	Give the kids some money
		Lorena: I...
			pose = entranced
		Lorena: You know what? I could spare something. Here you are.
		money_pool = 12
		money_pool /= player_count
		players.money += money_pool
		Narrator: The woman holds out a handful of coins. You snatch it away eagerly.
		Hester shakes
			wait time = 0
		Hester: *cough* Thank you very much, madam. Maybe we'll be able to get through the winter afterall.
			pose = default
		Lorena: May you, indeed. Now, may I have a pastry please?
			pose = default
		Helen: Sure thing, ma'am!
		exit Lorena
	Let's get them medicine.
		Lorena: That's just so horrible...
			pose = entranced
		Lorena: You know what? I just passed a chemist on the way here. I'll be back!
			pose = default
			important
		exit Lorena
		Hester: Hey, what'd you do that for? She had money to give us!
			pose = default
		Stephen: Who knows? We might need some medicine in the future!
			position = center

	What if we found them a home?
		Lorena: Uh, I...
			pose = entranced
		Lorena: Hey! This city <i>has</i> to have an orphanage! I've been all over the world, and have yet to find a city without one!
			pose = default
			important
		exit Lorena
		Hester:
			pose = default
		Helen: Well, that's never happened before... Odd woman, that one.

	Children are the future. Have all my money!
		allocation limit = 1
		Lorena: I...
			pose = entranced
		Lorena: You know what? I've spent too much time and money thinking about myself!
		Lorena: It's high time I put others before myself. Here, kids. Start a new life!
			pose = default
			important
		money_pool = 200
		money_pool /= player_count
		players.money += money_pool
		exit Lorena
		Hester:
			pose = default
		Narrator: The woman hands you a bag full of gold pieces and walks outside, arms outstretched
		Narrator: She does a little turn, laughs, and takes a breath of air - which seems somehow fresher with her burden of riches lifted.
		Narrator: You divide the money between yourselves.
		Hester: Well that went well!
			pose = default
	I just want a pastry...
		type = failure
		bad bids = 2
		Lorena: I... I'd just like a pastry, please!
		Helen: Alrighty! Here you go!
		Lorena: <i>I've gotta go.</i>
		exit Lorena
		Hester: Aw...
			pose = default

Section escape_scene
	music = gem_popper
	Narrator: Just then a commotion starts from across the street. A man is shouting to a police officer and pointing in your general direction. "Thief!" he yells.
	Harland blushes
	Harland: Well, looks like it's time to move them legs!
	exit Harland
	Stephen: Run!
	exit all
	Narrator: The alley you're in has a fence you could probably jump, and down the road is a set of pipes that look climbable - they'd never get you on the rooftops.
	Narrator: On the other hand, you're right next to an abandoned factory. If you can pry open one of the windows you could lose them pretty easily inside...
	Narrator: Then there's taking your chances bolting down the street...
	Narrator: Where to?
	total_captured = 0
	decision run
	Harland:
		position = left
	Stephen:
		position = right
	Nora:
		position = center
		pose = sad
	branch captured_check
	Stephen: Wait a second, where's Hester?
	Harland: Did anyone see which way she went?
	Nora: I think I saw her get into a carriage...
	Harland darkens
		wait time = 0
	Harland squishes
	Harland: She <i>what</i>?
	Nora: I dunno. She took off down the street and a black carriage pulled right up in front of her.
		important
	Nora: A man opened the door and held out his hand. I didn't see what happened next. It was all so fast...
	exit all
	Narrator: Chapter Complete
	Narrator: This story is still a work in progress, but we hope you've enjoyed your stay in Evanbrook so far.
	Narrator: Join us soon for more adventures!
	goto game_over

Branch captured_check
	Nobody captured
		show if
			total_captured == 0
		Harland: Whew, we made it!
		Nora: Is everyone okay?
	Default
		Narrator: $total_captured of you have been captured by the police.
		Nora: What are we gonna do?
		Harland: We'll just have to bust them out!


Decision run
	type = passthrough
	show stats
		energy
	timed = 15
	Streets
		active_player.hiding_quality = 30
		Narrator: $active_player.name takes off down the street.
		cop_roll = random(1,100)
		branch capture_check

	Fence (1 energy)
		branch fence_energy_check
		cop_roll = random(1,100)
		branch capture_check
	Factory (3 energy)
		branch factory_energy_check
		cop_roll = random(1,100)
		branch capture_check
	Pipes (4 energy)
		branch pipe_energy_check
		cop_roll = random(1,100)
		branch capture_check

Branch capture_check
	Caught
		show if
			cop_roll > active_player.hiding_quality
		Narrator: They can't shake the police, though! $active_player.name is taken away.

		active_player.name: NO!
		active_player.captured = true
		total_captured += 1
	Escaped
		Narrator: The inspector loses them.
		active_player.captured = false

Branch fence_energy_check
	Not enough energy
		show if
			active_player.energy < 1
		Narrator: $active_player.name tries to climb the fence, but is too weak!
		active_player.hiding_quality = 0
	Default
		Narrator: $active_player.name clears the fence in the alley way and runs!
		active_player.hiding_quality = 50

Branch factory_energy_check
	Not enough energy
		show if
			active_player.energy < 3
		Narrator: $active_player.name tries to pry a window open, but is too weak!
		active_player.hiding_quality = 0
	Default
		Narrator: $active_player.name pries a window open and disappears into the factory!
		active_player.hiding_quality = 80

Branch pipe_energy_check
	Not enough energy
		show if
			active_player.energy < 4
		Narrator: $active_player.name tries to climb the pipes, but is too weak!
		active_player.hiding_quality = 0
	Default
		Narrator: $active_player.name scrambles up to the rooftops and watches the drama playing out below.
		active_player.hiding_quality = 100

Section game_over
	clear
	clear scene
	Narrator: Portrait art: IICharacter Alpha and KH Mix
	Narrator: Backgrounds: Jordan Trudgett
	Narrator: Music: opengameart.org's syncopika, Mopz
	clear all
	music = off
	hide_GUIs
	curtains
	decision game_select