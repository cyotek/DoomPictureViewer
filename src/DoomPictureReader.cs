using Cyotek.Data.Wad;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Cyotek.Demo.DoomPictureViewer
{
  internal class DoomPictureReader
  {
    #region Private Fields

    private Color[] _palette;

    #endregion Private Fields

    #region Public Properties

    public Color[] Palette
    {
      get { return _palette; }
      set
      {
        if (value != null && value.Length != 256)
        {
          throw new ArgumentException("Palette must be comprised of exactly 256 colors.", nameof(value));
        }

        _palette = value;
      }
    }

    #endregion Public Properties

    #region Public Methods

    public Bitmap Read(string fileName)
    {
      using (Stream stream = File.OpenRead(fileName))
      {
        return this.Read(stream);
      }
    }

    public Bitmap Read(Stream stream)
    {
      byte[] buffer;

      buffer = new byte[stream.Length];
      stream.Read(buffer, 0, buffer.Length);

      return this.Read(buffer);
    }

    public Bitmap Read(byte[] data)
    {
      int width;
      int height;
      byte[] pixelData;

      width = WordHelpers.GetInt16Le(data, 0);
      height = WordHelpers.GetInt16Le(data, 2);

      pixelData = new byte[width * height];

      // doom images seem to use index
      // 255 for transparency, so we'll
      // set the image data to transparent 
      // by default for the entire image

      for (int i = 0; i < pixelData.Length; i++)
      {
        pixelData[i] = 255;
      }

      // each column is represented by one or
      // more sub columns, called "posts" in the
      // DOOM FAQ. Each post has a starting row,
      // and a height, followed by a seemly unused
      // value, the pixel data and then another
      // unused value

      for (int column = 0; column < width; column++)
      {
        int pointer;

        pointer = WordHelpers.GetInt32Le(data, (column * 4) + 8);

        do
        {
          int row;
          int postHeight;

          row = data[pointer];

          if (row != 255 && (postHeight = data[++pointer]) != 255)
          {
            pointer++; // unused value

            for (int i = 0; i < postHeight; i++)
            {
              if (row + i < height && pointer < data.Length - 1)
              {
                pixelData[((row + i) * width) + column] = data[++pointer];
              }
            }

            pointer++; // unused value
          }
          else
          {
            break;
          }
        } while (pointer < data.Length - 1 && data[++pointer] != 255);
      }

      return this.CreateIndexedBitmap(width, height, pixelData);
    }

    #endregion Public Methods

    #region Private Methods

    private Bitmap CreateIndexedBitmap(int width, int height, byte[] pixelData)
    {
      Bitmap bitmap;
      BitmapData bitmapData;
      ColorPalette palette;
      int index;
      int stride;

      bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
      bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

      // can't create brand new palettes
      // so need to rework the existing one
      palette = bitmap.Palette;

      for (int i = 0; i < 256; i++)
      {
        palette.Entries[i] = _palette[i];
      }

      bitmap.Palette = palette;

      // apply palette indexes to the bitmap
      index = 0;
      stride = bitmapData.Stride < 0 ? -bitmapData.Stride : bitmapData.Stride;

      unsafe
      {
        byte* row;

        row = (byte*)bitmapData.Scan0;

        for (int y = 0; y < height; y++)
        {
          for (int x = 0; x < width; x++)
          {
            row[x] = pixelData[index++];
          }

          row += stride;
        }
      }

      bitmap.UnlockBits(bitmapData);

      return bitmap;
    }

    #endregion Private Methods
  }
}