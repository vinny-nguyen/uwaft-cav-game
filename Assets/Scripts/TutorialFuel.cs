using UnityEngine;

public class TutorialFuel : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            FuelTest.instance.FillFuel();
            Destroy(gameObject);
        }
    }
}