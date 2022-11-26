using System;
using System.Collections.Generic;
using System.IO;
using Fougerite;
using Fougerite.Concurrent;
using UnityEngine;

namespace Wiper
{
    public class Wiper : Module
    {
        public IniParser Settings;
        public IniParser WhiteList;

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
        internal bool Check = false;
        private static Wiper _inst;
        internal GameObject GameO;

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
            get { return new Version("1.1.3"); }
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
            if (!File.Exists(Path.Combine(ModuleFolder, "Players.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Players.ini")).Dispose();
                Check = true;
            }
            else
            {
                IniParser players = new IniParser(Path.Combine(ModuleFolder, "Players.ini"));
                string[] enumm = players.EnumSection("Objects");
                if (enumm.Length > 0)
                {
                    foreach (string x in enumm)
                    {
                        try
                        {
                            ulong id = ulong.Parse(x);
                            string date = players.GetSetting("Objects", x);
                            // dd/MM/yyyy
                            string[] spl = date.Split('/');
                            CollectedIDs[id] = new DateTime(int.Parse(spl[2]), int.Parse(spl[1]), int.Parse(spl[0]));
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"[Wiper] Failed to parse datetime for {x} Error: {ex}");
                        }
                    }
                }
                else
                {
                    Check = true;
                }
            }
            ReloadConfig();

            LoadDecayList();
            Hooks.OnCommand += OnCommand;
            Hooks.OnPlayerConnected += OnPlayerConnected;
            Hooks.OnServerSaved += OnServerSaved;
            Hooks.OnServerLoaded += OnServerLoaded;
            GameO = new GameObject();
            GameO.AddComponent<WiperHandler>();
            UnityEngine.Object.DontDestroyOnLoad(GameO);
        }

        public override void DeInitialize()
        {
            if (GameO != null)
            {
                UnityEngine.Object.Destroy(GameO);
                GameO = null;
            }
            Check = false;
            Hooks.OnCommand -= OnCommand;
            Hooks.OnPlayerConnected -= OnPlayerConnected;
            Hooks.OnServerSaved -= OnServerSaved;
            Hooks.OnServerLoaded -= OnServerLoaded;
        }
        
        public void OnServerLoaded()
        {
            if (Check)
            {
                Logger.Log("[Wiper] Detecting Empty file... Cycling through all the objects and adding today's date to everyone.");
                IniParser players = new IniParser(Path.Combine(ModuleFolder, "Players.ini"));
                foreach (Entity x in World.GetWorld().Entities)
                {
                    if (!CollectedIDs.ContainsKey(x.UOwnerID))
                    {
                        CollectedIDs[x.UOwnerID] = DateTime.Today;
                        players.AddSetting("Objects", x.ToString(), CollectedIDs[x.UOwnerID].ToString("dd/MM/yyyy"));
                    }
                }
                players.Save();
                
                Logger.Log("[Wiper] Done.");
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
            int[] array = new int[2];
            array[0] = 0;
            array[1] = 0;

            List<ulong> Collected = new List<ulong>();
            foreach (ulong x in CollectedIDs.Keys)
            {
                if (WList.Contains(x))
                {
                    continue;
                }

                if ((DateTime.Today - CollectedIDs[x]).TotalDays > MaxDays)
                {
                    Collected.Add(x);
                }
            }

            if (Collected.Count == 0)
            {
                return;
            }

            foreach (ulong x in Collected)
            {
                if (CollectedIDs.ContainsKey(x)) // Just to be sure
                {
                    CollectedIDs.TryRemove(x);
                }

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
                    array[1] += 1;
                }
            }

            foreach (Entity x in data)
            {
                if (Collected.Contains(x.UOwnerID))
                {
                    x.Destroy();
                    array[0] += 1;
                }
            }

            if (Broadcast)
            {
                Server.GetServer().BroadcastFrom("Wiper",
                    $"Wiped {array[0]} amount of objects, and {array[1]} amount of user data.");
            }
            else if (player != null)
            {
                player.MessageFrom("Wiper",
                    $"Wiped {array[0]} amount of objects, and {array[1]} amount of user data.");
            }
        }

        public void OnServerSaved(int amount, double seconds)
        {
            Logger.LogDebug($"[Wiper] Saving Player Data. Count: {CollectedIDs.Keys.Count}");
            File.WriteAllText(Path.Combine(ModuleFolder, "Players.ini"), string.Empty);
            IniParser players = new IniParser(Path.Combine(ModuleFolder, "Players.ini"));
            foreach (ulong x in CollectedIDs.Keys)
            {
                players.AddSetting("Objects", x.ToString(), CollectedIDs[x].ToString("dd/MM/yyyy"));
            }
            players.Save();
            Logger.LogDebug("[Wiper] Save finished!");
        }

        public void OnPlayerConnected(Fougerite.Player player)
        {
            CollectedIDs[player.UID] = DateTime.Now;
        }

        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            switch (cmd)
            {
                case "wipehelp":
                {
                    if (player.Admin) // || player.Moderator ?
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
                    if (player.Admin)
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
                    if (player.Admin)
                    {
                        bool b = ReloadConfig();
                        player.MessageFrom("Wiper", b ? "Done." : "Failed to reload config!");
                    }

                    break;
                }
                case "wipeid":
                {
                    if (player.Admin)
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
                        
                        if (CollectedIDs.ContainsKey(uid))
                        {
                            CollectedIDs.TryRemove(uid);
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
                    if (player.Admin)
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
                    if (player.Admin)
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
                    if (player.Admin)
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
                    if (player.Admin)
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
            if (!File.Exists(Path.Combine(ModuleFolder, "Players.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Players.ini")).Dispose();
                CollectedIDs.Clear();
                foreach (Fougerite.Player x in Server.GetServer().Players)
                {
                    CollectedIDs[x.UID] = DateTime.Today;
                }
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
            return true;
        }
    }
}
