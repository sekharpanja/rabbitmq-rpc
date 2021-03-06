﻿using System;
using rabbitmq.library;

namespace rabbitmq_producer
{
    class ProducerConsole
    {
        public static string HostName = "localhost";
        public static string ExchangeName = "rpcExchange";

        private static Producer _producer;

        static void Main(string[] args)
        {
            //create the producer
            _producer = new Producer(HostName, ExchangeName);
            Console.WriteLine("Enter message to send:");

            while (true)
            {
                string line = Console.ReadLine(); // Get string from user
                if (line == "exit") // Check string
                {
                    break;
                }
                byte[] responseBytes = _producer.Get(System.Text.Encoding.UTF8.GetBytes(line), "rpc_test");
                if (responseBytes != null)
                    Console.WriteLine(System.Text.Encoding.UTF8.GetString(responseBytes));
            }
        }

    }
}
