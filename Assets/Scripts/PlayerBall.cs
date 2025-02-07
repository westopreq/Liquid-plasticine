using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float maxProjectileSize = 2f;
    public float shootSpeed = 10f;
    public float moveSpeed = 5f;
    public float minBallSize = 0.1f;
    public LineRenderer lineRenderer;
    public Transform door;
    public Transform ground;
    public float lineHeightOffset = 0.2f;
    public float playerShrinkRate = 0.1f;
    public float sphereRadius = 1f;
    public Material projectileMaterial;

    private float currentSize;
    private bool isShooting;
    private GameObject projectile;
    private bool isProjectileInitialized;
    private float projectileGrowthFactor;

    private float initialPlayerSize;
    private float initialProjectileSize;

    void Start()
    {
        currentSize = transform.localScale.x;
        initialPlayerSize = currentSize;
        initialProjectileSize = 0.2f;
    }

    void Update()
    {
        // Удалено перемещение на WASD

        if (Input.GetMouseButton(0))
        {
            if (currentSize <= minBallSize)
            {
                Debug.Log("Проигрыш: куля-игрок слишком мала!");
                return;
            }

            isShooting = true;
            currentSize = Mathf.Max(currentSize - playerShrinkRate * Time.deltaTime, minBallSize);
            transform.localScale = new Vector3(currentSize, currentSize, currentSize);

            if (!isProjectileInitialized)
            {
                Vector3 directionToDoor = (door.position - transform.position).normalized;
                Vector3 spawnPosition = transform.position + directionToDoor * sphereRadius;

                projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                projectile.transform.localScale = new Vector3(initialProjectileSize, initialProjectileSize, initialProjectileSize);
                projectileGrowthFactor = 0f;

                Projectile projectileScript = projectile.GetComponent<Projectile>();
                if (projectileScript != null)
                {
                    Vector3 doorPosition = new Vector3(door.position.x, transform.position.y, door.position.z);
                    Vector3 directionToDoorForProjectile = (doorPosition - spawnPosition).normalized;
                    projectileScript.SetDirection(directionToDoorForProjectile);
                    projectileScript.projectileMaterial = projectileMaterial;
                }

                isProjectileInitialized = true;
            }
            else
            {
                UpdateProjectileSize();
            }
        }
        else if (Input.GetMouseButtonUp(0) && isShooting)
        {
            if (projectile != null)
            {
                UpdateProjectileSize();
                Projectile projectileScript = projectile.GetComponent<Projectile>();
                if (projectileScript != null)
                {
                    // Используем размер снаряда для расчета радиуса взрыва
                    projectileScript.explosionRadius = projectile.transform.localScale.x;
                    projectileScript.StartMoving();
                }
            }
            isProjectileInitialized = false;
            isShooting = false;
        }

        UpdateLineRenderer();
    }

    void UpdateProjectileSize()
    {
        if (projectile != null)
        {
            projectileGrowthFactor += Time.deltaTime * 2f;
            float shrinkPercentage = (initialPlayerSize - currentSize) / (initialPlayerSize - minBallSize);
            float newProjectileSize = Mathf.Lerp(initialProjectileSize, maxProjectileSize, shrinkPercentage * projectileGrowthFactor);
            projectile.transform.localScale = new Vector3(newProjectileSize, newProjectileSize, newProjectileSize);
        }
    }

    void UpdateLineRenderer()
    {
        float groundHeight = ground.position.y + ground.localScale.y;
        float lineHeight = groundHeight + lineHeightOffset;

        Vector3 playerPosition = new Vector3(transform.position.x, lineHeight, transform.position.z);
        Vector3 doorPosition = new Vector3(door.position.x, lineHeight, door.position.z);

        lineRenderer.SetPosition(0, playerPosition);
        lineRenderer.SetPosition(1, doorPosition);

        float lineWidth = Mathf.Lerp(0.1f, 1f, currentSize / maxProjectileSize);
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }
}
