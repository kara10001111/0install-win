﻿/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Common.Properties;
using Common.Utils;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace Common.Controls
{
    /// <summary>
    /// A text editor that automatically validates changes using an external callback after a short period of no input.
    /// </summary>
    public partial class LiveEditor : UserControl
    {
        /// <summary>
        /// Raised when changes have accumulated after a short period of no input.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Cannot rename System.Action<T>")]
        [Description("Raised when changes have accumulated after a short period of no input.")]
        public event Action<string> ContentChanged;

        /// <summary>
        /// The text editor control used internally.
        /// </summary>
        internal TextEditorControl TextEditor { get; private set; }

        public LiveEditor()
        {
            InitializeComponent();
        }

        #region Set content
        /// <summary>
        /// Sets a new text to be edited.
        /// </summary>
        /// <param name="text">The text to set.</param>
        /// <param name="format">The format named used to determine the highlighting scheme (e.g. XML).</param>
        public void SetContent(string text, string format)
        {
            if (TextEditor != null) Controls.Remove(TextEditor);

            TextEditor = new TextEditorControl
            {
                Location = new Point(0, 0),
                Size = Size - new Size(0, statusStrip.Height),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                TabIndex = 0,
                ShowVRuler = false,
                Document =
                {
                    TextContent = text,
                    HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(format)
                }
            };
            TextEditor.TextChanged += TextEditor_TextChanged;
            TextEditor.Validating += TextEditor_Validating;
            Controls.Add(TextEditor);

            SetStatus(Resources.Info, "OK");
        }
        #endregion

        //--------------------//

        #region Events
        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            TextEditor.Document.MarkerStrategy.RemoveAll(marker => true);

            if (timer.Enabled) timer.Stop();
            else SetStatus(null, "Changed...");
            timer.Start();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Invalid user input may cause arbitrary exceptions.")]
        private void TextEditor_Validating(object sender, CancelEventArgs e)
        {
            if (timer.Enabled)
            { // Ensure pending validation is not lost
                try
                {
                    ValidateContent();
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                    e.Cancel = true;
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Invalid user input may cause arbitrary exceptions.")]
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                ValidateContent();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        #endregion

        #region Validation
        private void ValidateContent()
        {
            timer.Stop();

            if (ContentChanged != null) ContentChanged(TextEditor.Text);
            SetStatus(Resources.Info, "OK");
            TextEditor.Document.UndoStack.ClearAll();
        }

        private void HandleError(Exception ex)
        {
            SetStatus(Resources.Error, ex.Message);

            if (ex is InvalidDataException && ex.Source == "System.Xml" && ex.InnerException != null)
            {
                if (MonoUtils.IsUnix)
                { // WORKAROUND: ICSharpCode.TextEditor's MarkerStrategy does not work on Mono
                    SetStatus(Resources.Error, ex.Message + ":" + ex.InnerException.Message);
                }
                else
                {
                    // Parse exception message for position of the error
                    int lineStart = ex.Message.LastIndexOf('(') + 1;
                    int lineLength = ex.Message.LastIndexOf(',') - lineStart;
                    int charStart = ex.Message.LastIndexOf(' ') + 1;
                    int charLength = ex.Message.LastIndexOf(')') - charStart;
                    int lineNumber = int.Parse(ex.Message.Substring(lineStart, lineLength)) - 1;
                    int charNumber = int.Parse(ex.Message.Substring(charStart, charLength)) - 1;

                    int lineOffset = TextEditor.Document.GetLineSegment(lineNumber).Offset;
                    TextEditor.Document.MarkerStrategy.AddMarker(
                        new TextMarker(lineOffset + charNumber, 10, TextMarkerType.WaveLine) {ToolTip = ex.InnerException.Message});
                    TextEditor.Refresh();
                }
            }
        }

        private void SetStatus(Image image, string message)
        {
            labelStatus.Image = image;
            labelStatus.Text = message;
        }
        #endregion
    }
}
