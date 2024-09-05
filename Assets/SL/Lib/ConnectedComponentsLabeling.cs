using System;
using System.Collections.Generic;
using SL.Lib;
using UnityEngine;

public class ConnectedComponentLabeling
{
    private int[] labels;
    private int[] parent;
    private int nextLabel;
    private int width;
    private int height;
    public Tensor<int> LabelImage(Tensor<bool> binaryImage)
    {
        //Debug.Log($"{binaryImage}");
        width = binaryImage.Shape[1];
        height = binaryImage.Shape[0];
        int size = width * height;
        labels = new int[size];
        parent = new int[size];
        nextLabel = 1;

        // First pass
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!binaryImage[y, x]) continue;

                int index = y * width + x;
                int up = (y > 0) ? labels[(y - 1) * width + x] : 0;
                int left = (x > 0) ? labels[y * width + (x - 1)] : 0;

                if (up == 0 && left == 0)
                {
                    labels[index] = nextLabel;
                    parent[nextLabel] = nextLabel;
                    nextLabel++;
                }
                else if (up != 0 && left == 0)
                {
                    labels[index] = up;
                }
                else if (up == 0 && left != 0)
                {
                    labels[index] = left;
                }
                else
                {
                    labels[index] = Math.Min(up, left);
                    Union(up, left);
                }
            }
        }

        // Second pass
        for (int i = 0; i < size; i++)
        {
            if (labels[i] != 0)
            {
                labels[i] = Find(labels[i]);
            }
        }
        var result = new Tensor<int>(labels, new[] {height, width});
        //Debug.Log($"{result}");
        return result;
    }

    private void Union(int a, int b)
    {
        a = Find(a);
        b = Find(b);
        if (a != b)
        {
            parent[Math.Max(a, b)] = Math.Min(a, b);
        }
    }

    private int Find(int x)
    {
        if (parent[x] != x)
        {
            parent[x] = Find(parent[x]);
        }
        return parent[x];
    }
}