using UnityEngine;
using UnityEngine.InputSystem;
public class PedalController : MonoBehaviour
{
    public bool IsAccelerating { get; private set; }
    public bool IsBraking { get; private set; }

    public PedalVisualFeedback acceleratorVisual;
    public PedalVisualFeedback brakeVisual;

    public void OnAccelerateDown()
    {
        IsAccelerating = true;
        if (acceleratorVisual != null) acceleratorVisual.SetPressed(true);
    }
    public void OnAccelerateUp()
    {
        IsAccelerating = false;
        if (acceleratorVisual != null) acceleratorVisual.SetPressed(false);
    }
    public void OnBrakeDown()    
    {
        IsBraking = true;
        if (brakeVisual != null) brakeVisual.SetPressed(true);
    }
    public void OnBrakeUp()
    {
        IsBraking = false;
        if (brakeVisual != null) brakeVisual.SetPressed(false);
    }

    void Update()
    {
        // Accelerator pedal (D key)
        if (Input.GetKeyDown(KeyCode.D))
            acceleratorVisual.SetPressed(true);
        if (Input.GetKeyUp(KeyCode.D))
            acceleratorVisual.SetPressed(false);
    
        // Brake pedal (A key)
        if (Input.GetKeyDown(KeyCode.A))
            brakeVisual.SetPressed(true);
        if (Input.GetKeyUp(KeyCode.A))
            brakeVisual.SetPressed(false);
    }
}