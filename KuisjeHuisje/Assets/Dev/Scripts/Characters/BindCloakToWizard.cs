using UnityEngine;

public class BindCloakToWizard : ClothingSelection
{
	[SerializeField] private Top _top = Top.WizardCloak;
	[SerializeField] private Bottom _bottom = Bottom.COUNT;
	[SerializeField] private Hair _hair = Hair.WizardHat;
	// START
	//--------------------------------------------------
	private void Start()
    {
        ApplyAll(_top, _bottom, _hair);
	}
}
