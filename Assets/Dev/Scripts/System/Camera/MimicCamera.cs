using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MimicCamera : MonoBehaviour
{
    [SerializeField] private Camera _cameraToMimic;
    private Vector3 _offsetPos;
    private Quaternion _offsetRot;

    // START
    //--------------------------------------------------
    private void Start()
    {
        _offsetPos = transform.position - _cameraToMimic.transform.position;
        _offsetRot = Quaternion.Inverse(_cameraToMimic.transform.rotation) * transform.rotation;
    }

    // FUNCTION
    //--------------------------------------------------
    private void LateUpdate()
    {
        transform.position = _cameraToMimic.transform.position + _offsetPos;
        transform.rotation = _cameraToMimic.transform.rotation * _offsetRot;
    }
    public void ChangeOffsetPosition(Vector3 offset)
    {
        var newOffset = offset - _offsetPos;
        _offsetPos += newOffset;
    }
}
