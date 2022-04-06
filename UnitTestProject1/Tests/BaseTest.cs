using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject1.Methods;
using NUnit.Framework;

namespace UnitTestProject1.Tests
{
    public class BaseTest
    {
        public TestContext testContext { get; set; }
        public SQLHelper sQLHelper { get; set; }
        public MethodsForTests methods { get; set; }

        [SetUp]
        public void Init_Settings()
        {
            var choose_country = TestContext.Parameters["Country"].ToString();
            methods = new MethodsForTests();
            methods.OpenGemopay();
            methods.Choose_Country_and_OFD_type(choose_country);
            methods.OpenGemopay();
            methods.ChangeKKMNumber(choose_country);
            methods.OpenGemopay();
            methods.Load_Services_For_FreeReceipts();
        }
        [TearDown]
        public void CleanUp()
        {
            //process[] process = process.getprocessesbyname("gemopay");
            //foreach (process p in process)
            //{
            //    p.kill();
            //}

        }
    }
}
