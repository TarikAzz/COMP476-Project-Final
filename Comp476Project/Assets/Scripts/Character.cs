using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Character : NetworkBehaviour
{
    [Header("Health")]
    public Image HealthBar;
    public float MaxHealth;

    [SyncVar(hook = "UpdateHealth")]
    private float _currentHealth;

    void Start()
    {
        _currentHealth = MaxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        if (!isServer)
        {
            return;
        }

        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            _currentHealth = MaxHealth;
            Die();
        }
    }
    
    private void UpdateHealth(float currentHealth)
    {
        HealthBar.fillAmount = currentHealth / MaxHealth;
    }

    protected void Colorize(Color color)
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();

        for (var i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = color;
        }
    }

    public virtual void Die() { }
}
