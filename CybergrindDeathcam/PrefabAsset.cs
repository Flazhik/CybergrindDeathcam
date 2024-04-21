using System;

namespace CybergrindDeathcam
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PrefabAsset : Attribute
    {
        public string Path { get; }

        public PrefabAsset(string path = "")
        {
            Path = path;
        }
    }
}