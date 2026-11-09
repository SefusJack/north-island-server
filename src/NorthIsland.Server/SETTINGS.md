# Settings

The keys below are in the `Server` section unless they name another one. The section is merged from `appsettings.json` and the files in `config/`.

The files in `config/` have their own `SETTINGS.md` there.

## appsettings.json

| Setting | What it does |
|---|---|
| `BindAddress` | The IP address the game and status listeners bind to |
| `GamePort` | The TCP port of the game connection |
| `StatusPort` | The TCP port of the status connection; 0 turns it off |
| `ServerId` | The server id sent to the client in the server-selected packet and the status record |
| `ServerName` | The server name sent to the client in the server-selected packet |
| `ServerFlag` | The flag sent with the server-selected packet |
| `ServerStatus` | The status byte of the status record |
| `ClientVersions` | The login versions the server accepts; when empty, the 2010 and 2014 versions |
| `ItemDatSize` | The expected size of the client's Item.dat; null for none |
| `EnforceItemDatSize` | True to kick a login whose Item.dat size differs from ItemDatSize |
| `AutoRegister` | True to create an account when an unknown name logs in |
| `SkipCreationCipher` | True to tell the client to skip the cipher step of character creation |
| `AfterCreate` | What happens after a character is created: EnterWorld or CharacterList |
| `GameDataDirectory` | The directory of the client's data files or of the game; empty to search this server's folder, then the default install |
| `DataDirectory` | The directory of the server's own data (accounts and similar files) |
| `LogDirectory` | The directory of the log files |
| `AutosaveMinutes` | The minutes between autosaves; 0 or less turns them off |
| `MonsterDropChancePercent` | The chance in percent that a defeated monster drops one item of its item list |
| `FieldEncounterPercent` | The chance in percent that a step of a free player starts a field fight; 0 turns field fights off |
| `ExpMultiplierCap` | The largest product of the seasonal and potion experience multipliers; 0 or less means no cap |
| `MonsterDamageScale` | A factor applied to the damage monsters do |
| `TradeOnlineHours` | The hours an account must have been online before it may trade |
| `DefeatExpLossPercent` | The percent of the current level's experience a player loses when they end a battle down; 0 means no loss |
| `StatPointsPerLevel` | Stat points a character gets for every level gained |
| `SkillPointsPerLevel` | Skill points a character gets for every level gained |
| `LogPackets` | True to log every packet in both directions, typed passwords included |
| `Logging` | the log filter |
| `Logging.LogLevel` | the lowest level logged, per source |
| `Logging.LogLevel.Default` | the server's own messages |
| `Logging.LogLevel.Microsoft` | the messages of the .NET framework |

## events.json

```text
The seasonal event calendar and the weekly activity hours.
Copy this file to the data folder (data/events.json) to change it; that copy wins over this one.

events[]:
  name           for the log
  mode           "Auto" (on inside its window), "On" (always, for testing), "Off"
  start, end     yearly window "MM-dd", both days included; the end may be in the next year (12-31 .. 01-01)
  years          per-year windows for the lunar festivals, e.g. "2027": ["06-07", "06-16"]; they replace start / end
                 that year (a lunar festival with no row for this year is off)
  days           the days of the week the event is on, e.g. ["Saturday", "Sunday"]; used instead of the dates above
  expMultiplier  battle exp x this while on (no icon on the client, it just gets the exp)
  drops          the event drop table while on: { item, percent }. A defeated monster rolls it once, apart from its own
                 drop, and gets at most one row (the rows of every event on together; chance = the sum of the percents)
  startNotice    the client's own event line (S 23/57, RecvEventNotice) shown when the event begins and at login while
  endNotice      it is on / to everyone online when it ends; 0 or missing = none. The texts are the client's;
                 which entry an event uses is our choice
  scripts        eve.Emg events whose date conditions (kind 18, pinned to 2006-2009) follow this event instead of the
                 real date: { map, events: [event indices] } (normal and lead events share the numbering)

activities[]: weekly hours of the lobby's Q&A ("quiz") and Golden Compass ("compass"), used only when the server
option ActivitiesAlwaysOpen is false. Server local time.
```

### Notes on single events

Both end dates are included. The lunar festivals move every year, so their dates are listed under `years` in `events.json`.

| Event | When | In the game |
|---|---|---|
| New Year | Dec 31 to Jan 1 | Monsters can drop the New Year gift box. |
| Chinese New Year | 2 days before to 4 days after the lunar New Year | The Year Beast quest, Aunt Zhuang's fairy and the Red Pack wishes in Fishing Village, plus event drops. |
| Valentine's Day | Feb 13 to Feb 15 | The Western Valentine's quest in Holy Village, plus event drops. |
| Dragon Boat Festival | 2 days before to 7 days after the 5th of the 5th lunar month | The Dragon Boat quest and Qu Yuan's story in China, Xu Xian's trouble in Chang'an, and Dumpling drops. |
| Qixi | 2 days before to 8 days after the 7th of the 7th lunar month | Ming and Ling's wedding invitations in Chang'an and the wishes on the bamboo in Kyoto, plus Qixi stories in Chang'an and Fishing Village. |
| Mid-Autumn Festival | 2 days before to 7 days after the 15th of the 8th lunar month | The Mid-Autumn story in Chang'an, the Jade Rabbit in China, and Reunion Pack drops. |
| Halloween | Oct 30 to Nov 1 | Double experience and pumpkin drops. |
| Christmas | Dec 24 to Dec 30 | Santa Claus appears in Holy Village, plus event drops. |
| White Day | Mar 12 to Mar 24 | The White Day event in Holy Village. |
| Lantern Festival | 2 days before to 3 days after the 15th of the 1st lunar month | The Lantern Festival stories in Chang'an. |
| Ghost Month | 6 days before to 8 days after Qixi | The Night Parade of a Hundred Demons in Fishing Village. |
| Summer | Aug 10 to Aug 29 | The Summer Event in Holy Village. |
| Thanksgiving | Nov 26 to Dec 10 | The Thanksgiving event. |
| Double XP Weekend | Every Saturday and Sunday | Double experience. |

National Treasure runs on Mondays and the 15-round challenge on Fridays, both from 7 to 9 pm server time.

## quiz.json

Q&A questions (map 10079). 'correct' is 1-4. From a video of the original event; arithmetic questions are also generated (Server:QuizMathPercent). Add more freely. Prizes by final place (players still in at the end), modelled on the 15-round challenge's rewards; a full bag gets nothing.
