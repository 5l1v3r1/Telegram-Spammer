using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TeleSharp.TL;
using TLSharp.Core;

namespace TLSpammer
{
    /// <summary>
    /// Логика взаимодействия для AddPhones.xaml
    /// </summary>
    
    public partial class AddPhones : Window
    {
        TelegramClient client;
        List<String> users = new List<string>();

        public AddPhones()
        {
            InitializeComponent();

            //Отключаем кнопку чтобы у пользователя не было возможности запустить цикл спама до того момента, пока
            //он не выберет целевую аудиторию (файл)
            Btn_BeginSpam.IsEnabled = false;
            
        }

        //Логика завершения приложения
        private void Mni_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        //Логика открытия файла и добавления пользователей
        private void Btn_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            //Разрешаем выбор только текстовых документов
            ofd.Filter = "Text File (*txt)|*.txt";

            if (ofd.ShowDialog() == true)
            {
                //Отображаем путь к выбранному файлу
                Txb_FPath.Text = ofd.FileName;
                
                using(StreamReader sr = new StreamReader(ofd.FileName))
                {
                    //Временная строка, для чтения в неё
                    String line;

                    while((line = sr.ReadLine()) != null)
                    {
                        //Убираем лишние пробелы во избежание проблем с отправкой
                        //т.е. должно быть так: @nickname
                        //            а не так:      @nickname
                        line = line.Replace(" ", string.Empty);

                        //Если строка не пустая - добавляем в список пользователей и увеличиваем прогресс бар
                        if (line != "" && line[0].Equals('@'))
                        {
                            users.Add(line);

                            Progr_Users.Value = users.Count();
                        }

                    }
                }

                //Если всё прошло хорошо - активируем кнопку рассылки спама
                Progr_Users.Value = 100;
                Btn_BeginSpam.IsEnabled = true;
            }//if
        }

        //Логика рассылки сообщений
        private async void Btn_BeginSpam_Click(object sender, RoutedEventArgs e)
        {
           try
           {
                Txb_Waiting.IsEnabled = false;

               // получаем текущую сессию
               var session = new FileSessionStore();
               //Создаем клиента
               var cl = new TelegramClient(MainWindow.API_ID, MainWindow.API_HASH, session, "session");

               //переопределяем клиента
               client = cl;

                int time_to_sleep;

                int.TryParse(Txb_Waiting.Text, out time_to_sleep);

               //подключаем клиента
               await client.ConnectAsync();

                int imax = users.Count;

               foreach (string username in users)
               {
                    //ищем пользователя по его Никнейму
                    var found = await client.SearchUserAsync(username, 1);

                    if (found.Users.Count > 0)
                    {
                        //получаем пользователя
                        var u = found.Users.lists.OfType<TLUser>().First();

                        if (!u.Bot)
                        {
                            //отправляем ему сообщение
                            await client.SendMessageAsync(new TLInputPeerUser() { UserId = u.Id, AccessHash = (long)u.AccessHash }, Txb_Message.Text);
                            Thread.Sleep(time_to_sleep);
                        }
                    }

                }

                Progr_Spam.Value = 100;

                Txb_Waiting.IsEnabled = true;
           }
           catch (Exception ex)
           {
               MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
           }
        }

        private void Mni_About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aw = new AboutWindow();
            aw.ShowDialog();
        }

        private void Mni_Guide_Click(object sender, RoutedEventArgs e)
        {
            GuideWindow gw = new GuideWindow();
            gw.ShowDialog();
        }
    }
}
