using System;
using System.IO;
using System.Windows.Forms;

namespace Cyotek.Demo.DoomPictureViewer
{
  public partial class MainForm : Form
  {
    #region Private Fields

    private string _fileName;

    private DoomPictureRangeParser _parser;

    #endregion Private Fields

    #region Public Constructors

    public MainForm()
    {
      this.InitializeComponent();
    }

    #endregion Public Constructors

    #region Protected Methods

    protected override void OnLoad(EventArgs e)
    {
      _parser = new DoomPictureRangeParser();

      base.OnLoad(e);

      primaryPostsToolStripMenuItem.Checked = _parser.ShowPrimaryPosts;
      secondaryPostsToolStripMenuItem.Checked = _parser.ShowSecondaryPosts;
    }

    protected override void OnShown(EventArgs e)
    {
      string[] args;

      base.OnShown(e);

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

    private void CyotekLinkToolStripStatusLabel_Click(object sender, EventArgs e)
    {
      AboutDialog.OpenCyotekHomePage();

      cyotekLinkToolStripStatusLabel.LinkVisited = true;
    }

    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void FilePane_SelectedFileChanged(object sender, EventArgs e)
    {
      if (filePane.SelectedFile is FileInfo file && !string.Equals(_fileName, file.FullPath, StringComparison.OrdinalIgnoreCase))
      {
        this.OpenFile(file.FullPath);
      }
    }

    private void OpenFile(string fileName)
    {
      byte[] buffer;

      buffer = File.ReadAllBytes(fileName);

      hexViewer.Data = buffer;

      _parser.AddRanges(hexViewer, buffer);

      sizeToolStripStatusLabel.Text = string.Format("Size: {0}", buffer.Length);

      _fileName = fileName;
      this.UpdateWindowTitle();
      this.UpdateSelection();
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

    private void PrimaryPostsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      _parser.ShowPrimaryPosts = primaryPostsToolStripMenuItem.Checked;
      _parser.ShowSecondaryPosts = secondaryPostsToolStripMenuItem.Checked;

      if (!string.IsNullOrEmpty(_fileName))
      {
        _parser.AddRanges(hexViewer, hexViewer.Data);
      }
    }

    private void ToolTipsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      hexViewer.ShowToolTips = toolTipsToolStripMenuItem.Checked;
    }

    private void UpdateSelection()
    {
      filePane.SelectFile(_fileName);
    }

    private void UpdateWindowTitle()
    {
      this.Text = !string.IsNullOrEmpty(_fileName) ? string.Format("{1} - {0}", Application.ProductName, Path.GetFileName(_fileName)) : Application.ProductName;
    }

    #endregion Private Methods
  }
}