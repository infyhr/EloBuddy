using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using System.Drawing;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace ToxyCity {
    class ToxyCity {
        private static AIHeroClient myHero;
        private static string version = "1.0.0.0";
        private static int lastPing;
        private static string[] macroSentences = {
            "good shit go౦ԁ sHit👌 thats ✔ some good👌👌shit right👌👌there👌👌👌 right✔there ✔✔if i do ƽaү so my self 💯 i say so 💯 thats what im talking about right there right there (chorus: ʳᶦᵍʰᵗ ᵗʰᵉʳᵉ) mMMMMᎷМ💯 👌👌 👌НO0ОଠOOOOOОଠଠOoooᵒᵒᵒᵒᵒᵒᵒᵒᵒ👌 👌👌 👌 💯 👌 👀 👀 👀 👌👌Good shit",
            "do NOT sign me the FUCK up 👎👀👎👀👎👀👎👀👎👀 bad shit ba̷̶ ԁ sHit 👎 thats ❌ some bad 👎👎shit right 👎👎 th 👎 ere 👎👎👎 right ❌ there ❌ ❌ if i do ƽaү so my selｆ🚫 i say so 🚫 thats not what im talking about right there right there (chorus: ʳᶦᵍʰᵗ ᵗʰᵉʳᵉ) mMMMMᎷМ 🚫 👎 👎👎НO0ОଠＯOOＯOОଠଠOoooᵒᵒᵒᵒᵒᵒᵒᵒᵒ 👎 👎👎 👎 🚫 👎 👀 👀 👀 👎👎Bad shit",
            "hard",
            "do NOT sign me the FUCK up 👎👀👎👀👎👀👎👀👎👀 bad SCRIPT baddԁ sCriPt 👎 thats ❌ some bad 👎👎script right 👎👎 th 👎 ere 👎👎👎 right ❌ there ❌ ❌ if i do ƽaү so my selｆ🚫 i say so 🚫 thats not what im talking about right there right there (chorus: ʳᶦᵍʰᵗ ᵗʰᵉʳᵉ) mMMMMᎷМ 🚫 👎 👎👎НO0ОଠＯOOＯOОଠଠOoooᵒᵒᵒᵒᵒᵒᵒᵒᵒ 👎 👎👎 👎 🚫 👎 👀 👀 👀 👎👎Bad scriptbadscript",
            "gooooooooood scriptgood script THATS SOME GUD SKRIPT right there if i do ayyyyy so my sel ayyyyyy so hOOOOOOO00000000000000 goodscriptgoodscript sign me the FUCK UP goodskript banned when",
            "Anger management is training for temper control and is the skill of remaining calm and composed.[1] It has been described as deploying anger successfully.[1] Anger is frequently a result of frustration, or of feeling blocked or thwarted from something we feel to be important. Anger can also be a defensive response to underlying fear or feelings of vulnerability or powerlessness.[2] Anger management programs consider anger to be a motivation caused by an identifiable reason which can be logically analyzed and if suitable worked toward.[1]",
            "Gr8 b8, m8. I rel8, str8 appreci8, and congratul8. I r8 this b8 an 8/8. Plz no h8, I'm str8 ir8. Cr8 more, can't w8. We should convers8, I won't ber8, my number is 8888888, ask for N8. No calls l8 or out of st8. If on a d8, ask K8 to loc8. Even with a full pl8, I always have time to communic8 so don't hesit8. dont forget to medit8 and particip8 and masturb8 to allevi8 your ability to tabul8 the f8. We should meet up m8 and convers8 on how we can cre8 more gr8 b8, I'm sure everyone would appreci8, no h8. I don't mean to defl8 your hopes, but its hard to dict8 where the b8 will rel8 and we may end up with out being appreci8d, I'm sure you can rel8. We can cre8 b8 like alexander the gr8, stretch posts longer than the Nile's str8s. We'll be the captains of b8, 4chan our first m8s the growth r8 will spread to reddit and like real est8 and be a flow r8 of gr8 b8, like a blind d8 we'll coll8, meet me upst8 where we can convers8, or ice sk8 or lose w8 infl8 our hot air baloons and fly, tail g8. We could land in Kuw8, eat a soup pl8 followed by a dessert pl8 the payment r8 won't be too ir8 and hopefully our currency won't defl8. We'll head to the Israeli-St8, taker over like Herod the gr8 and b8 the jewish masses, 8 million, m8. We could interrel8 communism, thought it's past it's maturity d8, a department of st8, volunteer st8. reduce the infant mortality r8, all in the name of making gr8 b8 m8",
            "It-ji ma, it-ji ma, uriga ichiban Bich-i na, bich-i na, uliga bich-i na Singyeong kkeo, neoneun neo, naneun na We the killer whales, nalgae pigo don wieseo suyeong C-O-H-O-R-T, we the Cohort, pay these boys Motherfucker fucker, judung-iman ppeokkeumppeokkeum (fuck 'em) Neoneun neo, naneun na, bad-adeul-yeo eoseo Neoneun namdeul yoghal ttae naneun don beolleo"
        };

        /// <summary>
        /// EloBuddy initialization.
        /// </summary>
        public static void Init() {
            Bootstrap.Init(null);
            Loading.OnLoadingComplete += OnLoad;
        }

        public static string getVersion {
            get { return version; }
        }

        public static string[] getMacros {
            get { return macroSentences; }
        }

        /// <summary>
        /// Loads when the script is injected (successfully, hopefully).
        /// </summary>
        /// <param name="args">Event arguments</param>
        private static void OnLoad(EventArgs args) {
            // Register out champion
            myHero = ObjectManager.Player;

            // Initialize config
            Config.Initialize();
            lastPing = Environment.TickCount;

            // Main functions
            Game.OnTick += OnTick;
            Chat.OnClientSideMessage += OnClientSideMessage;
            Chat.OnMessage += OnMessage;
            Chat.Print("ToxyCity loaded, version " + version);
        }

        /// <summary>
        /// Hooks clientside messages.
        /// </summary>
        /// <param name="args"></param>
        private static void OnClientSideMessage(ChatClientSideMessageEventArgs args) {
            if(args.Message.StartsWith("You must wait")) args.Process = false; // Exploit?
        }

        /// <summary>
        /// Hooks any messages.
        /// </summary>
        /// <param name="args">EventArgs</param>
        /// <param name="sender">Sender</param>
        private static void OnMessage(AIHeroClient sender, ChatMessageEventArgs args) {
            
        }

        /// <summary>
        /// Environment onTick overload. All hack's functions.
        /// </summary>
        /// <param name="args">EventArgs</param>
        private static void OnTick(EventArgs args) {
            AIHeroClient pingTarget = Config.getPingTarget(); // Grab the ping target
            if(pingTarget != null) ProcessPing();

            if(Config.getEmoteToggled) ProcessEmote();
            if(Config.getCheckedMacro() != -1) ProcessMacro();
        }

        /// <summary>
        /// Processes checked macro.
        /// </summary>
        private static void ProcessMacro() {
            if((Environment.TickCount - lastPing) <= Config.getMacroDelay) return;

            if(Config.getMacroAll) {
                Chat.Say("/all " + macroSentences[Config.getCheckedMacro()]);
            }else {
                Chat.Say(macroSentences[Config.getCheckedMacro()]);
            }

            lastPing = Environment.TickCount;
        }

        /// <summary>
        /// Spams emotes
        /// </summary>
        private static void ProcessEmote() {
            double tick = 0;
            tick = TimeSpan.FromSeconds(Environment.TickCount).Minutes;

            if(ObjectManager.Player.HasBuff("Recall")) return;

            if(tick == 59) {
                switch(Config.getEmoteMode) {
                    case 0:
                        Player.DoEmote(EloBuddy.Emote.Laugh);
                    break;
                    case 1:
                        Player.DoEmote(EloBuddy.Emote.Taunt);
                    break;
                    case 2:
                        Player.DoEmote(EloBuddy.Emote.Joke);
                    break;
                    case 3:
                        Player.DoEmote(EloBuddy.Emote.Dance);
                    break;
                    case 4:
                        Player.DoEmote(EloBuddy.Emote.Toggle);
                    break;
                }
            }
        }

        /// <summary>
        /// Spams pings.
        /// </summary>
        private static void ProcessPing() {
            if((Environment.TickCount - lastPing) <= Config.getPingTimeout) return;

            int nId = Config.getPingTarget().NetworkId; // Target networkId

            if(Config.pingIsRandomized) {
                Array values = Enum.GetValues(typeof(PingCategory));
                Random random = new Random();
                PingCategory randomBar = (PingCategory)values.GetValue(random.Next(values.Length));

                TacticalMap.SendPing(randomBar, ObjectManager.GetUnitByNetworkId(Convert.ToUInt32(nId)));
                lastPing = Environment.TickCount;
                return;
            }

            switch(Config.getPingMode) {
                case 1:
                    TacticalMap.SendPing(PingCategory.Normal, ObjectManager.GetUnitByNetworkId(Convert.ToUInt32(nId)).Position);
                break;
                case 2:
                    TacticalMap.SendPing(PingCategory.Fallback, ObjectManager.GetUnitByNetworkId(Convert.ToUInt32(nId)));
                break;
                case 3:
                    TacticalMap.SendPing(PingCategory.Danger, ObjectManager.GetUnitByNetworkId(Convert.ToUInt32(nId)));
                break;
                case 4:
                    TacticalMap.SendPing(PingCategory.AssistMe, ObjectManager.GetUnitByNetworkId(Convert.ToUInt32(nId)));
                break;
                case 5:
                    TacticalMap.SendPing(PingCategory.OnMyWay, ObjectManager.GetUnitByNetworkId(Convert.ToUInt32(nId)));
                break;
                case 6:
                    TacticalMap.SendPing(PingCategory.EnemyMissing, ObjectManager.GetUnitByNetworkId(Convert.ToUInt32(nId)));
                break;
            }

            lastPing = Environment.TickCount;
        }
    }
}
