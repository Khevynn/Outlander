using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class InGamePopupsController : MonoBehaviour
{
    public static InGamePopupsController Instance { get; private set; }
    
    [Header("References")]
    private CamController camController;
    private PlayerController playerController;
    
    [Header("Interaction Popup")] 
    [SerializeField] private GameObject interactionPopup;
    [SerializeField] private TMP_Text interactionPopupTitle;
    [SerializeField] private TMP_Text interactionPopupQuantity;
    [SerializeField] private TMP_Text interactionPopupInteractionText;
    
    [Header("Objects Indicators")]
    [SerializeField] private GameObject miningIconGameObject;
    
    [Header("Fade In - Out")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    
    [Header("Storm Alert")]
    [SerializeField] private GameObject stormAlertGameObject;
    [SerializeField] private TMP_Text stormAlertTimer;
    
    [Header("Damage Alert")]
    [SerializeField] private GameObject damageAlertGameObject;
    
    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("InGamePopupsController already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }
    private void Start()
    {
        camController = Camera.main.transform.parent.GetComponent<CamController>();
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    public void ShowItemInfo(Item item)
    {
        interactionPopupTitle.text = item.GetItemData().Name;
        interactionPopupQuantity.text = $"x{item.GetAmountOfItems().ToString()}";
        interactionPopupInteractionText.text = "Collect";
        interactionPopup.SetActive(true);
    }
    public void HideItemInfoPopup()
    {
        interactionPopup.SetActive(false);
    }
    public void ShowMiningIcon()
    {
        miningIconGameObject.SetActive(true);
    }
    public void HideMiningIcon()
    {
        miningIconGameObject.SetActive(false);
    }

    public void ShowStormAlert()
    {
        stormAlertGameObject.SetActive(true);
        StartCoroutine(StormAlertFadeIn());
    }
    public void SetStormAlertTimer(float time)
    {
        stormAlertTimer.text = $"Time to skip it: {time:F1}s";
    }
    public void HideStormAlert()
    {
        StartCoroutine(StormAlertFadeOut());
        stormAlertGameObject.SetActive(false);
    }

    public void CallDamageAlert(float duration)
    {
        StartCoroutine(DamageAlert(duration));
    }
    public void CallFadeIn(float duration, bool fadeOut)
    {
        StartCoroutine(FadeIn(duration, fadeOut));
    }

    private IEnumerator StormAlertFadeIn()
    {
        var canvasGroup = stormAlertGameObject.GetComponent<CanvasGroup>();
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.fixedDeltaTime / 5;
            yield return null;
        }
    }
    private IEnumerator StormAlertFadeOut()
    {
        var canvasGroup = stormAlertGameObject.GetComponent<CanvasGroup>();
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.fixedDeltaTime / 5;
            yield return null;
        }
    }

    private IEnumerator DamageAlert(float duration)
    {
        damageAlertGameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        damageAlertGameObject.SetActive(false);
    }
    
    private IEnumerator FadeIn(float duration, bool fadeOut)
    {
        float time = 0f;
        playerController.DisableAllActions();
        while (time < duration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        camController.ResetCam();
        
        if(fadeOut)
            StartCoroutine(FadeOut(duration));
    }
    private IEnumerator FadeOut(float duration)
    {
        float time = 0f;
        playerController.EnableAllActions();
        while (time < duration)
        {   
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
    }
}