using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeleSharp.TL;
using TeleSharp.TL.Contacts;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using TLSharp.Core.Network;

namespace TLSpammer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Данные для работы с телеграмм клиентом
        public const int API_ID = 571639;
        public const string API_HASH = "7674768b0ead3a637c270a5f41783083";

        //Сам клиент. Нужен для подключения к сервису
        TelegramClient client;

        //Сами пользователи
        public TLUser user;

        //Их хэш для создания пользователя
        string hash_1;

        public  MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        //Обработчик логики авторизации
        private async void Btn_Auth_Click(object sender, RoutedEventArgs e)
        {
            if (Txb_Code1.Text.Length > 0)
            {

                if (hash_1 != null)
                {
                    //Подключаемся к существующему файлу сессии если она есть и данные совпадают. Если же файла нет - 
                    //он создается, если файл есть но данные не идентичны - перезаписываем сессию.
                    var session = new FileSessionStore();
                    //Подключаем клиент с указанием файла сессии.
                    var cl = new TelegramClient(API_ID, API_HASH, session, "session");

                    //Отправляем запрос на подключение клиента
                    await client.ConnectAsync();

                    //Авторизовываем пользователя
                    var user1 = await client.MakeAuthAsync(Txb_Phone1.Text, hash_1, Txb_Code1.Text);
                    
                    //Запоминаем пользователя и клиент в соответствующие переменные
                    user = user1;

                    client = cl;

                    //Запускаем следующее окно
                    AddPhones nextWin = new AddPhones();
                    nextWin.Show();
                }
                else
                {
                    MessageBox.Show("Что-то не так. Попробуйте снова", 
                                    "Ошибка", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error, 
                                    MessageBoxResult.OK);
                }
            }
        }

        private async void Btn_SendCodes_Click(object sender, RoutedEventArgs e)
        { 

            if (Txb_Phone1.Text.Length > 0)
            {
                try
                {
                    //Подключаем клиент.
                    client = new TelegramClient(API_ID, API_HASH);
                    await client.ConnectAsync();

                    //Высылаем код подтверждения
                    var hash1 = await client.SendCodeRequestAsync(Txb_Phone1.Text);
                    //Запоминаем хеш авторизуемового пользователя. Понадобится для его подключения
                    hash_1 = hash1;

                } catch (Exception ex)
                {
                    if(ex is FloodException)
                    {
                        MessageBox.Show("Слишком частое подключение к сессии. Придется подождать." + ex.Message, "FloodException", MessageBoxButton.OK, MessageBoxImage.Error);
                    } else if (ex is InvalidOperationException)
                    {
                        MessageBox.Show(ex.Message, "InvalidOperationException", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    MessageBox.Show(ex.ToString());
                } 
            }

        }
    }
}
