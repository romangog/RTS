using UnityEngine;

public class SpawnPointEnemy : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(this.transform.position, 0.5f);
    }
}
