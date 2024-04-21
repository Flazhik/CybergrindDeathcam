using System.Collections.Generic;

namespace CybergrindDeathcam.Utils
{
    public static class EnemyNames
    {
        private static readonly Dictionary<EnemyType, string> EnemyNamesDict = new Dictionary<EnemyType, string>
        {
            { EnemyType.Sisyphus, "SISYPHEAN INSURRECTIONIST" },
            { EnemyType.Mindflayer, "MINDFLAYER" },
            { EnemyType.Ferryman, "FERRYMAN" },
            { EnemyType.HideousMass, "HIDEOUS MASS" },
            { EnemyType.Swordsmachine, "SWORDSMACHINE" },
            { EnemyType.Gutterman, "GUTTERMAN" },
            { EnemyType.Guttertank, "GUTTERTANK" },
            { EnemyType.Mannequin, "MANNEQUIN" },
            { EnemyType.Cerberus, "CERBERUS" },
            { EnemyType.Drone, "DRONE" },
            { EnemyType.MaliciousFace, "MALICIOUS FACE" },
            { EnemyType.Streetcleaner, "STREETCLEANER" },
            { EnemyType.Virtue, "VIRTUE" },
            { EnemyType.Stalker, "STALKER" },
            { EnemyType.Stray, "STRAY" },
            { EnemyType.Schism, "SCHISM" },
            { EnemyType.Soldier, "SOLDIER" },
            { EnemyType.Turret, "TURRET" },
            { EnemyType.Filth, "FILTH" },
            { EnemyType.Idol, "IDOL" }
        };

        public static string GetEnemyName(EnemyType type) =>
            EnemyNamesDict.TryGetValue(type, out var name) ? name : "UNKNOWN ENTITY";
    }
}