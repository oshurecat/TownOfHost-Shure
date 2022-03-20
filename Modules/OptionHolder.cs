using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TownOfHost
{
    [Flags]
    public enum CustomGameMode
    {
        Standard = 0x01,
        HideAndSeek = 0x02,
        All = int.MaxValue
    }

    public static class Options
    {
        // オプションId
        public const int PresetId = 0;
        public const int ForceJapaneseOptionId = 999999;

        // プリセット
        private static readonly string[] presets =
        {
            "Preset_1", "Preset_2", "Preset_3",
            "Preset_4", "Preset_5"
        };

        // ゲームモード
        public static CustomOption GameMode;
        public static CustomGameMode CurrentGameMode
            => GameMode.Selection == 0 ? CustomGameMode.Standard : CustomGameMode.HideAndSeek;

        public static readonly string[] gameModes =
        {
            "Standard", "HideAndSeek",
        };

        // 役職数・確率
        public static Dictionary<CustomRoles, int> roleCounts;
        public static Dictionary<CustomRoles, float> roleSpawnChances;
        public static bool OptionControllerIsEnable = false;
        public static Dictionary<CustomRoles, CustomOption> CustomRoleCounts;
        public static Dictionary<CustomRoles, CustomOption> CustomRoleSpawnChances;
        public static readonly string[] rates =
        {
            "Rate0", "Rate10", "Rate20", "Rate30", "Rate40", "Rate50",
            "Rate60", "Rate70", "Rate80", "Rate90", "Rate100",
        };

        // 各役職の詳細設定
        public static CustomOption BountyTargetChangeTime;
        public static CustomOption BountySuccessKillCooldown;
        public static CustomOption BountyFailureKillCooldown;
        public static CustomOption BHDefaultKillCooldown;
        public static CustomOption SerialKillerCooldown;
        public static CustomOption SerialKillerLimit;
        public static CustomOption VampireKillDelay;
        public static CustomOption ShapeMasterShapeshiftDuration;
        public static CustomOption MadmateCanFixLightsOut; // TODO:mii-47 マッド役職統一
        public static CustomOption MadmateCanFixComms;
        public static CustomOption MadmateHasImpostorVision;
        public static CustomOption MadGuardianCanSeeWhoTriedToKill;
        public static CustomOption MadSnitchTasks;
        public static CustomOption CanMakeMadmateCount;

        public static CustomOption MayorAdditionalVote;
        public static CustomOption SabotageMasterSkillLimit;
        public static CustomOption SabotageMasterFixesDoors;
        public static CustomOption SabotageMasterFixesReactors;
        public static CustomOption SabotageMasterFixesOxygens;
        public static CustomOption SabotageMasterFixesComms;
        public static CustomOption SabotageMasterFixesElectrical;
        public static int SabotageMasterUsedSkillCount;
        public static CustomOption SheriffKillCooldown;
        public static CustomOption SheriffCanKillMadmate;
        public static CustomOption SheriffCanKillJester;
        public static CustomOption SheriffCanKillTerrorist;
        public static CustomOption SheriffCanKillOpportunist;

        // HideAndSeek
        public static CustomOption AllowCloseDoors;
        public static CustomOption KillDelay;
        public static CustomOption IgnoreCosmetics;
        public static CustomOption IgnoreVent;
        public static float HideAndSeekKillDelayTimer = 0f;
        public static float HideAndSeekImpVisionMin = 0.25f;

        // ボタン回数
        public static CustomOption SyncButtonMode;
        public static CustomOption SyncedButtonCount;
        public static int UsedButtonCount = 0;

        // タスク無効化
        public static CustomOption DisableSwipeCard;
        public static CustomOption DisableSubmitScan;
        public static CustomOption DisableUnlockSafe;
        public static CustomOption DisableUploadData;
        public static CustomOption DisableStartReactor;
        public static CustomOption DisableResetBreaker;

        // ランダムマップ
        public static CustomOption RandomMapsMode;
        public static CustomOption AddedTheSkeld;
        public static CustomOption AddedMiraHQ;
        public static CustomOption AddedPolus;
        public static CustomOption AddedTheAirShip;
        public static CustomOption AddedDleks;

        // 投票モード
        public static CustomOption WhenSkipVote;
        public static CustomOption WhenNonVote;
        public static CustomOption CanTerroristSuicideWin;
        public static readonly string[] voteModes =
        {
            "Default", "Suicide", "Skip"
        };
        public static VoteMode GetWhenSkipVote() => (VoteMode)WhenSkipVote.GetSelection();
        public static VoteMode GetWhenNonVote() => (VoteMode)WhenNonVote.GetSelection();

        // その他
        public static CustomOption NoGameEnd;
        public static CustomOption ForceJapanese;
        public static CustomOption AutoDisplayLastResult;
        public static CustomOption SuffixMode;
        public static readonly string[] suffixModes =
        {
            "SuffixMode_Node",
            "SuffixMode_Version",
            "SuffixMode_Streaming",
            "SuffixMode_Recording"
        };
        public static SuffixModes GetSuffixMode()
        {
            return (SuffixModes)SuffixMode.GetSelection();
        }



        public static int SnitchExposeTaskLeft = 1;


        private static bool IsLoaded = false;

        static Options()
        {
            resetRoleCounts();
        }
        public static void resetRoleCounts()
        {
            roleCounts = new Dictionary<CustomRoles, int>();
            roleSpawnChances = new Dictionary<CustomRoles, float>();

            foreach (var role in Enum.GetValues(typeof(CustomRoles)).Cast<CustomRoles>())
            {
                roleCounts.Add(role, 0);
                roleSpawnChances.Add(role, 0);
            }
        }

        public static void setRoleCount(CustomRoles role, int count)
        {
            roleCounts[role] = count;

            if (CustomRoleCounts.TryGetValue(role, out var option))
            {
                option.UpdateSelection(count);
            }
        }

        public static int getRoleCount(CustomRoles role)
        {
            return CustomRoleCounts.TryGetValue(role, out var option) ? option.GetSelection() : roleCounts[role];
        }

        public static void Load()
        {
            if (IsLoaded) return;

            // プリセット
            _ = CustomOption.Create(0, new Color(204f / 255f, 204f / 255f, 0, 1f), "Preset", presets, presets[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(CustomGameMode.All);

            // ゲームモード
            GameMode = CustomOption.Create(1, new Color(204f / 255f, 204f / 255f, 0, 1f), "GameMode", gameModes, gameModes[0], null, true)
                .SetGameMode(CustomGameMode.All);

            #region 役職・詳細設定
            CustomRoleCounts = new Dictionary<CustomRoles, CustomOption>();
            CustomRoleSpawnChances = new Dictionary<CustomRoles, CustomOption>();
            // Impostor
            SetupRoleOptions(1000, CustomRoles.BountyHunter);
            BountyTargetChangeTime = CustomOption.Create(1010, Color.white, "BountyTargetChangeTime", 150, 5, 1000, 5, CustomRoleSpawnChances[CustomRoles.BountyHunter]);
            BountySuccessKillCooldown = CustomOption.Create(1011, Color.white, "BountySuccessKillCooldown", 2, 5, 999, 1, CustomRoleSpawnChances[CustomRoles.BountyHunter]);
            BountyFailureKillCooldown = CustomOption.Create(1012, Color.white, "BountyFailureKillCooldown", 50, 5, 999, 1, CustomRoleSpawnChances[CustomRoles.BountyHunter]);
            BHDefaultKillCooldown = CustomOption.Create(1013, Color.white, "BHDefaultKillCooldown", 30, 2, 999, 1, CustomRoleSpawnChances[CustomRoles.BountyHunter]);
            SetupRoleOptions(1100, CustomRoles.SerialKiller);
            SerialKillerCooldown = CustomOption.Create(1110, Color.white, "SerialKillerCooldown", 20, 5, 1000, 1, CustomRoleSpawnChances[CustomRoles.SerialKiller]);
            SerialKillerLimit = CustomOption.Create(1111, Color.white, "SerialKillerLimit", 60, 5, 1000, 1, CustomRoleSpawnChances[CustomRoles.SerialKiller]);
            SetupRoleOptions(1200, CustomRoles.ShapeMaster);
            ShapeMasterShapeshiftDuration = CustomOption.Create(1210, Color.white, "ShapeMasterShapeshiftDuration", 10, 1, 1000, 1, CustomRoleSpawnChances[CustomRoles.ShapeMaster]);
            SetupRoleOptions(1300, CustomRoles.Vampire);
            VampireKillDelay = CustomOption.Create(1310, Color.white, "VampireKillDelay", 10, 1, 1000, 1, CustomRoleSpawnChances[CustomRoles.Vampire]);
            SetupRoleOptions(1400, CustomRoles.Warlock);
            CanMakeMadmateCount = CustomOption.Create(1410, Color.white, "CanMakeMadmateCount", 1, 1, 15, 1, CustomRoleSpawnChances[CustomRoles.Warlock]);
            SetupRoleOptions(1500, CustomRoles.Witch);
            SetupRoleOptions(1600, CustomRoles.Mafia);
            // Madmate
            SetupRoleOptions(10000, CustomRoles.Madmate);
            MadmateCanFixLightsOut = CustomOption.Create(10010, Color.white, "MadmateCanFixLightsOut", false, CustomRoleSpawnChances[CustomRoles.Madmate]);
            MadmateCanFixComms = CustomOption.Create(10011, Color.white, "MadmateCanFixComms", false, CustomRoleSpawnChances[CustomRoles.Madmate]);
            MadmateHasImpostorVision = CustomOption.Create(10012, Color.white, "MadmateHasImpostorVision", false, CustomRoleSpawnChances[CustomRoles.Madmate]);
            SetupRoleOptions(10100, CustomRoles.MadGuardian);
            MadGuardianCanSeeWhoTriedToKill = CustomOption.Create(10110, Color.white, "MadGuardianCanSeeWhoTriedToKill", false, CustomRoleSpawnChances[CustomRoles.MadGuardian]);
            SetupRoleOptions(10200, CustomRoles.MadSnitch);
            MadSnitchTasks = CustomOption.Create(10210, Color.white, "MadSnitchTasks", 4, 1, 20, 1, CustomRoleSpawnChances[CustomRoles.MadSnitch]);
            // Crewmate
            SetupRoleOptions(20000, CustomRoles.Bait);
            SetupRoleOptions(20100, CustomRoles.Lighter);
            SetupRoleOptions(20200, CustomRoles.Mayor);
            MayorAdditionalVote = CustomOption.Create(20210, Color.white, "MayorAdditionalVote", 1, 1, 99, 1, CustomRoleSpawnChances[CustomRoles.Mayor]);
            SetupRoleOptions(20300, CustomRoles.SabotageMaster);
            SabotageMasterSkillLimit = CustomOption.Create(20310, Color.white, "SabotageMasterSkillLimit", 1, 0, 99, 1, CustomRoleSpawnChances[CustomRoles.SabotageMaster]);
            SabotageMasterFixesDoors = CustomOption.Create(20311, Color.white, "SabotageMasterFixesDoors", false, CustomRoleSpawnChances[CustomRoles.SabotageMaster]);
            SabotageMasterFixesReactors = CustomOption.Create(20312, Color.white, "SabotageMasterFixesReactors", false, CustomRoleSpawnChances[CustomRoles.SabotageMaster]);
            SabotageMasterFixesOxygens = CustomOption.Create(20313, Color.white, "SabotageMasterFixesOxygens", false, CustomRoleSpawnChances[CustomRoles.SabotageMaster]);
            SabotageMasterFixesComms = CustomOption.Create(20314, Color.white, "SabotageMasterFixesCommunications", false, CustomRoleSpawnChances[CustomRoles.SabotageMaster]);
            SabotageMasterFixesElectrical = CustomOption.Create(20315, Color.white, "SabotageMasterFixesElectrical", false, CustomRoleSpawnChances[CustomRoles.SabotageMaster]);
            SetupRoleOptions(20400, CustomRoles.Sheriff);
            SheriffKillCooldown = CustomOption.Create(20410, Color.white, "SheriffKillCooldown", 30, 0, 990, 1, CustomRoleSpawnChances[CustomRoles.Sheriff]);
            SheriffCanKillMadmate = CustomOption.Create(20411, Color.white, "SheriffCanKillMadmate", true, CustomRoleSpawnChances[CustomRoles.Sheriff]);
            SheriffCanKillJester = CustomOption.Create(20412, Color.white, "SheriffCanKillJester", true, CustomRoleSpawnChances[CustomRoles.Sheriff]);
            SheriffCanKillTerrorist = CustomOption.Create(20413, Color.white, "SheriffCanKillTerrorist", true, CustomRoleSpawnChances[CustomRoles.Sheriff]);
            SheriffCanKillOpportunist = CustomOption.Create(20414, Color.white, "SheriffCanKillOpportunist", true, CustomRoleSpawnChances[CustomRoles.Sheriff]);
            SetupRoleOptions(20500, CustomRoles.Snitch);
            // Other
            SetupRoleOptions(50000, CustomRoles.Jester);
            SetupRoleOptions(50100, CustomRoles.Opportunist);
            SetupRoleOptions(50200, CustomRoles.Terrorist);
            #endregion

            // HideAndSeek
            SetupRoleOptions(100000, CustomRoles.Fox, CustomGameMode.HideAndSeek);
            SetupRoleOptions(100100, CustomRoles.Troll, CustomGameMode.HideAndSeek);
            AllowCloseDoors = CustomOption.Create(101000, Color.white, "AllowCloseDoors", false, null, true)
                .SetGameMode(CustomGameMode.HideAndSeek);
            KillDelay = CustomOption.Create(101001, Color.white, "HideAndSeekWaitingTime", 10, 0, 180, 5)
                .SetGameMode(CustomGameMode.HideAndSeek);
            IgnoreCosmetics = CustomOption.Create(101002, Color.white, "IgnoreCosmetics", false)
                .SetGameMode(CustomGameMode.HideAndSeek);
            IgnoreVent = CustomOption.Create(101003, Color.white, "IgnoreVent", false)
                .SetGameMode(CustomGameMode.HideAndSeek);

            // ボタン回数同期
            SyncButtonMode = CustomOption.Create(100200, Color.white, "SyncButtonMode", false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            SyncedButtonCount = CustomOption.Create(100201, Color.white, "SyncedButtonCount", 10, 0, 100, 1, SyncButtonMode)
                .SetGameMode(CustomGameMode.Standard);

            // タスク無効化
            DisableSwipeCard = CustomOption.Create(100300, Color.white, "DisableSwipeCardTask", false, null, true)
                .SetGameMode(CustomGameMode.All);
            DisableSubmitScan = CustomOption.Create(100301, Color.white, "DisableSubmitScanTask", false)
                .SetGameMode(CustomGameMode.All);
            DisableUnlockSafe = CustomOption.Create(100302, Color.white, "DisableUnlockSafeTask", false)
                .SetGameMode(CustomGameMode.All);
            DisableUploadData = CustomOption.Create(100303, Color.white, "DisableUploadDataTask", false)
                .SetGameMode(CustomGameMode.All);
            DisableStartReactor = CustomOption.Create(100304, Color.white, "DisableStartReactorTask", false)
                .SetGameMode(CustomGameMode.All);
            DisableResetBreaker = CustomOption.Create(100305, Color.white, "DisableResetBreakerTask", false)
                .SetGameMode(CustomGameMode.All);

            // ランダムマップ
            RandomMapsMode = CustomOption.Create(100400, Color.white, "RandomMapsMode", false, null, true)
                .SetGameMode(CustomGameMode.All);
            AddedTheSkeld = CustomOption.Create(100401, Color.white, "AddedTheSkeld", false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            AddedMiraHQ = CustomOption.Create(100402, Color.white, "AddedMIRAHQ", false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            AddedPolus = CustomOption.Create(100403, Color.white, "AddedPolus", false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            AddedTheAirShip = CustomOption.Create(100404, Color.white, "AddedTheAirShip", false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            // MapDleks = CustomOption.Create(100405, Color.white, "AddedDleks", false, RandomMapMode)
            //     .SetGameMode(CustomGameMode.All);

            // 投票モード
            WhenSkipVote = CustomOption.Create(100500, Color.white, "WhenSkipVote", voteModes, voteModes[0], null, true)
                .SetGameMode(CustomGameMode.Standard);
            WhenNonVote = CustomOption.Create(100501, Color.white, "WhenNonVote", voteModes, voteModes[0], null, false)
                .SetGameMode(CustomGameMode.Standard);
            CanTerroristSuicideWin = CustomOption.Create(100502, Color.white, "CanTerroristSuicideWin", false, null, false)
                .SetGameMode(CustomGameMode.Standard);

            // その他
            ForceJapanese = CustomOption.Create(ForceJapaneseOptionId, Color.white, "ForceJapanese", false, null, true)
                .SetGameMode(CustomGameMode.All);
            NoGameEnd = CustomOption.Create(100600, Color.white, "NoGameEnd", false, null, false)
                .SetGameMode(CustomGameMode.All);
            AutoDisplayLastResult = CustomOption.Create(100601, Color.white, "AutoDisplayLastResult", false)
                .SetGameMode(CustomGameMode.All);
            SuffixMode = CustomOption.Create(100602, Color.white, "SuffixMode", suffixModes, suffixModes[0])
                .SetGameMode(CustomGameMode.All);

            IsLoaded = true;
        }

        private static void SetupRoleOptions(int id, CustomRoles role, CustomGameMode customGameMode = CustomGameMode.Standard)
        {
            var spawnOption = CustomOption.Create(id, Utils.getRoleColor(role), Utils.getRoleName(role), rates, rates[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);
            var countOption = CustomOption.Create(id + 1, Color.white, "Maximum", 0, 0, 15, 1, spawnOption, false)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);

            CustomRoleSpawnChances.Add(role, spawnOption);
            CustomRoleCounts.Add(role, countOption);
        }

    }
}
