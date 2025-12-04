using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UsefulClasses
{
    public class ChessGrid<CustomTypeA, CustomTypeB>
    {
        private int width;
        private int height;
        private IChess[,] chessSlots;

        // ===========================
        // Tipos internos permitidos
        // ===========================
        private interface IChess { }

        private sealed class CellA : IChess
        {
            public CustomTypeA Value { get; }

            public CellA(CustomTypeA value)
            {
                Value = value;
            }
        }

        private sealed class CellB : IChess
        {
            public CustomTypeB Value { get; }

            public CellB(CustomTypeB value)
            {
                Value = value;
            }
        }

        // ===========================
        // Construtor
        // ===========================
        public ChessGrid(int newWidth, int newHeight)
        {
            width = newWidth;
            height = newHeight;

            chessSlots = new IChess[newWidth, newHeight];
        }

        // ===========================
        // Criação do tabuleiro alternado
        // ===========================
        public void CreateChessSlots()
        {
            bool isWidthEven = (width % 2) == 0;
            bool alternate = true;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (alternate)
                        chessSlots[x, y] = new CellA(default(CustomTypeA));
                    else
                        chessSlots[x, y] = new CellB(default(CustomTypeB));

                    // alternância por coluna
                    alternate = !alternate;
                }

                // se a largura é ímpar, inverte padrão a cada linha
                if (!isWidthEven)
                    alternate = !alternate;
            }
        }

        // ===========================
        // Set explícito para evitar erro de tipo
        // ===========================
        public void SetA(int x, int y, CustomTypeA value)
        {
            chessSlots[x, y] = new CellA(value);
        }

        public void SetB(int x, int y, CustomTypeB value)
        {
            chessSlots[x, y] = new CellB(value);
        }

        // ===========================
        // Get seguro com TryGet
        // ===========================
        public bool TryGetA(int x, int y, out CustomTypeA value)
        {
            if (chessSlots[x, y] is CellA a)
            {
                value = a.Value;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetB(int x, int y, out CustomTypeB value)
        {
            if (chessSlots[x, y] is CellB b)
            {
                value = b.Value;
                return true;
            }

            value = default;
            return false;
        }
        
        // ===========================
        // Métodos utilitários
        // ===========================
        public int Width  => width;
        public int Height => height;

        public bool IsCellA(int x, int y) => chessSlots[x, y] is CellA;
        public bool IsCellB(int x, int y) => chessSlots[x, y] is CellB;
    }

    public class ChessGrid<CustomType>
    {
        public int Width  { get; }
        public int Height { get; }

        private readonly CustomType[,] chessGrid;

        public ChessGrid(int newWidth, int newHeight, CustomType valueA, CustomType valueB)
        {
            if (newWidth <= 0)  throw new System.ArgumentOutOfRangeException(nameof(newWidth));
            if (newHeight <= 0) throw new System.ArgumentOutOfRangeException(nameof(newHeight));

            Width  = newWidth;
            Height = newHeight;

            chessGrid = new CustomType[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // padrão xadrez: (x + y) par = A, ímpar = B
                    bool useA = ((x + y) % 2) == 0;
                    chessGrid[x, y] = useA ? valueA : valueB;
                }
            }
        }

        public CustomType Get(int x, int y)
        {
            return chessGrid[x, y];
        }

        public void Set(int x, int y, CustomType value)
        {
            chessGrid[x, y] = value;
        }

        // açucar sintático opcional
        public CustomType this[int x, int y]
        {
            get => chessGrid[x, y];
            set => chessGrid[x, y] = value;
        }
    }

}
