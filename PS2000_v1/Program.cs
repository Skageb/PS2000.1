using System;
using System.Collections;
using System.IO.Ports;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace PS2000_v1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            //Application.Run(new Form1());

            main_form my_form = new main_form();
            Application.Run(my_form);
        }
    }

    internal class main_form : Form
    {
       
        private int staticLabelCount = 0;

        private PS2000 powersupply;

        private Label com_type_lbl;
        private Label current_voltage_label;

        private TextBox setVoltageInput;

        public main_form() // Constructor
        {
            powersupply = new PS2000(this);
            //powersupply.test2();
            // Set the size of the window
            this.Width = 1344; // Set the width
            this.Height = 756; // Set the height
            this.Text = "GUI for PS2000";


            com_type_lbl = this.add_label(0, 0, "Default device type set to Com3", new_label: com_type_lbl);
            current_voltage_label = this.add_label(1.5, 0, "", 200, current_voltage_label);

            //Change Com type button
            Button change_com_type_button = this.add_button(0, 1, "Change Com Type", change_com_type_bytton_Click);
            // Display voltage button
            Button displayVoltageBtn = this.add_button(1, 0, "Display Voltage", displayVoltageBtn_Click, 150);
            //Set voltage button
            Button setVoltageButton = this.add_button(1.33, 1, "Set Voltage", setVoltageButton_Click, 100);

            // Set voltage input field
            setVoltageInput = this.add_textBox(1, 1, "", 100,new_textBox: setVoltageInput);
        }

        private Label add_label(double col, int row, string text, int width = 200, Label new_label = null)
        {
            if (new_label is null){
                new_label = new Label();
            }
            new_label.Text = text;
            int x_coordinate;
            if (col == 0){
                x_coordinate = 10;
            }
            else
            {
                x_coordinate = (int)(col * 300.0);
            }
            new_label.Location = new Point(x_coordinate, 10 + row * 30);
            new_label.Width = width;
            this.Controls.Add(new_label);
            return new_label;
        }

        private Button add_button(double col, int row, string text, EventHandler on_click_func,int width = 200)
        {
            Button new_button = new Button();
            new_button.Text = text;
            int x_coordinate;
            if (col == 0)
            {
                x_coordinate = 10;
            }
            else
            {
                x_coordinate = (int)(col * 300.0);
            }
            new_button.Location = new Point(x_coordinate, 10 + row * 30);
            new_button.Width = width;
            new_button.Click += on_click_func;
            this.Controls.Add(new_button);
            return new_button;
        }

        private TextBox add_textBox(int col, int row, string text, int width = 200, TextBox new_textBox = null)
        {
            if (new_textBox is null)
            {
                new_textBox = new TextBox();
            }
            new_textBox.Text = text;
            int x_coordinate;
                if (col == 0){
                    x_coordinate = 10;
                }
                else
                {
                    x_coordinate = col* 300;
                }
            new_textBox.Location = new Point(x_coordinate, 10 + row * 30);
            new_textBox.Width = width;
            this.Controls.Add(new_textBox);
            return new_textBox;
        }

        private void setVoltageButton_Click(object sender, EventArgs e)
        {
            string inputValue = setVoltageInput.Text;
            if(float.TryParse(inputValue, out float voltageValue))
            {
                powersupply.SetVoltage(voltageValue);
                MessageBox.Show(string.Format("Voltage was set to {0}V", voltageValue));

            }
            else
            {
                MessageBox.Show("Please enter a valid voltage value!");
            }

        }



        private void displayVoltageBtn_Click(object sender, EventArgs e)
            {
                current_voltage_label.Text = string.Format("Current Voltage: {0}V", powersupply.GetVoltage());
            }

        private void change_com_type_bytton_Click(object sender, EventArgs e)
            {
                // This method will be called when the button is clicked
                if (powersupply.com_type == "Com4")
                {
                    powersupply.com_type = "Com3";
                    this.com_type_lbl.Text = "Com type set to Com3";
                }
                else
                {
                    powersupply.com_type = "Com4";
                    this.com_type_lbl.Text = "Com type set to Com4";
                }
            }

        private void AddStaticLabelOnStart(string labelText)
        {
            Label newLabel = new Label();
            newLabel.Text = labelText;

            // Determine position
            int baseTop = 10;  // Start position for the first label
            int spacing = 25;  // Vertical space between labels
            newLabel.Width = 380;
            newLabel.Top = baseTop + (staticLabelCount * spacing);

            this.Controls.Add(newLabel);
            staticLabelCount++;
        }

        private class PS2000
        {
            private main_form _form;
            public string com_type;

            public PS2000(main_form form) {
                _form = form;
                com_type = "Com3";
            }

            //Gets current voltage:
            public double GetVoltage()
            {
                double volt;
                int percentVolt = 0;

                // get voltage

                //SD = MessageType + CastType + Direction + Length
                int SDHex = (int)0x40 + (int)0x20 + 0x10 + 5; //6-1 ref spec 3.1.1
                byte SD = Convert.ToByte(SDHex.ToString(), 10);

                //SD, DN, OBJ, DATA, CS
                byte[] byteWithOutCheckSum = { SD, (int)0x00, (int)0x47, 0x0, 0x0 }; // quert status

                int sum = 0;
                int arrayLength = byteWithOutCheckSum.Length;
                for (int i = 0; i < arrayLength; i++)
                {
                    sum += byteWithOutCheckSum[i];
                }

                string hexSum = sum.ToString("X");
                string cs1 = "";
                string cs2 = "";
                if (hexSum.Length == 4)
                {
                    cs1 = hexSum.Substring(0, hexSum.Length / 2);
                    cs2 = hexSum.Substring(hexSum.Length / 2);
                }
                else if (hexSum.Length == 3)
                {
                    cs1 = hexSum.Substring(0, 1);
                    cs2 = hexSum.Substring(1);
                }
                else if ((hexSum.Length is 2) || (hexSum.Length is 1))
                {
                    cs1 = "0";
                    cs2 = hexSum;
                }

                if (cs1 != "")
                {


                    byteWithOutCheckSum[arrayLength - 2] = Convert.ToByte(cs1, 16);
                    byteWithOutCheckSum[arrayLength - 1] = Convert.ToByte(cs2, 16);
                }

                // now the byte array is ready to be sent

                List<byte> responseTelegram;
                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    // write to the USB port
                    port.Write(byteWithOutCheckSum, 0, byteWithOutCheckSum.Length);
                    Thread.Sleep(500);

                    responseTelegram = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            //Console.WriteLine(t);
                            responseTelegram.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                }

                if (responseTelegram == null)
                {
                    _form.AddStaticLabelOnStart("No telegram was read");
                }
                else
                {

                    string percentVoltString = responseTelegram[5].ToString("X") + responseTelegram[6].ToString("X");
                    percentVolt = Convert.ToInt32(percentVoltString, 16);


                }

                float nominalVoltage = 0;

                // get nominal voltage
                List<byte> response;
                byte[] bytesToSend = { 0x74, 0x00, 0x02, 0x00, 0x76 };

                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    port.Write(bytesToSend, 0, bytesToSend.Length);
                    Thread.Sleep(50);
                    response = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            response.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                }
                if (response == null)
                {
                    Console.WriteLine("No telegram was read");
                    return -1;
                }
                else
                {
                    byte[] byteArray = { response[6], response[5], response[4], response[3] };
                    nominalVoltage = BitConverter.ToSingle(byteArray, 0);
                    volt = (double)percentVolt * nominalVoltage / 25600;
                    Console.WriteLine(string.Format("Voltage:{0}", volt));
                    return volt;
                }
                
            }


            public void SetVoltage(float setVolt)
            {

                double volt;
                int percentVolt = 0;

                // get voltage

                //SD = MessageType + CastType + Direction + Length
                int SDHex = (int)0x40 + (int)0x20 + 0x10 + 5; //6-1 ref spec 3.1.1
                byte SD = Convert.ToByte(SDHex.ToString(), 10);

                //SD, DN, OBJ, DATA, CS
                byte[] byteWithOutCheckSum = { SD, (int)0x00, (int)0x47, 0x0, 0x0 }; // quert status

                int sum = 0;
                int arrayLength = byteWithOutCheckSum.Length;
                for (int i = 0; i < arrayLength; i++)
                {
                    sum += byteWithOutCheckSum[i];
                }

                string hexSum = sum.ToString("X");
                string cs1 = "";
                string cs2 = "";
                if (hexSum.Length == 4)
                {
                    cs1 = hexSum.Substring(0, hexSum.Length / 2);
                    cs2 = hexSum.Substring(hexSum.Length / 2);
                }
                else if (hexSum.Length == 3)
                {
                    cs1 = hexSum.Substring(0, 1);
                    cs2 = hexSum.Substring(1);
                }
                else if ((hexSum.Length is 2) || (hexSum.Length is 1))
                {
                    cs1 = "0";
                    cs2 = hexSum;
                }

                if (cs1 != "")
                {


                    byteWithOutCheckSum[arrayLength - 2] = Convert.ToByte(cs1, 16);
                    byteWithOutCheckSum[arrayLength - 1] = Convert.ToByte(cs2, 16);
                }

                // now the byte array is ready to be sent

                List<byte> responseTelegram;
                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    // write to the USB port
                    port.Write(byteWithOutCheckSum, 0, byteWithOutCheckSum.Length);
                    Thread.Sleep(500);

                    responseTelegram = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            //Console.WriteLine(t);
                            responseTelegram.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                }

                if (responseTelegram == null)
                {
                    _form.AddStaticLabelOnStart("No telegram was read");
                }
                else
                {

                    string percentVoltString = responseTelegram[5].ToString("X") + responseTelegram[6].ToString("X");
                    percentVolt = Convert.ToInt32(percentVoltString, 16);


                }

                float nominalVoltage = 0;

                // get nominal voltage
                List<byte> response;
                byte[] bytesToSend = { 0x74, 0x00, 0x02, 0x00, 0x76 };

                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    port.Write(bytesToSend, 0, bytesToSend.Length);
                    Thread.Sleep(50);
                    response = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            response.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                }
                if (response == null)
                {
                    Console.WriteLine("No telegram was read");
                }
                else
                {
                    byte[] byteArray = { response[6], response[5], response[4], response[3] };
                    nominalVoltage = BitConverter.ToSingle(byteArray, 0);
                    volt = (double)percentVolt * nominalVoltage / 25600;
                    Console.WriteLine(string.Format("Voltage:{0}", volt));
                }

                // setting voltage, 30V

                // remember to turn on remote control first


                // Remember the dataframe setup, SD, DN,   OBJ, DATA [hex1, hex2] checksum1, checksum2
                //OBJ 0x36 = 54

                byte[] bytesToSendToTurnOnRC = new byte[] { 0xF1, 0x00, 0x36, 0x10, 0x10, 0x01, 0x47 }; // Turn on remote control
                List<byte> RCresponse;
                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    port.Write(bytesToSendToTurnOnRC, 0, bytesToSendToTurnOnRC.Length);
                    Thread.Sleep(50);
                    RCresponse = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            RCresponse.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                    if (RCresponse[3] == 0)
                    {
                        Console.WriteLine("Remote Control is turned on");
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Remote control is not turned on due to error: {0}", RCresponse[3].ToString()));
                    }
                }

                // 
                int percentSetValue = (int)Math.Round((25600 * setVolt) / 84);

                string hexValue = percentSetValue.ToString("X");
                string hexValue1 = "";
                string hexValue2 = "";

                if (hexValue.Length == 4)
                {
                    hexValue1 = hexValue.Substring(0, hexValue.Length / 2);
                    hexValue2 = hexValue.Substring(hexValue.Length / 2);
                }
                else if (hexValue.Length == 3)
                {
                    hexValue1 = hexValue.Substring(0, 1);
                    hexValue2 = hexValue.Substring(1);
                }
                else if ((hexValue.Length is 2) || (hexValue.Length is 1))
                {
                    hexValue1 = "0";
                    hexValue2 = hexValue;
                }
                byte[] newbytesWithoutChecksum = { 0xF2, 0x00, 0x32, Convert.ToByte(hexValue1, 16), Convert.ToByte(hexValue2, 16), 0x0, 0x0 };

                int newsum = 0;
                int newarrayLength = newbytesWithoutChecksum.Length;
                for (int i = 0; i < newarrayLength; i++)
                {
                    newsum += newbytesWithoutChecksum[i];
                }

                string newhexSum = newsum.ToString("X");
                string newcs1 = "";
                string newcs2 = "";
                if (hexSum.Length == 4)
                {
                    newcs1 = newhexSum.Substring(0, newhexSum.Length / 2);
                    newcs2 = newhexSum.Substring(newhexSum.Length / 2);
                }
                else if (newhexSum.Length == 3)
                {
                    newcs1 = newhexSum.Substring(0, 1);
                    newcs2 = newhexSum.Substring(1);
                }
                else if ((newhexSum.Length is 2) || (newhexSum.Length is 1))
                {
                    newcs1 = "0";
                    newcs2 = newhexSum;
                }

                if (newcs1 != "")
                {


                    newbytesWithoutChecksum[newarrayLength - 2] = Convert.ToByte(newcs1, 16);
                    newbytesWithoutChecksum[newarrayLength - 1] = Convert.ToByte(newcs2, 16);
                }

                List<byte> newResponseTelegram;
                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    // write to the USB port
                    port.Write(newbytesWithoutChecksum, 0, newbytesWithoutChecksum.Length);
                    Thread.Sleep(500);

                    newResponseTelegram = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            //Console.WriteLine(t);
                            newResponseTelegram.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                }
                if (newResponseTelegram[3] == 0)
                {
                    Console.WriteLine("New voltage was set");
                }
                else
                {
                    Console.WriteLine(newResponseTelegram[3].ToString());
                }
            }

                public void test2()
            {

                double volt;
                int percentVolt = 0;

                // get voltage

                //SD = MessageType + CastType + Direction + Length
                int SDHex = (int)0x40 + (int)0x20 + 0x10 + 5; //6-1 ref spec 3.1.1
                byte SD = Convert.ToByte(SDHex.ToString(), 10);

                //SD, DN, OBJ, DATA, CS
                byte[] byteWithOutCheckSum = { SD, (int)0x00, (int)0x47, 0x0, 0x0 }; // quert status

                int sum = 0;
                int arrayLength = byteWithOutCheckSum.Length;
                for (int i = 0; i < arrayLength; i++)
                {
                    sum += byteWithOutCheckSum[i];
                }

                string hexSum = sum.ToString("X");
                string cs1 = "";
                string cs2 = "";
                if (hexSum.Length == 4)
                {
                    cs1 = hexSum.Substring(0, hexSum.Length / 2);
                    cs2 = hexSum.Substring(hexSum.Length / 2);
                }
                else if (hexSum.Length == 3)
                {
                    cs1 = hexSum.Substring(0, 1);
                    cs2 = hexSum.Substring(1);
                }
                else if ((hexSum.Length is 2) || (hexSum.Length is 1))
                {
                    cs1 = "0";
                    cs2 = hexSum;
                }

                if (cs1 != "")
                {


                    byteWithOutCheckSum[arrayLength - 2] = Convert.ToByte(cs1, 16);
                    byteWithOutCheckSum[arrayLength - 1] = Convert.ToByte(cs2, 16);
                }

                // now the byte array is ready to be sent

                List<byte> responseTelegram;
                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    // write to the USB port
                    port.Write(byteWithOutCheckSum, 0, byteWithOutCheckSum.Length);
                    Thread.Sleep(500);

                    responseTelegram = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            //Console.WriteLine(t);
                            responseTelegram.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                }

                if (responseTelegram == null)
                {
                    _form.AddStaticLabelOnStart("No telegram was read");
                }
                else
                {

                    string percentVoltString = responseTelegram[5].ToString("X") + responseTelegram[6].ToString("X");
                    percentVolt = Convert.ToInt32(percentVoltString, 16);


                }

                float nominalVoltage = 0;

                // get nominal voltage
                List<byte> response;
                byte[] bytesToSend = { 0x74, 0x00, 0x02, 0x00, 0x76 };

                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    port.Write(bytesToSend, 0, bytesToSend.Length);
                    Thread.Sleep(50);
                    response = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            response.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                }
                if (response == null)
                {
                    Console.WriteLine("No telegram was read");
                }
                else
                {
                    byte[] byteArray = { response[6], response[5], response[4], response[3] };
                    nominalVoltage = BitConverter.ToSingle(byteArray, 0);
                    volt = (double)percentVolt * nominalVoltage / 25600;
                    Console.WriteLine(string.Format("Voltage:{0}", volt));
                }

                // setting voltage, 30V

                // remember to turn on remote control first


                // Remember the dataframe setup, SD, DN,   OBJ, DATA [hex1, hex2] checksum1, checksum2
                //OBJ 0x36 = 54

                byte[] bytesToSendToTurnOnRC = new byte[] { 0xF1, 0x00, 0x36, 0x10, 0x10, 0x01, 0x47 }; // Turn on remote control
                List<byte> RCresponse;
                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    port.Write(bytesToSendToTurnOnRC, 0, bytesToSendToTurnOnRC.Length);
                    Thread.Sleep(50);
                    RCresponse = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            RCresponse.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                    if (RCresponse[3] == 0)
                    {
                        Console.WriteLine("Remote Control is turned on");
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Remote control is not turned on due to error: {0}", RCresponse[3].ToString()));
                    }
                }

                // 
                float setVolt = 60;
                int percentSetValue = (int)Math.Round((25600 * setVolt) / 84);

                string hexValue = percentSetValue.ToString("X");
                string hexValue1 = "";
                string hexValue2 = "";

                if (hexValue.Length == 4)
                {
                    hexValue1 = hexValue.Substring(0, hexValue.Length / 2);
                    hexValue2 = hexValue.Substring(hexValue.Length / 2);
                }
                else if (hexValue.Length == 3)
                {
                    hexValue1 = hexValue.Substring(0, 1);
                    hexValue2 = hexValue.Substring(1);
                }
                else if ((hexValue.Length is 2) || (hexValue.Length is 1))
                {
                    hexValue1 = "0";
                    hexValue2 = hexValue;
                }
                byte[] newbytesWithoutChecksum = { 0xF2, 0x00, 0x32, Convert.ToByte(hexValue1, 16), Convert.ToByte(hexValue2, 16), 0x0, 0x0 };

                int newsum = 0;
                int newarrayLength = newbytesWithoutChecksum.Length;
                for (int i = 0; i < newarrayLength; i++)
                {
                    newsum += newbytesWithoutChecksum[i];
                }

                string newhexSum = newsum.ToString("X");
                string newcs1 = "";
                string newcs2 = "";
                if (hexSum.Length == 4)
                {
                    newcs1 = newhexSum.Substring(0, newhexSum.Length / 2);
                    newcs2 = newhexSum.Substring(newhexSum.Length / 2);
                }
                else if (newhexSum.Length == 3)
                {
                    newcs1 = newhexSum.Substring(0, 1);
                    newcs2 = newhexSum.Substring(1);
                }
                else if ((newhexSum.Length is 2) || (newhexSum.Length is 1))
                {
                    newcs1 = "0";
                    newcs2 = newhexSum;
                }

                if (newcs1 != "")
                {


                    newbytesWithoutChecksum[newarrayLength - 2] = Convert.ToByte(newcs1, 16);
                    newbytesWithoutChecksum[newarrayLength - 1] = Convert.ToByte(newcs2, 16);
                }

                List<byte> newResponseTelegram;
                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    // write to the USB port
                    port.Write(newbytesWithoutChecksum, 0, newbytesWithoutChecksum.Length);
                    Thread.Sleep(500);

                    newResponseTelegram = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            //Console.WriteLine(t);
                            newResponseTelegram.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);
                }
                if (newResponseTelegram[3] == 0)
                {
                    Console.WriteLine("New voltage was set");
                }
                else
                {
                    Console.WriteLine(newResponseTelegram[3].ToString());
                }


                // reading serial number
                List<byte> Serialresponse;
                // Remember the dataframe setup, SD, DN,   OBJ, DATA checksum1, checksum2
                // OBJ = 0x01 = 1
                byte[] serialBytesToSend = { 0x7F, 0x00, 0x01, 0x00, 0x80 };
                using (SerialPort port = new SerialPort(this.com_type, 115200, 0, 8, StopBits.One))
                {
                    Thread.Sleep(500);
                    port.Open();
                    // write to the USB port
                    port.Write(serialBytesToSend, 0, serialBytesToSend.Length);
                    Thread.Sleep(500);

                    Serialresponse = new List<byte>();
                    int length = port.BytesToRead;
                    if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                        {
                            //Console.WriteLine(t);
                            Serialresponse.Add(t);
                        }
                    }
                    port.Close();
                    Thread.Sleep(500);

                    string binary = Convert.ToString(Serialresponse[0], 2);
                    string payloadLengtBinaryString = binary.Substring(4);
                    int payloadLength = Convert.ToInt32(payloadLengtBinaryString, 2);

                    string serialNumberString = "";

                    if (Serialresponse[2] == 1) // means that I got a response on obj, which is refers to the object list.
                    {
                        for (var i = 0; i < payloadLength; i++)
                        {
                            serialNumberString += Convert.ToChar(Serialresponse[3 + i]);
                        }
                    }

                    _form.AddStaticLabelOnStart(string.Format("serialNumberString:{0}", serialNumberString));

                }
            }
        }
    }
}