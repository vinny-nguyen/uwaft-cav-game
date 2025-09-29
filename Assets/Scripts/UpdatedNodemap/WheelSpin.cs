using UnityEngine;

public class WheelSpinner : MonoBehaviour
{
    [SerializeField] Transform tireFront;
    [SerializeField] Transform tireRear;
    [SerializeField] float spinSpeed = 360f; // degrees per second at nominal speed
    public bool spinning;

    void Update()
    {
        if (!spinning) return;
        float dt = Time.deltaTime * spinSpeed;
        tireFront.Rotate(0f, 0f, -dt);
        tireRear.Rotate(0f, 0f, -dt);
    }
}
