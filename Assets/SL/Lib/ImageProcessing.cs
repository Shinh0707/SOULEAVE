using System;
using System.Linq;
using System.Collections.Generic;

namespace SL.Lib
{
    public static class ImageProcessing
    {
        public enum ConvolutionMode
        {
            Full,
            Valid,
            Same
        }

        public enum PaddingMode
        {
            Constant,
            Reflect,
            Wrap
        }

        // Updated Convolve2D implementation
        public static double[,] Convolve2D(double[,] image, double[,] kernel, ConvolutionMode mode = ConvolutionMode.Full, PaddingMode paddingMode = PaddingMode.Constant, double constantValue = 0)
        {
            int imageHeight = image.GetLength(0);
            int imageWidth = image.GetLength(1);
            int kernelHeight = kernel.GetLength(0);
            int kernelWidth = kernel.GetLength(1);

            int resultHeight, resultWidth;
            int padTop, padBottom, padLeft, padRight;

            switch (mode)
            {
                case ConvolutionMode.Full:
                    resultHeight = imageHeight + kernelHeight - 1;
                    resultWidth = imageWidth + kernelWidth - 1;
                    padTop = kernelHeight - 1;
                    padBottom = kernelHeight - 1;
                    padLeft = kernelWidth - 1;
                    padRight = kernelWidth - 1;
                    break;
                case ConvolutionMode.Valid:
                    resultHeight = imageHeight - kernelHeight + 1;
                    resultWidth = imageWidth - kernelWidth + 1;
                    padTop = padBottom = padLeft = padRight = 0;
                    break;
                case ConvolutionMode.Same:
                default:
                    resultHeight = imageHeight;
                    resultWidth = imageWidth;
                    padTop = (kernelHeight - 1) / 2;
                    padBottom = kernelHeight / 2;
                    padLeft = (kernelWidth - 1) / 2;
                    padRight = kernelWidth / 2;
                    break;
            }

            double[,] paddedImage = PadImage(image, padTop, padBottom, padLeft, padRight, paddingMode, constantValue);
            double[,] result = new double[resultHeight, resultWidth];

            for (int i = 0; i < resultHeight; i++)
            {
                for (int j = 0; j < resultWidth; j++)
                {
                    double sum = Enumerable.Range(0, kernelHeight)
                        .SelectMany(m => Enumerable.Range(0, kernelWidth)
                            .Select(n => paddedImage[i + m, j + n] * kernel[kernelHeight - 1 - m, kernelWidth - 1 - n]))
                        .Sum();

                    result[i, j] = sum;
                }
            }

            return result;
        }

        private static double[,] PadImage(double[,] image, int padTop, int padBottom, int padLeft, int padRight, PaddingMode paddingMode, double constantValue)
        {
            int originalHeight = image.GetLength(0);
            int originalWidth = image.GetLength(1);
            int newHeight = originalHeight + padTop + padBottom;
            int newWidth = originalWidth + padLeft + padRight;

            double[,] paddedImage = new double[newHeight, newWidth];

            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    int originalI = i - padTop;
                    int originalJ = j - padLeft;

                    switch (paddingMode)
                    {
                        case PaddingMode.Constant:
                            paddedImage[i, j] = (originalI >= 0 && originalI < originalHeight && originalJ >= 0 && originalJ < originalWidth)
                                ? image[originalI, originalJ]
                                : constantValue;
                            break;
                        case PaddingMode.Reflect:
                            originalI = Math.Abs(originalI);
                            originalJ = Math.Abs(originalJ);
                            if (originalI >= originalHeight) originalI = 2 * originalHeight - originalI - 2;
                            if (originalJ >= originalWidth) originalJ = 2 * originalWidth - originalJ - 2;
                            paddedImage[i, j] = image[originalI, originalJ];
                            break;
                        case PaddingMode.Wrap:
                            originalI = (originalI + originalHeight) % originalHeight;
                            originalJ = (originalJ + originalWidth) % originalWidth;
                            paddedImage[i, j] = image[originalI, originalJ];
                            break;
                    }
                }
            }

            return paddedImage;
        }

        // ConnectedComponents implementation
        public static int[,] ConnectedComponents(bool[,] binaryImage)
        {
            int height = binaryImage.GetLength(0);
            int width = binaryImage.GetLength(1);
            int[,] labels = new int[height, width];
            int nextLabel = 1;

            Dictionary<int, HashSet<int>> equivalences = new Dictionary<int, HashSet<int>>();

            // First pass
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!binaryImage[y, x]) continue;

                    var neighborLabels = new List<int>();
                    if (x > 0 && binaryImage[y, x - 1]) neighborLabels.Add(labels[y, x - 1]);
                    if (y > 0 && binaryImage[y - 1, x]) neighborLabels.Add(labels[y - 1, x]);

                    if (!neighborLabels.Any())
                    {
                        labels[y, x] = nextLabel++;
                    }
                    else
                    {
                        int minLabel = neighborLabels.Min();
                        labels[y, x] = minLabel;

                        var currentEquivalences = neighborLabels.Distinct().Where(l => l != minLabel);
                        foreach (int label in currentEquivalences)
                        {
                            if (!equivalences.ContainsKey(minLabel))
                                equivalences[minLabel] = new HashSet<int>();
                            equivalences[minLabel].Add(label);
                        }
                    }
                }
            }

            // Resolve equivalences
            Dictionary<int, int> finalLabels = new Dictionary<int, int>();
            int finalLabel = 1;
            foreach (var kvp in equivalences)
            {
                if (!finalLabels.ContainsKey(kvp.Key))
                {
                    var allEquivalent = new HashSet<int> { kvp.Key };
                    allEquivalent.UnionWith(kvp.Value);

                    foreach (int label in allEquivalent)
                    {
                        finalLabels[label] = finalLabel;
                    }

                    finalLabel++;
                }
            }

            // Second pass
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (binaryImage[y, x])
                    {
                        labels[y, x] = finalLabels.ContainsKey(labels[y, x]) ? finalLabels[labels[y, x]] : labels[y, x];
                    }
                }
            }

            return labels;
        }
    }
}