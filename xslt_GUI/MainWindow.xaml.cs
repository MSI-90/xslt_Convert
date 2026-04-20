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

        // 2. Сотрудники
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
            .SelectMany(emp => emp.Elements("salary")) 
            .Sum(sal => decimal.Parse(
              sal.Attribute("amount")?.Value.Replace(',', '.') ?? "0",
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

        // 3. Исходный файл
        var doc2 = XDocument.Load(inputFile);
        var pay = doc2.Element("Pay");
        if (pay != null)
        {
          var total = pay.Descendants("item")
              .Sum(i => decimal.Parse(
                  i.Attribute("amount")?.Value.Replace(',', '.') ?? "0",
                  CultureInfo.InvariantCulture));
          pay.SetAttributeValue("totalAmount",
              total.ToString(CultureInfo.InvariantCulture));
        }
        doc2.Save(inputFile);

        // 4. Вывод: сотрудники
        grid.ItemsSource = list;

        // Вывод: по месяцам (в TextBlock)
        var byMonth = doc.Descendants("salary")
          .GroupBy(s => s.Attribute("mount")?.Value)
          .Select(g => new {
            Month = g.Key,
            Total = g.Sum(s => decimal.Parse(
              s.Attribute("amount")?.Value.Replace(',', '.') ?? "0",
              CultureInfo.InvariantCulture))
          })
          .OrderBy(x => x.Month)
          .ToList();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Выплаты по месяцам:");
        foreach (var m in byMonth)
        {
          sb.AppendLine($"• {m.Month}: {m.Total:N2}");
        }
        monthlyTotals.Text = sb.ToString();

        MessageBox.Show($"Готово!\nФайл: {Path.GetFileName(inputFile)}", "Успех",
            MessageBoxButton.OK, MessageBoxImage.Information);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Ошибка:\n{ex.Message}", "Ошибка",
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