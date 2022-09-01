using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackspaceKey : MonoBehaviour
{
	public static BackspaceKey Instance;
	private GridManager gm;
	private SpellChecker sc;
	public char key = '<';
	private Button button;

	private void Awake()
	{
		setupSingleton();
		gm = FindObjectOfType<GridManager>();
		sc = FindObjectOfType<SpellChecker>();
		button = GetComponent<Button>();
	}
	public void onKeyClicked()
	{
		gm.receiveKey(key);
		sc.receiveKey(key);
	}
	public void disableButton()
	{
		button.interactable = false;
	}
	public void enableButton()
	{
		button.interactable = true;
	}
	public bool isEnabled()
	{
		return button.IsInteractable();
	}
	private void setupSingleton()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
