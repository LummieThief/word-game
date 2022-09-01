using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShiftKey : MonoBehaviour
{
	public static ShiftKey Instance;
	private Button button;
	public bool held { get; private set; }

	private void Awake()
	{
		setupSingleton();
		button = GetComponent<Button>();
	}
	public void onKeyClicked()
	{
		setHeld(!held);
	}

	public void setHeld(bool val)
	{
		held = val;
		if (val)
		{
			var colors = button.colors;
			colors.normalColor = colors.pressedColor;
			colors.selectedColor = colors.pressedColor;
			colors.highlightedColor = colors.pressedColor;
			button.colors = colors;
		}
		else
		{
			var colors = button.colors;
			colors.normalColor = Color.white;
			colors.selectedColor = Color.white;
			colors.highlightedColor = Color.white;
			button.colors = colors;
		}
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
