using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public float openDistance = 5f;  // Расстояние, при котором дверь откроется
    private bool isOpen = false;
    
    public GameObject player; // Ссылка на объект игрока
    public DoorScript.Door doorScript; // Ссылка на скрипт двери

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
        // Устанавливаем состояние двери в открытое
        isOpen = true;
        Debug.Log("Дверь открыта!");
        
        if (doorScript != null)
        {
            if (!doorScript.open) // Только если дверь закрыта
            {
                doorScript.open = true;
                Debug.Log("Флаг open установлен в true.");
            }
            else
            {
                Debug.Log("Флаг open уже был установлен!");
            }
        }
        else
        {
            Debug.LogError("DoorScript не назначен в инспекторе!");
        }
    }
}
