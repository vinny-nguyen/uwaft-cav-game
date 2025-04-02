using UnityEngine;

public class CarUpgradeManager : MonoBehaviour
{
    // Assign these in the Inspector
    public GameObject[] carBodies; // [car_1, car_2, car_3, car_4]
    public GameObject[] tires;     // [tire_old, tire_new]

    private int currentNodeIndex = 0;

    void Start()
    {
        InitializeCar();
    }

    public void CompleteNode(int nodeIndex)
    {
        currentNodeIndex = nodeIndex;
        UpdateCarAppearance();
    }

    void UpdateCarAppearance()
    {
        // Step 1: Deactivate all car bodies and tires
        foreach (var car in carBodies) car.SetActive(false);
        foreach (var tire in tires) tire.SetActive(false);

        // Step 2: Determine which car body to show
        int carBodyIndex = 0; // Default to car_1

        if (currentNodeIndex >= 2) carBodyIndex = 1; // car_2 after node 2
        if (currentNodeIndex >= 4) carBodyIndex = 2; // car_3 after node 4
        if (currentNodeIndex >= 5) carBodyIndex = 3; // car_4 after node 5

        // Step 3: Determine which tires to show
        bool useNewTires = (currentNodeIndex >= 1); // New tires after node 1

        // Step 4: Activate the selected parts
        carBodies[carBodyIndex].SetActive(true);
        tires[useNewTires ? 1 : 0].SetActive(true);
    }

    void InitializeCar()
    {
        // Start with car_1 and old tires
        carBodies[0].SetActive(true);
        tires[0].SetActive(true);
    }
}