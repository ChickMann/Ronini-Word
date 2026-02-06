using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.Playables;

namespace ControlManager
{
    /// <summary>
    /// Quản lý logic chiến đấu, thời gian tấn công và xử lý kết quả thắng/thua.
    /// Refactored theo GDD mới: 1 Enemy - N Vocabs - Wave System.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [Header("Config")] 
        [SerializeField] private float timeDelayReady = 1f;
        [SerializeField] private float globalSpeedMultiplier = 1f;

        [Header("Knockback Settings")] 
        [SerializeField] private float backgroundKnockbackForce = 10f; 
        
        [Header("Refs")] [SerializeField] private PlayerController playerController;
        
        private CinemachineImpulseSource _myImpulse;
        

        // Data Runtime
        private LevelData _curentLevelData;
        private EnemyWaveData _currentWave;
        private int _currentWaveIndex;
        private int _currentVocabIndex;
        private VocabData _currentVocab;

        // State Runtime
        public EnemyController CurrentEnemy { get; private set; }
        public CombatState combatState;

        // Stats
        private int TotalMistakes;

        // Timer
        private float _attackTimer;
        private float _attackDuration;
        private float _lastPunishTime; // Chống spam click quá nhanh (0.2s)


        private void Start()
        {
            _myImpulse = GetComponent<CinemachineImpulseSource>();
            combatState = CombatState.Ending;
            if (!playerController) playerController = FindAnyObjectByType<PlayerController>();
           
        }
        

        private void OnEnable()
        {
            GameEvents.OnCharCorrect += OnCharCorrect;
            GameEvents.OnCharWrong += OnCharWrong;
            GameEvents.OnSubmitAnswer += OnVocabFinished;
        }

        private void OnDisable()
        {
            GameEvents.OnCharCorrect -= OnCharCorrect;
            GameEvents.OnCharWrong -= OnCharWrong;
            GameEvents.OnSubmitAnswer -= OnVocabFinished;
        }

        private void Update()
        {
            if(CurrentEnemy && CurrentEnemy.hasPlayerDetected && combatState == CombatState.Running) OnReadyToFight(); 
            
        }

        // --- LEVEL FLOW ---

        public void SetUpStartLevel(LevelData data)
        {
            _curentLevelData = data;
            _currentWaveIndex = 0;
            TotalMistakes = 0;
            StartWave();
        }

        private void StartWave()
        {
            playerController.ResetTrigger();
            playerController.CancelNextAttack();
            if (_currentWaveIndex >= _curentLevelData.Waves.Count)
            {
                CompletedLevel();
                return;
            }
            _currentWave = _curentLevelData.Waves[_currentWaveIndex];
            SpawnEnemy(_currentWave.EnemyProfile);
            combatState = CombatState.Running;
        }

        private void SpawnEnemy(EnemyProfile profile)
        {
            Vector2 pos = new Vector2(transform.position.x + profile.spawnDistanceX, transform.position.y);
            CurrentEnemy = Instantiate(profile.getPrefabEnemy(), pos, Quaternion.identity);
            CurrentEnemy.SetupEnemyData(profile);
        }

        public void OnReadyToFight()
        {
            combatState = CombatState.Readying;
            this.DelayAction(timeDelayReady, () =>
            {
                combatState = CombatState.Fighting;
                StartCombatRound();
                playerController.ReadyToFight();
            });
        }

        private void StartCombatRound()
        {
            Debug.Log("Start Combat Round");
            CurrentEnemy.SetActiveFuelSlider(true);
            _currentVocabIndex = 0;
            _curentLevelData.Waves[_currentWaveIndex].VocabList.Shuffle();
            _currentVocab = _curentLevelData.Waves[_currentWaveIndex].VocabList[_currentVocabIndex];
            GameManager.Instance.inputDisplayManager.GetVocabData(_currentVocab);
            GameManager.Instance.inputDisplayManager.InputNextVocab();
        }

        private void NextVocab()
        {
            GameManager.Instance.inputDisplayManager.LockButton(true);
            if (_currentVocabIndex >= _currentWave.VocabList.Count - 1) return;
             _currentVocabIndex++;
             _currentVocab = _curentLevelData.Waves[_currentWaveIndex].VocabList[_currentVocabIndex];
            GameManager.Instance.inputDisplayManager.GetVocabData(_currentVocab);
            GameManager.Instance.inputDisplayManager.InputNextVocab();
        }

        private void SetupFinisherPhase()
        {
             playerController.OnFocusing();
            CurrentEnemy.SetFuelGauge(1.2f);
        }

        // --- INPUT HANDLERS ---
        private void OnVocabFinished(bool isWrongInWave)
        {
            if(combatState == CombatState.Ending) return;
            bool isFinisher = _currentVocabIndex >= _currentWave.VocabList.Count-1;
            bool isBorken = _currentVocabIndex == _currentWave.VocabList.Count-2;
            Debug.Log("Finisher: " + isFinisher);
            if (isFinisher)
            {
                if(!isWrongInWave) playerController.AddHealth();
                playerController.DoParry();
                ExecuteWinFinisher();
            }
            else
            {
               
                if (isBorken)
                {
                    CurrentEnemy.BrokenStand(true);
                    playerController.ResetTrigger();
                    
                    playerController.OnFocusing();
                }
                NextVocab();
            }

        }
        private void OnCharCorrect()
        {
          CurrentEnemy.NextAttack();
          if(playerController.currentState == ActorState.Focusing) playerController.OnFocus();
          else playerController.DoParry();
          CurrentEnemy.ResetFuelGauge();
          
        }

        private void OnCharWrong()
        {
            TotalMistakes++;
            CurrentEnemy.NextAttack();
            playerController.DecreaseHealth();
            CurrentEnemy.ResetFuelGauge();
        }

        private void ExecuteWinFinisher()
        {
            Debug.Log("PERFECT FINISHER!");
            GameManager.Instance.inputDisplayManager.LockButton(true);
            CurrentEnemy.SetActiveFuelSlider(false);
            this.DelayAction(0.5f, CurrentEnemy.Die);
            GameManager.Instance.CutScenesManager.PlayFinisherSuccess(0.5f,CurrentEnemy,(() =>
            {   
                playerController.ResetTrigger();
                playerController.CancelNextAttack();
                playerController.Idle();
            
            }));
            this.DelayAction(5f,() =>KillEnemyAndNextWave(5f));
        }

      
        public void OnEndGame()
        {
            GameManager.Instance.inputDisplayManager.LockButton(true);
            CurrentEnemy.SetActiveFuelSlider(false);
           combatState = CombatState.Ending;
        }
     
        private void KillEnemyAndNextWave(float timeDestroy)
        {
            if (CurrentEnemy) Destroy(CurrentEnemy.gameObject,timeDestroy);
            _currentWaveIndex++;
            StartWave();
        }

        private void CompletedLevel()
        {
            combatState = CombatState.Ending;
        }
        public void DoCinematicShake()
        {
            if (_myImpulse != null)
            {
                _myImpulse.GenerateImpulse(Vector3.one * 0.1f);
            }
        }
        
    }
}
