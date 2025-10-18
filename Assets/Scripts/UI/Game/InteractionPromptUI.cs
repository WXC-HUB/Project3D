using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Interaction;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 交互提示UI
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private Text promptText;
        [SerializeField] private Image keyImage;
        [SerializeField] private Sprite eKeySprite;
        
        [Header("动画")]
        [SerializeField] private float fadeSpeed = 5f;
        
        [Header("目标")]
        [SerializeField] private InteractionSystem interactionSystem;
        
        private CanvasGroup canvasGroup;
        private bool isShowing = false;
        
        private void Awake()
        {
            canvasGroup = promptPanel?.GetComponent<CanvasGroup>();
            if (canvasGroup == null && promptPanel != null)
            {
                canvasGroup = promptPanel.AddComponent<CanvasGroup>();
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }
        
        private void Start()
        {
            if (interactionSystem != null)
            {
                interactionSystem.OnTargetChanged += OnTargetChanged;
            }
            
            if (promptPanel != null)
            {
                promptPanel.SetActive(false);
            }
        }
        
        private void Update()
        {
            // 淡入淡出动画
            if (canvasGroup != null)
            {
                float targetAlpha = isShowing ? 1f : 0f;
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
                
                // 完全隐藏时禁用面板
                if (canvasGroup.alpha < 0.01f && !isShowing)
                {
                    if (promptPanel != null)
                    {
                        promptPanel.SetActive(false);
                    }
                }
            }
        }
        
        private void OnTargetChanged(object sender, InteractionSystem.InteractionEventArgs e)
        {
            if (e.target != null && e.isAvailable)
            {
                ShowPrompt(e.target.GetInteractionPrompt());
            }
            else
            {
                HidePrompt();
            }
        }
        
        private void ShowPrompt(string text)
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(true);
            }
            
            if (promptText != null)
            {
                promptText.text = text;
            }
            
            if (keyImage != null && eKeySprite != null)
            {
                keyImage.sprite = eKeySprite;
            }
            
            isShowing = true;
        }
        
        private void HidePrompt()
        {
            isShowing = false;
        }
        
        /// <summary>
        /// 设置目标交互系统
        /// </summary>
        public void SetTarget(InteractionSystem system)
        {
            if (interactionSystem != null)
            {
                interactionSystem.OnTargetChanged -= OnTargetChanged;
            }
            
            interactionSystem = system;
            
            if (interactionSystem != null)
            {
                interactionSystem.OnTargetChanged += OnTargetChanged;
            }
        }
        
        private void OnDestroy()
        {
            if (interactionSystem != null)
            {
                interactionSystem.OnTargetChanged -= OnTargetChanged;
            }
        }
    }
}

