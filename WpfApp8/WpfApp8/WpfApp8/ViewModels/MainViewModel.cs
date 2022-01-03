using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfApp8.Command;
using WpfApp8.Views;

namespace WpfApp8.ViewModels
{
    public class MainViewModel
    {
        public RelayCommand FileFromCommand { get; set; }
        public RelayCommand StartCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        MainView mainView;
        CancellationTokenSource cts;
        public List<string> Text { get; set; } = new List<string>();

        public MainViewModel(MainView _mainView)
        {
            mainView = _mainView;
            FileFromCommand = new RelayCommand
               (
                 act => FileButtonClick(),
                 pre => true
               );
            StartCommand = new RelayCommand
               (
                 act => ThreadPool.QueueUserWorkItem(e =>
                 {
                     cts = new CancellationTokenSource();
                     Process(cts.Token);
                 }),
                 pre => true
               );
            CancelCommand = new RelayCommand
              (
                act => 
                {
                    cts.Cancel();
                },
                pre => true
              );

        }

        public string XORCipher(string data, string key)
        {
            int dataLen = data.Length;
            int keyLen = key.Length;
            char[] output = new char[dataLen];

            for (int i = 0; i < dataLen; ++i)
            {
                output[i] = (char)(data[i] ^ key[i % keyLen]);
            }

            return new string(output);
        }

        public void FileButtonClick()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files|*.*|Text files|*.txt";
            openFileDialog.FilterIndex = 2;
            if (openFileDialog.ShowDialog() == true)
            {
               mainView.txbFile.Text = openFileDialog.FileName;
            }
        }

        public void Process(CancellationToken token)
        {

            string filePath = "";
            string passwordKey = "";
            bool? isEncrypt = null;
            mainView.Dispatcher.Invoke(new Action(() =>
            {
                filePath = mainView.txbFile.Text;
                passwordKey = mainView.txbPassword.Text;
                if (mainView.rbEncrypt.IsChecked == true)
                    isEncrypt = true;
                else if (mainView.rbDecrypt.IsChecked == true)
                    isEncrypt = false;
                mainView.pbLoading.Value = 0;
            }));


            if (!string.IsNullOrEmpty(filePath) && isEncrypt == true)
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    Text.Clear();
                    while ((line = sr.ReadLine()) != null)
                    {
                        Text.Add(line);
                    }
                }
                File.WriteAllText(filePath, String.Empty);
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    foreach (var item in Text)
                    {
                        if (token.IsCancellationRequested) break;
                        Thread.Sleep(500);
                        mainView.Dispatcher.Invoke(new Action(() => mainView.pbLoading.Value += 100 / Text.Count));
                        var encrypt = XORCipher(item, passwordKey);
                        sw.WriteLine(encrypt);
                    }
                }
                if (!token.IsCancellationRequested)
                {
                    mainView.Dispatcher.Invoke(new Action(() => mainView.pbLoading.Value = 100));
                    MessageBox.Show("Successfully!");
                }       
            }
            if (!string.IsNullOrEmpty(filePath) && isEncrypt == false)
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    Text.Clear();
                    while ((line = sr.ReadLine()) != null)
                    {
                        Text.Add(line);
                    }
                }
                File.WriteAllText(filePath, String.Empty);
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    foreach (var item in Text)
                    {
                        if (token.IsCancellationRequested) break;
                        Thread.Sleep(500);
                        mainView.Dispatcher.Invoke(new Action(() => mainView.pbLoading.Value += 100 / Text.Count));
                        var encrypt = XORCipher(item, passwordKey);
                        sw.WriteLine(encrypt);
                    }
                }
                if (!token.IsCancellationRequested)
                {
                    mainView.Dispatcher.Invoke(new Action(() => mainView.pbLoading.Value = 100));
                    MessageBox.Show("Successfully!");
                }
            }
        }
    }
}
