using System;
using EloBuddy;
using EloBuddy.SDK.Events;

namespace ToxyCity {
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
            ToxyCity.Init();
        }
    }
}
