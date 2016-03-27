﻿/*
 * Copyright 2010-2016 Bastian Eicher
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

using FluentAssertions;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Requirements"/>.
    /// </summary>
    [TestFixture]
    public class RequirementsTest
    {
        #region Helpers
        /// <summary>
        /// Creates test <see cref="Requirements"/>.
        /// </summary>
        public static Requirements CreateTestRequirements()
        {
            return new Requirements(FeedTest.Test1Uri, "command", new Architecture(OS.Windows, Cpu.I586))
            {
                //Languages = {"de-DE", "en-US"},
                ExtraRestrictions =
                {
                    {FeedTest.Test1Uri, new VersionRange("1.0..!2.0")},
                    {FeedTest.Test2Uri, new VersionRange("2.0..!3.0")}
                }
            };
        }
        #endregion

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var requirements1 = CreateTestRequirements();
            requirements1.Languages.Add("fr");
            var requirements2 = requirements1.Clone();

            // Ensure data stayed the same
            requirements2.Should().Be(requirements1, because: "Cloned objects should be equal.");
            requirements2.GetHashCode().Should().Be(requirements1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            requirements2.Should().NotBeSameAs(requirements1, because: "Cloning should not return the same reference.");
        }

        [Test(Description = "Ensures that the class can be serialized to a command-line argument string")]
        public void TestToCommandLineArgs()
        {
            CreateTestRequirements().ToCommandLineArgs()
                .Should().Equal("--command", "command", "--os", "Windows", "--cpu", "i586", "--version-for", "http://0install.de/feeds/test/test1.xml", "1.0..!2.0", "--version-for", "http://0install.de/feeds/test/test2.xml", "2.0..!3.0", "http://0install.de/feeds/test/test1.xml");
        }

        [Test]
        public void TestJson()
        {
            CreateTestRequirements().ToJsonString()
                .Should().Be("{\"interface\":\"http://0install.de/feeds/test/test1.xml\",\"command\":\"command\",\"source\":false,\"os\":\"Windows\",\"cpu\":\"i586\",\"extra_restrictions\":{\"http://0install.de/feeds/test/test1.xml\":\"1.0..!2.0\",\"http://0install.de/feeds/test/test2.xml\":\"2.0..!3.0\"}}");
        }

        [Test]
        public void TestXml()
        {
            var requirements = new Requirements(FeedTest.Test1Uri, "command", new Architecture(OS.Windows, Cpu.I586));
            string xml = requirements.ToXmlString();
            XmlStorage.FromXmlString<Requirements>(xml).Should().Be(requirements);
        }
    }
}
