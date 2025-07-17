using UnityEngine;

public class AttackCheck : MonoBehaviour
{
    PlayerController player;

    private void Start()
    {
        player = transform.parent.GetComponent<PlayerController>();
    }
}
