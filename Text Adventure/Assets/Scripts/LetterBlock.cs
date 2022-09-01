using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LetterBlock : MonoBehaviour
{

	[SerializeField] TextMeshPro tmp;
	[SerializeField] TextMeshPro scoreText;
	[SerializeField] GameObject highlightedPlane, lockedPlane, solvedPlane, underline;
	[SerializeField] Animator animator;
	[SerializeField] Material defaultMat, solvedMat, lockedMat, solvedLockedMat;
	[SerializeField] MeshRenderer tileRenderer;

	public int row { get; private set; }
	public int col { get; private set; }
	public bool highlighted { get; private set; }
	public bool solved { get; private set; }
	public bool locked { get; private set; }
	public bool upper { get; private set; }
	public char getLetter()
	{
		return tmp.text.ToLower()[0];
	}
	public void setLetter(char c)
	{
		tmp.text = "" + c.ToString().ToUpper()[0];
	}
	public void setPosition(int row, int col)
	{
		this.row = row;
		this.col = col;
	}
	public void setColor(Color c)
	{
		tmp.color = c;
	}

	public void setHighlighted(bool highlighted)
	{
		if (this.highlighted == highlighted)
			return;

		animator.SetBool("isHighlighted", highlighted);
		this.highlighted = highlighted;
	}
	public void setLocked(bool locked)
	{
		this.locked = locked;
		updateMaterial();
	}
	public void setSolved(bool solved)
	{
		this.solved = solved;
		updateMaterial();
	}

	public void setUpper(bool val)
	{
		upper = val;
		underline.SetActive(val);
	}

	// Resets the state of the letter block to how it was when initialized
	public void reset()
	{
		setHighlighted(false);
		setSolved(false);
		setLocked(false);
		setUpper(false);
		setLetter('#');
	}

	private void updateMaterial()
	{
		if (solved)
		{
			if (locked)
				tileRenderer.sharedMaterial = solvedLockedMat;
			else
				tileRenderer.sharedMaterial = solvedMat;
		}
		else
		{
			if (locked)
				tileRenderer.sharedMaterial = lockedMat;
			else
				tileRenderer.sharedMaterial = defaultMat;
		}
	}


	public void setScore(int s)
	{
		setScore(s, 0);
	}
	public void setScore(int s, int order)
	{
		if (s > 0)
		{
			scoreText.text = "+" + s;
		}
		else
		{
			scoreText.text = "";
		}
		animator.SetBool("isFlipping", true);
		animator.SetTrigger("suspend");
		StartCoroutine("c_delayFlip", 0.1f * order);
	}

	private IEnumerator c_delayFlip(float delay)
	{
		yield return new WaitForSeconds(delay);
		animator.SetTrigger("flip");
		animator.SetBool("isFlipping", false);
	}
}
