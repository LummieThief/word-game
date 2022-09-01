using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Scoreboard : MonoBehaviour
{
	public static Scoreboard Instance;
    private Text text;
    private int score;
	private int highscore;
	private void Awake()
	{
		setupSingleton();
		text = GetComponent<Text>();
	}
	public void addScore(int val)
	{
		setScore(score + val);
	}
	public void setHighscore(int val)
	{
		highscore = val;
		redrawScore();
	}

	public void setScore(int val)
	{
		score = val;
		if (score > highscore)
		{
			highscore = score;
		}
		redrawScore();
	}

	public int getScore()
	{
		return score;
	}

	private void redrawScore()
	{
		text.text = "Best  : " + highscore + "\nScore: " + score;
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
