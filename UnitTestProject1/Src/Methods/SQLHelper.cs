using System;
using System.Collections.Generic;
using UnitTestProject1.NDSEnum;
using System.Data.SQLite;

namespace UnitTestProject1.Methods
{
    public class SQLHelper
    {
        string connectionString = "Data source=C:\\Users\\aleksandr.skvortsov\\Documents\\GemoPay\\DB\\PC_DB.sqlite;Cache=Shared;Mode=ReadOnly;";
        double receipt_amount;
        string receipt_num;
        int ID;
        string NDS;

        /// <summary>
        /// Метод по выбору номера чека и сумме заказа через SQL
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, double> GetOrderForReturn_bySQL()
        {
            //Создаем словарь с определенным набором в данном случае string - номер заказа, double - сумма заказа
            Dictionary<string, double> ReceiptInfo = new Dictionary<string, double>();
            //Создаем экземпляр класса SQL
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            //Пробуем подключиться к базе 
            try
            {
                //Если успешно мы провалимся дальше
                connection.Open();
            }
            //А если нет, то создаем исключение, записываем сообщение и по итогу теста выкидываем его в лог
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //Получаем количество проданных товаров

            //Если нет то возвращаем NaN
            SQLiteCommand command = new SQLiteCommand
            {
                //Прописываем свойство для команды в тело 
                Connection = connection,
                //Прописываем саму команду в теле
                CommandText = "SELECT ReceiptNumber, TotalAmount from FreeReceipts where OrderNumber in (SELECT OrderNumber from FreeReceipts  group by ReceiptNumber having count(*) < 2) and ReceiptType <> 2 order by CreatedAt desc limit 1"
            };
            //Выполняем команду с помощью ридера
            SQLiteDataReader reader = command.ExecuteReader();
            //Проверяем есть ли заказ по нашему запросу
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    //Приписываем номер чека и сумму заказа в наш словарь
                    receipt_num = reader.GetString(0);
                    receipt_amount = reader.GetDouble(1);
                    ReceiptInfo.Add(receipt_num, receipt_amount);
                }
                //Если есть, мы закрываем ридер и соединение с базой и возвращаем true
                reader.Close();
                connection.Close();

                return ReceiptInfo;
            }
            else
            {
                //Если нет, то мы закрываем ридер и соединение и возвращаем false
                reader.Close();
                connection.Close();
                ReceiptInfo.Add("NaN", 0);
                return ReceiptInfo;
            }
        }

        /// <summary>
        /// Проверка, что оплата записалась в базу и совпадает с заказом
        /// </summary>
        /// <param name="Order"></param>
        /// <returns></returns>
        public bool CheckLastOrder_SQL(string Order)
        {
            //Создаем экземпляр класса SQL
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            //Пробуем подключиться к базе 
            try
            {
                //Если успешно мы провалимся дальше
                connection.Open();
            }
            //А если нет, то создаем исключение, записываем сообщение и по итогу теста выкидываем его в лог
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            //При успешном подключении мы создаем команду для SQL
            SQLiteCommand command = new SQLiteCommand
            {
                //Прописываем свойство для команды в тело 
                Connection = connection,
                //Прописываем саму команду в теле
                CommandText = "SELECT OrderNumber FROM FreeReceipts where OrderNumber = " + Order
            };
            //Выполняем команду с помощью ридера
            SQLiteDataReader reader = command.ExecuteReader();
            //Проверяем есть ли заказ по нашему запросу
            if (reader.HasRows)
            {
                //Если есть, мы закрываем ридер и соединение с базой и возвращаем true
                reader.Close();
                connection.Close();
                return true;
            }
            else
            {
                //Если нет, то мы закрываем ридер и соединение и возвращаем false
                reader.Close();
                connection.Close();
                return false;
            }

        }

        /// <summary>
        /// Проверка типа оплаты у заказы
        /// </summary>
        /// <param name="Cheque_number"></param>
        /// <returns></returns>
        public int CheckPayment_bySQL(string Cheque_number)
        {
            //Задаем переменные которые проверяют оплаты по карте и по наличным
            int TotalAmountPaid = 0;
            int TotalAmount = 0;
            //Конектимся к БД
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            try
            {
                //Если успешно мы провалимся дальше
                connection.Open();
            }
            //А если нет, то создаем исключение, записываем сообщение и по итогу теста выкидываем его в лог
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Данное число означает, что произошла ошибка
                return 99;
            }
            //Выбираем чек по номеру
            SQLiteCommand command = new SQLiteCommand
            {
                //Прописываем свойство для команды в тело 
                Connection = connection,
                //Прописываем саму команду в теле
                CommandText = "select TotalAmountPaid, TotalAmount from FreeReceipts where ReceiptNumber = " + Cheque_number
            };
            //Находим чек и приписываем к переменным сумму оплат
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    TotalAmountPaid = reader.GetInt32(0);
                    TotalAmount = reader.GetInt32(1);
                }
                //Если есть, мы закрываем ридер и соединение с базой и возвращаем true
                reader.Close();
                connection.Close();
            }
            else
            {
                return 99;
            }
            //Проверяем какой тип оплаты, если по наличности нет суммы, то это безналичная оплата и возвращаем 3
            if (TotalAmountPaid == 0)
            {
                //Возвращаем безналичную
                return 3;
            }
            //Но если есть
            else
            {
                //То проверяем гибридная она или нет, проверяем, что общая сумма по наличным не равна нулю и что она меньше общей суммы заказа
                if (TotalAmountPaid != 0 && TotalAmount > TotalAmountPaid)
                {
                    //Возвращаем гибридную
                    return 2;
                }
                else
                {
                    //Возвращаем наличную
                    return 1;
                }
            }
        }

        public string CheckNDSExist_bySQL(string Order)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            //Пробуем подключиться к базе 
            try
            {
                //Если успешно мы провалимся дальше
                connection.Open();
            }
            //А если нет, то создаем исключение, записываем сообщение и по итогу теста выкидываем его в лог
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "7777";
            }

            SQLiteCommand command = new SQLiteCommand
            {
                Connection = connection,
                CommandText = "SELECT Id FROM FreeReceipts where OrderNumber = " + Order
            };
            SQLiteDataReader reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                while (reader.Read())
                {
                    ID = reader.GetInt32(0);
                }
                reader.Close();
            }

            SQLiteCommand command1 = new SQLiteCommand
            {
                Connection = connection,
                CommandText = "Select NDS from FreeReceiptServices where FreeReceiptId = " + ID
            };

            SQLiteDataReader reader1 = command1.ExecuteReader();
            if(reader1.HasRows)
            {
                while(reader1.Read())
                {
                    NDS = reader1.GetString(0);
                }
                reader1.Close();
            }
            connection.Close();
            if(NDS == "Без НДС")
            {
                NDS = 0.ToString();
            }
            NDSValue nds = (NDSValue)Enum.Parse(typeof(NDSValue), NDS);

            return nds.ToString();
        }
    }
}
