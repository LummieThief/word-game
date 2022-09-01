using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpellChecker : MonoBehaviour
{
    private bool selected = false;
    private TMP_InputField inputField;
    public Color validColor;
    public Color invalidColor;

	private void Awake()
	{
        inputField = GetComponent<TMP_InputField>();
	}

	public void onSelected()
	{
        if (selected)
            return;
        selected = true;
	}

    public void onDeselected()
	{
        int mask = LayerMask.GetMask("InterfaceMask");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //(Input.GetTouch(0).position)
        RaycastHit hit;
        // Checks to see if the keyboard was clicked
        if (Physics.Raycast(ray, out hit, 100, mask))
        {
            // The keyboard was clicked, so we dont want to deselect the input field
            inputField.ActivateInputField();
        } 
        else
		{
            selected = false;
            inputField.text = "";
            inputField.image.color = Color.white;
        }
    }

    public void receiveKey(char c)
    {
        if (selected)
		{
            if (c == '<')
			{
                if (inputField.text.Length > 0)
				{
                    int newCaretPosition = Mathf.Max(inputField.caretPosition - 1, 0);
                    inputField.text = inputField.text.Substring(0, Mathf.Max(inputField.caretPosition - 1, 0)) +
                        inputField.text.Substring(inputField.caretPosition, inputField.text.Length - inputField.caretPosition);
                    inputField.caretPosition = newCaretPosition;
                    //inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
                }
            }
            else
			{
                if (inputField.text.Length >= inputField.characterLimit)
				{
                    return;
				}
                string first = inputField.text.Substring(0, inputField.caretPosition);
                string second = inputField.text.Substring(inputField.caretPosition, inputField.text.Length - inputField.caretPosition);
                inputField.text = first + c.ToString().ToUpper() + second;
                //inputField.text += c.ToString().ToUpper();
                inputField.caretPosition++;
            }

            if (inputField.text.Length == 0)
			{
                inputField.image.color = Color.white;
            }
            else if (WordValidator.Instance.hasWord(inputField.text.ToLower()))
			{
                inputField.image.color = validColor;
            } 
            else
			{
                inputField.image.color = invalidColor;
            }
            
        }
    }
}
