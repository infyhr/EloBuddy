using System;
using EloBuddy;
using EloBuddy.SDK.Events;

namespace Katakomba {
    class Program {
        static void Main(string[] args) {
            Loading.OnLoadingComplete += Game_OnStart;
        }

        private static void Game_OnStart(EventArgs args) {
            if(ObjectManager.Player.ChampionName.ToLower() == "katarina") Katakomba.Init();
        }
    }
}