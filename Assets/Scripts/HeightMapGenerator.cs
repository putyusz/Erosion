using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {

    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre) {
        float[,] values = Noise.GenerateNoiseMap2(width, settings.noiseSettings, sampleCentre);

        //AnimationCurve heightCurve_threadSafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                values[i, j] *= /*heightCurve_threadSafe.Evaluate(values[i, j]) **/ settings.heightMultiplier;
                if (values[i, j] > maxValue) {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue) {
                    minValue = values[i, j];
                }
            }
        }
        return new HeightMap(values, minValue, maxValue);
    }

    public static HeightMap GeneratePlane(int width, int height) {
        float[,] values = new float[width, height];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                values[i, j] = 0;
            }
        }
        return new HeightMap(values, 0, 0);
    }
}

public struct HeightMap {
    public float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float minValue, float maxValue) {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}