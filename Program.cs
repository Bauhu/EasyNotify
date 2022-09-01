// Use this code inside a project created with the Visual C# > Windows Desktop > Console Application template.
// Replace the code in Program.cs with this code.


using System;

using System.IO.Ports;
using System.Globalization;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System.Data;

public class EasyNotify
{

    static SerialPort _port;
    static bool _isOpen;
    static bool _notify=true;
    static KeyCode _lastKeyCode;
    static uint _lastAddress;
    static KeyCode NewKeyCode;
    static uint NewAddress;
    static TimeSpan t;
    static int _lastTime;
    static TimeSpan nt;
    static int NewTime;

    public static void Main()
    {
       
        _port = new SerialPort("COM3", 57600, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.None,
            DtrEnable = true,
            RtsEnable = true,
            NewLine = "\r"
        };
        _port.Open();
        //_port.WriteLine("GETP?\rID?");
        _isOpen = true;

        while (_isOpen)
        {
            //Console.WriteLine(ProcessLine(_port.ReadLine()));

            var line = _port.ReadLine();
            var parts = line.Split(',', '\t');
            if (parts.Length == 0)
            {

            }

            switch (parts[0])
            {

                case "REC":
                    nt = DateTime.Now - new DateTime(1970, 1, 1);
                    NewTime = (int)nt.TotalSeconds;
                    NewAddress = uint.Parse(parts[1], NumberStyles.HexNumber);
                    NewKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), parts[2]);
                    var timeSpan = NewTime - _lastTime;
                    if (_lastKeyCode == NewKeyCode && _lastAddress == NewAddress && timeSpan<3)
                    {
                        //Console.WriteLine($"Address: {NewAddress}, Address: {NewKeyCode}");

                    }
                    else
                    {
                        _lastKeyCode=NewKeyCode;
                        _lastAddress=NewAddress;
                        t = DateTime.Now - new DateTime(1970, 1, 1);
                        _lastTime = (int)t.TotalSeconds;
                         Console.WriteLine($"Address: {NewAddress}, Address: {NewKeyCode}");
                        Notify(NewAddress, NewKeyCode);

                    }
                    break;
                case "OK":
                    break;
                default:
                    Console.WriteLine($"Unexpected input: {line}");
                    break;
            }

        }
       
    }
    public static void Notify(uint Addr, KeyCode Key)
    {

        using (StreamReader r = new StreamReader("config.json"))
        {
            string json = r.ReadToEnd();


            DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(json);
            DataTable dataTable = dataSet.Tables["devices"];



            foreach (DataRow row in dataTable.Rows)
            {
                var found = false;
                if(System.Convert.ToInt32(row["address"]) == Addr && System.Convert.ToString(row["key"]) == System.Convert.ToString(Key))
                {
                    found = true;
                    Console.WriteLine(row["address"] + " - " + row["name"] + " - " + row["event"] + " - " + System.Convert.ToInt32(row["address"]));
                }
                if (found)
                {
                    if (_notify)
                    {
                        new ToastContentBuilder()

                          
                        
                            .AddText(System.Convert.ToString(row["name"]))
                            .AddText(System.Convert.ToString(row["event"])).Show();

                    }
                }
                
            }
        }


      


    }


    public enum KeyCode
    {
        A = 1,
        B,
        C,
        D
    }
}