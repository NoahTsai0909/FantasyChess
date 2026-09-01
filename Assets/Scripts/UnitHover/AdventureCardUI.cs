using UnityEngine;
using UnityEngine.UI;

public class AdventureCardUI : MonoBehaviour
{
    public Image artworkImage;
    public AdventureDefinitionSO AdventureData { get; private set; }

    public void Setup(AdventureDefinitionSO data)
    {
        AdventureData = data;
        if (data.artworkCard != null) artworkImage.sprite = data.artworkCard;
    }
}
