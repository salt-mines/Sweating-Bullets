using System;
using UnityEditor;
using UnityEngine;

public static class Utils
{
    /// <summary>
    ///     Log an error and stop Unity play mode.
    /// </summary>
    /// <param name="format">format string</param>
    /// <param name="args">format args</param>
    public static void Error(string format, params object[] args)
    {
        Debug.LogErrorFormat(format, args);
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    ///     Check if two floats have the same sign.
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <returns>true if both floats have the same sign</returns>
    public static bool SameSign(float num1, float num2)
    {
        return num1 < 0 && num2 < 0 || num1 >= 0 && num2 >= 0;
    }

    /// <summary>
    ///     Convert decibels into linear volume (0-1).
    /// </summary>
    /// <param name="db"></param>
    /// <returns>volume in linear scale</returns>
    public static float DecibelToLinear(float db)
    {
        return Mathf.Pow(10.0f, db / 20.0f);
    }

    /// <summary>
    ///     Convert linear volume [0, 1] into decibels. Exactly 0 volume returns -120 dB.
    /// </summary>
    /// <param name="linear"></param>
    /// <returns>volume in dB</returns>
    public static float LinearToDecibel(float linear)
    {
        if (linear == 0f)
            return -120f;

        return Mathf.Log10(linear) * 20f;
    }

    /// <summary>
    ///     Get command line argument by name (hyphen included).
    /// </summary>
    /// <param name="name">name of argument to find, with hyphen</param>
    /// <returns>value passed with given name</returns>
    public static string GetArgument(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
            if (args[i] == name)
                return args.Length > i + 1 ? args[i + 1] : args[i];
        return null;
    }
}