﻿using FluentSharp.NUnit;
using FluentSharp.O2Platform;
using NUnit.Framework;

namespace UnitTests.FluentSharp.O2Platform
{    
    public class Test_O2_Platform_Config : NUnitTests
    {
        [Test]
        public void O2PlatformConfig_Ctor()
        {
            var o2PlatformConfig = O2_Platform_Config.Current;
            var appData          = o2PlatformConfig.CurrentUser_AppData;
            var mainFolder       = o2PlatformConfig.Folder_Root;

            o2PlatformConfig.assert_Is_Not_Null  ();
            appData         .assert_Folder_Exists();
            mainFolder      .assert_Folder_Exists();
        }
    }
}
