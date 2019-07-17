using UnityEngine;

public static class Utils
{
    public static void Error(string format, params object[] args)
    {
        Debug.LogErrorFormat(format, args);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public static bool SameSign(float num1, float num2)
    {
        return num1 < 0 && num2 < 0 || num1 >= 0 && num2 >= 0;
    }

    public static float DecibelToLinear(float db)
    {
        return Mathf.Pow(10.0f, db / 20.0f);
    }

    public static float LinearToDecibel(float linear)
    {
        if (linear == 0f)
            return -120f;

        return Mathf.Log10(linear) * 20f;
    }
}