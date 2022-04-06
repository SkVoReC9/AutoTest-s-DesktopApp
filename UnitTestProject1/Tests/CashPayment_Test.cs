using System;
using System.Collections.Generic;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject1.Methods;
using NUnit.Framework;
using UnitTestProject1.Tests;

namespace UnitTestProject1
{ 
    [TestFixture]
    public class GemopayTestFunc : BaseTest
    {

        [Test]
        public void CashPayment_Test()
        {   
            //Открываем Gemopay
            methods.OpenGemopay();
            //Открываем окно Свободных чеков и получаем поле для определения среды
            bool IsProd = methods.OpenFreeCheque();
            //Проводим тест оплаты и получаем номер заказа при оплате
            var Order = methods.TakePaymentGemopay(1);
            //Проверяем по базе и получаем ответ
            var expected_value = sQLHelper.CheckLastOrder_SQL(Order);
            var NDS_Exist = sQLHelper.CheckNDSExist_bySQL(Order);
            if (NDS_Exist == "7777")
            {
                Assert.IsTrue(false);
            }
            Console.WriteLine("НДС по заказу в тесте составляет {0}", NDS_Exist);
            //Если все ок, то тест пройден
            Assert.IsTrue(expected_value);
            Assert.IsNotNull(NDS_Exist);

        }

        //[Test]
        //public void CardPayment_Test()
        //{
        //    //Открываем Gemopay
        //    methods.OpenGemopay();
        //    //Открываем окно Свободных чеков и получаем поле для определения среды
        //    bool IsProd = methods.OpenFreeCheque();
        //    //Проводим тест оплаты и получаем номер заказа при оплате
        //    var Order = methods.TakePaymentGemopay(2);
        //    //Проверяем по базе и получаем ответ
        //    var expected_value = sQLHelper.CheckLastOrder_SQL(Order);
        //    var NDS_Exist = sQLHelper.CheckNDSExist_bySQL(Order);
        //    if (NDS_Exist == "7777")
        //    {
        //        Assert.IsTrue(false);
        //    }
        //    Console.WriteLine("НДС по заказу в тесте составляет {0}", NDS_Exist);
        //    //Если все ок, то тест пройден
        //    Assert.IsTrue(expected_value);
        //    Assert.IsNotNull(NDS_Exist);
        //}

        //[Test]
        //public void CombinePayment_Test()
        //{
        //    //Открываем Gemopay
        //    methods.OpenGemopay();
        //    //Открываем окно Свободных чеков и получаем поле для определения среды
        //    bool IsProd = methods.OpenFreeCheque();
        //    //Проводим тест оплаты и получаем номер заказа при оплате
        //    var Order = methods.TakePaymentGemopay(3);
        //    //Проверяем по базе и получаем ответ
        //    var expected_value = sQLHelper.CheckLastOrder_SQL(Order);
        //    var NDS_Exist = sQLHelper.CheckNDSExist_bySQL(Order);
        //    if (NDS_Exist == "7777")
        //    {
        //        Assert.IsTrue(false);
        //    }
        //    Console.WriteLine("НДС по заказу в тесте составляет {0}", NDS_Exist);
        //    //Если все ок, то тест пройден
        //    Assert.IsTrue(expected_value);
        //    Assert.IsNotNull(NDS_Exist);
        //}


        ////[TestMethod]
        ////public void FreeChequeSend_Test()
        ////{
        ////    Открываем Gemopay
        ////    methods.OpenGemopay();
        ////    Отправляем чеки в Skynet
        ////    methods.SendCheque();
        ////}

        //[Test]
        //public void TakeReturnOfOrder_ByMoney()
        //{
        //    Dictionary<string, double> ReceiptNumber = sQLHelper.GetOrderForReturn_bySQL();
        //    if(!ReceiptNumber.ContainsKey("NaN"))
        //    {
        //        methods.OpenGemopay();
        //        methods.OpenFreeCheque();
        //        var cheque = methods.ReturnOrder_ByMoney(ReceiptNumber);
        //    }else
        //    {
        //        Assert.Fail();
        //    }
            
        //}


        ////ЭТОТ МЕТОД ТОЛЬКО ДЛЯ ДЕБАГА НОВЫХ ФУНКЦИЙ!!! ДЛЯ ЗАПУСКА НУЖНЫХ ТЕСТОВ ИСПОЛЬЗУЕМ Manifest.playlist
        ////[TestMethod]
        ////public void debug()
        ////{
        ////    methods.OpenGemopay();
        ////    methods.Load_Services_For_FreeReceipts();

        ////frmCashInOut - Форма для внесения/изъятия средств. Внести/Изъять наличность имя по которому будем заходить туда
        //// ККМ для работы с загрузкой услуг txtKKMNumber -> 00018727 btnFRCheck -> Click btnOk -> Click
        ////}

    }
}
