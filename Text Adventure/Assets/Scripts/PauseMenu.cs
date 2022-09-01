using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
	public static bool isOpen = false;
	[SerializeField]
	private GameObject mainMenuUI, optionsMenuUI, pauseMenuUI;

	public void onPauseClicked()
	{
		pauseMenuUI.SetActive(true);
		isOpen = true;
	}

	public void onRestartClicked()
	{
		pauseMenuUI.SetActive(false);
		isOpen = false;
		GridManager.Instance.generateGrid();
	}

	public void onOptionsClicked()
	{
		optionsMenuUI.SetActive(true);
	}

	public void onMainMenuClicked()
	{
		pauseMenuUI.SetActive(false);
		isOpen = false;
		mainMenuUI.SetActive(true);
		MainMenu.isOpen = true;
	}

	public void onResumeClicked()
	{
		pauseMenuUI.SetActive(false);
		isOpen = false;
	}
}
