﻿/*
 * Copyright 2010-2012 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Storage;
using Common.Tasks;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Uses <see cref="System.Windows.Forms"/> to inform the user about the progress of tasks and ask the user questions.
    /// </summary>
    /// <remarks>
    /// This class is heavily multi-threaded. The UI is prepared in a background thread to allow simultaneous continuation of computation.
    /// Any calls relying on the UI to aquire user input will block automatically.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposal is handled sufficiently by GC in this case")]
    public class GuiHandler : MarshalByRefObject, IHandler
    {
        #region Variables
        private ProgressForm _form;

        /// <summary>Synchronization object used to prevent multiple concurrent generic <see cref="ITask"/>s.</summary>
        private readonly object _genericTaskLock = new object();

        /// <summary>A barrier that blocks threads until the <see cref="_form"/>'s handle is ready.</summary>
        private readonly ManualResetEvent _guiReady = new ManualResetEvent(false);

        /// <summary>A wait handle used by <see cref="AuditSelections"/> to be signaled once the user is satisfied with the <see cref="Selections"/>.</summary>
        private readonly AutoResetEvent _auditWaitHandle = new AutoResetEvent(false);
        #endregion

        #region Properties
        private readonly CancellationToken _cancellationToken = new CancellationToken();

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get { return _cancellationToken; } }

        /// <inheritdoc />
        public bool Batch { get; set; }

        private string _actionTitle;

        /// <inheritdoc />
        public void SetGuiHints(string actionTitle, int delay)
        {
            _actionTitle = actionTitle;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new GUI handler with an external <see cref="CancellationToken"/>.
        /// </summary>
        public GuiHandler(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Creates a new GUI handler with its own <see cref="CancellationToken"/>.
        /// </summary>
        public GuiHandler() : this(new CancellationToken())
        {}
        #endregion

        //--------------------//

        #region Task tracking
        /// <inheritdoc />
        public void RunTask(ITask task, object tag)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            // If GUI does not exist or was closed cancel, otherwise wait until it is ready
            if (_form == null) return;
            _guiReady.WaitOne();

            if (tag is ManifestDigest)
            {
                // Handle events coming from a non-UI thread, don't block caller
                _form.BeginInvoke(new Action(() => { if (_form.IsHandleCreated) _form.TrackTask(task, (ManifestDigest)tag); }));
            }
            else
            {
                lock (_genericTaskLock) // Prevent multiple concurrent generic tasks
                {
                    // Handle events coming from a non-UI thread, don't block caller
                    _form.BeginInvoke(new Action(() => { if (_form.IsHandleCreated) _form.TrackTask(task); }));
                }
            }

            if (!_cancellationToken.IsCancellationRequested)
                task.RunSync(_cancellationToken);
        }
        #endregion

        #region UI control
        /// <inheritdoc/>
        public void ShowProgressUI()
        {
            // Can only show GUI once
            if (_form != null) return;

            _form = new ProgressForm(delegate
            { // Cancel callback
                _cancellationToken.RequestCancellation();
                _auditWaitHandle.Set();
            });

            // Initialize GUI with a low priority
            var thread = new Thread(GuiThread);
            thread.SetApartmentState(ApartmentState.STA); // Make COM work
            thread.Start();
        }

        /// <summary>
        /// Runs a message pump for the GUI.
        /// </summary>
        private void GuiThread()
        {
            _form.Initialize();
            if (_actionTitle != null) _form.Text = _actionTitle;
            if (Locations.IsPortable) _form.Text += @" - " + Resources.PortableMode;

            // Show the tray icon or the form
            if (Batch) _form.ShowTrayIcon(_actionTitle, ToolTipIcon.None);
            else _form.Show();

            // Start the message loop and set the wait handle as soon as it is running
            Application.Idle += delegate { _guiReady.Set(); };
            Application.Run();
        }

        /// <inheritdoc/>
        public void DisableProgressUI()
        {
            // If GUI does not exist or was closed cancel, otherwise wait until it is ready
            if (_form == null) return;
            _guiReady.WaitOne();

            _form.Invoke(new Action(() => { if (_form.IsHandleCreated) _form.Enabled = false; }));
        }

        /// <inheritdoc/>
        public void CloseProgressUI()
        {
            // If GUI does not exist or was closed cancel, otherwise wait until it is ready
            if (_form == null) return;
            _guiReady.WaitOne();

            try
            {
                _form.Invoke(new Action(() =>
                {
                    if (_form.IsHandleCreated) _form.HideTrayIcon();
                    Application.ExitThread();
                    _form.Dispose();
                }));
                _form = null;
                _guiReady.Reset();
            }
            catch (InvalidOperationException)
            {
                // Don't worry if the form was disposed in the meantime
            }
        }
        #endregion

        #region Question
        /// <inheritdoc />
        public bool AskQuestion(string question, string batchInformation)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(question)) throw new ArgumentNullException("question");
            #endregion

            // If GUI does not exist or was closed cancel, otherwise wait until it is ready
            if (_form == null) return false;
            _guiReady.WaitOne();

            // Handle events coming from a non-UI thread, block caller until user has answered
            bool result = false;
            _form.Invoke(new Action(delegate
            {
                if (!_form.IsHandleCreated) return;

                // Auto-deny unknown keys and inform via tray icon when in batch mode
                if (Batch && !string.IsNullOrEmpty(batchInformation)) _form.ShowTrayIcon(batchInformation, ToolTipIcon.Warning);
                else
                {
                    switch (Msg.YesNoCancel(_form, question, MsgSeverity.Info))
                    {
                        case DialogResult.Yes:
                            result = true;
                            break;
                        case DialogResult.No:
                            result = false;
                            break;
                        default:
                            throw new OperationCanceledException();
                    }
                }
            }));
            return result;
        }
        #endregion

        #region Selections UI
        /// <inheritdoc/>
        public void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            // If GUI does not exist or was closed cancel, otherwise wait until it is ready
            if (_form == null) return;
            _guiReady.WaitOne();

            try
            {
                _form.Invoke(new Action(() => { if (_form.IsHandleCreated) _form.ShowSelections(selections, feedCache); }));
            }
            catch (InvalidOperationException)
            {
                // Don't worry if the form was disposed in the meantime
            }
        }

        /// <inheritdoc/>
        public void AuditSelections(Func<Selections> solveCallback)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            #endregion

            // If GUI does not exist or was closed cancel, otherwise wait until it is ready
            if (_form == null) return;
            _guiReady.WaitOne();
            if (!_form.IsHandleCreated) return;

            // Show selection auditing screen and then asynchronously wait until its done
            _form.Invoke(new Action(() => { if (_form.IsHandleCreated) _form.Show(); })); // Leave tray icon mode
            _form.Invoke(new Action(() => _form.BeginAuditSelections(solveCallback, _auditWaitHandle)));
            _auditWaitHandle.WaitOne();
        }
        #endregion

        #region Messages
        /// <inheritdoc />
        public void Output(string title, string information)
        {
            DisableProgressUI();
            if (Batch) ShowBalloonMessage(title, information);
            else OutputBox.Show(title, information);
        }

        /// <summary>
        /// Displays a tray icon with balloon message detached from the main GUI (will stick around even after the process ends).
        /// </summary>
        /// <param name="title">The title of the balloon message.</param>
        /// <param name="information">The balloon message text.</param>
        private void ShowBalloonMessage(string title, string information)
        {
            // Remove existing tray icon to give new balloon priority
            _form.Invoke(new Action(() => { if (_form.IsHandleCreated) _form.HideTrayIcon(); }));

            var icon = new NotifyIcon {Visible = true, Icon = Resources.TrayIcon};
            icon.ShowBalloonTip(10000, title, information, ToolTipIcon.Info);
        }
        #endregion

        #region Dialogs
        /// <inheritdoc/>
        public void ShowIntegrateApp(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            // If GUI does not exist or was closed cancel, otherwise wait until it is ready
            if (_form == null) return;
            _guiReady.WaitOne();
            if (!_form.IsHandleCreated) return;

            var integrationForm = new IntegrateAppForm(integrationManager, appEntry, feed);
            integrationForm.VisibleChanged += delegate
            { // The IntegrateAppForm and ProgressForm take turns in being visible
                _form.Invoke(new Action(delegate
                {
                    // Prevent ProgressForm from flashing up again when the user cancels
                    _form.Visible = !integrationForm.Visible && (integrationForm.DialogResult != DialogResult.Cancel);
                    if (integrationForm.Visible) _form.HideTrayIcon();
                }));
            };
            integrationForm.ShowDialog();
        }

        /// <inheritdoc/>
        public bool ShowConfig(Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            return (ConfigForm.ShowDialog(config) == DialogResult.OK);
        }
        #endregion
    }
}
