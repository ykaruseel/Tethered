using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, что столкнулись именно с одним из игроков.
        // Используем теги Player1/Player2, которые мы добавили.
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            // Проверяем, что GameManager существует
            if (GameManager.Instance != null)
            {
                // 1. Устанавливаем чекпоинт в позицию этого кубика
                GameManager.Instance.SetCheckpoint(transform.position);
                
                Debug.Log("Чекпоинт установлен на: " + transform.position);

                // 2. Опционально: Отключаем этот чекпоинт, чтобы он не срабатывал 
                // снова и не спамил консоль при повторном прохождении
                GetComponent<Collider2D>().enabled = false; 
                
                // Опционально: Сделай кубик зеленым или невидимым
                // GetComponent<SpriteRenderer>().color = Color.green;
            }
        }
    }
}
