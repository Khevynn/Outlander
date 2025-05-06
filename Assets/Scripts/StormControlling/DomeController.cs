using System;
using UnityEngine;

public class DomeController : MonoBehaviour
{
    [Header("Damage Control")]
    [SerializeField] private float timeBetweenDamage;
    [SerializeField] private float damageAmount;
    private float currentTimer;
    
    private bool isPlayerOutside;
    private HealthComponent playerHealthComponent;

    private void Start()
    {
        playerHealthComponent = GameObject.FindWithTag("Player").GetComponent<HealthComponent>();
        currentTimer = timeBetweenDamage;
    }

    private void FixedUpdate()
    {
        if (currentTimer > 0f)
            currentTimer -= Time.fixedDeltaTime;

        if (isPlayerOutside && currentTimer <= 0)
            DealDamageToPlayer();
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
