using SmallHedge.AudioManager;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ControlManager
{
    public class PlayerController : Actor
    {
        [Header("Player Stats")]
        [SerializeField] private int maxHealth=3;

        [SerializeField] private bool hasShield;

        [Header("setting")]
        [SerializeField] private float delayTakeDamage;
        
        [Header("VFX References")]
        [SerializeField] private ParticleSystem attackParticle;

        [SerializeField] private ParticleSystem runParticle;
        [SerializeField] private ParticleSystem finisherParticle;
        
        [Header("UI/UX")]
        [SerializeField] private GameObject uiContainer;
        [SerializeField] private GameObject[] heartUI;
        [SerializeField] private GameObject shieldUI;
        [SerializeField] private Sprite heartBreak;
        [SerializeField] private Sprite heartAdd;
        [SerializeField] private Sprite shieldBreak;
        [SerializeField] private Sprite shieldAdd;
        

        [Header("Runtime Stats (Debug)")]
        [SerializeField] private int currentHealth;
        [SerializeField] private bool isBrokenStand;
        [SerializeField] private bool isMoving;

        private CombatManager _combatManager;
       
        
       

        public int CurrentHealth 
        { 
            get => currentHealth; 
            private set => currentHealth = value; 
        }

        protected override void Start()
        {
            base.Start();
            currentHealth = maxHealth;
            isMoving = false;
            isBrokenStand = false;
            _combatManager = GameManager.Instance.combatManager;
        }

        private void Update()
        {
            if (_combatManager.combatState == CombatState.Running)
            { 
                BrokenStand(false);
                Moving(true);
            }
            else
            {
                if(isBrokenStand) BrokenStand(true);
                Moving(false);
                Attack();
            }

            if (_combatManager.combatState == CombatState.Ending) SetActiveUI(false);
            else SetActiveUI(true);
        }

        #region Action


        private void ActiveBrokenStand()
        {
            ResetTrigger();
            isBrokenStand = true;
            BrokenStand(true);
           AudioManager.PlaySound(SoundType.BrokenStand);
        }

        private void DeactiveBrokenStand()
        {
            isBrokenStand = false;
            BrokenStand(false);
        }

        private void Moving(bool isMoving)
        {
            if(this.isMoving == isMoving) return;
            if (isMoving)
            {
                Running();
            }
            else
            {
                Stopping();
            }
            this.isMoving = isMoving;
        }
        public override void Running()
        {
            base.Running();
            ToggleFootStepEffect(true);
        }

        public override void Stopping()
        {
            base.Stopping();
            ToggleFootStepEffect(false);
        }

        public void ReadyToFight()
        {
            Stopping();
           FightStand();
        }

        public void DoParry()
        {
            base.NextAttack();
            PlayEffectSword();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            PlayEffectSword();
        }


        #endregion
    
        #region Sound Effect And Particle

        public void PlayEffectSword()
        {
            if (attackParticle) 
                attackParticle.Play();
            
            AudioManager.PlaySound(SoundType.Parry);

        }
        
        private void ToggleFootStepEffect(bool isPlay)
        {
            
            if(runParticle && isPlay) runParticle.Play();
        }

        public void PlaySoundFootStep()
        {
            AudioManager.PlaySound(SoundType.Footstep);
        }


        #endregion
    
        #region Health
        public void DecreaseHealth()
        {
            if (hasShield)
            {
                OnFocus();
                hasShield = false;
                return;
            }
            TakeDamage();
            CurrentHealth--;
            AudioManager.PlaySound(SoundType.Hurt);
            UpdateHealthUI(false);
            if(currentHealth==1) ActiveBrokenStand();
            if (CurrentHealth <= 0)
            {
                ResetTrigger();
                DeactiveBrokenStand();
                Die();
                GameManager.Instance.EndLevel();
            }
            if(currentState == ActorState.Focusing) OnFocusing();
            
        }
      

        public void AddHealth()
        {
            if (currentHealth < maxHealth)
            {
                UpdateHealthUI(true);
                currentHealth++;
            }
            else SetShield(true);
            DeactiveBrokenStand();
        }

        public void SetShield(bool isShield)
        {
            AudioManager.PlaySound(SoundType.Shield);
            hasShield = isShield;
            UpdateShieldUI(isShield);
        }

        private void UpdateHealthUI(bool isIncrease)
        {
            if(currentHealth <0) return;
            Image heart = heartUI[currentHealth].GetComponent<Image>();
            if (!isIncrease) heart.sprite = heartBreak;
            else heart.sprite = heartAdd;
        }

        private void UpdateShieldUI(bool isIncrease)
        {
            Image shield = shieldUI.GetComponent<Image>();
            if (!isIncrease) shield.sprite = shieldBreak;
            else shield.sprite = shieldAdd;
        }

        public void SetActiveUI(bool isActive)
        {
            uiContainer.SetActive(isActive);
        }

        public void ResetPlayer()
        {
            // 1. Reset dữ liệu số
            currentHealth = maxHealth;
            hasShield = false;

            // 2. Reset toàn bộ UI Tim (Phải dùng vòng lặp để đảm bảo TẤT CẢ đều sáng lại)
            if (heartUI != null)
            {
                foreach (var heartObj in heartUI)
                {
                    if (heartObj != null)
                    {
                        // Gán trực tiếp sprite đầy máu
                        var img = heartObj.GetComponent<Image>();
                        if (img != null) img.sprite = heartAdd; 
                    }
                }
            }

            // 3. Reset UI Khiên (Về trạng thái tắt/vỡ)
            if (shieldUI != null)
            {
                var shieldImg = shieldUI.GetComponent<Image>();
                if (shieldImg != null) shieldImg.sprite = shieldBreak;
            }

            // 4. Reset trạng thái Animation & Logic
            ResetTrigger();         // Xóa các trigger tấn công/bị thương còn tồn đọng
            DeactiveBrokenStand();  // Tắt trạng thái thở dốc
            Idle();                 // Về trạng thái đứng chờ
    
            // 5. Bật lại UI (đề phòng trường hợp game over bị tắt đi)
            SetActiveUI(true);
        }
        #endregion
    
    
  
    }
}
