using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ItemManager;
using PieceManager;
using ServerSync;
using CreatureManager;
using LocationManager;
using UnityEngine;
using UnityEngine.Serialization;
using Microsoft.Win32;
using TMPro;
using BepInEx.Bootstrap;
using System.Collections;
using System.Threading;
using UnityEngine.UIElements;

namespace Timbercraft
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TimbercraftPlugin : BaseUnityPlugin
    {
        // DEUS SEJA LOUVADO!
        internal const string ModName = "Timbercraft";
        internal const string ModVersion = "0.0.1";
        internal const string Author = "marlthon";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Timbercraft =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        public static ConfigEntry<bool> SuppressUnicodeNotFoundWarning { get; private set; }

        private static readonly ConfigSync ConfigSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion, ModRequired = true };

        public ConfigEntry<float> maximumPlacementDistance;
        public void Awake()
        {
            // Needed for ServerSync to add locking of config toggle
            _serverConfigLocked = config("General", "Force Server Config", true, "Force Server Config");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            maximumPlacementDistance = config("1 - Additional", "Build distance alteration", 10f, "Build Distance  (Maximum Placement Distance)");

            Patch_ProgressiveConstruction.Init(this);
            BuildingZonePatch.Init(_harmony);

            #region TIMBERCRAFT HAMMER

            Item TimberHammer = new Item("mar_timbercraft", "TimberHammer");
            TimberHammer.Name.English("TimberCraft Hammer");
            TimberHammer.Name.Portuguese_Brazilian("Martelo do Construtor");
            TimberHammer.Description.English("TimberCraft Hammer.");
            TimberHammer.Description.Portuguese_Brazilian("Martelo do Construtor");
            TimberHammer.Crafting.Add(ItemManager.CraftingTable.Workbench, 1);
            TimberHammer.RequiredItems.Add("Wood", 1);
            TimberHammer.RequiredItems.Add("Resin", 1);
            TimberHammer.CraftAmount = 1;
            TimberHammer.Configurable = Configurability.Recipe;

            GameObject TimberHammerPieceTable = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "TimberHammerPieceTable");

            #endregion

            #region HOUSES

            BuildPiece MH_Outhouse = new("mar_timbercraft", "MH_Outhouse");
            MH_Outhouse.Category.Set("Buildings");
            MH_Outhouse.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Outhouse.Tool.Add("TimberHammer");
            MH_Outhouse.Name.English("Outhouse");
            MH_Outhouse.Name.Portuguese_Brazilian("Privada");
            MH_Outhouse.Description.English("Outhouse");
            MH_Outhouse.Description.Portuguese_Brazilian("Privada");
            MH_Outhouse.RequiredItems.Add("Wood", 1, true);
            MH_Outhouse.RequiredItems.Add("Resin", 1, true);
            MH_Outhouse.RequiredItems.Add("Coal", 1, true);
            MH_Outhouse.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Outhouse.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_A2_C_Hut = new("mar_timbercraft", "MH_A2_C_Hut");
            MH_A2_C_Hut.Category.Set("Buildings");
            MH_A2_C_Hut.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_A2_C_Hut.Tool.Add("TimberHammer");
            MH_A2_C_Hut.Name.English("MH_A2_C_Hut");
            MH_A2_C_Hut.Name.Portuguese_Brazilian("MH_A2_C_Hut");
            MH_A2_C_Hut.Description.English("MH_A2_C_Hut");
            MH_A2_C_Hut.Description.Portuguese_Brazilian("MH_A2_C_Hut");
            MH_A2_C_Hut.RequiredItems.Add("Wood", 1, true);
            MH_A2_C_Hut.RequiredItems.Add("Resin", 1, true);
            MH_A2_C_Hut.RequiredItems.Add("Coal", 1, true);
            MH_A2_C_Hut.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_A2_C_Hut.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_A3_Hut = new("mar_timbercraft", "MH_A3_Hut");
            MH_A3_Hut.Category.Set("Buildings");
            MH_A3_Hut.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_A3_Hut.Tool.Add("TimberHammer");
            MH_A3_Hut.Name.English("MH_A3_Hut");
            MH_A3_Hut.Name.Portuguese_Brazilian("MH_A3_Hut");
            MH_A3_Hut.Description.English("MH_A3_Hut");
            MH_A3_Hut.Description.Portuguese_Brazilian("MH_A3_Hut");
            MH_A3_Hut.RequiredItems.Add("Wood", 1, true);
            MH_A3_Hut.RequiredItems.Add("Resin", 1, true);
            MH_A3_Hut.RequiredItems.Add("Coal", 1, true);
            MH_A3_Hut.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_A3_Hut.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_A4_A_House = new("mar_timbercraft", "MH_A4_A_House");
            MH_A4_A_House.Category.Set("Buildings");
            MH_A4_A_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_A4_A_House.Tool.Add("TimberHammer");
            MH_A4_A_House.Name.English("MH_A4_A_House");
            MH_A4_A_House.Name.Portuguese_Brazilian("MH_A4_A_House");
            MH_A4_A_House.Description.English("MH_A4_A_House");
            MH_A4_A_House.Description.Portuguese_Brazilian("MH_A4_A_House");
            MH_A4_A_House.RequiredItems.Add("Wood", 1, true);
            MH_A4_A_House.RequiredItems.Add("Resin", 1, true);
            MH_A4_A_House.RequiredItems.Add("Coal", 1, true);
            MH_A4_A_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_A4_A_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_A5_A_House = new("mar_timbercraft", "MH_A5_A_House");
            MH_A5_A_House.Category.Set("Buildings");
            MH_A5_A_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_A5_A_House.Tool.Add("TimberHammer");
            MH_A5_A_House.Name.English("MH_A5_A_House");
            MH_A5_A_House.Name.Portuguese_Brazilian("MH_A5_A_House");
            MH_A5_A_House.Description.English("MH_A5_A_House");
            MH_A5_A_House.Description.Portuguese_Brazilian("MH_A5_A_House");
            MH_A5_A_House.RequiredItems.Add("Wood", 1, true);
            MH_A5_A_House.RequiredItems.Add("Resin", 1, true);
            MH_A5_A_House.RequiredItems.Add("Coal", 1, true);
            MH_A5_A_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_A5_A_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_A6_A_House = new("mar_timbercraft", "MH_A6_A_House");
            MH_A6_A_House.Category.Set("Buildings");
            MH_A6_A_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_A6_A_House.Tool.Add("TimberHammer");
            MH_A6_A_House.Name.English("MH_A6_A_House");
            MH_A6_A_House.Name.Portuguese_Brazilian("MH_A6_A_House");
            MH_A6_A_House.Description.English("MH_A6_A_House");
            MH_A6_A_House.Description.Portuguese_Brazilian("MH_A6_A_House");
            MH_A6_A_House.RequiredItems.Add("Wood", 1, true);
            MH_A6_A_House.RequiredItems.Add("Resin", 1, true);
            MH_A6_A_House.RequiredItems.Add("Coal", 1, true);
            MH_A6_A_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_A6_A_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_A7_A_House = new("mar_timbercraft", "MH_A7_A_House");
            MH_A7_A_House.Category.Set("Buildings");
            MH_A7_A_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_A7_A_House.Tool.Add("TimberHammer");
            MH_A7_A_House.Name.English("MH_A7_A_House");
            MH_A7_A_House.Name.Portuguese_Brazilian("MH_A7_A_House");
            MH_A7_A_House.Description.English("MH_A7_A_House");
            MH_A7_A_House.Description.Portuguese_Brazilian("MH_A7_A_House");
            MH_A7_A_House.RequiredItems.Add("Wood", 1, true);
            MH_A7_A_House.RequiredItems.Add("Resin", 1, true);
            MH_A7_A_House.RequiredItems.Add("Coal", 1, true);
            MH_A7_A_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_A7_A_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_A8_A_House = new("mar_timbercraft", "MH_A8_A_House");
            MH_A8_A_House.Category.Set("Buildings");
            MH_A8_A_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_A8_A_House.Tool.Add("TimberHammer");
            MH_A8_A_House.Name.English("MH_A8_A_House");
            MH_A8_A_House.Name.Portuguese_Brazilian("MH_A8_A_House");
            MH_A8_A_House.Description.English("MH_A8_A_House");
            MH_A8_A_House.Description.Portuguese_Brazilian("MH_A8_A_House");
            MH_A8_A_House.RequiredItems.Add("Wood", 1, true);
            MH_A8_A_House.RequiredItems.Add("Resin", 1, true);
            MH_A8_A_House.RequiredItems.Add("Coal", 1, true);
            MH_A8_A_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_A8_A_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_B1_A_Shop = new("mar_timbercraft", "MH_B1_A_Shop");
            MH_B1_A_Shop.Category.Set("Buildings");
            MH_B1_A_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_B1_A_Shop.Tool.Add("TimberHammer");
            MH_B1_A_Shop.Name.English("MH_B1_A_Shop");
            MH_B1_A_Shop.Name.Portuguese_Brazilian("MH_B1_A_Shop");
            MH_B1_A_Shop.Description.English("MH_B1_A_Shop");
            MH_B1_A_Shop.Description.Portuguese_Brazilian("MH_B1_A_Shop");
            MH_B1_A_Shop.RequiredItems.Add("Wood", 1, true);
            MH_B1_A_Shop.RequiredItems.Add("Resin", 1, true);
            MH_B1_A_Shop.RequiredItems.Add("Coal", 1, true);
            MH_B1_A_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_B1_A_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_B1_House = new("mar_timbercraft", "MH_B1_House");
            MH_B1_House.Category.Set("Buildings");
            MH_B1_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_B1_House.Tool.Add("TimberHammer");
            MH_B1_House.Name.English("MH_B1_House");
            MH_B1_House.Name.Portuguese_Brazilian("MH_B1_House");
            MH_B1_House.Description.English("MH_B1_House");
            MH_B1_House.Description.Portuguese_Brazilian("MH_B1_House");
            MH_B1_House.RequiredItems.Add("Wood", 1, true);
            MH_B1_House.RequiredItems.Add("Resin", 1, true);
            MH_B1_House.RequiredItems.Add("Coal", 1, true);
            MH_B1_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_B1_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_B2_A_Shop = new("mar_timbercraft", "MH_B2_A_Shop");
            MH_B2_A_Shop.Category.Set("Buildings");
            MH_B2_A_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_B2_A_Shop.Tool.Add("TimberHammer");
            MH_B2_A_Shop.Name.English("MH_B2_A_Shop");
            MH_B2_A_Shop.Name.Portuguese_Brazilian("MH_B2_A_Shop");
            MH_B2_A_Shop.Description.English("MH_B2_A_Shop");
            MH_B2_A_Shop.Description.Portuguese_Brazilian("MH_B2_A_Shop");
            MH_B2_A_Shop.RequiredItems.Add("Wood", 1, true);
            MH_B2_A_Shop.RequiredItems.Add("Resin", 1, true);
            MH_B2_A_Shop.RequiredItems.Add("Coal", 1, true);
            MH_B2_A_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_B2_A_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_B2_House = new("mar_timbercraft", "MH_B2_House");
            MH_B2_House.Category.Set("Buildings");
            MH_B2_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_B2_House.Tool.Add("TimberHammer");
            MH_B2_House.Name.English("MH_B2_House");
            MH_B2_House.Name.Portuguese_Brazilian("MH_B2_House");
            MH_B2_House.Description.English("MH_B2_House");
            MH_B2_House.Description.Portuguese_Brazilian("MH_B2_House");
            MH_B2_House.RequiredItems.Add("Wood", 1, true);
            MH_B2_House.RequiredItems.Add("Resin", 1, true);
            MH_B2_House.RequiredItems.Add("Coal", 1, true);
            MH_B2_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_B2_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_B3_A_Shop = new("mar_timbercraft", "MH_B3_A_Shop");
            MH_B3_A_Shop.Category.Set("Buildings");
            MH_B3_A_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_B3_A_Shop.Tool.Add("TimberHammer");
            MH_B3_A_Shop.Name.English("MH_B3_A_Shop");
            MH_B3_A_Shop.Name.Portuguese_Brazilian("MH_B3_A_Shop");
            MH_B3_A_Shop.Description.English("MH_B3_A_Shop");
            MH_B3_A_Shop.Description.Portuguese_Brazilian("MH_B3_A_Shop");
            MH_B3_A_Shop.RequiredItems.Add("Wood", 1, true);
            MH_B3_A_Shop.RequiredItems.Add("Resin", 1, true);
            MH_B3_A_Shop.RequiredItems.Add("Coal", 1, true);
            MH_B3_A_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_B3_A_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_B3_House = new("mar_timbercraft", "MH_B3_House");
            MH_B3_House.Category.Set("Buildings");
            MH_B3_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_B3_House.Tool.Add("TimberHammer");
            MH_B3_House.Name.English("MH_B3_House");
            MH_B3_House.Name.Portuguese_Brazilian("MH_B3_House");
            MH_B3_House.Description.English("MH_B3_House");
            MH_B3_House.Description.Portuguese_Brazilian("MH_B3_House");
            MH_B3_House.RequiredItems.Add("Wood", 1, true);
            MH_B3_House.RequiredItems.Add("Resin", 1, true);
            MH_B3_House.RequiredItems.Add("Coal", 1, true);
            MH_B3_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_B3_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_B4_A_Shop = new("mar_timbercraft", "MH_B4_A_Shop");
            MH_B4_A_Shop.Category.Set("Buildings");
            MH_B4_A_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_B4_A_Shop.Tool.Add("TimberHammer");
            MH_B4_A_Shop.Name.English("MH_B4_A_Shop");
            MH_B4_A_Shop.Name.Portuguese_Brazilian("MH_B4_A_Shop");
            MH_B4_A_Shop.Description.English("MH_B4_A_Shop");
            MH_B4_A_Shop.Description.Portuguese_Brazilian("MH_B4_A_Shop");
            MH_B4_A_Shop.RequiredItems.Add("Wood", 1, true);
            MH_B4_A_Shop.RequiredItems.Add("Resin", 1, true);
            MH_B4_A_Shop.RequiredItems.Add("Coal", 1, true);
            MH_B4_A_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_B4_A_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_B4_House = new("mar_timbercraft", "MH_B4_House");
            MH_B4_House.Category.Set("Buildings");
            MH_B4_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_B4_House.Tool.Add("TimberHammer");
            MH_B4_House.Name.English("MH_B4_House");
            MH_B4_House.Name.Portuguese_Brazilian("MH_B4_House");
            MH_B4_House.Description.English("MH_B4_House");
            MH_B4_House.Description.Portuguese_Brazilian("MH_B4_House");
            MH_B4_House.RequiredItems.Add("Wood", 1, true);
            MH_B4_House.RequiredItems.Add("Resin", 1, true);
            MH_B4_House.RequiredItems.Add("Coal", 1, true);
            MH_B4_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_B4_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_B5_House = new("mar_timbercraft", "MH_B5_House");
            MH_B5_House.Category.Set("Buildings");
            MH_B5_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_B5_House.Tool.Add("TimberHammer");
            MH_B5_House.Name.English("MH_B5_House");
            MH_B5_House.Name.Portuguese_Brazilian("MH_B5_House");
            MH_B5_House.Description.English("MH_B5_House");
            MH_B5_House.Description.Portuguese_Brazilian("MH_B5_House");
            MH_B5_House.RequiredItems.Add("Wood", 1, true);
            MH_B5_House.RequiredItems.Add("Resin", 1, true);
            MH_B5_House.RequiredItems.Add("Coal", 1, true);
            MH_B5_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_B5_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_C1_A1_Apartment = new("mar_timbercraft", "MH_C1_A1_Apartment");
            MH_C1_A1_Apartment.Category.Set("Buildings");
            MH_C1_A1_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_C1_A1_Apartment.Tool.Add("TimberHammer");
            MH_C1_A1_Apartment.Name.English("MH_C1_A1_Apartment");
            MH_C1_A1_Apartment.Name.Portuguese_Brazilian("MH_C1_A1_Apartment");
            MH_C1_A1_Apartment.Description.English("MH_C1_A1_Apartment");
            MH_C1_A1_Apartment.Description.Portuguese_Brazilian("MH_C1_A1_Apartment");
            MH_C1_A1_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_C1_A1_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_C1_A1_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_C1_A1_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_C1_A1_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_C1_B1_Apartment = new("mar_timbercraft", "MH_C1_B1_Apartment");
            MH_C1_B1_Apartment.Category.Set("Buildings");
            MH_C1_B1_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_C1_B1_Apartment.Tool.Add("TimberHammer");
            MH_C1_B1_Apartment.Name.English("MH_C1_B1_Apartment");
            MH_C1_B1_Apartment.Name.Portuguese_Brazilian("MH_C1_B1_Apartment");
            MH_C1_B1_Apartment.Description.English("MH_C1_B1_Apartment");
            MH_C1_B1_Apartment.Description.Portuguese_Brazilian("MH_C1_B1_Apartment");
            MH_C1_B1_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_C1_B1_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_C1_B1_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_C1_B1_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_C1_B1_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_C1_Shop = new("mar_timbercraft", "MH_C1_Shop");
            MH_C1_Shop.Category.Set("Buildings");
            MH_C1_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_C1_Shop.Tool.Add("TimberHammer");
            MH_C1_Shop.Name.English("MH_C1_Shop");
            MH_C1_Shop.Name.Portuguese_Brazilian("MH_C1_Shop");
            MH_C1_Shop.Description.English("MH_C1_Shop");
            MH_C1_Shop.Description.Portuguese_Brazilian("MH_C1_Shop");
            MH_C1_Shop.RequiredItems.Add("Wood", 1, true);
            MH_C1_Shop.RequiredItems.Add("Resin", 1, true);
            MH_C1_Shop.RequiredItems.Add("Coal", 1, true);
            MH_C1_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_C1_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_C2_A1_Apartment = new("mar_timbercraft", "MH_C2_A1_Apartment");
            MH_C2_A1_Apartment.Category.Set("Buildings");
            MH_C2_A1_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_C2_A1_Apartment.Tool.Add("TimberHammer");
            MH_C2_A1_Apartment.Name.English("MH_C2_A1_Apartment");
            MH_C2_A1_Apartment.Name.Portuguese_Brazilian("MH_C2_A1_Apartment");
            MH_C2_A1_Apartment.Description.English("MH_C2_A1_Apartment");
            MH_C2_A1_Apartment.Description.Portuguese_Brazilian("MH_C2_A1_Apartment");
            MH_C2_A1_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_C2_A1_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_C2_A1_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_C2_A1_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_C2_A1_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_C2_B1_Apartment = new("mar_timbercraft", "MH_C2_B1_Apartment");
            MH_C2_B1_Apartment.Category.Set("Buildings");
            MH_C2_B1_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_C2_B1_Apartment.Tool.Add("TimberHammer");
            MH_C2_B1_Apartment.Name.English("MH_C2_B1_Apartment");
            MH_C2_B1_Apartment.Name.Portuguese_Brazilian("MH_C2_B1_Apartment");
            MH_C2_B1_Apartment.Description.English("MH_C2_B1_Apartment");
            MH_C2_B1_Apartment.Description.Portuguese_Brazilian("MH_C2_B1_Apartment");
            MH_C2_B1_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_C2_B1_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_C2_B1_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_C2_B1_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_C2_B1_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_C2_Shop = new("mar_timbercraft", "MH_C2_Shop");
            MH_C2_Shop.Category.Set("Buildings");
            MH_C2_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_C2_Shop.Tool.Add("TimberHammer");
            MH_C2_Shop.Name.English("MH_C2_Shop");
            MH_C2_Shop.Name.Portuguese_Brazilian("MH_C2_Shop");
            MH_C2_Shop.Description.English("MH_C2_Shop");
            MH_C2_Shop.Description.Portuguese_Brazilian("MH_C2_Shop");
            MH_C2_Shop.RequiredItems.Add("Wood", 1, true);
            MH_C2_Shop.RequiredItems.Add("Resin", 1, true);
            MH_C2_Shop.RequiredItems.Add("Coal", 1, true);
            MH_C2_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_C2_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_C3_Shop = new("mar_timbercraft", "MH_C3_Shop");
            MH_C3_Shop.Category.Set("Buildings");
            MH_C3_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_C3_Shop.Tool.Add("TimberHammer");
            MH_C3_Shop.Name.English("MH_C3_Shop");
            MH_C3_Shop.Name.Portuguese_Brazilian("MH_C3_Shop");
            MH_C3_Shop.Description.English("MH_C3_Shop");
            MH_C3_Shop.Description.Portuguese_Brazilian("MH_C3_Shop");
            MH_C3_Shop.RequiredItems.Add("Wood", 1, true);
            MH_C3_Shop.RequiredItems.Add("Resin", 1, true);
            MH_C3_Shop.RequiredItems.Add("Coal", 1, true);
            MH_C3_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_C3_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_C4_Shop = new("mar_timbercraft", "MH_C4_Shop");
            MH_C4_Shop.Category.Set("Buildings");
            MH_C4_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_C4_Shop.Tool.Add("TimberHammer");
            MH_C4_Shop.Name.English("MH_C4_Shop");
            MH_C4_Shop.Name.Portuguese_Brazilian("MH_C4_Shop");
            MH_C4_Shop.Description.English("MH_C4_Shop");
            MH_C4_Shop.Description.Portuguese_Brazilian("MH_C4_Shop");
            MH_C4_Shop.RequiredItems.Add("Wood", 1, true);
            MH_C4_Shop.RequiredItems.Add("Resin", 1, true);
            MH_C4_Shop.RequiredItems.Add("Coal", 1, true);
            MH_C4_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_C4_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D1_A_2Story_Shop = new("mar_timbercraft", "MH_D1_A_2Story_Shop");
            MH_D1_A_2Story_Shop.Category.Set("Buildings");
            MH_D1_A_2Story_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D1_A_2Story_Shop.Tool.Add("TimberHammer");
            MH_D1_A_2Story_Shop.Name.English("MH_D1_A_2Story_Shop");
            MH_D1_A_2Story_Shop.Name.Portuguese_Brazilian("MH_D1_A_2Story_Shop");
            MH_D1_A_2Story_Shop.Description.English("MH_D1_A_2Story_Shop");
            MH_D1_A_2Story_Shop.Description.Portuguese_Brazilian("MH_D1_A_2Story_Shop");
            MH_D1_A_2Story_Shop.RequiredItems.Add("Wood", 1, true);
            MH_D1_A_2Story_Shop.RequiredItems.Add("Resin", 1, true);
            MH_D1_A_2Story_Shop.RequiredItems.Add("Coal", 1, true);
            MH_D1_A_2Story_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D1_A_2Story_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D2_A_2Story_Shop = new("mar_timbercraft", "MH_D2_A_2Story_Shop");
            MH_D2_A_2Story_Shop.Category.Set("Buildings");
            MH_D2_A_2Story_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D2_A_2Story_Shop.Tool.Add("TimberHammer");
            MH_D2_A_2Story_Shop.Name.English("MH_D2_A_2Story_Shop");
            MH_D2_A_2Story_Shop.Name.Portuguese_Brazilian("MH_D2_A_2Story_Shop");
            MH_D2_A_2Story_Shop.Description.English("MH_D2_A_2Story_Shop");
            MH_D2_A_2Story_Shop.Description.Portuguese_Brazilian("MH_D2_A_2Story_Shop");
            MH_D2_A_2Story_Shop.RequiredItems.Add("Wood", 1, true);
            MH_D2_A_2Story_Shop.RequiredItems.Add("Resin", 1, true);
            MH_D2_A_2Story_Shop.RequiredItems.Add("Coal", 1, true);
            MH_D2_A_2Story_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D2_A_2Story_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D2_Apartment = new("mar_timbercraft", "MH_D2_Apartment");
            MH_D2_Apartment.Category.Set("Buildings");
            MH_D2_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D2_Apartment.Tool.Add("TimberHammer");
            MH_D2_Apartment.Name.English("MH_D2_Apartment");
            MH_D2_Apartment.Name.Portuguese_Brazilian("MH_D2_Apartment");
            MH_D2_Apartment.Description.English("MH_D2_Apartment");
            MH_D2_Apartment.Description.Portuguese_Brazilian("MH_D2_Apartment");
            MH_D2_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_D2_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_D2_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_D2_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D2_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D3_A_2Story_House = new("mar_timbercraft", "MH_D3_A_2Story_House");
            MH_D3_A_2Story_House.Category.Set("Buildings");
            MH_D3_A_2Story_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D3_A_2Story_House.Tool.Add("TimberHammer");
            MH_D3_A_2Story_House.Name.English("MH_D3_A_2Story_House");
            MH_D3_A_2Story_House.Name.Portuguese_Brazilian("MH_D3_A_2Story_House");
            MH_D3_A_2Story_House.Description.English("MH_D3_A_2Story_House");
            MH_D3_A_2Story_House.Description.Portuguese_Brazilian("MH_D3_A_2Story_House");
            MH_D3_A_2Story_House.RequiredItems.Add("Wood", 1, true);
            MH_D3_A_2Story_House.RequiredItems.Add("Resin", 1, true);
            MH_D3_A_2Story_House.RequiredItems.Add("Coal", 1, true);
            MH_D3_A_2Story_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D3_A_2Story_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D3_Apartment = new("mar_timbercraft", "MH_D3_Apartment");
            MH_D3_Apartment.Category.Set("Buildings");
            MH_D3_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D3_Apartment.Tool.Add("TimberHammer");
            MH_D3_Apartment.Name.English("MH_D3_Apartment");
            MH_D3_Apartment.Name.Portuguese_Brazilian("MH_D3_Apartment");
            MH_D3_Apartment.Description.English("MH_D3_Apartment");
            MH_D3_Apartment.Description.Portuguese_Brazilian("MH_D3_Apartment");
            MH_D3_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_D3_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_D3_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_D3_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D3_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D4_A_2Story_Shop = new("mar_timbercraft", "MH_D4_A_2Story_Shop");
            MH_D4_A_2Story_Shop.Category.Set("Buildings");
            MH_D4_A_2Story_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D4_A_2Story_Shop.Tool.Add("TimberHammer");
            MH_D4_A_2Story_Shop.Name.English("MH_D4_A_2Story_Shop");
            MH_D4_A_2Story_Shop.Name.Portuguese_Brazilian("MH_D4_A_2Story_Shop");
            MH_D4_A_2Story_Shop.Description.English("MH_D4_A_2Story_Shop");
            MH_D4_A_2Story_Shop.Description.Portuguese_Brazilian("MH_D4_A_2Story_Shop");
            MH_D4_A_2Story_Shop.RequiredItems.Add("Wood", 1, true);
            MH_D4_A_2Story_Shop.RequiredItems.Add("Resin", 1, true);
            MH_D4_A_2Story_Shop.RequiredItems.Add("Coal", 1, true);
            MH_D4_A_2Story_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D4_A_2Story_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D4_Apartment = new("mar_timbercraft", "MH_D4_Apartment");
            MH_D4_Apartment.Category.Set("Buildings");
            MH_D4_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D4_Apartment.Tool.Add("TimberHammer");
            MH_D4_Apartment.Name.English("MH_D4_Apartment");
            MH_D4_Apartment.Name.Portuguese_Brazilian("MH_D4_Apartment");
            MH_D4_Apartment.Description.English("MH_D4_Apartment");
            MH_D4_Apartment.Description.Portuguese_Brazilian("MH_D4_Apartment");
            MH_D4_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_D4_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_D4_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_D4_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D4_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D6_Apartment = new("mar_timbercraft", "MH_D6_Apartment");
            MH_D6_Apartment.Category.Set("Buildings");
            MH_D6_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D6_Apartment.Tool.Add("TimberHammer");
            MH_D6_Apartment.Name.English("MH_D6_Apartment");
            MH_D6_Apartment.Name.Portuguese_Brazilian("MH_D6_Apartment");
            MH_D6_Apartment.Description.English("MH_D6_Apartment");
            MH_D6_Apartment.Description.Portuguese_Brazilian("MH_D6_Apartment");
            MH_D6_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_D6_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_D6_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_D6_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D6_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D7_Apartment = new("mar_timbercraft", "MH_D7_Apartment");
            MH_D7_Apartment.Category.Set("Buildings");
            MH_D7_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D7_Apartment.Tool.Add("TimberHammer");
            MH_D7_Apartment.Name.English("MH_D7_Apartment");
            MH_D7_Apartment.Name.Portuguese_Brazilian("MH_D7_Apartment");
            MH_D7_Apartment.Description.English("MH_D7_Apartment");
            MH_D7_Apartment.Description.Portuguese_Brazilian("MH_D7_Apartment");
            MH_D7_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_D7_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_D7_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_D7_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D7_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_D8_Apartment = new("mar_timbercraft", "MH_D8_Apartment");
            MH_D8_Apartment.Category.Set("Buildings");
            MH_D8_Apartment.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_D8_Apartment.Tool.Add("TimberHammer");
            MH_D8_Apartment.Name.English("MH_D8_Apartment");
            MH_D8_Apartment.Name.Portuguese_Brazilian("MH_D8_Apartment");
            MH_D8_Apartment.Description.English("MH_D8_Apartment");
            MH_D8_Apartment.Description.Portuguese_Brazilian("MH_D8_Apartment");
            MH_D8_Apartment.RequiredItems.Add("Wood", 1, true);
            MH_D8_Apartment.RequiredItems.Add("Resin", 1, true);
            MH_D8_Apartment.RequiredItems.Add("Coal", 1, true);
            MH_D8_Apartment.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_D8_Apartment.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_E1_A_3Story_House = new("mar_timbercraft", "MH_E1_A_3Story_House");
            MH_E1_A_3Story_House.Category.Set("Buildings");
            MH_E1_A_3Story_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_E1_A_3Story_House.Tool.Add("TimberHammer");
            MH_E1_A_3Story_House.Name.English("MH_E1_A_3Story_House");
            MH_E1_A_3Story_House.Name.Portuguese_Brazilian("MH_E1_A_3Story_House");
            MH_E1_A_3Story_House.Description.English("MH_E1_A_3Story_House");
            MH_E1_A_3Story_House.Description.Portuguese_Brazilian("MH_E1_A_3Story_House");
            MH_E1_A_3Story_House.RequiredItems.Add("Wood", 1, true);
            MH_E1_A_3Story_House.RequiredItems.Add("Resin", 1, true);
            MH_E1_A_3Story_House.RequiredItems.Add("Coal", 1, true);
            MH_E1_A_3Story_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_E1_A_3Story_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_E2_A_3Story_House = new("mar_timbercraft", "MH_E2_A_3Story_House");
            MH_E2_A_3Story_House.Category.Set("Buildings");
            MH_E2_A_3Story_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_E2_A_3Story_House.Tool.Add("TimberHammer");
            MH_E2_A_3Story_House.Name.English("MH_E2_A_3Story_House");
            MH_E2_A_3Story_House.Name.Portuguese_Brazilian("MH_E2_A_3Story_House");
            MH_E2_A_3Story_House.Description.English("MH_E2_A_3Story_House");
            MH_E2_A_3Story_House.Description.Portuguese_Brazilian("MH_E2_A_3Story_House");
            MH_E2_A_3Story_House.RequiredItems.Add("Wood", 1, true);
            MH_E2_A_3Story_House.RequiredItems.Add("Resin", 1, true);
            MH_E2_A_3Story_House.RequiredItems.Add("Coal", 1, true);
            MH_E2_A_3Story_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_E2_A_3Story_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_E3_A_3Story_House = new("mar_timbercraft", "MH_E3_A_3Story_House");
            MH_E3_A_3Story_House.Category.Set("Buildings");
            MH_E3_A_3Story_House.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_E3_A_3Story_House.Tool.Add("TimberHammer");
            MH_E3_A_3Story_House.Name.English("MH_E3_A_3Story_House");
            MH_E3_A_3Story_House.Name.Portuguese_Brazilian("MH_E3_A_3Story_House");
            MH_E3_A_3Story_House.Description.English("MH_E3_A_3Story_House");
            MH_E3_A_3Story_House.Description.Portuguese_Brazilian("MH_E3_A_3Story_House");
            MH_E3_A_3Story_House.RequiredItems.Add("Wood", 1, true);
            MH_E3_A_3Story_House.RequiredItems.Add("Resin", 1, true);
            MH_E3_A_3Story_House.RequiredItems.Add("Coal", 1, true);
            MH_E3_A_3Story_House.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_E3_A_3Story_House.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_E6_A_3Story_Shop = new("mar_timbercraft", "MH_E6_A_3Story_Shop");
            MH_E6_A_3Story_Shop.Category.Set("Buildings");
            MH_E6_A_3Story_Shop.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_E6_A_3Story_Shop.Tool.Add("TimberHammer");
            MH_E6_A_3Story_Shop.Name.English("MH_E6_A_3Story_Shop");
            MH_E6_A_3Story_Shop.Name.Portuguese_Brazilian("MH_E6_A_3Story_Shop");
            MH_E6_A_3Story_Shop.Description.English("MH_E6_A_3Story_Shop");
            MH_E6_A_3Story_Shop.Description.Portuguese_Brazilian("MH_E6_A_3Story_Shop");
            MH_E6_A_3Story_Shop.RequiredItems.Add("Wood", 1, true);
            MH_E6_A_3Story_Shop.RequiredItems.Add("Resin", 1, true);
            MH_E6_A_3Story_Shop.RequiredItems.Add("Coal", 1, true);
            MH_E6_A_3Story_Shop.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_E6_A_3Story_Shop.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_F1_A_4Story_Inn = new("mar_timbercraft", "MH_F1_A_4Story_Inn");
            MH_F1_A_4Story_Inn.Category.Set("Buildings");
            MH_F1_A_4Story_Inn.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_F1_A_4Story_Inn.Tool.Add("TimberHammer");
            MH_F1_A_4Story_Inn.Name.English("MH_F1_A_4Story_Inn");
            MH_F1_A_4Story_Inn.Name.Portuguese_Brazilian("MH_F1_A_4Story_Inn");
            MH_F1_A_4Story_Inn.Description.English("MH_F1_A_4Story_Inn");
            MH_F1_A_4Story_Inn.Description.Portuguese_Brazilian("MH_F1_A_4Story_Inn");
            MH_F1_A_4Story_Inn.RequiredItems.Add("Wood", 1, true);
            MH_F1_A_4Story_Inn.RequiredItems.Add("Resin", 1, true);
            MH_F1_A_4Story_Inn.RequiredItems.Add("Coal", 1, true);
            MH_F1_A_4Story_Inn.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_F1_A_4Story_Inn.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_G2_A_Gazebo = new("mar_timbercraft", "MH_G2_A_Gazebo");
            MH_G2_A_Gazebo.Category.Set("Buildings");
            MH_G2_A_Gazebo.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_G2_A_Gazebo.Tool.Add("TimberHammer");
            MH_G2_A_Gazebo.Name.English("MH_G2_A_Gazebo");
            MH_G2_A_Gazebo.Name.Portuguese_Brazilian("MH_G2_A_Gazebo");
            MH_G2_A_Gazebo.Description.English("MH_G2_A_Gazebo");
            MH_G2_A_Gazebo.Description.Portuguese_Brazilian("MH_G2_A_Gazebo");
            MH_G2_A_Gazebo.RequiredItems.Add("Wood", 1, true);
            MH_G2_A_Gazebo.RequiredItems.Add("Resin", 1, true);
            MH_G2_A_Gazebo.RequiredItems.Add("Coal", 1, true);
            MH_G2_A_Gazebo.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_G2_A_Gazebo.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_G5_A_WaterWell = new("mar_timbercraft", "MH_G5_A_WaterWell");
            MH_G5_A_WaterWell.Category.Set("Buildings");
            MH_G5_A_WaterWell.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_G5_A_WaterWell.Tool.Add("TimberHammer");
            MH_G5_A_WaterWell.Name.English("MH_G5_A_WaterWell");
            MH_G5_A_WaterWell.Name.Portuguese_Brazilian("MH_G5_A_WaterWell");
            MH_G5_A_WaterWell.Description.English("MH_G5_A_WaterWell");
            MH_G5_A_WaterWell.Description.Portuguese_Brazilian("MH_G5_A_WaterWell");
            MH_G5_A_WaterWell.RequiredItems.Add("Wood", 1, true);
            MH_G5_A_WaterWell.RequiredItems.Add("Resin", 1, true);
            MH_G5_A_WaterWell.RequiredItems.Add("Coal", 1, true);
            MH_G5_A_WaterWell.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_G5_A_WaterWell.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Stage_mdl = new("mar_timbercraft", "MH_Stage_mdl");
            MH_Stage_mdl.Category.Set("Buildings");
            MH_Stage_mdl.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Stage_mdl.Tool.Add("TimberHammer");
            MH_Stage_mdl.Name.English("MH_Stage_mdl");
            MH_Stage_mdl.Name.Portuguese_Brazilian("MH_Stage_mdl");
            MH_Stage_mdl.Description.English("MH_Stage_mdl");
            MH_Stage_mdl.Description.Portuguese_Brazilian("MH_Stage_mdl");
            MH_Stage_mdl.RequiredItems.Add("Wood", 1, true);
            MH_Stage_mdl.RequiredItems.Add("Resin", 1, true);
            MH_Stage_mdl.RequiredItems.Add("Coal", 1, true);
            MH_Stage_mdl.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Stage_mdl.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece ME_TimbercraftDoor = new("mar_timbercraft", "ME_TimbercraftDoor");
            ME_TimbercraftDoor.Category.Set("Buildings");
            ME_TimbercraftDoor.Crafting.Set(PieceManager.CraftingTable.Workbench);
            ME_TimbercraftDoor.Tool.Add("TimberHammer");
            ME_TimbercraftDoor.Name.English("Timbercraft Door");
            ME_TimbercraftDoor.Name.Portuguese_Brazilian("Porta Timbercraft");
            ME_TimbercraftDoor.Description.English("This door fits perfectly into Timbercraft constructions.");
            ME_TimbercraftDoor.Description.Portuguese_Brazilian("Esta porta encaixa perfitamente nas contruções do Timbercraft.");
            ME_TimbercraftDoor.RequiredItems.Add("Wood", 1, true);
            ME_TimbercraftDoor.RequiredItems.Add("Resin", 1, true);
            ME_TimbercraftDoor.RequiredItems.Add("Coal", 1, true);
            ME_TimbercraftDoor.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(ME_TimbercraftDoor.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece F3_B_4Story_Inn = new("mar_timbercraft", "F3_B_4Story_Inn");
            F3_B_4Story_Inn.Category.Set("Buildings");
            F3_B_4Story_Inn.Crafting.Set(PieceManager.CraftingTable.Workbench);
            F3_B_4Story_Inn.Tool.Add("TimberHammer");
            F3_B_4Story_Inn.Name.English("F3_B_4Story_Inn");
            F3_B_4Story_Inn.Name.Portuguese_Brazilian("F3_B_4Story_Inn");
            F3_B_4Story_Inn.Description.English("F3_B_4Story_Inn");
            F3_B_4Story_Inn.Description.Portuguese_Brazilian("F3_B_4Story_Inn");
            F3_B_4Story_Inn.RequiredItems.Add("Wood", 1, true);
            F3_B_4Story_Inn.RequiredItems.Add("Resin", 1, true);
            F3_B_4Story_Inn.RequiredItems.Add("Coal", 1, true);
            F3_B_4Story_Inn.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(F3_B_4Story_Inn.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            #endregion

            #region UTILITIES

            BuildPiece MH_MedievalChest_01 = new("mar_timbercraft", "MH_MedievalChest_01");
            MH_MedievalChest_01.Category.Set("Utilities");
            MH_MedievalChest_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_MedievalChest_01.Tool.Add("TimberHammer");
            MH_MedievalChest_01.Name.English("Medieval Chest");
            MH_MedievalChest_01.Name.Portuguese_Brazilian("Baú Medieval");
            MH_MedievalChest_01.Description.English("A medieval wooden chest for storing items.");
            MH_MedievalChest_01.Description.Portuguese_Brazilian("Um baú de madeira medieval para armazenar itens.");
            MH_MedievalChest_01.RequiredItems.Add("Wood", 1, true);
            MH_MedievalChest_01.RequiredItems.Add("Resin", 1, true);
            MH_MedievalChest_01.RequiredItems.Add("Coal", 1, true);
            MH_MedievalChest_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_MedievalChest_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_MedievalChest_02 = new("mar_timbercraft", "MH_MedievalChest_02");
            MH_MedievalChest_02.Category.Set("Utilities");
            MH_MedievalChest_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_MedievalChest_02.Tool.Add("TimberHammer");
            MH_MedievalChest_02.Name.English("Medieval Chest II");
            MH_MedievalChest_02.Name.Portuguese_Brazilian("Baú Medieval II");
            MH_MedievalChest_02.Description.English("A larger medieval wooden chest for storing items.");
            MH_MedievalChest_02.Description.Portuguese_Brazilian("Um baú de madeira medieval maior para armazenar itens.");
            MH_MedievalChest_02.RequiredItems.Add("Wood", 1, true);
            MH_MedievalChest_02.RequiredItems.Add("Resin", 1, true);
            MH_MedievalChest_02.RequiredItems.Add("Coal", 1, true);
            MH_MedievalChest_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_MedievalChest_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_MedievalPrivateChest = new("mar_timbercraft", "MH_MedievalPrivateChest");
            MH_MedievalPrivateChest.Category.Set("Utilities");
            MH_MedievalPrivateChest.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_MedievalPrivateChest.Tool.Add("TimberHammer");
            MH_MedievalPrivateChest.Name.English("Medieval Private Chest");
            MH_MedievalPrivateChest.Name.Portuguese_Brazilian("Baú Medieval Privado");
            MH_MedievalPrivateChest.Description.English("A locked medieval chest for private storage.");
            MH_MedievalPrivateChest.Description.Portuguese_Brazilian("Um baú medieval trancado para armazenamento privado.");
            MH_MedievalPrivateChest.RequiredItems.Add("Wood", 1, true);
            MH_MedievalPrivateChest.RequiredItems.Add("Resin", 1, true);
            MH_MedievalPrivateChest.RequiredItems.Add("Coal", 1, true);
            MH_MedievalPrivateChest.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_MedievalPrivateChest.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            #endregion

            #region FURNITURE

            BuildPiece MH_Bar_01 = new("mar_timbercraft", "MH_Bar_01");
            MH_Bar_01.Category.Set("Furniture");
            MH_Bar_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Bar_01.Tool.Add("TimberHammer");
            MH_Bar_01.Name.English("Bar Counter I");
            MH_Bar_01.Name.Portuguese_Brazilian("Balcão de Bar I");
            MH_Bar_01.Description.English("A medieval wooden bar counter.");
            MH_Bar_01.Description.Portuguese_Brazilian("Um balcão de bar medieval de madeira.");
            MH_Bar_01.RequiredItems.Add("Wood", 1, true);
            MH_Bar_01.RequiredItems.Add("Resin", 1, true);
            MH_Bar_01.RequiredItems.Add("Coal", 1, true);
            MH_Bar_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Bar_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Bar_02 = new("mar_timbercraft", "MH_Bar_02");
            MH_Bar_02.Category.Set("Furniture");
            MH_Bar_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Bar_02.Tool.Add("TimberHammer");
            MH_Bar_02.Name.English("Bar Counter II");
            MH_Bar_02.Name.Portuguese_Brazilian("Balcão de Bar II");
            MH_Bar_02.Description.English("A medieval wooden bar counter.");
            MH_Bar_02.Description.Portuguese_Brazilian("Um balcão de bar medieval de madeira.");
            MH_Bar_02.RequiredItems.Add("Wood", 1, true);
            MH_Bar_02.RequiredItems.Add("Resin", 1, true);
            MH_Bar_02.RequiredItems.Add("Coal", 1, true);
            MH_Bar_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Bar_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Bar_03 = new("mar_timbercraft", "MH_Bar_03");
            MH_Bar_03.Category.Set("Furniture");
            MH_Bar_03.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Bar_03.Tool.Add("TimberHammer");
            MH_Bar_03.Name.English("Bar Counter III");
            MH_Bar_03.Name.Portuguese_Brazilian("Balcão de Bar III");
            MH_Bar_03.Description.English("A medieval wooden bar counter.");
            MH_Bar_03.Description.Portuguese_Brazilian("Um balcão de bar medieval de madeira.");
            MH_Bar_03.RequiredItems.Add("Wood", 1, true);
            MH_Bar_03.RequiredItems.Add("Resin", 1, true);
            MH_Bar_03.RequiredItems.Add("Coal", 1, true);
            MH_Bar_03.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Bar_03.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_BookShelf_01 = new("mar_timbercraft", "MH_BookShelf_01");
            MH_BookShelf_01.Category.Set("Furniture");
            MH_BookShelf_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_BookShelf_01.Tool.Add("TimberHammer");
            MH_BookShelf_01.Name.English("Bookshelf");
            MH_BookShelf_01.Name.Portuguese_Brazilian("Estante de Livros");
            MH_BookShelf_01.Description.English("A wooden medieval bookshelf filled with books.");
            MH_BookShelf_01.Description.Portuguese_Brazilian("Uma estante de madeira medieval repleta de livros.");
            MH_BookShelf_01.RequiredItems.Add("Wood", 1, true);
            MH_BookShelf_01.RequiredItems.Add("Resin", 1, true);
            MH_BookShelf_01.RequiredItems.Add("Coal", 1, true);
            MH_BookShelf_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_BookShelf_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Chair_01 = new("mar_timbercraft", "MH_Chair_01");
            MH_Chair_01.Category.Set("Furniture");
            MH_Chair_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Chair_01.Tool.Add("TimberHammer");
            MH_Chair_01.Name.English("Medieval Chair");
            MH_Chair_01.Name.Portuguese_Brazilian("Cadeira Medieval");
            MH_Chair_01.Description.English("A simple medieval wooden chair.");
            MH_Chair_01.Description.Portuguese_Brazilian("Uma simples cadeira de madeira medieval.");
            MH_Chair_01.RequiredItems.Add("Wood", 1, true);
            MH_Chair_01.RequiredItems.Add("Resin", 1, true);
            MH_Chair_01.RequiredItems.Add("Coal", 1, true);
            MH_Chair_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Chair_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_CupBoard_01 = new("mar_timbercraft", "MH_CupBoard_01");
            MH_CupBoard_01.Category.Set("Furniture");
            MH_CupBoard_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_CupBoard_01.Tool.Add("TimberHammer");
            MH_CupBoard_01.Name.English("Cupboard I");
            MH_CupBoard_01.Name.Portuguese_Brazilian("Armário I");
            MH_CupBoard_01.Description.English("A medieval wooden cupboard.");
            MH_CupBoard_01.Description.Portuguese_Brazilian("Um armário de madeira medieval.");
            MH_CupBoard_01.RequiredItems.Add("Wood", 1, true);
            MH_CupBoard_01.RequiredItems.Add("Resin", 1, true);
            MH_CupBoard_01.RequiredItems.Add("Coal", 1, true);
            MH_CupBoard_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_CupBoard_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_CupBoard_02 = new("mar_timbercraft", "MH_CupBoard_02");
            MH_CupBoard_02.Category.Set("Furniture");
            MH_CupBoard_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_CupBoard_02.Tool.Add("TimberHammer");
            MH_CupBoard_02.Name.English("Cupboard II");
            MH_CupBoard_02.Name.Portuguese_Brazilian("Armário II");
            MH_CupBoard_02.Description.English("A medieval wooden cupboard.");
            MH_CupBoard_02.Description.Portuguese_Brazilian("Um armário de madeira medieval.");
            MH_CupBoard_02.RequiredItems.Add("Wood", 1, true);
            MH_CupBoard_02.RequiredItems.Add("Resin", 1, true);
            MH_CupBoard_02.RequiredItems.Add("Coal", 1, true);
            MH_CupBoard_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_CupBoard_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_CupBoard_03 = new("mar_timbercraft", "MH_CupBoard_03");
            MH_CupBoard_03.Category.Set("Furniture");
            MH_CupBoard_03.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_CupBoard_03.Tool.Add("TimberHammer");
            MH_CupBoard_03.Name.English("Cupboard III");
            MH_CupBoard_03.Name.Portuguese_Brazilian("Armário III");
            MH_CupBoard_03.Description.English("A medieval wooden cupboard.");
            MH_CupBoard_03.Description.Portuguese_Brazilian("Um armário de madeira medieval.");
            MH_CupBoard_03.RequiredItems.Add("Wood", 1, true);
            MH_CupBoard_03.RequiredItems.Add("Resin", 1, true);
            MH_CupBoard_03.RequiredItems.Add("Coal", 1, true);
            MH_CupBoard_03.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_CupBoard_03.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Table_01 = new("mar_timbercraft", "MH_DECO_Table_01");
            MH_DECO_Table_01.Category.Set("Furniture");
            MH_DECO_Table_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Table_01.Tool.Add("TimberHammer");
            MH_DECO_Table_01.Name.English("Decorated Table I");
            MH_DECO_Table_01.Name.Portuguese_Brazilian("Mesa Decorada I");
            MH_DECO_Table_01.Description.English("A decorated medieval table with items on top.");
            MH_DECO_Table_01.Description.Portuguese_Brazilian("Uma mesa medieval decorada com itens sobre ela.");
            MH_DECO_Table_01.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Table_01.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Table_01.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Table_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Table_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Table_02 = new("mar_timbercraft", "MH_DECO_Table_02");
            MH_DECO_Table_02.Category.Set("Furniture");
            MH_DECO_Table_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Table_02.Tool.Add("TimberHammer");
            MH_DECO_Table_02.Name.English("Decorated Table II");
            MH_DECO_Table_02.Name.Portuguese_Brazilian("Mesa Decorada II");
            MH_DECO_Table_02.Description.English("A decorated medieval table with items on top.");
            MH_DECO_Table_02.Description.Portuguese_Brazilian("Uma mesa medieval decorada com itens sobre ela.");
            MH_DECO_Table_02.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Table_02.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Table_02.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Table_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Table_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Table_03 = new("mar_timbercraft", "MH_DECO_Table_03");
            MH_DECO_Table_03.Category.Set("Furniture");
            MH_DECO_Table_03.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Table_03.Tool.Add("TimberHammer");
            MH_DECO_Table_03.Name.English("Decorated Table III");
            MH_DECO_Table_03.Name.Portuguese_Brazilian("Mesa Decorada III");
            MH_DECO_Table_03.Description.English("A decorated medieval table with items on top.");
            MH_DECO_Table_03.Description.Portuguese_Brazilian("Uma mesa medieval decorada com itens sobre ela.");
            MH_DECO_Table_03.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Table_03.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Table_03.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Table_03.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Table_03.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Table_04 = new("mar_timbercraft", "MH_DECO_Table_04");
            MH_DECO_Table_04.Category.Set("Furniture");
            MH_DECO_Table_04.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Table_04.Tool.Add("TimberHammer");
            MH_DECO_Table_04.Name.English("Decorated Table IV");
            MH_DECO_Table_04.Name.Portuguese_Brazilian("Mesa Decorada IV");
            MH_DECO_Table_04.Description.English("A decorated medieval table with items on top.");
            MH_DECO_Table_04.Description.Portuguese_Brazilian("Uma mesa medieval decorada com itens sobre ela.");
            MH_DECO_Table_04.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Table_04.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Table_04.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Table_04.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Table_04.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Table_05 = new("mar_timbercraft", "MH_DECO_Table_05");
            MH_DECO_Table_05.Category.Set("Furniture");
            MH_DECO_Table_05.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Table_05.Tool.Add("TimberHammer");
            MH_DECO_Table_05.Name.English("Decorated Table V");
            MH_DECO_Table_05.Name.Portuguese_Brazilian("Mesa Decorada V");
            MH_DECO_Table_05.Description.English("A decorated medieval table with items on top.");
            MH_DECO_Table_05.Description.Portuguese_Brazilian("Uma mesa medieval decorada com itens sobre ela.");
            MH_DECO_Table_05.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Table_05.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Table_05.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Table_05.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Table_05.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Table_06 = new("mar_timbercraft", "MH_DECO_Table_06");
            MH_DECO_Table_06.Category.Set("Furniture");
            MH_DECO_Table_06.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Table_06.Tool.Add("TimberHammer");
            MH_DECO_Table_06.Name.English("Decorated Table VI");
            MH_DECO_Table_06.Name.Portuguese_Brazilian("Mesa Decorada VI");
            MH_DECO_Table_06.Description.English("A decorated medieval table with items on top.");
            MH_DECO_Table_06.Description.Portuguese_Brazilian("Uma mesa medieval decorada com itens sobre ela.");
            MH_DECO_Table_06.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Table_06.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Table_06.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Table_06.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Table_06.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Table_07 = new("mar_timbercraft", "MH_DECO_Table_07");
            MH_DECO_Table_07.Category.Set("Furniture");
            MH_DECO_Table_07.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Table_07.Tool.Add("TimberHammer");
            MH_DECO_Table_07.Name.English("Decorated Table VII");
            MH_DECO_Table_07.Name.Portuguese_Brazilian("Mesa Decorada VII");
            MH_DECO_Table_07.Description.English("A decorated medieval table with items on top.");
            MH_DECO_Table_07.Description.Portuguese_Brazilian("Uma mesa medieval decorada com itens sobre ela.");
            MH_DECO_Table_07.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Table_07.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Table_07.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Table_07.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Table_07.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Wall_Shelf_01 = new("mar_timbercraft", "MH_DECO_Wall_Shelf_01");
            MH_DECO_Wall_Shelf_01.Category.Set("Furniture");
            MH_DECO_Wall_Shelf_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Wall_Shelf_01.Tool.Add("TimberHammer");
            MH_DECO_Wall_Shelf_01.Name.English("Decorated Wall Shelf I");
            MH_DECO_Wall_Shelf_01.Name.Portuguese_Brazilian("Prateleira Decorada I");
            MH_DECO_Wall_Shelf_01.Description.English("A decorative wall shelf with medieval items.");
            MH_DECO_Wall_Shelf_01.Description.Portuguese_Brazilian("Uma prateleira de parede decorativa com itens medievais.");
            MH_DECO_Wall_Shelf_01.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Wall_Shelf_01.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Wall_Shelf_01.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Wall_Shelf_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Wall_Shelf_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Wall_Shelf_02 = new("mar_timbercraft", "MH_DECO_Wall_Shelf_02");
            MH_DECO_Wall_Shelf_02.Category.Set("Furniture");
            MH_DECO_Wall_Shelf_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Wall_Shelf_02.Tool.Add("TimberHammer");
            MH_DECO_Wall_Shelf_02.Name.English("Decorated Wall Shelf II");
            MH_DECO_Wall_Shelf_02.Name.Portuguese_Brazilian("Prateleira Decorada II");
            MH_DECO_Wall_Shelf_02.Description.English("A decorative wall shelf with medieval items.");
            MH_DECO_Wall_Shelf_02.Description.Portuguese_Brazilian("Uma prateleira de parede decorativa com itens medievais.");
            MH_DECO_Wall_Shelf_02.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Wall_Shelf_02.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Wall_Shelf_02.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Wall_Shelf_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Wall_Shelf_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Wall_Shelf_03 = new("mar_timbercraft", "MH_DECO_Wall_Shelf_03");
            MH_DECO_Wall_Shelf_03.Category.Set("Furniture");
            MH_DECO_Wall_Shelf_03.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Wall_Shelf_03.Tool.Add("TimberHammer");
            MH_DECO_Wall_Shelf_03.Name.English("Decorated Wall Shelf III");
            MH_DECO_Wall_Shelf_03.Name.Portuguese_Brazilian("Prateleira Decorada III");
            MH_DECO_Wall_Shelf_03.Description.English("A decorative wall shelf with medieval items.");
            MH_DECO_Wall_Shelf_03.Description.Portuguese_Brazilian("Uma prateleira de parede decorativa com itens medievais.");
            MH_DECO_Wall_Shelf_03.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Wall_Shelf_03.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Wall_Shelf_03.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Wall_Shelf_03.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Wall_Shelf_03.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DECO_Wall_Shelf_04 = new("mar_timbercraft", "MH_DECO_Wall_Shelf_04");
            MH_DECO_Wall_Shelf_04.Category.Set("Furniture");
            MH_DECO_Wall_Shelf_04.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DECO_Wall_Shelf_04.Tool.Add("TimberHammer");
            MH_DECO_Wall_Shelf_04.Name.English("Decorated Wall Shelf IV");
            MH_DECO_Wall_Shelf_04.Name.Portuguese_Brazilian("Prateleira Decorada IV");
            MH_DECO_Wall_Shelf_04.Description.English("A decorative wall shelf with medieval items.");
            MH_DECO_Wall_Shelf_04.Description.Portuguese_Brazilian("Uma prateleira de parede decorativa com itens medievais.");
            MH_DECO_Wall_Shelf_04.RequiredItems.Add("Wood", 1, true);
            MH_DECO_Wall_Shelf_04.RequiredItems.Add("Resin", 1, true);
            MH_DECO_Wall_Shelf_04.RequiredItems.Add("Coal", 1, true);
            MH_DECO_Wall_Shelf_04.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DECO_Wall_Shelf_04.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_DoubleBed_01 = new("mar_timbercraft", "MH_DoubleBed_01");
            MH_DoubleBed_01.Category.Set("Furniture");
            MH_DoubleBed_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_DoubleBed_01.Tool.Add("TimberHammer");
            MH_DoubleBed_01.Name.English("Double Bed");
            MH_DoubleBed_01.Name.Portuguese_Brazilian("Cama de Casal");
            MH_DoubleBed_01.Description.English("A medieval double bed.");
            MH_DoubleBed_01.Description.Portuguese_Brazilian("Uma cama de casal medieval.");
            MH_DoubleBed_01.RequiredItems.Add("Wood", 1, true);
            MH_DoubleBed_01.RequiredItems.Add("Resin", 1, true);
            MH_DoubleBed_01.RequiredItems.Add("Coal", 1, true);
            MH_DoubleBed_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_DoubleBed_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_MeadRack_01 = new("mar_timbercraft", "MH_MeadRack_01");
            MH_MeadRack_01.Category.Set("Furniture");
            MH_MeadRack_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_MeadRack_01.Tool.Add("TimberHammer");
            MH_MeadRack_01.Name.English("Mead Rack");
            MH_MeadRack_01.Name.Portuguese_Brazilian("Suporte de Hidromel");
            MH_MeadRack_01.Description.English("A wooden rack for storing mead barrels and bottles.");
            MH_MeadRack_01.Description.Portuguese_Brazilian("Um suporte de madeira para armazenar barris e garrafas de hidromel.");
            MH_MeadRack_01.RequiredItems.Add("Wood", 1, true);
            MH_MeadRack_01.RequiredItems.Add("Resin", 1, true);
            MH_MeadRack_01.RequiredItems.Add("Coal", 1, true);
            MH_MeadRack_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_MeadRack_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_SingleBed_01 = new("mar_timbercraft", "MH_SingleBed_01");
            MH_SingleBed_01.Category.Set("Furniture");
            MH_SingleBed_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_SingleBed_01.Tool.Add("TimberHammer");
            MH_SingleBed_01.Name.English("Single Bed");
            MH_SingleBed_01.Name.Portuguese_Brazilian("Cama de Solteiro");
            MH_SingleBed_01.Description.English("A medieval single bed.");
            MH_SingleBed_01.Description.Portuguese_Brazilian("Uma cama de solteiro medieval.");
            MH_SingleBed_01.RequiredItems.Add("Wood", 1, true);
            MH_SingleBed_01.RequiredItems.Add("Resin", 1, true);
            MH_SingleBed_01.RequiredItems.Add("Coal", 1, true);
            MH_SingleBed_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_SingleBed_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Stool_01 = new("mar_timbercraft", "MH_Stool_01");
            MH_Stool_01.Category.Set("Furniture");
            MH_Stool_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Stool_01.Tool.Add("TimberHammer");
            MH_Stool_01.Name.English("Stool");
            MH_Stool_01.Name.Portuguese_Brazilian("Banquinho");
            MH_Stool_01.Description.English("A simple medieval wooden stool.");
            MH_Stool_01.Description.Portuguese_Brazilian("Um simples banquinho de madeira medieval.");
            MH_Stool_01.RequiredItems.Add("Wood", 1, true);
            MH_Stool_01.RequiredItems.Add("Resin", 1, true);
            MH_Stool_01.RequiredItems.Add("Coal", 1, true);
            MH_Stool_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Stool_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Table_01 = new("mar_timbercraft", "MH_Table_01");
            MH_Table_01.Category.Set("Furniture");
            MH_Table_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Table_01.Tool.Add("TimberHammer");
            MH_Table_01.Name.English("Medieval Table I");
            MH_Table_01.Name.Portuguese_Brazilian("Mesa Medieval I");
            MH_Table_01.Description.English("A simple medieval wooden table.");
            MH_Table_01.Description.Portuguese_Brazilian("Uma simples mesa de madeira medieval.");
            MH_Table_01.RequiredItems.Add("Wood", 1, true);
            MH_Table_01.RequiredItems.Add("Resin", 1, true);
            MH_Table_01.RequiredItems.Add("Coal", 1, true);
            MH_Table_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Table_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Table_02 = new("mar_timbercraft", "MH_Table_02");
            MH_Table_02.Category.Set("Furniture");
            MH_Table_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Table_02.Tool.Add("TimberHammer");
            MH_Table_02.Name.English("Medieval Table II");
            MH_Table_02.Name.Portuguese_Brazilian("Mesa Medieval II");
            MH_Table_02.Description.English("A simple medieval wooden table.");
            MH_Table_02.Description.Portuguese_Brazilian("Uma simples mesa de madeira medieval.");
            MH_Table_02.RequiredItems.Add("Wood", 1, true);
            MH_Table_02.RequiredItems.Add("Resin", 1, true);
            MH_Table_02.RequiredItems.Add("Coal", 1, true);
            MH_Table_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Table_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Table_03 = new("mar_timbercraft", "MH_Table_03");
            MH_Table_03.Category.Set("Furniture");
            MH_Table_03.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Table_03.Tool.Add("TimberHammer");
            MH_Table_03.Name.English("Medieval Table III");
            MH_Table_03.Name.Portuguese_Brazilian("Mesa Medieval III");
            MH_Table_03.Description.English("A simple medieval wooden table.");
            MH_Table_03.Description.Portuguese_Brazilian("Uma simples mesa de madeira medieval.");
            MH_Table_03.RequiredItems.Add("Wood", 1, true);
            MH_Table_03.RequiredItems.Add("Resin", 1, true);
            MH_Table_03.RequiredItems.Add("Coal", 1, true);
            MH_Table_03.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Table_03.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Table_04 = new("mar_timbercraft", "MH_Table_04");
            MH_Table_04.Category.Set("Furniture");
            MH_Table_04.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Table_04.Tool.Add("TimberHammer");
            MH_Table_04.Name.English("Medieval Table IV");
            MH_Table_04.Name.Portuguese_Brazilian("Mesa Medieval IV");
            MH_Table_04.Description.English("A simple medieval wooden table.");
            MH_Table_04.Description.Portuguese_Brazilian("Uma simples mesa de madeira medieval.");
            MH_Table_04.RequiredItems.Add("Wood", 1, true);
            MH_Table_04.RequiredItems.Add("Resin", 1, true);
            MH_Table_04.RequiredItems.Add("Coal", 1, true);
            MH_Table_04.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Table_04.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Table_05 = new("mar_timbercraft", "MH_Table_05");
            MH_Table_05.Category.Set("Furniture");
            MH_Table_05.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Table_05.Tool.Add("TimberHammer");
            MH_Table_05.Name.English("Medieval Table V");
            MH_Table_05.Name.Portuguese_Brazilian("Mesa Medieval V");
            MH_Table_05.Description.English("A simple medieval wooden table.");
            MH_Table_05.Description.Portuguese_Brazilian("Uma simples mesa de madeira medieval.");
            MH_Table_05.RequiredItems.Add("Wood", 1, true);
            MH_Table_05.RequiredItems.Add("Resin", 1, true);
            MH_Table_05.RequiredItems.Add("Coal", 1, true);
            MH_Table_05.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Table_05.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Table_06 = new("mar_timbercraft", "MH_Table_06");
            MH_Table_06.Category.Set("Furniture");
            MH_Table_06.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Table_06.Tool.Add("TimberHammer");
            MH_Table_06.Name.English("Medieval Table VI");
            MH_Table_06.Name.Portuguese_Brazilian("Mesa Medieval VI");
            MH_Table_06.Description.English("A simple medieval wooden table.");
            MH_Table_06.Description.Portuguese_Brazilian("Uma simples mesa de madeira medieval.");
            MH_Table_06.RequiredItems.Add("Wood", 1, true);
            MH_Table_06.RequiredItems.Add("Resin", 1, true);
            MH_Table_06.RequiredItems.Add("Coal", 1, true);
            MH_Table_06.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Table_06.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Wall_Shelf_01 = new("mar_timbercraft", "MH_Wall_Shelf_01");
            MH_Wall_Shelf_01.Category.Set("Furniture");
            MH_Wall_Shelf_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Wall_Shelf_01.Tool.Add("TimberHammer");
            MH_Wall_Shelf_01.Name.English("Wall Shelf");
            MH_Wall_Shelf_01.Name.Portuguese_Brazilian("Prateleira de Parede");
            MH_Wall_Shelf_01.Description.English("A simple medieval wooden wall shelf.");
            MH_Wall_Shelf_01.Description.Portuguese_Brazilian("Uma simples prateleira de parede de madeira medieval.");
            MH_Wall_Shelf_01.RequiredItems.Add("Wood", 1, true);
            MH_Wall_Shelf_01.RequiredItems.Add("Resin", 1, true);
            MH_Wall_Shelf_01.RequiredItems.Add("Coal", 1, true);
            MH_Wall_Shelf_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Wall_Shelf_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Wardrobe_01 = new("mar_timbercraft", "MH_Wardrobe_01");
            MH_Wardrobe_01.Category.Set("Furniture");
            MH_Wardrobe_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Wardrobe_01.Tool.Add("TimberHammer");
            MH_Wardrobe_01.Name.English("Wardrobe I");
            MH_Wardrobe_01.Name.Portuguese_Brazilian("Guarda-Roupa I");
            MH_Wardrobe_01.Description.English("A medieval wooden wardrobe.");
            MH_Wardrobe_01.Description.Portuguese_Brazilian("Um guarda-roupa de madeira medieval.");
            MH_Wardrobe_01.RequiredItems.Add("Wood", 1, true);
            MH_Wardrobe_01.RequiredItems.Add("Resin", 1, true);
            MH_Wardrobe_01.RequiredItems.Add("Coal", 1, true);
            MH_Wardrobe_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Wardrobe_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Wardrobe_02 = new("mar_timbercraft", "MH_Wardrobe_02");
            MH_Wardrobe_02.Category.Set("Furniture");
            MH_Wardrobe_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Wardrobe_02.Tool.Add("TimberHammer");
            MH_Wardrobe_02.Name.English("Wardrobe II");
            MH_Wardrobe_02.Name.Portuguese_Brazilian("Guarda-Roupa II");
            MH_Wardrobe_02.Description.English("A medieval wooden wardrobe.");
            MH_Wardrobe_02.Description.Portuguese_Brazilian("Um guarda-roupa de madeira medieval.");
            MH_Wardrobe_02.RequiredItems.Add("Wood", 1, true);
            MH_Wardrobe_02.RequiredItems.Add("Resin", 1, true);
            MH_Wardrobe_02.RequiredItems.Add("Coal", 1, true);
            MH_Wardrobe_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Wardrobe_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Wardrobe_03 = new("mar_timbercraft", "MH_Wardrobe_03");
            MH_Wardrobe_03.Category.Set("Furniture");
            MH_Wardrobe_03.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Wardrobe_03.Tool.Add("TimberHammer");
            MH_Wardrobe_03.Name.English("Wardrobe III");
            MH_Wardrobe_03.Name.Portuguese_Brazilian("Guarda-Roupa III");
            MH_Wardrobe_03.Description.English("A medieval wooden wardrobe.");
            MH_Wardrobe_03.Description.Portuguese_Brazilian("Um guarda-roupa de madeira medieval.");
            MH_Wardrobe_03.RequiredItems.Add("Wood", 1, true);
            MH_Wardrobe_03.RequiredItems.Add("Resin", 1, true);
            MH_Wardrobe_03.RequiredItems.Add("Coal", 1, true);
            MH_Wardrobe_03.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Wardrobe_03.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_WineRack_01 = new("mar_timbercraft", "MH_WineRack_01");
            MH_WineRack_01.Category.Set("Furniture");
            MH_WineRack_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_WineRack_01.Tool.Add("TimberHammer");
            MH_WineRack_01.Name.English("Wine Rack");
            MH_WineRack_01.Name.Portuguese_Brazilian("Adega de Vinho");
            MH_WineRack_01.Description.English("A wooden rack for storing wine bottles.");
            MH_WineRack_01.Description.Portuguese_Brazilian("Um suporte de madeira para armazenar garrafas de vinho.");
            MH_WineRack_01.RequiredItems.Add("Wood", 1, true);
            MH_WineRack_01.RequiredItems.Add("Resin", 1, true);
            MH_WineRack_01.RequiredItems.Add("Coal", 1, true);
            MH_WineRack_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_WineRack_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            #endregion

            #region LIGHTING

            BuildPiece MH_Candle = new("mar_timbercraft", "MH_Candle");
            MH_Candle.Category.Set("Lighting");
            MH_Candle.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Candle.Tool.Add("TimberHammer");
            MH_Candle.Name.English("Candle");
            MH_Candle.Name.Portuguese_Brazilian("Vela");
            MH_Candle.Description.English("A simple medieval candle.");
            MH_Candle.Description.Portuguese_Brazilian("Uma simples vela medieval.");
            MH_Candle.RequiredItems.Add("Wood", 1, true);
            MH_Candle.RequiredItems.Add("Resin", 1, true);
            MH_Candle.RequiredItems.Add("Coal", 1, true);
            MH_Candle.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Candle.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_CandleHolder_01 = new("mar_timbercraft", "MH_CandleHolder_01");
            MH_CandleHolder_01.Category.Set("Lighting");
            MH_CandleHolder_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_CandleHolder_01.Tool.Add("TimberHammer");
            MH_CandleHolder_01.Name.English("Candle Holder I");
            MH_CandleHolder_01.Name.Portuguese_Brazilian("Castiçal I");
            MH_CandleHolder_01.Description.English("A medieval candle holder.");
            MH_CandleHolder_01.Description.Portuguese_Brazilian("Um castiçal medieval.");
            MH_CandleHolder_01.RequiredItems.Add("Wood", 1, true);
            MH_CandleHolder_01.RequiredItems.Add("Resin", 1, true);
            MH_CandleHolder_01.RequiredItems.Add("Coal", 1, true);
            MH_CandleHolder_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_CandleHolder_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_CandleHolder_02 = new("mar_timbercraft", "MH_CandleHolder_02");
            MH_CandleHolder_02.Category.Set("Lighting");
            MH_CandleHolder_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_CandleHolder_02.Tool.Add("TimberHammer");
            MH_CandleHolder_02.Name.English("Candle Holder II");
            MH_CandleHolder_02.Name.Portuguese_Brazilian("Castiçal II");
            MH_CandleHolder_02.Description.English("A medieval candle holder.");
            MH_CandleHolder_02.Description.Portuguese_Brazilian("Um castiçal medieval.");
            MH_CandleHolder_02.RequiredItems.Add("Wood", 1, true);
            MH_CandleHolder_02.RequiredItems.Add("Resin", 1, true);
            MH_CandleHolder_02.RequiredItems.Add("Coal", 1, true);
            MH_CandleHolder_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_CandleHolder_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_CandleHolder_03 = new("mar_timbercraft", "MH_CandleHolder_03");
            MH_CandleHolder_03.Category.Set("Lighting");
            MH_CandleHolder_03.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_CandleHolder_03.Tool.Add("TimberHammer");
            MH_CandleHolder_03.Name.English("Candle Holder III");
            MH_CandleHolder_03.Name.Portuguese_Brazilian("Castiçal III");
            MH_CandleHolder_03.Description.English("A medieval candle holder.");
            MH_CandleHolder_03.Description.Portuguese_Brazilian("Um castiçal medieval.");
            MH_CandleHolder_03.RequiredItems.Add("Wood", 1, true);
            MH_CandleHolder_03.RequiredItems.Add("Resin", 1, true);
            MH_CandleHolder_03.RequiredItems.Add("Coal", 1, true);
            MH_CandleHolder_03.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_CandleHolder_03.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_CandleHolder_04 = new("mar_timbercraft", "MH_CandleHolder_04");
            MH_CandleHolder_04.Category.Set("Lighting");
            MH_CandleHolder_04.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_CandleHolder_04.Tool.Add("TimberHammer");
            MH_CandleHolder_04.Name.English("Candle Holder IV");
            MH_CandleHolder_04.Name.Portuguese_Brazilian("Castiçal IV");
            MH_CandleHolder_04.Description.English("A medieval candle holder.");
            MH_CandleHolder_04.Description.Portuguese_Brazilian("Um castiçal medieval.");
            MH_CandleHolder_04.RequiredItems.Add("Wood", 1, true);
            MH_CandleHolder_04.RequiredItems.Add("Resin", 1, true);
            MH_CandleHolder_04.RequiredItems.Add("Coal", 1, true);
            MH_CandleHolder_04.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_CandleHolder_04.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_CandleHolder_05 = new("mar_timbercraft", "MH_CandleHolder_05");
            MH_CandleHolder_05.Category.Set("Lighting");
            MH_CandleHolder_05.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_CandleHolder_05.Tool.Add("TimberHammer");
            MH_CandleHolder_05.Name.English("Candle Holder V");
            MH_CandleHolder_05.Name.Portuguese_Brazilian("Castiçal V");
            MH_CandleHolder_05.Description.English("A medieval candle holder.");
            MH_CandleHolder_05.Description.Portuguese_Brazilian("Um castiçal medieval.");
            MH_CandleHolder_05.RequiredItems.Add("Wood", 1, true);
            MH_CandleHolder_05.RequiredItems.Add("Resin", 1, true);
            MH_CandleHolder_05.RequiredItems.Add("Coal", 1, true);
            MH_CandleHolder_05.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_CandleHolder_05.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_CandleHolder_06 = new("mar_timbercraft", "MH_CandleHolder_06");
            MH_CandleHolder_06.Category.Set("Lighting");
            MH_CandleHolder_06.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_CandleHolder_06.Tool.Add("TimberHammer");
            MH_CandleHolder_06.Name.English("Candle Holder VI");
            MH_CandleHolder_06.Name.Portuguese_Brazilian("Castiçal VI");
            MH_CandleHolder_06.Description.English("A medieval candle holder.");
            MH_CandleHolder_06.Description.Portuguese_Brazilian("Um castiçal medieval.");
            MH_CandleHolder_06.RequiredItems.Add("Wood", 1, true);
            MH_CandleHolder_06.RequiredItems.Add("Resin", 1, true);
            MH_CandleHolder_06.RequiredItems.Add("Coal", 1, true);
            MH_CandleHolder_06.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_CandleHolder_06.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Chandelier_01 = new("mar_timbercraft", "MH_Chandelier_01");
            MH_Chandelier_01.Category.Set("Lighting");
            MH_Chandelier_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Chandelier_01.Tool.Add("TimberHammer");
            MH_Chandelier_01.Name.English("Chandelier I");
            MH_Chandelier_01.Name.Portuguese_Brazilian("Lustre I");
            MH_Chandelier_01.Description.English("A decorative medieval chandelier.");
            MH_Chandelier_01.Description.Portuguese_Brazilian("Um lustre medieval decorativo.");
            MH_Chandelier_01.RequiredItems.Add("Wood", 1, true);
            MH_Chandelier_01.RequiredItems.Add("Resin", 1, true);
            MH_Chandelier_01.RequiredItems.Add("Coal", 1, true);
            MH_Chandelier_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Chandelier_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Chandelier_02 = new("mar_timbercraft", "MH_Chandelier_02");
            MH_Chandelier_02.Category.Set("Lighting");
            MH_Chandelier_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Chandelier_02.Tool.Add("TimberHammer");
            MH_Chandelier_02.Name.English("Chandelier II");
            MH_Chandelier_02.Name.Portuguese_Brazilian("Lustre II");
            MH_Chandelier_02.Description.English("A decorative medieval chandelier.");
            MH_Chandelier_02.Description.Portuguese_Brazilian("Um lustre medieval decorativo.");
            MH_Chandelier_02.RequiredItems.Add("Wood", 1, true);
            MH_Chandelier_02.RequiredItems.Add("Resin", 1, true);
            MH_Chandelier_02.RequiredItems.Add("Coal", 1, true);
            MH_Chandelier_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Chandelier_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Chandelier_03 = new("mar_timbercraft", "MH_Chandelier_03");
            MH_Chandelier_03.Category.Set("Lighting");
            MH_Chandelier_03.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Chandelier_03.Tool.Add("TimberHammer");
            MH_Chandelier_03.Name.English("Chandelier III");
            MH_Chandelier_03.Name.Portuguese_Brazilian("Lustre III");
            MH_Chandelier_03.Description.English("A decorative medieval chandelier.");
            MH_Chandelier_03.Description.Portuguese_Brazilian("Um lustre medieval decorativo.");
            MH_Chandelier_03.RequiredItems.Add("Wood", 1, true);
            MH_Chandelier_03.RequiredItems.Add("Resin", 1, true);
            MH_Chandelier_03.RequiredItems.Add("Coal", 1, true);
            MH_Chandelier_03.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Chandelier_03.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            #endregion

            #region EXTERNAL

            BuildPiece BannerPole1 = new("mar_timbercraft", "BannerPole1");
            BannerPole1.Category.Set("External");
            BannerPole1.Crafting.Set(PieceManager.CraftingTable.Workbench);
            BannerPole1.Tool.Add("TimberHammer");
            BannerPole1.Name.English("Banner Pole 1");
            BannerPole1.Name.Portuguese_Brazilian("Mastro de Banner 1");
            BannerPole1.Description.English("A decorative medieval banner pole.");
            BannerPole1.Description.Portuguese_Brazilian("Um mastro de banner medieval decorativo.");
            BannerPole1.RequiredItems.Add("Wood", 1, true);
            BannerPole1.RequiredItems.Add("Resin", 1, true);
            BannerPole1.RequiredItems.Add("Coal", 1, true);
            BannerPole1.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(BannerPole1.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece BannerPole2 = new("mar_timbercraft", "BannerPole2");
            BannerPole2.Category.Set("External");
            BannerPole2.Crafting.Set(PieceManager.CraftingTable.Workbench);
            BannerPole2.Tool.Add("TimberHammer");
            BannerPole2.Name.English("Banner Pole 2");
            BannerPole2.Name.Portuguese_Brazilian("Mastro de Banner 2");
            BannerPole2.Description.English("A decorative medieval banner pole.");
            BannerPole2.Description.Portuguese_Brazilian("Um mastro de banner medieval decorativo.");
            BannerPole2.RequiredItems.Add("Wood", 1, true);
            BannerPole2.RequiredItems.Add("Resin", 1, true);
            BannerPole2.RequiredItems.Add("Coal", 1, true);
            BannerPole2.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(BannerPole2.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece BannerPole3 = new("mar_timbercraft", "BannerPole3");
            BannerPole3.Category.Set("External");
            BannerPole3.Crafting.Set(PieceManager.CraftingTable.Workbench);
            BannerPole3.Tool.Add("TimberHammer");
            BannerPole3.Name.English("Banner Pole 3");
            BannerPole3.Name.Portuguese_Brazilian("Mastro de Banner 3");
            BannerPole3.Description.English("A decorative medieval banner pole.");
            BannerPole3.Description.Portuguese_Brazilian("Um mastro de banner medieval decorativo.");
            BannerPole3.RequiredItems.Add("Wood", 1, true);
            BannerPole3.RequiredItems.Add("Resin", 1, true);
            BannerPole3.RequiredItems.Add("Coal", 1, true);
            BannerPole3.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(BannerPole3.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece BannerPole4 = new("mar_timbercraft", "BannerPole4");
            BannerPole4.Category.Set("External");
            BannerPole4.Crafting.Set(PieceManager.CraftingTable.Workbench);
            BannerPole4.Tool.Add("TimberHammer");
            BannerPole4.Name.English("Banner Pole 4");
            BannerPole4.Name.Portuguese_Brazilian("Mastro de Banner 4");
            BannerPole4.Description.English("A decorative medieval banner pole.");
            BannerPole4.Description.Portuguese_Brazilian("Um mastro de banner medieval decorativo.");
            BannerPole4.RequiredItems.Add("Wood", 1, true);
            BannerPole4.RequiredItems.Add("Resin", 1, true);
            BannerPole4.RequiredItems.Add("Coal", 1, true);
            BannerPole4.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(BannerPole4.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece BannerPole5 = new("mar_timbercraft", "BannerPole5");
            BannerPole5.Category.Set("External");
            BannerPole5.Crafting.Set(PieceManager.CraftingTable.Workbench);
            BannerPole5.Tool.Add("TimberHammer");
            BannerPole5.Name.English("Banner Pole 5");
            BannerPole5.Name.Portuguese_Brazilian("Mastro de Banner 5");
            BannerPole5.Description.English("A decorative medieval banner pole.");
            BannerPole5.Description.Portuguese_Brazilian("Um mastro de banner medieval decorativo.");
            BannerPole5.RequiredItems.Add("Wood", 1, true);
            BannerPole5.RequiredItems.Add("Resin", 1, true);
            BannerPole5.RequiredItems.Add("Coal", 1, true);
            BannerPole5.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(BannerPole5.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Sign_Alchemy_Village = new("mar_timbercraft", "Sign_Alchemy_Village");
            Sign_Alchemy_Village.Category.Set("External");
            Sign_Alchemy_Village.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Sign_Alchemy_Village.Tool.Add("TimberHammer");
            Sign_Alchemy_Village.Name.English("Alchemy Shop Sign");
            Sign_Alchemy_Village.Name.Portuguese_Brazilian("Placa de Alquimia");
            Sign_Alchemy_Village.Description.English("A wooden sign for an alchemy shop.");
            Sign_Alchemy_Village.Description.Portuguese_Brazilian("Uma placa de madeira para uma loja de alquimia.");
            Sign_Alchemy_Village.RequiredItems.Add("Wood", 1, true);
            Sign_Alchemy_Village.RequiredItems.Add("Resin", 1, true);
            Sign_Alchemy_Village.RequiredItems.Add("Coal", 1, true);
            Sign_Alchemy_Village.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Sign_Alchemy_Village.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Sign_Blacksmith_Village = new("mar_timbercraft", "Sign_Blacksmith_Village");
            Sign_Blacksmith_Village.Category.Set("External");
            Sign_Blacksmith_Village.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Sign_Blacksmith_Village.Tool.Add("TimberHammer");
            Sign_Blacksmith_Village.Name.English("Blacksmith Shop Sign");
            Sign_Blacksmith_Village.Name.Portuguese_Brazilian("Placa de Ferreiro");
            Sign_Blacksmith_Village.Description.English("A wooden sign for a blacksmith shop.");
            Sign_Blacksmith_Village.Description.Portuguese_Brazilian("Uma placa de madeira para uma ferraria.");
            Sign_Blacksmith_Village.RequiredItems.Add("Wood", 1, true);
            Sign_Blacksmith_Village.RequiredItems.Add("Resin", 1, true);
            Sign_Blacksmith_Village.RequiredItems.Add("Coal", 1, true);
            Sign_Blacksmith_Village.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Sign_Blacksmith_Village.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Sign_Goods_Village = new("mar_timbercraft", "Sign_Goods_Village");
            Sign_Goods_Village.Category.Set("External");
            Sign_Goods_Village.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Sign_Goods_Village.Tool.Add("TimberHammer");
            Sign_Goods_Village.Name.English("General Goods Sign");
            Sign_Goods_Village.Name.Portuguese_Brazilian("Placa de Mercadorias");
            Sign_Goods_Village.Description.English("A wooden sign for a general goods store.");
            Sign_Goods_Village.Description.Portuguese_Brazilian("Uma placa de madeira para uma loja de mercadorias gerais.");
            Sign_Goods_Village.RequiredItems.Add("Wood", 1, true);
            Sign_Goods_Village.RequiredItems.Add("Resin", 1, true);
            Sign_Goods_Village.RequiredItems.Add("Coal", 1, true);
            Sign_Goods_Village.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Sign_Goods_Village.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Sign_Inn_Village = new("mar_timbercraft", "Sign_Inn_Village");
            Sign_Inn_Village.Category.Set("External");
            Sign_Inn_Village.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Sign_Inn_Village.Tool.Add("TimberHammer");
            Sign_Inn_Village.Name.English("Inn Sign");
            Sign_Inn_Village.Name.Portuguese_Brazilian("Placa de Pousada");
            Sign_Inn_Village.Description.English("A wooden sign for an inn.");
            Sign_Inn_Village.Description.Portuguese_Brazilian("Uma placa de madeira para uma pousada.");
            Sign_Inn_Village.RequiredItems.Add("Wood", 1, true);
            Sign_Inn_Village.RequiredItems.Add("Resin", 1, true);
            Sign_Inn_Village.RequiredItems.Add("Coal", 1, true);
            Sign_Inn_Village.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Sign_Inn_Village.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Sign_Magic_Village = new("mar_timbercraft", "Sign_Magic_Village");
            Sign_Magic_Village.Category.Set("External");
            Sign_Magic_Village.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Sign_Magic_Village.Tool.Add("TimberHammer");
            Sign_Magic_Village.Name.English("Magic Shop Sign");
            Sign_Magic_Village.Name.Portuguese_Brazilian("Placa de Loja de Magia");
            Sign_Magic_Village.Description.English("A wooden sign for a magic shop.");
            Sign_Magic_Village.Description.Portuguese_Brazilian("Uma placa de madeira para uma loja de magia.");
            Sign_Magic_Village.RequiredItems.Add("Wood", 1, true);
            Sign_Magic_Village.RequiredItems.Add("Resin", 1, true);
            Sign_Magic_Village.RequiredItems.Add("Coal", 1, true);
            Sign_Magic_Village.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Sign_Magic_Village.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Sign_Tavern_Village = new("mar_timbercraft", "Sign_Tavern_Village");
            Sign_Tavern_Village.Category.Set("External");
            Sign_Tavern_Village.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Sign_Tavern_Village.Tool.Add("TimberHammer");
            Sign_Tavern_Village.Name.English("Tavern Sign");
            Sign_Tavern_Village.Name.Portuguese_Brazilian("Placa de Taverna");
            Sign_Tavern_Village.Description.English("A wooden sign for a tavern.");
            Sign_Tavern_Village.Description.Portuguese_Brazilian("Uma placa de madeira para uma taverna.");
            Sign_Tavern_Village.RequiredItems.Add("Wood", 1, true);
            Sign_Tavern_Village.RequiredItems.Add("Resin", 1, true);
            Sign_Tavern_Village.RequiredItems.Add("Coal", 1, true);
            Sign_Tavern_Village.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Sign_Tavern_Village.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Dummy_village = new("mar_timbercraft", "Dummy_village");
            Dummy_village.Category.Set("External");
            Dummy_village.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Dummy_village.Tool.Add("TimberHammer");
            Dummy_village.Name.English("Training Dummy");
            Dummy_village.Name.Portuguese_Brazilian("Boneco de Treino");
            Dummy_village.Description.English("A wooden training dummy for combat practice.");
            Dummy_village.Description.Portuguese_Brazilian("Um boneco de madeira para praticar combate.");
            Dummy_village.RequiredItems.Add("Wood", 1, true);
            Dummy_village.RequiredItems.Add("Resin", 1, true);
            Dummy_village.RequiredItems.Add("Coal", 1, true);
            Dummy_village.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Dummy_village.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece IronFirePit_Village = new("mar_timbercraft", "IronFirePit_Village");
            IronFirePit_Village.Category.Set("External");
            IronFirePit_Village.Crafting.Set(PieceManager.CraftingTable.Workbench);
            IronFirePit_Village.Tool.Add("TimberHammer");
            IronFirePit_Village.Name.English("Iron Fire Pit");
            IronFirePit_Village.Name.Portuguese_Brazilian("Fogueira de Ferro");
            IronFirePit_Village.Description.English("An iron fire pit for outdoor lighting and warmth.");
            IronFirePit_Village.Description.Portuguese_Brazilian("Uma fogueira de ferro para iluminação e calor ao ar livre.");
            IronFirePit_Village.RequiredItems.Add("Wood", 1, true);
            IronFirePit_Village.RequiredItems.Add("Resin", 1, true);
            IronFirePit_Village.RequiredItems.Add("Coal", 1, true);
            IronFirePit_Village.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(IronFirePit_Village.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece StreetLampPole = new("mar_timbercraft", "StreetLampPole");
            StreetLampPole.Category.Set("External");
            StreetLampPole.Crafting.Set(PieceManager.CraftingTable.Workbench);
            StreetLampPole.Tool.Add("TimberHammer");
            StreetLampPole.Name.English("Street Lamp Pole");
            StreetLampPole.Name.Portuguese_Brazilian("Poste de Lampião");
            StreetLampPole.Description.English("A medieval street lamp pole for illuminating roads.");
            StreetLampPole.Description.Portuguese_Brazilian("Um poste de lampião medieval para iluminar ruas.");
            StreetLampPole.RequiredItems.Add("Wood", 1, true);
            StreetLampPole.RequiredItems.Add("Resin", 1, true);
            StreetLampPole.RequiredItems.Add("Coal", 1, true);
            StreetLampPole.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(StreetLampPole.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece WheelPostPole = new("mar_timbercraft", "WheelPostPole");
            WheelPostPole.Category.Set("External");
            WheelPostPole.Crafting.Set(PieceManager.CraftingTable.Workbench);
            WheelPostPole.Tool.Add("TimberHammer");
            WheelPostPole.Name.English("Wheel Post Pole");
            WheelPostPole.Name.Portuguese_Brazilian("Poste com Roda");
            WheelPostPole.Description.English("A decorative post with a wooden wheel.");
            WheelPostPole.Description.Portuguese_Brazilian("Um poste decorativo com uma roda de madeira.");
            WheelPostPole.RequiredItems.Add("Wood", 1, true);
            WheelPostPole.RequiredItems.Add("Resin", 1, true);
            WheelPostPole.RequiredItems.Add("Coal", 1, true);
            WheelPostPole.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(WheelPostPole.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Canopy_01 = new("mar_timbercraft", "Canopy_01");
            Canopy_01.Category.Set("External");
            Canopy_01.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Canopy_01.Tool.Add("TimberHammer");
            Canopy_01.Name.English("Canopy 01");
            Canopy_01.Name.Portuguese_Brazilian("Toldo 01");
            Canopy_01.Description.English("An Canopy to protect and decorate your villa.");
            Canopy_01.Description.Portuguese_Brazilian("Um toldo para proteger e decorar a sua vila.");
            Canopy_01.RequiredItems.Add("Wood", 1, true);
            Canopy_01.RequiredItems.Add("Resin", 1, true);
            Canopy_01.RequiredItems.Add("Coal", 1, true);
            Canopy_01.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Canopy_01.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Canopy_02 = new("mar_timbercraft", "Canopy_02");
            Canopy_02.Category.Set("External");
            Canopy_02.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Canopy_02.Tool.Add("TimberHammer");
            Canopy_02.Name.English("Canopy 02");
            Canopy_02.Name.Portuguese_Brazilian("Toldo 02");
            Canopy_02.Description.English("An Canopy to protect and decorate your villa.");
            Canopy_02.Description.Portuguese_Brazilian("Um toldo para proteger e decorar a sua vila.");
            Canopy_02.RequiredItems.Add("Wood", 1, true);
            Canopy_02.RequiredItems.Add("Resin", 1, true);
            Canopy_02.RequiredItems.Add("Coal", 1, true);
            Canopy_02.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Canopy_02.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Canopy_03 = new("mar_timbercraft", "Canopy_03");
            Canopy_03.Category.Set("External");
            Canopy_03.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Canopy_03.Tool.Add("TimberHammer");
            Canopy_03.Name.English("Canopy 03");
            Canopy_03.Name.Portuguese_Brazilian("Toldo 03");
            Canopy_03.Description.English("An Canopy to protect and decorate your villa.");
            Canopy_03.Description.Portuguese_Brazilian("Um toldo para proteger e decorar a sua vila.");
            Canopy_03.RequiredItems.Add("Wood", 1, true);
            Canopy_03.RequiredItems.Add("Resin", 1, true);
            Canopy_03.RequiredItems.Add("Coal", 1, true);
            Canopy_03.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Canopy_03.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Canopy_04 = new("mar_timbercraft", "Canopy_04");
            Canopy_04.Category.Set("External");
            Canopy_04.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Canopy_04.Tool.Add("TimberHammer");
            Canopy_04.Name.English("Canopy 04");
            Canopy_04.Name.Portuguese_Brazilian("Toldo 04");
            Canopy_04.Description.English("An Canopy to protect and decorate your villa.");
            Canopy_04.Description.Portuguese_Brazilian("Um toldo para proteger e decorar a sua vila.");
            Canopy_04.RequiredItems.Add("Wood", 1, true);
            Canopy_04.RequiredItems.Add("Resin", 1, true);
            Canopy_04.RequiredItems.Add("Coal", 1, true);
            Canopy_04.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Canopy_04.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Canopy_05 = new("mar_timbercraft", "Canopy_05");
            Canopy_05.Category.Set("External");
            Canopy_05.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Canopy_05.Tool.Add("TimberHammer");
            Canopy_05.Name.English("Canopy 05");
            Canopy_05.Name.Portuguese_Brazilian("Toldo 05");
            Canopy_05.Description.English("An Canopy to protect and decorate your villa.");
            Canopy_05.Description.Portuguese_Brazilian("Um toldo para proteger e decorar a sua vila.");
            Canopy_05.RequiredItems.Add("Wood", 1, true);
            Canopy_05.RequiredItems.Add("Resin", 1, true);
            Canopy_05.RequiredItems.Add("Coal", 1, true);
            Canopy_05.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Canopy_05.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece Canopy_06 = new("mar_timbercraft", "Canopy_06");
            Canopy_06.Category.Set("External");
            Canopy_06.Crafting.Set(PieceManager.CraftingTable.Workbench);
            Canopy_06.Tool.Add("TimberHammer");
            Canopy_06.Name.English("Canopy 06");
            Canopy_06.Name.Portuguese_Brazilian("Toldo 06");
            Canopy_06.Description.English("An Canopy to protect and decorate your villa.");
            Canopy_06.Description.Portuguese_Brazilian("Um toldo para proteger e decorar a sua vila.");
            Canopy_06.RequiredItems.Add("Wood", 1, true);
            Canopy_06.RequiredItems.Add("Resin", 1, true);
            Canopy_06.RequiredItems.Add("Coal", 1, true);
            Canopy_06.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(Canopy_06.Prefab, MaterialReplacer.ShaderType.UseUnityShader);


            #endregion

            #region DECORATION

            BuildPiece MH_blacksmith_anvil = new("mar_timbercraft", "MH_blacksmith_anvil");
            MH_blacksmith_anvil.Category.Set("Decorations");
            MH_blacksmith_anvil.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_blacksmith_anvil.Tool.Add("TimberHammer");
            MH_blacksmith_anvil.Name.English("Blacksmith Anvil");
            MH_blacksmith_anvil.Name.Portuguese_Brazilian("Bigorna de Ferreiro");
            MH_blacksmith_anvil.Description.English("A heavy iron anvil used in a blacksmith's workshop.");
            MH_blacksmith_anvil.Description.Portuguese_Brazilian("Uma pesada bigorna de ferro usada em uma ferraria.");
            MH_blacksmith_anvil.RequiredItems.Add("Wood", 1, true);
            MH_blacksmith_anvil.RequiredItems.Add("Resin", 1, true);
            MH_blacksmith_anvil.RequiredItems.Add("Coal", 1, true);
            MH_blacksmith_anvil.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_blacksmith_anvil.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Blacksmith_Grinding_Stone = new("mar_timbercraft", "MH_Blacksmith_Grinding_Stone");
            MH_Blacksmith_Grinding_Stone.Category.Set("Decorations");
            MH_Blacksmith_Grinding_Stone.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Blacksmith_Grinding_Stone.Tool.Add("TimberHammer");
            MH_Blacksmith_Grinding_Stone.Name.English("Grinding Stone");
            MH_Blacksmith_Grinding_Stone.Name.Portuguese_Brazilian("Pedra de Afiar");
            MH_Blacksmith_Grinding_Stone.Description.English("A stone wheel used for sharpening blades.");
            MH_Blacksmith_Grinding_Stone.Description.Portuguese_Brazilian("Uma roda de pedra usada para afiar lâminas.");
            MH_Blacksmith_Grinding_Stone.RequiredItems.Add("Wood", 1, true);
            MH_Blacksmith_Grinding_Stone.RequiredItems.Add("Resin", 1, true);
            MH_Blacksmith_Grinding_Stone.RequiredItems.Add("Coal", 1, true);
            MH_Blacksmith_Grinding_Stone.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Blacksmith_Grinding_Stone.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece MH_Blacksmith_Tools_Rack = new("mar_timbercraft", "MH_Blacksmith_Tools_Rack");
            MH_Blacksmith_Tools_Rack.Category.Set("Decorations");
            MH_Blacksmith_Tools_Rack.Crafting.Set(PieceManager.CraftingTable.Workbench);
            MH_Blacksmith_Tools_Rack.Tool.Add("TimberHammer");
            MH_Blacksmith_Tools_Rack.Name.English("Blacksmith Tools Rack");
            MH_Blacksmith_Tools_Rack.Name.Portuguese_Brazilian("Suporte de Ferramentas de Ferreiro");
            MH_Blacksmith_Tools_Rack.Description.English("A wall rack holding blacksmith tools and equipment.");
            MH_Blacksmith_Tools_Rack.Description.Portuguese_Brazilian("Um suporte de parede com ferramentas e equipamentos de ferreiro.");
            MH_Blacksmith_Tools_Rack.RequiredItems.Add("Wood", 1, true);
            MH_Blacksmith_Tools_Rack.RequiredItems.Add("Resin", 1, true);
            MH_Blacksmith_Tools_Rack.RequiredItems.Add("Coal", 1, true);
            MH_Blacksmith_Tools_Rack.RequiredItems.Add("Stone", 1, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(MH_Blacksmith_Tools_Rack.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            #endregion

            #region EFFECTS

            GameObject sfx_build_hammer_metal_Constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "sfx_build_hammer_metal_Constructions");
            GameObject sfx_build_hammer_stone_construction = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "sfx_build_hammer_stone_construction");
            GameObject sfx_build_hammer_wood_construction = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "sfx_build_hammer_wood_construction");
            GameObject sfx_chest_close_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "sfx_chest_close_constructions");
            GameObject sfx_chest_open_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "sfx_chest_open_constructions");
            GameObject sfx_metal_blocked_Constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "sfx_metal_blocked_Constructions");
            GameObject sfx_rock_destroyed_construction = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "sfx_rock_destroyed_construction");
            GameObject sfx_wood_destroyed_construction = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "sfx_wood_destroyed_construction");

            GameObject vfx_HitSparks_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_HitSparks_constructions");
            GameObject vfx_Place_construction = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_Place_construction");
            GameObject vfx_Place_Lustres_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_Place_Lustres_constructions");
            GameObject vfx_Place_stone_floor_2x2_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_Place_stone_floor_2x2_constructions");
            GameObject vfx_Place_stone_floor_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_Place_stone_floor_constructions");
            GameObject vfx_Place_wood_pole_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_Place_wood_pole_constructions");
            GameObject vfx_Place_wood_wall_construction = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_Place_wood_wall_construction");
            GameObject vfx_Place_workbench_construction = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_Place_workbench_construction");
            GameObject vfx_RockHit_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_RockHit_constructions");
            GameObject vfx_SawDust_construction = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_SawDust_construction");
            GameObject vfx_stone_floor_2x2_destroyed_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_stone_floor_2x2_destroyed_constructions");
            GameObject vfx_stone_floor_destroyed_constructions = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "vfx_stone_floor_destroyed_constructions");

            #endregion


            SetupWatcher();
            _harmony.PatchAll();
        }
        private void OnDestroy()
        {
            Config.Save();
        }
        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }
        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Timbercraft.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Timbercraft.LogError($"There was an issue loading your {ConfigFileName}");
                Timbercraft.LogError("Please check your config entries for spelling and format!");
            }
        }

        private static ConfigEntry<bool>? _serverConfigLocked;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        public ConfigEntry<T> ModConfig<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, description, synchronizedSetting);
        }
        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        private static int GetZDO(int prefabHash)
        {
            int prefabCount = 0;
            foreach (List<ZDO> zdoList in ZDOMan.instance.m_objectsBySector)
            {
                if (zdoList == null) continue;

                for (int index = 0; index < zdoList.Count; ++index)
                {
                    ZDO zdo2 = zdoList[index];
                    if (zdo2.GetPrefab() == prefabHash)
                    {
                        prefabCount++;
                    }
                }
            }

            return prefabCount;
        }
        private static int GetPrefabCount(int prefabHash)
        {
            int prefabCount = 0;
            foreach (List<ZDO> zdoList in ZDOMan.instance.m_objectsBySector)
            {
                if (zdoList == null) continue;

                for (int index = 0; index < zdoList.Count; ++index)
                {
                    ZDO zdo2 = zdoList[index];
                    if (zdo2.GetPrefab() == prefabHash)
                    {
                        prefabCount++;
                    }
                }
            }

            return prefabCount;
        }
    }
}