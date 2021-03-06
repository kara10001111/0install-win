// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Collections;
using ZeroInstall.Commands;
using ZeroInstall.Commands.Basic;

ProgramUtils.Init();

using var handler = new CliCommandHandler();
return (int)ProgramUtils.Run("0install", args.Prepend(Run.Name), handler);
