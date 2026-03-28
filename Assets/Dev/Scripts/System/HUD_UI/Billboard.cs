using UnityEngine;
public class Billboard : MonoBehaviour
{
    private Transform _cameraTransform;

    // FUNCTIONALITY
    //--------------------------------------------------
    private void Update()
    {
        _cameraTransform = CameraSwitchManager.Instance.GetClosestCamera(transform).transform;
        Vector3 toCam = _cameraTransform.position - transform.position;
        toCam.y = 0f;

        if (toCam.sqrMagnitude < 0.0001f)
            return;
        
        transform.rotation = Quaternion.LookRotation(toCam.normalized, _cameraTransform.up);
    }
}