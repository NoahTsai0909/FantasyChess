using UnityEngine;

public class VFXAutoDestroy : MonoBehaviour
{
    public float lifetime = 1.5f; // Make sure this is longer than your longest animation/particle

    private void Start()
    {
        // Tells Unity to destroy this GameObject after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }
}
