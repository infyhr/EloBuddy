using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace ToxyCity {
    class Config {
        private static readonly Menu menu, PingMenu, EmoteMenu, EtcMenu;

        /// <summary>
        /// Initializes the main menu.
        /// </summary>
        static Config() {
            menu = MainMenu.AddMenu("ToxyCity", "toxycity");
            menu.AddGroupLabel("ToxyCity");
            menu.AddLabel("Version: " + ToxyCity.getVersion);
            menu.AddSeparator();
            menu.AddLabel("www.github.com/infyhr/EloBuddy");

            // Ping pong!
            PingMenu = menu.AddSubMenu("Pings", "Pings");
            foreach(var ally in EntityManager.Heroes.Allies.Where(x => !x.IsMe)) {
                PingMenu.Add("ping." + ally.ChampionName, new CheckBox("Ping " + ally.ChampionName, false));
            }
            PingMenu.AddSeparator();
            PingMenu.Add("ping.mode", new Slider("Ping mode", 1, 1, 6));
            PingMenu.Add("ping.timeout", new Slider("Ping timeout", 3000, 50, 10000));
            PingMenu.AddSeparator();
            PingMenu.Add("ping.randomize", new CheckBox("Randomize pings?", false));

            // Emotes
            EmoteMenu = menu.AddSubMenu("Emotes", "Emotes");
            var EmoteList = EmoteMenu.Add("emote.mode", new Slider("EmoteList", 0, 0, 4));
            EmoteList.OnValueChange += delegate {
                EmoteList.DisplayName = "Mode: " + new[]{"Laugh", "Taunt", "Joke", "Dance", "Mastery"}
                [EmoteList.CurrentValue];
            };
            EmoteMenu.Add("emote.toggle", new KeyBind("Toggle", false, KeyBind.BindTypes.PressToggle, 'L'));

            // Macros (user+premade), spam timeout and shit

            // etc, AutoGG (simple si)
            EtcMenu = menu.AddSubMenu("Et cetera", "Et cetera");
            EtcMenu.Add("etc.autogg", new CheckBox("Auto GG followup", true));
        }

        public static void Initialize() {}

        /// <summary>
        /// Returns the target being pinged.
        /// </summary>
        /// <returns></returns>
        public static AIHeroClient getPingTarget() {
            var target = EntityManager.Heroes.Allies.Where(it => !it.IsMe).FirstOrDefault(x => PingMenu["ping." + x.ChampionName].Cast<CheckBox>().CurrentValue);
            return target;
        }
        
        /// <summary>
        /// Returns the autogg mode
        /// </summary>
        public static bool getAutoGG {
            get { return EtcMenu["etc.autogg"].Cast<CheckBox>().CurrentValue; }
        }

        /// <summary>
        /// Returns the ping mode
        /// </summary>
        public static int getPingMode {
            get { return PingMenu["ping.mode"].Cast<Slider>().CurrentValue; }
        }
        
        /// <summary>
        /// Returns the ping timeout
        /// </summary>
        public static int getPingTimeout {
            get { return PingMenu["ping.timeout"].Cast<Slider>().CurrentValue; }
        }

        /// <summary>
        /// Returns whether the randomized ping feature is on
        /// </summary>
        public static bool pingIsRandomized {
            get { return PingMenu["ping.randomize"].Cast<CheckBox>().CurrentValue; }
        }

        /// <summary>
        /// Returns the emote mode
        /// </summary>
        public static int getEmoteMode {
            get { return EmoteMenu["emote.mode"].Cast<Slider>().CurrentValue; }
        }

        /// <summary>
        /// Returns true if the emote spam is on
        /// </summary>
        public static bool getEmoteToggled {
            get { return EmoteMenu["emote.toggle"].Cast<KeyBind>().CurrentValue; }
        }
    }
}
