using UnityEngine;
using System.Collections;

public class EnemyInfection : MonoBehaviour
{
    public Material infectionMaterial; // Материал заражения
    private Material originalMaterial;
    private Renderer enemyRenderer;
    
    private bool isInfected = false;
    private float infectionProgress = 0f;
    private float infectionSpeed = 1f; // Скорость заражения
    
    public ParticleSystem deathEffect; // Эффект смерти

    private void Start()
    {
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
        }
    }

    public void StartInfection(Material newMaterial, ParticleSystem effectPrefab)
    {
        if (!isInfected)
        {
            infectionMaterial = new Material(newMaterial); // Создаём копию материала
            isInfected = true;
            deathEffect = effectPrefab; // Сохраняем эффект
            StartCoroutine(Infect());
        }
    }

    private IEnumerator Infect()
    {
        while (infectionProgress < 1f)
        {
            infectionProgress += Time.deltaTime * infectionSpeed;
            enemyRenderer.material.Lerp(originalMaterial, infectionMaterial, infectionProgress);
            yield return null;
        }

        TriggerDeathEffect();
    }

    private void TriggerDeathEffect()
    {
        if (deathEffect != null)
        {
            ParticleSystem effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        Destroy(gameObject);
    }
}
