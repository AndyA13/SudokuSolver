namespace Sudoku
{
    using System.Linq;

    public class Program
    {
        private static Position[,] _board = new Position[9, 9];
        private static int _selectedX;
        private static int _selectedY;


        public static void Main(string[] args)
        {
            InitBoard();
            PopulateBoard();

            bool exit = false;

            while (!exit)
            {
                Console.WriteLine();
                Console.WriteLine("1. Solve (simple)");
                Console.WriteLine("2. Solve (regressive)");
                Console.WriteLine("3. Edit Board");
                Console.WriteLine("4. Reset");
                Console.WriteLine("5. Exit");

                Console.Write("Select: ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        Solve();
                        break;

                    case "2":
                        SolveRegressive();
                        break;
                    
                    case "3":
                        PopulateBoard();
                        break;
                    
                    case "4":
                        InitBoard();
                        break;

                    case "5":
                        exit = true;
                        break;
                }
            }
        }

        public static void InitBoard()
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    _board[x, y] = new Position(x, y, 0);
                }
            }
        }

        public static void PopulateBoard()
        {
            while (true)
            {
                DrawBoard();
                var position = _board[_selectedX, _selectedY];
                Console.WriteLine($"{_selectedX} , {_selectedY} - {position.X}, {position.Y}");
                Console.WriteLine("Press esc to return to menu");

                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape)
                {
                    return;
                }

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        _selectedX = _selectedX == 0 ? 8 : _selectedX - 1;
                        break;

                    case ConsoleKey.RightArrow:
                        _selectedY = _selectedY == 8 ? 0 : _selectedY + 1;
                        break;

                    case ConsoleKey.DownArrow:
                        _selectedX = _selectedX == 8 ? 0 : _selectedX + 1;
                        break;

                    case ConsoleKey.LeftArrow:
                        _selectedY = _selectedY == 0 ? 8 : _selectedY - 1;
                        break;
                }

                if (Char.IsNumber(key.KeyChar))
                {
                    _board[_selectedX, _selectedY].Value = int.Parse(key.KeyChar.ToString());
                }
            }
        }

        public static void DrawBoard()
        {
            Console.Clear();
            Console.WriteLine("  -  -  -   -  -  -   -  -  - ");

            for (int x = 0; x < 9; x++)
            {
                if (x == 3 || x == 6)
                {
                    Console.WriteLine("| -  -  -   -  -  -   -  -  - |");
                }

                Console.Write("|");

                for (int y = 0; y < 9; y++)
                {
                    if (y > 0 && y % 3 == 0)
                    {
                        Console.Write("|");
                    }

                    if (_selectedX == x && _selectedY == y)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.Gray;
                    }

                    Console.Write(_board[x, y].Value == 0 ? "   " : " " + _board[x,y].Value + " ");

                    Console.ResetColor();
                }

                Console.WriteLine("|");
            }
            Console.WriteLine("  -  -  -   -  -  -   -  -  - ");

        }

        // Solves the puzzle only using valid solutions given the row, column and box
        // No advanced methods or guessing
        public static void Solve()
        {
            int cycles = 0;
            int positionsSolved = _board.Flattened().Count(x => x.Value > 0);

            int lastCycleWherePositionWasSolved = 0;
            int lastNumberOfPositionsSolved = 0;

            while (positionsSolved < 81)
            {
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        if (_board[x, y].Value == 0)
                        {
                            int answer = SolvePosition(x, y);

                            if (answer > 0)
                            {
                                _board[x, y].Value = answer;
                            }
                        }
                    }
                }

                DrawBoard();

                positionsSolved = _board.Flattened().Count(x => x.Value > 0);

                if (positionsSolved > lastNumberOfPositionsSolved)
                {
                    lastNumberOfPositionsSolved = positionsSolved;
                    lastCycleWherePositionWasSolved = cycles;
                }
                else if (cycles - lastCycleWherePositionWasSolved > 3)
                {
                    Console.WriteLine($"No positions solved in last 3 cycles");
                    Console.WriteLine($"Stopping on cycle {cycles}");
                    break;
                }

                cycles++;

                if (cycles > 500)
                {
                    Console.WriteLine("Stopping, reached 1,000 cycles");
                    break;
                }
            }

            Console.WriteLine($"Solved in {cycles} cycles");
            Console.WriteLine("Finished -- Press enter");
            Console.ReadLine();
        }

        // Gets a solution if there is only one possible answer
        public static int SolvePosition(int x, int y)
        {
            var possibleAnswers = GetValidSolutionsForPosition(x, y);

            if (possibleAnswers.Count == 1)
            {
                return possibleAnswers.Single();
            }

            return 0;
        }

        // Gets all valid numbers for a given position
        private static List<int> GetValidSolutionsForPosition(int x, int y)
        {
            List<int> answers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9} ;

            // Row
            for (int col = 0; col < 9; col++)
            {
                int value = _board[x, col].Value;

                if (value > 0 && answers.Contains(value))
                {
                    answers.Remove(value);
                }
            }

            // Column
            for (int row = 0; row < 9; row++)
            {
                int value = _board[row, y].Value;

                if (value > 0 && answers.Contains(value))
                {
                    answers.Remove(value);
                }
            }

            // Square
            var xrange = GetBox(x);
            var yrange = GetBox(y);

            for (int xb = xrange.Item1; xb <= xrange.Item2; xb++)
            {
                for (int yb = yrange.Item1; yb <= yrange.Item2; yb++)
                {
                    int value = _board[xb, yb].Value;

                    if (value > 0 && answers.Contains(value))
                    {
                        answers.Remove(value);
                    }
                }
            }

            return answers;
        }

        // Basically brute forcing with some respect to valid solutions
        public static void SolveRegressive()
        {
            var unsolvedPositions = _board.Flattened().Where(x => x.Value == 0).ToList();

            int index = 0;
            int lastPosition = unsolvedPositions.Count - 1;
            int cycles = 0;
            bool exit = false;

            while (!exit)
            {
                var position = unsolvedPositions[index];

                if (SolveRegressivePosition(position))
                {
                    cycles++;
                    DrawBoard();

                    if (index == lastPosition)
                    {
                        Console.WriteLine($"Puzzle solved in {cycles} cycles");
                        exit = true;
                    }
                    else
                    {
                        index++;
                    }
                }
                else
                {
                    if (index == 0)
                    {
                        Console.WriteLine($"Failed to solve with regressive function");
                        exit = true;
                    }
                    else
                    {
                        index--;
                    }
                }
            }
        }

        private static bool SolveRegressivePosition(Position position)
        {
            var validSolutions = GetValidSolutionsForPosition(position.X, position.Y);

            // If we have a possible solution that is greater than the current value
            if (validSolutions.Any() && validSolutions.Max() > position.Value)
            {
                position.Value = validSolutions.Where(x => x > position.Value).OrderBy(x => x).First();
                return true;
            }

            // No valid solution
            // Reset to 0 and go back a position
            position.Value = 0;
            return false;
        }

        // Gets the index bounds for the Sudoku box at the given index
        private static Tuple<int, int> GetBox(int index)
        {
            if (index < 3)
            {
                return new Tuple<int, int>(0, 2);
            }

            if (index < 6)
            {
                return new Tuple<int, int>(3, 5);
            }

            return new Tuple<int, int>(6, 8);
        }
    }
}
