using System;
using System.IO;

namespace minesweeper
{
    class Program
    {
        // variables for the position of the cursor
        public int currentLeft;
        public int currentTop;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.play();   
        }

        public void play()
        {
            Console.Clear();
            // height and width of the board and the number of mines on the board
            int height = 0, width = 0, numMines = 0;

            Console.WriteLine("Please select the level of difficulty");
            Console.WriteLine("1) Easy");
            Console.WriteLine("2) Normal");
            Console.WriteLine("3) Hard\n");
            Console.Write("Enter: ");

            int choice = 0;
            
            // take and validate input for the difficulty level
            while (true)
            {
                int.TryParse(Console.ReadLine(), out choice);
                if (choice < 1 || choice > 3)
                {
                    Console.Write("Please Select a valid option: ");
                    continue;
                }
                break;
            }

            // set width, height and numMines according to the difficulty level selected
            switch(choice)
            {
                // easy
                case 1:
                    height = width = 9;
                    numMines = 10;
                    break;
                // normal
                case 2:
                    height = width = 16;
                    numMines = 40;
                    break;
                // hard
                case 3:
                    height = 16;
                    width = 30;
                    numMines = 99;
                    break;
            }

            // make a board with according to the difficulty level
            Board b = new Board(width, height, numMines);
            Program p = new Program();

            // make a highscore object and pass height and width of the board 
            // to find the position to display score box on the screen
            Highscores h = new Highscores(height, width);

            // clear the console
            Console.Clear();

            // set the cursot position to (0,0)
            p.currentLeft = 0;
            p.currentTop = 0;

            // print the hidden board
            b.printHiddenBoard();

            // Print instructions for the user
            Console.WriteLine("\n\n");
            Console.WriteLine("Use arrow keys to move the cursor");
            Console.WriteLine("Press \"Enter\" to reveal a panel");
            Console.WriteLine("Press \"F\" to flag or unflag a panel");
            Console.WriteLine("Press \"R\" to Reset the game");
            Console.WriteLine("Press \"ESC\" to Exit");
            

            // set the cursor position to display scores
            Console.SetCursorPosition(0, b.height + 6);

            // display the scores
            h.displayHighscores();

            Console.SetCursorPosition(0, 0);

            // main game loop
            while (true)
            {
                // read the key that the user presses
                ConsoleKey key = Console.ReadKey(true).Key;
                
                // if key is R reset the game and break the loop
                if (key == ConsoleKey.R)
                {
                    play();
                    break;
                }

                // if key = ESC, quit the program and break the loop
                if (key == ConsoleKey.Escape)
                {
                    Console.SetCursorPosition(0, 25);
                    break;
                }

                // for any other key call the performAction function of the board class
                // which returns true if player has won the game
                else if (b.performAction(key, ref p.currentTop, ref p.currentLeft))
                {
                    // print that the player has won
                    b.printWon();

                    // get score of the player
                    int score = b.getScore();

                    // display score of the player
                    Console.WriteLine(score);

                    // add score to the file
                    h.addHighScore(score);
                }
            }

            Console.ResetColor();
        }
    }


    // class for working with board
    class Board
    {
        public int width; // width of the board
        public int height; // height of the board
        public char[,] board; // 2D array for holding the board
        public char[,] tempBoard; // a temp array to hold hidden board
        public bool[,] hidden; // 2D array for checking if a tile is hidden or not

        // game is started or not
        bool started;

        // number of hidden tiles remaining on the board
        int hiddenTiles;
        int numMines; // number of mines on the board

        // start and end time of the game to find the score
        DateTime startTime, endTime;

        // constructor receive width, height and number of mines on the board
        public Board(int width, int height, int numMines)
        {
            // initializing the variables
            this.width = width;
            this.height = height;

            // hidden tiles in start are equal to total tiles minus number of mines on the board
            hiddenTiles = (width * height) - numMines;

            // initializing the arrays
            board = new char[height, width];
            tempBoard = new char[height, width];
            hidden = new bool[height, width];

            // initializing the board
            initializeBoard(numMines);
        }

        // intitialize the board by adding mines and numbers on the board
        public void initializeBoard(int numMines)
        {

            this.numMines = numMines;

            // make a random variable to get random places to place the mines
            Random rand = new Random();
            int top = rand.Next(0, height - 1), left = rand.Next(0, width - 1);

            // loop for all the mines
            for (int i = 0; i < numMines; i++)
            {
                // if there is already a mine on the location then repeat the iteration
                if (board[top, left] == 'X')
                    i--;


                // place the mine on the board
                board[top, left] = 'X';

                // find random position on the board to place the mine
                top = rand.Next(0, height - 1);
                left = rand.Next(0, width - 1);
            }
            // placing of mines finished


            // after placing all the mines place the numbers on the board
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // initialize temp board with all #'s
                    tempBoard[i, j] = '#';

                    // set hidden to true for every tile
                    hidden[i, j] = true;

                    // to count the number of mines arround a tile
                    int count = 0;

                    // if the tile is not a mine then count the number for it
                    if (board[i, j] != 'X')
                    {
                        // check for all 8 adjacent sides for mines and increment the count if a mine is found

                        // bottom
                        if (i + 1 < height)
                            if (board[i + 1, j] == 'X')
                                count++;
                        // top
                        if (i - 1 >= 0)
                            if (board[i - 1, j] == 'X')
                                count++;

                        // right
                        if (j + 1 < width)
                            if (board[i, j + 1] == 'X')
                                count++;

                        // left
                        if (j - 1 >= 0)
                            if (board[i, j - 1] == 'X')
                                count++;

                        // bottom right
                        if (i + 1 < height && j + 1 < width)
                            if (board[i + 1, j + 1] == 'X')
                                count++;

                        // bottom left
                        if (i + 1 < height && j - 1 >= 0)
                            if (board[i + 1, j - 1] == 'X')
                                count++;

                        // top right
                        if (i - 1 >= 0 && j + 1 < width)
                            if (board[i - 1, j + 1] == 'X')
                                count++;

                        // top left
                        if (i - 1 >= 0 && j - 1 >= 0)
                            if (board[i - 1, j - 1] == 'X')
                                count++;

                        // if count is greater then zero then put it on the board
                        if (count > 0)
                            board[i, j] = (char)(count + '0');
                        // else place a dash on the board
                        else
                            board[i, j] = '-';
                    }
                }
            }
            // placing of numbers finished

        }

        // print the hidden tiles
        public void printHiddenBoard()
        {
            // set the background and foreground color
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;

            // display the hidden board
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Console.Write(tempBoard[i, j]);
                }
                Console.WriteLine("");
            }

            // reset the background and foreground colors
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        // print the orignal board
        public void printBoard()
        {
            // print the orignal board
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // function to print the tile according to defined color
                    printBoard(i, j);
                }
                Console.WriteLine("");
            }

            // reset the colors
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        // function to get the score of the player if he won the game
        public int getScore()
        {
            int score = 0;
            // find the time between the start and end of the game
            TimeSpan span = endTime - startTime;

            // 10 seconds = 1 point
            // subtract the score from 1000 so that the heigher score means earlier finish
            score = 1000 - (int)(span.TotalSeconds / 10);

            // return the score
            return score;
        }

        // key is the key pressed
        // currentTop and currentLeft is the top and left position of the cursor
        public bool performAction(ConsoleKey key, ref int currentTop, ref int currentLeft)
        {
            // if up arrow is pressed move the currsor up
            if (key == ConsoleKey.UpArrow)
            {
                if (currentTop - 1 >= 0)
                    Console.SetCursorPosition(currentLeft, --currentTop);
            }

            // if down arrow is pressed move the currsor down
            if (key == ConsoleKey.DownArrow)
            {
                if (currentTop + 1 < height)
                    Console.SetCursorPosition(currentLeft, ++currentTop);
            }

            // if left arrow is pressed move the currsor left
            if (key == ConsoleKey.LeftArrow)
            {
                if (currentLeft - 1 >= 0)
                    Console.SetCursorPosition(--currentLeft, currentTop);
            }

            // if right arrow is pressed move the currsor right
            if (key == ConsoleKey.RightArrow)
            {
                if (currentLeft + 1 < width)
                    Console.SetCursorPosition(++currentLeft, currentTop);
            }

            // if enter is pressed reveal the tile
            if (key == ConsoleKey.Enter)
            {
                // if game is not started start it and store the starting time
                if (!started)
                {
                    started = true;

                    // start time = current time
                    startTime = DateTime.Now;
                }

                // reveal the tile
                reveal(currentTop, currentLeft);

                // restore the cursor position
                Console.SetCursorPosition(currentLeft, currentTop);
            }

            // if F key is pressed
            if (key == ConsoleKey.F)
            {
                // if the tile is hidden then flag it
                if (hidden[currentTop, currentLeft])
                {
                    hidden[currentTop, currentLeft] = false;
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("F");
                    tempBoard[currentTop, currentLeft] = 'F';
                }

                // if it is already flaged then unflag it
                else if (!hidden[currentTop, currentLeft] && tempBoard[currentTop, currentLeft] == 'F')
                {
                    hidden[currentTop, currentLeft] = true;
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("#");
                    tempBoard[currentTop, currentLeft] = '#';
                }

                // restore the cursor position
                Console.SetCursorPosition(currentLeft, currentTop);
            }

            // return if the player won the game or not
            return winner();
        }

        // print to the screen if the player lost the game
        public void printLost()
        {
            Console.SetCursorPosition(width + 7, 18);
            Console.WriteLine("You have stepped on a mine and have lost the game...!!!");

            // set started to false because the game is now ended
            started = false;
        }

        // print to the screen if the player won the game
        public void printWon()
        {
            // set the end time of the game to calculate score
            endTime = DateTime.Now;
            Console.SetCursorPosition(width + 7, 18);
            Console.WriteLine("Congratulations You have won the game...!!!");

            // set started to false becuse the game is ended
            started = false;
        }

        // returns true if player won the game
        public bool winner()
        {
            // if there are no hidden tiles on the board return true
            if (hiddenTiles == 0)
                return true;

            // else return false
            return false;
        }

        public void printBoard(int x, int y)
        {
            // set the cursor position
            Console.SetCursorPosition(y, x);
            Console.BackgroundColor = ConsoleColor.Gray;

            // set the color according to the tile
            if (board[x, y] == '1')
                Console.ForegroundColor = ConsoleColor.DarkBlue;
            if (board[x, y] == '2')
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (board[x, y] == '3')
                Console.ForegroundColor = ConsoleColor.DarkRed;
            if (board[x, y] == '4')
                Console.ForegroundColor = ConsoleColor.Blue;
            if (board[x, y] == '-')
                Console.ForegroundColor = ConsoleColor.Black;
            if (board[x, y] == 'X')
                Console.ForegroundColor = ConsoleColor.Red;

            // print the tile content to the screen
            Console.Write(board[x, y]);

            // restore the colors
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        // reveal what is under a tile
        public bool reveal(int posX, int posY)
        {
            // set the tile to not hidden
            hidden[posX, posY] = false;

            // if the tile on which the player clicked is a mine
            if (board[posX, posY] == 'X')
            {
                // print the complete board
                Console.SetCursorPosition(0, 0);
                printBoard();

                // tell the player that he lost the game
                printLost();
                return true;
            }

            // if tile is a number then reduce the number of hidden tiles and print the tile to the screen
            if (board[posX, posY] >= '0' && board[posX, posY] <= '8')
            {
                hiddenTiles--;
                printBoard(posX, posY);
                return false;
            }

            // if it is neither a number nor a mine
            else
            {
                int i = posX, j = posY;

                // reduce the number of hidden tiles
                hiddenTiles--;

                // print the given tile
                printBoard(posX, posY);

                // and do the same for all adjacent tiles that are not hidden and are not mines
                if (i + 1 < height)
                    if (board[i + 1, j] != 'X' && hidden[i + 1, j])
                    {
                        hidden[i + 1, j] = false;
                        reveal(i + 1, j);
                    }

                if (i - 1 >= 0)
                    if (board[i - 1, j] != 'X' && hidden[i - 1, j])
                    {
                        hidden[i - 1, j] = false;
                        reveal(i - 1, j);
                    }

                if (j + 1 < width)
                    if (board[i, j + 1] != 'X' && hidden[i, j + 1])
                    {
                        hidden[i, j + 1] = false;
                        reveal(i, j + 1);
                    }

                if (j - 1 >= 0)
                    if (board[i, j - 1] != 'X' && hidden[i, j - 1])
                    {
                        hidden[i, j - 1] = false;
                        reveal(i, j - 1);
                    }

                if (i + 1 < height && j + 1 < width)
                    if (board[i + 1, j + 1] != 'X' && hidden[i + 1, j + 1])
                    {
                        hidden[i + 1, j + 1] = false;
                        reveal(i + 1, j + 1);
                    }

                if (i + 1 < height && j - 1 >= 0)
                    if (board[i + 1, j - 1] != 'X' && hidden[i + 1, j - 1])
                    {
                        hidden[i + 1, j - 1] = false;
                        reveal(i + 1, j - 1);
                    }

                if (i - 1 >= 0 && j + 1 < width)
                    if (board[i - 1, j + 1] != 'X' && hidden[i - 1, j + 1])
                    {
                        hidden[i - 1, j + 1] = false;
                        reveal(i - 1, j + 1);
                    }

                if (i - 1 >= 0 && j - 1 >= 0)
                    if (board[i - 1, j - 1] != 'X' && hidden[i - 1, j - 1])
                    {
                        hidden[i - 1, j - 1] = false;
                        reveal(i - 1, j - 1);
                    }
                return false;
            }

        }
    }



    // class for saving and retriving highscores
    class Highscores
    {
        string[] names; // array to hold names of all highscore holders
        int[] scores; // array to hold scores of all highscore holders
        int numRecords; // num of records
        int numLines; // num of records present in the file
        int height;  // height of the board
        int width;  // width of the board

        // constructor takes 2 arguments the height and width of the board
        // to find the position to display the score board
        public Highscores(int height, int width)
        {
            this.height = height;
            this.width = width;

            numRecords = 5;

            // open or create the file to make sure we have a file to read from
            FileStream f = new FileStream("highscores.txt", FileMode.Append);
            f.Close(); // close the file

            // check the number of line in the file
            numLines = File.ReadAllLines("highscores.txt").Length;

            // initialize the name and score arrays to the size of number of records
            names = new string[numRecords];
            scores = new int[numRecords];

            // read scores from the file
            readScoresFromFile();
        }

        // read scores from the file and stores them into arrays
        public void readScoresFromFile()
        {
            // open the file to read from
            FileStream f = new FileStream("highscores.txt", FileMode.Open, FileAccess.Read);

            // make a stream reader object
            StreamReader sr = new StreamReader(f);

            // read all the records into arrays
            for (int i = 0; i < numRecords; i++)
            {
                // if there is a record
                if (numLines >= i * 2)
                {
                    names[i] = sr.ReadLine();
                    int.TryParse(sr.ReadLine(), out scores[i]);
                }

                // if there is no record present
                else
                {
                    names[i] = "NaN";
                    scores[i] = 0;
                }
            }

            // close the file
            f.Close();
        }

        // display the score board on the screen
        public void displayHighscores()
        {
            Console.SetCursorPosition(width + 7, 0);
            Console.WriteLine("----------------------------");
            Console.SetCursorPosition(width + 7, 1);
            Console.WriteLine("*        HIGHSCORES        *");
            Console.SetCursorPosition(width + 7, 2);
            Console.WriteLine("----------------------------");
            Console.SetCursorPosition(width + 7, 3);
            Console.WriteLine("* Name              Score  *");
            Console.SetCursorPosition(width + 7, 4);
            Console.WriteLine("----------------------------");

            for (int i = 0; i < numRecords; i++)
            {
                Console.SetCursorPosition(width + 7, 5 + i);
                Console.Write("* " + names[i]);
                Console.SetCursorPosition(width + 27, 5 + i);
                Console.Write(scores[i]);
                Console.SetCursorPosition(width + 34, 5 + i);
                Console.Write("*");
            }

            Console.SetCursorPosition(width + 7, 10);
            Console.WriteLine("----------------------------");
        }

        // return the min score in the file
        public int getMinScore()
        {
            // set min to first score in the record
            int min = scores[0];

            // loop through all the records
            for (int i = 1; i < numRecords; i++)
            {
                // if a smaller value is found set min to that value
                if (scores[i] < min)
                    min = scores[i];
            }

            // return min
            return min;
        }

        // add a high score to the file if it is in top 5 scores
        public void addHighScore(int score)
        {
            // if it is in top 5 heighest scores
            if (score > getMinScore())
            {
                string name;

                // loop through all the scores to find its position
                for (int i = 0; i < numRecords; i++)
                {
                    // if player score is greater then the score
                    if (scores[i] < score)
                    {
                        // ask for player name
                        Console.Write("Name: ");

                        // save the name in variable
                        name = Console.ReadLine();

                        // save the score in score array
                        scores[i] = score;

                        // save the name in names array
                        names[i] = name;

                        // break the loop
                        break;
                    }
                }
            }

            // open the file to write the updated scores to the file
            FileStream f = new FileStream("highscores.txt", FileMode.Open, FileAccess.Write);
            StreamWriter sw = new StreamWriter(f);

            // write the updated score to the file
            for (int i = 0; i < 5; i++)
            {
                sw.WriteLine(names[i]);
                sw.WriteLine(scores[i]);
            }

            // flush the writer object and close the writer object and file
            sw.Flush();
            sw.Close();
            f.Close();
        }
    }
}
