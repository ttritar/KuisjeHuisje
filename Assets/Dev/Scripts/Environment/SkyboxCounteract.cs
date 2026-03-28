using UnityEngine;

public class SkyboxCounteract : MonoBehaviour
{
    [SerializeField] private GameObject _cam;

    // FUNCTIONALITY
    //--------------------------------------------------
    private void LateUpdate()
    {
        transform.rotation = _cam.transform.rotation;
    }
}
