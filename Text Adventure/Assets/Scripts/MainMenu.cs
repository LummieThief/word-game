using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public static bool isOpen = true;
	[SerializeField] GameObject mainMenuUI, optionsMenuUI, tutorialMenuUI;
	[SerializeField] Button continueButton;
	private void Awake()
	{
		mainMenuUI.SetActive(true);
		if (!SaveLoad.Instance.state.initialized)
		{
			continueButton.interactable = false;
		}
	}

	public void onContinueClicked()
	{
		mainMenuUI.SetActive(false);
		isOpen = false;
		GridManager.Instance.loadGame();
	}

    public void onNewGameClicked()
	{
		mainMenuUI.SetActive(false);
		continueButton.interactable = true;
		isOpen = false;
		GridManager.Instance.generateGrid();
	}

	public void onTutorialClicked()
	{
		tutorialMenuUI.SetActive(true);
	}

	public void onOptionsClicked()
	{
		optionsMenuUI.SetActive(true);
	}
}
