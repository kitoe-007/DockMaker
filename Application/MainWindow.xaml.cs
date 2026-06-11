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
using Autoaccept_electrical_app.auxiliary_modules;

namespace Autoaccept_electrical_app
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Button_extract_data_from_file_Click(object sender, RoutedEventArgs e)
        {
            var extractor = new launch_and_data_extraction();
            string a = extractor.launch_script("C:\\Users\\user\\Desktop\\работа\\Заготовки\\извлечение PDF\\py\\.venv\\Scripts\\python.exe", "C:\\Users\\user\\Desktop\\работа\\Заготовки\\извлечение PDF\\py\\PDF_extractor.py", "C:\\Users\\user\\Desktop\\работа\\Заготовки\\извлечение PDF\\py\\files\\test.pdf");
            Console.WriteLine(a);
            lab.Content = a;
        }
    }
}
