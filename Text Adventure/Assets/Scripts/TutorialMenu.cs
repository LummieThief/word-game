using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenu : MonoBehaviour
{
	[SerializeField] GameObject tutorialMenuUI;
	[SerializeField] GameObject[] slides;
	private int slideNumber = 0;

	public void onNextClicked()
	{
		slides[slideNumber].SetActive(false);
		slideNumber++;
		if (slideNumber >= slides.Length)
		{
			slideNumber = 0;
			slides[0].SetActive(true);
			tutorialMenuUI.SetActive(false);
		}
		else
		{
			slides[slideNumber].SetActive(true);
		}
	}

	public void onPrevClicked()
	{
		slides[slideNumber].SetActive(false);
		slideNumber--;
		if (slideNumber < 0)
		{
			slideNumber = 0;
			slides[0].SetActive(true);
			tutorialMenuUI.SetActive(false);
		}
		else
		{
			slides[slideNumber].SetActive(true);
		}
	}
}
