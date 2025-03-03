using System.Collections.Generic;
using System.Globalization;

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
            { EnemyType.Puppet, "Puppet" },
            { EnemyType.V2, "V2" },
            { EnemyType.V2Second, "V2" },
            { EnemyType.Gabriel, "Gabriel, Judge of Hell" },
            { EnemyType.GabrielSecond, "Gabriel, Apostate of Hate" },
            { EnemyType.Mandalore, "Mysterious Druid Knight (& Owl)" },
            { EnemyType.FleshPrison, "Flesh Prison" },
            { EnemyType.FleshPanopticon, "Flesh Panopticon" },
        };

        public static string GetEnemyName(this EnemyIdentifier value)
        {
            // Smelly, but it works
            if (value.FullName != "None")
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.FullName.ToLower());
            
            return EnemyNamesDict.TryGetValue(value.enemyType, out var name)
                ? name
                : "Unknown Entity";
        }
    }
}