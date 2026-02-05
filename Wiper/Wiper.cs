using System;
using System.Collections.Generic;
using System.IO;
using Fougerite;
using Fougerite.Caches;
using Fougerite.Concurrent;
using Fougerite.Events;
using Fougerite.Permissions;

namespace Wiper
{
    public class Wiper : Module
    {
        public IniParser Settings;
        public IniParser WhiteList;

        [Obsolete("This is not used anymore, it will be removed in future updates.", false)]
        public ConcurrentDictionary<ulong, DateTime> CollectedIDs = new ConcurrentDictionary<ulong, DateTime>();
        public Dictionary<string, float> EntityList = new Dictionary<string, float>(); 
        public List<ulong> WList = new List<ulong>(); 
        public bool UseDayLimit = true;
        public int MaxDays = 14;
        public bool UseDecay = false;
        public int DecayTimer = 30;
        public int WipeCheckTimer = 30;
        public bool Broadcast = true;
        public string UserDataPath = "\\rust_server_Data\\userdata\\";
        private static Wiper _inst;

        public override string Name
        {
            get { return "Wiper"; }
        }

        public override string Author
        {
            get { return "DreTaX"; }
        }

        public override string Description
        {
            get { return "Wiper"; }
        }

        public override Version Version
        {
            get { return new Version("1.2.0"); }
        }

        public static Wiper Instance
        {
            get { return _inst; }
        }

        public override void Initialize()
        {
            _inst = this;
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Settings.AddSetting("Settings", "UseDayLimit", "True");
                Settings.AddSetting("Settings", "MaxDays", "14");
                Settings.AddSetting("Settings", "UseDecay", "False");
                Settings.AddSetting("Settings", "DecayTimer", "30");
                Settings.AddSetting("Settings", "WipeCheckTimer", "30");
                Settings.AddSetting("Settings", "Broadcast", "True");
                Settings.AddSetting("Settings", "UserDataPath", "\\rust_server_Data\\userdata\\");
                Settings.Save();
            }
            else
            {
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
            }
            if (!File.Exists(Path.Combine(ModuleFolder, "WhiteList.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "WhiteList.ini")).Dispose();
                WhiteList = new IniParser(Path.Combine(ModuleFolder, "WhiteList.ini"));
                WhiteList.AddSetting("WhiteList", "1234212312312", "1");
                WhiteList.Save();
            }
            
            ReloadConfig();

            LoadDecayList();
            Hooks.OnCommand += OnCommand;

            if (UseDecay)
            {
                CreateTimer("Decay", DecayTimer * 60 * 1000, DecayCallback, true);
            }
            if (UseDayLimit)
            {
                CreateTimer("WipeCheck", WipeCheckTimer * 60 * 1000, WipeCheckCallback, true);
            }
        }

        public override void DeInitialize()
        {
            KillTimers();
            Hooks.OnCommand -= OnCommand;
        }

        private void DecayCallback(TimedEvent te)
        {
            if (UseDecay)
            {
                ForceDecay();
            }
        }

        private void WipeCheckCallback(TimedEvent te)
        {
            if (UseDayLimit)
            {
                if (Broadcast)
                {
                    Server.GetServer().BroadcastFrom("Wiper", "Server is checking for wipeable objects...");
                }
                LaunchCheck();
            }
        }

        public void LoadDecayList()
        {
            try
            {
                IniParser ini = new IniParser(Path.Combine(ModuleFolder, "Health.ini"));
                EntityList["WoodFoundation"] = float.Parse(ini.GetSetting("Damage", "WoodFoundation"));
                EntityList["WoodDoorFrame"] = float.Parse(ini.GetSetting("Damage", "WoodDoorFrame"));
                EntityList["WoodPillar"] = float.Parse(ini.GetSetting("Damage", "WoodPillar"));
                EntityList["WoodWall"] = float.Parse(ini.GetSetting("Damage", "WoodWall"));
                EntityList["WoodCeiling"] = float.Parse(ini.GetSetting("Damage", "WoodCeiling"));
                EntityList["WoodWindowFrame"] = float.Parse(ini.GetSetting("Damage", "WoodWindowFrame"));
                EntityList["WoodStairs"] = float.Parse(ini.GetSetting("Damage", "WoodStairs"));
                EntityList["WoodRamp"] = float.Parse(ini.GetSetting("Damage", "WoodRamp"));
                EntityList["WoodSpikeWall"] = float.Parse(ini.GetSetting("Damage", "WoodSpikeWall"));
                EntityList["LargeWoodSpikeWall"] = float.Parse(ini.GetSetting("Damage", "LargeWoodSpikeWall"));
                EntityList["WoodBox"] = float.Parse(ini.GetSetting("Damage", "WoodBox"));
                EntityList["WoodBoxLarge"] = float.Parse(ini.GetSetting("Damage", "WoodBoxLarge"));
                EntityList["WoodGate"] = float.Parse(ini.GetSetting("Damage", "WoodGate"));
                EntityList["WoodGateway"] = float.Parse(ini.GetSetting("Damage", "WoodGateway"));
                EntityList["WoodenDoor"] = float.Parse(ini.GetSetting("Damage", "WoodenDoor"));
                EntityList["Wood_Shelter"] = float.Parse(ini.GetSetting("Damage", "Wood_Shelter"));
                EntityList["MetalWall"] = float.Parse(ini.GetSetting("Damage", "MetalWall"));
                EntityList["MetalCeiling"] = float.Parse(ini.GetSetting("Damage", "MetalCeiling"));
                EntityList["MetalDoorFrame"] = float.Parse(ini.GetSetting("Damage", "MetalDoorFrame"));
                EntityList["MetalPillar"] = float.Parse(ini.GetSetting("Damage", "MetalPillar"));
                EntityList["MetalFoundation"] = float.Parse(ini.GetSetting("Damage", "MetalFoundation"));
                EntityList["MetalStairs"] = float.Parse(ini.GetSetting("Damage", "MetalStairs"));
                EntityList["MetalRamp"] = float.Parse(ini.GetSetting("Damage", "MetalRamp"));
                EntityList["MetalWindowFrame"] = float.Parse(ini.GetSetting("Damage", "MetalWindowFrame"));
                EntityList["MetalDoor"] = float.Parse(ini.GetSetting("Damage", "MetalDoor"));
                EntityList["MetalBarsWindow"] = float.Parse(ini.GetSetting("Damage", "MetalBarsWindow"));
                EntityList["SmallStash"] = float.Parse(ini.GetSetting("Damage", "SmallStash"));
                EntityList["Campfire"] = float.Parse(ini.GetSetting("Damage", "Campfire"));
                EntityList["Furnace"] = float.Parse(ini.GetSetting("Damage", "Furnace"));
                EntityList["Workbench"] = float.Parse(ini.GetSetting("Damage", "Workbench"));
                EntityList["Barricade_Fence_Deployable"] = float.Parse(ini.GetSetting("Damage", "Barricade_Fence_Deployable"));
                EntityList["RepairBench"] = float.Parse(ini.GetSetting("Damage", "RepairBench"));
                EntityList["SleepingBagA"] = float.Parse(ini.GetSetting("Damage", "SleepingBagA"));
                EntityList["SingleBed"] = float.Parse(ini.GetSetting("Damage", "SingleBed"));
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Wiper] Failed to read Health.ini: {ex}");
            }
        }

        public void ForceDecay()
        {
            List<Entity> data = World.GetWorld().Entities;
            foreach (Entity x in data)
            {
                if (EntityList.ContainsKey(x.Name))
                {
                    x.Health -= EntityList[x.Name];
                    if (x.Health <= 0)
                    {
                        x.Destroy();
                    }
                }
            }
        }
        

        public void LaunchCheck(Fougerite.Player player = null)
        {
            List<Entity> data = World.GetWorld().Entities;

            int objects = 0;
            int users = 0;

            List<ulong> Collected = new List<ulong>();
            var cache = PlayerCache.GetPlayerCache().CachedPlayers;
            foreach (var id in cache.Keys)
            {
                if (WList.Contains(id))
                {
                    continue;
                }

                var date = cache[id].LastLogout;
                if (date != null)
                {
                    if ((DateTime.Now - date).Value.Days > MaxDays)
                    {
                        Collected.Add(id);
                    }
                }
            }

            if (Collected.Count == 0)
            {
                return;
            }

            foreach (ulong x in Collected)
            {
                DirectoryInfo di = new DirectoryInfo($"{Util.GetRootFolder()}{UserDataPath}\\{x}");
                if (di.Exists)
                {
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    di.Delete();
                    users += 1;
                }
            }

            foreach (Entity x in data)
            {
                if (Collected.Contains(x.UOwnerID))
                {
                    x.Destroy();
                    objects += 1;
                }
            }

            if (Broadcast)
            {
                Server.GetServer().BroadcastFrom("Wiper",
                    $"Wiped {objects} amount of objects, and {users} amount of user data.");
            }
            else if (player != null)
            {
                player.MessageFrom("Wiper",
                    $"Wiped {objects} amount of objects, and {users} amount of user data.");
            }
        }

        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            switch (cmd)
            {
                case "wipehelp":
                {
                    if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "wiper.wipehelp"))
                    {
                        player.MessageFrom("Wiper", "Wiper Commands:");
                        player.MessageFrom("Wiper", "/wipecheck - Checks for inactive objects");
                        player.MessageFrom("Wiper", "/wipereload - Reloads config.");
                        player.MessageFrom("Wiper", "/wipeid playerid - Wipes All the objects of the ID");
                        player.MessageFrom("Wiper", "/wipebarr - Deletes all barricades");
                        player.MessageFrom("Wiper", "/wipecampf - Deletes all camp fires");
                        player.MessageFrom("Wiper", "/wipeforced - Force a decay");
                        player.MessageFrom("Wiper", "/wipewl id - Adds ID to wipe whitelist.");
                    }

                    break;
                }
                case "wipecheck":
                {
                    if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "wiper.wipecheck"))
                    {
                        if (Broadcast)
                        {
                            Server.GetServer().BroadcastFrom("Wiper", "Checking for Wipeable unused objects....");
                        }
                        LaunchCheck(player);
                    }

                    break;
                }
                case "wipereload":
                {
                    if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "wiper.wipereload"))
                    {
                        bool b = ReloadConfig();
                        player.MessageFrom("Wiper", b ? "Done." : "Failed to reload config!");
                    }

                    break;
                }
                case "wipeid":
                {
                    if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "wiper.wipeid"))
                    {
                        if (args.Length == 0)
                        {
                            player.MessageFrom("Wiper", "/wipeid playerid - Wipes All the objects of the ID");
                            return;
                        }
                        
                        string id = args[0];
                        ulong uid;
                        bool b = ulong.TryParse(id, out uid);
                        if (!b)
                        {
                            player.MessageFrom("Wiper", "/wipeid playerid - Wipes All the objects of the ID");
                            return;
                        }
                        
                        int c = 0;
                        foreach (Entity x in World.GetWorld().Entities)
                        {
                            if (x.UOwnerID == uid)
                            {
                                x.Destroy();
                                c++;
                            }
                        }
                        
                        DirectoryInfo di = new DirectoryInfo($"{Util.GetRootFolder()}{UserDataPath}\\{id}");
                        if (di.Exists)
                        {
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (DirectoryInfo dir in di.GetDirectories())
                            {
                                dir.Delete(true);
                            }
                            di.Delete();
                        }
                        player.MessageFrom("Wiper", $"Wiped: {c} objects.");
                    }

                    break;
                }
                case "wipebarr":
                {
                    if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "wiper.wipebarr"))
                    {
                        int c = 0;
                        foreach (Entity x in World.GetWorld().Entities)
                        {
                            if (x.Name.ToLower().Contains("barricade"))
                            {
                                x.Destroy();
                                c++;
                            }
                        }
                        player.MessageFrom("Wiper", $"Wiped: {c} objects.");
                    }

                    break;
                }
                case "wipecampf":
                {
                    if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "wiper.wipecampf"))
                    {
                        int c = 0;
                        foreach (Entity x in World.GetWorld().Entities)
                        {
                            if (x.Name.ToLower().Contains("camp"))
                            {
                                x.Destroy();
                                c++;
                            }
                        }
                        player.MessageFrom("Wiper", $"Wiped: {c} objects.");
                    }

                    break;
                }
                case "wipewl":
                {
                    if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "wiper.wipewl"))
                    {
                        if (args.Length == 0)
                        {
                            player.MessageFrom("Wiper", "/wipewl playerid - Adds playerid to the whitelist.");
                            return;
                        }
                        
                        string id = args[0];
                        ulong uid;
                        bool b = ulong.TryParse(id, out uid);
                        if (!b)
                        {
                            player.MessageFrom("Wiper", "/wipewl playerid - Adds playerid to the whitelist.");
                            return;
                        }

                        if (WList.Contains(uid))
                        {
                            player.MessageFrom("Wiper", "ID is already added!");
                            return;
                        }

                        WhiteList.AddSetting("WhiteList", id, "1");
                        WhiteList.Save();
                        player.MessageFrom("Wiper", "Added!");
                    }

                    break;
                }
                case "wipeall":
                {
                    if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "wiper.wipeall"))
                    {
                        List<string> list = new List<string>();
                        int c = 0;
                        foreach (Entity x in World.GetWorld().Entities)
                        {
                            if (!list.Contains(x.OwnerID))
                            {
                                list.Add(x.OwnerID);
                            }
                            x.Destroy();
                        }

                        foreach (string id in list)
                        {
                            DirectoryInfo di = new DirectoryInfo($"{Util.GetRootFolder()}{UserDataPath}\\{id}");
                            if (di.Exists)
                            {
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                foreach (DirectoryInfo dir in di.GetDirectories())
                                {
                                    dir.Delete(true);
                                }
                                di.Delete();
                            }
                        }
                        player.MessageFrom("Wiper", $"Wiped: {c} objects.");
                    }

                    break;
                }
                case "wipeforced":
                {
                    if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "wiper.forcedecay"))
                    {
                        ForceDecay();
                        player.MessageFrom("Wiper", "Forced decay cycle finished.");
                    }
                    break;
                }
            }
        }

        private bool ReloadConfig()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Settings.AddSetting("Settings", "UseDayLimit", "True");
                Settings.AddSetting("Settings", "MaxDays", "14");
                Settings.AddSetting("Settings", "UseDecay", "False");
                Settings.AddSetting("Settings", "DecayTimer", "30");
                Settings.AddSetting("Settings", "WipeCheckTimer", "30");
                Settings.AddSetting("Settings", "Broadcast", "True");
                Settings.AddSetting("Settings", "UserDataPath", "\\rust_server_Data\\userdata\\");
                Settings.Save();
            }
            if (!File.Exists(Path.Combine(ModuleFolder, "WhiteList.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "WhiteList.ini")).Dispose();
                WhiteList = new IniParser(Path.Combine(ModuleFolder, "WhiteList.ini"));
                WhiteList.AddSetting("WhiteList", "1234212312312", "1");
                WhiteList.Save();
            }

            Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
            try
            {
                MaxDays = int.Parse(Settings.GetSetting("Settings", "MaxDays"));
                DecayTimer = int.Parse(Settings.GetSetting("Settings", "DecayTimer"));
                WipeCheckTimer = int.Parse(Settings.GetSetting("Settings", "WipeCheckTimer"));
                UseDayLimit = Settings.GetBoolSetting("Settings", "UseDayLimit");
                UseDecay = Settings.GetBoolSetting("Settings", "UseDecay");
                Broadcast = Settings.GetBoolSetting("Settings", "Broadcast");
                UserDataPath = @Settings.GetSetting("Settings", "UserDataPath");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Wiper] Failed to read config, possible wrong value somewhere! Ex: {ex}");
                return false;
            }
            LoadDecayList();


            WhiteList = new IniParser(Path.Combine(ModuleFolder, "WhiteList.ini"));
            WList.Clear();
            string[] enumm = WhiteList.EnumSection("WhiteList");
            if (enumm.Length > 0)
            {
                foreach (string x in enumm)
                {
                    try
                    {
                        ulong id = ulong.Parse(x);
                        WList.Add(id);
                    }
                    catch
                    {
                        Logger.LogError($"[Wiper] Failed to parse whitelist for {x}");
                        return false;
                    }
                }
            }

            KillTimers();
            if (UseDecay)
            {
                CreateTimer("Decay", DecayTimer * 60 * 1000, DecayCallback, true);
            }
            if (UseDayLimit)
            {
                CreateTimer("WipeCheck", WipeCheckTimer * 60 * 1000, WipeCheckCallback, true);
            }

            return true;
        }
    }
}