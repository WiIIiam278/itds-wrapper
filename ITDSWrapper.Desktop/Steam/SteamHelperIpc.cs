using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace ITDSWrapper.Desktop.Steam;

public class SteamHelperIpc
{
    private readonly NamedPipeClientStream _steamworksHelperPipe = new("SteamworksHelperPipe");
    private readonly NamedPipeServerStream _steamworksReturnPipe = new("SteamworksReturnPipe");

    public SteamHelperIpc()
    {
        _steamworksHelperPipe.Connect();
        _steamworksReturnPipe.WaitForConnection();
    }
    
    public void SendCommand(string command)
    {
        try
        {
            _steamworksHelperPipe.Write(
                Encoding.UTF8.GetBytes(command).ToList().Concat(new byte[512 - Encoding.UTF8.GetByteCount(command)])
                    .ToArray(), 0, 512);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public byte[] ReceiveResponse()
    {
        try
        {
            byte[] buffer = new byte[512];
            _steamworksReturnPipe.ReadExactly(buffer);
            return buffer;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return [];
        }
    }
}