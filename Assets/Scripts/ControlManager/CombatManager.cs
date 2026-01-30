using System.Collections.Generic;
using System.Linq;
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
        [Header("Config")] [SerializeField] private float spawnDistanceX;
        [SerializeField] private float timeDelayReady = 1f;
        [SerializeField] private float globalSpeedMultiplier = 1f;

        [Header("Knockback Settings")] [SerializeField]
        private float _enemyKnockbackForce = 8.0f; // Lực đẩy lùi Enemy (Vận tốc)

        [SerializeField] private float _backgroundKnockbackForce = 10f; // Lực đẩy background
        [SerializeField] private float finishKnockbackMultiplier = 2.5f;

        [Header("Refs")] [SerializeField] private PlayerController playerController;

        // Data Runtime
        private List<EnemyWaveData> _waves;
        private int _currentWaveIndex;
        private int countVocabIsCorrect;
        private EnemyWaveData _currentWave;
        private VocabData currentVocab = null;

        // State Runtime
        public EnemyController CurrentEnemy { get; private set; }
        public CombatState CurrentState;

        // Stats
        public int CurrentMistakes { get; private set; } // Lỗi trong từ hiện tại
        public int TotalMistakes { get; private set; } // Tổng lỗi toàn game

        // Timer
        private float _attackTimer;
        private float _attackDuration;
        private float _lastPunishTime; // Chống spam click quá nhanh (0.2s)

        // Flags
        private bool _isLastChancePhase = false; // Cờ đánh dấu đang trong giai đoạn "Cơ hội cuối cùng"

        private void Start()
        {
            CurrentState = CombatState.Waiting;
            if (!playerController) playerController = FindAnyObjectByType<PlayerController>();
            GameManager.Instance.inputDisplayManager.LockButton(true);
        }

        private void OnEnable()
        {
            GameEvents.OnLevelStart += SetUpStartLevel;
            GameEvents.OnReadyToFight += OnReadyToFight;
            GameEvents.OnCharCorrect += OnCharCorrect;
            GameEvents.OnCharWrong += OnCharWrong;
            GameEvents.OnSubmitAnswer += OnVocabFinished;
            GameEvents.OnFinisherFail += ExecuteFailFinisher;
            GameEvents.OnPlayerBroken += HandlePlayerDefeated;

            GameEvents.OnEndGame += OnEndGame;

        }

        private void OnDisable()
        {
            GameEvents.OnLevelStart -= SetUpStartLevel;
            GameEvents.OnReadyToFight -= OnReadyToFight;
            GameEvents.OnCharCorrect -= OnCharCorrect;
            GameEvents.OnCharWrong -= OnCharWrong;
            GameEvents.OnSubmitAnswer -= OnVocabFinished;
            GameEvents.OnFinisherFail -= ExecuteFailFinisher;
            GameEvents.OnPlayerBroken -= HandlePlayerDefeated;

 
            GameEvents.OnEndGame -= OnEndGame;
        }
        // --- LEVEL FLOW ---

        private void SetUpStartLevel(LevelData data)
        {
            _waves = data.Waves;
            _currentWaveIndex = 0;
            TotalMistakes = 0;
            _isLastChancePhase = false;
            StartWave();
        }

        private void StartWave()
        {
            _currentWave = _waves[_currentWaveIndex];
            countVocabIsCorrect = 0;
            _isLastChancePhase = false;
            foreach (VocabData vc in _currentWave.VocabList)
            {
                vc.isCorrect = false;
            }

            SpawnEnemy(_currentWave.EnemyProfile);
        }

        private void SpawnEnemy(EnemyProfile profile)
        {
            Vector2 pos = new Vector2(transform.position.x + spawnDistanceX, transform.position.y);
            CurrentEnemy = Instantiate(profile.getPrefabEnemy(), pos, Quaternion.identity);
            CurrentEnemy.SetupEnemyData(profile);
        }

        private void OnReadyToFight()
        {
            CurrentState = CombatState.Readying;
            this.DelayAction(timeDelayReady, () =>
            {
                CurrentState = CombatState.Fighting;
                StartCombatRound();
            });
            playerController.ReadyToFight(timeDelayReady);
        }

        private void StartCombatRound()
        {
            CurrentEnemy.SetActiveFuelSlider(true);
            GameManager.Instance.inputDisplayManager.LockButton(false);
            
            List<VocabData> vocabList = _currentWave.VocabList.ToList();
            foreach (VocabData vc in vocabList)
            {
                if (!currentVocab)
                {
                    currentVocab = vc;
                    break;
                }

                if (currentVocab != vc && !vc.isCorrect)
                {
                    currentVocab = vc;
                    break;
                }
            }

            bool isFinisher = countVocabIsCorrect == _currentWave.VocabList.Count - 1;
            CurrentMistakes = 0;

            GameManager.Instance.inputDisplayManager.GetVocabData(currentVocab);
            GameManager.Instance.inputDisplayManager.NextVocab();

            if (isFinisher)
            {
                SetupFinisherPhase();
            }
            else
            {
                if(playerController.currentState != ActorState.BrokenStand && playerController.currentState != ActorState.Focusing) playerController.FightStand();
            }
        }

        private void SetupFinisherPhase()
        {
             playerController.OnFocusing();
            CurrentEnemy.SetFuelGauge(1.2f);
        }

        // --- INPUT HANDLERS ---

        private void OnCharCorrect()
        {
            if (playerController.CurrentState != ActorState.Focusing)
            {
                CurrentEnemy.NextAttack();
                playerController.DoParry();

                if (playerController.currentState != ActorState.BrokenStand)
                {
                    float force = _enemyKnockbackForce;
                    if (countVocabIsCorrect == _currentWave.VocabList.Count - 1) force *= finishKnockbackMultiplier;
                    this.DelayAction(0.2f, () =>
                    {
                        if (CurrentEnemy) CurrentEnemy.TriggerKnockback(force);
                        if (GameManager.Instance.backGroundManager)
                            GameManager.Instance.backGroundManager.TriggerKnockback(_backgroundKnockbackForce);
                    });
                }
              
                CurrentEnemy.ResetFuelGauge();
            }
            else
            {
                playerController.CancelNextAttack();
                playerController.OnFocus();
            }

        }

        private void OnCharWrong()
        {
            
            CurrentMistakes++;
            TotalMistakes++;

            bool isFocusing = playerController.CurrentState == ActorState.Focusing;

            if (isFocusing)
            {
                if (CurrentMistakes >= playerController.CurrentHealth)
                {
                    ExecuteFailFinisher();
                }
            }
            else
            {
                HandleInputWrong();
            }
        }

        private void HandleInputWrong()
        {
            if (Time.time - _lastPunishTime < 0.2f) return;
            _lastPunishTime = Time.time;

            Debug.Log("CombatManager: HandleInputWrong. Applying IMMEDIATE penalty.");

            CurrentEnemy.NextAttack();
            playerController.DecreaseHealth();
            UpdateMistakes();
        }

        private void UpdateMistakes()
        {
            // Logic cập nhật UI mistake nếu cần
        }

        private void ResetAttackTimer()
        {
            // Reset timer logic if needed (Currently using FuelGauge)
        }

        // --- RESOLUTION ---

        private void HandlePlayerDefeated(bool isNextVocab)
        {
            Debug.Log("Player Broken! Entering Last Chance Phase...");
            playerController.BrokenStand(true);
            _isLastChancePhase = true; // Bật cờ đánh dấu đang trong giai đoạn hồi phục sinh tử
            CurrentEnemy.ResetFuelGauge();
            playerController.ResetHealth();
            if(isNextVocab) StartCombatRound();
            
        }

        /// <summary>
        /// Hàm này dùng để hồi phục trạng thái Player khi sang từ vựng mới.
        /// Đã đổi tên từ NextVocab -> ResetPlayerState để tránh nhầm lẫn.
        /// </summary>
     

        private void OnVocabFinished()
        {
            countVocabIsCorrect = 0;
            foreach (VocabData vc in _currentWave.VocabList)
            {
                if (vc.isCorrect) countVocabIsCorrect++;
            }

            Debug.Log($"Vocab Finished. Progress: {countVocabIsCorrect}/{_currentWave.VocabList.Count}");

            // Kiểm tra cờ Last Chance
            if (_isLastChancePhase)
            {
                Debug.Log("Last Chance Success! Resetting Wave & Standing Up.");

                _isLastChancePhase = false; // Tắt cờ vì đã thành công

                countVocabIsCorrect = 0;
                foreach (VocabData vc in _currentWave.VocabList)
                {
                    vc.isCorrect = false;
                }

                playerController.BrokenStand(false);
                playerController.ResetHealth();
                playerController.ResetLife();
                StartCombatRound();
                return;
            }

            playerController.ResetHealth();
            bool isFinisher = countVocabIsCorrect >= _currentWave.VocabList.Count;
            bool isBroken = countVocabIsCorrect == _currentWave.VocabList.Count - 1;

            if (isFinisher)
            {
                ExecuteWinFinisher();
            }
            else
            {
                if (isBroken)  this.DelayAction(0.2f, () => { CurrentEnemy.BrokenStand(true); });
                StartCombatRound();
            }
        }

        private void ExecuteWinFinisher()
        {
            Debug.Log("PERFECT FINISHER!");
            GameManager.Instance.inputDisplayManager.LockButton(true);
            CurrentEnemy.SetActiveFuelSlider(false);
            GameManager.Instance.CutScenesManager.PlayFinisherSuccess(CurrentEnemy);
            this.DelayAction(5f, KillEnemyAndNextWave);
        }

        private void ExecuteFailFinisher()
        {
            Debug.Log("FAILED FINISHER -> RESET FIGHT");
            GameManager.Instance.inputDisplayManager.LockButton(true);
            CurrentEnemy.SetActiveFuelSlider(false);
            GameManager.Instance.CutScenesManager.PlayFinisherFail(CurrentEnemy,(() =>
            {
                this.DelayAction(0.2f, StartCombatRound);
            }));

            countVocabIsCorrect = 0;
            foreach (VocabData vc in _currentWave.VocabList)
            {
                if (vc.isCorrect) vc.isCorrect = false;
            }
                HandlePlayerDefeated(false); // Phuong an tam thoi SOS
            
             playerController.DecreaseLife();
            if (CurrentEnemy)
            {
                CurrentEnemy.BrokenStand(false);
            }
           
            
        }

        private void OnEndGame()
        {
            GameManager.Instance.inputDisplayManager.LockButton(true);
            CurrentEnemy.SetActiveFuelSlider(false);
            GameManager.Instance.gameState = GameState.CompletedLevel;
        }
     
        private void KillEnemyAndNextWave()
        {
            if (CurrentEnemy) Destroy(CurrentEnemy.gameObject);
            _currentWaveIndex++;
            StartWave();
        }

    }
}
