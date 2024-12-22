using System;
using MathNet.Numerics.Optimization;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


public class HomographyCalculator
{
    // 1. Homografi Matrisi Hesaplama (Linear)
    public static Matrix<double> CalculateHomography(List<Tuple<double, double>> scenePoints, List<Tuple<double, double>> imagePoints)
    {
        if (scenePoints.Count != imagePoints.Count || scenePoints.Count < 4)
        {
            throw new ArgumentException("At least 4 point correspondences are required, and the number of scene and image points must match.");
        }

        var A = new List<double[]>();

        for (int i = 0; i < scenePoints.Count; i++)
        {
            double x = scenePoints[i].Item1;
            double y = scenePoints[i].Item2;
            double u = imagePoints[i].Item1;
            double v = imagePoints[i].Item2;

            A.Add(new double[] { x, y, 1, 0, 0, 0, -u * x, -u * y, -u });
            A.Add(new double[] { 0, 0, 0, x, y, 1, -v * x, -v * y, -v });
        }

        var matrixA = DenseMatrix.OfRows(A.Count, 9, A);
        var svd = matrixA.Svd();
        var h = svd.VT.Row(svd.VT.RowCount - 1);

        return DenseMatrix.Build.DenseOfRowMajor(3, 3, h); // Return as 3x3 matrix
    }

    // 2. Homografi Matrisi Hesaplama (Non-Linear Optimization)
    /*public static Matrix<double> CalculateHomographyNonLinear(List<Tuple<double, double>> scenePoints, List<Tuple<double, double>> imagePoints)
    {
        if (scenePoints.Count != imagePoints.Count || scenePoints.Count < 4)
        {
            throw new ArgumentException("At least 4 point correspondences are required, and the number of scene and image points must match.");
        }

        // 1. Lineer yöntem ile başlangıç homografi matrisini tahmin et
        var initialHomography = CalculateHomography(scenePoints, imagePoints);

        // 2. Optimize edilecek parametreleri (h0, h1, ..., h8) başlangıç matrisi olarak kullan
        var initialParameters = initialHomography.ToColumnMajorArray();

        // 3. Optimizasyon için maliyet fonksiyonunu tanımla
        Func<Vector<double>, double> costFunction = parameters =>
        {
            double totalError = 0;
            var homographyMatrix = DenseMatrix.OfColumnMajor(3, 3, parameters.ToArray());

            for (int i = 0; i < scenePoints.Count; i++)
            {
                var scenePoint = scenePoints[i];
                var imagePoint = TransformSceneToImage(scenePoint, homographyMatrix);

                // Öklid mesafesi hatasını hesapla
                double error = Math.Pow(imagePoint.Item1 - imagePoints[i].Item1, 2) +
                               Math.Pow(imagePoint.Item2 - imagePoints[i].Item2, 2);
                totalError += error;
            }

            return totalError;
        };

        // 4. Optimizasyon yap
        // Optimizasyon yap
        var optimizer = new LevenbergMarquardtMinimizer();
        var objectiveModel = ObjectiveFunction.Create(costFunction);
        var initialGuess = Vector<double>.Build.DenseOfArray(initialParameters);
        var result = optimizer.FindMinimum(objectiveModel, initialGuess);

        // 5. Optimize edilen parametrelerden homografi matrisini oluştur
        return DenseMatrix.OfColumnMajor(3, 3, result.MinimizingPoint.ToArray());
    }*/

    private static double[] ComputeGradients(List<Tuple<double, double>> scenePoints, List<Tuple<double, double>> imagePoints, double[] parameters)
    {
        // Parametre sayısı (h0, h1, ..., h8) toplamda 9 parametre
        var gradients = new double[parameters.Length];

        // Homografi matrisi
        var homographyMatrix = DenseMatrix.OfColumnMajor(3, 3, parameters);

        for (int i = 0; i < scenePoints.Count; i++)
        {
            // Sahne ve görüntü noktalarını al
            var scenePoint = scenePoints[i];
            var x = scenePoint.Item1;
            var y = scenePoint.Item2;

            var actualImagePoint = imagePoints[i];
            var uActual = actualImagePoint.Item1;
            var vActual = actualImagePoint.Item2;

            // Sahne noktasını homografi matrisi ile dönüştür
            var denominator = (homographyMatrix[2, 0] * x + homographyMatrix[2, 1] * y + homographyMatrix[2, 2]);
            var u = (homographyMatrix[0, 0] * x + homographyMatrix[0, 1] * y + homographyMatrix[0, 2]) / denominator;
            var v = (homographyMatrix[1, 0] * x + homographyMatrix[1, 1] * y + homographyMatrix[1, 2]) / denominator;

            // Hataları hesapla
            var errorU = u - uActual;
            var errorV = v - vActual;

            // Gradyanları hesapla (zincir kuralı kullanılarak)
            for (int j = 0; j < gradients.Length; j++)
            {
                if (j < 3) // h0, h1, h2 için
                {
                    gradients[j] += 2 * errorU * x / denominator + 2 * errorV * y / denominator;
                }
                else if (j < 6) // h3, h4, h5 için
                {
                    gradients[j] += 2 * errorU * y / denominator + 2 * errorV * x / denominator;
                }
                else // h6, h7, h8 için
                {
                    gradients[j] += -2 * (errorU * u + errorV * v) / (denominator * denominator);
                }
            }
        }

        return gradients;
    }


    // Gradyan inişi metodu
    private static double[] GradientDescent(List<Tuple<double, double>> scenePoints, List<Tuple<double, double>> imagePoints, double[] initialParameters, double learningRate, int maxIterations)
    {
        var parameters = (double[])initialParameters.Clone();

        for (int iter = 0; iter < maxIterations; iter++)
        {
            var gradients = ComputeGradients(scenePoints, imagePoints, parameters);

            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] -= learningRate * gradients[i];
            }
        }

        return parameters;
    }
    
    // 3. Weighted Homography Calculation
    /*public static Matrix<double> CalculateWeightedHomography(List<Tuple<double, double>> scenePoints, List<Tuple<double, double>> imagePoints, Matrix<double> correspondenceMatrix)
    {
        if (scenePoints.Count != correspondenceMatrix.RowCount || imagePoints.Count != correspondenceMatrix.ColumnCount)
        {
            throw new ArgumentException("Correspondence matrix dimensions must match the number of scene and image points.");
        }

        var selectedScenePoints = new List<Tuple<double, double>>();
        var selectedImagePoints = new List<Tuple<double, double>>();

        // Select best matches based on correspondence matrix
        for (int i = 0; i < correspondenceMatrix.RowCount; i++)
        {
            int bestMatchIndex = 0;
            double bestMatchValue = double.MinValue;

            for (int j = 0; j < correspondenceMatrix.ColumnCount; j++)
            {
                if (correspondenceMatrix[i, j] > bestMatchValue)
                {
                    bestMatchValue = correspondenceMatrix[i, j];
                    bestMatchIndex = j;
                }
            }

            selectedScenePoints.Add(scenePoints[i]);
            selectedImagePoints.Add(imagePoints[bestMatchIndex]);
        }

        // Use Non-Linear Optimization for Final Homography Calculation
        return CalculateHomographyNonLinear(selectedScenePoints, selectedImagePoints);
    }*/

    // 4. Scene → Image Transformation
    public static Tuple<double, double> TransformSceneToImage(Tuple<double, double> scenePoint, Matrix<double> homographyMatrix)
    {
        var x = scenePoint.Item1;
        var y = scenePoint.Item2;

        var u = (homographyMatrix[0, 0] * x + homographyMatrix[0, 1] * y + homographyMatrix[0, 2]) /
                (homographyMatrix[2, 0] * x + homographyMatrix[2, 1] * y + homographyMatrix[2, 2]);

        var v = (homographyMatrix[1, 0] * x + homographyMatrix[1, 1] * y + homographyMatrix[1, 2]) /
                (homographyMatrix[2, 0] * x + homographyMatrix[2, 1] * y + homographyMatrix[2, 2]);

        return Tuple.Create(u, v);
    }

    // 5. Image → Scene Transformation
    public static Tuple<double, double> TransformImageToScene(Tuple<double, double> imagePoint, Matrix<double> homographyMatrix)
    {
        var u = imagePoint.Item1;
        var v = imagePoint.Item2;

        var inverseHomography = homographyMatrix.Inverse();

        var x = (inverseHomography[0, 0] * u + inverseHomography[0, 1] * v + inverseHomography[0, 2]) /
                (inverseHomography[2, 0] * u + inverseHomography[2, 1] * v + inverseHomography[2, 2]);

        var y = (inverseHomography[1, 0] * u + inverseHomography[1, 1] * v + inverseHomography[1, 2]) /
                (inverseHomography[2, 0] * u + inverseHomography[2, 1] * v + inverseHomography[2, 2]);

        return Tuple.Create(x, y);
    }

    // 6. Error Calculation
    public static double CalculateError(List<Tuple<double, double>> scenePoints, List<Tuple<double, double>> actualImagePoints, Matrix<double> homographyMatrix)
    {
        double totalError = 0;

        for (int i = 0; i < scenePoints.Count; i++)
        {
            var transformedPoint = TransformSceneToImage(scenePoints[i], homographyMatrix);

            var error = Math.Sqrt(Math.Pow(transformedPoint.Item1 - actualImagePoints[i].Item1, 2) +
                                  Math.Pow(transformedPoint.Item2 - actualImagePoints[i].Item2, 2));
            totalError += error;
        }

        return totalError / scenePoints.Count; // Average Error
    }
}
