using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    //enum o,x or empty
    enum Field
    {
        O,
        X,
        Empty
    }
    class TicTacToe
    {
        private Field[,] board = new Field[3,3];
        private Field lastplaced;
        private int moveCounter = 0;
        public TicTacToe()
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = Field.Empty;
                }
            }
            lastplaced = Field.O;
        }
        public Field getField(int collum,int row)
        {
            return board[row, collum];
        }

        public bool isTie() {
            if (moveCounter >= 9 && !win(Field.X) && !win(Field.O)) {
                return true;
            }

            return false;
        }

        public Tuple<bool,bool> setField(int collum, int row,Field placing)
        {
            Debug.Print(collum+" "+ row);
            if (placing == lastplaced || board[row,collum]!=Field.Empty)
            {
                return new Tuple<bool,bool>(false,false);
            }
            board[row, collum] = placing;
            lastplaced = placing;
            moveCounter++;
            bool won = win(placing);
            return new Tuple<bool, bool>(won,true);
        }
        
        private bool win(Field possibleWinner)
        {
            int diagonal = 0;
            int antidiagonal = 0;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                int counter = 0;
                int transposeCounter = 0;
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == possibleWinner)
                    {
                        counter++;
                        //Debug.Print("why" + counter);
                    }
                    if (board[j, i] == possibleWinner)
                    {
                        transposeCounter++;
                    }
                    if (i == j && board[i, j] == possibleWinner)
                    {
                        diagonal++;
                    }
                    if (i == board.GetLength(1) - 1 - j && board[i,j]==possibleWinner) 
                    {
                        antidiagonal++;
                    }
                }
                if (counter == 3 || transposeCounter == 3||diagonal==3||antidiagonal==3)
                {
                    Debug.Print(counter + " " + transposeCounter + " " + diagonal + " " + antidiagonal);
                    return true;
                }
            }
          
            
            return false;
        }
    }
    
}
