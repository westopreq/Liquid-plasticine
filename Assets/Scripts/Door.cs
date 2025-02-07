using UnityEngine;

public class Door : MonoBehaviour
{
    public float openDistance = 5f;  // Расстояние, при котором дверь откроется
    private bool isOpen = false;
    
    public GameObject player; // Ссылка на объект игрока

    void Update()
    {
        // Проверяем расстояние от игрока до двери
        if (Vector3.Distance(transform.position, player.transform.position) <= openDistance && !isOpen)
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        // Открытие двери (например, движение или изменение состояния)
        isOpen = true;
        Debug.Log("Дверь открыта!");
        // Здесь ты можешь добавить анимацию или другие эффекты
    }
}
