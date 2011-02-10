﻿/*
 * Copyright 2010 Simon E. Silva Lauinger, Bastian Eicher
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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Undo;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Publish.WinForms.FeedStructure;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Publish.WinForms.Controls;
using Binding = ZeroInstall.Model.Binding;
using Icon = ZeroInstall.Model.Icon;

namespace ZeroInstall.Publish.WinForms
{
    public partial class MainForm : Form
    {
        #region Events
        /// <summary>To be called when the controls on the form need to filled with content from the feed.</summary>
        private event SimpleEventHandler Populate;

        /// <summary>To be called when the <see cref="treeViewFeedStructure"/> on the form need to filled with content from the feed.</summary>
        private event SimpleEventHandler UpdateStructureButtons;
        #endregion

        #region Constants
        private const string FeedFileFilter = "Zero Install Feed (*.xml)|*.xml|All Files|*.*";
        private readonly string[] _supportedInjectorVersions = new[] { "", "0.31", "0.32", "0.33", "0.34",
            "0.35", "0.36", "0.37", "0.38", "0.39", "0.40", "0.41", "0.41.1", "0.42", "0.42.1", "0.43", "0.44", "0.45"};
        #endregion

        #region Variables
        private FeedEditing _feedEditing = new FeedEditing();
        #endregion

        #region Properties
        /// <summary>
        /// Returns part of the <see cref="Feed"/> currently selected in the <see cref="treeViewFeedStructure"/>.
        /// </summary>
        private object SelectedFeedStructureElement
        {
            get
            {
                // Default to the top-level of the feed if nothing is selected
                return (treeViewFeedStructure.SelectedNode == null ? _feedEditing.Feed : treeViewFeedStructure.SelectedNode.Tag);
            }
        }
        #endregion

        #region Initialization

        /// <summary>
        /// Creats a new <see cref="MainForm"/> object.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            InitializeComponentsExtended();
            InitializeCommandHooks();
            InitializeEditingHooks();
        }

        /// <summary>
        /// Initializes settings of the form components which can't be setted by the property grid.
        /// </summary>
        private void InitializeComponentsExtended()
        {
            InitializeSaveFileDialog();
            InitializeLoadFileDialog();
            InitializeTreeViewFeedStructure();
            InitializeFeedStructureButtons();
            InitializeComboBoxMinInjectorVersion();
            InitializeComboBoxGpg();
        }

        /// <summary>
        /// Initializes the <see cref="saveFileDialog"/> with a file filter for .xml files.
        /// </summary>
        private void InitializeSaveFileDialog()
        {
            if (_feedEditing.Path != null) saveFileDialog.InitialDirectory = _feedEditing.Path;
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = FeedFileFilter;
        }

        /// <summary>
        /// Initializes the <see cref="openFileDialog"/> with a file filter for .xml files.
        /// </summary>
        private void InitializeLoadFileDialog()
        {
            if (_feedEditing.Path != null) openFileDialog.InitialDirectory = _feedEditing.Path;
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = FeedFileFilter;
        }

        /// <summary>
        /// Adds <see cref="Feed"/> to the Tag of the first <see cref="TreeNode"/> of <see cref="treeViewFeedStructure"/>.
        /// </summary>
        private void InitializeTreeViewFeedStructure()
        {
            treeViewFeedStructure.Nodes[0].Tag = _feedEditing.Feed;
        }

        /// <summary>
        /// Configures buttons to add and remove entries from the <see cref="treeViewFeedStructure"/> and the coressponding backend model.
        /// </summary>
        private void InitializeFeedStructureButtons()
        {
            SetupFeedStructureHooks<IElementContainer, Element, Implementation>(btnAddImplementation, container => container.Elements, implementation => new ImplementationForm {Implementation = implementation});
            SetupFeedStructureHooks<IElementContainer, Element, PackageImplementation>(btnAddPackageImplementation, container => container.Elements, implementation => new PackageImplementationForm {PackageImplementation = implementation});
            SetupFeedStructureHooks<IElementContainer, Element, Group>(btnAddGroup, container => container.Elements, group => new GroupForm {Group = group});

            SetupFeedStructureHooks<IBindingContainer, Binding, EnvironmentBinding>(btnAddEnvironmentBinding, container => container.Bindings, binding => new EnvironmentBindingForm {EnvironmentBinding = binding});
            SetupFeedStructureHooks<IBindingContainer, Binding, OverlayBinding>(btnAddOverlayBinding, container => container.Bindings, binding => new OverlayBindingForm {OverlayBinding = binding});

            SetupFeedStructureHooks<IDependencyContainer, Dependency, Dependency>(btnAddDependency, container => container.Dependencies, dependency => new DependencyForm {Dependency = dependency});

            // ToDo: Add Command dialog
            SetupFeedStructureHooks<Element, Command, Command>(btnAddCommand, element => element.Commands, null);

            // ToDo: Add special case handling
            SetupFeedStructureHooks<Implementation, RetrievalMethod, Archive>(buttonAddArchive, implementation => implementation.RetrievalMethods, null);
            SetupFeedStructureHooks<Implementation, RetrievalMethod, Recipe>(buttonAddRecipe, implementation => implementation.RetrievalMethods, null);
        }

        /// <summary>
        /// A delegate describing how to get the collection of <typeparamref name="TEntry"/>s in a <typeparamref cref="TContainer"/>.
        /// </summary>
        private delegate ICollection<TEntry> ContainerCollectionRetreival<TContainer, TEntry>(TContainer container);

        // ToDo: Move to Common
        private delegate TResult TempDeleg<TResult, TInput>(TInput input);

        /// <summary>
        /// Configures event handlers to make a button add new elements to <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <typeparam name="TContainer">The type of element in the <see cref="treeViewFeedStructure"/> that this button can can add sub-elements to.</typeparam>
        /// <typeparam name="TGeneralEntry">ToDo</typeparam>
        /// <typeparam name="TSpecialEntry">ToDo</typeparam>
        /// <param name="button">The button to hook up.</param>
        /// <param name="getCollection">A delegate describing how to get the collection of <typeparamref name="TSpecialEntry"/>s in a <typeparamref cref="TContainer"/>.</param>
        /// <param name="getEditDialog">ToDo</param>
        private void SetupFeedStructureHooks<TContainer, TGeneralEntry, TSpecialEntry>(Button button, ContainerCollectionRetreival<TContainer, TGeneralEntry> getCollection, TempDeleg<Form, TSpecialEntry> getEditDialog)
            where TContainer : class
            where TSpecialEntry : class, TGeneralEntry, new()
        {
            button.Click += delegate
            {
                if (treeViewFeedStructure.SelectedNode == null) return;
                var container = SelectedFeedStructureElement as TContainer;
                if (container != null)
                {
                    _feedEditing.ExecuteCommand(new AddToCollection<TGeneralEntry>(getCollection(container), new TSpecialEntry()));
                    FillFeedTab();
                }
            };
            UpdateStructureButtons += () => button.Enabled = SelectedFeedStructureElement is TContainer;

            // Hook up edit dialogs to TreeView
            treeViewFeedStructure.DoubleClick += delegate
            {
                var entry = SelectedFeedStructureElement as TSpecialEntry;
                if (entry != null)
                {
                    getEditDialog(entry).ShowDialog();
                    FillFeedTab();
                }
            };
        }

        private void InitializeComboBoxMinInjectorVersion()
        {
            comboBoxMinInjectorVersion.Items.AddRange(_supportedInjectorVersions);
        }

        /// <summary>
        /// Adds a list of secret gpg keys of the user to comboBoxGpg.
        /// </summary>
        private void InitializeComboBoxGpg()
        {
            toolStripComboBoxGpg.Items.Add(string.Empty);

            foreach (var secretKey in GetGnuPGSecretKeys())
            {
                toolStripComboBoxGpg.Items.Add(secretKey);
            }
        }

        /// <summary>
        /// Returns all GnuPG secret keys of the user. If GnuPG can not be found on the system, a message box informs the user.
        /// </summary>
        /// <returns>The GnuPG secret keys.</returns>
        private IEnumerable<GnuPGSecretKey> GetGnuPGSecretKeys()
        {
            try
            {
                return new GnuPG().ListSecretKeys();
            }
            catch (IOException)
            {
                Msg.Inform(this, "GnuPG could not be found on your system.\nYou can not sign feeds.",
                           MsgSeverity.Warn);
                return new GnuPGSecretKey[0];
            }
        }

        /// <summary>
        /// Sets up hooks for keeping the WinForms controls synchronized with the <see cref="Feed"/> data using the command pattern.
        /// </summary>
        private void InitializeCommandHooks()
        {
            SetupCommandHooks(textName, () => _feedEditing.Feed.Name, value => _feedEditing.Feed.Name = value);
            SetupCommandHooks(checkedListBoxCategories, () => _feedEditing.Feed.Categories);
            SetupCommandHooks(textInterfaceUri, () => _feedEditing.Feed.Uri, value => _feedEditing.Feed.Uri = value);
            SetupCommandHooks(textHomepage, () => _feedEditing.Feed.Homepage, value => _feedEditing.Feed.Homepage = value);
            SetupCommandHooks(summariesControl, () => _feedEditing.Feed.Summaries);
            SetupCommandHooks(descriptionControl, () => _feedEditing.Feed.Descriptions);
            SetupCommandHooks(checkBoxNeedsTerminal, () => _feedEditing.Feed.NeedsTerminal, value => _feedEditing.Feed.NeedsTerminal = value);
            SetupCommandHooks(iconManagementControl, () => _feedEditing.Feed.Icons);
            SetupCommandHooks(comboBoxMinInjectorVersion, () => _feedEditing.Feed.MinInjectorVersionString, value => _feedEditing.Feed.MinInjectorVersionString = value);
        }

        /// <summary>
        /// Sets up event handlers for <see cref="_feedEditing"/> integration.
        /// </summary>
        private void InitializeEditingHooks()
        {
            _feedEditing.Update += OnUpdate;
            _feedEditing.UndoEnabled += value => buttonUndo.Enabled = value;
            _feedEditing.RedoEnabled += value => buttonRedo.Enabled = value;

            buttonUndo.Enabled = buttonRedo.Enabled = false;
        }

        #endregion

        #region Undo/Redo

        private void OnUpdate()
        {
            FillForm();
            if (Populate != null) Populate();
        }

        /// <summary>
        /// Hooks up a <see cref="UriTextBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the corresponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the corresponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(UriTextBox textBox, SimpleResult<Uri> getValue, Action<Uri> setValue)
        {
            // Transfer data from the feed to the TextBox when refreshing
            Populate += delegate
            {
                textBox.CausesValidation = false;
                textBox.Uri = getValue();
                textBox.CausesValidation = true;
            };

            // Transfer data from the TextBox to the feed via a command object
            textBox.Validating += (sender, e) =>
            {
                // Detect lower-level validation failures
                if (e.Cancel) return;

                // Ignore irrelevant changes
                if (textBox.Uri == getValue()) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<Uri>(textBox.Uri, getValue, setValue));
            };

            // Enable the undo button even before the command has been created
            textBox.KeyPress += delegate
            {
                try { _feedEditing.UpdateButtonStatus(textBox.Uri != getValue()); }
                catch (UriFormatException) {}
            };
            textBox.ClearButtonClicked += delegate
            {
                try { _feedEditing.UpdateButtonStatus(textBox.Uri != getValue()); }
                catch (UriFormatException) {}
            };
        }
        
        /// <summary>
        /// Hooks up a <see cref="HintTextBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the corresponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the corresponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(HintTextBox textBox, SimpleResult<string> getValue, Action<string> setValue)
        {
            SetupCommandHooks((TextBox)textBox, getValue, setValue);

            // Enable the undo button even before the command has been created
            textBox.ClearButtonClicked += delegate { _feedEditing.UpdateButtonStatus(StringUtils.CompareEmptyNull(textBox.Text, getValue())); };
        }

        /// <summary>
        /// Hooks up a <see cref="TextBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the corresponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the corresponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(TextBox textBox, SimpleResult<string> getValue, Action<string> setValue)
        {
            // Transfer data from the feed to the TextBox when refreshing
            Populate += delegate
            {
                textBox.CausesValidation = false;
                textBox.Text = getValue();
                textBox.CausesValidation = true;
            };

            // Transfer data from the TextBox to the feed via a command object
            textBox.Validating += (sender, e) =>
            {
                // Detect lower-level validation failures
                if (e.Cancel) return;

                // Ignore irrelevant changes
                if (StringUtils.CompareEmptyNull(textBox.Text, getValue())) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<string>(textBox.Text, getValue, setValue));
            };

            // Enable the undo button even before the command has been created
            textBox.KeyPress += delegate { _feedEditing.UpdateButtonStatus(StringUtils.CompareEmptyNull(textBox.Text, getValue())); };
        }

        /// <summary>
        /// Hooks up a <see cref="CheckBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="checkBox">The <see cref="TextBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the corresponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the corresponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(CheckBox checkBox, SimpleResult<bool> getValue, Action<bool> setValue)
        {
            // Transfer data from the feed to the CheckBox when refreshing
            Populate += delegate
            {
                checkBox.CausesValidation = false;
                checkBox.Checked = getValue();
                checkBox.CausesValidation = true;
            };

            // Transfer data from the CheckBox to the feed via a command object
            checkBox.Validating += (sender, e) =>
            {
                // Detect lower-level validation failures
                if (e.Cancel) return;

                // Ignore irrelevant changes
                if (checkBox.Checked == getValue()) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<bool>(checkBox.Checked, getValue, setValue));
            };
        }

        /// <summary>
        /// Hooks up a <see cref="CheckedListBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="checkedListBox">The <see cref="CheckedListBox"/> to track for input and to update.</param>
        /// <param name="getCollection">A delegate that reads the corresponding value from the collection.</param>
        private void SetupCommandHooks(CheckedListBox checkedListBox, SimpleResult<ICollection<string>> getCollection)
        {
            ItemCheckEventHandler itemCheckEventHandler = (sender, e) =>
            {
                switch (e.NewValue)
                {
                    case CheckState.Checked:
                        _feedEditing.ExecuteCommand(new AddToCollection<string>(getCollection(), checkedListBox.Items[e.Index].ToString()));
                        break;
                    case CheckState.Unchecked:
                        _feedEditing.ExecuteCommand(new RemoveFromCollection<string>(getCollection(), checkedListBox.Items[e.Index].ToString()));
                        break;
                }
            };

            Populate += delegate
            {
                checkedListBox.ItemCheck -= itemCheckEventHandler;
                for(int i = 0; i < checkedListBox.Items.Count; i++)
                {
                    if(getCollection().Contains(checkedListBox.Items[i].ToString()))
                    {
                        checkedListBox.SetItemChecked(i, true);
                    }
                }
                checkedListBox.ItemCheck += itemCheckEventHandler;
            };

            checkedListBox.ItemCheck += itemCheckEventHandler;
        }

        /// <summary>
        /// Hooks up a <see cref="ComboBox"/> for automatic synchronization with the <see cref="Feed"/> via command objects.
        /// </summary>
        /// <param name="comboBox">The <see cref="ComboBox"/> to track for input and to update.</param>
        /// <param name="getValue">A delegate that reads the corresponding value from the <see cref="Feed"/>.</param>
        /// <param name="setValue">A delegate that sets the corresponding value in the <see cref="Feed"/>.</param>
        private void SetupCommandHooks(ComboBox comboBox, SimpleResult<string> getValue, Action<string> setValue)
        {
            Populate += delegate
            {
                comboBox.CausesValidation = false;
                comboBox.SelectedItem = getValue() ?? string.Empty;
                comboBox.CausesValidation = true;
            };

            comboBox.Validating += (sender, e) =>
            {
                if (e.Cancel) return;

                // Normalize unselected or default entry to null
                string selectedValue = (comboBox.SelectedItem ?? "").ToString();
                if (selectedValue == "") selectedValue = null;

                if (selectedValue == getValue()) return;

                _feedEditing.ExecuteCommand(new SetValueCommand<string>(selectedValue, getValue, setValue));
            };

        }

        private void SetupCommandHooks(LocalizableTextControl localizableTextControl, SimpleResult<LocalizableStringCollection> getCollection)
        {

            localizableTextControl.Values.ItemsAdded += (sender, itemCountEventArgs) => _feedEditing.ExecuteCommand(new AddToCollection<LocalizableString>(getCollection(), itemCountEventArgs.Item));

            localizableTextControl.Values.ItemsRemoved += (sender, itemCountEventArgs) => _feedEditing.ExecuteCommand(new RemoveFromCollection<LocalizableString>(getCollection(), itemCountEventArgs.Item));

            Populate += delegate
            {
                localizableTextControl.Values = getCollection();
            };
        }

        private void SetupCommandHooks(IconManagementControl iconManagementControl, SimpleResult<C5.ArrayList<Icon>> getCollection)
        {
            iconManagementControl.IconUrls.ItemInserted +=(sender, eventArgs) => _feedEditing.ExecuteCommand(new AddToCollection<Icon>(getCollection(), eventArgs.Item));
            
            iconManagementControl.IconUrls.ItemsRemoved += (sender, eventArgs) => _feedEditing.ExecuteCommand(new RemoveFromCollection<Icon>(getCollection(), eventArgs.Item));

            Populate += () => iconManagementControl.IconUrls = getCollection();
        }
        #endregion

        #region Toolbar

        /// <summary>
        /// Sets all controls on the <see cref="MainForm"/> to default values.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ToolStripButtonNew_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            if (_feedEditing.Changed)
            {
                if (!AskSave()) return;
            }

            _feedEditing = new FeedEditing();
            OnUpdate();
            InitializeEditingHooks();
        }

        /// <summary>
        /// Shows a dialog to open a new <see cref="ZeroInstall.Model"/> for editing.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            if (_feedEditing.Changed)
            {
                if (!AskSave()) return;
            }

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _feedEditing = FeedEditing.Load(openFileDialog.FileName);
                }
                    #region Error handling

                catch (InvalidOperationException)
                {
                    Msg.Inform(this, "The feed you tried to open is not valid.", MsgSeverity.Error);
                }
                catch (UnauthorizedAccessException exception)
                {
                    Msg.Inform(this, exception.Message, MsgSeverity.Error);
                }
                catch (IOException exception)
                {
                    Msg.Inform(this, exception.Message, MsgSeverity.Error);
                }

                #endregion

                InitializeEditingHooks();

                OnUpdate();
            }
        }

        /// <summary>
        /// Shows a dialog to save the edited <see cref="ZeroInstall.Model"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ToolStripButtonSave_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            Save();
        }

        private void toolStripButtonSaveAs_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            SaveAs();
        }

        private void buttonUndo_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            _feedEditing.Undo();
        }

        private void buttonRedo_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            _feedEditing.Redo();
        }

        #endregion

        #region Save and open

        /// <summary>
        /// Saves feed to a specific path as xml.
        /// </summary>
        /// <param name="toPath">Path to save.</param>
        private void SaveFeed(string toPath)
        {
            SaveAdvancedTab();

            _feedEditing.Save(toPath);
            SignFeed(toPath);
        }

        /// <summary>
        /// Saves the values from <see cref="tabPageAdvanced"/>.
        /// </summary>
        private void SaveAdvancedTab()
        {
            _feedEditing.Feed.Feeds.Clear();
            foreach (var feed in listBoxExternalFeeds.Items) _feedEditing.Feed.Feeds.Add((FeedReference) feed);

            _feedEditing.Feed.FeedFor.Clear();
            foreach (InterfaceReference feedFor in listBoxFeedFor.Items) _feedEditing.Feed.FeedFor.Add(feedFor);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_feedEditing.Changed)
            {
                if (!AskSave()) e.Cancel = true;
            }
        }

        /// <summary>
        /// Asks the user whether he wants to save the feed.
        /// </summary>
        /// <returns><see langword="true"/> if all went well (either Yes or No), <see langword="false"/> if the user chose to cancel.</returns>
        private bool AskSave()
        {
            switch (
                Msg.Choose(this, "Do you want to save the changes you made?", MsgSeverity.Info, true,
                           "&Save\nSave the file and then close", "&Don't save\nIgnore the unsaved changes"))
            {
                case DialogResult.Yes:
                    return Save();

                case DialogResult.No:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Saves the feed at at a new location.
        /// </summary>
        /// <returns>The result of the "Save as" common dialog box used.</returns>
        /// <returns><see langword="true"/> if all went well, <see langword="false"/> if the user chose to cancel.</returns>
        private bool SaveAs()
        {
            saveFileDialog.FileName = _feedEditing.Path;
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                SaveFeed(saveFileDialog.FileName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves the feed at its original location.
        /// </summary>
        /// <returns><see langword="true"/> if all went well, <see langword="false"/> if the user chose to cancel.</returns>
        private bool Save()
        {
            if (string.IsNullOrEmpty(_feedEditing.Path)) return SaveAs();
            else
            {
                SaveFeed(_feedEditing.Path);
                return true;
            }
        }

        /// <summary>
        /// Asks the user for his/her GnuPG passphrase and adds the Base64 signature of the given file to the end of it.
        /// </summary>
        /// <param name="path">The feed file to sign.</param>
        private void SignFeed(string path)
        {
            bool wrongPassphrase = false;

            if (string.IsNullOrEmpty(toolStripComboBoxGpg.Text)) return;
            var key = (GnuPGSecretKey) toolStripComboBoxGpg.SelectedItem;
            do
            {
                string passphrase = InputBox.Show(
                    (wrongPassphrase
                         ? "Wrong passphrase entered.\nPlease retry entering the GnuPG passphrase for "
                         : "Please enter the GnuPG passphrase for ") + key.UserID,
                    "Enter GnuPG passphrase", String.Empty, true);

                if (passphrase == null) return;

                try
                {
                    FeedUtils.SignFeed(path, key.KeyID, passphrase);
                }
                catch (WrongPassphraseException)
                {
                    wrongPassphrase = true;
                }
            } while (wrongPassphrase);
        }

        #endregion

        #region Fill form controls

        /// <summary>
        /// Clears all form controls and fills them with the values from a <see cref="Feed"/>.
        /// </summary>
        private void FillForm()
        {
            ResetFormControls();

            FillFeedTab();
            FillAdvancedTab();
        }

        /// <summary>
        /// Fills the <see cref="tabPageFeed"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        private void FillFeedTab()
        {
            treeViewFeedStructure.BeginUpdate();
            treeViewFeedStructure.Nodes[0].Nodes.Clear();
            treeViewFeedStructure.Nodes[0].Tag = _feedEditing.Feed;
            BuildElementsTreeNodes(_feedEditing.Feed.Elements, treeViewFeedStructure.Nodes[0]);
            treeViewFeedStructure.EndUpdate();

            treeViewFeedStructure.ExpandAll();

            if (UpdateStructureButtons != null) UpdateStructureButtons();
        }

        /// <summary>
        /// Fills the <see cref="tabPageAdvanced"/> with the values from a <see cref="ZeroInstall.Model.Feed"/>.
        /// </summary>
        private void FillAdvancedTab()
        {
            foreach (var feed in _feedEditing.Feed.Feeds) listBoxExternalFeeds.Items.Add(feed);
            foreach (var feedFor in _feedEditing.Feed.FeedFor) listBoxFeedFor.Items.Add(feedFor);
        }

        #endregion

        #region Reset form controls

        /// <summary>
        /// Sets all controls on the <see cref="MainForm"/> to default values.
        /// </summary>
        private void ResetFormControls()
        {
            ResetFeedTabControls();
            ResetAdvancedTabControls();
        }

        /// <summary>
        /// Sets all controls on <see cref="tabPageFeed"/> to default values.
        /// </summary>
        private void ResetFeedTabControls()
        {
            treeViewFeedStructure.Nodes[0].Nodes.Clear();
        }

        /// <summary>
        /// Sets all controls on <see cref="tabPageAdvanced"/> to default values.
        /// </summary>
        private void ResetAdvancedTabControls()
        {
            listBoxExternalFeeds.Items.Clear();
            hintTextBoxFeedFor.Clear();
            listBoxFeedFor.Items.Clear();
            feedReferenceControl.FeedReference = null;
        }

        #endregion

        #region Tabs

        #region Feed Tab

        #region treeviewFeedStructure methods

        /// <summary>
        /// Enables the buttons which allow the user to add specific new <see cref="TreeNode"/>s in subject to the selected <see cref="TreeNode"/>.
        /// For example: The user selected a "Dependency"-node. Now only the buttons <see cref="btnAddEnvironmentBinding"/> and <see cref="btnAddOverlayBinding"/> will be enabled.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TreeViewFeedStructureAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (UpdateStructureButtons != null) UpdateStructureButtons();
        }

        /// <summary>
        /// Opens a new window to edit the selected entry.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TreeViewFeedStructureNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null || selectedNode == treeViewFeedStructure.Nodes[0]) return;

            selectedNode.Toggle();

            // show a dialog to change the selected object
            if (selectedNode.Tag is Archive)
            {
                var archiveForm = new ArchiveForm {Archive = (Archive) selectedNode.Tag};
                if (archiveForm.ShowDialog() != DialogResult.OK) return;

                var manifestDigestFromArchive = archiveForm.ManifestDigest;
                var implementationNode = selectedNode.Parent;

                if (implementationNode.FirstNode.Tag is ManifestDigest)
                {
                    var existingManifestDigest = (ManifestDigest) implementationNode.FirstNode.Tag;
                    if (ControlHelpers.IsEmpty(existingManifestDigest))
                    {
                        implementationNode.FirstNode.Tag = manifestDigestFromArchive;
                        ((Implementation) implementationNode.Tag).ManifestDigest = manifestDigestFromArchive;
                    }
                    else if (
                        !ControlHelpers.CompareManifestDigests(existingManifestDigest, manifestDigestFromArchive))
                    {
                        Msg.Inform(this,
                                   "The manifest digest of this archive is not the same as the manifest digest of the other archives. The archive was discarded.",
                                   MsgSeverity.Warn);
                        selectedNode.Tag = new Archive();
                        return;
                    }
                }
                else
                {
                    InsertManifestDigestNode(implementationNode, manifestDigestFromArchive);
                }
                var implementation = (Implementation) implementationNode.Tag;

                if (String.IsNullOrEmpty(implementation.ID) || implementation.ID.StartsWith("sha1new="))
                {
                    implementation.ID = "sha1new=" + manifestDigestFromArchive.Sha1New;
                }
            }
            else if (selectedNode.Tag is Recipe)
            {
                var recipeForm = new RecipeForm {Recipe = (Recipe) selectedNode.Tag};
                if (recipeForm.ShowDialog() != DialogResult.OK) return;

                var manifestDigestFromRecipe = recipeForm.ManifestDigest;
                var implementationNode = selectedNode.Parent;

                if (implementationNode.FirstNode.Tag is ManifestDigest)
                {
                    var existingManifestDigest = (ManifestDigest) implementationNode.FirstNode.Tag;
                    if (ControlHelpers.IsEmpty(existingManifestDigest))
                    {
                        implementationNode.FirstNode.Tag = manifestDigestFromRecipe;
                        ((Implementation) implementationNode.Tag).ManifestDigest = manifestDigestFromRecipe;
                    }
                    else if (!ControlHelpers.CompareManifestDigests(existingManifestDigest, manifestDigestFromRecipe))
                    {
                        Msg.Inform(this,
                                   "The manifest digest of this recipe is not the same as the manifest digest of the other retrieval methods. The recipe was discarded.",
                                   MsgSeverity.Warn);
                        selectedNode.Tag = new Recipe {Steps = {new Archive()}};
                        return;
                    }
                }
                else
                {
                    InsertManifestDigestNode(selectedNode.Parent, manifestDigestFromRecipe);
                }
            }
        }

        /// <summary>
        /// Inserts a new <see cref="TreeNode"/> to the first position of <paramref name="insertInto"/>.
        /// Adds <paramref name="toAddToTag"/> to the Tag of this <see cref="TreeNode"/>.
        /// </summary>
        /// <param name="insertInto">New <see cref="TreeNode"/> will be inserted to this <see cref="TreeNode"/>s first position.</param>
        /// <param name="toAddToTag"><see cref="ManifestDigest"/> to add to the Tag of the new <see cref="TreeNode"/>.</param>
        private static void InsertManifestDigestNode(TreeNode insertInto, ManifestDigest toAddToTag)
        {
            var manifestDigestNode = new TreeNode("Manifest digest") {Tag = toAddToTag};
            ((Implementation) insertInto.Tag).ManifestDigest = toAddToTag;
            insertInto.Nodes.Insert(0, manifestDigestNode);
        }

        /// <summary>
        /// Removes the Tag of the selected <see cref="TreeNode"/> from <see cref="Feed"/> and rebuilds <see cref="treeViewFeedStructure"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnRemoveFeedStructureObjectClick(object sender, EventArgs e)
        {
            var selectedNode = treeViewFeedStructure.SelectedNode;
            if (selectedNode == null || selectedNode == treeViewFeedStructure.Nodes[0]) return;
            RemoveObjectFromFeedStructure(selectedNode.Parent.Tag, selectedNode.Tag);
            FillFeedTab();
            treeViewFeedStructure.SelectedNode = treeViewFeedStructure.Nodes[0];
        }

        /// <summary>
        /// Removes an feed structure object from <see cref="Feed"/>.
        /// </summary>
        /// <param name="container">Not used.</param>
        /// <param name="toRemove">Not used.</param>
        private static void RemoveObjectFromFeedStructure(object container, object toRemove)
        {
            if (toRemove is Element) ((IElementContainer) container).Elements.Remove((Element) toRemove);
            else if (toRemove is Dependency) ((Element) container).Dependencies.Remove((Dependency) toRemove);
            else if (toRemove is Binding) ((IBindingContainer) container).Bindings.Remove((Binding) toRemove);
            else if (toRemove is RetrievalMethod)
            {
                var implementationContainer = (Implementation) container;
                implementationContainer.RetrievalMethods.Remove((RetrievalMethod) toRemove);
                if (implementationContainer.RetrievalMethods.Count == 0)
                {
                    implementationContainer.ManifestDigest = default(ManifestDigest);
                    if (implementationContainer.ID != null && implementationContainer.ID.StartsWith("sha1new="))
                        implementationContainer.ID = String.Empty;
                }
            }
        }

        private static void BuildElementsTreeNodes(IEnumerable<Element> elements, TreeNode parentNode)
        {
            #region Sanity checks

            if (elements == null) throw new ArgumentNullException("elements");

            #endregion

            foreach (var element in elements)
            {
                var group = element as Group;
                if (group != null)
                {
                    var groupNode = new TreeNode(group.ToString()) {Tag = group};
                    parentNode.Nodes.Add(groupNode);
                    BuildElementsTreeNodes(group.Elements, groupNode);
                    BuildDependencyTreeNodes(group.Dependencies, groupNode);
                    BuildBindingTreeNodes(group.Bindings, groupNode);
                }
                else
                {
                    var implementation = element as Implementation;
                    if (implementation != null)
                    {
                        var implementationNode = new TreeNode(implementation.ToString()) {Tag = implementation};
                        parentNode.Nodes.Add(implementationNode);
                        BuildManifestDigestTreeNode(implementation.ManifestDigest, implementationNode);
                        BuildRetrievalMethodsTreeNodes(implementation.RetrievalMethods, implementationNode);
                        BuildDependencyTreeNodes(implementation.Dependencies, implementationNode);
                        BuildBindingTreeNodes(implementation.Bindings, implementationNode);
                    }
                    else
                    {
                        var packageImplementation = element as PackageImplementation;
                        if (packageImplementation != null)
                        {
                            var packageImplementationNode = new TreeNode(packageImplementation.ToString())
                                                                {Tag = packageImplementation};
                            parentNode.Nodes.Add(packageImplementationNode);
                            BuildDependencyTreeNodes(packageImplementation.Dependencies, packageImplementationNode);
                            BuildBindingTreeNodes(packageImplementation.Bindings, packageImplementationNode);
                        }
                    }
                }
            }
        }

        private static void BuildBindingTreeNodes(IEnumerable<Binding> bindings, TreeNode parentNode)
        {
            #region Sanity checks

            if (bindings == null) throw new ArgumentNullException("bindings");

            #endregion

            foreach (var binding in bindings)
            {
                var bindingNode = new TreeNode(binding.ToString()) {Tag = binding};
                parentNode.Nodes.Add(bindingNode);
            }
        }

        private static void BuildManifestDigestTreeNode(ManifestDigest manifestDigest, TreeNode parentNode)
        {
            if (ControlHelpers.IsEmpty(manifestDigest)) return;
            var manifestDigestNode = new TreeNode("Manifest digest") {Tag = manifestDigest};
            parentNode.Nodes.Insert(0, manifestDigestNode);
        }

        private static void BuildDependencyTreeNodes(IEnumerable<Dependency> dependencies, TreeNode parentNode)
        {
            #region Sanity checks

            if (dependencies == null) throw new ArgumentNullException("dependencies");

            #endregion

            foreach (var dependency in dependencies)
            {
                string constraints = String.Empty;
                foreach (var constraint in dependency.Constraints) constraints += constraint.ToString();

                var dependencyNode = new TreeNode(string.Format("{0} {1}", dependency, constraints)) {Tag = dependency};
                parentNode.Nodes.Add(dependencyNode);
                BuildBindingTreeNodes(dependency.Bindings, dependencyNode);
            }
        }

        private static void BuildRetrievalMethodsTreeNodes(IEnumerable<RetrievalMethod> retrievalMethods,
                                                           TreeNode parentNode)
        {
            #region Sanity checks

            if (retrievalMethods == null) throw new ArgumentNullException("retrievalMethods");

            #endregion

            foreach (var retrievalMethod in retrievalMethods)
            {
                var retrievalMethodNode = new TreeNode(retrievalMethod.ToString()) {Tag = retrievalMethod};
                parentNode.Nodes.Add(retrievalMethodNode);
            }
        }

        #endregion

        #endregion

        #region Advanced Tab

        #region External Feeds Group

        /// <summary>
        /// Adds a clone of <see cref="FeedReference"/> from <see cref="feedReferenceControl"/> to <see cref="listBoxExternalFeeds"/> if no equal object is in the list.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnExtFeedsAddClick(object sender, EventArgs e)
        {
            var feedReference = feedReferenceControl.FeedReference.CloneFeedPreferences();
            if (string.IsNullOrEmpty(feedReference.Source)) return;
            foreach (FeedReference feedReferenceFromListBox in listBoxExternalFeeds.Items)
            {
                if (feedReference.Equals(feedReferenceFromListBox)) return;
            }
            listBoxExternalFeeds.Items.Add(feedReference);
        }

        /// <summary>
        /// Removes the selected <see cref="FeedReference"/> from <see cref="listBoxExternalFeeds"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnExtFeedsRemoveClick(object sender, EventArgs e)
        {
            var selectedItem = listBoxExternalFeeds.SelectedItem;
            if (selectedItem == null) return;
            listBoxExternalFeeds.Items.Remove(selectedItem);
        }

        /// <summary>
        /// Loads a clone of the selected <see cref="FeedReference"/> from <see cref="listBoxExternalFeeds"/> into <see cref="feedReferenceControl"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ListBoxExtFeedsSelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (FeedReference) listBoxExternalFeeds.SelectedItem;
            if (selectedItem == null) return;
            feedReferenceControl.FeedReference = selectedItem.CloneFeedPreferences();
        }

        /// <summary>
        /// Updates the selected <see cref="FeedReference"/> in <see cref="listBoxExternalFeeds"/> with the new values from <see cref="feedReferenceControl"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnExtFeedUpdateClick(object sender, EventArgs e)
        {
            var selectedFeedReferenceIndex = listBoxExternalFeeds.SelectedIndex;
            var feedReference = feedReferenceControl.FeedReference.CloneFeedPreferences();
            if (selectedFeedReferenceIndex < 0) return;
            if (String.IsNullOrEmpty(feedReference.Source)) return;
            listBoxExternalFeeds.Items[selectedFeedReferenceIndex] = feedReference;
        }

        #endregion

        #region FeedFor Group

        /// <summary>
        /// Adds a new <see cref="InterfaceReference"/> with the Uri from <see cref="hintTextBoxFeedFor"/> to <see cref="listBoxFeedFor"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnFeedForAddClick(object sender, EventArgs e)
        {
            Uri uri;
            if (!Feed.IsValidUrl(hintTextBoxFeedFor.Text, out uri)) return;
            var interfaceReference = new InterfaceReference {Target = uri};
            listBoxFeedFor.Items.Add(interfaceReference);
        }

        /// <summary>
        /// Removes the selected entry from <see cref="listBoxFeedFor"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnFeedForRemoveClick(object sender, EventArgs e)
        {
            var feedFor = listBoxFeedFor.SelectedItem;
            if (feedFor == null) return;
            listBoxFeedFor.Items.Remove(feedFor);
        }

        /// <summary>
        /// Clears <see cref="listBoxFeedFor"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnFeedForClearClick(object sender, EventArgs e)
        {
            listBoxFeedFor.Items.Clear();
        }

        #endregion

        #endregion

        #endregion
    }
}