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

using System.IO;
using FluentAssertions;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using Xunit;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="CatalogManager"/>.
    /// </summary>
    public class CatalogManagerTest : TestWithContainer<CatalogManager>
    {
        [Fact]
        public void TestGetOnline()
        {
            var catalog = CatalogTest.CreateTestCatalog();
            catalog.Normalize();

            var catalogStream = new MemoryStream();
            catalog.SaveXml(catalogStream);
            var array = catalogStream.ToArray();
            catalogStream.Position = 0;

            using (var server = new MicroServer("catalog.xml", catalogStream))
            {
                var uri = new FeedUri(server.FileUri);
                CatalogManager.SetSources(new[] {uri});
                GetMock<ITrustManager>().Setup(x => x.CheckTrust(array, uri, null)).Returns(OpenPgpUtilsTest.TestSignature);

                Sut.GetOnline().Should().Be(catalog);
            }
        }

        [Fact]
        public void TestGetCached()
        {
            var catalog = CatalogTest.CreateTestCatalog();
            catalog.Normalize();

            Sut.GetCached().Should().BeNull();
            TestGetOnline();
            Sut.GetCached().Should().Be(catalog);
        }

        private static readonly FeedUri _testSource = new FeedUri("http://localhost/test/");

        [Fact]
        public void TestAddSourceExisting()
        {
            Sut.AddSource(CatalogManager.DefaultSource).Should().BeFalse();
            CatalogManager.GetSources().Should().Equal(CatalogManager.DefaultSource);
        }

        [Fact]
        public void TestAddSourceNew()
        {
            Sut.AddSource(_testSource).Should().BeTrue();
            CatalogManager.GetSources().Should().Equal(CatalogManager.DefaultSource, _testSource);
        }

        [Fact]
        public void TestRemoveSource()
        {
            Sut.RemoveSource(CatalogManager.DefaultSource).Should().BeTrue();
            CatalogManager.GetSources().Should().BeEmpty();
        }

        [Fact]
        public void TestRemoveSourceMissing()
        {
            Sut.RemoveSource(_testSource).Should().BeFalse();
            CatalogManager.GetSources().Should().Equal(CatalogManager.DefaultSource);
        }

        [Fact]
        public void TestSetSources()
        {
            CatalogManager.SetSources(new[] {_testSource});
            CatalogManager.GetSources().Should().Equal(_testSource);
        }
    }
}
