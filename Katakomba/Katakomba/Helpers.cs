using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace Katakomba {
    class Helpers {
        //public static InventorySlot zhonyaSlot; // where our zhonya lives
        public static Item zhonya; // Zhonya test

        /// <summary>
        /// Calculates our hero's damage.
        /// </summary>
        /// <param name="myHero">Reference to AIHeroClient -- ourselves</param>
        /// <param name="target">Reference to AIHeroClient -- the target</param>
        /// <param name="useQ">Consider Q damage</param>
        /// <param name="useW">Consider W damage</param>
        /// <param name="useE">Consider E damage</param>
        /// <param name="useMark">Consider mark damage (if any)</param>
        /// <returns>float of the damage</returns>
        public static float MyDamage(AIHeroClient myHero, AIHeroClient target, bool useQ, bool useW, bool useE, bool useMark = true) {
            float calculation = 0;

            if(useQ)    calculation += myHero.GetSpellDamage(target, SpellSlot.Q);
            if(useW)    calculation += myHero.GetSpellDamage(target, SpellSlot.W);
            if(useE)    calculation += myHero.GetSpellDamage(target, SpellSlot.E);
            if(useMark) calculation += target.HasBuff("katarinaqmark") ? myHero.GetSpellDamage(target, SpellSlot.Q) : 0;

            // Consider ignite damage
            var ignite = new Spell.Targeted(Katakomba.IgniteSlot, 600);
            calculation += ignite.IsReady() ? myHero.GetSpellDamage(target, Katakomba.IgniteSlot) : 0;

            return calculation;
        }

        /// <summary>
        /// Casts zhonya if Katarina is low on health.
        /// </summary>
        /// <param name="myHero">AIHeroClient reference -- ourselves</param>
        /// <returns>true if zhonya suceeded</returns>
        public static bool CastZhonya(AIHeroClient myHero, int zHealth) {
            zhonya = new Item((int)ItemId.Zhonyas_Hourglass);
            if(zhonya == null || !zhonya.IsReady() || !zhonya.IsOwned()) return false;

            if(myHero.HealthPercent <= zHealth) {
                zhonya.Cast();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates greezyness factor.
        /// </summary>
        public static void Greezyness(AIHeroClient myHero) {
            int newGreezyness;

            // Greezyness formula.
            newGreezyness = (myHero.ChampionsKilled + 2 * myHero.DoubleKills + 3 * myHero.TripleKills + 4 * myHero.QuadraKills + 5 * myHero.PentaKills) + 1;
            newGreezyness += myHero.Assists;

            // Compare the now calculated greezyness with the object's one.
            if(newGreezyness != Katakomba.greezyNess) {
                // If they differ, let the user know.
                Chat.Print("+" + (newGreezyness - Katakomba.greezyNess).ToString() + " greezyness factor");
            }

            // Update the object variable so it can be drawn correctly.
            Katakomba.greezyNess = newGreezyness;
        }
    }
}
