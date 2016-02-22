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
            if(Config.getAutoGG && sender.IsAlly && args.Message.ToLower().Equals("gg") && !sender.IsMe) {
                Chat.Say("/all gg");
            }
        }

        /// <summary>
        /// Environment onTick overload. All hack's functions.
        /// </summary>
        /// <param name="args">EventArgs</param>
        private static void OnTick(EventArgs args) {
            AIHeroClient pingTarget = Config.getPingTarget(); // Grab the ping target
            if(pingTarget != null) ProcessPing();

            if(Config.getEmoteToggled) ProcessEmote();
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