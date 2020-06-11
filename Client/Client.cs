using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
  class Client
  {
    private String serverHost;

    private Socket clientSocket;

    private const int PORT = 8034;

    private enum Commands
    {
      doNothing,
      getInverseMatrix,
      showClientMessage
    }

    private Commands GetCommand(string data, ref string commandParams)
    {

      if (data.StartsWith("$"))
      {
        string[] messageText = data.Split('#');

        string command = messageText[0];

        if (command.Equals("$getInverseMatrix"))
        {

          try
          {
            commandParams = messageText[1];
          }

          catch
          {
            Console.WriteLine("Command pattern is $<command_name>#<command_params>");
          }

          return Commands.getInverseMatrix;
        }

      }

      return Commands.showClientMessage;

    }

    public Client(String serverHost)
    {
      try
      {

        this.serverHost = serverHost;

        Console.WriteLine($"Connected with {serverHost}");

        clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        clientSocket.Connect(serverHost, PORT);

        new Thread(() =>
        {
          Communicate();
        }
        ).Start();

        while (true)
        {
          String userData = "";

          userData = Console.ReadLine();

          SendData(userData);
        }

      }

      catch (Exception e)
      {
        Console.WriteLine("Something gone wrong.");
      }
    }

    public void SendData(String data)
    {
      if (clientSocket != null)
      {

        try
        {

          if (data.Trim().Equals(""))
          {
            return;
          }

          var bytesBuffer = Encoding.UTF8.GetBytes(data);

          clientSocket.Send(bytesBuffer);

          Console.WriteLine("Message to the server was sent!");
        }

        catch (Exception e)
        {
          Console.WriteLine("Could not send a message.");
        }

      }
    }

    void Communicate()
    {
      if (clientSocket != null)
      {
        Console.WriteLine("Communicate with server was started");

        while (true)
        {
          String data = ReceiveData();

          String commandParams = "";

          Commands command = GetCommand(data, ref commandParams);

          if (command == Commands.getInverseMatrix)
          {
            commandParams = commandParams.Remove(commandParams.Length - 1);

            var matrix = Matrix.MyMatrix.getMatrixFromRawString(commandParams, ' ');

            Console.WriteLine("The Inverse Matrix:");
            Console.WriteLine(Matrix.MyMatrix.MatrixAsString(matrix));
          }

          else
          {
            Console.WriteLine($"Server sent: {data}");
          }

        }
      }
    }

    String ReceiveData()
    {
      var result = new StringBuilder();

      if (clientSocket != null)
      {

        try
        {
          var bytesBuffer = new byte[256];

          var dataSize = 0;

          do
          {
            dataSize = clientSocket.Receive(bytesBuffer);

            result.Append(Encoding.UTF8.GetString(bytesBuffer, 0, dataSize));
          }
          while (clientSocket.Available > 0);

          Console.WriteLine("Data from the server was got.");
        }

        catch (Exception e)
        {
          Console.WriteLine("Could not get data from the server.");
        }

      }

      return result.ToString();
    }

  }
}