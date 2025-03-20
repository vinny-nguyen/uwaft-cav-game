using UnityEngine;
using System.Collections.Generic;

public class CarSelectionManager : MonoBehaviour {
    public List<GameObject> carPrefabs; // List of available car prefabs
    public Transform carSpawnPoint;     // Where the car should appear
    private GameObject currentCar;

    // Call this method from UI button, passing the index of the car
    public void SelectCar(int index) {
        if (currentCar != null)
            Destroy(currentCar);

        currentCar = Instantiate(carPrefabs[index], carSpawnPoint.position, carSpawnPoint.rotation);
        
        // Optionally update camera, UI or map to follow the new car
    }
}
