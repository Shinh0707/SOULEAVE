using System;
using System.Collections.Generic;
using SL.Lib;

public class ConnectedComponentLabeling
{
    private Tensor<int> labels;
    private int currentLabel;
    private int width;
    private int height;

    public Tensor<int> LabelImage(Tensor<bool> binaryImage)
    {
        width = binaryImage.Shape[1];
        height = binaryImage.Shape[0];
        labels = new Tensor<int>(new int[height * width], new[] { height, width });
        currentLabel = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (binaryImage[y, x] && labels[y, x] == 0)
                {
                    currentLabel++;
                    TraceContour(binaryImage, x, y);
                }
            }
        }

        return labels;
    }

    private void TraceContour(Tensor<bool> image, int startX, int startY)
    {
        int x = startX;
        int y = startY;
        int direction = 0; // 0: right, 1: down, 2: left, 3: up

        do
        {
            labels[y, x] = currentLabel;

            // Find the next pixel
            for (int i = 0; i < 4; i++)
            {
                int nextDirection = (direction + 3 - i) % 4;
                int nextX = x + dx[nextDirection];
                int nextY = y + dy[nextDirection];

                if (IsValid(nextX, nextY) && image[nextY, nextX])
                {
                    x = nextX;
                    y = nextY;
                    direction = nextDirection;
                    break;
                }
            }

            // Fill the component
            FillComponent(image, x, y);

        } while (x != startX || y != startY);
    }

    private void FillComponent(Tensor<bool> image, int x, int y)
    {
        Stack<(int, int)> stack = new Stack<(int, int)>();
        stack.Push((x, y));

        while (stack.Count > 0)
        {
            (int cx, int cy) = stack.Pop();

            for (int i = 0; i < 4; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];

                if (IsValid(nx, ny) && image[ny, nx] && labels[ny, nx] == 0)
                {
                    labels[ny, nx] = currentLabel;
                    stack.Push((nx, ny));
                }
            }
        }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private static readonly int[] dx = { 1, 0, -1, 0 };
    private static readonly int[] dy = { 0, 1, 0, -1 };
}