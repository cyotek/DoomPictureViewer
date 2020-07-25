using Cyotek.Data.Wad;
using Cyotek.Windows.Forms.Demo;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cyotek.Demo.DoomPictureViewer
{
  internal class DoomPictureRangeParser
  {
    #region Private Fields

    private bool _showPrimaryPosts;

    private bool _showSecondaryPosts;

    #endregion Private Fields

    #region Public Constructors

    public DoomPictureRangeParser()
    {
      _showPrimaryPosts = true;
      _showSecondaryPosts = true;
    }

    #endregion Public Constructors

    #region Public Properties

    public bool ShowPrimaryPosts
    {
      get { return _showPrimaryPosts; }
      set { _showPrimaryPosts = value; }
    }

    public bool ShowSecondaryPosts
    {
      get { return _showSecondaryPosts; }
      set { _showSecondaryPosts = value; }
    }

    #endregion Public Properties

    #region Public Methods

    public void AddRanges(HexViewer control, byte[] buffer)
    {
      control.Clear();

      if (buffer.Length > 8)
      {
        int[] pointer;
        short width;
        short height;
        
        width = WordHelpers.GetInt16Le(buffer, 0);
        height = WordHelpers.GetInt16Le(buffer, 2);

        // do some basic sanity checking
        if (width > 0 && width <= 320 && height > 0 && height <= 200)
        {
          control.AddRange(0, 2, Color.MediumSeaGreen, Color.White, "Width"); // width
          control.AddRange(2, 2, Color.SeaGreen, Color.White, "Height"); // height
          control.AddRange(4, 2, Color.DeepPink, Color.White, "X-Offset"); // x offset
          control.AddRange(6, 2, Color.HotPink, Color.White, "Y-Offset"); // y offset

          pointer = new int[width];

          for (int i = 0; i < width; i++)
          {
            int index;

            index = 8 + (i * 4);
            pointer[i] = WordHelpers.GetInt32Le(buffer, index);

            control.AddRange(new HexViewer.ByteGroup
            {
              Start = index,
              Length = 4,
              ForeColor = Color.White,
              BackColor = Color.Orange,
              Pointer = pointer[i],
              Type = "Pointer #" + i.ToString()
            }); // column pointer
          }

          for (int i = 0; i < width; i++)
          {
            int index;
            byte length;
            int sourceIndex;

            index = pointer[i];
            length = buffer[index + 1];
            sourceIndex = 8 + (i * 4);

            // post
            if (_showPrimaryPosts)
            {
              this.AddPost(control, index, length, sourceIndex, true);
            }

            if (length != 255)
            {
              index = this.IncrementColumn(buffer, index + 3, length);
              //index += length + 4; // row, length, 2xunused, pixel data

              if (buffer[index] == 255)
              {
                this.AddColumnEndMarker(control, index, pointer[i]);
              }
              else
              {
                while (true)
                {
                  length = buffer[index + 1];

                  if (length == 255 || index + length + 4 > buffer.Length)
                  {
                    break;
                  }
                  else if (_showSecondaryPosts)
                  {
                    this.AddPost(control, index, length, sourceIndex, false);
                  }

                  index = this.IncrementColumn(buffer, index + 3, length);
                  //  index += length + 4;

                  if (buffer[index] == 255)
                  {
                    this.AddColumnEndMarker(control, index, pointer[i]);
                    break;
                  }
                }
              }
            }
          }
        }
      }

      control.Invalidate();
    }

    #endregion Public Methods

    #region Private Methods

    private void AddColumnEndMarker(HexViewer control, int start, int source)
    {
      control.AddRange(new HexViewer.ByteGroup
      {
        Start = start,
        Length = 1,
        BackColor = Color.OrangeRed,
        ForeColor = Color.White,
        Type = "End Of Column",
        Pointer = source
      }); // end of column
    }

    private void AddPost(HexViewer control, int index, int length, int sourceIndex, bool isPrimary)
    {
      control.AddRange(new HexViewer.ByteGroup
      {
        Start = index,
        Length = 1,
        BackColor = isPrimary ? Color.DarkBlue : Color.Gray,
        ForeColor = Color.White,
        Type = "Row",
        Pointer = sourceIndex
      }); // row

      control.AddRange(new HexViewer.ByteGroup
      {
        Start = index + 1,
        Length = 1,
        BackColor = Color.MediumBlue,
        ForeColor = Color.White,
        Type = "Length",
        Pointer = index
      }); // length

      if (length != 255)
      {
        control.AddRange(new HexViewer.ByteGroup
        {
          Start = index + 2,
          Length = 1,
          BackColor = Color.Black,
          ForeColor = Color.White,
          Type = "Unused",
          Pointer = index
        }); // dead pixel?

        control.AddRange(new HexViewer.ByteGroup
        {
          Start = index + 3,
          Length = length,
          BackColor = Color.LightSkyBlue,
          ForeColor = Color.Black,
          Type = "Palette Index",
          Pointer = index
        }); // pixels

        control.AddRange(new HexViewer.ByteGroup
        {
          Start = index + 4 + length - 1,
          Length = 1,
          BackColor = Color.Black,
          ForeColor = Color.White,
          Type = "Unused",
          Pointer = index
        }); // dead pixel?
      }
    }

    private int IncrementColumn(byte[] buffer, int start, byte length)
    {
      for (int i = 0; i < length; i++)
      {
        if (buffer[start + i] == 255)
        {
          length = (byte)(i - 1);
          break;
        }
      }

      return start + length + 1;
    }

    #endregion Private Methods
  }
}