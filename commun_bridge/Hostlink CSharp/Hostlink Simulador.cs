using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using EasyModbus;

namespace Hostlink_CSharp
{
    public partial class HostlinkSimulador : Form
    {
        ModbusClient modbusClient;
        bool status=false;

        Thread mythread;

        public HostlinkSimulador()
        {
            InitializeComponent();
        }
        string esquerda_PC = "@00WD0100123456*\r";
        #region ler

        private void Add1_ler_Click(object sender, EventArgs e)
        {
            try
            {
                ler_1.Text = "@" + Convert.ToInt32(node_ler.Text).ToString("X2") + "RD";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Add2_ler_Click(object sender, EventArgs e)
        {
            try
            {
                ler_2.Text = ler_1.Text + Convert.ToInt32(DMini_ler.Text).ToString("D4");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Add3_ler_Click(object sender, EventArgs e)
        {
            try
            {
                ler_3.Text = ler_2.Text + Convert.ToInt32(NWords_ler.Text).ToString("D4");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Calcularfcs_ler_Click(object sender, EventArgs e)
        {
            try
            {
                int num = ler_3.Text.Length;
                char[] tramaAux = new char[50];
                char[] TramaAux2 = new char[50];

                for (int i = 0; i < num; i++)
                    tramaAux[i] = ler_3.Text[i];

                tramaAux[num] = '\0';

                int a = (int)tramaAux.Length;
                char b = tramaAux[0];
                char c = tramaAux[1];
                char d = (char)(b ^ c);
                for (int i = 2; i < a; i++)
                {
                    d = (char)(d ^ tramaAux[i]);
                }
                ler_4.Text = ((int)d).ToString("X0"); //fcs

                ler_5.Text = ler_3.Text + ler_4.Text + "*" + "\r";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void enviar_ler_Click(object sender, EventArgs e)
        {
            try
            {
                char[] resposta = new char[50];

                serialPort1.PortName = COM_ler.Text;
                serialPort1.Open();

                serialPort1.Write(ler_5.Text);
                Thread.Sleep(500); //1s
                ler_6.Text = serialPort1.ReadExisting();

                serialPort1.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region escrever

        private void Add1_escrever_Click(object sender, EventArgs e)
        {
            try
            {
                escrever_1.Text = "@" + Convert.ToInt32(node_escrever.Text).ToString("X2") + "WD";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Add2_escrever_Click(object sender, EventArgs e)
        {
            try
            {
                escrever_2.Text = escrever_1.Text + Convert.ToInt32(DMini_escrever.Text).ToString("D4");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Add3_escrever_Click(object sender, EventArgs e)
        {
            try
            {
                escrever_3.Text = escrever_2.Text + Convert.ToInt64(Dados_escrever.Text).ToString("D4");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Calcularfcs_escrever_Click(object sender, EventArgs e)
        {
            try
            {
                int num = escrever_3.Text.Length;
                char[] tramaAux = new char[50];
                char[] TramaAux2 = new char[50];

                for (int i = 0; i < num; i++)
                    tramaAux[i] = escrever_3.Text[i];

                tramaAux[num] = '\0';

                int a = (int)tramaAux.Length;
                char b = tramaAux[0];
                char c = tramaAux[1];
                char d = (char)(b ^ c);
                for (int i = 2; i < a; i++)
                {
                    d = (char)(d ^ tramaAux[i]);
                }
                escrever_4.Text = ((int)d).ToString("X0"); //fcs

                escrever_5.Text = escrever_3.Text + escrever_4.Text + "*" + "\r";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void enviar_escrever_Click(object sender, EventArgs e)
        {
            try
            {
                char[] resposta = new char[50];

                serialPort1.PortName = COM_escrever.Text;
                serialPort1.Open();

                serialPort1.Write(escrever_5.Text);
                Thread.Sleep(500); //40ms
                escrever_6.Text = serialPort1.ReadExisting();

                serialPort1.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        static int BoolArrayToInt(bool[] arr)
        {
            if (arr.Length > 31)
                throw new ApplicationException("too many elements to be converted to a single int");
            int val = 0;
            for (int i = 0; i < arr.Length; ++i)
                if (arr[i]) val |= 1 << i;
            return val; 
        }


        //thead
        void CommunThread()
        {
            bool[] bit = new bool[31];

            while (status)
            {
                ///-----------------------------------READ FACTORY DIGITAL SENSORS------------------------------------------
                //lê por Modbus as inputs (sensores) de 0 15, este valor deve ser ajustado ao número de sensores utilizados
                bool[] readDiscretInput = modbusClient.ReadDiscreteInputs(0, 15);

                //cria a trama para guardar na memoria do PLC os dados dos sensores (D0001) - por hostlink
                int Convert_bool2int = BoolArrayToInt(readDiscretInput); // COnverte de array de bool para um número inteiro
                string Convert_int2StringHex = Convert_bool2int.ToString("X4"); // converte para string formato HEXADECIMAL

                string trama = "@00WD1000" + Convert_int2StringHex;

                int num = trama.Length;
                char[] tramaAux = new char[50];
                char[] TramaAux2 = new char[50];

                for (int i = 0; i < trama.Length; i++)
                    tramaAux[i] = trama[i];

                tramaAux[trama.Length] = '\0';

                int a = (int)tramaAux.Length;
                char b = tramaAux[0];
                char c = tramaAux[1];
                char d = (char)(b ^ c);
                for (int i = 2; i < a; i++)
                {
                    d = (char)(d ^ tramaAux[i]);
                }
                trama = trama + ((int)d).ToString("X0") + "*" + "\r";

                //envia a trama criada por hostlink
                serialPort1.Write(trama);

                //aguarda a confirmação do PLC de trama recebida
                do
                {
                    Thread.Sleep(100); //40ms
                    trama = serialPort1.ReadExisting();
                } while (trama == "");
                serialPort1.DiscardInBuffer();

                //lê por Modbus as inputs (sensores) de 0 15, este valor deve ser ajustado ao número de sensores utilizados
                readDiscretInput = modbusClient.ReadDiscreteInputs(16, 31);

                //cria a trama para guardar na memoria do PLC os dados dos sensores (D0001) - por hostlink
                Convert_bool2int = BoolArrayToInt(readDiscretInput); // COnverte de array de bool para um número inteiro
                Convert_int2StringHex = Convert_bool2int.ToString("X4"); // converte para string formato HEXADECIMAL

                trama = "@00WD1001" + Convert_int2StringHex;

                num = trama.Length;
                tramaAux = new char[50];
                TramaAux2 = new char[50];

                for (int i = 0; i < trama.Length; i++)
                    tramaAux[i] = trama[i];

                tramaAux[trama.Length] = '\0';

                a = (int)tramaAux.Length;
                b = tramaAux[0];
                c = tramaAux[1];
                d = (char)(b ^ c);
                for (int i = 2; i < a; i++)
                {
                    d = (char)(d ^ tramaAux[i]);
                }
                trama = trama + ((int)d).ToString("X0") + "*" + "\r";

                //envia a trama criada por hostlink
                serialPort1.Write(trama);

                //aguarda a confirmação do PLC de trama recebida
                do
                {
                    Thread.Sleep(100); //40ms
                    trama = serialPort1.ReadExisting();
                } while (trama == "");
                serialPort1.DiscardInBuffer();

                ///-----------------------------------READ FACTORY ANALOG SENSORS AND WRITE IN PLC MEMORY--------------------------------------------
                // ################################################################################################# Sensor analogico 1
                int[] readInputRegisters = modbusClient.ReadInputRegisters(0, 1);
                //escreve esse valor na posição de memoria D0000
                Convert_int2StringHex = readInputRegisters[0].ToString("X4"); // converte para string formato HEXADECIMAL
                trama = "@00WD0000" + Convert_int2StringHex;

                num = trama.Length;


                for (int i = 0; i < trama.Length; i++)
                    tramaAux[i] = trama[i];

                tramaAux[trama.Length] = '\0';

                a = (int)tramaAux.Length;
                b = tramaAux[0];
                c = tramaAux[1];
                d = (char)(b ^ c);
                for (int i = 2; i < a; i++)
                {
                    d = (char)(d ^ tramaAux[i]);
                }
                trama = trama + ((int)d).ToString("X0") + "*" + "\r";

                //envia a trama criada por hostlink
                serialPort1.Write(trama);

                //aguarda a confirmação do PLC de trama recebida
                do
                {
                    Thread.Sleep(100); //40ms
                    trama = serialPort1.ReadExisting();
                } while (trama == "");
                serialPort1.DiscardInBuffer();

                // ################################################################################################# Sensor analogico 2
                readInputRegisters = modbusClient.ReadInputRegisters(1, 2);
                //escreve esse valor na posição de memoria D0000
                Convert_int2StringHex = readInputRegisters[0].ToString("X4"); // converte para string formato HEXADECIMAL
                trama = "@00WD0001" + Convert_int2StringHex;

                num = trama.Length;


                for (int i = 0; i < trama.Length; i++)
                    tramaAux[i] = trama[i];

                tramaAux[trama.Length] = '\0';

                a = (int)tramaAux.Length;
                b = tramaAux[0];
                c = tramaAux[1];
                d = (char)(b ^ c);
                for (int i = 2; i < a; i++)
                {
                    d = (char)(d ^ tramaAux[i]);
                }
                trama = trama + ((int)d).ToString("X0") + "*" + "\r";

                //envia a trama criada por hostlink
                serialPort1.Write(trama);

                //aguarda a confirmação do PLC de trama recebida
                do
                {
                    Thread.Sleep(100); //40ms
                    trama = serialPort1.ReadExisting();
                } while (trama == "");
                serialPort1.DiscardInBuffer();

                // ################################################################################################# Sensor analogico 3
                readInputRegisters = modbusClient.ReadInputRegisters(2, 3);
                //escreve esse valor na posição de memoria D0000
                Convert_int2StringHex = readInputRegisters[0].ToString("X4"); // converte para string formato HEXADECIMAL
                trama = "@00WD0002" + Convert_int2StringHex;

                num = trama.Length;


                for (int i = 0; i < trama.Length; i++)
                    tramaAux[i] = trama[i];

                tramaAux[trama.Length] = '\0';

                a = (int)tramaAux.Length;
                b = tramaAux[0];
                c = tramaAux[1];
                d = (char)(b ^ c);
                for (int i = 2; i < a; i++)
                {
                    d = (char)(d ^ tramaAux[i]);
                }
                trama = trama + ((int)d).ToString("X0") + "*" + "\r";

                //envia a trama criada por hostlink
                serialPort1.Write(trama);

                //aguarda a confirmação do PLC de trama recebida
                do
                {
                    Thread.Sleep(100); //40ms
                    trama = serialPort1.ReadExisting();
                } while (trama == "");
                serialPort1.DiscardInBuffer();

                ///----------------------------------------------------------------------------------------------------------



                ///-----------------------------------READ PLC MEMORY AND WRITE IN FACTORY DIGITAL COILS--------------------------------------------
                //vai ler a memória D2000
                serialPort1.Write("@00RD2000000256*\r");
                do
                {
                    Thread.Sleep(100); //40ms
                    trama = serialPort1.ReadExisting();
                } while (trama == "");
                serialPort1.DiscardInBuffer();

                if (trama[0] == '@' && trama.Length > 14)
                {
                    //Descodifica de D2000.0 as D2000.15
                    bit[0] = Convert.ToBoolean(Convert.ToByte(trama[10]) & 0x1);
                    bit[1] = Convert.ToBoolean(Convert.ToByte(trama[10]) & 0x2);
                    bit[2] = Convert.ToBoolean(Convert.ToByte(trama[10]) & 0x4);
                    bit[3] = Convert.ToBoolean(Convert.ToByte(trama[10]) & 0x8);
                    bit[4] = Convert.ToBoolean(Convert.ToByte(trama[9]) & 0x1);
                    bit[5] = Convert.ToBoolean(Convert.ToByte(trama[9]) & 0x2);
                    bit[6] = Convert.ToBoolean(Convert.ToByte(trama[9]) & 0x4);
                    bit[7] = Convert.ToBoolean(Convert.ToByte(trama[9]) & 0x8);
                    bit[8] = Convert.ToBoolean(Convert.ToByte(trama[8]) & 0x1);
                    bit[9] = Convert.ToBoolean(Convert.ToByte(trama[8]) & 0x2);
                    bit[10] = Convert.ToBoolean(Convert.ToByte(trama[8]) & 0x4);
                    bit[11] = Convert.ToBoolean(Convert.ToByte(trama[8]) & 0x8);
                    bit[12] = Convert.ToBoolean(Convert.ToByte(trama[7]) & 0x1);
                    bit[13] = Convert.ToBoolean(Convert.ToByte(trama[7]) & 0x2);
                    bit[14] = Convert.ToBoolean(Convert.ToByte(trama[7]) & 0x4);
                    bit[15] = Convert.ToBoolean(Convert.ToByte(trama[7]) & 0x8);

                    //Descodifica de D2001.0 as D2001.15
                    bit[16] = Convert.ToBoolean(Convert.ToByte(trama[14]) & 0x1);
                    bit[17] = Convert.ToBoolean(Convert.ToByte(trama[14]) & 0x2);
                    bit[18] = Convert.ToBoolean(Convert.ToByte(trama[14]) & 0x4);
                    bit[19] = Convert.ToBoolean(Convert.ToByte(trama[14]) & 0x8);
                    bit[20] = Convert.ToBoolean(Convert.ToByte(trama[13]) & 0x1);
                    bit[21] = Convert.ToBoolean(Convert.ToByte(trama[13]) & 0x2);
                    bit[22] = Convert.ToBoolean(Convert.ToByte(trama[13]) & 0x4);
                    bit[23] = Convert.ToBoolean(Convert.ToByte(trama[13]) & 0x8);
                    bit[24] = Convert.ToBoolean(Convert.ToByte(trama[12]) & 0x1);
                    bit[25] = Convert.ToBoolean(Convert.ToByte(trama[12]) & 0x2);
                    bit[26] = Convert.ToBoolean(Convert.ToByte(trama[12]) & 0x4);
                    bit[27] = Convert.ToBoolean(Convert.ToByte(trama[12]) & 0x8);
                    bit[28] = Convert.ToBoolean(Convert.ToByte(trama[11]) & 0x1);
                    bit[29] = Convert.ToBoolean(Convert.ToByte(trama[11]) & 0x2);
                    bit[30] = Convert.ToBoolean(Convert.ToByte(trama[11]) & 0x4);

                    //manda o valor de D0.0 a D0.15 para as coils 0 a 15
                    modbusClient.WriteMultipleCoils(0, bit);// new bool[] { D0[0], D0[1], D0[2], D0[3], D0[4], D0[5], D0[6], D0[7], D0[8], D0[9], D0[10], D0[11], D0[12], D0[13], D0[14], D0[15] });    //Write Coils starting with Address 0
                                                                                                                                                                                                ///----------------------------------------------------------------------------------------------------------                                                                                                                                                                            ///----------------------------------------------------------------------------------------------------------
                }

                ///-----------------------------------READ PLC MEMORY AND WRITE IN FACTORY ANALOG HOLDING REGISTERS--------------------------------------------
                serialPort1.Write("@00RD0100000750*\r");
                do
                {
                    Thread.Sleep(300); //40ms
                    trama = serialPort1.ReadExisting();
                } while (trama == "");
                serialPort1.DiscardInBuffer();
                if (trama[0] == '@' && trama.Length > 14)
                {
                    int[] ArrayInteiros = new int[7];

                    //D100.0 a D100.15
                    bit[0] = Convert.ToBoolean(Convert.ToByte(trama[10]) & 0x1);
                    bit[1] = Convert.ToBoolean(Convert.ToByte(trama[10]) & 0x2);
                    bit[2] = Convert.ToBoolean(Convert.ToByte(trama[10]) & 0x4);
                    bit[3] = Convert.ToBoolean(Convert.ToByte(trama[10]) & 0x8);
                    bit[4] = Convert.ToBoolean(Convert.ToByte(trama[9]) & 0x1);
                    bit[5] = Convert.ToBoolean(Convert.ToByte(trama[9]) & 0x2);
                    bit[6] = Convert.ToBoolean(Convert.ToByte(trama[9]) & 0x4);
                    bit[7] = Convert.ToBoolean(Convert.ToByte(trama[9]) & 0x8);
                    bit[8] = Convert.ToBoolean(Convert.ToByte(trama[8]) & 0x1);
                    bit[9] = Convert.ToBoolean(Convert.ToByte(trama[8]) & 0x2);
                    bit[10] = Convert.ToBoolean(Convert.ToByte(trama[8]) & 0x4);
                    bit[11] = Convert.ToBoolean(Convert.ToByte(trama[8]) & 0x8);
                    bit[12] = Convert.ToBoolean(Convert.ToByte(trama[7]) & 0x1);
                    bit[13] = Convert.ToBoolean(Convert.ToByte(trama[7]) & 0x2);
                    bit[14] = Convert.ToBoolean(Convert.ToByte(trama[7]) & 0x4);
                    bit[15] = Convert.ToBoolean(Convert.ToByte(trama[7]) & 0x8);

                    ArrayInteiros[0] = BoolArrayToInt(bit);

                    //D101.0 a D101.15
                    bit[0] = Convert.ToBoolean(Convert.ToByte(trama[14]) & 0x1);
                    bit[1] = Convert.ToBoolean(Convert.ToByte(trama[14]) & 0x2);
                    bit[2] = Convert.ToBoolean(Convert.ToByte(trama[14]) & 0x4);
                    bit[3] = Convert.ToBoolean(Convert.ToByte(trama[14]) & 0x8);
                    bit[4] = Convert.ToBoolean(Convert.ToByte(trama[13]) & 0x1);
                    bit[5] = Convert.ToBoolean(Convert.ToByte(trama[13]) & 0x2);
                    bit[6] = Convert.ToBoolean(Convert.ToByte(trama[13]) & 0x4);
                    bit[7] = Convert.ToBoolean(Convert.ToByte(trama[13]) & 0x8);
                    bit[8] = Convert.ToBoolean(Convert.ToByte(trama[12]) & 0x1);
                    bit[9] = Convert.ToBoolean(Convert.ToByte(trama[12]) & 0x2);
                    bit[10] = Convert.ToBoolean(Convert.ToByte(trama[12]) & 0x4);
                    bit[11] = Convert.ToBoolean(Convert.ToByte(trama[12]) & 0x8);
                    bit[12] = Convert.ToBoolean(Convert.ToByte(trama[11]) & 0x1);
                    bit[13] = Convert.ToBoolean(Convert.ToByte(trama[11]) & 0x2);
                    bit[14] = Convert.ToBoolean(Convert.ToByte(trama[11]) & 0x4);
                    bit[15] = Convert.ToBoolean(Convert.ToByte(trama[11]) & 0x8);

                    ArrayInteiros[1] = BoolArrayToInt(bit);

                    //D102.0 a D102.15
                    bit[0] = Convert.ToBoolean(Convert.ToByte(trama[18]) & 0x1);
                    bit[1] = Convert.ToBoolean(Convert.ToByte(trama[18]) & 0x2);
                    bit[2] = Convert.ToBoolean(Convert.ToByte(trama[18]) & 0x4);
                    bit[3] = Convert.ToBoolean(Convert.ToByte(trama[18]) & 0x8);
                    bit[4] = Convert.ToBoolean(Convert.ToByte(trama[17]) & 0x1);
                    bit[5] = Convert.ToBoolean(Convert.ToByte(trama[17]) & 0x2);
                    bit[6] = Convert.ToBoolean(Convert.ToByte(trama[17]) & 0x4);
                    bit[7] = Convert.ToBoolean(Convert.ToByte(trama[17]) & 0x8);
                    bit[8] = Convert.ToBoolean(Convert.ToByte(trama[16]) & 0x1);
                    bit[9] = Convert.ToBoolean(Convert.ToByte(trama[16]) & 0x2);
                    bit[10] = Convert.ToBoolean(Convert.ToByte(trama[16]) & 0x4);
                    bit[11] = Convert.ToBoolean(Convert.ToByte(trama[16]) & 0x8);
                    bit[12] = Convert.ToBoolean(Convert.ToByte(trama[15]) & 0x1);
                    bit[13] = Convert.ToBoolean(Convert.ToByte(trama[15]) & 0x2);
                    bit[14] = Convert.ToBoolean(Convert.ToByte(trama[15]) & 0x4);
                    bit[15] = Convert.ToBoolean(Convert.ToByte(trama[15]) & 0x8);

                    ArrayInteiros[2] = BoolArrayToInt(bit);

                    //D103.0 a D103.15
                    bit[0] = Convert.ToBoolean(Convert.ToByte(trama[22]) & 0x1);
                    bit[1] = Convert.ToBoolean(Convert.ToByte(trama[22]) & 0x2);
                    bit[2] = Convert.ToBoolean(Convert.ToByte(trama[22]) & 0x4);
                    bit[3] = Convert.ToBoolean(Convert.ToByte(trama[22]) & 0x8);
                    bit[4] = Convert.ToBoolean(Convert.ToByte(trama[21]) & 0x1);
                    bit[5] = Convert.ToBoolean(Convert.ToByte(trama[21]) & 0x2);
                    bit[6] = Convert.ToBoolean(Convert.ToByte(trama[21]) & 0x4);
                    bit[7] = Convert.ToBoolean(Convert.ToByte(trama[21]) & 0x8);
                    bit[8] = Convert.ToBoolean(Convert.ToByte(trama[20]) & 0x1);
                    bit[9] = Convert.ToBoolean(Convert.ToByte(trama[20]) & 0x2);
                    bit[10] = Convert.ToBoolean(Convert.ToByte(trama[20]) & 0x4);
                    bit[11] = Convert.ToBoolean(Convert.ToByte(trama[20]) & 0x8);
                    bit[12] = Convert.ToBoolean(Convert.ToByte(trama[19]) & 0x1);
                    bit[13] = Convert.ToBoolean(Convert.ToByte(trama[19]) & 0x2);
                    bit[14] = Convert.ToBoolean(Convert.ToByte(trama[19]) & 0x4);
                    bit[15] = Convert.ToBoolean(Convert.ToByte(trama[19]) & 0x8);

                    ArrayInteiros[3] = BoolArrayToInt(bit);

                    //D104.0 a D104.15
                    bit[0] = Convert.ToBoolean(Convert.ToByte(trama[26]) & 0x1);
                    bit[1] = Convert.ToBoolean(Convert.ToByte(trama[26]) & 0x2);
                    bit[2] = Convert.ToBoolean(Convert.ToByte(trama[26]) & 0x4);
                    bit[3] = Convert.ToBoolean(Convert.ToByte(trama[26]) & 0x8);
                    bit[4] = Convert.ToBoolean(Convert.ToByte(trama[25]) & 0x1);
                    bit[5] = Convert.ToBoolean(Convert.ToByte(trama[25]) & 0x2);
                    bit[6] = Convert.ToBoolean(Convert.ToByte(trama[25]) & 0x4);
                    bit[7] = Convert.ToBoolean(Convert.ToByte(trama[25]) & 0x8);
                    bit[8] = Convert.ToBoolean(Convert.ToByte(trama[24]) & 0x1);
                    bit[9] = Convert.ToBoolean(Convert.ToByte(trama[24]) & 0x2);
                    bit[10] = Convert.ToBoolean(Convert.ToByte(trama[24]) & 0x4);
                    bit[11] = Convert.ToBoolean(Convert.ToByte(trama[24]) & 0x8);
                    bit[12] = Convert.ToBoolean(Convert.ToByte(trama[23]) & 0x1);
                    bit[13] = Convert.ToBoolean(Convert.ToByte(trama[23]) & 0x2);
                    bit[14] = Convert.ToBoolean(Convert.ToByte(trama[23]) & 0x4);
                    bit[15] = Convert.ToBoolean(Convert.ToByte(trama[23]) & 0x8);

                    ArrayInteiros[4] = BoolArrayToInt(bit);

                    //D105.0 a D105.15
                    bit[0] = Convert.ToBoolean(Convert.ToByte(trama[30]) & 0x1);
                    bit[1] = Convert.ToBoolean(Convert.ToByte(trama[30]) & 0x2);
                    bit[2] = Convert.ToBoolean(Convert.ToByte(trama[30]) & 0x4);
                    bit[3] = Convert.ToBoolean(Convert.ToByte(trama[30]) & 0x8);
                    bit[4] = Convert.ToBoolean(Convert.ToByte(trama[29]) & 0x1);
                    bit[5] = Convert.ToBoolean(Convert.ToByte(trama[29]) & 0x2);
                    bit[6] = Convert.ToBoolean(Convert.ToByte(trama[29]) & 0x4);
                    bit[7] = Convert.ToBoolean(Convert.ToByte(trama[29]) & 0x8);
                    bit[8] = Convert.ToBoolean(Convert.ToByte(trama[28]) & 0x1);
                    bit[9] = Convert.ToBoolean(Convert.ToByte(trama[28]) & 0x2);
                    bit[10] = Convert.ToBoolean(Convert.ToByte(trama[28]) & 0x4);
                    bit[11] = Convert.ToBoolean(Convert.ToByte(trama[28]) & 0x8);
                    bit[12] = Convert.ToBoolean(Convert.ToByte(trama[27]) & 0x1);
                    bit[13] = Convert.ToBoolean(Convert.ToByte(trama[27]) & 0x2);
                    bit[14] = Convert.ToBoolean(Convert.ToByte(trama[27]) & 0x4);
                    bit[15] = Convert.ToBoolean(Convert.ToByte(trama[27]) & 0x8);

                    ArrayInteiros[5] = BoolArrayToInt(bit);

                    //D105.0 a D105.15
                    bit[0] = Convert.ToBoolean(Convert.ToByte(trama[34]) & 0x1);
                    bit[1] = Convert.ToBoolean(Convert.ToByte(trama[34]) & 0x2);
                    bit[2] = Convert.ToBoolean(Convert.ToByte(trama[34]) & 0x4);
                    bit[3] = Convert.ToBoolean(Convert.ToByte(trama[34]) & 0x8);
                    bit[4] = Convert.ToBoolean(Convert.ToByte(trama[33]) & 0x1);
                    bit[5] = Convert.ToBoolean(Convert.ToByte(trama[33]) & 0x2);
                    bit[6] = Convert.ToBoolean(Convert.ToByte(trama[33]) & 0x4);
                    bit[7] = Convert.ToBoolean(Convert.ToByte(trama[33]) & 0x8);
                    bit[8] = Convert.ToBoolean(Convert.ToByte(trama[32]) & 0x1);
                    bit[9] = Convert.ToBoolean(Convert.ToByte(trama[32]) & 0x2);
                    bit[10] = Convert.ToBoolean(Convert.ToByte(trama[32]) & 0x4);
                    bit[11] = Convert.ToBoolean(Convert.ToByte(trama[32]) & 0x8);
                    bit[12] = Convert.ToBoolean(Convert.ToByte(trama[31]) & 0x1);
                    bit[13] = Convert.ToBoolean(Convert.ToByte(trama[31]) & 0x2);
                    bit[14] = Convert.ToBoolean(Convert.ToByte(trama[31]) & 0x4);
                    bit[15] = Convert.ToBoolean(Convert.ToByte(trama[31]) & 0x8);

                    ArrayInteiros[6] = BoolArrayToInt(bit);

                    modbusClient.WriteMultipleRegisters(0, ArrayInteiros);
                }
            }
        }


        //MODBUS-<> HOSTLINK BRIDGE
        private void button1_Click(object sender, EventArgs e)
        {
            mythread = new Thread(CommunThread);
            mythread.Start();
            status = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //cria cliente para comunicação com servidor da Factory IO
            modbusClient = new ModbusClient("127.0.0.1", 502);    //Ip-Address and Port of Modbus-TCP-Server
            modbusClient.Connect();

            //criar ligação RS232 com o PLC
            serialPort1.Open();


        }

        private void button4_Click(object sender, EventArgs e)
        {
            modbusClient.Disconnect();

            serialPort1.Close();
        }
    }
}
