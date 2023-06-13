using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointAlly : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(this.transform.position, 0.5f);
    }
}
