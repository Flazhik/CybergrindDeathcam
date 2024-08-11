using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UKEnemyIdentifier.Utils.ReflectionUtils;

namespace UKEnemyIdentifier.Components
{
    [ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
    public class EnemyIdentifierManager: MonoSingleton<EnemyIdentifierManager>
    {
        public event PlayerHurtEventDelegate OnPlayerHurt;
        
        internal bool DeadAlready;
        internal GameObject LastHurtingFactor;

        private List<(GameObject, EnemyIdentifier)> _killingFactorByOrigin = new List<(GameObject, EnemyIdentifier)>();
        
        internal void PlayerHurt(EnemyIdentifier eid, int damage, bool playerKilled = false)
        {
            OnPlayerHurt?.Invoke(new PlayerHurtEvent
            {
                EnemyId = eid,
                PlayerIsKilled = playerKilled,
                Damage = damage
            });
        }

        private void Start()
        {
            SceneManager.sceneLoaded += (s, lsm) => DeadAlready = false;
            StartCoroutine(SlowUpdate());
        }

        private void Update()
        {
            LastHurtingFactor = default;
        }

        internal void RegisterFactor(GameObject factor, EnemyIdentifier emitter)
        {
            _killingFactorByOrigin.Add((factor, emitter));
        }
        
        internal void RegisterProjectileSpread(GameObject factor, GameObject projectile)
        {
            var origin = _killingFactorByOrigin.FirstOrDefault(e => e.Item1 == projectile);
            if (origin != default)
                _killingFactorByOrigin.Add((factor, origin.Item2));
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal EnemyIdentifier GetIdentifierFor(Projectile projectile)
        {
            return _killingFactorByOrigin
                .Where(pair => pair.Item1 == projectile.gameObject)
                .Select(pair => pair.Item2)
                .FirstOrDefault();
        }
        
        internal EnemyIdentifier GetIdentifierFor(RevolverBeam beam)
        {
            return _killingFactorByOrigin
                .Where(pair => pair.Item1 == beam.gameObject)
                .Select(pair => pair.Item2)
                .FirstOrDefault();
        }        
        
        internal EnemyIdentifier GetIdentifierFor(LightningStrikeExplosive beam)
        {
            return _killingFactorByOrigin
                .Where(pair => pair.Item1 == beam.gameObject)
                .Select(pair => pair.Item2)
                .FirstOrDefault();
        }
        
        internal void RegisterRevolverBeamExplosion(RevolverBeam beam, Explosion explosion)
        {
            var beamSource = GetIdentifierFor(beam);
            if (beamSource == default)
                return;

            explosion.originEnemy = beamSource;
        }
        
        internal void RegisterLightningStrikeExplosion(LightningStrikeExplosive lightning, Explosion explosion)
        {
            var lightningSource = GetIdentifierFor(lightning);
            if (lightningSource == default)
                return;

            explosion.originEnemy = lightningSource;
        }

        internal EnemyIdentifier IdentifyEnemy()
        {
            if (LastHurtingFactor == default)
                return default;

            if (LastHurtingFactor.TryGetComponent<Projectile>(out _)
                || LastHurtingFactor.TryGetComponent<PhysicalShockwave>(out _)
                || LastHurtingFactor.TryGetComponent<MassSpear>(out _)
                || LastHurtingFactor.TryGetComponent<RevolverBeam>(out _)
                || LastHurtingFactor.TryGetComponent<ContinuousBeam>(out _))
                return FindByHurtingFactor(LastHurtingFactor);
            
            if (LastHurtingFactor.TryGetComponent<SwingCheck2>(out var swingCheck))
                return swingCheck.eid;

            if (LastHurtingFactor.TryGetComponent<Explosion>(out var explosion))
                return explosion.originEnemy;

            if (LastHurtingFactor.TryGetComponent<FireZone>(out var thisFffire))
                return thisFffire.source == FlameSource.Streetcleaner
                    ? ((Streetcleaner)GetPrivate(thisFffire, typeof(FireZone), "sc")).eid
                    : default;

            if (LastHurtingFactor.TryGetComponent<VirtueInsignia>(out var virtueBeam))
                return (EnemyIdentifier)GetPrivate(virtueBeam.parentDrone, typeof(Drone), "eid");

            return default;
        }

        private EnemyIdentifier FindByHurtingFactor(GameObject go)
        {
            return _killingFactorByOrigin
                .Where(pair => pair.Item1 == go)
                .Select(pair => pair.Item2)
                .FirstOrDefault();
        }

        // Clean up the list every now and then
        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        private IEnumerator SlowUpdate()
        {
            while (true)
            {
                UpdateList();
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        private void UpdateList()
        {
            _killingFactorByOrigin = _killingFactorByOrigin
                .Where(e => e.Item1 != default && e.Item2 != default)
                .ToList();
        }

        public class PlayerHurtEvent
        {
            public EnemyIdentifier EnemyId;
            public bool PlayerIsKilled;
            public int Damage;
        }
        
        public delegate void PlayerHurtEventDelegate(PlayerHurtEvent data);
    }
}