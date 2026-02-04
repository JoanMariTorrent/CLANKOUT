using System.Collections;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private TrailRenderer trail;

    public void Init(Vector3 startPos, Vector3 endPos)
    {
        transform.position = startPos;

        StartCoroutine(MoveRoutine(startPos, endPos));
    }

    private IEnumerator MoveRoutine(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        float duration = distance / speed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;

        if(trail != null)
        {
            yield return new WaitForSeconds(trail.time);
        }

        Destroy(gameObject);
    }
}
