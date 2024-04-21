using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CybergrindDeathcam.Utils
{
    public static class EnemyIdentifierExtension
    {
        public static GameObject GetObject(this EnemyIdentifier value) =>
            FirstNonNull(new MonoBehaviour[] { value.zombie, value.drone, value.machine, value.spider, value.statue } );

        private static GameObject FirstNonNull(IEnumerable<MonoBehaviour> values) =>
            values.Where(m => m != null).Select(m => m.gameObject).FirstOrDefault();
    }
}