using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WeatherForecast
{
	class Program
	{
		static void Main(string[] args)
		{
			var e1Task = StartE1Task("http://www.e1.ru");
			var gismeteoTask = StartGismeteoTask("http://www.gismeteo.ru");
            var weatherForecasters = new HashSet<Task<double>> { e1Task, gismeteoTask};

		    while (weatherForecasters.Count > 0)
		    {
		        var resultTask = Task.WhenAny(weatherForecasters).Result;
		        if (resultTask.IsFaulted)
		            weatherForecasters.Remove(resultTask);
		        else
		        {
                    Console.WriteLine(resultTask.Result);
                    return;
		        }
		    }
		    Console.WriteLine("Can't get weather! :(");
		}

		private static Task<double> StartE1Task(string url)
		{
			return Task.Run(() =>
			{
				using(var client = new WebClient())
				{
					var html = client.DownloadString(url);
					var match = Regex.Match(html, ">(.*?)&deg;C<", RegexOptions.None);
					var degreesStr = match.Groups[1].Value;
					return double.Parse(degreesStr.Replace(',','.'), CultureInfo.InvariantCulture);
				}
			});
		}

		private static Task<double> StartGismeteoTask(string baseUrl)
		{
			return Task.Run(() =>
			{
				using(var client = new WebClient())
				{
					var html = Encoding.UTF8.GetString(client.DownloadData(baseUrl));
					var match = Regex.Match(html, @"\t<a href=""(.*?)"">Екатеринбург</a>", RegexOptions.None);
					return match.Groups[1].Value;
				}
			}).ContinueWith(task =>
			{
				var href = baseUrl + task.Result;
				using(var client = new WebClient())
				{
					var html = Encoding.UTF8.GetString(client.DownloadData(href));
					var match = Regex.Match(html, @"<dd class='value m_temp c'>(.*?)<span", RegexOptions.None);
					var degreesStr = match.Groups[1].Value;
					degreesStr = degreesStr.Replace("&minus;", "-");
					return double.Parse(degreesStr, CultureInfo.InvariantCulture);
				}
			});
		}
	}
}
