using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ControlManager
{
    public class EnemyController : Actor
    {
        [Header("Enemy Config")]
        public EnemyProfile enemyData;
        public int health;
        public float speed;
    
        [Header("Fuel Gause Settings")]
        [SerializeField] private bool isFuelGausePause;
        [SerializeField] private float fuelGauseFirstChar;
        
        [Header("UI References")]
        [SerializeField] private Slider fuelGaugeSlider;
        [SerializeField] private GameObject healthContainer;
        
        [Header("Prefab References")]
        [SerializeField] private GameObject healthPrefab;
        [SerializeField] private Sprite heartBreak;
        
        [Header("Debug")]
        [SerializeField] private float currentFuelGauge;
        [SerializeField] private int currentIndexHealth;
   
        // State
        public bool hasPlayerDetected { get; private set; }

        // Components & Cache
        private Rigidbody2D rb2d;
        private BackGroundManager _backgroundManager;
        private CombatManager _combatManager;
        private List<GameObject> heathList;

        protected override void Awake()
        {
            base.Awake();
            rb2d = GetComponent<Rigidbody2D>();
            _combatManager = GameManager.Instance.combatManager;
            heathList = new List<GameObject>();
            currentIndexHealth = 0;
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
     

        public void SetupEnemyData(EnemyProfile data,int countVocab)
        {
            enemyData = data;
            if (_backgroundManager) _backgroundManager.SetRunState(false);
            health = countVocab;
            InstantiateHealth();
        }

        private void Moving(bool isMoving,float speed =0)
        {
            if (isMoving)
            {
                if (!enemyData) return;
                this.speed = speed==0? this.speed: speed;
                rb2d.linearVelocity = new Vector2(this.speed* -1f, rb2d.linearVelocity.y);
                if (_backgroundManager)  _backgroundManager.SetRunState(true);
            }
            else
            {
                if (_backgroundManager) _backgroundManager.SetRunState(false);
            }
            
        }

        private void InstantiateHealth()
        {
            for (int i = 0; i < health; i++)
            {
               GameObject a = Instantiate(healthPrefab, healthContainer.transform);
               heathList.Add(a);
            }
        }

        public void HealthDecrease()
        {
           if(currentIndexHealth> health) return;
            heathList[currentIndexHealth].GetComponent<Image>().sprite = heartBreak;
            currentIndexHealth++;
        }

        public void SetActiveHealth(bool isActive)
        {
            healthContainer.SetActive(isActive);
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