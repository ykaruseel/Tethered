using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI; 
using TMPro; // <--- 1. ДОБАВИЛИ ЭТУ БИБЛИОТЕКУ

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("Кнопка ИГРАТЬ")]
    public Button playButton;
    
    // <--- 2. ИЗМЕНИЛИ ТИП ПЕРЕМЕННОЙ (было Text, стало TMP_Text)
    public TMP_Text statusText; 

    void Start()
    {
        playButton.interactable = false;
        
        // Добавил проверку (!= null), чтобы не было ошибок, если забудешь привязать
        if(statusText != null) statusText.text = "Подключение к серверу...";

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if(statusText != null) statusText.text = "Готово! Жми играть.";
        playButton.interactable = true; 
    }

    public void ConnectToRoom()
    {
        if(statusText != null) statusText.text = "Входим в комнату...";
        PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        if(statusText != null) statusText.text = "Ура! Загружаем игру...";
        PhotonNetwork.LoadLevel("SampleScene"); 
    }
}
