using Cyotek.Demo;
using Cyotek.Demo.DoomPictureViewer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Windows.Forms;
using FileInfo = Cyotek.Demo.FileInfo;

namespace Cyotek.Windows.Forms.Demo
{
  public partial class MainForm : Form
  {
    #region Private Fields

    private Bitmap _bitmap;

    private string _fileName;

    private Color[] _palette;

    private string _paletteFileName;

    private DoomPictureReader _reader;

    #endregion Private Fields

    #region Public Constructors

    public MainForm()
    {
      this.InitializeComponent();
    }

    #endregion Public Constructors

    #region Protected Methods

    protected override void OnShown(EventArgs e)
    {
      string[] args;

      base.OnShown(e);

      this.LoadPalettes(Path.Combine(Application.StartupPath, "palettes"));
      this.OpenPalette(Path.Combine(Application.StartupPath, "palettes\\doom1.pal"));
      filePane.Path = Path.Combine(Application.StartupPath, "samples");

      args = Environment.GetCommandLineArgs();

      if (args.Length == 2)
      {
        this.OpenFile(args[1]);
      }
      else
      {
        filePane.EnsureSelection();
      }
    }

    #endregion Protected Methods

    #region Private Methods

    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      AboutDialog.ShowAboutDialog();
    }

    private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (_bitmap != null)
      {
        ClipboardHelper.CopyImage(_bitmap);
      }
      else
      {
        SystemSounds.Beep.Play();
      }
    }

    private void CyotekLinkToolStripStatusLabel_Click(object sender, EventArgs e)
    {
      AboutDialog.OpenCyotekHomePage();

      cyotekLinkToolStripStatusLabel.LinkVisited = true;
    }

    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void ExportImageToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (_bitmap != null)
      {
        string fileName;

        fileName = FileDialogHelper.GetSaveFileName("Export Image", "Portable Network Graphics (*.png)|*.png|Bitmaps (*.bmp)|*.bmp|All Files (*.*)|*.*", "png", Path.ChangeExtension(_fileName, "png"));

        if (!string.IsNullOrEmpty(fileName))
        {
          try
          {
            ImageFormat format;

            format = string.Equals(".bmp", Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase) ? ImageFormat.Bmp : ImageFormat.Png;

            _bitmap.Save(fileName, format);
          }
          catch (Exception ex)
          {
            MessageBox.Show(string.Format("Failed to export image. {0}", ex.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
        }
      }
      else
      {
        SystemSounds.Beep.Play();
      }
    }

    private void FilePane_SelectedFileChanged(object sender, EventArgs e)
    {
      if (filePane.SelectedFile is FileInfo file && !string.Equals(_fileName, file.FullPath, StringComparison.OrdinalIgnoreCase))
      {
        this.OpenFile(file.FullPath);
      }
    }

    private Color[] LoadPalette(string fileName)
    {
      Color[] palette;
      byte[] buffer;
      int size;

      buffer = File.ReadAllBytes(fileName);
      size = buffer.Length / 3;
      palette = new Color[size];

      for (int i = 0; i < size; i++)
      {
        int offset;

        offset = i * 3;
        palette[i] = Color.FromArgb(buffer[offset], buffer[offset + 1], buffer[offset + 2]);
      }

      return palette;
    }

    private void LoadPalettes(string path)
    {
      int index;

      index = 0;

      foreach (string fileName in Directory.EnumerateFiles(path, "*.pal"))
      {
        ToolStripMenuItem menuItem;

        menuItem = new ToolStripMenuItem
        {
          Text = Path.GetFileName(fileName),
          Tag = fileName
        };

        menuItem.Click += this.PaletteMenuItemClickHandler;

        paletteToolStripMenuItem.DropDownItems.Insert(index++, menuItem);
      }
    }

    private void OpenFile(string fileName)
    {
      previewImageBox.Text = null;
      previewImageBox.Image = null;
      _bitmap?.Dispose();
      _bitmap = null;

      try
      {
        _bitmap = _reader.Read(fileName);

        previewImageBox.Image = _bitmap;

        _fileName = fileName;
      }
      catch (Exception ex)
      {
        previewImageBox.Text = string.Format("Failed to open '{0}'. {1}", fileName, ex.Message);
        _fileName = null;
      }

      if (autoFitToolStripMenuItem.Checked)
      {
        previewImageBox.ZoomToFit();
      }

      this.UpdateStatusBar();
      this.UpdateWindowTitle();
      this.UpdateSelection();
    }

    private void OpenPalette(string fileName)
    {
      _palette = this.LoadPalette(fileName);

      if (useTransparencyToolStripMenuItem.Checked)
      {
        _palette[255] = Color.Transparent; // DOOM uses 255 for transparency
      }

      _reader = new DoomPictureReader
      {
        Palette = _palette
      };

      foreach (ToolStripItem item in paletteToolStripMenuItem.DropDownItems)
      {
        if (item is ToolStripMenuItem menuItem && menuItem.Tag is string paletteFileName)
        {
          menuItem.Checked = string.Equals(fileName, paletteFileName, StringComparison.OrdinalIgnoreCase);
        }
      }

      if (!string.IsNullOrEmpty(_fileName))
      {
        this.OpenFile(_fileName);
      }

      _paletteFileName = fileName;

      this.UpdateWindowTitle();
    }

    private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
    {
      string fileName;

      fileName = FileDialogHelper.GetOpenFileName("Open File", "All Files (*.*)|*.*", string.Empty);

      if (!string.IsNullOrEmpty(fileName))
      {
        this.OpenFile(fileName);
      }
    }

    private void PaletteMenuItemClickHandler(object sender, EventArgs e)
    {
      this.OpenPalette((string)((ToolStripMenuItem)sender).Tag);
    }

    private void UpdateSelection()
    {
      filePane.SelectFile(_fileName);
    }

    private void UpdateStatusBar()
    {
      widthToolStripStatusLabel.Text = string.Format("Width: {0}", _bitmap?.Width ?? 0);
      heightToolStripStatusLabel.Text = string.Format("Height: {0}", _bitmap?.Height ?? 0);
    }

    private void UpdateWindowTitle()
    {
      this.Text = !string.IsNullOrEmpty(_fileName) ? string.Format("{1} [{2}] - {0}", Application.ProductName, Path.GetFileName(_fileName), Path.GetFileName(_paletteFileName)) : Application.ProductName;
    }

    private void UseTransparencyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.OpenPalette(_paletteFileName);
    }

    #endregion Private Methods
  }
}