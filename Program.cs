using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _24_10_UDP_MulticastGroup_Client
{
    internal class Program
    {
        private static List<MulticastGroups> multicastGroups = new List<MulticastGroups>();
     
        private static string msg_elektronic = "В категорию 'Электроника' добавлены новые товары:";
        private static string msg_clouses = "В категорию 'Одежда' добавлены новые товары:";
        private static string msg_products = "В категорию 'Продукты' добавлены новые товары:";

        private static string brodcastAddress_elektronic = "239.0.0.1";
        private static string brodcastAddress_clouses = "239.0.0.2";
        private static string brodcastAddress_products = "239.0.0.3";

        private static int port_elektronic = 8001;
        private static int port_clouses = 8002;
        private static int port_products = 8003;

        static async Task Main(string[] args)
        {
            await Menu();
        }

        private static async Task Menu()
        {
            while (true)
            {
                Console.WriteLine("Выберите категорию товара, на которую хотите подписаться:");
                Console.WriteLine("1 - Электроника ");
                Console.WriteLine("2 - Одежда");
                Console.WriteLine("3 - Продукты");

                int inputCategory;
                while (!Int32.TryParse(Console.ReadLine(), out inputCategory) || inputCategory < 1 || inputCategory > 3)
                {
                    Console.WriteLine("Не верный ввод.Введите число от 1 до 3:");
                    Console.Write("категория - ");
                }

                switch (inputCategory)
                {
                    case 0: break;
                    case 1:
                        multicastGroups.Add(new MulticastGroups(brodcastAddress_elektronic, port_elektronic));
                        break;
                    case 2:
                        multicastGroups.Add(new MulticastGroups(brodcastAddress_clouses, port_clouses));
                        break;
                    case 3:
                        multicastGroups.Add(new MulticastGroups(brodcastAddress_products, port_products));
                        break;
                }

                Console.WriteLine("Хотите еще подписаться на другую категорию?(Y/N)");

                string flag = Console.ReadLine().ToUpper();
                while (flag != "Y" && flag != "N")
                {
                    Console.WriteLine("Не верный ввод. Подписаться на другую категорию? (Y/N)");
                    flag = Console.ReadLine().ToUpper();
                }
                if (flag != "Y")
                {
                    break;
                }
            }
            if (multicastGroups.Count > 0)
            {
                Console.WriteLine("Получение рассылки активировано...");
                var tasks = new List<Task>();

                foreach (var group in multicastGroups)
                {
                    tasks.Add(Task.Run(() => ReceiveMessages(group.adress, group.port)));
                }



                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении сообщений: {ex.Message}");
                }

                Console.WriteLine("Все задачи завершены.");
            }
            else
            {
                Console.WriteLine("Вы не подписались на ни одну категорию.");
            }

        }

       
        private static async Task ReceiveMessages(string address,int port)
        {
            using var udpClient = new UdpClient(port);
            var multicastAddress = IPAddress.Parse(address);

            udpClient.JoinMulticastGroup(multicastAddress);
            Console.WriteLine($"Подписка активирована на {address}. Ожидание сообщений...");

            try
            {
                while (true)
                {
                    var result = await udpClient.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    Console.WriteLine($"Получено сообщение: {message}");
                     await Task.Delay(100);
               }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка сокета: {ex.Message}");
            }
            finally
            {
                udpClient.DropMulticastGroup(multicastAddress);
                Console.WriteLine($"Подписка на {address} завершена.");
            }

        }


        record MulticastGroups(string adress, int port);
    }
}
