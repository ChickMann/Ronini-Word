using UnityEngine;
using UnityEngine.UI;

namespace ControlManager
{
    public class EnemyController : Actor
    {
        [Header("Enemy Config")]
        public EnemyProfile enemyData;
    
        [Header("Fuel Gause Settings")]
        [SerializeField] private bool isFuelGausePause;
        [SerializeField] private float fuelGauseFirstChar;


        [Header("UI References")]
        [SerializeField] private Slider fuelGaugeSlider;
        
        [Header("Debug")]
        [SerializeField] private float currentFuelGauge;
   
        // State
        public bool hasPlayerDetected { get; private set; }

        // Components & Cache
        public Rigidbody2D rb2d;
        private BackGroundManager _backgroundManager;
        private CombatManager _combatManager;
        

        protected override void Awake()
        {
            base.Awake();
            rb2d = GetComponent<Rigidbody2D>();
            _combatManager = GameManager.Instance.combatManager;
        }

        protected override void Start()
        {
            base.Start();
            currentFuelGauge = enemyData.FuelGauge + fuelGauseFirstChar;
            if (fuelGaugeSlider)
            {
                fuelGaugeSlider.maxValue = enemyData.FuelGauge;
                fuelGaugeSlider.value = currentFuelGauge;
            }

            SetActiveFuelSlider(false);

            if (GameManager.Instance != null)
            {
                _backgroundManager = GameManager.Instance.backGroundManager;
            }
        }
        

        private void Update()
        {
            if (!isFuelGausePause) FuelGaugeDercease();
            if (_combatManager.combatState == CombatState.Running)
            {
                Moving(true);
            }
            else
            {
                Moving(false);
                 Attack();
            }
        }
        public void SetHasPlayerDetected(bool hasPlayerDetected) => this.hasPlayerDetected = hasPlayerDetected;
     

        public void SetupEnemyData(EnemyProfile data)
        {
            enemyData = data;
            if (_backgroundManager) _backgroundManager.SetRunState(false);

        }

        private void Moving(bool isMoving,float speed =0)
        {
            if (isMoving)
            {
                if (!enemyData) return;
                speed = speed==0? 3f: speed;
                rb2d.linearVelocity = new Vector2(speed* -1f, rb2d.linearVelocity.y);
                if (_backgroundManager)  _backgroundManager.SetRunState(true);
            }
            else
            {
                if (_backgroundManager) _backgroundManager.SetRunState(false);
            }
        }

        #region Fuel_Gauge
        // ReSharper disable Unity.PerformanceAnalysis
        private void FuelGaugeDercease()
        {
            currentFuelGauge -= Time.deltaTime;
            
            UpdateFuelSlider(currentFuelGauge);
            if (currentFuelGauge <= 0)
            {
               GameEvents.OnCharWrong?.Invoke();
                currentFuelGauge = enemyData.FuelGauge;
            }
        }

        public void SetFuelGauge(float multi)
        {
            float a = enemyData.FuelGauge * multi;
            UpdateFuelSlider(a,a);
        }
        

        public void SetActiveFuelSlider(bool isActive)
        {
            isFuelGausePause = !isActive;
            if (fuelGaugeSlider) fuelGaugeSlider.gameObject.SetActive(isActive);
            ResetFuelGauge();
        }

        private void UpdateFuelSlider(float value, float max = 0)
        {
            if (fuelGaugeSlider)
            {
                fuelGaugeSlider.maxValue = max==0? fuelGaugeSlider.maxValue: max;
                fuelGaugeSlider.value = value;
            }
        }
     

        public void ResetFuelGauge()
        {
            fuelGaugeSlider.maxValue = enemyData.FuelGauge;
            currentFuelGauge = enemyData.FuelGauge;
        }

        #endregion
        
       
    }
}