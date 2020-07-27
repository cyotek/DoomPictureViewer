﻿using System.IO;
using System.Windows.Forms;

// Decoding DOOM Picture Files
// https://www.cyotek.com/blog/decoding-doom-picture-files
// Copyright © 2020 Cyotek Ltd. All Rights Reserved.

// This work is licensed under the Creative Commons Attribution 4.0 International License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/.

// Found this example useful?
// https://www.paypal.me/cyotek

namespace Cyotek.Demo
{
  internal static class FileDialogHelper
  {
    #region Public Methods

    public static string GetFolderName(string description)
    {
      return FileDialogHelper.GetFolderName(description, string.Empty);
    }

    public static string GetFolderName(string description, string selectedPath)
    {
      using (FolderBrowserDialog dialog = new FolderBrowserDialog
      {
        Description = description,
        ShowNewFolderButton = true,
        SelectedPath = selectedPath
      })
      {
        return dialog.ShowDialog() == DialogResult.OK
          ? dialog.SelectedPath
          : null;
      }
    }

    public static string GetNextFileName(string path, string baseName)
    {
      string fileName;
      string name;
      string ext;
      int index;

      name = Path.GetFileNameWithoutExtension(baseName);
      ext = Path.GetExtension(baseName);
      index = 0;

      do
      {
        index++;
        fileName = Path.Combine(path, string.Format("{0}-{2}{1}", name, ext, index));
      } while (File.Exists(fileName));

      return fileName;
    }

    public static string GetOpenFileName(string title, string filter, string defaultExtension)
    {
      return FileDialogHelper.GetFileName<OpenFileDialog>(title, filter, defaultExtension, string.Empty);
    }

    public static string GetOpenFileName(string title, string filter, string defaultExtension, string defaultFileName)
    {
      return FileDialogHelper.GetFileName<OpenFileDialog>(title, filter, defaultExtension, defaultFileName);
    }

    public static string GetSaveFileName(string title, string filter, string defaultExtension)
    {
      return FileDialogHelper.GetFileName<SaveFileDialog>(title, filter, defaultExtension, string.Empty);
    }

    public static string GetSaveFileName(string title, string filter, string defaultExtension, string defaultFileName)
    {
      return FileDialogHelper.GetFileName<SaveFileDialog>(title, filter, defaultExtension, defaultFileName);
    }

    #endregion Public Methods

    #region Private Methods

    private static string GetFileName<T>(string title, string filter, string defaultExtension, string defaultFileName)
      where T : FileDialog, new()
    {
      using (T dialog = new T
      {
        Title = title,
        DefaultExt = defaultExtension,
        Filter = filter,
        FileName = defaultFileName
      })
      {
        return dialog.ShowDialog() == DialogResult.OK
          ? dialog.FileName
          : null;
      }
    }

    #endregion Private Methods
  }
}