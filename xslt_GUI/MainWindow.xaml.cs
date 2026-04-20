using System.Globalization;
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
        // 1. XSLT преобразование
        var xslt = new XslCompiledTransform();
        xslt.Load("transform.xslt");
        xslt.Transform("Data1.xml", "Employees.xml");

        // 2. Обрабатываем Employees.xml
        var doc = XDocument.Load("Employees.xml");
        var employees = doc.Descendants("Employee").ToList();

        var list = new List<EmployeeView>();

        // Группируем по имени+фамилии
        var grouped = employees
            .GroupBy(emp => new
            {
              Name = emp.Attribute("name")?.Value,
              Surname = emp.Attribute("surname")?.Value
            })
            .ToList();

        foreach (var group in grouped)
        {
          // 🔧 Суммируем salary из вложенных <amount>
          var sum = group
              .SelectMany(emp => emp.Elements("amount")) // берём все <amount> внутри сотрудника
              .Sum(amount => decimal.Parse(
                  amount.Attribute("salary")?.Value.Replace(',', '.') ?? "0",
                  CultureInfo.InvariantCulture));

          // Добавляем атрибут totalSalary
          group.First().SetAttributeValue("totalSalary", sum.ToString(CultureInfo.InvariantCulture));

          // Добавляем в список для UI
          list.Add(new EmployeeView
          {
            Name = group.Key.Name,
            Surname = group.Key.Surname,
            Total = sum
          });
        }

        doc.Save("Employees.xml");

        // 3. Обновляем Data1.xml (сумма всех amount в Pay)
        var doc2 = XDocument.Load("Data1.xml");
        var payElement = doc2.Element("Pay");

        if (payElement != null)
        {
          var totalAmount = payElement
              .Descendants("item")
              .Sum(item => decimal.Parse(
                  item.Attribute("amount")?.Value.Replace(',', '.') ?? "0",
                  CultureInfo.InvariantCulture));

          payElement.SetAttributeValue("totalAmount", totalAmount.ToString(CultureInfo.InvariantCulture));
        }
        doc2.Save("Data1.xml");

        // 4. Вывод в DataGrid
        grid.ItemsSource = list;

        MessageBox.Show("Готово!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }
}