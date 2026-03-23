using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cs2Overlay;

public class GameState
{
    [JsonPropertyName("map")]
    public MapState? Map { get; set; }

    [JsonPropertyName("round")]
    public RoundState? Round { get; set; }

    [JsonPropertyName("allplayers")]
    public Dictionary<string, PlayerState>? AllPlayers { get; set; }

    [JsonPropertyName("phase_countdowns")]
    public PhaseCountdowns? PhaseCountdowns { get; set; }
}

public class MapState
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("team_ct")]
    public TeamInfo? TeamCT { get; set; }

    [JsonPropertyName("team_t")]
    public TeamInfo? TeamT { get; set; }
}

public class TeamInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }
}

public class RoundState
{
    [JsonPropertyName("phase")]
    public string? Phase { get; set; }

    [JsonPropertyName("win_team")]
    public string? WinTeam { get; set; }
}

public class PhaseCountdowns
{
    [JsonPropertyName("phase")]
    public string? Phase { get; set; }

    [JsonPropertyName("phase_ends_in")]
    public double PhaseEndsIn { get; set; }
}

public class PlayerState
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("team")]
    public string? Team { get; set; }

    [JsonPropertyName("state")]
    public InternalPlayerState? State { get; set; }

    [JsonPropertyName("weapons")]
    public Dictionary<string, WeaponInfo>? Weapons { get; set; }
}

public class InternalPlayerState
{
    [JsonPropertyName("health")]
    public int Health { get; set; }

    [JsonPropertyName("money")]
    public int Money { get; set; }
}

public class WeaponInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}

public static class WeaponExtensions
{
    public static string GetActiveWeaponName(this Dictionary<string, WeaponInfo> weapons)
    {
        foreach (var w in weapons.Values)
        {
            if (w.State == "active")
                return w.Name ?? "Weapon";
        }
        return "Weapon";
    }

    public static string GetActiveWeaponIconPath(this Dictionary<string, WeaponInfo> weapons)
    {
        foreach (var w in weapons.Values)
        {
            if (w.State == "active")
            {
                var name = w.Name ?? string.Empty;
                var type = w.Type ?? string.Empty;

                // Prefer per-weapon icon by name
                var lower = name.ToLowerInvariant();

                // Rifles
                if (lower.Contains("ak47") || lower.Contains("ak-47"))
                    return "assets/weapons/ak47.svg";
                if (lower.Contains("m4a1_silencer") || lower.Contains("m4a1-s") || lower.Contains("m4a1"))
                    return "assets/weapons/m4a1_silencer.svg";
                if (lower.Contains("m4a4"))
                    return "assets/weapons/m4a1.svg";
                if (lower.Contains("famas"))
                    return "assets/weapons/famas.svg";
                if (lower.Contains("galilar") || lower.Contains("galil"))
                    return "assets/weapons/galilar.svg";
                if (lower.Contains("aug"))
                    return "assets/weapons/aug.svg";
                if (lower.Contains("sg556") || lower.Contains("sg-556"))
                    return "assets/weapons/sg556.svg";

                // Snipers
                if (lower.Contains("awp"))
                    return "assets/weapons/awp.svg";
                if (lower.Contains("ssg08") || lower.Contains("scout"))
                    return "assets/weapons/ssg08.svg";
                if (lower.Contains("scar20") || lower.Contains("scar-20"))
                    return "assets/weapons/scar20.svg";
                if (lower.Contains("g3sg1"))
                    return "assets/weapons/g3sg1.svg";

                // SMGs
                if (lower.Contains("mp9"))
                    return "assets/weapons/mp9.svg";
                if (lower.Contains("mp7"))
                    return "assets/weapons/mp7.svg";
                if (lower.Contains("mp5"))
                    return "assets/weapons/mp5sd.svg";
                if (lower.Contains("mac10") || lower.Contains("mac-10"))
                    return "assets/weapons/mac10.svg";
                if (lower.Contains("ump45") || lower.Contains("ump-45"))
                    return "assets/weapons/ump45.svg";
                if (lower.Contains("bizon"))
                    return "assets/weapons/bizon.svg";
                if (lower.Contains("p90"))
                    return "assets/weapons/p90.svg";

                // Pistols
                if (lower.Contains("glock"))
                    return "assets/weapons/glock.svg";
                if (lower.Contains("hkp2000") || lower.Contains("p2000"))
                    return "assets/weapons/hkp2000.svg";
                if (lower.Contains("usp"))
                    return "assets/weapons/usp_silencer.svg";
                if (lower.Contains("p250"))
                    return "assets/weapons/p250.svg";
                if (lower.Contains("cz75"))
                    return "assets/weapons/cz75a.svg";
                if (lower.Contains("fiveseven") || lower.Contains("five-seven"))
                    return "assets/weapons/fiveseven.svg";
                if (lower.Contains("tec9") || lower.Contains("tec-9"))
                    return "assets/weapons/tec9.svg";
                if (lower.Contains("deagle") || lower.Contains("deserteagle"))
                    return "assets/weapons/deagle.svg";
                if (lower.Contains("revolver") || lower.Contains("r8"))
                    return "assets/weapons/revolver.svg";

                // Utility / equipment
                if (lower.Contains("hegrenade"))
                    return "assets/weapons/hegrenade.svg";
                if (lower.Contains("flashbang"))
                    return "assets/weapons/flashbang.svg";
                if (lower.Contains("smokegrenade"))
                    return "assets/weapons/smokegrenade.svg";
                if (lower.Contains("incgrenade") || lower.Contains("molotov"))
                    return lower.Contains("incgrenade") ? "assets/weapons/incgrenade.svg" : "assets/weapons/molotov.svg";
                if (lower.Contains("decoy"))
                    return "assets/weapons/decoy.svg";
                if (lower.Contains("taser") || lower.Contains("zeus"))
                    return "assets/weapons/taser.svg";
                if (lower.Contains("c4"))
                    return "assets/weapons/c4.svg";

                // Knife / melee
                if (type == "Knife" || lower.Contains("knife"))
                    return "assets/weapons/knife.svg";

                // Fallback per-type icons
                return type switch
                {
                    "Pistol" => "assets/weapons/hkp2000.svg",
                    "Rifle" => "assets/weapons/ak47.svg",
                    "SniperRifle" => "assets/weapons/awp.svg",
                    "SMG" => "assets/weapons/mp9.svg",
                    "Grenade" => "assets/weapons/hegrenade.svg",
                    _ => "assets/weapons/ak47.svg"
                };
            }
        }
        return "assets/weapons/ak47.svg";
    }
}

