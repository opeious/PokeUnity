using System;

public class Bitmap : IDisposable
{
    public int Width, Height;
    
    public PixelFormat PixelFormat;
    
    public Bitmap (string fileName)
    {
    }

    public Bitmap (Bitmap bmp)
    {
    }

    public Bitmap (Bitmap bmp, int width, int height)
    {
    }

    public Bitmap (int width, int height, PixelFormat format32BppArgb)
    {
    }

    public void Dispose()
    {
        
    }
}