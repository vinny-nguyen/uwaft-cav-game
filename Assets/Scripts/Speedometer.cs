using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public Rigidbody2D carRB;
    public TextMeshProUGUI speedometerText;

    void Update()
    {
        if (carRB != null && speedometerText != null)
        {
            // Converts m/s to km/h:
            float speed = carRB.linearVelocity.magnitude * 3.6f;
            speedometerText.text = speed.ToString("0") + " km/h";
        }
    }
}