using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class HpBarControlelr : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject camera;

    [SerializeField] private Image HpImage;
    [SerializeField] private Image StaminaImage;
    void Start()
    {
        Camera mainCamera = Camera.main;
        camera = mainCamera.gameObject;


    }

    // Update is called once per frame
    void Update()
    {
        WatchToCamera();
    }

    public void UpdateHp(float hp, float maxHp)
    {
        HpImage.fillAmount = hp/maxHp;
    }
    public void UpdateStamina(float stamina, float maxStamina)
    {
        StaminaImage.fillAmount = stamina/maxStamina;
    }


    private void WatchToCamera()
    {
        transform.LookAt(new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z));
        transform.rotation = Quaternion.Euler(new Vector3(transform.position.x, 0, 0));
    }
}
