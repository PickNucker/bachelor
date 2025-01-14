using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SelectForm : NetworkBehaviour
{
    [SerializeField] private List<GameObject> forms = new List<GameObject>();

    [SerializeField] TMP_Dropdown dropdown;

    private void Start()
    {
        if (dropdown != null)
        {
            // Füge ein Event hinzu, das ausgelöst wird, wenn eine Auswahl getroffen wird
            dropdown.onValueChanged.AddListener(ChangeForm);
        }
    }

    public void GetDropDownValue()
    {
        int index = dropdown.value;
        ChangeForm(index);
    }

    public void ChangeForm(int formToGetSelected)
    {
        if (IsServer)
        {
            ChangeFormInternal(formToGetSelected);
            ChangeFormClientRpc(formToGetSelected);
        }
        else
        {
            ChangeFormServerRpc(formToGetSelected);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void ChangeFormServerRpc(int formToGetSelected)
    {
        ChangeFormInternal(formToGetSelected);

        ChangeFormClientRpc(formToGetSelected);
    }

    [ClientRpc]
    private void ChangeFormClientRpc(int formToGetSelected)
    {
        ChangeFormInternal(formToGetSelected);
    }

    private void ChangeFormInternal(int formToGetSelected)
    {
        foreach (GameObject form in forms)
        {
            form.SetActive(false);
        }

        //dropdown[formToGetSelected].SetActive(true);

        if (formToGetSelected >= 0 && formToGetSelected <= forms.Count)
        {
            forms[formToGetSelected].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Ungültiger Index: {formToGetSelected}. Verfügbare Formen: {forms.Count}");
        }
    }
}
