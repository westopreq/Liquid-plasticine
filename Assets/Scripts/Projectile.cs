using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public Material projectileMaterial;
    private Vector3 direction;
    private bool isMoving = false;
    private bool hasCollided = false;
    private bool isExploding = false;
    private Vector3 stopPosition;

    public float explosionRadius;
    public ParticleSystem impactEffect;
    public ParticleSystem enemyDeathEffect; // Новый эффект смерти врага

    public float infectionRadiusMultiplier = 1.5f;

    public delegate void ProjectileDestroyed();
    public event ProjectileDestroyed OnProjectileDestroyed;

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;
    }

    public void SetMaterial(Material material)
    {
        projectileMaterial = material;
    }

    public void StartMoving()
    {
        isMoving = true;
        Debug.Log("Снаряд начал движение.");
    }

    private void Update()
    {
        if (isMoving && !isExploding)
        {
            transform.position += direction * speed * Time.deltaTime;

            if (hasCollided && Vector3.Distance(transform.position, stopPosition) < 0.1f)
            {
                Debug.Log("Снаряд достиг цели, начало взрыва.");
                StartCoroutine(Explode());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isExploding) return; // Игнорируем если уже взрывается

        if (other.CompareTag("Enemy"))
        {
            if (hasCollided) return; // Уже обработано

            hasCollided = true;
            StartInfection(other.gameObject);

            float infectionRadius = transform.localScale.x * infectionRadiusMultiplier;
            Collider[] enemiesInRadius = Physics.OverlapSphere(transform.position, infectionRadius);

            foreach (Collider col in enemiesInRadius)
            {
                if (col.CompareTag("Enemy"))
                {
                    StartInfection(col.gameObject);
                }
            }

            if (other.TryGetComponent(out Renderer enemyRenderer))
            {
                Vector3 enemySize = enemyRenderer.bounds.size;
                float stopDistance = (enemySize.x + enemySize.y) / 2f;
                stopPosition = transform.position + direction * stopDistance;
            }

            StartCoroutine(Explode());
        }
    }

    private void StartInfection(GameObject enemy)
    {
        EnemyInfection infection = enemy.GetComponent<EnemyInfection>();
        if (infection == null)
        {
            infection = enemy.AddComponent<EnemyInfection>();
        }

        infection.StartInfection(projectileMaterial, enemyDeathEffect);
    }

    private IEnumerator Explode()
    {
        if (isExploding) yield break;

        isExploding = true;

        CreateImpactEffect(transform.position);

        if (TryGetComponent(out Renderer renderer)) renderer.enabled = false;
        if (TryGetComponent(out Collider collider)) collider.enabled = false;

        if (impactEffect != null)
        {
            float effectDuration = impactEffect.main.duration;
            yield return new WaitForSeconds(effectDuration);
        }

        Destroy(gameObject);
    }

    private void CreateImpactEffect(Vector3 position)
    {
        if (impactEffect != null)
        {
            ParticleSystem effect = Instantiate(impactEffect, position, Quaternion.identity);
            var main = effect.main;
            main.startColor = projectileMaterial.color;
            effect.Play();
            Destroy(effect.gameObject, main.duration);
        }
    }
}
