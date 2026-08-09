using UnityEngine;
using UnityEngine.UI;

public class FuelController : MonoBehaviour
{
    public static FuelController Instance;

    [SerializeField] private Image fuelFrontImage;
    [SerializeField] private float drainSpeed = 5f;
    [SerializeField] private float maxFuelAmount = 100f;
    [SerializeField] private Gradient fuelGradient;

    private float currentFuelAmount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentFuelAmount = maxFuelAmount;
        UpdateFuelUI();
    }

    private void UpdateFuelUI()
{
    float fuelPercent = currentFuelAmount / maxFuelAmount;

    fuelFrontImage.fillAmount = fuelPercent;
    fuelFrontImage.color = fuelGradient.Evaluate(fuelPercent);
}

    private void Update()
    {
        currentFuelAmount -= drainSpeed * Time.deltaTime;
        currentFuelAmount = Mathf.Max(currentFuelAmount, 0f);

        UpdateFuelUI();
    }
}