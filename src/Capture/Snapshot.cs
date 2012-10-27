﻿/*
 * Copyright 2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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
using System.Linq;
using System.Security;
using Common;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.Capture.Properties;
using ZeroInstall.DesktopIntegration.Windows;

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Represents the systems state at a point in time. This is used to determine changes.
    /// </summary>
    [Serializable]
    public class Snapshot
    {
        #region Variables
        /// <summary>A list of associations of services with clients (e.g. web browsers, mail readers, ...).</summary>
        public ComparableTuple<string>[] ServiceAssocs;

        /// <summary>A list of applications registered as AutoPlay handlers.</summary>
        public string[] AutoPlayHandlersUser, AutoPlayHandlersMachine;

        /// <summary>A list of associations of an AutoPlay events with an AutoPlay handlers.</summary>
        public ComparableTuple<string>[] AutoPlayAssocsUser, AutoPlayAssocsMachine;

        /// <summary>A list of associations of file extensions with programatic identifiers.</summary>
        public ComparableTuple<string>[] FileAssocs;

        /// <summary>A list of protocol associations for well-known protocols (e.g. HTTP, FTP, ...).</summary>
        public ComparableTuple<string>[] ProtocolAssocs;

        /// <summary>A list of programatic indentifiers.</summary>
        public string[] ProgIDs;

        /// <summary>A list of COM class IDs.</summary>
        public string[] ClassIDs;

        /// <summary>A list of applications registered as candidates for default programs.</summary>
        public string[] RegisteredApplications;

        /// <summary>A list of simple context menu entries for all file types.</summary>
        public string[] FilesContextMenuSimple;

        /// <summary>A list of extended (COM-based) context menu entries for all file types.</summary>
        public string[] FilesContextMenuExtended;

        /// <summary>A list of (COM-based) property sheets for all file types.</summary>
        public string[] FilesPropertySheets;

        /// <summary>A list of simple context menu entries for all filesystem objects (files and directories).</summary>
        public string[] AllContextMenuSimple;

        /// <summary>A list of extended (COM-based) context menu entries for all filesystem objects (files and directories).</summary>
        public string[] AllContextMenuExtended;

        /// <summary>A list of (COM-based) property sheets for all file-system entries.</summary>
        public string[] AllPropertySheets;

        /// <summary>A list of applications registered in the Windows Games Explorer.</summary>
        public string[] Games;

        /// <summary>A list of program installation directories.</summary>
        public string[] ProgramsDirs;
        #endregion

        //--------------------//

        #region Take snapshot
        /// <summary>
        /// Takes a snapshot of the current system state.
        /// </summary>
        /// <returns>The newly created snapshot.</returns>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry or file system was not permitted.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called while running on a platform for which capturing is not supported.</exception>
        public static Snapshot Take()
        {
            if (!WindowsUtils.IsWindows) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            var snapshot = new Snapshot();

            try
            {
                TakeRegistry(snapshot);
            }
                #region Error handling
            catch (SecurityException ex)
            {
                // Wrap exception since only certain exception types are allowed in tasks
                throw new UnauthorizedAccessException(ex.Message, ex);
            }
            #endregion

            TakeFileSystem(snapshot);

            return snapshot;
        }
        #endregion

        #region Registry
        /// <summary>
        /// Stores information about the current state of the registry in a snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to store the data in.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void TakeRegistry(Snapshot snapshot)
        {
            snapshot.ServiceAssocs = GetServiceAssocs();
            snapshot.AutoPlayHandlersUser = RegUtils.GetSubKeyNames(Registry.CurrentUser, AutoPlay.RegKeyHandlers);
            snapshot.AutoPlayHandlersMachine = RegUtils.GetSubKeyNames(Registry.LocalMachine, AutoPlay.RegKeyHandlers);
            snapshot.AutoPlayAssocsUser = GetAutoPlayAssocs(Registry.CurrentUser);
            snapshot.AutoPlayAssocsMachine = GetAutoPlayAssocs(Registry.LocalMachine);
            GetFileAssocData(out snapshot.FileAssocs, out snapshot.ProgIDs);
            snapshot.ProtocolAssocs = GetProtocolAssoc();
            snapshot.ClassIDs = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ComServer.RegKeyClassesIDs);
            snapshot.RegisteredApplications = RegUtils.GetValueNames(Registry.LocalMachine, AppRegistration.RegKeyMachineRegisteredApplications);

            snapshot.FilesContextMenuSimple = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesFilesPrefix + "\\" + ContextMenu.RegKeyContextMenuSimplePostfix);
            snapshot.FilesContextMenuExtended = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesFilesPrefix + "\\" + ContextMenu.RegKeyContextMenuExtendedPostfix);
            snapshot.FilesPropertySheets = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesFilesPrefix + "\\" + ContextMenu.RegKeyPropertySheetsPostfix);

            snapshot.AllContextMenuSimple = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesAllPrefix + "\\" + ContextMenu.RegKeyContextMenuSimplePostfix);
            snapshot.AllContextMenuExtended = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesAllPrefix + "\\" + ContextMenu.RegKeyContextMenuExtendedPostfix);
            snapshot.AllPropertySheets = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesAllPrefix + "\\" + ContextMenu.RegKeyPropertySheetsPostfix);

            snapshot.Games = RegUtils.GetSubKeyNames(Registry.LocalMachine, GamesExplorer.RegKeyMachineGames);
        }

        /// <summary>
        /// Retrieves a list of service associations from the registry.
        /// </summary>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static ComparableTuple<string>[] GetServiceAssocs()
        {
            using (var clientsKey = Registry.LocalMachine.OpenSubKey(DefaultProgram.RegKeyMachineClients))
            {
                if (clientsKey == null) return new ComparableTuple<string>[0];

                return (
                    from serviceName in clientsKey.GetSubKeyNames()
                    from clientName in RegUtils.GetSubKeyNames(clientsKey, serviceName)
                    select new ComparableTuple<string>(serviceName, clientName)).ToArray();
            }
        }

        /// <summary>
        /// Retrieves a list of file assocations and programatic indentifiers the registry.
        /// </summary>
        private static void GetFileAssocData(out ComparableTuple<string>[] fileAssocs, out string[] progIDs)
        {
            var fileAssocsList = new List<ComparableTuple<string>>();
            var progIDsList = new List<string>();

            foreach (string keyName in Registry.ClassesRoot.GetSubKeyNames())
            {
                if (keyName.StartsWith("."))
                {
                    using (var assocKey = Registry.ClassesRoot.OpenSubKey(keyName))
                    {
                        // Get the main ProgID
                        fileAssocsList.Add(new ComparableTuple<string>(keyName, assocKey.GetValue("", "").ToString()));

                        // Get additional ProgIDs
                        // ReSharper disable AccessToForEachVariableInClosure
                        fileAssocsList.AddRange(RegUtils.GetValueNames(assocKey, FileType.RegSubKeyOpenWith).Select(progID => new ComparableTuple<string>(keyName, progID)));
                        // ReSharper restore AccessToForEachVariableInClosure
                    }
                }
                else progIDsList.Add(keyName);
            }

            fileAssocs = fileAssocsList.ToArray();
            progIDs = progIDsList.ToArray();
        }

        /// <summary>
        /// Retrieves a list of protocol associations for well-known protocols (e.g. HTTP, FTP, ...).
        /// </summary>
        private static ComparableTuple<string>[] GetProtocolAssoc()
        {
            return (
                from protocol in new[] {"ftp", "gopher", "http", "https"}
                let command = Registry.GetValue(@"HKEY_CLASSES_ROOT\" + protocol + @"\shell\open\command", "", "") as string
                where !string.IsNullOrEmpty(command)
                select new ComparableTuple<string>(protocol, command)).ToArray();
        }

        /// <summary>
        /// Retrieves a list of AutoPlay associations from the registry.
        /// </summary>
        /// <param name="hive">The registry hive to search in (usually HKCU or HKLM).</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static ComparableTuple<string>[] GetAutoPlayAssocs(RegistryKey hive)
        {
            using (var eventsKey = hive.OpenSubKey(AutoPlay.RegKeyAssocs))
            {
                if (eventsKey == null) return new ComparableTuple<string>[0];

                return (
                    from eventName in eventsKey.GetSubKeyNames()
                    from handlerName in RegUtils.GetValueNames(eventsKey, eventName)
                    select new ComparableTuple<string>(eventName, handlerName)).ToArray();
            }
        }
        #endregion

        #region File system
        /// <summary>
        /// Stores information about the current state of the file system in a snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to store the data in.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file system was not permitted.</exception>
        private static void TakeFileSystem(Snapshot snapshot)
        {
            // Locate installation directories
            string programFiles32Bit = WindowsUtils.Is64BitProcess
                ? Environment.GetEnvironmentVariable("ProgramFiles(x86)")
                : Environment.GetEnvironmentVariable("ProgramFiles");
            string programFiles64Bit = WindowsUtils.Is64BitProcess
                ? Environment.GetEnvironmentVariable("ProgramFiles")
                : null;

            // Build a list of all installation directorie
            var programDirs = new List<string>();
            if (string.IsNullOrEmpty(programFiles32Bit)) Log.Warn(Resources.MissingProgramFiles32Bit);
            else programDirs.AddRange(Directory.GetDirectories(programFiles32Bit));
            if (!string.IsNullOrEmpty(programFiles64Bit))
                programDirs.AddRange(Directory.GetDirectories(programFiles64Bit));
            snapshot.ProgramsDirs = programDirs.ToArray();
        }
        #endregion

        #region Diff
        /// <summary>
        /// Determines which elements have been added to the system between two snapshots.
        /// </summary>
        /// <param name="oldSnapshot">The first snapshot taken.</param>
        /// <param name="newSnapshot">The second snapshot taken.</param>
        /// <returns>A snapshot containing all elements that are present in <paramref name="newSnapshot"/> but not in <paramref name="oldSnapshot"/>.</returns>
        /// <remarks>Assumes that all internal arrays are sorted alphabetically.</remarks>
        public static Snapshot Diff(Snapshot oldSnapshot, Snapshot newSnapshot)
        {
            #region Sanity checks
            if (oldSnapshot == null) throw new ArgumentNullException("oldSnapshot");
            if (newSnapshot == null) throw new ArgumentNullException("newSnapshot");
            #endregion

            return new Snapshot
            {
                ServiceAssocs = newSnapshot.ServiceAssocs.GetAddedElements(oldSnapshot.ServiceAssocs),
                AutoPlayHandlersUser = newSnapshot.AutoPlayHandlersUser.GetAddedElements(oldSnapshot.AutoPlayHandlersUser),
                AutoPlayHandlersMachine = newSnapshot.AutoPlayHandlersMachine.GetAddedElements(oldSnapshot.AutoPlayHandlersMachine),
                AutoPlayAssocsUser = newSnapshot.AutoPlayAssocsUser.GetAddedElements(oldSnapshot.AutoPlayAssocsUser),
                AutoPlayAssocsMachine = newSnapshot.AutoPlayAssocsMachine.GetAddedElements(oldSnapshot.AutoPlayAssocsMachine),
                FileAssocs = newSnapshot.FileAssocs.GetAddedElements(oldSnapshot.FileAssocs),
                ProtocolAssocs = newSnapshot.ProtocolAssocs.GetAddedElements(oldSnapshot.ProtocolAssocs),
                ProgIDs = newSnapshot.ProgIDs.GetAddedElements(oldSnapshot.ProgIDs, StringComparer.InvariantCultureIgnoreCase),
                ClassIDs = newSnapshot.ClassIDs.GetAddedElements(oldSnapshot.ClassIDs, StringComparer.InvariantCultureIgnoreCase),
                RegisteredApplications = newSnapshot.RegisteredApplications.GetAddedElements(oldSnapshot.RegisteredApplications),
                FilesContextMenuSimple = newSnapshot.FilesContextMenuSimple.GetAddedElements(oldSnapshot.FilesContextMenuSimple),
                FilesContextMenuExtended = newSnapshot.FilesContextMenuExtended.GetAddedElements(oldSnapshot.FilesContextMenuExtended),
                FilesPropertySheets = newSnapshot.FilesPropertySheets.GetAddedElements(oldSnapshot.FilesPropertySheets),
                AllContextMenuSimple = newSnapshot.AllContextMenuSimple.GetAddedElements(oldSnapshot.AllContextMenuSimple),
                AllContextMenuExtended = newSnapshot.AllContextMenuExtended.GetAddedElements(oldSnapshot.AllContextMenuExtended),
                AllPropertySheets = newSnapshot.AllPropertySheets.GetAddedElements(oldSnapshot.AllPropertySheets),
                Games = newSnapshot.Games.GetAddedElements(oldSnapshot.Games),
                ProgramsDirs = newSnapshot.ProgramsDirs.GetAddedElements(oldSnapshot.ProgramsDirs, StringComparer.InvariantCultureIgnoreCase)
            };
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a <see cref="Snapshot"/> from a binary file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Snapshot"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the binary data.</exception>
        public static Snapshot Load(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return BinaryStorage.Load<Snapshot>(path);
        }

        /// <summary>
        /// Loads a <see cref="Snapshot"/> from a stream containing a binary file.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="Snapshot"/>.</returns>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the binary data.</exception>
        public static Snapshot Load(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return BinaryStorage.Load<Snapshot>(stream);
        }

        /// <summary>
        /// Saves this <see cref="Snapshot"/> to a binary file.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            BinaryStorage.Save(path, this);
        }

        /// <summary>
        /// Saves this <see cref="Snapshot"/> to a stream as a binary file.
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            BinaryStorage.Save(stream, this);
        }
        #endregion
    }
}
