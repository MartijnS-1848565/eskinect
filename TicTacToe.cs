using System;
using System.Collections.Generic;
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
        public Field getField(int row,int collum)
        {
            return board[row, collum];
        }
        public Tuple<bool,bool> setField(int row, int collum,Field placing)
        {
            if (placing == lastplaced)
            {
                return new Tuple<bool,bool>(false,false);
            }
            board[row, collum] = placing;
            lastplaced = placing;
            bool won = win(placing);
            return new Tuple<bool, bool>(won,true);
        }
        public bool win(Field possibleWinner)
        {
            int diagonal = 0;
            int antidiagonal = 0;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                int counter = 0;
                int transposeCounter = 0;
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i,j]==possibleWinner)
                    {
                        counter++;
                    }
                    if (board[j,i]==possibleWinner)
                    {
                        transposeCounter++;
                    }
                    if (i == j && board[i,j]==possibleWinner)
                    {
                        diagonal++;
                    }
                    if (i == board.GetLength(1) - 1 - j);
                    {
                        antidiagonal++;
                    }
                }
                if (counter == 3||transposeCounter==3||diagonal==3||antidiagonal==3)
                {
                    return true;
                }
            }
          
            
            return false;
        }
    }
    
}
