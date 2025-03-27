using UnityEngine;

public class CollectFuel : MonoBehaviour {
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            FuelController.instance.FillFuel();
            Destroy(gameObject);
        }
    }
}
