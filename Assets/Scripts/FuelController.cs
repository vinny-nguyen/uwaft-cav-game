using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuelController : MonoBehaviour {
    public static FuelController instance;

    [SerializeField] private Image _Fuel_Image;
    [SerializeField, Range(0.1f, 5f)] private float _Fuel_Drain_Speed = 5f;
    [SerializeField] private float _Max_Fuel_Amount = 80f;
    [SerializeField] private Gradient _Fuel_Gradient;
    
    private float _Current_Fuel_Amount;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    private void Start() {
        _Current_Fuel_Amount = _Max_Fuel_Amount;
        UpdateUI();
    }

    private void Update() {
        _Current_Fuel_Amount -= Time.deltaTime * _Fuel_Drain_Speed;
        UpdateUI();

        if (_Current_Fuel_Amount <= 0f) {
            GameManager.instance.GameOver();
        }
    }

    private void UpdateUI() {
        _Fuel_Image.fillAmount = (_Current_Fuel_Amount / _Max_Fuel_Amount); // .fillAmount for Amount of Fuel Bar shown
        _Fuel_Image.color = _Fuel_Gradient.Evaluate(_Fuel_Image.fillAmount); // Gradient colours for Fuel Bar
    }

    public void FillFuel() {
        _Current_Fuel_Amount = _Max_Fuel_Amount;
        UpdateUI();
        
    }
}
