using UnityEngine;

public class Block : MonoBehaviour
{
    Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Barrier"))
        {
            anim.SetBool("IsDying", true);
            
            Destroy(gameObject,1f);
        }
    }
}
