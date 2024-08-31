using System;
using System.Collections.Generic;

namespace SL.Lib
{
    /// <summary>
    /// �p�f�B���O���[�h�̒��ۊ��N���X
    /// </summary>
    public abstract class PadMode<T> where T : IComparable<T>
    {
        public abstract T GetPaddedValue(Tensor2D<T> matrix, int row, int col);
    }

    /// <summary>
    /// �萔�l�Ńp�f�B���O���郂�[�h
    /// </summary>
    public class ConstantPadMode<T> : PadMode<T> where T : IComparable<T>
    {
        private readonly T constantValue;

        public ConstantPadMode(T constantValue)
        {
            this.constantValue = constantValue;
        }

        public override T GetPaddedValue(Tensor2D<T> matrix, int row, int col)
        {
            return constantValue;
        }
    }

    /// <summary>
    /// �G�b�W�̒l�Ńp�f�B���O���郂�[�h
    /// </summary>
    public class EdgePadMode<T> : PadMode<T> where T : IComparable<T>
    {
        public override T GetPaddedValue(Tensor2D<T> matrix, int row, int col)
        {
            return matrix[Math.Clamp(row, 0, matrix.Rows - 1), Math.Clamp(col, 0, matrix.Cols - 1)];
        }
    }

    /// <summary>
    /// ���˂����ăp�f�B���O���郂�[�h
    /// </summary>
    public class ReflectPadMode<T> : PadMode<T> where T : IComparable<T>
    {
        public override T GetPaddedValue(Tensor2D<T> matrix, int row, int col)
        {
            return matrix[Reflect(row, matrix.Rows), Reflect(col, matrix.Cols)];
        }

        private static int Reflect(int index, int size)
        {
            if (index < 0)
                return -index - 1;
            if (index >= size)
                return 2 * size - index - 1;
            return index;
        }
    }

    /// <summary>
    /// �Ώ̂Ƀp�f�B���O���郂�[�h
    /// </summary>
    public class SymmetricPadMode<T> : PadMode<T> where T : IComparable<T>
    {
        public override T GetPaddedValue(Tensor2D<T> matrix, int row, int col)
        {
            return matrix[Symmetric(row, matrix.Rows), Symmetric(col, matrix.Cols)];
        }

        private static int Symmetric(int index, int size)
        {
            if (index < 0)
                return -index;
            if (index >= size)
                return 2 * size - index - 2;
            return index;
        }
    }

    /// <summary>
    /// �����t���ăp�f�B���O���郂�[�h
    /// </summary>
    public class WrapPadMode<T> : PadMode<T> where T : IComparable<T>
    {
        public override T GetPaddedValue(Tensor2D<T> matrix, int row, int col)
        {
            return matrix[Wrap(row, matrix.Rows), Wrap(col, matrix.Cols)];
        }

        private static int Wrap(int index, int size)
        {
            return ((index % size) + size) % size;
        }
    }

    public static class Tensor2DMathematics
    {
        /// <summary>
        /// �s��̗v�f�Ԃ̍������v�Z���܂��B
        /// </summary>
        /// <typeparam name="T">�s��̗v�f�̌^</typeparam>
        /// <param name="tensor">���͍s��</param>
        /// <param name="axis">��������鎲�i0: �s����, 1: ������j</param>
        /// <param name="prependValue">���ʂ̐擪�ɒǉ�����l�i�I�v�V�����j</param>
        /// <param name="appendValue">���ʂ̖����ɒǉ�����l�i�I�v�V�����j</param>
        /// <returns>������������V�����s��</returns>
        public static Tensor2D<T> Diff<T>(this Tensor2D<T> tensor, int axis, T prependValue = default, T appendValue = default) where T : IComparable<T>
        {
            int rows = tensor.Rows;
            int cols = tensor.Cols;
            Tensor2D<T> result;

            if (axis == 0) // �s�����̍���
            {
                result = new Tensor2D<T>(rows, cols);
                for (int i = 0; i < rows; i++)
                {
                    result[i, 0] = i == 0 ? prependValue : NumericOperations.Subtract(tensor[i, 0], tensor[i - 1, 0]);
                    for (int j = 1; j < cols; j++)
                    {
                        result[i, j] = NumericOperations.Subtract(tensor[i, j], tensor[i, j - 1]);
                    }
                }
                if (!EqualityComparer<T>.Default.Equals(appendValue, default))
                {
                    for (int i = 0; i < rows; i++)
                    {
                        result[i, cols - 1] = appendValue;
                    }
                }
            }
            else if (axis == 1) // ������̍���
            {
                result = new Tensor2D<T>(rows, cols);
                for (int j = 0; j < cols; j++)
                {
                    result[0, j] = j == 0 ? prependValue : NumericOperations.Subtract(tensor[0, j], tensor[0, j - 1]);
                    for (int i = 1; i < rows; i++)
                    {
                        result[i, j] = NumericOperations.Subtract(tensor[i, j], tensor[i - 1, j]);
                    }
                }
                if (!EqualityComparer<T>.Default.Equals(appendValue, default))
                {
                    for (int j = 0; j < cols; j++)
                    {
                        result[rows - 1, j] = appendValue;
                    }
                }
            }
            else
            {
                throw new ArgumentException("Axis must be 0 or 1");
            }

            return result;
        }

        /// <summary>
        /// �J�[�l�����g�p���čs�����ݍ��݂܂��B
        /// </summary>
        /// <typeparam name="T">�s��̗v�f�̌^</typeparam>
        /// <param name="tensor">���͍s��</param>
        /// <param name="kernel">��ݍ��݃J�[�l��</param>
        /// <param name="mode">��ݍ��݃��[�h�i"valid", "same", or "full"�j</param>
        /// <returns>��ݍ��݌�̐V�����s��</returns>
        public static Tensor2D<T> Convolve<T>(this Tensor2D<T> tensor, Tensor2D<T> kernel, string mode = "valid") where T : IComparable<T>
        {
            int m = tensor.Rows;
            int n = tensor.Cols;
            int km = kernel.Rows;
            int kn = kernel.Cols;

            int rm, rn; // ���ʂ̍s��̃T�C�Y
            switch (mode.ToLower())
            {
                case "valid":
                    rm = m - km + 1;
                    rn = n - kn + 1;
                    break;
                case "same":
                    rm = m;
                    rn = n;
                    break;
                case "full":
                    rm = m + km - 1;
                    rn = n + kn - 1;
                    break;
                default:
                    throw new ArgumentException("Invalid mode. Use 'valid', 'same', or 'full'.");
            }

            Tensor2D<T> result = new Tensor2D<T>(rm, rn);

            int startI = mode == "full" ? 0 : (km - 1) / 2;
            int startJ = mode == "full" ? 0 : (kn - 1) / 2;
            int endI = mode == "full" ? rm : m + (km - 1) / 2;
            int endJ = mode == "full" ? rn : n + (kn - 1) / 2;

            for (int i = startI; i < endI; i++)
            {
                for (int j = startJ; j < endJ; j++)
                {
                    T sum = default;
                    for (int ki = 0; ki < km; ki++)
                    {
                        for (int kj = 0; kj < kn; kj++)
                        {
                            int mi = i - startI + ki;
                            int mj = j - startJ + kj;
                            if (mi >= 0 && mi < m && mj >= 0 && mj < n)
                            {
                                sum = NumericOperations.Add(sum, NumericOperations.Multiply(tensor[mi, mj], kernel[km - 1 - ki, kn - 1 - kj]));
                            }
                        }
                    }
                    result[i - startI, j - startJ] = sum;
                }
            }

            return result;
        }
    }
}