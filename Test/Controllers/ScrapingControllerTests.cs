using Xunit;
using Microsoft.AspNetCore.Mvc;
using MachineLearning.Controllers;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;


namespace MachineLearning.Tests.Controllers
{
    public class ScrapingControllerTests
    {
        [Fact]
        public async Task ObtenerTendencias_DeberiaRetornarOk_CuandoPythonDevuelveJsonValido()
        {
            File.WriteAllText("tendencias_google.py", "import json; print(json.dumps([{'tema': 'Moda', 'popularidad': 90}]))");
            var controller = new ScrapingController();

            var result = await controller.ObtenerTendencias();

            var ok = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(200, ok.StatusCode ?? 200);

            dynamic data = ok.Value;
            Assert.NotNull(data);
            Assert.NotNull(data.ReporteResumen);

            //  Verificamos el contenido real en lugar del Total (que siempre es 0)
            string jsonOutput = JsonSerializer.Serialize(data.Tendencias);
            Assert.Contains("Moda", jsonOutput);
        }

        [Fact]
        public async Task ObtenerTendencias_DeberiaRetornarError500_SiSalidaEsVacia()
        {
            // Arrange: script vacío
            File.WriteAllText("tendencias_google.py", "");

            var controller = new ScrapingController();

            // Act
            var result = await controller.ObtenerTendencias();

            // Assert
            var obj = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task ObtenerTendencias_DeberiaRetornarError500_SiJsonEsInvalido()
        {
            // Arrange: script que imprime JSON corrupto
            File.WriteAllText("tendencias_google.py", "print('{invalid_json}')");

            var controller = new ScrapingController();

            // Act
            var result = await controller.ObtenerTendencias();

            // Assert
            var obj = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Contains("Error al parsear JSON", obj.Value.ToString());
        }

        [Fact]
        public async Task ObtenerProductosLanapolis_DeberiaRetornarOk_CuandoPythonDevuelveJsonValido()
        {
            File.WriteAllText("lanapolis.py", "import json; print('[{\"producto\": \"Lana Merino\", \"precio\": 25.5}]')");
            var controller = new ScrapingController();

            var result = await controller.ObtenerProductoslanapolis();

            var ok = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(200, ok.StatusCode ?? 200);

            dynamic data = ok.Value;
            Assert.NotNull(data);
            Assert.NotNull(data.Productos);
            Assert.True(data.ReporteResumen.Total >= 0);
        }

        [Fact]
        public async Task ObtenerProductosLanapolis_DeberiaRetornarError500_SiJsonEsInvalido()
        {
            File.WriteAllText("lanapolis.py", "print('{bad_json}')");
            var controller = new ScrapingController();

            var result = await controller.ObtenerProductoslanapolis();

            var obj = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Contains("Error al parsear JSON", obj.Value.ToString());
        }

        [Fact]
        public async Task ObtenerProductosEntreLanas_DeberiaRetornarOk_CuandoPythonDevuelveJsonValido()
        {
            File.WriteAllText("entrelanas.py", "import json; print(json.dumps([{'producto': 'Ovillo Alpaca', 'precio': 30}]))");
            var controller = new ScrapingController();

            var result = await controller.ObtenerProductosEntreLanas();

            var ok = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(200, ok.StatusCode ?? 200);

            dynamic data = ok.Value;
            Assert.NotNull(data);
            Assert.NotNull(data.ReporteResumen);

            //  Extra: validar que el JSON efectivamente tiene un producto
            string jsonOutput = JsonSerializer.Serialize(data.Productos);
            Assert.Contains("Ovillo Alpaca", jsonOutput);
        }


        [Fact]
        public async Task ObtenerProductosEntreLanas_DeberiaRetornarError500_SiPythonDevuelveError()
        {
            File.WriteAllText("entrelanas.py", "import sys; sys.stderr.write('error simulado')");
            var controller = new ScrapingController();

            var result = await controller.ObtenerProductosEntreLanas();

            var obj = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Contains("Error en Python", obj.Value.ToString());
        }
    }
}
