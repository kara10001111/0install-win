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
using System.Collections.Generic;
using System.Runtime.Remoting;
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Uses a background service to add new entries to a store that requires elevated privileges to write.
    /// </summary>
    /// <remarks>
    ///   <para>The represented store data is mutable but the class itself is immutable.</para>
    ///   <para>All <see cref="RemotingException"/>s are wrapped as <see cref="UnauthorizedAccessException"/>s.</para>
    /// </remarks>
    public sealed class ServiceStore : MarshalByRefObject, IStore
    {
        #region Constants
        /// <summary>
        /// The default port name to use to contact the service process.
        /// </summary>
        public const string IpcPortName = "ZeroInstallStoreService";

        /// <summary>
        /// The Uri fragment to use to request an <see cref="IStore"/> object from the service.
        /// </summary>
        public const string IpcObjectUri = "Store";
        #endregion

        #region Variables
        /// <summary>The store to use for read-access.</summary>
        private readonly IStore _serviceProxy;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the IPC subsystem using the default IPC port name. No actual connection is established yet.
        /// </summary>
        public ServiceStore() : this(IpcPortName)
        {}

        /// <summary>
        /// Initializes the IPC subsystem using a custom IPC port name. No actual connection is established yet.
        /// </summary>
        /// <param name="ipcPortName">The port name to use to contact the service process.</param>
        public ServiceStore(string ipcPortName)
        {
            _serviceProxy = (IStore)Activator.GetObject(typeof(IStore), "ipc://" + ipcPortName + "/" + IpcObjectUri);
        }
        #endregion

        //--------------------//

        #region List all
        /// <inheritdoc />
        public IEnumerable<ManifestDigest> ListAll()
        {
            try
            {
                return _serviceProxy.ListAll();
            }
                #region Error handling
            catch (RemotingException ex)
            {
                throw new UnauthorizedAccessException(Resources.StoreServiceCommunicationProblem, ex);
            }
            #endregion
        }

        /// <inheritdoc />
        public IEnumerable<string> ListAllTemp()
        {
            try
            {
                return _serviceProxy.ListAllTemp();
            }
                #region Error handling
            catch (RemotingException ex)
            {
                throw new UnauthorizedAccessException(Resources.StoreServiceCommunicationProblem, ex);
            }
            #endregion
        }
        #endregion

        #region Contains
        /// <inheritdoc />
        public bool Contains(ManifestDigest manifestDigest)
        {
            try
            {
                return _serviceProxy.Contains(manifestDigest);
            }
                #region Error handling
            catch (RemotingException)
            {
                return false;
            }
            #endregion
        }

        /// <inheritdoc />
        public bool Contains(string directory)
        {
            try
            {
                return _serviceProxy.Contains(directory);
            }
                #region Error handling
            catch (RemotingException)
            {
                return false;
            }
            #endregion
        }
        #endregion

        #region Get
        /// <inheritdoc />
        public string GetPath(ManifestDigest manifestDigest)
        {
            try
            {
                return _serviceProxy.GetPath(manifestDigest);
            }
                #region Error handling
            catch (RemotingException ex)
            {
                throw new UnauthorizedAccessException(Resources.StoreServiceCommunicationProblem, ex);
            }
            #endregion
        }
        #endregion

        #region Add directory
        /// <inheritdoc />
        public void AddDirectory(string path, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            try
            {
                _serviceProxy.AddDirectory(path, manifestDigest, handler);
            }
                #region Error handling
            catch (RemotingException ex)
            {
                throw new UnauthorizedAccessException(Resources.StoreServiceCommunicationProblem, ex);
            }
            #endregion
        }
        #endregion

        #region Add archive
        /// <inheritdoc />
        public void AddArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            try
            {
                _serviceProxy.AddArchives(archiveInfos, manifestDigest, handler);
            }
                #region Error handling
            catch (RemotingException ex)
            {
                throw new UnauthorizedAccessException(Resources.StoreServiceCommunicationProblem, ex);
            }
            #endregion
        }
        #endregion

        #region Remove
        /// <inheritdoc />
        public void Remove(ManifestDigest manifestDigest)
        {
            try
            {
                _serviceProxy.Remove(manifestDigest);
            }
                #region Error handling
            catch (RemotingException ex)
            {
                throw new UnauthorizedAccessException(Resources.StoreServiceCommunicationProblem, ex);
            }
            #endregion
        }
        #endregion

        #region Optimise
        /// <inheritdoc />
        public void Optimise(ITaskHandler handler)
        {
            // Optimizing should use direct access via a DirectoryStore instead
        }
        #endregion

        #region Verify
        /// <inheritdoc />
        public void Verify(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            // Verification should use direct access via a DirectoryStore instead
        }
        #endregion

        #region Audit
        /// <inheritdoc />
        public IEnumerable<DigestMismatchException> Audit(ITaskHandler handler)
        {
            // Auditing should use direct access via a DirectoryStore instead
            return null;
        }
        #endregion

        #region Caches
        /// <inheritdoc />
        public void ClearCaches()
        {
            // No internal caching
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the Store in the form "ServiceStorey: ServiceProxy". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "ServiceStorey: " + _serviceProxy;
        }
        #endregion
    }
}
