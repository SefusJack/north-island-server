# Settings in config/

The keys below are in the `Server` section unless they name another one. The section is merged from `appsettings.json` and the files in `config/`.

## battle.json

| Setting | What it does |
|---|---|
| `BattleActionDelayMs` | A fixed pause in milliseconds between the actions of a round; 0 uses the animation lengths of the skills instead |
| `BattleActionMarginMs` | Extra milliseconds added to the animation time when the server waits for the client to play a combination or knock-off |
| `BattleTickDrawMs` | Milliseconds the client's battle loop spends drawing per tick, used to estimate how long an action plays |
| `BattleTickVerticalBlankHz` | The display refresh rate in Hz the battle loop waits for per tick; 0 means no wait |
| `BattleTickCameraMs` | Milliseconds the camera needs to glide back after an attacker returns to its cell |
| `BattleCommandGraceSeconds` | Seconds added to the command time before the server plays a round with the commands it has |
| `ComboSpeedRange` | The largest difference in turn speed between two fighters that can still join the same combination |
| `ComboFullLevelGap` | The level lead of the attacking side over the target at which a further attacker always joins a combination |
| `ComboHighLevelGap` | The level lead from which ComboHighPercent is the join chance |
| `ComboHighPercent` | The join chance in percent at ComboHighLevelGap |
| `ComboMidLevelGap` | The level lead from which ComboMidPercent is the join chance |
| `ComboMidPercent` | The join chance in percent at ComboMidLevelGap |
| `ComboBasePercent` | The join chance in percent below the mid gap; with the sliding curve, the value the chance starts from |
| `ComboChanceCurve` | How the join chance of a combination depends on the level lead: Steps (fixed bands) or Sliding |
| `ComboOddsLevels` | Which fighters' levels are averaged for the combination odds: All, Players or Members |
| `ComboRebornLevelBonus` | The levels a fighter counts per rebirth on top of its level for the combination odds |
| `ComboDamageBonusPercent` | The percent added to the damage of every member of a combination; 0 is the plain sum |
| `DefeatedMonsterHideSeconds` | How many seconds a monster that was beaten stays hidden on the map |
| `BattleImmunitySeconds` | How many seconds after a battle a player cannot be drawn into a new fight |
| `MonsterGoldRewards` | True when winning a battle also pays gold |
| `FieldEncounterCooldownSeconds` | The seconds after a field fight (and after arriving on a map) before the next field fight can start |
| `FieldEncounterRadius` | How close in pixels a field monster's place or wander box must be to the player to be fought |
| `ElementAdvantageMultiplier` | The damage factor when the attacker's element overcomes the target's |
| `ElementDisadvantageMultiplier` | The damage factor when the target's element overcomes the attacker's |
| `DropToKillingBlowOnly` | True when only the fighters of the felling blow roll a monster's drops, instead of every winner |
| `BurstMaxLevel` | The highest level that still earns burst experience |
| `BurstablePetPercent` | The percent of the player's burst experience a burstable pet earns |
| `BurstDebuffBasePercent` | The percent of the combination damage paid as burst experience for a monster that fell without a debuff |
| `BurstDebuffStepPercent` | The percent added to that for each distinct debuff the monster carried when it fell |
| `BurstDebuffSteps` | The most debuffs that count towards the burst experience |
| `CussDamagePercent` | The percent more damage a target under Cuss takes |
| `ShrinkDamagePercent` | The percent more damage a target under Shrink takes |
| `FreezeDamagePercent` | The percent more damage a frozen target takes |
| `WindowDamage` | True to compute hits from the fighters' status window values instead of from the stats |
| `SkillPowerPerLevel` | How much of a skill's power it gains with each level above 1 when WindowDamage is on: 0.412 makes level 18 eight times level 1; 0 means the level changes nothing |
| `HotFireOn` | Whether the Hot Fire multiplier applies to the attack value (Attack) or to the finished damage (Damage) |
| `HotFireMultiplier` | The multiplier Hot Fire Attack applies |
| `MaxDamagePerHit` | The most damage one hit can do when WindowDamage is on; 0 or less means no cap |
| `FervidityMessPercent` | The chance in percent that a Fervidity hit puts mess on a target it left standing |
| `BuffSharePercent` | The percent of their pet's experience a player gets after buffing it |
| `DefenceSaddleDivisor` | The divisor of the pet's defence the Defence and Spirit Saddles lend the rider |
| `FleeChancePercent` | The base chance in percent that a flee attempt succeeds |
| `FleeSpeedWeight` | How strongly the speed lead changes the flee chance factor |
| `FleeLevelStep` | How much each level above the enemies' average changes the flee chance factor |
| `FleeFactorMin` | The lowest factor applied to FleeChancePercent |
| `FleeFactorMax` | The highest factor applied to FleeChancePercent |
| `CapturedPetAmity` | The amity a captured monster joins with; 0 means the usual amity of a new pet |
| `CaptureBasePercent` | The base chance in percent of a capture, before the missing-HP part and the level factor |
| `CaptureHpPercent` | The chance in percent a capture gains when all of the monster's HP is missing, scaled by the share that is missing |
| `CaptureLevelStep` | How much the capture level factor changes for each level the catcher is above the monster |
| `CaptureLevelFactorMin` | The lowest capture level factor, which is also the factor for a monster above the catcher's level |
| `CaptureLevelFactorMax` | The highest capture level factor |
| `CaptureLevelsAbove` | How many levels above the catcher a monster may be and still be captured |
| `MonsterNetLevels` | The levels added to CaptureLevelsAbove while the catcher wears a Monster Net that is not used up |
| `CaptureHpBelowPercent` | A monster whose HP is this percent of its maximum or more cannot be captured; 0 turns the rule off |

## progression.json

| Setting | What it does |
|---|---|
| `MonsterExpBase` | The base term of the experience a monster pays |
| `MonsterExpPerLevel` | The experience term a monster pays per monster level |
| `ExpPerLevelUnderMonster` | The experience term added per level the monster is above the fighter |
| `ExpPerLevelOverMonster` | The experience term subtracted per level the monster is below the fighter |
| `ExpSoloKillFactor` | The share of a monster's experience a fighter earns for felling it alone |
| `ExpComboKillFactor` | The share of a monster's experience a fighter earns for felling it in a combination |
| `ExpHitFactor` | The share of a monster's experience a fighter earns for hitting it without felling it |
| `ExpPerExtraAction` | The share added to the experience for each action after the first on a monster |
| `LevelGapFullUpTo` | The level lead over a monster up to which its experience is paid in full |
| `LevelGapZeroFrom` | The level lead over a monster from which no experience is paid |
| `TrainingIslandFlatExp` | The experience a win on the Training Island pays when the level gap would leave nothing |
| `PetDefeatAmityLoss` | The amity a pet loses when it falls in a battle |
| `PetDefeatExpLossPercent` | The percent of the current level's exp a pet loses when it falls; 0 means it loses amity only |
| `PetAmityPerLevel` | The amity a pet gains for each level it gains |
| `PetRunAwayAmity` | The amity below which a pet that fell can run away; 0 means no pet runs away |
| `PetRunAwayChancePercent` | The chance in percent (0..100) that a pet below PetRunAwayAmity runs away after a fall |
| `PetStatsPerLevel` | The points added to each of a pet's five abilities for each level it gains |
| `PetBonusStatsPerLevel` | The extra points added to a pet's highest ability for each level it gains |

## world.json

| Setting | What it does |
|---|---|
| `Spawn` | Where new characters start |
| `Spawn.MapId` | The map id |
| `Spawn.X` | The x position on the map |
| `Spawn.Y` | The y position on the map |
| `StarterKit` | The items a new character starts with |
| `StarterKit.Enabled` | Whether new characters get the starter kit |
| `StarterKit.Equip` | Item ids put on after the role's default outfit |
| `StarterKit.Bag` | Item ids put in the bag |
| `BagSortFirst` | The item ids the bag sort puts first, in this order: Notepad, Remote Control, Electron Control, Tent |
| `NpcSpawnState` | The state sent for scene objects at map entry; a value outside 0 to 255 sends none |
| `GroundItemRespawnSeconds` | The seconds until a taken map item is back on the ground |
| `DroppedItemSeconds` | The seconds a dropped item stays on the ground |
| `DropScatterPixels` | How far in pixels, on each axis, a dropped item or gold can land from the player; 0 drops it at their feet |
| `CompassStreakForLife` | The right answers in a row that give a Golden Compass player a life back; 0 or less turns it off |
| `LifeSkillExp` | The skill experience a life skill gains per use: an item gathered, a fish caught, or a compound that makes an item |
| `Durability` | Equipment wear settings |
| `Durability.WearPerBattle` | The wear added to each worn piece after a battle that wears gear; 0 switches wear off |
| `Durability.WearMinLevelGap` | How many levels the highest monster must be above the player for a battle to wear gear; 0 or less makes every battle wear |
| `Durability.GunWearPerBattle` | The wear a gun gets after every battle, whatever the level gap; 0 for none |
| `Durability.PestleWearPerBattle` | The wear the Attack Pestle gets after every battle, whatever the level gap; 0 for none |
| `WorldChatSp` | The SP a world chat message costs a player wearing the Radio Set |
| `Guilds` | The rules for guilds, such as the dismissal checks |
| `Guilds.AutoDismiss` | Whether guilds that miss the checks are dismissed |
| `Guilds.RecruitDays` | Days after founding in which the guild must recruit enough members |
| `Guilds.RecruitMembers` | How many members of RecruitLevel the guild needs after RecruitDays |
| `Guilds.RecruitLevel` | The level the recruited members need |
| `Guilds.OnlineDays` | Days after founding in which enough members must be online together once |
| `Guilds.OnlineMembers` | How many members must be online together once |
| `Guilds.SweepMinutes` | Minutes between two dismissal checks; 0 or less switches the check off |
| `Guilds.MaxGuilds` | The most guilds the server holds; 0 means no limit |
| `HotSpring` | Settings of the hot springs |
| `HotSpring.Enabled` | Switches the hot springs on or off |
| `HotSpring.MinutesPerPoint` | Minutes in the water per bonus step |
| `HotSpring.MaxPerVisit` | The most steps one visit gives |
| `HotSpring.PointsPerStep` | The bonus points added to the character's maximum HP and SP per step |

## mall.json

| Setting | What it does |
|---|---|
| `ItemMall` | The item mall settings and catalog |
| `ItemMall.RotationGroups` | Number of rotation groups; 0 turns rotation off |
| `ItemMall.RotationDays` | Days each rotation group stays on sale |
| `ItemMall.Items` | The configured mall rows; when empty, DefaultItems is used |

### Rows of `ItemMall.Items`

Each row: Item = the item id sold; Category = the mall tab it appears in; Price = price of one unit in mall points; Hot = hot marker sent to the client (0 or missing for none); Rotation = rotation group (0 or missing means always shown). Rows may also set Pack (items per purchase, 1 if missing) and Discount (percent of the price charged, 100 if missing).

| Item | Name |
|---|---|
| 26001 | Sky Sword Box |
| 26002 | Lord Sword Box |
| 26003 | Beast Fork Box |
| 26004 | Spirit Fork Box |
| 26005 | Polished Wand Box |
| 26006 | Light Wand Box |
| 26007 | Attack Pestle Box |
| 26015 | Crescent Bow Box |
| 26016 | Golden Rings Box |
| 26019 | Thunder Spear Box |
| 34170 | Weapon Model |
| 21159 | Defense Armor |
| 21172 | Fur Clothes (Male) |
| 21173 | Fur Clothes(Female) |
| 21234 | Kanako's Jaket |
| 21235 | Red Bellyband |
| 22166 | Snow Ears |
| 22233 | Thunder Hairpin |
| 22235 | Deity Headscarf |
| 22236 | Fairy Crest |
| 23138 | Gold Rings |
| 23164 | Emerald Armlet |
| 23165 | Plasma Machine |
| 23166 | Demon Bell |
| 23167 | Mayan Gloves |
| 24179 | Cowboy Boots |
| 24180 | Strong Beast Boots |
| 24181 | Angel Rope Boots |
| 24182 | Thunder Boots |
| 25198 | Steel Hammer 99T |
| 25207 | Friendship Brooch |
| 25228 | Steel Hammer 200T |
| 25229 | Steel Hammer 400T |
| 25230 | Steel Hammer 600T |
| 25567 | Energy Source |
| 26008 | Energy Halo Box |
| 26009 | Golden Ears Box |
| 26010 | Stand Ears Box |
| 26011 | Jade Ears Box |
| 26012 | Dark Evil Wing Box |
| 26013 | Angel Wings Box |
| 26014 | Archangel Wings Box |
| 26017 | Purple Wing Robe Box |
| 26018 | Kung-Fu Suit Box |
| 34167 | Sewing Tools |
| 25196 | VIP Card |
| 25208 | Swift Saddle |
| 25218 | Monster Net |
| 25535 | Bash Saddle |
| 25536 | Magic Saddle |
| 25540 | Decelerate Saddle |
| 25604 | Fast Jet |
| 27018 | Braised Tilapia |
| 27019 | Blessing Strawberry |
| 30099 | Key of Gold Box |
| 30101 | Strong Scroll |
| 30124 | Registration Voucher |
| 30133 | Boarded Voucher |
| 30135 | Wishing Coin |
| 32163 | Fruit Cake |
| 32164 | Neapolitan Sundaes |
| 32173 | Pork Bone Noodle |
| 32174 | Pork Miso Noodle |
| 33054 | Lollipop |
| 34006 | Normal GPS |
| 34007 | Carnie Token |
| 34010 | Alchemy Book 2 |
| 34011 | Alchemy Book 3 |
| 34015 | Glazed Stone Bag |
| 34017 | Dog Bone |
| 34018 | Transfer Mirror |
| 34020 | Magic Set Box |
| 34021 | Magic Magnet |
| 34022 | Tent Palette |
| 34025 | Pet Egg |
| 34026 | Protective EXP Pill |
| 34073 | Mixed Noodles |
| 34099 | 2X EXP Copper Card |
| 34100 | 2X EXP Silver Card |
| 34101 | 2X EXP Gold Card |
| 34102 | 4 Leaves Grass |
| 34103 | 5 Leaves Grass |
| 34104 | Repairing Spanner |
| 34105 | Repairing Potion |
| 34119 | Copper Amity Jelly |
| 34120 | Silver Amity Jelly |
| 34121 | Gold Amity Jelly |
| 34128 | Electron Control |
| 34129 | Big Vacuum Cleaner |
| 34132 | Loudspeaker |
| 34133 | Space Contract |
| 34137 | Pet Lethe Scroll |
| 34155 | Small Tear Plier |
| 34160 | Shape Clay |
| 34163 | Teleport Scroll |
| 34164 | Escape Button |
| 34166 | Delay Clock |
| 34168 | Time Mantle |
| 34169 | Bamboo Dragonfly |
| 34171 | Gift Pack |
| 34180 | Stopwatch |
| 34181 | Training Voucher |
| 34192 | War Pack |
| 34193 | Water Pack |
| 34198 | Rebirth Box |
| 34200 | Present Case |
| 34229 | Gear Pack |
| 34235 | Spring Fireworks |
| 34236 | Chili Sauce |
| 34249 | Wasabi |
| 34269 | Potential Pill |
| 34281 | Healing Potion |
| 34282 | SP Potion |
| 34283 | Super HP Potion |
| 34284 | Super SP Potion |
| 35100 | Training Box |
| 37065 | Coal Block |
| 37075 | Liquid Gas |
| 37090 | Wisdom Crystal |
| 37101 | Magic Wool Ball |
| 37143 | Magnet Stone |
| 37248 | High Uranium |
| 37249 | Fine Uranium |
| 37257 | Memory Crystal |
| 47011 | Unleaded Gasoline |
| 47012 | Great Gasoline |
| 47013 | Diesel Oil |
| 60008 | Hot Water |
| 25194 | Fixed Stick |
| 27002 | Great Syrup |
| 27003 | Roca Perfume |
| 27006 | Elin's Oil |
| 27011 | Cotton Candy |
| 27012 | Wolf Deity Bean |
| 27013 | Delicate Riceball |
| 27014 | Great Soup |
| 27015 | Angel's Feather |
| 27016 | Magic Bean |
| 27017 | Mellow Wine |
| 30067 | Yacht Voucher |
| 30072 | Holy Spirit Voucher |
| 30086 | Jade Rabbit Voucher |
| 30119 | Snow Monster Voucher |
| 30121 | Mammoth Voucher |
| 30163 | Christmas Bell |
| 30178 | Cocoa Nib |
| 30179 | Soft Sweet |
| 30238 | Gold Griffin Voucher |
| 30303 | Cerberus Ticket |
| 33027 | Grenade |
| 33053 | Korean Cake |
| 33055 | Big Bowling |
| 34012 | Alchemy Book 4 |
| 34030 | Glaze Bag |
| 34153 | Sky Banger |
| 34154 | Dragon Banger |
| 34172 | 7 Colors Egg |
| 34199 | Strengthen Pack |
| 34201 | Transferred Pack |
| 34234 | New Year Box |
| 34240 | Spring Riddlepaper |
| 34241 | New Year Pack |
| 34248 | Scroll Pack |
| 34302 | Anniversary Big Bag |
| 34306 | Crystal Ice Cube |
| 34309 | Super Gift Box |
| 34311 | Friendship Pumpkin |
| 34316 | Thanksgiving Egg |
| 34396 | 2010 Jolly Pack |
| 35103 | Maid Box |
| 55009 | Super Reunion Pack |
| 55013 | 2011 Ghost Pack |
| 55017 | 2011 Big Egg |
| 61011 | Nimbus Spar |
| 61031 | Nimbus Diamond |
| 61050 | Dark Spar |
| 61056 | Fairy's Spar |
| 62005 | 18% Bomb Diamond |
| 62006 | 20% Bomb Diamond |
| 34157 | Cherry Door Voucher |
| 34158 | Beach Door Voucher |
| 34175 | Seasonal Suit |
| 34176 | Rabbit Suit |
| 34177 | Cat Suit |
