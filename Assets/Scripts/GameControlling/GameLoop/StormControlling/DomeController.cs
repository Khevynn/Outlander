using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VolumetricFogAndMist2;

public class DomeController : MonoBehaviour
{
    [Header("References")]
    private HealthComponent playerHealthComponent;
    private FogVoid fogVoidComponent;
    private MeshRenderer domeRenderer;

    [Header("UI References")] 
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text percentageText;
    
    [Header("Time Limit Control")] 
    [SerializeField] private float maxDurationOfDome;
    private float currentDurationOfDome;
    
    [Header("Damage Control")]
    [SerializeField] private float timeBetweenDamage;
    [SerializeField] private float damageAmount;
    private float currentTimer;
    
    private bool isDomeActive;
    private bool isPlayerOutside;
    
    private void Start()
    {
        fogVoidComponent = GetComponent<FogVoid>();
        domeRenderer = GetComponent<MeshRenderer>();
        playerHealthComponent = GameObject.FindWithTag("Player").GetComponent<HealthComponent>();
        currentDurationOfDome = maxDurationOfDome;
        currentTimer = timeBetweenDamage;
        isDomeActive = true;
    }

    private void FixedUpdate()
    {
        // Update dome duration and handle dome deactivation
        if (currentDurationOfDome > 0f)
        {
            currentDurationOfDome -= Time.fixedDeltaTime;
            fillImage.fillAmount = currentDurationOfDome / maxDurationOfDome;
            percentageText.text = $"{((currentDurationOfDome / maxDurationOfDome) * 100f):F0}%";
            return;
        }

        if (isDomeActive)
        {
            DeactivateDome();
            return;
        }

        // Handle dome fading and fog falloff
        if (!isDomeActive)
        {
            fogVoidComponent.falloff = Mathf.Min(1f, fogVoidComponent.falloff + Time.fixedDeltaTime / 3f);

            if (fogVoidComponent.falloff >= 1f)
                isPlayerOutside = true;

            Material mat = domeRenderer.material;

            float domeTransparency = Mathf.Clamp01(mat.GetFloat("_OutlineAlpha") - Time.fixedDeltaTime / 3f);
            float domeMainTransparency = Mathf.Clamp01(mat.GetFloat("_MainAlpha") - Time.fixedDeltaTime / 3f);

            mat.SetFloat("_OutlineAlpha", domeTransparency);
            mat.SetFloat("_MainAlpha", domeMainTransparency);
        }

        // Timer and damage handling
        if (currentTimer > 0f)
        {
            currentTimer -= Time.fixedDeltaTime;
        }

        if (isPlayerOutside && currentTimer <= 0f)
            DealDamageToPlayer();
    }

    private void DeactivateDome()
    {
        isDomeActive = false;
    }

    private void DealDamageToPlayer()
    {
        playerHealthComponent.TakeDamage(damageAmount);
        currentTimer = timeBetweenDamage;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        isPlayerOutside = false;
    }
    private void OnTriggerExit(Collider other)
    {
        isPlayerOutside = true;
    }
}
