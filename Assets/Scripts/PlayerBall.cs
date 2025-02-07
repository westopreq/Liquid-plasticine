using UnityEngine;
using System.Collections;

public class PlayerBall : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float maxProjectileSize = 2f;
    public float shootSpeed = 3f;
    public float minBallSize = 0.1f;
    public LineRenderer lineRenderer;
    public Transform door;
    public Transform ground;
    public float lineHeightOffset = 0.2f;
    public float playerShrinkRate = 0.1f;
    public float sphereRadius;
    public Material projectileMaterial;
    public float stopDistance = 1f;

    public float jumpStep = 2f;
    public float jumpHeight = 1f;
    public float jumpDuration = 0.5f;

    public ParticleSystem deathEffect;

    private float currentSize;
    private bool isShooting;
    private GameObject projectile;
    private bool isProjectileInitialized;
    private float initialProjectileSize = 0.2f;
    private bool isMoving = false;
    private bool isJumping = false;

    void Start()
    {
        currentSize = transform.localScale.x;
        maxProjectileSize = currentSize;
        sphereRadius = currentSize / 2; 
        UpdateHeight();
        UpdateLineRenderer();
    }

    void Update()
    {
        if (Input.touchCount > 0) 
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began) // Начало касания
            {
                isShooting = true;
            }

            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) // Удержание касания
            {
                if (currentSize <= minBallSize)
                {
                    Debug.Log("Проигрыш: игрок слишком мал!");
                    LoseGame();
                    return;
                }

                currentSize = Mathf.Max(currentSize - playerShrinkRate * Time.deltaTime, minBallSize);
                transform.localScale = new Vector3(currentSize, currentSize, currentSize);
                sphereRadius = currentSize / 2;
                UpdateHeight();

                if (!isProjectileInitialized)
                {
                    Vector3 spawnPosition = transform.position + (door.position - transform.position).normalized * sphereRadius;
                    projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                    projectile.transform.localScale = new Vector3(initialProjectileSize, initialProjectileSize, initialProjectileSize);

                    Projectile projectileScript = projectile.GetComponent<Projectile>();
                    if (projectileScript != null)
                    {
                        Vector3 directionToDoor = (door.position - spawnPosition).normalized;
                        projectileScript.SetDirection(directionToDoor);
                        projectileScript.projectileMaterial = projectileMaterial;
                    }
                    isProjectileInitialized = true;
                }
                else
                {
                    UpdateProjectileSize();
                }
                UpdateLineRenderer();
            }

            if (touch.phase == TouchPhase.Ended && isShooting) // Отпускание пальца
            {
                if (projectile != null)
                {
                    UpdateProjectileSize();
                    Projectile projectileScript = projectile.GetComponent<Projectile>();
                    if (projectileScript != null)
                    {
                        projectileScript.explosionRadius = projectile.transform.localScale.x;
                        projectileScript.StartMoving();
                    }
                }
                isProjectileInitialized = false;
                isShooting = false;
                maxProjectileSize = currentSize;
                sphereRadius = currentSize / 2;

                if (IsPathClear())
                {
                    isMoving = true;
                    StartCoroutine(JumpTowardsDoor());
                }
            }
        }

        UpdateLineRenderer();
    }

    bool IsPathClear()
    {
        Vector3 direction = (door.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, door.position);

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, currentSize / 2, direction, distance);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                return false;
            }
        }
        return true;
    }

    IEnumerator JumpTowardsDoor()
    {
        isJumping = true;

        while (isMoving)
        {
            Vector3 direction = (door.position - transform.position).normalized;
            float distanceToDoor = Vector3.Distance(transform.position, door.position);

            if (distanceToDoor <= (currentSize / 2) + stopDistance)
            {
                isMoving = false;
                isJumping = false;
                yield break;
            }

            Vector3 jumpTarget = transform.position + direction * jumpStep;

            float elapsedTime = 0f;
            Vector3 startPos = transform.position;
            Vector3 peakPos = startPos + new Vector3(0, jumpHeight, 0);
            Vector3 endPos = jumpTarget;

            while (elapsedTime < jumpDuration / 2)
            {
                transform.position = Vector3.Lerp(startPos, peakPos, (elapsedTime / (jumpDuration / 2)));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            elapsedTime = 0f;

            while (elapsedTime < jumpDuration / 2)
            {
                transform.position = Vector3.Lerp(peakPos, endPos, (elapsedTime / (jumpDuration / 2)));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            UpdateHeight();
        }

        isJumping = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            Debug.Log("Игрок коснулся двери!");
            isMoving = false;
            isJumping = false;
            StopAllCoroutines();
            TriggerDeathEffect();
        }
    }

    void LoseGame()
    {
        Debug.Log("Игрок проиграл!");

        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject); // Удаляем линию
        }

        TriggerDeathEffect();
    }

    void TriggerDeathEffect()
    {
        Debug.Log("Игрок исчезает...");

        if (deathEffect != null)
        {
            ParticleSystem effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        Destroy(gameObject, 0.5f);
    }

    void UpdateProjectileSize()
    {
        if (projectile != null)
        {
            float newProjectileSize = projectile.transform.localScale.x + playerShrinkRate * Time.deltaTime;
            newProjectileSize = Mathf.Min(newProjectileSize, maxProjectileSize);
            projectile.transform.localScale = new Vector3(newProjectileSize, newProjectileSize, newProjectileSize);
        }
    }

    void UpdateHeight()
    {
        float groundTop = ground.position.y + (ground.localScale.y / 2f);
        float newHeight = groundTop + (currentSize / 2) + lineHeightOffset;
        transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
    }

    void UpdateLineRenderer()
    {
        if (lineRenderer == null) return;

        float groundTop = ground.position.y + (ground.localScale.y / 2f);
        float lineHeight = groundTop + (currentSize / 2) + lineHeightOffset;

        Vector3 playerPosition = new Vector3(transform.position.x, lineHeight, transform.position.z);
        Vector3 doorPosition = new Vector3(door.position.x, lineHeight, door.position.z);

        lineRenderer.SetPosition(0, playerPosition);
        lineRenderer.SetPosition(1, doorPosition);

        lineRenderer.startWidth = currentSize;
        lineRenderer.endWidth = currentSize;
    }
}
