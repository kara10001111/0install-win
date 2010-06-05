﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using Common.Helpers;

namespace ZeroInstall.DownloadBroker
{
    public abstract class HierarchyEntry
    {
        public delegate void EntryHandler(HierarchyEntry entry);

        private EntryContainer _parent;

        public string Name { get; private set; }
        public DateTime LastWriteTime { get; private set; }
        public virtual string RelativePath
        {
            get
            {
                if (_parent.IsRoot()) return Name;
                else return _parent.RelativePath + "/" + Name;
            }
        }

        protected HierarchyEntry(string name, EntryContainer parent)
        {
            #region Preconditions
            if (name != null && name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) throw new ArgumentException("Invalid file name.");
            #endregion

            _parent = parent;
            Name = name;
            LastWriteTime = DateTime.Today.ToUniversalTime();
        }

        public abstract void WriteIntoFolder(string folderPath);
        public virtual void RecurseInto(EntryHandler action)
        {
            action.Invoke(this);
        }

        public bool IsRoot()
        {
            return _parent == null;
        }
    }

    public class FileEntry : HierarchyEntry
    {
        private MemoryStream _content;

        public byte[] Content
        {
            get { return _content.ToArray(); }
        }

        public FileEntry(string name, byte[] content, EntryContainer parent) : base(name, parent)
        {
            #region Preconditions
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (content == null) throw new ArgumentNullException("content");
            if (parent == null) throw new ArgumentNullException("parent");
            #endregion

            _content = new MemoryStream(content.Length);
            _content.Write(content, 0, content.Length);
            _content.Seek(0, SeekOrigin.Begin);
        }

        public override void WriteIntoFolder(string folderPath)
        {
            #region Preconditions
            if (folderPath == null) throw new ArgumentNullException("folderPath");
            if (folderPath.IndexOfAny(Path.GetInvalidPathChars()) != -1) throw new ArgumentException("Invalid path supplied.");
            if (!Directory.Exists(folderPath)) throw new InvalidOperationException("Folder " + Path.GetFullPath(folderPath) + " does not exist.");
            #endregion

            string combinedPath = Path.Combine(folderPath, Name);
            CheckWritePath(combinedPath);

            WriteFileTo(combinedPath);
        }

        private static void CheckWritePath(string combinedPath)
        {
            if (File.Exists(combinedPath)) throw new InvalidOperationException("Can't overwrite existing file.");
            if (Directory.Exists(combinedPath)) throw new InvalidOperationException("Can't overwrite existing folder.");
        }

        private void WriteFileTo(string combinedPath)
        {
            File.WriteAllBytes(combinedPath, _content.ToArray());
            File.SetLastWriteTimeUtc(combinedPath, LastWriteTime);
        }
    }

    public abstract class EntryContainer : HierarchyEntry
    {
        protected List<HierarchyEntry> entries = new List<HierarchyEntry>();

        protected EntryContainer(string name, EntryContainer parent) : base(name, parent)
        { }

        public void Add(HierarchyEntry newEntry)
        {
            entries.Add(newEntry);
            entries.Sort((HierarchyEntry left, HierarchyEntry right) => StringComparer.InvariantCulture.Compare(left.Name, right.Name));
        }

        public override void WriteIntoFolder(string folderPath)
        {
            #region Preconditions
            if (folderPath == null) throw new ArgumentNullException("folderPath");
            if (folderPath.IndexOfAny(Path.GetInvalidPathChars()) != -1) throw new ArgumentException("Invalid path supplied.");
            if (!Directory.Exists(folderPath)) throw new InvalidOperationException("Folder " + Path.GetFullPath(folderPath) + " does not exist.");
            #endregion

            string combinedPath = CombineNameWithPath(folderPath);
            CheckAndPrepareWritePath(combinedPath);
            WriteContentsIntoFolder(combinedPath);
        }

        protected abstract string CombineNameWithPath(string path);
        protected abstract void CheckAndPrepareWritePath(string thisFoldersPath);

        protected void WriteContentsIntoFolder(string combinedPath)
        {
            foreach (var currentEntry in entries)
            {
                currentEntry.WriteIntoFolder(combinedPath);
            }
        }
    }

    public class FolderEntry : EntryContainer
    {
        public FolderEntry(string name, EntryContainer parent) : base(name, parent)
        {
            #region Preconditions
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (parent == null) throw new ArgumentNullException("parent");
            #endregion
        }

        protected override string CombineNameWithPath(string path)
        {
            return Path.Combine(path, Name);
        }

        protected override void CheckAndPrepareWritePath(string thisFoldersPath)
        {
            if (Directory.Exists(thisFoldersPath)) throw new InvalidOperationException("Can't overwrite existing folder.");
            if (File.Exists(thisFoldersPath)) throw new InvalidOperationException("Can't overwrite existing file.");
            Directory.CreateDirectory(thisFoldersPath);
        }

        public override void RecurseInto(EntryHandler action)
        {
            base.RecurseInto(action);
            foreach (var entry in entries)
            {
                entry.RecurseInto(action);
            }
        }
    }

    public class RootEntry : EntryContainer
    {
        public override string RelativePath
        {
            get
            {
                return "";
            }
        }

        public RootEntry() : base(null, null)
        { }

        protected override string CombineNameWithPath(string path)
        {
            return path;
        }

        protected override void CheckAndPrepareWritePath(string path)
        {
            if (Directory.GetFileSystemEntries(path).Length > 0) throw new InvalidOperationException("Can't write into non-empty folder.");
        }

        public override void RecurseInto(EntryHandler action)
        {
            foreach (var entry in entries)
            {
                entry.RecurseInto(action);
            }
        }
    }

    public class PackageBuilder
    {
        EntryContainer packageHierarchy;

        public PackageBuilder()
        {
            packageHierarchy = new RootEntry();
        }

        public PackageBuilder(FolderEntry folder)
        {
            packageHierarchy = folder;
        }

        public PackageBuilder AddFolder(string name)
        {
            var item = new FolderEntry(name, packageHierarchy);
            packageHierarchy.Add(item);
            return new PackageBuilder(item);
        }

        public PackageBuilder AddFile(string name, byte[] content)
        {
            packageHierarchy.Add(new FileEntry(name, content, packageHierarchy));
            return this;
        }

        public void WritePackageInto(string packageDirectory)
        {
            packageHierarchy.WriteIntoFolder(packageDirectory);
        }

        public void GeneratePackageArchive(Stream output)
        {
            using (var zip = new ZipOutputStream(output))
            {
                zip.SetLevel(9);

                HierarchyEntry.EntryHandler entryToZip = delegate (HierarchyEntry entry)
                {
                    if (entry.GetType() == typeof (FolderEntry))
                        return;
                    var zipEntry = new ZipEntry(entry.RelativePath);
                    zipEntry.DateTime = entry.LastWriteTime;
                    zip.PutNextEntry(zipEntry);
                    if (entry.GetType() == typeof (FileEntry))
                    {
                        var fileEntry = (FileEntry)entry;
                        var writer = new BinaryWriter(zip);
                        writer.Write(fileEntry.Content);
                    }
                };
                packageHierarchy.RecurseInto(entryToZip);
            }
        }

        public ManifestDigest ComputePackageDigest()
        {
            MemoryStream dotFile = new MemoryStream();
            WriteHierarchyManifestToStream(dotFile);
            dotFile.Seek(0, SeekOrigin.Begin);
            return new ManifestDigest(ManifestFormat.Sha256.Prefix + FileHelper.ComputeHash(dotFile, ManifestFormat.Sha256.HashAlgorithm));
        }

        private void WriteHierarchyManifestToStream(MemoryStream dotFile)
        {
            var writer = new StreamWriter(dotFile) { NewLine = "\n" };
            HierarchyEntry.EntryHandler entryToDotFile = delegate(HierarchyEntry entry)
            {
                ManifestNode node = null;
                if (entry.Name == "")
                    return;
                if (entry.GetType() == typeof(FolderEntry))
                {
                    node = new ManifestDirectory(FileHelper.UnixTime(entry.LastWriteTime), "/" + entry.RelativePath);
                }
                else if (entry.GetType() == typeof(FileEntry))
                {
                    var fileEntry = (FileEntry)entry;
                    string hash;
                    long size;
                    using (var entryData = new MemoryStream(fileEntry.Content))
                    {
                        size = entryData.Length;
                        hash = FileHelper.ComputeHash(entryData, ManifestFormat.Sha256.HashAlgorithm);
                    }
                    node = new ManifestFile(hash, FileHelper.UnixTime(entry.LastWriteTime), size, entry.Name);
                }
                writer.WriteLine(ManifestFormat.Sha256.GenerateEntryForNode(node));
            };
            packageHierarchy.RecurseInto(entryToDotFile);
            writer.Flush();
        }
    }
}
