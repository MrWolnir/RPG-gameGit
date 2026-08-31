using UnityEngine;
using UnityEngine.Rendering;

public class PersistentFog : MonoBehaviour
{
    void Start()
    {
        // Это говорит URP: "Не удаляй содержимое этой текстуры после отрисовки"
        var cam = GetComponent<Camera>();
        if (cam.targetTexture != null)
        {
            cam.targetTexture.DiscardContents(false, false);
        }
    }
}