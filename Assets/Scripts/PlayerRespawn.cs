using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private float fallY = -8f;

    private Transform currentRespawnPoint;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        currentRespawnPoint = startPoint;
    }

    private void Update()
    {
        if (transform.position.y < fallY) Respawn();
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null) return;
        currentRespawnPoint = checkpoint;
    }

    public void Respawn()
    {
        Transform point = currentRespawnPoint != null ? currentRespawnPoint : startPoint;
        if (point == null) return;
        characterController.enabled = false;
        transform.SetPositionAndRotation(point.position, point.rotation);
        characterController.enabled = true;
    }
}
