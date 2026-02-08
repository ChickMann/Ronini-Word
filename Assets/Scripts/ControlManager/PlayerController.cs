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
            PlayerEffectSword();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            PlayerEffectSword();
        }


        #endregion
    
        #region Sound Effect And Particle

        public void PlayerEffectSword()
        {
            if (attackParticle) 
                attackParticle.Play();
          
            GameManager.Instance.PlayerSwordEffect();

        }
        
        private void ToggleFootStepEffect(bool isPlay)
        {
            GameManager.Instance.PlayerFootStepEffect(isPlay);
            if(runParticle && isPlay) runParticle.Play();
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
            hasShield = isShield;
            UpdateShieldUI(isShield);
        }

        private void UpdateHealthUI(bool isIncrease)
        {
            if(currentHealth <0) return;
            Debug.Log(heartUI[currentHealth]);
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
        [ContextMenu( "Add Health Test")]
        public void AddHealthTest()
        {
            AddHealth();
        }
        [ContextMenu( "Add Shield Test")]
        public void AddShieldTest()
        {
           SetShield(true);
        }
        [ContextMenu( "remove Shield Test")]
        public void RemoveShieldTest()
        {
            SetShield(false);
        }
        #endregion
    
    
  
    }
}
