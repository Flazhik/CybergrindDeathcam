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
        private const float ZoomInRate = 0.04f;
        private const float TimeFreezeRate = 0.4f;
        private const float DistanceToKillerThreshold = 0.5f;
        private const string CanvasPath = "/Player/FinishCanvas (1)";

        public EnemyIdentifier killer;
        public bool skipLeaderboards;
        
        private DeathcamState _state;
        private GameObject _killerObj;
        private float _killerHeight;

        public FinalCyberRank finalCyberRank;
        private NewMovement _nm;
        private TimeController _timeController;

        private Vector3 _riseToPosition;
        private Vector3 _risingVelocity;
        private float _distanceToKiller;

        private void Awake()
        {
            enabled = false;
        }

        private void Start()
        {
            _timeController = TimeController.Instance;
            finalCyberRank = GameObject.Find($"{CanvasPath}/Panel").GetComponent<FinalCyberRank>();
            _state = DeathcamState.FocusingOnKiller;
            _timeController.controlTimeScale = false;

            if (CybergrindDeathcam.SkipLeaderboards.value)
            {
                var cgData = GameProgressSaver.GetBestCyber();
                var difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
                var closeToPersonalBest = cgData.preciseWavesByDifficulty[difficulty] - finalCyberRank.savedWaves <=
                                          CybergrindDeathcam.LeaderboardsSkipThreshold.value;
                var showLeaderboardsUnconditionally =
                    CybergrindDeathcam.AlwaysShowLeaderboardsStartingFrom.value <= finalCyberRank.savedWaves;

                skipLeaderboards = !showLeaderboardsUnconditionally && !closeToPersonalBest;
            }

            if (CybergrindDeathcam.UseCampaignDeathSequence.value)
            {
                NewMovement.Instance.deathSequence.gameObject.SetActive(true);
                return;
            }

            if (killer == null)
                return;

            _nm = gameObject.GetComponent<NewMovement>();
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = _deathcamSfx;

            _riseToPosition = _nm.transform.position  + new Vector3(0f, RisingDistance, 0f);
            _killerObj = killer.GetObject();
            _killerHeight = GetKillerHeight();
            
            StartCoroutine(DeathCamRoutine());
            EnableNoclip();
        }

        private void Update()
        {
            if (CybergrindDeathcam.UseCampaignDeathSequence.value)
                return;
            
            if (_timeController.timeScale > 0.0)
            {
                if (KillerIsDeadOrUnknown())
                    MoveTimescaleTowards(0.0f, skipLeaderboards ? 0.2f : 0.01f);
                else
                {
                    switch (_state)
                    {
                        case DeathcamState.FocusingOnKiller:
                        {
                            MoveTimescaleTowards(0.0f, 0.01f);
                            FocusOnKiller();
                            return;
                        }

                        case DeathcamState.ZoomIn:
                        {
                            ZoomIn();
                            return;
                        }
                        case DeathcamState.TimeFreeze:
                        {
                            if (_timeController.timeScale > 0.0)
                                MoveTimescaleTowards(0.0f, TimeFreezeRate);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid Deathcam state");
                    }
                }

                Time.timeScale = _timeController.timeScale * _timeController.timeScaleModifier;
                return;
            }

            if (skipLeaderboards)
            {
                GameProgressSaver.AddMoney(finalCyberRank.totalPoints);
                SceneHelper.RestartScene();
            }

            StartCoroutine(ScheduleEndgameScreen(KillerIsDeadOrUnknown() ? 0f : 2f));
        }

        private IEnumerator ScheduleEndgameScreen(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            DisplayEndgameScreen();
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

        private void FocusOnKiller()
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

            if (KillerIsDeadOrUnknown())
            {
                _state = DeathcamState.TimeFreeze;
                return;
            }
            
            var distanceCorrection = _killerHeight * DeathcamDistanceCoefficient();
            var currentKillerPosition = _killerObj.transform.position;
            var cameraTransform = _nm.cc.cam.transform;
            var cameraPosition = cameraTransform.position;
            var targetPositionCorrection = distanceCorrection * ((currentKillerPosition - cameraPosition) / _distanceToKiller) - new Vector3(0f, _killerHeight / 4, 0f);
            var relativePosition = currentKillerPosition - cameraPosition;
            var cameraTargetPosition = currentKillerPosition - targetPositionCorrection;
            if (Vector3.Distance(cameraPosition, cameraTargetPosition) < DistanceToKillerThreshold)
            {
                _state = DeathcamState.TimeFreeze;
                return;
            }
            var lookRotation = Quaternion.LookRotation(relativePosition);
            _nm.cc.cam.transform.rotation =
                Quaternion.Lerp(_nm.cc.cam.transform.rotation, lookRotation, ZoomInRate);
            _nm.cc.cam.transform.position = Vector3.Lerp(cameraPosition, cameraTargetPosition, ZoomInRate);
            
            _distanceToKiller = Vector3.Distance(currentKillerPosition, _nm.cc.cam.transform.position);
            // _zoomInCutoff--;
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

        private void MoveTimescaleTowards(float target, float maxDelta) =>
            _timeController.timeScale = Mathf.MoveTowards(_timeController.timeScale, target,
                Time.unscaledDeltaTime *(_timeController.timeScale + maxDelta));

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

        private void DisplayEndgameScreen()
        {
            finalCyberRank.Appear();
            MonoSingleton<MusicManager>.Instance.forcedOff = true;
            MonoSingleton<MusicManager>.Instance.StopMusic();
        }
    }

    public enum DeathcamState
    {
        FocusingOnKiller,
        ZoomIn,
        TimeFreeze
    }
}