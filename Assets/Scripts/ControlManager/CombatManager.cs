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

        [Header("Knockback Settings")] 
        [SerializeField] private float backgroundKnockbackForce = 10f; 
        
        [Header("Refs")] 
        [SerializeField] private PlayerController playerController;
        
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
        public void ResetAll()
        {
            if (CurrentEnemy) 
            {
                Destroy(CurrentEnemy.gameObject);
            }
            playerController.ResetPlayer();
          
        }
        public void SetUpStartLevel(LevelData data)
        {
            _curentLevelData = data;
            _currentWaveIndex = 0;
            _currentVocabIndex = 0;
            if (CurrentEnemy) 
            {
                Destroy(CurrentEnemy.gameObject);
            }
            StartWave();
            playerController.ResetPlayer();
        }

        private void StartWave()
        {
            GameManager.Instance.inputDisplayManager.ResetStartWave();
            playerController.ResetTrigger();
            playerController.CancelNextAttack();
            if (_currentWaveIndex >= _curentLevelData.Waves.Count)
            {
               GameManager.Instance.EndLevel();
                return;
            }
            _currentWave = _curentLevelData.Waves[_currentWaveIndex];
            SpawnEnemy(_currentWave.EnemyProfile,_currentWave.VocabList.Count);
            combatState = CombatState.Running;
        }

        private void SpawnEnemy(EnemyProfile profile, int countVocab)
        {
            Vector2 pos = new Vector2(transform.position.x + profile.spawnDistanceX, transform.position.y);
            CurrentEnemy = Instantiate(profile.getPrefabEnemy(), pos, Quaternion.identity);
            CurrentEnemy.SetupEnemyData(profile,countVocab);
        }

        public void OnReadyToFight()
        {
            combatState = CombatState.Readying;
            this.DelayAction(timeDelayReady, () =>
            {
                combatState = CombatState.Fighting;
                StartCombatRound();
                playerController.ReadyToFight();
                CurrentEnemy.FightStand();
            });
        }

        private void StartCombatRound()
        {
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
            CurrentEnemy.HealthDecrease();
          
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
                    SmallHedge.AudioManager.AudioManager.PlaySound(SmallHedge.AudioManager.SoundType.BrokenStand);
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
            CurrentEnemy.NextAttack();
            playerController.DecreaseHealth();
            CurrentEnemy.ResetFuelGauge();
          DoCinematicShake();
          GameManager.Instance.inputDisplayManager.SetIsWrongInWave();
            
        }

        private void ExecuteWinFinisher()
        {
            Debug.Log("PERFECT FINISHER!");
            GameManager.Instance.inputDisplayManager.LockButton(true);
            CurrentEnemy.SetActiveFuelSlider(false);
            GameManager.Instance.CutScenesManager.ScheduleTimelineAction(1.22f, () =>
            {
                CurrentEnemy.Die();
                CurrentEnemy.SetActiveHealth(false);
            });
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
           CurrentEnemy.SetActiveHealth(false);
         GameDataManager.Instance.OnWaveEnded();
        }
     
        private void KillEnemyAndNextWave(float timeDestroy)
        {
            if (CurrentEnemy) Destroy(CurrentEnemy.gameObject,timeDestroy);
            _currentWaveIndex++;
            StartWave();
        }

  
        public void DoCinematicShake()
        {
            if (_myImpulse != null)
            {
                _myImpulse.GenerateImpulse(Vector3.right * 0.3f);
            }
        }
        
        public void ResetLevlel()
        {
            combatState = CombatState.Running;
            // 1. Hủy Enemy hiện tại trên Scene (nếu có) để chuẩn bị spawn con mới
            if (CurrentEnemy) 
            {
                Destroy(CurrentEnemy.gameObject);
            }

            // 2. Reset lại index của từ vựng về 0
            _currentVocabIndex = 0;
            _currentWaveIndex = 0;
            
            // 3. Bắt đầu lại wave hiện tại (StartWave đã bao gồm logic reset UI và Player)
            StartWave();
            playerController.ResetPlayer();
        }
      
    }
}
