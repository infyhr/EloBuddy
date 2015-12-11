using System;
using EloBuddy;
using EloBuddy.SDK.Events;

namespace Katakomba {
    class Program {
        static void Main(string[] args) {
            if(args != null) {
                try {
                    Loading.OnLoadingComplete += Game_OnStart;
                } catch(Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void Game_OnStart(EventArgs args) {
            if(ObjectManager.Player.ChampionName.ToLower() == "katarina") Katakomba.Init();
        }
    }
}