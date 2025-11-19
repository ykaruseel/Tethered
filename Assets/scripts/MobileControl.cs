using UnityEngine;
using Photon.Pun; // Нужно для поиска "своего" игрока

public class MobileControl : MonoBehaviour
{
    // Эта функция ищет ТВОЕГО персонажа среди всех на сцене
    private PlayerMovement GetMyPlayer()
    {
        // Находим всех игроков с компонентом PlayerMovement
        var players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        
        foreach (var p in players)
        {
            // Если этот игрок принадлежит мне (IsMine == true)
            if (p.view.IsMine) 
            {
                return p; // Возвращаем его!
            }
        }
        return null; // Если не нашли (например, еще не загрузился)
    }

    // --- ЭТИ ФУНКЦИИ ВЕШАЕМ НА КНОПКИ ---

    public void LeftDown() 
    {
        // Находим игрока -> Если он есть -> Нажимаем кнопку
        GetMyPlayer()?.OnLeftDown(); 
    }

    public void RightDown() => GetMyPlayer()?.OnRightDown();
    
    public void JumpDown()  => GetMyPlayer()?.OnJumpDown();
    
    public void Stop()      => GetMyPlayer()?.OnButtonUp();
}
