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

    private void Start()
    {
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
        }
    }

    public void StartInfection(Material newMaterial)
    {
        if (!isInfected)
        {
            infectionMaterial = new Material(newMaterial); // Создаём копию материала
            isInfected = true;
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

        Destroy(gameObject); // Уничтожаем врага после полного заражения
    }
}
