using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ObbyCheckpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var respawn = other.GetComponent<PlayerRespawn>();
        if (respawn != null) respawn.SetCheckpoint(respawnPoint != null ? respawnPoint : transform);
    }
}
