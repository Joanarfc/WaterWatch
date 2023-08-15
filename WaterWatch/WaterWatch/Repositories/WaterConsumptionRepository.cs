using Microsoft.EntityFrameworkCore;
using WaterWatch.Data;
using WaterWatch.Models;
using Newtonsoft.Json;

namespace WaterWatch.Repositories
{
    public class WaterConsumptionRepository : IWaterConsumptionRepository
    {
        private readonly IDataContext _context;
        private readonly IWebHostEnvironment _webHostEnv;

        public WaterConsumptionRepository(IDataContext context, IWebHostEnvironment webHostEnv)
        {
            _context = context;
            _webHostEnv = webHostEnv;
        }

        public async Task<IEnumerable<WaterConsumption>> GetAll()
        {
            SaveData();
            return await _context.Consumptions.ToListAsync();
        }

        public async Task<IEnumerable<WaterConsumption>> GetTopTenConsumers()
        {
            var q = _context.Consumptions
            .OrderByDescending(avgKL => avgKL.averageMonthlyKL)
            .Take(10)
            .ToListAsync();

            return await q;
        }

        public void SaveData()
        {
            // Check if the table is empty before we load the data, else skip the extract transform and load process
            var res_dataset = _context.Consumptions.ToList();

            if (res_dataset.Count() == 0)
            {
                Console.WriteLine("No data");

                var wwwrootPath = _webHostEnv.WebRootPath;

                var filePath = Path.Combine(wwwrootPath, "files", "water_consumption.geojson");

                if (File.Exists(filePath))
                {
                    var geoJSON = File.ReadAllText(filePath);
                    dynamic jsonObj = JsonConvert.DeserializeObject(geoJSON);

                    foreach (var feature in jsonObj["features"])
                    {
                        // Extract values from the file object using the fields
                        string str_neighborhood = feature["properties"]["neighbourhood"];
                        string str_suburb_group = feature["properties"]["suburb_group"];
                        string str_avgMonthlyKL = feature["properties"]["averageMonthlyKL"];
                        string str_geometry = feature["geometry"]["coordinates"].ToString(Newtonsoft.Json.Formatting.None);

                        // Apply transformations

                        // Remove .0's from the values
                        string conv_avgMonthlyKL = str_avgMonthlyKL.Replace(".0", "");

                        // Convert string to an int
                        int avgMonthlyKl = Convert.ToInt32(conv_avgMonthlyKL);

                        // Load the data into our table
                        WaterConsumption wc = new()
                        {
                            neighborhood = str_neighborhood,
                            suburb_group = str_suburb_group,
                            averageMonthlyKL = avgMonthlyKl,
                            coordinates = str_geometry
                        };

                        _context.Consumptions.Add(wc);
                        _context.SaveChanges();
                    }
                }
                else
                {
                Console.WriteLine("The file does not exist.");
                }
            }
            else
            {
                Console.WriteLine("Data loaded.");
            }
        }
    }
}