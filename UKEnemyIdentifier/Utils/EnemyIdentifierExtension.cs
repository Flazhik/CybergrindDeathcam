using System.Collections.Generic;

namespace UKEnemyIdentifier.Utils
{
    public static class EnemyIdentifierExtension
    {
        private static readonly Dictionary<EnemyType, string> EnemyNamesDict = new Dictionary<EnemyType, string>
        {
            { EnemyType.Sisyphus, "Sisyphean Insurrectionist" },
            { EnemyType.Mindflayer, "Mindflayer" },
            { EnemyType.Ferryman, "Ferryman" },
            { EnemyType.HideousMass, "Hideous Mass" },
            { EnemyType.Swordsmachine, "Swordsmachine" },
            { EnemyType.Gutterman, "Gutterman" },
            { EnemyType.Guttertank, "Guttertank" },
            { EnemyType.Mannequin, "Mannequin" },
            { EnemyType.Cerberus, "Cerberus" },
            { EnemyType.Drone, "Drone" },
            { EnemyType.MaliciousFace, "Malicious Face" },
            { EnemyType.Streetcleaner, "Streetcleaner" },
            { EnemyType.Virtue, "Virtue" },
            { EnemyType.Stalker, "Stalker" },
            { EnemyType.Stray, "Stray" },
            { EnemyType.Schism, "Schism" },
            { EnemyType.Soldier, "Soldier" },
            { EnemyType.Turret, "Turret" },
            { EnemyType.Filth, "Filth" },
            { EnemyType.Idol, "Idol" }
        };
        
        public static string GetEnemyName(this EnemyIdentifier value) =>
            EnemyNamesDict.TryGetValue(value.enemyType, out var name) ? name : "UNKNOWN ENTITY";
    }
}