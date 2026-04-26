using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters p = CommandParameters as ModbusWriteCommandParameters;
            byte[] request = new byte[12];

            request[0] = (byte)(p.TransactionId >> 8);
            request[1] = (byte)(p.TransactionId);
            request[2] = (byte)(p.ProtocolId >> 8);
            request[3] = (byte)(p.ProtocolId);
            request[4] = (byte)(p.Length >> 8);
            request[5] = (byte)(p.Length);
            request[6] = p.UnitId;
            request[7] = p.FunctionCode;
            request[8] = (byte)(p.OutputAddress >> 8);
            request[9] = (byte)(p.OutputAddress);

            if (p.Value == 1)
            {
                request[10] = 0xFF;
                request[11] = 0x00;
            }
            else
            {
                request[10] = 0x00;
                request[11] = 0x00;
            }

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters p = CommandParameters as ModbusWriteCommandParameters;
            var result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == p.FunctionCode + 0x80)
                HandeException(response[8]);

            ushort value = (ushort)(response[10] == 0xFF ? 1 : 0);
            result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, p.OutputAddress), value);
            return result;
        }
    }
}