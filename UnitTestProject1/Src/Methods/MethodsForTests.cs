using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System.Threading;
using System;
using System.Collections.Generic;

namespace UnitTestProject1.Methods
{
    public class MethodsForTests
    {
        /// <summary>
        /// Общие поля класса для работы всех методов с окном Gemopay 
        /// appGP поле для приложения Gemopay, сюда записывается сам процесс и из него забираются все окна 
        /// automationGP, переменная для подключения фреймворка к приложению
        /// conditionGP, переменная для поиска и работы с элементами формы Gemopay
        /// winGP1 основное окно Gemopay, именно в нем происходит поиск элементов и работа с ними
        /// </summary>
        Application appGP;
        UIA3Automation automationGP;
        ConditionFactory conditionGP;
        Window winGP1;
        string Cheque;
        string[] Services = { "GNP105", "12", "31.27", "9.1" };
        SQLHelper helper = new SQLHelper();
        //Создаем подключение к БД Gemopay 

        /// <summary>
        /// Метод для открытия контекстного меню из под рабочего стола
        /// </summary>
        public void OpenGemopay()
        {
            //Присоединение к процессу рабочего стола, чтобы открыть меню Gemopay
            Application appDesktop = Application.Attach("explorer.exe");
            UIA3Automation uIA3Automation = new UIA3Automation();
            ConditionFactory condition = new ConditionFactory(new UIA3PropertyLibrary());
            //Получаем экземпляр рабочего стола и нажимаем на кнопочку Gemopay
            Window winDesktop = appDesktop.GetMainWindow(uIA3Automation);
            winDesktop.FindFirstDescendant(condition.ByName("GemoPay")).AsButton().RightClick();
        }

        /// <summary>
        /// Метод для привязки к процессу Gemopay и открытия окна Свободных чеков
        /// </summary>
        public bool OpenFreeCheque()
        {
            //Присоединяемся к процессу Gwmopay 
            appGP = Application.Attach("Gemopay.exe");
            automationGP = new UIA3Automation();
            conditionGP = new ConditionFactory(new UIA3PropertyLibrary());
            //Получаем экземпляр открывшегося окна Gemopay
            Window winGP = appGP.GetMainWindow(automationGP);
            //Получаем заголовок окна. Prod это реальный ккм, Test это эмулятор ККМ
            var Prod_Or_Test = winGP.Title;
            //Нажимаем на вкладку свободные чеки, чтобы их открыть
            winGP.FindFirstDescendant(conditionGP.ByName("Свободные чеки")).AsMenuItem().Click();
            winGP.Close();
            //Обработка заголовка окна, если true то в дальнейшем тесты будут использовать укороченный формат тестов, если false дополнительные данные для эмулятора
            if (Prod_Or_Test == "Prod")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Метод проведения возврата по чеку и сумме заказа
        /// </summary>
        /// <param name="ReceiptNumber"></param>
        /// <returns></returns>
        public string ReturnOrder_ByMoney(Dictionary<string, double> ReceiptNumber)
        {

            //Получаем экземпляр окна Свободные чеки
            winGP1 = appGP.GetMainWindow(automationGP);
            //Разворачиваем заказ на весь экран
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("Maximize-Restore")).AsButton().Click();
            //Создаем новый чек
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnNewCheck")).AsButton().Click();
            //Выбираем тип Продажа
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("chkRefund")).AsCheckBox().Click();
            //Вводим номер чека по заказу
            var v = ReceiptNumber.GetEnumerator();
            //Начинаем перебирать наш словарь, что вставить их данные в форму, т.к просто забрать из словаря не получится данные
            foreach (KeyValuePair<string, double> keyValuePair in ReceiptNumber)
            {
                //Проставлем номер чека и сумму
                winGP1.FindFirstDescendant(conditionGP.ByAutomationId("frmReturnReceipt")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("txtReceiptNum")).AsTextBox().Enter(keyValuePair.Key);
                winGP1.FindFirstDescendant(conditionGP.ByAutomationId("frmReturnReceipt")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("ctrlReceiptSum")).AsTextBox().Enter(keyValuePair.Value.ToString());
                //Забираем номер чека, чтобы проверить тип оплаты
                Cheque = keyValuePair.Key;
            }
            //Выбираем оператора
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("cmbOperator")).AsComboBox().Select(0).Click();
            //И два раза нажимаем далее
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnNext")).AsButton().Click();
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnNext")).AsButton().Click();
            //Проверяем какая оплата по заказу
            int IsCard = helper.CheckPayment_bySQL(Cheque);
            //Функция перебора типа оплаты и выполнения выплаты на эмуляторе
            switch (IsCard)
            {
                case 1:
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    winGP1.Close();
                    return Cheque;
                case 2:
                    //Затем фискальный чек
                    //Т.к оплата комбинированная то сначала у нас идет оплата по карте, в окне эмулятора мы подтверждаем операцию по карте
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор пинпада")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("btnOk")).AsButton().Click();
                    //Печатаем слип
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    //Закрываем окно
                    winGP1.Close();
                    return Cheque;
                case 3:
                    //Т.к оплата комбинированная то сначала у нас идет оплата по карте, в окне эмулятора мы подтверждаем операцию по карте
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор пинпада")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("btnOk")).AsButton().Click();
                    //Печатаем слип
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    //Затем фискальный чек
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    winGP1.Close();
                    return Cheque;
                default:
                    return Cheque;

            }
        }


        /// <summary>
        /// Метод для теста принудительной отправки чека
        /// </summary>
        public void SendCheque()
        {
            //Присоединяемся к процессу Gemopay
            appGP = Application.Attach("Gemopay.exe");
            automationGP = new UIA3Automation();
            conditionGP = new ConditionFactory(new UIA3PropertyLibrary());
            //Получаем экземпляр окна Gemopay
            Window winGP = appGP.GetMainWindow(automationGP);
            //winGP.FindFirstDescendant(conditionGP.ByName("Служебные")).AsMenuItem().Click();
            //Получаем все доступные элементы вкладки Служебные
            var menu = winGP.FindFirstDescendant(conditionGP.ByName("Служебные")).AsMenuItem();
            //Т.к мы храним объекты, а не экземпляры, то нужно сделать вызов функции объекта через Invoke, в качестве элемента массива указываем его имя
            menu.Items["Отправить свободные чеки"].Invoke();
            //Закрываем окно
            winGP.Close();
        }

        /// <summary>
        /// Метод для внесения и изъятия наличности посредством Gemopay
        /// </summary>
        public void CashIn()
        {
            //Присоединяемся к процессу Gemopay
            appGP = Application.Attach("Gemopay.exe");
            automationGP = new UIA3Automation();
            conditionGP = new ConditionFactory(new UIA3PropertyLibrary());
            //Получаем экземпляр окна Gemopay
            Window winGP = appGP.GetMainWindow(automationGP);
            var menu = winGP.FindFirstDescendant(conditionGP.ByName("Служебные")).AsMenuItem();
            menu.Items["Внести/Изъять наличность"].Invoke();
            Window[] winGp2 = appGP.GetAllTopLevelWindows(automationGP);
            winGP1 = winGp2[0];
        }


        public string TakePaymentGemopay(int NumberOfTest)
        {
            double TotalPrice = 0;
            winGP1 = appGP.GetMainWindow(automationGP);
            Thread.Sleep(3000);
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("Maximize-Restore")).AsButton().Click();
            //Создаем новый чек
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnNewCheck")).AsButton().Click();
            //Выбираем тип продажа
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("chkSale")).AsCheckBox().Click();
            //Получаем номер заказа, который потом будет проверяться по базе Gemopay 
            var Order = winGP1.FindFirstDescendant(conditionGP.ByAutomationId("numOrderNumber")).AsComboBox().FindFirstDescendant(conditionGP.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)).AsTextBox().Text;
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("cmbOperator")).AsComboBox().Select(0).Click();
            //В зависимости от номера теста, мы проставляем определенное имя и дату рождения
            switch(NumberOfTest)
            {
                case 1:
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("txtPatientName")).AsTextBox().Enter("Автотест Пациент Наличная оплата");
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("dtPatientBirthday")).AsTextBox().Enter("28.2.1980");
                    goto C;
                case 2:
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("txtPatientName")).AsTextBox().Enter("Автотест Пациент Оплата Картой");
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("dtPatientBirthday")).AsTextBox().Enter("10.12.1980");
                    goto C;
                case 3:
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("txtPatientName")).AsTextBox().Enter("Автотест Пациент Комбинированный");
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("dtPatientBirthday")).AsTextBox().Enter("28.2.1990");
                    goto C;
                default:
                    goto E;

            }
        C:
            //Нажимаем добавить услугу
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnAddService")).AsButton().Click();
            //Вводим номер услуги
            Random rand = new Random();
            string Service = Services[rand.Next(Services.Length)];
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("frmCheckServiceAdd")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("txtServiceMask")).AsTextBox().Enter(Service);
            Thread.Sleep(500);
            //Выбираем первую услугу в списке
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("grid")).AsDataGridView().FindFirstDescendant(conditionGP.ByName("Строка 0")).AsGridCell().DoubleClick();
            //Также как и с предыдущим Switch, мы выбираем для каждого номера, свой тип оплаты
            switch(NumberOfTest)
            {
                case 1:
                    //Нажимаем Выписать чек
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnSave")).AsButton().Click();
                    //Нажимаем провести оплату
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("frmPayment")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("btnRetry")).AsButton().Click();
                    //Т.к эта ветка для эмулятора, то мы дополнительно нажимаем ДА в форме эмулятора
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    //Закрываем окно Свободные чеки
                    winGP1.Close();
                    return Order;
                case 2:
                    //Забираем общую сумму заказ
                    var TotalPriceCard = winGP1.FindFirstDescendant(conditionGP.ByAutomationId("txtTotalAmount")).AsTextBox().Text;
                    //И печатаем ее в поле оплата по карте
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("txtBankCardSum")).AsTextBox().Enter(TotalPriceCard.ToString());
                    //Нажимаем Выписать чек
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnSave")).AsButton().Click();
                    //В эмуляторе пинпада выбираем успешную оплату
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор пинпада")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("btnOk")).AsButton().Click();
                    //Печатаем слип
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    //Затем фискальный чек
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    winGP1.Close();
                    return Order;
                case 3:
                    //Т.к это комбинированная оплата мы забираем общую цену заказа
                    TotalPrice = Double.Parse(winGP1.FindFirstDescendant(conditionGP.ByAutomationId("txtTotalAmount")).AsTextBox().Text);
                    //Делим сумму пополам без остатка
                    double PriceCard = TotalPrice / 2;
                    //Добавляем полученную сумму деления в поле оплата по карте, а поле оплата наличными Gemopay заполнить автоматически
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("txtBankCardSum")).AsTextBox().Enter(PriceCard.ToString());
                    //Нажимаем Выписать чек
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnSave")).AsButton().Click();
                    //Нажимаем провести оплату
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("frmPayment")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("btnRetry")).AsButton().Click();
                    //Т.к оплата комбинированная то сначала у нас идет оплата по карте, в окне эмулятора мы подтверждаем операцию по карте
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор пинпада")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("btnOk")).AsButton().Click();
                    //Затем подтверждаем печать слипа
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    //А затем печать фискального чека
                    winGP1.FindFirstDescendant(conditionGP.ByName("Эмулятор ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("1")).AsButton().Click();
                    //Закрываем окно
                    winGP1.Close();
                    return Order;
                default:
                    goto E;
            }
            //Если прозошла какая-либо ошибка, мы возвращаем сообщение об ошибке
        E:
            return "Error!";
        }
        
        public void Choose_Country_and_OFD_type(string country)
        {
            appGP = Application.Attach("Gemopay.exe");
            automationGP = new UIA3Automation();
            conditionGP = new ConditionFactory(new UIA3PropertyLibrary());
            Window winGP = appGP.GetMainWindow(automationGP);
            //winGP.FindFirstDescendant(conditionGP.ByName("Служебные")).AsMenuItem().Click();
            //Нажимаем на настройки
            winGP.FindFirstDescendant(conditionGP.ByName("Настройки")).AsMenuItem().Click();
            Thread.Sleep(500);
            //Пооучаем все окна приложения Gemopay
            Window[] winGP2 = appGP.GetAllTopLevelWindows(automationGP);
            //Выбираем которое поверх всех
            winGP1 = winGP2[0];
            //Переходим во вкладку дополнительно
            winGP1.FindFirstDescendant(conditionGP.ByName("Дополнительно")).AsTabItem().Click();
            //Подавляющее число отделений Киргизии работает на ОФД-2 поэтому мы выбираем его, но в будущем будет возможность параметрами выбирать его
            if (country == "rus")
            {
                winGP1.FindFirstDescendant(conditionGP.ByAutomationId("cmbCountry")).AsComboBox().Select(0).Click();
                //Если да, нажимаем применить
                winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnSave")).AsButton().Click();
                winGP1.FindFirstDescendant(conditionGP.ByName("Изменение настроек")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("2")).AsButton().Click();
                //И выходим из настроек
                winGP1.Close();
            }
            else
            {
                winGP1.FindFirstDescendant(conditionGP.ByAutomationId("cmbCountry")).AsComboBox().Select(1).Click();

                var checkOFD = winGP1.FindFirstDescendant(conditionGP.ByAutomationId("rbtnOFD2")).AsRadioButton().IsChecked;
                //Проверяем проставлен ли офд-2
                if (checkOFD)
                {
                    //Если да, нажимаем применить
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnSave")).AsButton().Click();
                    winGP1.FindFirstDescendant(conditionGP.ByName("Изменение настроек")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("2")).AsButton().Click();
                    //И выходим из настроек
                    winGP1.Close();
                }
                else
                {
                    //А если нет, то мы выбирае ОФД-2 и нажимаем применить
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("rbtnOFD2")).AsRadioButton().Click();
                    winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnSave")).AsButton().Click();
                    winGP1.FindFirstDescendant(conditionGP.ByName("Изменение настроек")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("2")).AsButton().Click();
                    //Выходим из настроек
                    winGP1.Close();
                }
            }
            winGP1 = appGP.GetMainWindow(automationGP);
            winGP1.Close();
        }

        /// <summary>
        /// Загрузка услуг из Skynet, актуально при смене страны и кассы
        /// </summary>
        public void Load_Services_For_FreeReceipts()
        {
            appGP = Application.Attach("Gemopay.exe");
            automationGP = new UIA3Automation();
            conditionGP = new ConditionFactory(new UIA3PropertyLibrary());
            Window winGP = appGP.GetMainWindow(automationGP);
            //winGP.FindFirstDescendant(conditionGP.ByName("Служебные")).AsMenuItem().Click();
            //Нажимаем на настройки
            winGP.FindFirstDescendant(conditionGP.ByName("Настройки")).AsMenuItem().Click();
            Thread.Sleep(500);
            //Пооучаем все окна приложения Gemopay
            Window[] winGP2 = appGP.GetAllTopLevelWindows(automationGP);
            //Выбираем которое поверх всех
            winGP1 = winGP2[0];
            winGP1.FindFirstDescendant(conditionGP.ByName("Справочники")).AsTabItem().Click();
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnLoad")).AsButton().Click();
            try
            {

                Retry.WhileException(() =>
                {
                    using (var automation = new UIA3Automation())
                    {
                        winGP1.FindFirstDescendant(conditionGP.ByName("Загрузка справочников")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("2")).AsButton().Click();
                    }

                }, TimeSpan.FromSeconds(30), null, true);
            }catch(Exception ex)
            {
                //Пролетаем мимо полуошибки
            }
            winGP1.Close();
            winGP1 = appGP.GetMainWindow(automationGP);
            winGP1.Close();
        }

        public void ChangeKKMNumber(string country)
        {
            appGP = Application.Attach("Gemopay.exe");
            automationGP = new UIA3Automation();
            conditionGP = new ConditionFactory(new UIA3PropertyLibrary());
            Window winGP = appGP.GetMainWindow(automationGP);
            //winGP.FindFirstDescendant(conditionGP.ByName("Служебные")).AsMenuItem().Click();
            //Нажимаем на настройки
            winGP.FindFirstDescendant(conditionGP.ByName("Настройки")).AsMenuItem().Click();
            Thread.Sleep(500);
            //Пооучаем все окна приложения Gemopay
            Window[] winGP2 = appGP.GetAllTopLevelWindows(automationGP);
            //Выбираем которое поверх всех
            winGP1 = winGP2[0];
            if(country == "rus")
            {
                winGP1.FindFirstDescendant(conditionGP.ByAutomationId("txtKKMNumber")).AsTextBox().Enter("00008019");
            }else
            {
                winGP1.FindFirstDescendant(conditionGP.ByAutomationId("txtKKMNumber")).AsTextBox().Enter("00018727");
            }
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnFRCheck")).AsButton().Click();
            Thread.Sleep(1000);
            winGP1.FindFirstDescendant(conditionGP.ByName("Проверка ФР")).AsWindow().FindFirstDescendant(conditionGP.ByAutomationId("2")).AsButton().Click();
            winGP1.FindFirstDescendant(conditionGP.ByAutomationId("btnClose")).AsButton().Click();
            winGP1 = appGP.GetMainWindow(automationGP);
            winGP1.Close();

        }

        public void Choose_Sending_Type(string type)
        {
            //frmPrintCheck
            //checkBoxPrint
            //txtMail
            //btnSendCheck
        }
    }


}


