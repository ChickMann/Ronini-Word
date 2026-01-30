using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

namespace ControlManager
{
    public class PlayerController : Actor
    {
        [Header("Player Stats")]
        [SerializeField] private int maxHealth=3;
        [SerializeField] private int maxLife = 2;

        [Header("setting")]
        [SerializeField] private float delayTakeDamage;
        
        [Header("VFX References")]
        [SerializeField] private ParticleSystem attackParticle;

        [SerializeField] private ParticleSystem runParticle;
        [SerializeField] private ParticleSystem finisherParticle;

        [Header("Runtime Stats (Debug)")]
        [SerializeField] private int currentHealth;
        [SerializeField] private int currentLife;
        
        private CinemachineImpulseSource _myImpulse;

        public int CurrentHealth 
        { 
            get => currentHealth; 
            private set => currentHealth = value; 
        }
    
        public int CurrentLife 
        { 
            get => currentLife; 
            private set => currentLife = value; 
        }
        
        
    
        protected override void Start()
        {
            _myImpulse = GetComponent<CinemachineImpulseSource>();
            base.Start();
            ResetLife();
            ResetHealth();
        }

        private void OnEnable()
        {
            GameEvents.OnLevelStart += OnLevelStartHandler;

        }

        private void OnDisable()
        {
            GameEvents.OnLevelStart -= OnLevelStartHandler;
        
        }

        private void Update()
        {
            Attack();
        }

        private void OnLevelStartHandler(LevelData data)
        {
            StartLevel();
        }

        public void StartLevel()
        {
            Running();
        }

        #region Action
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

        public void ReadyToFight(float timeDelayReady)
        {
            Stopping();
            this.DelayAction(timeDelayReady, () => FightStand());
        }

        public void DoParry()
        {
            base.NextAttack();
            PlayerEffectSword();
        }
        public void DoFinisher()
        {

            base.OnFinisher();
            if(finisherParticle) finisherParticle.Play();
        }


    
        #endregion
    
        #region Sound Effect And Particle

        public void PlayerEffectSword()
        {
            if (attackParticle) 
                attackParticle.Play();
          
            PlaySwordSound();
        }
        private void PlaySwordSound()
        {
            GameManager.Instance.PlayerSwordEffect();
        }

        private void ToggleFootStepEffect(bool isPlay)
        {
            GameManager.Instance.PlayerFootStepEffect(isPlay);
            if(runParticle && isPlay) runParticle.Play();
        }


        #endregion
    
        #region Health and life
        public void DecreaseHealth()
        {
            LockAttack(delayTakeDamage);
            this.DelayAction( delayTakeDamage, TakeDamage);
            CurrentHealth--;
        
            if (CurrentHealth <= 0)
            {
              
                DecreaseLife();
                ResetHealth();
            
                GameEvents.OnPlayerBroken?.Invoke(true);
            }
        }
        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
        }

        public void ResetLife()
        {
            CurrentLife = maxLife;
            ResetHealth(); 
        }

        public void DecreaseLife()
        {
            CurrentLife--;
            if (CurrentLife <= 0)  this.DelayAction( delayTakeDamage,()=>
            {
                CancelNextAttack();
                Die();
                GameEvents.OnEndGame?.Invoke();
            });
      
        }

        public void LockAttack(float time)
        {
            CancelNextAttack();
            if (CurrentLife <= 0)  this.DelayAction( time+0.1f,()=>
            {
                if(currentState == ActorState.BrokenStand || currentState == ActorState.Dead) return;
                NextAttack();
            });
        }
        
        public void DoCinematicShake()
        {
            if (_myImpulse != null)
            {
                _myImpulse.GenerateImpulse(Vector3.one * 0.1f);
            }
        }
        #endregion
    
    
  
    }
}
