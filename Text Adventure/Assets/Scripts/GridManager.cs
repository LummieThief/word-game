using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[RequireComponent(typeof(WordValidator))]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    // The prefab of a letterblock for instantiation
    public GameObject letterBlockPrefab;
    // The number of letterblocks vertically
    private int height;
    // The number of letterblocks horizontally
    private int width;
    // The minimum number of letters for a word to be valid
    private int minWordLength = 3;
    // The side length in letterblocks of the square camera viewport
    private int viewportSize = 8;
    // The percentage of letters that spawn locked
    public float lockedBlockPercentage = 0.1f;
    // An array of all of the letterblocks in the grid (size [height, width])
    private LetterBlock[,] letterBlocks;
    // Stores whether or not the grid has been instantiated
    public bool initialized { get; private set; }
    // Stores the letterblock that was clicked down on at the start of a drag event
    private LetterBlock dragStartBlock;
    // Stores the letterblock that is currently at the end of the drag event
    private LetterBlock dragEndBlock;
    // Stores the direction of the drag event. Horizontal right is (1, 0), diagonal left down
    // is (-1, -1), etc.
    private Vector2 dragDirection;
    // Whether or not there is currently a drag event
    private bool dragging = false;
    // 1 if only the first letter of the word is capitalized, -1 if a non-starting letter is capitalized, 0 otherwise. Set in getSelectedWord()
    private int capitalization = 0;
    // The letterblock that will be changing its letter
    private LetterBlock selectedBlock;
    // Whether or not the wild tile has been played
    private bool wildPlayed;
    // The number of solved tiles
    private int numSolved = 0;
    // An array of the frequencies of each letter, in percentage, in the english language.
    private float[] letterFrequencies =
    {
        8.4966f,    //A
        2.0720f,    //B
        4.5388f,    //C
        3.3844f,    //D
        11.1607f,   //E
        1.8121f,    //F
        2.4705f,    //G
        3.0034f,    //H
        7.5448f,    //I
        0.1965f,    //J
        1.1016f,    //K
        5.4893f,    //L
        3.0129f,    //M
        6.6544f,    //N
        7.1635f,    //O
        3.1671f,    //P
        0.1962f,    //Q
        7.5809f,    //R
        5.7351f,    //S
        6.9509f,    //T
        3.6308f,    //U
        1.0074f,    //V
        1.2899f,    //W
        0.2902f,    //X
        1.7779f,    //Y
        0.2722f     //Z
    };

	private void Awake()
	{
        setupSingleton();
        height = width = viewportSize;
        initialize();
    }

	private void Update()
    {
        if (!MainMenu.isOpen && !PauseMenu.isOpen)
		{
            handleInput();
        }
    }

    public void generateGrid()
	{
        resetBlocks();
        resetScore();
        paintBlocks(generateString(width, height));
        lockBlocks();
        wildPlayed = false;
        saveGame();
        //Debug.Log("Initial State: " + getSimpleState());
    }

    // Handles user input
    private void handleInput()
	{
        if(Input.inputString.Length > 0)
		{
            receiveKey(Input.inputString[0]);
		}

        if (initialized)
        {
            if (Input.GetMouseButtonDown(0)) //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                // sets up raycasting variables
                int mask = LayerMask.GetMask("LetterBlocks", "InterfaceMask", "Tiles");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //(Input.GetTouch(0).position)
                RaycastHit hit;
                // Checks to see if a block was clicked on
                if (Physics.Raycast(ray, out hit, 100, mask))
                {
                    // Something was clicked, either a block, a tile, or the interface
                    // If we click the interface, we dont want to affect the board at all.
                    // If we click a block, we want to either start a drag event or select event.
                    // If we click a tile, we want to get its block.
                    if (hit.transform.gameObject.layer != LayerMask.NameToLayer("InterfaceMask"))
					{
                        // either a block or tile was clicked
                        LetterBlock block = hit.transform.gameObject.GetComponent<LetterBlock>();
                        if (block == null)
						{
                            // a tile was clicked, so we have to get the block component of its parent.
                            block = hit.transform.gameObject.GetComponentInParent<LetterBlock>();
						}

                        deselectBlock();
                        if (block.highlighted /*&& !block.solved*/ && !block.locked)
                        {
                            // We should start a select event
                            selectedBlock = block;
                            selectedBlock.setColor(Color.red);
                        }
                        else
                        {
                            // We should start a drag event
                            dragging = true;
                            dragStartBlock = block;
                            dragEndBlock = block;
                        }
                    }
                    // else, the interface was clicked so we skip.
                        
                }
                else
				{
                    // Clicked outside the grid but not on the interface
                    deselectBlock();
                    clearHighlights();
				}

            }
            if (dragging)
            {
                if (Input.GetMouseButtonUp(0)) //(Input.touchCount == 0)
                {
                    resolveDrag();
                }
                else
                {
                    highlightDrag();
                }
            }
        }
    }

    // Called by KeyboardKey when a new key is clicked, or by this when a key is pressed
    // on the keyboard
    public void receiveKey(char c)
    {
        // If we dont have a letter selected we cant do anything with keyboard input
        if (selectedBlock == null)
            return;

        // stores the current value of the tile in case we need to revert it
        char oldLetter = selectedBlock.getLetter();

        // Now that the old value is stored, we replace the selected tile with the received key
        if (c == '<')
		{
            // If the received key was backspace, set the selected tile to a space
            selectedBlock.setLetter(' ');
        } 
        else
		{
            // Otherwise, set the tile as normal
            selectedBlock.setLetter(c);
        }
        
        // Now we get the new word spelled by the selected tiles on the grid
        string selectedWord = getSelectedWord();

        // If one of the tiles is wild and can be substituted for a valid letter, this is true.
        // Otherwise false.
        bool wildExists = checkWild(selectedWord);

        // If the word is in the dictionary or the word has a wild configuration that is in the dictionary,
        // and we are not trying to place a capital letter in the middle of a word
        // and we are not playing a second wild tile
        if ((WordValidator.Instance.hasWord(selectedWord) || wildExists) &&
            !(ShiftKey.Instance.held && (selectedBlock != dragStartBlock)) &&
            !(wildPlayed && c == '<'))
        {
            // If shift is held, it must be the first letter, so we make it uppercase
            if (ShiftKey.Instance.held)
                selectedBlock.setUpper(true);

            // If backspace was pressed and made it to here, the user successfully played the wild tile.
            if (c == '<')
                wildPlayed = true;

            // We lock the selected block since it was changed, then deselect it.
            selectedBlock.setLocked(true);
            deselectBlock();

            // Finally, we solve all of the blocks that are highlighted.
            solveHighlights();
        }
        else
        {
            // The highlighted word is invalid, so we revert the block we changed then deselect it
            selectedBlock.setLetter(oldLetter);
            deselectBlock();

            // Finally, we clear all of the highlighted blocks without solving them.
            clearHighlights();
        }

        // The user typed a letter (whether successful or unsuccessful), so we release shift
        ShiftKey.Instance.setHeld(false);
    }

    // Returns whether or not any spaces in the string can be replaced
    // by a letter and have the word be valid. Always returns false if
    // the string doesnt have any spaces.
    private bool checkWild(string word)
	{
        if (!word.Contains(" "))
            return false;

        bool wildExists = false;
        char c = 'a';
        while (!WordValidator.Instance.hasWord(word.Replace(' ', c)) && c <= 'z')
        {
            c++;
        }
        if (c <= 'z')
        {
            Debug.Log(word.Replace(' ', c));
            wildExists = true;
        }
        return wildExists;
    }

    private void deselectBlock()
	{
        if (selectedBlock != null)
        {
            selectedBlock.setColor(Color.black);
            selectedBlock = null;
        }
    }

    // During a drag event, highlights the letters that are being selected.
    // Requires that there is a touch
    private void highlightDrag()
	{
        int mask = LayerMask.GetMask("LetterBlocks");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // (Input.GetTouch(0).position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, mask))
        {
            // Resets the highlights before drawing new ones
            clearHighlights();
            dragStartBlock.setHighlighted(true);

            dragEndBlock = hit.transform.gameObject.GetComponent<LetterBlock>();
            if (dragEndBlock != dragStartBlock)
            {
                // Determines which direction the user is dragging
                int hOffset = dragEndBlock.col - dragStartBlock.col;
                int vOffset = dragEndBlock.row - dragStartBlock.row;
                if (Mathf.Abs(hOffset) > Mathf.Abs(vOffset))
                {
                    dragDirection = new Vector2(Mathf.Sign(hOffset), 0);
                }
                if (Mathf.Abs(hOffset) < Mathf.Abs(vOffset))
                {
                    dragDirection = new Vector2(0, Mathf.Sign(vOffset));
                }

                // Highlights the line of blocks between dragBlock and hitBlock
                if (dragDirection.x != 0)
                {
                    // Dragging horizontally
                    int startIndexH = Mathf.Min(dragEndBlock.col, dragStartBlock.col);
                    int endIndexH = Mathf.Max(dragEndBlock.col, dragStartBlock.col);
                    for (int i = startIndexH; i <= endIndexH; i++)
                    {
                        letterBlocks[dragStartBlock.row, i].setHighlighted(true);
                    }
                }
                else
                {
                    // Dragging vertically
                    int startIndexV = Mathf.Min(dragEndBlock.row, dragStartBlock.row);
                    int endIndexV = Mathf.Max(dragEndBlock.row, dragStartBlock.row);
                    for (int i = startIndexV; i <= endIndexV; i++)
                    {
                        letterBlocks[i, dragStartBlock.col].setHighlighted(true);
                    }
                }
            }
        }
    }

    // At the end of a drag event, handles cleaning up the grid and dealing
    // with the selected word. Clears highlights if the word is less than the
    // min word length, but keeps highlights if the word is at least the min word
    // length.
    private void resolveDrag()
	{
        string selection = getSelectedWord();
        if (selection.Length < minWordLength /*|| !isSelectionConnected()*/ || capitalization == -1)
        {
            clearHighlights();
        }

        // The mouse was released from a drag event
        dragging = false;
    }

    // Uses the drag direction and highlighted blocks in the grid
    // to determine what the selected word is. Also resets the capitalized
    // flag
    private string getSelectedWord()
	{
        capitalization = 0;
        string selection = "";
        if (dragDirection.x != 0)
        {
            // Dragging horizontally
            int startIndexH = Mathf.Min(dragEndBlock.col, dragStartBlock.col);
            int endIndexH = Mathf.Max(dragEndBlock.col, dragStartBlock.col);
            // Reads the letters of the blocks between startIndexH and endIndexH
            // from left to right
            for (int i = startIndexH; i <= endIndexH; i++)
            {
                selection += letterBlocks[dragStartBlock.row, i].getLetter();
                if (letterBlocks[dragStartBlock.row, i].upper)
				{
                    if (i == startIndexH)
                        capitalization = 1;
                    else
                        capitalization = -1;
				}
            }
        }
        else
        {
            // Dragging vertically
            int startIndexV = Mathf.Min(dragEndBlock.row, dragStartBlock.row);
            int endIndexV = Mathf.Max(dragEndBlock.row, dragStartBlock.row);
            // Reads the letters of the blocks between startIndexV and endIndexV
            // from the top down
            for (int i = endIndexV; i >= startIndexV; i--)
            {
                selection += letterBlocks[i, dragStartBlock.col].getLetter();
                if (letterBlocks[i, dragStartBlock.col].upper)
                {
                    if (i == endIndexV)
                        capitalization = 1;
                    else
                        capitalization = -1;
                }
            }
        }
        return selection;
    }

    // Initializes the grid by instantiating all the blocks
    private void initialize()
    {
        if (!initialized)
        {
            instantiateBlocks();
            initialized = true;
        }
    }

    // Uninitializes the grid by destroying all the blocks
    private void uninitialize()
    {
        if (initialized)
        {
            destroyBlocks();
            initialized = false;
        }
    }

    // Generates a string of s_width*s_height random letters based off of their frequencies in
    // letterFrequencies.
    private string generateString(int s_width, int s_height)
    {
        // We will be doing a lot of concatination, so a string builder is faster
        StringBuilder builder = new StringBuilder();
        // Generates the whole string, storing it in builder
        for (int r = 0; r < s_height; r++)
        {
            // Generates one line of the string
            for (int c = 0; c < s_width; c++)
            {
                // Picks a random number between 0 and 100, because the values
                // in letterFrequencies add up to 100.
                float rand = Random.Range(0.00001f, 100f);
                float sum = 0;
                int letterIndex = 0;
                // Sums up the frequencies of the letters until rand is less than
                // the sum, which effectively turns letterIndex into a randomly
                // generated value 1-26 corresponding to the letters of the alphabet,
                // with each letter having a probability to be chosen based on the
                // values in letterFrequencies.
                while (rand > sum)
                {
                    sum += letterFrequencies[letterIndex];
                    letterIndex++;
                }
                // The character corresponding to 1 is set to be 'a', and the following
                // 25 characters are chosen by adding to the ascii value of 'a'.
                char letter = 'a';
                letter += (char)(letterIndex - 1);
                builder.Append(letter);
            }
            // Adds an EOL character after a row has been generated
            //builder.Append("\n");
        }
        return builder.ToString();
    }

    // Instantiates the grid of width*height letter blocks from the bottom up, with the bottom row
    // being centered on this gameobject.
    private void instantiateBlocks()
    {
        letterBlocks = new LetterBlock[height, width];
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                Vector3 blockPos = new Vector3(transform.position.x + (0.5f + c - (width / 2.0f)) * letterBlockPrefab.transform.localScale.x,
                                               transform.position.y + r * letterBlockPrefab.transform.localScale.y,
                                               transform.position.z);
                letterBlocks[r, c] = Instantiate(letterBlockPrefab, blockPos, Quaternion.identity, transform).GetComponent<LetterBlock>();
                letterBlocks[r, c].setPosition(r, c);
                
            }
        }
    }

    // Locks a percentage of the blocks in the grid
    private void lockBlocks()
	{
        HashSet<int> lockedBlocks = new HashSet<int>();
        while (lockedBlocks.Count < (int)(width * height * lockedBlockPercentage))
        {
            lockedBlocks.Add(Random.Range(0, width * height));
        }

        int i = 0;
        foreach(LetterBlock lb in letterBlocks)
		{
            lb.setLocked(false);
            if (lockedBlocks.Contains(i))
            {
                lb.setLocked(true);
            }
            i++;
		}
    }

    // Iterates through the letterblocks in the grid and sets their characters to the ones contained in text
    private void paintBlocks(string text)
    {
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                letterBlocks[r, c].setLetter(text[r * width + c]);
            }
        }
    }

    // Iterates through the letterblocks in the grid and sets their characters to the ones contained in text,
    // and sets the state of the letter block based on the flag after the character.
    // FLAGS:
    // d = default
    // s = solved;
    // l = locked;
    // c = locked and solved
    private void paintBlocksAdvanced(string text)
	{
        int i = 0;
        char letter;
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                letter = text[i];
                letterBlocks[r, c].setLetter(letter.ToString().ToLower()[0]);
                letterBlocks[r, c].setUpper(letter == '\t' || (letter <= 'Z' && letter >= 'A'));
                switch (text[i + 1])
				{
                    case 'd':
                        break;
                    case 's':
                        letterBlocks[r, c].setSolved(true);
                        break;
                    case 'l':
                        letterBlocks[r, c].setLocked(true);
                        break;
                    case 'c':
                        letterBlocks[r, c].setSolved(true);
                        letterBlocks[r, c].setLocked(true);
                        break;
                }
                i += 2;
            }
        }
    }

    // Resets the state of the grid to how it was when it was first initialized
    private void resetBlocks()
	{
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                letterBlocks[r, c].reset();
            }
        }
    }

    // Resets the score to 0
    private void resetScore()
	{
        Scoreboard.Instance.setScore(0);
	}

    // Destroys all the blocks in the grid
    private void destroyBlocks()
    {
        foreach (LetterBlock lb in letterBlocks)
        {
            Destroy(lb.gameObject);
            Debug.Log("destroyed letter block");
        }
    }

    // Removes the highlight from all of the letterblocks in the grid
    private void clearHighlights()
	{
        foreach(LetterBlock lb in letterBlocks)
		{
            lb.setHighlighted(false);
		}
    }

    // Sets all highlighted letterblocks in the grid to be solved
    private void solveHighlights()
    {
        // The number of points each letter in the word will give.
        int wordLength = getSelectedWord().Length + capitalization;
        // A list of the highlighted blocks
        List<LetterBlock> highlightedBlocks = new List<LetterBlock>();
        // The number of points this word scored
        int wordScore = 0;
        // the order in which the tiles should flip
        int order = 0;

        // traverses the letterblocks in reverse row order so that we can get
        // a good solve animation
        for (int r = height - 1; r >= 0; r--)
        {
            for (int c = 0; c < width; c++)
            {
                LetterBlock lb = letterBlocks[r, c];
                if (lb.highlighted)
                {
                    if (lb.solved)
                    {
                        lb.setScore(0, order);
                    }
                    else
                    {
                        lb.setScore(wordLength, order);
                        wordScore += wordLength;
                        lb.setSolved(true);
                        numSolved++;
                    }
                    order++;
                    highlightedBlocks.Add(lb);
                }
                lb.setHighlighted(false);
            }
        }
        Scoreboard.Instance.addScore(wordScore);

        if (numSolved == width * height)
		{
            Scoreboard.Instance.addScore((int)(Scoreboard.Instance.getScore() * 0.25f));
		}

        saveGame();
    }

    // Returns whether any of the highlighted blocks on the grid
    // are solved.
    private bool isSelectionConnected()
	{
        bool isFirstMove = true;
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (letterBlocks[r, c].solved)
				{
                    isFirstMove = false;
                    if (letterBlocks[r, c].highlighted)
					{
                        return true;
                    }
				}
            }
        }
        return isFirstMove;
    }

    // saves the current state of the game
    private void saveGame()
	{
        string board = "";
        char c;
        char f;
        foreach(LetterBlock lb in letterBlocks)
		{
            c = lb.getLetter();
            // changes c to uppercase if the letterblock uppercase
            if (lb.upper)
			{
                if (c == ' ')
				{
                    c = '\t';
				}
                else
				{
                    c = c.ToString().ToUpper()[0];
                }
			}
            board += c;

            // adds the flag after the letter
            if (lb.locked)
			{
                if (lb.solved)
				{
                    f = 'c';
				}
                else
				{
                    f = 'l';
				}
			}
            else if (lb.solved)
			{
                f = 's';
			}
            else
			{
                f = 'd';
			}
            board += f;
		}
        SaveState state = SaveLoad.Instance.state;
        state.board = board;
        state.wildPlayed = wildPlayed;
        state.score = Scoreboard.Instance.getScore();
        if (state.score > state.highscore)
		{
            state.highscore = state.score;
		}
        SaveLoad.Instance.Save();
    }

    // loads a saved state of the game
    public void loadGame()
	{
        resetBlocks();
        SaveState state = SaveLoad.Instance.state;
        wildPlayed = state.wildPlayed;
        Scoreboard.Instance.setScore(state.score);
        Scoreboard.Instance.setHighscore(state.highscore);
        paintBlocksAdvanced(state.board);
	}

    private string getSimpleState()
	{
        StringBuilder builder = new StringBuilder();
        foreach (LetterBlock lb in letterBlocks)
		{
            builder.Append(lb.getLetter());
		}
        return builder.ToString();
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
