using System;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class HomographyTest : MonoBehaviour
{
    void Start()
    {
        // Örnek sahne ve görüntü noktaları
        var scenePoints = new List<Tuple<double, double>>
        {
            Tuple.Create(0.0, 0.0),
            Tuple.Create(1.0, 0.0),
            Tuple.Create(1.0, 1.0),
            Tuple.Create(0.0, 1.0),
            Tuple.Create(0.5, 0.5)
        };

        var imagePoints = new List<Tuple<double, double>>
        {
            Tuple.Create(100.0, 100.0),
            Tuple.Create(200.0, 100.0),
            Tuple.Create(200.0, 200.0),
            Tuple.Create(100.0, 200.0),
            Tuple.Create(150.0, 150.0)
        };

        // 1. Lineer Homografi Matrisi Hesaplama
        var linearHomography = HomographyCalculator.CalculateHomography(scenePoints, imagePoints);
        Debug.Log("Linear Homography Matrix:");
        Debug.Log(linearHomography);

        // 2. Scene → Image Dönüşümü (Lineer ile)
        Debug.Log("Linear Scene to Image Transformations:");
        foreach (var scenePoint in scenePoints)
        {
            var imagePoint = HomographyCalculator.TransformSceneToImage(scenePoint, linearHomography);
            Debug.Log($"Scene Point: {scenePoint} -> Image Point: {imagePoint}");
        }

        // 3. Error Hesaplama (Lineer ile)
        Debug.Log("Error Calculation with Linear Homography:");
        var averageErrorLinear = HomographyCalculator.CalculateError(scenePoints, imagePoints, linearHomography);
        Debug.Log($"Linear Average Projection Error: {averageErrorLinear}");
    }
}
