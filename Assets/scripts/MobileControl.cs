using UnityEngine;
using Photon.Pun;

public class MobileControl : MonoBehaviour
{
    // Эта функция ищет ТВОЕГО персонажа среди всех на сцене
    private PlayerMovement2 GetMyPlayer() 
    {
        // 🛑 ИСПРАВЛЕНО: Используем самый простой способ поиска. 
        // Это убирает ошибку "красного" FindObjectsSortMode.
        var players = FindObjectsOfType<PlayerMovement2>(); 
        
        foreach (var p in players)
        {
            // Если этот игрок принадлежит мне (IsMine == true)
            if (p.view.IsMine) 
            {
                return p; // Возвращаем его!
            }
        }
        return null; // Если не нашли 
    }

    // --- ЭТИ ФУНКЦИИ ВЕШАЕМ НА КНОПКИ ---

    public void LeftDown() 
    {
        GetMyPlayer()?.OnLeftDown(); 
    }

    public void RightDown() => GetMyPlayer()?.OnRightDown();
    
    public void JumpDown()  => GetMyPlayer()?.OnJumpDown();
    
    public void Stop()      => GetMyPlayer()?.OnButtonUp();
}
