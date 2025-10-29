using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Credit CritHax / Zehs (https://www.github.com/ZehsTeam)
namespace Scopophobia
{
    internal static class Utils
    {
        public static string GetEnumName(object e)
        {
            try
            {
                return System.Enum.GetName(e.GetType(), e);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static AnimationCurve CreateAnimationCurve(float[] values)
        {
            if (values == null || values.Length == 0)
            {
                return null;
            }

            AnimationCurve animationCurve = new AnimationCurve();

            if (values.Length == 1)
            {
                animationCurve.AddKey(new Keyframe(0f, values[0]));
                animationCurve.AddKey(new Keyframe(0.5f, values[0]));
                animationCurve.AddKey(new Keyframe(1f, values[0]));
                return animationCurve;
            }

            float timeIncrement = 1f / values.Length - 1;

            for (int i = 0; i < values.Length; i++)
            {
                float time = Mathf.Clamp(timeIncrement * i, 0f, 1f);
                animationCurve.AddKey(time, values[i]);
            }

            return animationCurve;
        }

        public static float[] ToFloatsArray(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return [];

            List<float> floats = [];

            string[] items = text.Split(',').Select(x => x.Trim()).ToArray();

            foreach (var item in items)
            {
                if (float.TryParse(item, out float parsedFloat))
                {
                    floats.Add(parsedFloat);
                }
            }

            return floats.ToArray();
        }
    }
}
