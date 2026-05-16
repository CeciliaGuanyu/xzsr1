using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class InteractableObject : MonoBehaviour
{
    public string displayName;
    [TextArea(2, 5)]
    public string description;

    public UnityEvent onInteracted;

    [Header("Highlight")]
    public Color highlightColor = new Color(0.3f, 0.25f, 0.1f);

    private Dictionary<Renderer, Material[]> normalMats = new();
    private Dictionary<Renderer, Material[]> highlightMats = new();
    private bool matsReady;

    void Reset()
    {
        displayName = gameObject.name;
    }

    void OnDestroy()
    {
        foreach (var mats in highlightMats.Values)
            foreach (var m in mats)
                Destroy(m);
    }

    void EnsureMaterials()
    {
        if (matsReady) return;
        matsReady = true;

        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            normalMats[r] = r.sharedMaterials;

            var hMats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < hMats.Length; i++)
            {
                hMats[i] = new Material(r.sharedMaterials[i]);
                hMats[i].EnableKeyword("_EMISSION");
                hMats[i].SetColor("_EmissionColor", highlightColor);
            }
            highlightMats[r] = hMats;
        }
    }

    public void SetHighlight(bool on)
    {
        EnsureMaterials();
        foreach (var r in normalMats.Keys)
            r.materials = on ? highlightMats[r] : normalMats[r];
    }
}
