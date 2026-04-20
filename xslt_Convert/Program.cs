using System.Xml;
using System.Xml.Xsl;


namespace xslt_Convert
{
  internal class Program
  {
    static void Main(string[] args)
    {
      Convert();
    }

    /// <summary>
    /// // Метод для преобразования XML с помощью XSLT
    /// здесь предполагается что данные лежат в \bin\Debug\net8.0
    /// </summary>
    static void Convert()
    {
      var xslt = new XslCompiledTransform();

      try
      {
        // Проверяем, существует ли файл .xslt
        if (!File.Exists("transform.xslt"))
        {
          Console.WriteLine("Ошибка: Файл transform.xslt не найден.");
          return;
        }
        // Проверяем, существует ли файл XML для преобразования
        if (!File.Exists("Data2.xml"))
        {
          Console.WriteLine("Ошибка: Файл Data1.xml не найден.");
          return;
        }

        // (файл .xslt с правилами преобразования)
        xslt.Load("transform.xslt");

        // преобразование XML (Data2.xml) в новый XML (Employees.xml)
        xslt.Transform("Data1.xml", "Employees.xml");

        Console.WriteLine("Готово! Создан Employees.xml");

      }
      catch (Exception ex)
      {
        Console.WriteLine($"Произошла ошибка: {ex.Message}");
        return;
      }
    }
  }
}