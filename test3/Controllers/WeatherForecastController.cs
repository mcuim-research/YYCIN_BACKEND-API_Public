using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
namespace test3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {


        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> GetCurrentWeather()
        {
            throw new NotImplementedException();
        }
    }
    public class MyClass
    {

    }
}