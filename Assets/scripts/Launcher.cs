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
        if(statusText != null) statusText.text = "Connection to the cave...";

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if(statusText != null) statusText.text = "Done! Tap something on the screen!.";
        playButton.interactable = true; 
    }

    public void ConnectToRoom()
    {
        if(statusText != null) statusText.text = "Entering a friend's consciousness...";
        PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        if(statusText != null) statusText.text = "Hooray! We're downloading something there...";
        PhotonNetwork.LoadLevel("SampleScene"); 
    }
}
