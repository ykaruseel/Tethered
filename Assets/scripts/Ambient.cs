using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AmbientLoop : MonoBehaviour
{
    [Header("FMOD Событие эмбиента (луп)")]
    [SerializeField] private EventReference ambientEvent;

    private EventInstance ambientInstance;

    void Start()
    {
        // Создаём инстанс звука (чтобы можно было управлять)
        ambientInstance = RuntimeManager.CreateInstance(ambientEvent);

        // Можно задать позицию в 3D или сделать 2D
        RuntimeManager.AttachInstanceToGameObject(ambientInstance, transform, GetComponent<Rigidbody2D>());

        // Запускаем эмбиент
        ambientInstance.start();
        ambientInstance.release(); // отпускаем, но он играет, пока не остановится вручную
    }

    void OnDestroy()
    {
        // При уничтожении объекта — выключаем эмбиент
        ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
