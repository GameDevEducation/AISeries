using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillageUIResourcePanel : MonoBehaviour
{
    [SerializeField] Village LinkedVillage;
    [SerializeField] WorldResource.EType ResourceType;
    [SerializeField] TextMeshProUGUI ResourceTypeLabel;
    [SerializeField] TextMeshProUGUI ResourceAmountLabel;
    [SerializeField] Slider ResourceProgress;
    int AmountStored = 0;

    // Start is called before the first frame update
    void Start()
    {
        ResourceTypeLabel.text = ResourceType.ToString();

        AmountStored = LinkedVillage.GetAmountStored(ResourceType);
        ResourceAmountLabel.text = AmountStored.ToString();
        ResourceProgress.value = AmountStored;
    }

    // Update is called once per frame
    void Update()
    {
        var currentAmount = LinkedVillage.GetAmountStored(ResourceType);

        if (currentAmount != AmountStored)
        {
            AmountStored = currentAmount;
            ResourceAmountLabel.text = AmountStored.ToString();
            ResourceProgress.value = AmountStored;
        }
    }
}
