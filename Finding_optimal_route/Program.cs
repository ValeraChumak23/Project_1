using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;

class Program
{
    static void Main(string[] args)
    {
        string directoryPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        string filePath = System.IO.Path.Combine(directoryPath,"Матрица.xlsx"); // Путь к файлу с матрицей смежности

        // Считывание матрицы из файла
        int[,] adjacencyMatrix = ReadAdjacencyMatrixFromExcel(filePath);

        // Вывод матрицы на консоль
        Console.WriteLine("Считанная матрица смежности:");
        PrintMatrix(adjacencyMatrix);
        // Поиск листьев (финальных вершин) в ацикличном-древовидном графе
        Console.WriteLine("\nОпределение конечных вершин и их выиграшей:");
        Dictionary<int, int> final_peaks = new Dictionary<int, int>();
        Dictionary<int, string> Levels = new Dictionary<int, string>();
        List<int> peaks_level_0 = new List<int>();
        Final_peaks(adjacencyMatrix, final_peaks, peaks_level_0);
        // Определение ходов для каждого игрока
        Console.WriteLine("\nОпределение ходов игроков");
        List<string> list_moves_player_1 = new List<string>();
        List<string> list_moves_player_2 = new List<string>();
        Defining_moves(list_moves_player_1, list_moves_player_2, adjacencyMatrix);
        // Алгоритм определения глубин вершин графа
        Console.WriteLine("\nРасставление уровней вершин");
        int[,] new_adjacencyMatrix = CalculateMatrixDepths(adjacencyMatrix, peaks_level_0);
        PrintMatrix(new_adjacencyMatrix);

    }

    static int[,] ReadAdjacencyMatrixFromExcel(string filePath)
    {
        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Count();
            var columns = worksheet.ColumnsUsed().Count();

            // Создаем матрицу
            int[,] matrix = new int[rows - 1, columns - 1];

            for (int i = 2; i <= rows; i++) // Пропускаем заголовок
            {
                for (int j = 2; j <= columns; j++) // Пропускаем заголовок
                {
                    matrix[i - 2, j - 2] = int.Parse(worksheet.Cell(i, j).Value.ToString());
                }
            }

            return matrix;
        }
    }

    static void PrintMatrix(int[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    static void Final_peaks(int[,] matrix, Dictionary<int, int> final_peaks, List<int> level_0)
    {
        Random random = new Random();
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            int number_of_0_transitions = 0;

            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j] == 0)
                {
                    number_of_0_transitions += 1;
                }
            }
            if (number_of_0_transitions == matrix.GetLength(1))
            {
                int winning_sum = random.Next(1, 10);
                final_peaks.Add(i, winning_sum);
                level_0.Add(i);
                Console.WriteLine($"Вершина {i} конечная и имеет выигрыш {winning_sum}");
            }
        }
    }

    static void Defining_moves(List<string> player_1, List<string> player_2, int[,] matrix)
    {
        List<string> player_1_coordinates = new List<string>();
        List<string> player_2_coordinates = new List<string>();
        bool flag_player_1 = true;
        bool flag_player_2 = false;
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (flag_player_1 && matrix[i, j] == 1)
                {
                    player_1.Add(j.ToString());
                    player_1_coordinates.Add($"({i}->{j})");
                }
                if (flag_player_2 && matrix[i, j] == 1)
                {
                    player_2.Add(j.ToString());
                    player_2_coordinates.Add($"({i}->{j})");
                }
            }
            if (flag_player_1 && !flag_player_2 && player_1.Contains((i + 1).ToString()))
            {
                flag_player_1 = false;
                flag_player_2 = true;
            }
            else if (!flag_player_1 && flag_player_2 && player_1.Contains((i + 1).ToString()))
            {
                flag_player_1 = false;
                flag_player_2 = true;
            }
            if (flag_player_2 && !flag_player_1 && player_2.Contains((i + 1).ToString()))
            {
                flag_player_2 = false;
                flag_player_1 = true;
            }
            else if (!flag_player_2 && flag_player_1 && player_2.Contains((i + 1).ToString()))
            {
                flag_player_2 = false;
                flag_player_1 = true;
            }
        }

        foreach (var i in player_1_coordinates)
        {
            Console.WriteLine($"Координаты переходов игрока 1 {i}");
        }
        foreach (var i in player_2_coordinates)
        {
            Console.WriteLine($"Координаты переходов игрока 2 {i}");
        }
    }
    // добавить строку с уровнями
    static int[,] CalculateMatrixDepths(int[,] matrix, List<int> level_0)
    {
        int size = matrix.GetLength(0);
        int[,] extendedMatrix = new int[size + 1, size + 1];

        // Копируем исходную матрицу, добавляя нулевую строку и столбец
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                extendedMatrix[i + 1, j + 1] = matrix[i, j];
            }
        }

        // Множество отмеченных столбцов
        HashSet<int> markedColumns = new HashSet<int>();

        // Инициализация: устанавливаем уровень 0 для конечных вершин
        foreach (int vertex in level_0)
        {
            if (vertex >= 0 && vertex < size)
            {
                extendedMatrix[vertex + 1, 0] = 0; // Устанавливаем уровень в нулевом столбце
                extendedMatrix[0, vertex + 1] = 0; // Устанавливаем уровень в нулевой строке
                markedColumns.Add(vertex);        // Отмечаем столбец
            }
        }

        // Алгоритм по шагам
        int depth = 1; // Начинаем с уровня 1
        bool changesMade;
        do
        {
            changesMade = false;

            for (int i = 0; i < size; i++)
            {
                // Пропускаем строки, которые уже помечены
                if (extendedMatrix[i + 1, 0] != 0 || level_0.Contains(i))
                    continue;

                // Проверяем условие: символы "1" только в отмеченных столбцах
                bool allOnesInMarkedColumns = true;
                for (int j = 1; j < size + 1; j++)
                {
                    if (extendedMatrix[i + 1, j] == 1 && !markedColumns.Contains(j - 1))
                    {
                        allOnesInMarkedColumns = false;
                        break;
                    }
                }

                // Если условие выполняется, помечаем текущий столбец
                if (allOnesInMarkedColumns)
                {
                    extendedMatrix[i + 1, 0] = depth; // Записываем глубину в столбец
                    extendedMatrix[0, i + 1] = depth; // Записываем глубину в строку
                    markedColumns.Add(i);             // Отмечаем столбец
                    changesMade = true;
                }
            }

            depth++;
        } while (changesMade);

        return extendedMatrix;
    }
}
