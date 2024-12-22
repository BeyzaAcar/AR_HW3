using System;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class Problem1_6_7Test : MonoBehaviour
{
    void Start()
    {
        // 1.6 ve 1.7 için sahne ve görüntü noktaları
        var homographyScenePoints = new List<Tuple<double, double>>
        {
            Tuple.Create(0.0, 0.0),
            Tuple.Create(1.0, 0.0),
            Tuple.Create(1.0, 1.0),
            Tuple.Create(0.0, 1.0),
            Tuple.Create(0.5, 0.5)
        };

        var homographyImagePoints = new List<Tuple<double, double>>
        {
            Tuple.Create(100.0, 100.0),
            Tuple.Create(200.0, 100.0),
            Tuple.Create(200.0, 200.0),
            Tuple.Create(100.0, 200.0),
            Tuple.Create(150.0, 150.0)
        };

        // Lineer Homografi Matrisi Hesaplama
        var linearHomography = HomographyCalculator.CalculateHomography(homographyScenePoints, homographyImagePoints);
        Debug.Log("Linear Homography Matrix:");
        Debug.Log(linearHomography);

        // 1.6 Sahne → Görüntü Dönüşümleri (Lineer ile)
        Debug.Log("Linear Scene to Image Transformations:");
        foreach (var scenePoint in homographyScenePoints)
        {
            var imagePoint = HomographyCalculator.TransformSceneToImage(scenePoint, linearHomography);
            Debug.Log($"Scene Point: {scenePoint} -> Image Point: {imagePoint}");
        }

        // 1.7 Görüntü → Sahne Dönüşümleri (Lineer ile)
        Debug.Log("Linear Image to Scene Transformations:");
        foreach (var imagePoint in homographyImagePoints)
        {
            var scenePoint = HomographyCalculator.TransformImageToScene(imagePoint, linearHomography);
            Debug.Log($"Image Point: {imagePoint} -> Scene Point: {scenePoint}");
        }

        // 1.7 Hata Hesaplama (Lineer ile)
        Debug.Log("Error Calculation with Linear Homography:");
        var averageErrorLinear = HomographyCalculator.CalculateError(homographyScenePoints, homographyImagePoints, linearHomography);
        Debug.Log($"Linear Average Projection Error: {averageErrorLinear}");
    }
}
