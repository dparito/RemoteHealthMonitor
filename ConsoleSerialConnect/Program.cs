using System.IO.Ports;

internal class Program
{
    private static void Main(string[] args)
    {
        using (SerialPort serialPort2 = new SerialPort(portName:"COM21", baudRate:115200))
        {
            serialPort2.DataReceived += (sender, args) =>
            {
                Console.WriteLine("COM2 Received: " + serialPort2.ReadLine());
            };

            serialPort2.Open();

            serialPort2.WriteLine("Hello, COM2!");

            Thread.Sleep(200);
        }
    }
}