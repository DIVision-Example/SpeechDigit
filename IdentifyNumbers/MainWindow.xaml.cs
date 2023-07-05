using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace IdentifyNumbers {
    public partial class MainWindow : System.Windows.Window {
        string[] label_name = new string[] {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11"};

        string image_path = "./img/";
        string onnx_path = "./digits.onnx";

        public MainWindow() {
            InitializeComponent();
        }

        private void FindFolder_Click(object sender, RoutedEventArgs e) {
            // 폴더 선택 대화 상자 생성
            var dialog = new FolderBrowserDialog();

            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                image_path = dialog.SelectedPath;
                FolderName.Text = dialog.SelectedPath;

                System.IO.DirectoryInfo DI = new System.IO.DirectoryInfo(image_path);

                if (System.IO.Directory.Exists(image_path)) {

                    FindFolder.IsEnabled = false;
                    Debug.WriteLine(image_path);

                    foreach(var items in DI.GetFiles()) {
                        NumOCR ocr = new NumOCR(image_path, items.Name, onnx_path, label_name);
                    }
                }
            }
        }
    }
}