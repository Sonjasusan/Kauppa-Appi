﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace CrossPlatformTest
{
    [TestFixture(Platform.Android)] //testataan androidia
    //[TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void WelcomeTextIsDisplayed()
        {
            AppResult[] results = app.WaitForElement(c => c.Marked("Tervetuloa käyttämään Kauppa-Appia! Tämä on sovellus kaupassakäymisen helpottamiseen."));
            app.Screenshot("Welcome screen.");

            Assert.IsTrue(results.Any());
        }

        [Test]
        public void WelcomeTextIsNotDisplayed()
        {
            AppResult[] results = app.WaitForElement(c => c.Marked("Tervetuloa käyttämään Kauppa-Appia! Tämä on sovellus kaupassakäymisen helpottamiseen."));

            Assert.IsFalse(results.Any());
        }
    }
}
