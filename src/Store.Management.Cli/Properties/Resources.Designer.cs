﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZeroInstall.Store.Management.Cli.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ZeroInstall.Store.Management.Cli.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to One or more problems were found in the store..
        /// </summary>
        internal static string AuditErrors {
            get {
                return ResourceManager.GetString("AuditErrors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No problems were found in the store..
        /// </summary>
        internal static string AuditPass {
            get {
                return ResourceManager.GetString("AuditPass", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To add a directory to the store (makes a copy):
        ///0store add sha256=XXX directory
        ///
        ///To add an archive to the store:
        ///0store add sha256=XXX archive.tgz
        ///
        ///To add a subdirectory of an archive to the store:
        ///0store add sha256=XXX archive.tgz subdir
        ///
        ///The actual digest is calculated and compared to the given one. If they don&apos;t match, the operation is rejected..
        /// </summary>
        internal static string DetailsAdd {
            get {
                return ResourceManager.GetString("DetailsAdd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Verifies  every  implementation in each of the given cache directories, or in all of the default cache directories if no arguments are given. This will detect any packages which have been tampered with since they were unpacked.
        ///See the &apos;verify&apos; command for details of the verification performed on each package..
        /// </summary>
        internal static string DetailsAudit {
            get {
                return ResourceManager.GetString("DetailsAudit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To  copy an implementation (a directory with a name in the form &quot;algorithm=value&quot;), use the &apos;copy&apos; command. This is similar to performing a normal recursive directory copy followed by a &apos;0store verify&apos; to check that the name matches the contents.
        ///
        ///Examples:
        ///Windows: 0store copy %localappdata%\0install.net\implementations\sha256=XXX %localappdata%\0install.net\implementations\
        ///Linux: 0store copy ~someuser/.cache/0install.net/implementations/sha256=XXX /var/cache/0install.net/implementations/.
        /// </summary>
        internal static string DetailsCopy {
            get {
                return ResourceManager.GetString("DetailsCopy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To find the path of a stored item:
        ///0store find sha256=XXX.
        /// </summary>
        internal static string DetailsFind {
            get {
                return ResourceManager.GetString("DetailsFind", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To see the list of all implementations in all currently configured stores:
        ///0store list.
        /// </summary>
        internal static string DetailsList {
            get {
                return ResourceManager.GetString("DetailsList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To verify or remove feeds or implementations:
        ///0store manage
        ///
        ///Displays a GUI for managing implementations in the store. Associations with cached feeds are displayed..
        /// </summary>
        internal static string DetailsManage {
            get {
                return ResourceManager.GetString("DetailsManage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To generate the manifest for a directory structure and print it to the console:
        ///0store manifest DIRECTORY [ALGORITHM]
        ///
        ///The manifest lists every file and directory in the tree, along with the digest of each file, thus uniquely identifying that particular set of files. After the manifest, the last line gives the digest of the manifest itself.
        ///
        ///This value is needed when creating feed files. However, the 0publish command will automatically calculate the required digest for you and add it to a feed file.
        ///
        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string DetailsManifest {
            get {
                return ResourceManager.GetString("DetailsManifest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To hard-link duplicate files together to save space:
        ///0store optimise [CACHE]
        ///
        ///This reads in all the manifest files in the cache directory and looks for duplicates (files with the same permissions, modification time and digest). When it finds a pair, it deletes one file and replaces it (atomically) with a hard-link to the other.
        ///
        ///Implementations using the old &apos;sha1&apos; algorithm are not optimised..
        /// </summary>
        internal static string DetailsOptimise {
            get {
                return ResourceManager.GetString("DetailsOptimise", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To remove an item from the store:
        ///0store remove sha256=XXX.
        /// </summary>
        internal static string DetailsRemove {
            get {
                return ResourceManager.GetString("DetailsRemove", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To check that an item is stored correctly:
        ///0store verify /path/to/sha256=XXX
        ///
        ///This calculates the manifest of the directory and checks that its digest matches the directory&apos;s name. It also checks that it matches the digest of the .manifest file inside the directory..
        /// </summary>
        internal static string DetailsVerify {
            get {
                return ResourceManager.GetString("DetailsVerify", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The directory &apos;{0}&apos; was not found..
        /// </summary>
        internal static string DirectoryNotFound {
            get {
                return ResourceManager.GetString("DirectoryNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This program comes with ABSOLUTELY NO WARRANTY, to the extent permitted by law.
        ///You may redistribute copies of this program under the terms of the GNU Lesser General Public License..
        /// </summary>
        internal static string LicenseInfo {
            get {
                return ResourceManager.GetString("LicenseInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This store does not support auditing..
        /// </summary>
        internal static string NoAuditSupport {
            get {
                return ResourceManager.GetString("NoAuditSupport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No such file or directory: {0}.
        /// </summary>
        internal static string NoSuchFileOrDirectory {
            get {
                return ResourceManager.GetString("NoSuchFileOrDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show the built-in help text..
        /// </summary>
        internal static string OptionHelp {
            get {
                return ResourceManager.GetString("OptionHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Display more comprehensive documentation for the operation modes..
        /// </summary>
        internal static string OptionMan {
            get {
                return ResourceManager.GetString("OptionMan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Options:.
        /// </summary>
        internal static string Options {
            get {
                return ResourceManager.GetString("Options", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Display version information..
        /// </summary>
        internal static string OptionVersion {
            get {
                return ResourceManager.GetString("OptionVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Portable mode.
        /// </summary>
        internal static string PortableMode {
            get {
                return ResourceManager.GetString("PortableMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The store entry is OK..
        /// </summary>
        internal static string StoreEntryOK {
            get {
                return ResourceManager.GetString("StoreEntryOK", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully removed {0}..
        /// </summary>
        internal static string SuccessfullyRemoved {
            get {
                return ResourceManager.GetString("SuccessfullyRemoved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown operation mode.
        ///Try 0store --help.
        /// </summary>
        internal static string UnknownMode {
            get {
                return ResourceManager.GetString("UnknownMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage:.
        /// </summary>
        internal static string Usage {
            get {
                return ResourceManager.GetString("Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0store add DIGEST (DIRECTORY | (ARCHIVE [EXTRACT])).
        /// </summary>
        internal static string UsageAdd {
            get {
                return ResourceManager.GetString("UsageAdd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0store audit [CACHE+].
        /// </summary>
        internal static string UsageAudit {
            get {
                return ResourceManager.GetString("UsageAudit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0store copy DIRECTORY [CACHE].
        /// </summary>
        internal static string UsageCopy {
            get {
                return ResourceManager.GetString("UsageCopy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0store find DIGEST.
        /// </summary>
        internal static string UsageFind {
            get {
                return ResourceManager.GetString("UsageFind", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0store list.
        /// </summary>
        internal static string UsageList {
            get {
                return ResourceManager.GetString("UsageList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0store manifest DIRECTORY [ALGORITHM].
        /// </summary>
        internal static string UsageManifest {
            get {
                return ResourceManager.GetString("UsageManifest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0store optimise [CACHE+].
        /// </summary>
        internal static string UsageOptimize {
            get {
                return ResourceManager.GetString("UsageOptimize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0store remove DIGEST+.
        /// </summary>
        internal static string UsageRemove {
            get {
                return ResourceManager.GetString("UsageRemove", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0store verify (DIGEST|DIRECTORY)+.
        /// </summary>
        internal static string UsageVerify {
            get {
                return ResourceManager.GetString("UsageVerify", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong number of arguments.
        ///Usage: {0}.
        /// </summary>
        internal static string WrongNoArguments {
            get {
                return ResourceManager.GetString("WrongNoArguments", resourceCulture);
            }
        }
    }
}
