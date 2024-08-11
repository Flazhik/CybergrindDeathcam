using System;
using System.Collections;
using CybergrindDeathcam.Utils;
using TMPro;
using UnityEngine;
using static CybergrindDeathcam.Utils.ReflectionUtils;
using EnemyIdentifierExtension = UKEnemyIdentifier.Utils.EnemyIdentifierExtension;

#pragma warning disable CS0649

namespace CybergrindDeathcam.Components
{
    public class DeathCam: MonoBehaviour
    {
        [PrefabAsset("assets/ui/elements/notification.prefab")]
        private static GameObject _notificationPrefab;
        [PrefabAsset("assets/audio/deathcam_sfx.mp3")]
        private static AudioClip _deathcamSfx;
        private static AudioSource _audioSource;
        
        private const float RisingDistance = 10;
        private const float RisingTime = 0.1f;
        private const float PointCameraAtKillerTime = 0.02f;
        private const float ZoomInRate = 0.12f;
        private const float TimeFreezeRate = 0.2f;
        private const float DistanceToKillerThreshold = 0.5f;
        private const float TimeScaleThreshold = 0.2f;
        private const string CanvasPath = "/Player/FinishCanvas (1)";

        public EnemyIdentifier killer;

        private DeathcamState _state;
        private GameObject _killerObj;
        private float _killerHeight;
        private bool _skipLeaderboards;
        private bool _skipIgnored;
        
        private NewMovement _nm;
        private TimeController _timeController;
        private FinalCyberRank _finalCyberRank;
        
        private Vector3 _riseToPosition;
        private Vector3 _risingVelocity;
        private float _distanceToKiller;
        private float _zoomInCutoff = 60;

        private void Awake()
        {
            enabled = false;
        }

        private void Start()
        {
            _timeController = TimeController.Instance;
            _finalCyberRank = GameObject.Find($"{CanvasPath}/Panel").GetComponent<FinalCyberRank>();
            _state = DeathcamState.FocusingOnKiller;
            _skipLeaderboards = CybergrindDeathcam.SkipLeaderboards.value;

            if (killer == null)
                return;

            _nm = gameObject.GetComponent<NewMovement>();
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = _deathcamSfx;

            _riseToPosition = _nm.transform.position  + new Vector3(0f, RisingDistance, 0f);
            _killerObj = killer.GetObject();
            _killerHeight = GetKillerHeight();
            EnableNoclip();
            StartCoroutine(DeathCamRoutine());
        }

        private void Update()
        {
            if (Time.timeScale > TimeScaleThreshold || !_skipLeaderboards || _skipIgnored)
                return;
            
            var cgData = GameProgressSaver.GetBestCyber();
            var difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
            var closeToPersonalBest = cgData.preciseWavesByDifficulty[difficulty] - _finalCyberRank.savedWaves <= 
                                      CybergrindDeathcam.LeaderboardsSkipThreshold.value;
            var showLeaderboardsUnconditionally =
                CybergrindDeathcam.AlwaysShowLeaderboardsStartingFrom.value <= _finalCyberRank.savedWaves;

            if (showLeaderboardsUnconditionally || closeToPersonalBest)
            {
                _skipIgnored = true;
                return;
            }

            GameProgressSaver.AddMoney(_finalCyberRank.totalPoints);
            SceneHelper.RestartScene();
        }

        private void FixedUpdate()
        {
            if (KillerIsDeadOrUnknown())
            {
                if (_skipLeaderboards)
                    _timeController.timeScaleModifier = 0.05f;
                Time.timeScale = _timeController.timeScale * _timeController.timeScaleModifier;
                return;
            }
            
            switch (_state)
            {
                case DeathcamState.FocusingOnKiller:
                {
                    FocusingOnKiller();
                    break;
                }
                
                case DeathcamState.ZoomIn:
                {
                    ZoomIn();
                    break;
                }
                case DeathcamState.TimeFreeze:
                {
                    _timeController.controlTimeScale = false;
                    while (_timeController.timeScale > 0.0)
                    {
                        Time.timeScale = Mathf.MoveTowards(Time.timeScale, 0.0f, TimeFreezeRate);
                        return;
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException($"Invalid Deathcam state");
            }
        }

        private IEnumerator DeathCamRoutine()
        {
            yield return new WaitForSecondsRealtime(2);
            ShowNotification();
            _audioSource.Play();
            _state = DeathcamState.ZoomIn;
        }

        private void EnableNoclip()
        {
            _nm.GetComponent<Rigidbody>().isKinematic = true;
            _nm.GetComponent<KeepInBounds>().enabled = false;
        }

        private void FocusingOnKiller()
        {
            if (KillerIsDeadOrUnknown())
                return;

            if (Math.Abs(_nm.transform.position.y - _riseToPosition.y) > float.Epsilon)
                _nm.transform.position = Vector3.SmoothDamp(_nm.transform.position, _riseToPosition, ref _risingVelocity, RisingTime);

            var relativePosition = _killerObj.transform.position - _nm.cc.cam.transform.position;
            var killerPosition = Quaternion.LookRotation(relativePosition);
            _nm.cc.cam.transform.rotation =
                Quaternion.Lerp(_nm.cc.cam.transform.rotation, killerPosition, PointCameraAtKillerTime);
        }

        private void ZoomIn()
        {
            _distanceToKiller = Vector3.Distance(_killerObj.transform.position, _nm.cc.cam.transform.position);

            if (_zoomInCutoff == 0 || KillerIsDeadOrUnknown())
            {
                _state = DeathcamState.TimeFreeze;
                return;
            }
            
            var distanceCorrection = _killerHeight * DeathcamDistanceCoefficient();
            if (_distanceToKiller - distanceCorrection < DistanceToKillerThreshold)
            {
                _state = DeathcamState.TimeFreeze;
                return;
            }

            var currentKillerPosition = _killerObj.transform.position;
            var cameraTransform = _nm.cc.cam.transform;
            var cameraPosition = cameraTransform.position;
            
            var targetPositionCorrection = distanceCorrection * ((currentKillerPosition - cameraPosition) / _distanceToKiller) - new Vector3(0f, _killerHeight / 4, 0f);
            var relativePosition = currentKillerPosition - cameraPosition;
            var cameraTargetPosition = currentKillerPosition - targetPositionCorrection;

            var lookRotation = Quaternion.LookRotation(relativePosition);
            _nm.cc.cam.transform.rotation =
                Quaternion.Lerp(_nm.cc.cam.transform.rotation, lookRotation, ZoomInRate);
            _nm.cc.cam.transform.position = Vector3.Lerp(cameraPosition, cameraTargetPosition, ZoomInRate);
            
            _distanceToKiller = Vector3.Distance(currentKillerPosition, _nm.cc.cam.transform.position);
            _zoomInCutoff--;
        }

        private float GetKillerHeight()
        {
            var ensim = _killerObj.GetComponentsInChildren<EnemySimplifier>();
            if (ensim.Length == 0)
                return default;

            var renderer = (Renderer)GetPrivate(ensim[0], typeof(EnemySimplifier), "meshrenderer");
            
            // Fun fact: Guttertanks are 59994 units tall (for the reference, Insurrectionist's height is 15 units)
            if (_killerObj.TryGetComponent<Guttertank>(out _))
                return 6f;
            
            return renderer == default ? default : renderer.bounds.size.y;
        }

        private bool KillerIsDeadOrUnknown() => _killerObj == null || killer == null || killer.dead;

        private float DeathcamDistanceCoefficient()
        {
            return killer.enemyType switch
            {
                EnemyType.Swordsmachine => 3.5f,
                EnemyType.Gutterman => 1.3f,
                EnemyType.Mannequin => 0.7f,
                EnemyType.Cerberus => 0.6f,
                EnemyType.MaliciousFace => 0.6f,
                EnemyType.HideousMass => 0.6f,
                _ => 1.0f
            };
        }

        private void ShowNotification()
        {
            var canvas = GameObject.Find(CanvasPath);
            var notification = Instantiate(_notificationPrefab, canvas.transform);
            notification.transform.SetSiblingIndex(0);
            var text = notification.transform.Find("KillerName").GetComponent<TextMeshProUGUI>();
            text.text = EnemyIdentifierExtension.GetEnemyName(killer).ToUpper();
        }
    }

    public enum DeathcamState
    {
        FocusingOnKiller,
        ZoomIn,
        TimeFreeze
    }
}