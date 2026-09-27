using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Infra2DAction
{
    public class HUDSystem : MonoBehaviour
    {
        [Header("Life Bar")]
        [SerializeField] private Image _lifeFillImage;
        [SerializeField] private Animator _lifeAnimator;
        [SerializeField] private float _normalLifeAnimSpeed = 1f;
        [SerializeField] private float _damagedLifeAnimSpeed = 2.5f;
        [SerializeField] private float _damagedSpeedDuration = 1.5f;

        [Header("Resource Bar")]
        [SerializeField] private Image _resourceFillImage;
        [SerializeField] private TMP_Text _resourceText;

        [Header("Currency")]
        [SerializeField] private TMP_Text _currencyText;

        private float _lifeAnimTimer;

        public void Initialize()
        {
            if (_lifeAnimator != null)
                _lifeAnimator.speed = _normalLifeAnimSpeed;

            DebugSystem.Log("HUDSystem initialized.", "UI", "HUDSystem");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Subscribe<ResourceChangedEvent>(OnResourceChanged);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Unsubscribe<ResourceChangedEvent>(OnResourceChanged);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
        }

        private void Update()
        {
            if (_lifeAnimTimer <= 0f) return;

            _lifeAnimTimer -= Time.deltaTime;
            if (_lifeAnimTimer <= 0f && _lifeAnimator != null)
                _lifeAnimator.speed = _normalLifeAnimSpeed;
        }

        private void OnHealthChanged(PlayerHealthChangedEvent e)
        {
            if (_lifeFillImage != null && e.MaxHP > 0)
                _lifeFillImage.fillAmount = (float)e.CurrentHP / e.MaxHP;
        }

        private void OnPlayerDamaged(PlayerDamagedEvent e)
        {
            if (_lifeAnimator == null) return;

            _lifeAnimator.speed = _damagedLifeAnimSpeed;
            _lifeAnimTimer = _damagedSpeedDuration;
        }

        private void OnResourceChanged(ResourceChangedEvent e)
        {
            if (_resourceFillImage != null && e.MaxResource > 0f)
                _resourceFillImage.fillAmount = e.CurrentResource / e.MaxResource;

            if (_resourceText != null)
                _resourceText.text = $"{Mathf.RoundToInt(e.CurrentResource)}/{Mathf.RoundToInt(e.MaxResource)}";
        }

        private void OnCurrencyChanged(CurrencyChangedEvent e)
        {
            if (_currencyText != null)
                _currencyText.text = e.CurrentAmount.ToString();
        }
    }
}
