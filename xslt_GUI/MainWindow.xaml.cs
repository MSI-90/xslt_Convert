using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace xslt_GUI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
      try
      {
        string inputFile = filePathTextBox.Text.Trim();

        if (!File.Exists(inputFile))
        {
          MessageBox.Show($"Файл не найден:\n{inputFile}", "Ошибка",
              MessageBoxButton.OK, MessageBoxImage.Warning);
          return;
        }

        if (!File.Exists("transform.xslt"))
        {
          MessageBox.Show("Файл transform.xslt не найден!", "Ошибка",
              MessageBoxButton.OK, MessageBoxImage.Warning);
          return;
        }

        // 1. XSLT
        var xslt = new XslCompiledTransform();
        xslt.Load("transform.xslt");
        xslt.Transform(inputFile, "Employees.xml");

        // 2. Employees.xml → сотрудники
        var doc = XDocument.Load("Employees.xml");
        var employees = doc.Descendants("Employee").ToList();
        var list = new List<EmployeeView>();

        var grouped = employees
            .GroupBy(emp => new
            {
              Name = emp.Attribute("name")?.Value,
              Surname = emp.Attribute("surname")?.Value
            })
            .ToList();

        foreach (var group in grouped)
        {
          var sum = group
              .SelectMany(emp => emp.Elements("amount"))
              .Sum(amount => decimal.Parse(
                  amount.Attribute("salary")?.Value.Replace(',', '.') ?? "0",
                  CultureInfo.InvariantCulture));

          group.First().SetAttributeValue("totalSalary",
              sum.ToString(CultureInfo.InvariantCulture));

          list.Add(new EmployeeView
          {
            Name = group.Key.Name,
            Surname = group.Key.Surname,
            Total = sum
          });
        }

        doc.Save("Employees.xml");

        // 3. Обновляем исходный файл
        var doc2 = XDocument.Load(inputFile);
        var payElement = doc2.Element("Pay");

        if (payElement != null)
        {
          var totalAmount = payElement
              .Descendants("item")
              .Sum(item => decimal.Parse(
                  item.Attribute("amount")?.Value.Replace(',', '.') ?? "0",
                  CultureInfo.InvariantCulture));

          payElement.SetAttributeValue("totalAmount",
              totalAmount.ToString(CultureInfo.InvariantCulture));
        }
        doc2.Save(inputFile);

        // 4. Вывод в ОДНУ таблицу
        grid.ItemsSource = list;

        MessageBox.Show($"✅ Готово!\nОбработан файл: {Path.GetFileName(inputFile)}", "Успех",
            MessageBoxButton.OK, MessageBoxImage.Information);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"❌ Ошибка:\n{ex.Message}", "Ошибка",
            MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      var openFileDialog = new OpenFileDialog
      {
        Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
        Title = "Выберите XML файл для обработки",
        InitialDirectory = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location)
      };

      if (openFileDialog.ShowDialog() == true)
      {
        filePathTextBox.Text = openFileDialog.FileName;
      }
    }
  }
}