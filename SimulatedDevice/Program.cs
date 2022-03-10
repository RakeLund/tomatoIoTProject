using System.Text;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;

namespace SimulatedDevice
{
    class Program
    {
        private static double currentTemperature = 20;
        private static double currentHumidity = 70;
        private static double currentLightIntensity = 1200;

        private static void Main(string[] args)
        {
            if (args == null)
            {
                Console.WriteLine("Please add the device connectionstring as a commandline argument");
                return;
            }
            string connectionstring = args[0];
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(connectionstring, TransportType.Mqtt);
            SendMessagesAsync(deviceClient);
            Console.ReadLine();
        }

        private static async void SendMessagesAsync(DeviceClient deviceClient)
        {
            while (true)
            {
                currentTemperature = GetNewValue(currentTemperature);
                currentHumidity = GetNewValue(currentHumidity);
                currentLightIntensity = GetNewValue(currentLightIntensity);

                var telemetry = new Telemetry { Temperature = currentTemperature, Humidity = currentHumidity, LightIntensity = currentLightIntensity };
                if (telemetry.Humidity > 100)
                {
                    telemetry.Humidity = 100;
                    currentHumidity = 99;
                }

                string stringMessage = JsonSerializer.Serialize<Telemetry>(telemetry);
                Console.WriteLine($"Sending message: {stringMessage}");
                Message message = new Message(Encoding.UTF8.GetBytes(stringMessage));
                await deviceClient.SendEventAsync(message);
                await Task.Delay(15000 * 60); // Every 15 minutes
            }
        }
        private static double GetNewValue(double CurrentValue)
        {
            Random random = new Random();
            var change = random.NextDouble() * CurrentValue * 0.05;
            if (random.NextDouble() > 0.5) change *= (-1);
            var result = CurrentValue + change;
            return Math.Round(result, 2);
        }
    }
}