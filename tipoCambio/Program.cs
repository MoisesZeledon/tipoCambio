using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tc_Servicio;

namespace tipoCambio
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var tCambio = new Tipo_Cambio_BCNSoapClient(Tipo_Cambio_BCNSoapClient.EndpointConfiguration.Tipo_Cambio_BCNSoap);
                List<Tasa> list = new List<Tasa>();

                DateTime fecRequerida = DateTime.Now;

                int anio = fecRequerida.Year;
                int mes = fecRequerida.Month;

                var tcMesResponse = await tCambio.RecuperaTC_MesAsync(anio, mes);
                var xmlElement = tcMesResponse.Body.RecuperaTC_MesResult;

                if (xmlElement == null)
                {
                    Console.WriteLine("No Data found");
                }
                else
                {
                    // Parse XML string with LINQ to XML
                    var xDoc = XDocument.Parse(xmlElement.OuterXml);
                    Console.WriteLine(xmlElement.OuterXml);

                    foreach (var tcElem in xDoc.Descendants("Tc"))
                    {
                        var fechaStr = tcElem.Element("Fecha")?.Value;
                        var valorStr = tcElem.Element("Valor")?.Value;

                        if (DateTime.TryParse(fechaStr, out DateTime fecha) &&
                            decimal.TryParse(valorStr, out decimal valor))
                        {
                            list.Add(new Tasa { Fecha = fecha, Valor = valor });
                        }
                    }

                    if (list.Any())
                    {
                        // File path for the CSV

                        // Get the current user's desktop path
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                        // Specify the file name
                        string fileName = "TC.csv";

                        // Combine desktop path and file name
                        string filePath = Path.Combine(desktopPath, fileName);

                        //string filePath = "TC.csv";

                        //Write CSV manually
                        using (var writer = new StreamWriter(filePath))
                        {
                            // Write the header
                            writer.WriteLine("Fecha,Valor");

                            foreach (var tasaCambiaria in list)
                            {
                                // Manually print the values of 'fecha' and 'valor' for each 'tasaCambiaria' object
                                writer.WriteLine($"{tasaCambiaria.Fecha.ToShortDateString()},{tasaCambiaria.Valor}");
                            }
                        }

                        Console.WriteLine($"Data exported successfully to {filePath}");
                    }
                    else
                    {
                        Console.WriteLine("There was an error exporting the data");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }

    public class Tasa
    {
        public DateTime Fecha { get; set; }
        public decimal Valor { get; set; }
    }
}
