using UnityEngine;

public class testscript : MonoBehaviour
{

    [SerializeField]
    private Event e;

    void Start()
    {
        e.Invoke();
    }
}
