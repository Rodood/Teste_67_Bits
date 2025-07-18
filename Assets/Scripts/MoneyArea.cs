using UnityEngine;

public class MoneyArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StackController stack = other.GetComponent<StackController>();

            if (stack.stacked != null && stack.stacked.Count > 0)
            {
                for(int i = 0;  i < stack.stacked.Count; i++)
                {
                    GameManager.instance.UpdateMoney();
                }

                stack.ClearStack();
            }
        }
    }
}
