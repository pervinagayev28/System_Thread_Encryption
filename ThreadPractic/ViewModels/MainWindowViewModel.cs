using ChatAppService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Whatsapp.Commands;

namespace ThreadPractic.ViewModels
{
    public class MainWindowViewModel : ServiceINotifyPropertyChanged
    {
        private string filePath;
        private long maximum;
        private long currentValue;
        private long precentage;
        public ICommand LoadCommand { get; set; }
        public ICommand CopyCommand { get; set; }
    
        public string FilePath { get => filePath; set { filePath = value; OnPropertyChanged(); } }
        public long Maximum { get => maximum; set { maximum = value; OnPropertyChanged(); } }
        public long CurrentValue { get => currentValue; set { currentValue = value; OnPropertyChanged(); } }
        public long Precentage { get => precentage; set { precentage = value; OnPropertyChanged(); } }
   
        public MainWindowViewModel()
        {
            LoadCommand = new Command(ExecuteLoadFromCommand);
            CopyCommand = new Command(ExecuteCopyCommand, CanExecuteCopyCommand);
         
        }

   
     
      
        private bool CanExecuteCopyCommand(object obj) =>
            !string.IsNullOrEmpty(FilePath)
            && ((((RadioButton)((StackPanel)obj).FindName("E")).IsChecked == true
            || (((RadioButton)((StackPanel)obj).FindName("D")).IsChecked == true)));

        private void ExecuteCopyCommand(object obj)
        {
           
            if (((RadioButton)((StackPanel)obj).FindName("E")).IsChecked == true)
                Encryption(FilePath);
            else
                Decryption(FilePath);



        }

        private void ExecuteLoadFromCommand(object obj) =>
            FilePath = LoadImage()!;


        private string? LoadImage()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
                return fileDialog.FileName;
            return null;
        }

        private  void Encryption(string input)
        {

            using (FileStream fsInput = new FileStream(input, FileMode.Open))
            {
                using (FileStream fsOutput = new FileStream("temp.txt", FileMode.Create))
                {
                    using (AesManaged aes = new AesManaged())
                    {
                        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                        aes.Key = Convert.FromBase64String(config?.GetSection("Keys")["Key"]!);
                        aes.IV = Convert.FromBase64String(config?.GetSection("Keys")["Iv"]!);

                        ICryptoTransform encryptor = aes.CreateEncryptor();
                        using (CryptoStream cs = new CryptoStream(fsOutput, encryptor, CryptoStreamMode.Write))
                            fsInput.CopyTo(cs);
                    }
                }
            }

            CopyFile(input);


    }

        private async void Decryption(string input)
        {
            
                using (FileStream fsInput = new FileStream(input, FileMode.Open))
                {
                    using (FileStream fsOutput = new FileStream("temp.txt", FileMode.Create))
                    {
                        using (AesManaged aes = new AesManaged())
                        {
                            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                            aes.Key = Convert.FromBase64String(config?.GetSection("Keys")["Key"]!);
                            aes.IV = Convert.FromBase64String(config?.GetSection("Keys")["Iv"]!);

                            ICryptoTransform decryptor = aes.CreateDecryptor();
                            using (CryptoStream cs = new CryptoStream(fsOutput, decryptor, CryptoStreamMode.Write))
                            {
                                fsInput.CopyTo(cs);
                            }
                        }
                    }
                }
                CopyFile(input);
        }
        private void CopyFile(string input)
        {
            new Thread(() =>
            {

                using (FileStream stream = new("temp.txt", FileMode.Open, FileAccess.Read))
                {
                    using (FileStream writeStream = new(input, FileMode.Open, FileAccess.Write))
                    {
                        writeStream.SetLength(0);
                        var length = stream.Length;
                        Maximum = stream.Length;
                        var readTemp = 1;
                        var buffer = new byte[readTemp];
                        CurrentValue = 0;

                        do
                        {
                            Thread.Sleep(100);
                            var data = stream.Read(buffer, 0, readTemp);
                            writeStream.Write(buffer, 0, readTemp);
                            length -= readTemp;
                            CurrentValue += readTemp; ;
                            Precentage = CurrentValue * 100 / Maximum;
                        } while (length > 0);
                    }
                };
                File.Delete("temp.txt");
            }).Start();
           
        }
    }
}
