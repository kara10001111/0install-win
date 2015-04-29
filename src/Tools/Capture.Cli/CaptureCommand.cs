﻿/*
 * Copyright 2011 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Publish;
using ZeroInstall.Publish.Capture;

namespace ZeroInstall.Capture.Cli
{
    /// <summary>
    /// Structure for storing user-selected arguments for a capture operation.
    /// </summary>
    internal class CaptureCommand
    {
        #region Parse
        private readonly ITaskHandler _handler;

        /// <summary>Ignore warnings and perform the operation anyway.</summary>
        private bool _force;

        /// <summary>The directory the application to be captured is installed in.</summary>
        private string _installationDirectory;

        /// <summary>The relative path to the main EXE of the application to be captured.</summary>
        private string _mainExe;

        /// <summary>Indicates whether to collect installation files in addition to registry data.</summary>
        private bool _collectFiles;

        private readonly List<string> _additionalArgs;

        public CaptureCommand(IEnumerable<string> args, ITaskHandler handler)
        {
            _handler = handler;

            var options = new OptionSet
            {
                {
                    "V|version", _ =>
                    {
                        var assembly = Assembly.GetEntryAssembly().GetName();
                        Console.WriteLine(@"Zero Install Capture CLI v{0}", assembly.Version);
                        throw new OperationCanceledException();
                    }
                },
                {"f|force", _ => _force = true},
                {
                    "installation-dir=", value =>
                    {
                        try
                        {
                            _installationDirectory = Path.GetFullPath(value);
                        }
                            #region Error handling
                        catch (ArgumentException ex)
                        {
                            // Wrap exception since only certain exception types are allowed
                            throw new OptionException(ex.Message, "installation-dir");
                        }
                        catch (NotSupportedException ex)
                        {
                            // Wrap exception since only certain exception types are allowed
                            throw new OptionException(ex.Message, "installation-dir");
                        }
                        #endregion
                    }
                },
                {"main-exe=", value => _mainExe = value},
                {"collect-files", _ => _collectFiles = true},
                {
                    "h|help|?", _ =>
                    {
                        PrintHelp();
                        throw new OperationCanceledException();
                    }
                }
            };
            _additionalArgs = options.Parse(args);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private static ExitCode PrintHelp()
        {
            Console.WriteLine("0capture start myapp.snapshot [--force]");
            Console.WriteLine("0capture finish myapp.snapshot myapp.xml [--force]");
            Console.WriteLine("\t[--installation-dir=C:\\myapp] [--main-exe=myapp.exe] [--collect-files]");

            return ExitCode.InvalidArguments;
        }
        #endregion

        public ExitCode Execute()
        {
            if (_additionalArgs.Count == 0) return PrintHelp();

            switch (_additionalArgs[0])
            {
                case "start":
                    return Start();
                case "finish":
                    return Finish();
                default:
                    return PrintHelp();
            }
        }

        private ExitCode Start()
        {
            if (_additionalArgs.Count != 2) return PrintHelp();
            string snapshotFile = _additionalArgs[1];
            if (FileExists(snapshotFile)) return ExitCode.IOError;

            var session = CaptureSession.Start(new FeedBuilder());
            session.Save(snapshotFile);

            return ExitCode.OK;
        }

        private ExitCode Finish()
        {
            if (_additionalArgs.Count != 3) return PrintHelp();
            string snapshotFile = _additionalArgs[1];
            string feedFile = _additionalArgs[2];
            if (FileExists(feedFile)) return ExitCode.IOError;

            var feedBuilder = new FeedBuilder();
            var session = CaptureSession.Load(snapshotFile, feedBuilder);

            session.InstallationDir = _installationDirectory;
            session.Diff(_handler);

            feedBuilder.MainCandidate = string.IsNullOrEmpty(_mainExe)
                ? feedBuilder.Candidates.FirstOrDefault()
                : feedBuilder.Candidates.FirstOrDefault(x => StringUtils.EqualsIgnoreCase(FileUtils.UnifySlashes(x.RelativePath), _mainExe));
            session.Finish();
            feedBuilder.Build().Save(feedFile);

            if (_collectFiles) session.CollectFiles();

            return ExitCode.OK;
        }

        private bool FileExists(string path)
        {
            if (File.Exists(path) && !_force)
            {
                Log.Error(string.Format("The file '{0}' already exists. Use --force to overwrite.", Path.GetFullPath(path)));
                return true;
            }
            else return false;
        }
    }
}