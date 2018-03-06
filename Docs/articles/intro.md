# How to play

## Registering
Players first need to register. This can be done with the ```.rpg register racename``` command, where racename is a valid race. You can get a list of valid races with the ```.info races``` command.

## First Steps
You can check your current location with the ```.look``` command. This will give a description of the current location with any exits, buildings and other things. Do note that not everything is shown here. A important detail is the color of the response. It ranges from gold (sanctuary) to red (hostile). Currently, it has no effect. But no monsters can be encountered in a sanctuary.

You can enter buildings using the ```.enter buildingname``` command (where buildingname is the name of a building located in the current location). What happens next depends on the type of building, but most of the can be interacted with by clicking the response emoji's.

## Getting XP
Travel to a location that has encounters. Encounters can range from enemies, to special events. (*Note that only monsters are implemented atm*). You can explore the location with the ```.explore``` command. This will start a combat, asks for interaction if needed, and resolves it. Combat will conclude automatically. The combat results will show the last few entries of the combat log, if you want the full combat log, use the ```.combatlog``` commands. The bot will then whisper the complete combat log to you.

## Getting Skills
After you have leveled up, you will gain some skill and attribute points. 

* Attribute points can be spent by using the ```.train attributename``` command. Where attributename is either the full (eg. Strength) or shorthand (eg. STR) of the attribute. This will increase the attribute by 1 at the cost of a attribute points. 

* Skillpoints can be spent at skill trainers. The location of these (and the skills they carry) vary. Once you have found one, enter his building. Click on the response emoji to learn/increase this skill (*Note that not all ranks, correct costs and upper limits have been implemented yet*).

## Using Skills
Skills can be used by using the ```.skill use skillname``` command to use the skill in general, or the ```.skill use skillname on target``` command to use the skill on a specific target. Not all skills can be used on everyone/everything however.

For example, if you have used the woodcutting skill, you can use it by traveling to a location that contains the resource. Then issue the ```.skill use woodcutting``` command to start cutting wood. A timer will start, and as soon as you are done, the bot will notify you.

Some skills will have instant results, but a cooldown as well (like Perception). If you are trying to use the skill while it's on cooldown, you will get a notification about it, along with how many seconds remain.

***TODO* Finish docs**