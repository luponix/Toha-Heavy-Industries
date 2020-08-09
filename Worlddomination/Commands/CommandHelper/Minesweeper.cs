using System;
using System.Collections.Generic;
using System.Text;

namespace Worlddomination.Commands
{
    class Minesweeper
    {
        private int x;
        private int y;
        private int bombs;

        public Minesweeper(int x_, int y_, int bombs_)
        {
            x = x_;
            y = y_;
            bombs = bombs_;
        }

        public string GenerateMinesweeperField()
        {

            int[,] values = new int[x, y];
            string[,] result = new string[x, y];

            var rand = new Random();
            for (int i = 0; i < bombs; i++)
            {
                int randX = rand.Next(x);
                int randY = rand.Next(y);
                values[randX, randY] = 9; // creating the bombs

                for (int a = -1; a < 2; a++) // iterating over the surrounding fields
                {
                    if (randX + a >= 0 && randX + a < x)
                    {
                        for (int b = -1; b < 2; b++)
                        {
                            if (randY + b >= 0 && randY + b < y)
                            {
                                values[randX + a, randY + b]++;
                            }
                        }
                    }

                }
            }


            // translating the array into a string array
            for (int i = 0; i < x; i++)
            {
                for (int o = 0; o < y; o++)
                {
                    result[i, o] = translate(values[i, o]);
                }
            }

            // converting the array into one string with several lines
            string erg = "";

            for (int i = 0; i < y; i++)
            {
                for (int o = 0; o < x; o++)
                {
                    erg += result[o, i];
                }
                erg += "\n";
            }

            return erg;



        }

        private string translate(int number)
        {
            switch (number)
            {
                case 0:
                    return "||:zero:||";
                case 1:
                    return "||:one:||";
                case 2:
                    return "||:two:||";
                case 3:
                    return "||:three:||";
                case 4:
                    return "||:four:||";
                case 5:
                    return "||:five:||";
                case 6:
                    return "||:six:||";
                case 7:
                    return "||:seven:||";
                case 8:
                    return "||:eight:||";
                default:
                    return "||:bomb:||";
            }
        }
    }
}
