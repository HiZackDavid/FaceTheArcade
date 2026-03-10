using UnityEngine;
using UnityEngine.UI;

public class InteractionArea : MonoBehaviour
{

    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 10.0f;

    private Color prevCol;
    private GameObject curObject;

    private bool isInteractuable = false;

    public bool canInteract() => isInteractuable;
    public GameObject getCurObject() => curObject;

    private void OnTriggerEnter(Collider other)
    {
        SetOutline(other.gameObject);
        isInteractuable = true;
    }

    private void OnTriggerExit(Collider other)
    {
        RemoveOutline(other.gameObject);
        isInteractuable = false;
    }

    private void SetOutline(GameObject target) 
    {
        if (target.CompareTag("Selectable"))
        {
            MeshRenderer renderer = target.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat;
                if (renderer.materials.Length > 1)
                    mat = renderer.materials[1];
                else
                    mat = renderer.material;

                prevCol = mat.color;
                mat.color = Color.white;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.white);
            }
            else
            {
                Outline outline = target.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = target.AddComponent<Outline>();
                }

                outline.enabled = true;
                outline.OutlineColor = outlineColor;
                outline.OutlineWidth = outlineWidth;
            }

            curObject = target;
            UIManager.instace.showHideableHUD();
        }
    }

    private void RemoveOutline(GameObject target) 
    {
        if (target.CompareTag("Selectable"))
        {

            MeshRenderer renderer = target.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat;
                if (renderer.materials.Length > 1)
                    mat = renderer.materials[1];
                else
                    mat = renderer.material;

                mat.color = prevCol;
                mat.DisableKeyword("_EMISSION");
            }
            else
            {
                Outline outline = target.GetComponent<Outline>();
                if (outline != null)
                    outline.enabled = false;
            }

            UIManager.instace.hideHideableHUD();
        }

    }
}
