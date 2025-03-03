using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UKEnemyIdentifier.Utils.ReflectionUtils;
using Object = UnityEngine.Object;

namespace UKEnemyIdentifier.Components
{
    [ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
    public class EnemyIdentifierManager: MonoSingleton<EnemyIdentifierManager>
    {
        public event PlayerHurtEventDelegate OnPlayerHurt;
        
        internal bool DeadAlready;
        internal GameObject LastHurtingFactor;

        private List<(GameObject, EnemyIdentifier)> _killingFactorByOrigin = new List<(GameObject, EnemyIdentifier)>();

        private void Start()
        {
            SceneManager.sceneLoaded += (s, lsm) => DeadAlready = false;
            InvokeRepeating(nameof(ListCleanup), 1f, 1f);
        }

        private void ListCleanup()
        {
            _killingFactorByOrigin = _killingFactorByOrigin
                .Where(e => e.Item1 != default && e.Item2 != default)
                .ToList();
        }
        
        private void Update() => LastHurtingFactor = default;

        internal void RegisterFactor(GameObject factor, EnemyIdentifier emitter) =>
            _killingFactorByOrigin.Add((factor, emitter));

        internal EnemyIdentifier GetIdentifierFor(Component component)
        {
            return _killingFactorByOrigin
                .Where(pair => pair.Item1 == component.gameObject)
                .Select(pair => pair.Item2)
                .FirstOrDefault();
        }
        
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void RegisterExplosion(Component component, Explosion explosion)
        {
            var source = GetIdentifierFor(component);
            if (source == default)
                return;

            explosion.originEnemy = source;
        }

        internal EnemyIdentifier IdentifyEnemy()
        {
            if (LastHurtingFactor == default)
                return default;

            return EnemyItselfIsDamagingFactor(out var identifier)
                   || EnemyIdStoredInDictionary(out identifier)
                   || LookForEnemyIdInDamagingFactor(out identifier)
                ? identifier
                : default;
        }
        
        private bool EnemyIdStoredInDictionary(out EnemyIdentifier identifier)
        {
            if (LastHurtingFactor.TryGetComponent<Projectile>(out _)
                || LastHurtingFactor.TryGetComponent<PhysicalShockwave>(out _)
                || LastHurtingFactor.TryGetComponent<MassSpear>(out _)
                || LastHurtingFactor.TryGetComponent<RevolverBeam>(out _)
                || LastHurtingFactor.TryGetComponent<ContinuousBeam>(out _)
                || LastHurtingFactor.TryGetComponent<Nail>(out _)
                || LastHurtingFactor.TryGetComponent<BlackHoleProjectile>(out _))
            {
                identifier = FindByHurtingFactor(LastHurtingFactor);
                return true;
            }

            identifier = default;
            return false;
            
            EnemyIdentifier FindByHurtingFactor(Object go)
            {
                return _killingFactorByOrigin
                    .Where(pair => pair.Item1 == go)
                    .Select(pair => pair.Item2)
                    .FirstOrDefault();
            }
        }

        private bool EnemyItselfIsDamagingFactor(out EnemyIdentifier identifier)
        {
            if (LastHurtingFactor.TryGetComponent<MinosPrime>(out var minos))
                return Found(() => (EnemyIdentifier)GetPrivate(minos, typeof(MinosPrime), "eid"), out identifier);

            if (LastHurtingFactor.TryGetComponent<SisyphusPrime>(out var sissy))
                return Found(() => (EnemyIdentifier)GetPrivate(sissy, typeof(SisyphusPrime), "eid"), out identifier);

            if (LastHurtingFactor.TryGetComponent<GabrielSecond>(out var gabrielSecond))
                return Found(() => (EnemyIdentifier)GetPrivate(gabrielSecond, typeof(GabrielSecond), "eid"), out identifier);

            identifier = default;
            return false;

            bool Found(Func<EnemyIdentifier> func, out EnemyIdentifier i)
            {
                i = func.Invoke();
                return true;
            }
        }

        private bool LookForEnemyIdInDamagingFactor(out EnemyIdentifier identifier)
        {
            if (LastHurtingFactor.TryGetComponent<SwingCheck2>(out var swingCheck))
                return Found(() => swingCheck.eid, out identifier);

            if (LastHurtingFactor.TryGetComponent<Explosion>(out var explosion))
                return Found(() => explosion.originEnemy, out identifier);

            if (LastHurtingFactor.TryGetComponent<FireZone>(out var thisFffire))
                return Found(() => thisFffire.source == FlameSource.Streetcleaner
                    ? ((Streetcleaner)GetPrivate(thisFffire, typeof(FireZone), "sc")).eid
                    : default,
                    out identifier);

            if (LastHurtingFactor.TryGetComponent<VirtueInsignia>(out var virtueBeam))
            {
                return Found(() => virtueBeam.parentDrone != null
                    ? (EnemyIdentifier)GetPrivate(virtueBeam.parentDrone, typeof(Drone), "eid")
                    : virtueBeam.otherParent.gameObject.GetComponent<EnemyIdentifier>(),
                    out identifier);
            }

            if (LastHurtingFactor.TryGetComponent<Coin>(out var coin))
                return Found(() => (EnemyIdentifier)GetPrivate(coin, typeof(Coin), "eid"), out identifier);

            identifier = default;
            return false;
            
            bool Found(Func<EnemyIdentifier> func, out EnemyIdentifier i) 
            {
                i = func.Invoke();
                return true;
            }
        }
        
        internal void PlayerHurt(EnemyIdentifier eid, int damage, bool playerKilled = false)
        {
            OnPlayerHurt?.Invoke(new PlayerHurtEvent
            {
                EnemyId = eid,
                PlayerIsKilled = playerKilled,
                Damage = damage
            });
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