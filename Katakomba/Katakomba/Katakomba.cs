using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Drawing;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;
using SharpDX;

namespace Katakomba {
    class Katakomba {
        private static AIHeroClient myHero; // Self
        private static Spell.Targeted Q;
        private static Spell.Active W;
        private static Spell.Targeted E;
        private static Spell.Active R;
        public static  SpellSlot IgniteSlot; // ignite
        private static Menu menu, ComboMenu, HarassMenu, KillStealMenu, EtcMenu; // menus
        private static bool _isChanneling; // channeling the ultimate

        private static string version = "1.1.2.0"; // Katakomba version

        private static AIHeroClient target; // enemy target
        private static InventorySlot wardSlot; // where our ward resides!
        private static string currentCombo; // Current Combo mode.

        // jumpKS stuff
        public static int LastPlaced; // Last placed tick time of the ward
        public static Vector3 LastWardPos; // Last placed position of the ward

        // Greezyness factor.
        public static float greezyNess;
        public static int lastDeath; // TickCount of when someone last died.

        /// <summary>
        /// EloBuddy initialization.
        /// </summary>
        public static void Init() {
            Bootstrap.Init(null);
            Loading.OnLoadingComplete += OnLoad;
        }

        /// <summary>
        /// Main menu configuration.
        /// </summary>
        private static void InitMenu() {
            menu = MainMenu.AddMenu("Katakomba", "Katakomba");

            // Some main crap
            menu.AddGroupLabel("Katakomba");
            menu.AddLabel("Version: " + version);
            menu.AddSeparator();
            menu.AddLabel("www.github.com/infyhr/EloBuddy");

            // Combo
            ComboMenu = menu.AddSubMenu("Combo", "KatakombaCombo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("combokey", new KeyBind("Combo Key", false, KeyBind.BindTypes.HoldActive, 'C'));
            ComboMenu.AddLabel("Remember to unbind orbwalker combo!");

            // Harrasment
            HarassMenu = menu.AddSubMenu("Harass", "KatakombaHarass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("autoharass", new CheckBox("Auto Harass with Q W?", true));

            // Killsteal
            KillStealMenu = menu.AddSubMenu("KillSteal", "KatakombaKillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("killsteal", new CheckBox("Do Killsteal?", true));
            KillStealMenu.Add("fleeks", new CheckBox("Flee KS", true));
            KillStealMenu.Add("useignite", new CheckBox("Use ignite?", true));

            // Etc menu
            EtcMenu = menu.AddSubMenu("Et cetera", "KatakombaEtc");
            EtcMenu.AddGroupLabel("To be added.");
            EtcMenu.Add("WardJump", new KeyBind("Ward Jump", false, KeyBind.BindTypes.HoldActive, 'Z'));
            EtcMenu.Add("draw", new CheckBox("Enable drawings", true));
        }

        private static void OnLoad(EventArgs args) {
            // Register out champion
            myHero = ObjectManager.Player;
            if(myHero.Hero != Champion.Katarina) return;

            // Register spell slots
            Q = new Spell.Targeted(SpellSlot.Q, 675);
            W = new Spell.Active(SpellSlot.W,   375);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R,   550);

            // Register the Ignite slot, if any
            IgniteSlot = Player.Spells.FirstOrDefault(o => o.SData.Name.ToLower().Contains("summonerdot")).Slot;

            // Initialize the menu
            InitMenu();

            // Main functions
            Game.OnTick += OnTick;
            Console.WriteLine("Katakomba Loaded successfully. Version " + version);
            Orbwalker.OnPreAttack     += Orbwalker_OnPreAttack;
            Player.OnProcessSpellCast += Player_OnProcessSpellCast;
            Drawing.OnDraw            += OnDraw;
        }

        /// <summary>
        /// Gets hit whenever a spell is being cast.
        /// </summary>
        /// <param name="sender">Who is casting</param>
        /// <param name="args">Caster's arguments</param>
        private static void Player_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            if(!sender.IsMe || args.SData.Name != "KatarinaR" || !myHero.HasBuff("katarinarsound")) return;

            _isChanneling = true;
            Console.WriteLine("Channeling ultimate . . .");
            Orbwalker.DisableMovement.Equals(true);
            Orbwalker.DisableAttacking.Equals(true);
            _isChanneling = false;
        }

        /// <summary>
        /// Function that hits before an attack has been made. Stops Katarina's antispin.
        /// </summary>
        /// <param name="target">Who is being attacked</param>
        /// <param name="args">Orbwalker attack arguments</param>
        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args) {
            if(myHero.IsMe) args.Process = !myHero.HasBuff("KatarinaR");
        }

        /// <summary>
        /// Figure out if we are in an ultimate state, channeling it.
        /// </summary>
        /// <returns>true if we are channeling</returns>
        private static bool InUltimate() {
            return myHero.HasBuff("KatarinaR") || Player.Instance.Spellbook.IsChanneling || myHero.HasBuff("katarinarsound");
        }

        /// <summary>
        /// Draws custom stuff on screen. Currently draws killable targets.
        /// </summary>
        /// <param name="args">EventArgs</param>
        private static void OnDraw(EventArgs args) {
            if(!EtcMenu["draw"].Cast<CheckBox>().CurrentValue) return;
            if(target == null) return;

            var hpPos = target.HPBarPosition;
            Drawing.DrawText(hpPos.X - 10, hpPos.Y + 40, Color.Pink, "Target " + currentCombo);
        }

        /// <summary>
        /// Environment onTick overload. All hack's functions.
        /// </summary>
        /// <param name="args"></param>
        private static void OnTick(EventArgs args) {
            // Figure if ult is being channeled, if so, stop with execution.
            if(InUltimate()) {
                Orbwalker.DisableMovement.Equals(true);
                Orbwalker.DisableAttacking.Equals(true);
            }

            // Process combo
            if(ComboMenu["combokey"].Cast<KeyBind>().CurrentValue) Combo();

            // Killsteal
            if(KillStealMenu["killsteal"].Cast<CheckBox>().CurrentValue) KillSteal();

            // Harass
            if(HarassMenu["autoharass"].Cast<CheckBox>().CurrentValue) AutoHarass();

            // WardJump
            if(EtcMenu["WardJump"].Cast<KeyBind>().CurrentValue) WardJump();

            // Orbwalker
            switch(Orbwalker.ActiveModesFlags) {
                // OrbWalker combomode
                case Orbwalker.ActiveModes.Combo:
                    // This must not be bound.
                break;

                // OrbWalker harass, we don't care about.
                case Orbwalker.ActiveModes.Harass:
                    // We don't care about this.
                break;

                // OrbWalker LaneClear mode.
                case Orbwalker.ActiveModes.LaneClear:
                    laneClear();
                break;

                // OrbWalker LastHit mode
                case Orbwalker.ActiveModes.LastHit:
                    // Not implemented... yet.
                break;

                // OrbWalker flee
                case Orbwalker.ActiveModes.Flee:
                    flee();
                break;
            }

            //Core.DelayAction(Greezyness, 10000); // Calculate greezyness every 10 seconds.
        }

        /// <summary>
        /// Calculates greezyness factor.
        /// </summary>
        public static void Greezyness() {
            /*
            greezynessFactor = (numofkills) * 1 + (numofdouble) * 2 + (numoftriple) * 3 + (numofquadra) * 4 + (numofpenta) * 5
            if(kill in <1s) greezynessFactor *= 1.1;
            ^^^^^^ -> hook onDeath event.
            */
        }

        /// <summary>
        /// Handles ignite (doesn't work always/buggy?)
        /// </summary>
        /// <param name="target"></param>
        private static void HandleIgnite() {
            if(!KillStealMenu["useignite"].Cast<CheckBox>().CurrentValue) return;
            var ignite = new Spell.Targeted(IgniteSlot, 600);
            if(ignite.IsReady() && ignite.IsInRange(target) && target != null) ignite.Cast(target);
        }

        /// <summary>
        /// Steals kills.
        /// </summary>
        private static void KillSteal() {
            target = TargetSelector.GetTarget(1375, DamageType.Magical);
            if(target == null) return;

            // QEW
            if(Q.IsInRange(target.ServerPosition) && E.IsInRange(target.ServerPosition) && E.IsReady() && Q.IsReady() && W.IsReady() &&
              (Helpers.MyDamage(myHero, target, true, true, true) > target.Health)) {
                Katakomba.currentCombo = "(QEW)";
                Console.WriteLine("KS: QEW");
                Q.Cast(target);
                E.Cast(target);
                W.Cast();
                HandleIgnite();
                return;
            }

            // EQ(W)
            if(E.IsInRange(target.ServerPosition) && E.IsReady() && Q.IsReady() &&
              (Helpers.MyDamage(myHero, target, true, false, true) > target.Health)) {
                Katakomba.currentCombo = "(EQW)";
                Console.WriteLine("KS: EQ");
                E.Cast(target);
                Q.Cast(target);
                W.Cast();
                HandleIgnite();
                return;
            }

            // EW
            if(E.IsInRange(target.ServerPosition) && E.IsReady() && W.IsReady() &&
               (Helpers.MyDamage(myHero, target, false, true, true) > target.Health)) {
                Katakomba.currentCombo = "(EW)";
                Console.WriteLine("KS: EW");
                E.Cast(target);
                W.Cast();
                HandleIgnite();
                return;
            }

            // E
            if(E.IsInRange(target.ServerPosition) && E.IsReady() && (Helpers.MyDamage(myHero, target, false, false, true, false)) > target.Health) {
                Katakomba.currentCombo = "(E)";
                Console.WriteLine("KS: E");
                E.Cast(target);
                HandleIgnite();
                return;
            }

            // Q + wardJump + Shunpo environment
            if(KillStealMenu["fleeks"].Cast<CheckBox>().CurrentValue) {
                if((Helpers.MyDamage(myHero, target, true, false, false) > target.Health) && !Q.IsInRange(target.ServerPosition) && Q.IsReady()) {
                    Katakomba.currentCombo = "(WARD)";
                    Console.WriteLine("ward ks! (1)");
                    jumpKS();
                    HandleIgnite();
                    Console.WriteLine("ward ks! (2)");
                    return;
                }
            }

            Katakomba.currentCombo = "";
        }

        /// <summary>
        /// Jumps and steals kills
        /// </summary>
        private static void jumpKS() {
            // Try to jump to any ward at first
            foreach(Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward =>
               E.IsReady() && Q.IsReady() && ward.Name.ToLower().Contains("ward") &&
               ward.Distance(target.ServerPosition) < Q.Range && ward.Distance(myHero.Position) < E.Range)) {
                E.Cast(ward);
                return;
            }

            // If that fails, try to jump to any hero
            foreach(Obj_AI_Base hero in ObjectManager.Get<Obj_AI_Base>().Where(hero =>
               E.IsReady() && Q.IsReady() && hero.Distance(target.ServerPosition) < Q.Range &&
               hero.Distance(myHero.Position) < E.Range && hero.IsValidTarget(E.Range))) {
                E.Cast(hero);
                return;
            }

            // Finally, try a minion.
            foreach(Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion =>
               E.IsReady() && Q.IsReady() && minion.Distance(target.ServerPosition) < Q.Range &&
               minion.Distance(myHero.Position) < E.Range && minion.IsValidTarget(E.Range))) {
                E.Cast(minion);
                return;
            }

            // Cast Q if we are in range now
            if(myHero.Distance(target.Position) < Q.Range) {
                Q.Cast(target);
                return;
            }

            // If we can't shunpo then there is no reason to even place the upcoming ward
            if(Environment.TickCount <= LastPlaced + 3000 || !E.IsReady()) return;

            // Calculate the ideal position of our ward
            Vector3 position = myHero.ServerPosition + Vector3.Normalize(target.ServerPosition - myHero.ServerPosition) * 590;

            // If the distance now is Q-able, wardKs away!
            if(target.Distance(position) < Q.Range) {
                // Find the best ward slot.
                wardSlot = myHero.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Warding_Totem_Trinket || a.Id == ItemId.Vision_Ward || a.Id == ItemId.Stealth_Ward || a.Id == ItemId.Greater_Vision_Totem_Trinket || a.Id == ItemId.Greater_Stealth_Totem_Trinket || a.Id == ItemId.Sightstone);
                if(wardSlot == null) return;

                // Cast and log.
                wardSlot.Cast(position);
                LastWardPos = position;
                LastPlaced = Environment.TickCount;
            }

            // Last Q check, if we've jumped this will trigger.
            if(myHero.Distance(target.Position) < Q.Range) {
                Q.Cast(target);
            }
        }

        /// <summary>
        /// Main combo. OrbWalker combo MUST be unbound.
        /// </summary>
        private static void Combo() {
            target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if(target == null) return;

            // Cast Zhonya while in combo mode.
            if(Helpers.CastZhonya(myHero)) {
                _isChanneling = false;
                return; // Stop comboing. No point.
            }

            // If we can Q them, why not. This will ensure that Q is in mid air until we land.
            if(Q.IsReady() && myHero.Distance(target.Position) <= Q.Range) Q.Cast(target);

            // E
            if(E.IsReady() && myHero.Distance(target.Position) <= E.Range) {
                // Disable orbwalking so we don't cancel our jump (lol, this can actually happen lmfao)
                Orbwalker.DisableAttacking.Equals(true);
                Orbwalker.DisableMovement.Equals(true);

                // Jump on 'em
                E.Cast(target);
            }

            // W
            if(W.IsReady() && myHero.Distance(target.Position) <= W.Range && Q.IsOnCooldown) W.Cast();

            // R
            if(R.IsReady() && myHero.CountEnemiesInRange(R.Range) > 0) {
                if(!Q.IsReady() && !E.IsReady()) {
                    Orbwalker.DisableAttacking.Equals(true);
                    Orbwalker.DisableMovement.Equals(true);
                    R.Cast();
                }
            }
        }

        /// <summary>
        /// Automatically harasses (Q+W) anyone in range.
        /// </summary>
        public static void AutoHarass() {
            target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if(target.IsValidTarget()) {
                if(Q.IsInRange(target)) Q.Cast(target);
                if(W.IsInRange(target)) W.Cast();
            }
        }

        // Soon (tm)
        public static void laneClear() {}

        /// <summary>
        /// Tries to flee away (orbWalker integration) by trying to jump to any ward, then hero, then minion. If all fails, casts a ward.
        /// </summary>
        public static void flee() {
            // Check if E is ready -- if not, no point.
            if(!E.IsReady()) return;

            // Try to sniff for any wards.
            foreach(var ward in ObjectManager.Get<Obj_Ward>().Where(ward => ward.Distance(myHero.Position) <= E.Range)) {
                Console.WriteLine("Found ward and should E to it: " + ward.Position + ward.Name);
                E.Cast(ward);
                return;
            }

            // Try to sniff for any ALLY hero
            foreach(Obj_AI_Base hero in ObjectManager.Get<Obj_AI_Base>().Where(hero => hero.Distance(myHero.Position) <= E.Range && !hero.IsDead)) {
                if(hero.Name != myHero.Name && !hero.Name.Contains("Turret") && hero.IsAlly) {
                    Console.WriteLine("Found hero and should E to it: " + hero.Position + hero.Name);
                    E.Cast(hero);
                    return;
                }
            }

            // Try to sniff for any minion
            foreach(Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.Distance(myHero.Position) <= E.Range)) {
                Console.WriteLine("Found minion and should E to it: " + minion.Position + minion.Name);
                E.Cast(minion);
                return;
            }

            // All seem to have failed, WardJump instead.
            WardJump();
        }

        /// <summary>
        /// Tries to cast any ward/trinket and then proceed to jump on it.
        /// </summary>
        public static void WardJump() {
            wardSlot = myHero.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Warding_Totem_Trinket || a.Id == ItemId.Vision_Ward || a.Id == ItemId.Stealth_Ward || a.Id == ItemId.Greater_Vision_Totem_Trinket || a.Id == ItemId.Greater_Stealth_Totem_Trinket || a.Id == ItemId.Sightstone);
            if(wardSlot == null || !Player.GetSpell(wardSlot.SpellSlot).IsReady) return;
            
            // Try to calculate our ideal ward position.
            Vector3 position = myHero.ServerPosition + Vector3.Normalize(Game.CursorPos - myHero.ServerPosition) * 590;

            // anti spam wards.
            if(Environment.TickCount <= LastPlaced + 3000) return;

            wardSlot.Cast(position);
            if(E.IsReady()) {
                E.Cast(position);
                // if it doesn't work then try again
                foreach(var ward in ObjectManager.Get<Obj_Ward>().Where(ward => ward.Distance(myHero.Position) <= E.Range)) {
                    E.Cast(ward);
                    return;
                }
            }

            LastWardPos = position;
            LastPlaced = Environment.TickCount;
        }
    }
}