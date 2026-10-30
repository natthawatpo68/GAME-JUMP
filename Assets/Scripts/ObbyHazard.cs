using UnityEngine;

public sealed class ObbyHazard : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var respawn = other.GetComponent<PlayerRespawn>();
        if (respawn != null) respawn.Respawn();
    }
}
