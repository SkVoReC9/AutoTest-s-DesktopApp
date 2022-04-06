using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System.Threading;
using System;
using System.Collections.Generic;
namespace UnitTestProject1.Src.FlaUI_Helper
{
    public class FlaUI_Conditions
    { 
        public void FindElement_byAutomationID(string AutomationID, string TypeOfElement, AutomationElement element, ConditionFactory condition)
        {
            switch (TypeOfElement)
            {
                case "Button":
                    element.FindFirstDescendant(condition.ByAutomationId("Maximize-Restore")).AsButton();
                    break;
                case "TextBox":
                    break;
                case "ComboBox":
                    break;
                default:
                    break;
            }
        }

        public void FindElement_byName(string Name, string TypeOfElement, AutomationElement element, ConditionFactory condition)
        {
            switch (TypeOfElement)
            {
                case "Button":
                    element.FindFirstDescendant(condition.ByAutomationId("Maximize-Restore")).AsButton();
                    break;
                case "TextBox":
                    break;
                case "ComboBox":
                    break;
                default:
                    break;
            }
        }

        public void FindElement_byAutomationID_In_Window(string AutomationID, string TypeOfElement, AutomationElement element, ConditionFactory condition)
        {
            switch (TypeOfElement)
            {
                case "Button":
                    element.FindFirstDescendant(condition.ByAutomationId("Maximize-Restore")).AsButton();
                    break;
                case "TextBox":
                    break;
                case "ComboBox":
                    break;
                default:
                    break;
            }
        }

        public void FindElement_byName_In_Window(string AutomationID, string TypeOfElement, AutomationElement element, ConditionFactory condition)
        {
            switch (TypeOfElement)
            {
                case "Button":
                    element.FindFirstDescendant(condition.ByAutomationId("Maximize-Restore")).AsButton();
                    break;
                case "TextBox":
                    break;
                case "ComboBox":
                    break;
                default:
                    break;
            }
        }
    }
}
