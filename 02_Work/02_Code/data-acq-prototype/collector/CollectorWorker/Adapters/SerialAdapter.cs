// 简单示例：可用 System.IO.Ports.SerialPort 实现
using System.IO.Ports;

public class SerialAdapter
{
    private SerialPort _port;
    public SerialAdapter(string portName, int baudRate = 9600)
    {
        _port = new SerialPort(portName, baudRate);
        _port.DataReceived += OnDataReceived;
        _port.Open();
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var sp = (SerialPort)sender;
        var data = sp.ReadExisting();
        // 解析并上报
    }
}
