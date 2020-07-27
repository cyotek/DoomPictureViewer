using System.IO;

// Decoding DOOM Picture Files
// https://www.cyotek.com/blog/decoding-doom-picture-files
// Copyright © 2020 Cyotek Ltd. All Rights Reserved.

// This work is licensed under the Creative Commons Attribution 4.0 International License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/.

// Found this example useful?
// https://www.paypal.me/cyotek

namespace Cyotek.Demo
{
  internal class FileInfo
  {
    #region Private Fields

    private readonly string _fullPath;

    #endregion Private Fields

    #region Public Constructors

    public FileInfo(string fullPath)
    {
      _fullPath = fullPath;
    }

    #endregion Public Constructors

    #region Public Properties

    public string FullPath
    {
      get { return _fullPath; }
    }

    #endregion Public Properties

    #region Public Methods

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>
    /// A string that represents the current object.
    /// </returns>
    public override string ToString()
    {
      return Path.GetFileName(_fullPath);
    }

    #endregion Public Methods
  }
}