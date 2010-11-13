﻿/*
 * Copyright 2010 Roland Leopold Walkling, Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.IO;

namespace Common.Storage
{
    /// <summary>
    /// Helper class to move an existing directory to a temporary directory within a <code>using</code> statement block.
    /// </summary>
    public sealed class TemporaryDirectoryMove : IDisposable
    {
        #region Properties
        /// <summary>
        /// The path were the directory was originally located.
        /// </summary>
        public string OriginalPath { get; private set; }

        /// <summary>
        /// The path were the directory was moved to.
        /// </summary>
        public string BackupPath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Renames an existing directory by moving it to a path generated by <see cref="Path.GetRandomFileName"/>.
        /// If the path doesn't point to anything, it does nothing.
        /// </summary>
        /// <param name="path">The path of the directory to move.</param>
        public TemporaryDirectoryMove(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            if (Directory.Exists(path))
            {
                // Create a sibling directory with a random name
                string inexistantPath = Path.Combine(Path.Combine(path, ".."), Path.GetRandomFileName());

                Directory.Move(path, inexistantPath);
                OriginalPath = path;
                BackupPath = inexistantPath;
            }
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Deletes the directory currently existing at the original path and moves the previously renamed directory to it's original path.
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(OriginalPath)) Directory.Delete(OriginalPath, true);
            Directory.Move(BackupPath, OriginalPath);
        }
        #endregion
    }
}
