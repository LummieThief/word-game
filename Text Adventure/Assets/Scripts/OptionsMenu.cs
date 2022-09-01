using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject optionsMenuUI;

	public void onBackClicked()
	{
		optionsMenuUI.SetActive(false);
	}
}
