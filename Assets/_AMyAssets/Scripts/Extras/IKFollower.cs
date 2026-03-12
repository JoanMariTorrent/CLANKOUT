using UnityEngine;

// Pon este script en tu IK_Target
public class IKFollower : MonoBehaviour
{
    public Transform targetToFollow;
    [SerializeField] private Transform transformAPose;

    void LateUpdate() 
    {
        if (targetToFollow != null)
        {
            transform.position = targetToFollow.position;
            transform.rotation = targetToFollow.rotation;
        }
        else
        {
            transform.position = transformAPose.position;
            transform.rotation = transformAPose.rotation;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if(newTarget != null) targetToFollow = newTarget;
    }
}
