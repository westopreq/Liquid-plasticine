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

    // Публичная переменная для множителя радиуса заражения, редактируемая в инспекторе
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
        if (isExploding)
        {
            // Не обрабатываем столкновения, если уже идет взрыв
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Снаряд столкнулся с врагом: " + other.name);

            if (hasCollided)
            {
                Debug.Log("Снаряд уже столкнулся с врагом и не должен двигаться.");
                return; // Прерываем дальнейшую обработку, если уже было столкновение
            }

            hasCollided = true;
            StartInfection(other.gameObject);

            // Вычисляем радиус заражения с учетом множителя
            float infectionRadius = transform.localScale.x * infectionRadiusMultiplier;

            // Ищем все врагов в радиусе заражения
            Collider[] enemiesInRadius = Physics.OverlapSphere(transform.position, infectionRadius);

            // Заражаем всех врагов в радиусе
            foreach (Collider col in enemiesInRadius)
            {
                if (col.CompareTag("Enemy"))
                {
                    StartInfection(col.gameObject);
                }
            }

            Renderer enemyRenderer = other.GetComponent<Renderer>();
            if (enemyRenderer != null)
            {
                Vector3 enemySize = enemyRenderer.bounds.size;
                float stopDistance = (enemySize.x + enemySize.y) / 2f;
                stopPosition = transform.position + direction * stopDistance;
                Debug.Log("Рассчитано расстояние остановки: " + stopDistance);
            }

            // Запускаем взрыв после первого столкновения
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

        infection.StartInfection(projectileMaterial);
        CreateImpactEffect(enemy.transform.position);
    }

    private IEnumerator Explode()
    {
        if (isExploding)
        {
            Debug.Log("Снаряд уже взрывается, повторный вызов запрещен.");
            yield break; // Если уже взрывается, не выполняем повторно
        }

        isExploding = true;
        Debug.Log("Процесс взрыва начался.");

        // Включение эффектов
        CreateImpactEffect(transform.position);

        // Отключение рендера и коллайдера
        if (TryGetComponent(out Renderer renderer)) renderer.enabled = false;
        if (TryGetComponent(out Collider collider)) collider.enabled = false;

        // Выводим сообщение о том, что эффект взрыва длится
        if (impactEffect != null)
        {
            float effectDuration = impactEffect.main.duration;
            Debug.Log("Время длительности эффекта: " + effectDuration);
            yield return new WaitForSeconds(effectDuration);
        }

        Debug.Log("Снаряд уничтожен.");
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
