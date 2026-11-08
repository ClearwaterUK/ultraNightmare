using System;
using UltraNightmare.UIComponents;
using UnityEngine;

namespace UltraNightmare.Components;

public class Descriptor : MonoBehaviour
{
    public string descriptorText;

    public void OnMouseExit()
    {
        UltraNightmareMainPanel.Descriptor.gameObject.SetActive(false);
    }

    public void OnMouseEnter()
    {
        UltraNightmareMainPanel.Descriptor.gameObject.SetActive(true);
        UltraNightmareMainPanel.Descriptor.text = descriptorText;
    }
}