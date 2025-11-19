using UnityEngine;
using Photon.Pun; // Добавляем библиотеку Photon

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, что в зону смерти упал Игрок (проверяем оба тега)
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            // Получаем компонент сети
            PhotonView view = collision.GetComponent<PhotonView>();

            // ВАЖНО: Респавним игрока ТОЛЬКО если это НАШ игрок.
            // Мы не можем трогать чужого игрока.
            if (view != null && view.IsMine)
            {
                // Вызываем НОВУЮ функцию из GameManager и передаем туда СЕБЯ
                GameManager.Instance.RespawnMyPlayer(collision.gameObject);
            }
        }
    }
}
