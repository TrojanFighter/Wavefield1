using UnityEngine;

public static class CameraUtil
{
    private static int GCD(int a, int b)
    {
        while (true)
        {
            if (b == 0) return a;
            var temp = a;
            a = b;
            b = temp % b;
        }
    }

    public struct AspectRatio
    {
        public readonly int width, height;

        public AspectRatio(int width, int height)
        {
            Debug.Assert(width > 0 && height > 0, "Aspect ratio must contain only integers greater than 0");
            this.width = width;
            this.height = height;
        }

        public static implicit operator double(AspectRatio ratio)
        {
            return (double) ratio.width / ratio.height;
        }

        public static implicit operator float(AspectRatio ratio)
        {
            return (float) ratio.width / ratio.height;
        }

        public static AspectRatio FromResolution(int width, int height)
        {
            var divisor = GCD(width, height);
            do divisor = GCD(width /= divisor, height /= divisor);
            while (divisor > 1);
            return new AspectRatio(width, height);
        }

    }

    public static void SetupAspectCamera(AspectRatio ratio, double scale = 1)
    {
        var hrh = (float) (ratio.height / 2.0 * scale);
        var camera = Camera.main;
        camera.orthographic = true;
        camera.orthographicSize = hrh;
        camera.transform.position = new Vector3(hrh * ratio, hrh, -10);
    }

    public static Vector3 RandomPositionInView(float zPos=0)
    {
        var pos = Camera.main.ViewportToWorldPoint(new Vector3(Random.value, Random.value, 0));
        pos.z = zPos;
        return pos;
    }


}