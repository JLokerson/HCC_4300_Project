using UnityEngine;

[RequireComponent(typeof(Collider))]
public class XPOrb : MonoBehaviour
{
    [SerializeField] private int xpValue = 10;

    private void OnTriggerEnter(Collider other)
    {
        var xp = other.GetComponentInParent<XPSystem>();
        if (xp != null)
        {
            xp.AddXP(xpValue);
            Destroy(gameObject);
        }
    }
}
