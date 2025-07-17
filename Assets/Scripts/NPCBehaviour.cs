using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public Animator anim;

    private void Awake()
    {
        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    public void Knockout()
    {
        anim.enabled = false;
        this.gameObject.layer = 0;
    }
}
