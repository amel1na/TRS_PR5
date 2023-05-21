using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor_Amelina
{
    internal class Program
    {
        static object _bufferLock = new object(); // Об'єкт для блокування доступу до буфера
        static int _bufferSize; // Розмір буфера
        static int[] _buffer; // Буфер
        public static int _itemsToConsume; // Кількість елементів, які споживач має зчитати

        static void Main(string[] args)
        {
            _bufferSize = 5;
            _itemsToConsume = 10;

            _buffer = new int[_bufferSize];

            Producer[] producers = new Producer[3]; // Створюємо 3 виробника
            Consumer consumer = new Consumer(); // Створюємо споживач

            for (int i = 0; i < producers.Length; i++)
            {
                producers[i] = new Producer(i + 1); // Встановлюємо індекс виробника
                Thread producerThread = new Thread(producers[i].Produce);
                producerThread.Start();
            }

            Thread consumerThread = new Thread(consumer.Consume);
            consumerThread.Start();
        }
        class Producer
        {
            private int _id;

            public Producer(int id)
            {
                _id = id;
            }

            public void Produce()
            {
                for (int i = 0; i < _itemsToConsume; i++)
                {
                    lock (_bufferLock)
                    {
                        while (IsBufferFull())
                        {
                            Monitor.Wait(_bufferLock); // Чекаємо, коли буфер стане неповним
                        }

                        int item = i + 1;
                        AddToBuffer(item);
                        Console.WriteLine($"Producer {_id} produced item {item}");

                        Monitor.Pulse(_bufferLock); // Сповіщуємо споживача про наявність нового елементу
                    }
                }
            }

            private bool IsBufferFull()
            {
                return _buffer[_bufferSize - 1] != 0;
            }

            private void AddToBuffer(int item)
            {
                for (int i = 0; i < _bufferSize; i++)
                {
                    if (_buffer[i] == 0)
                    {
                        _buffer[i] = item;
                        break;
                    }
                }
            }
        }
        class Consumer
        {
            public void Consume()
            {
                for (int i = 0; i < _itemsToConsume; i++)
                {
                    lock (_bufferLock)
                    {
                        while (IsBufferEmpty())
                        {
                            Monitor.Wait(_bufferLock); // Чекаємо, коли буфер стане непорожнім
                        }

                        int item = RemoveFromBuffer();
                        Console.WriteLine($"Consumer consumed item {item}");

                        Monitor.Pulse(_bufferLock); // Сповіщуємо виробників про наявність вільного місця в буфері
                    }
                }
            }

            private bool IsBufferEmpty()
            {
                return _buffer[0] == 0;
            }

            private int RemoveFromBuffer()
            {
                int item = _buffer[0];

                for (int i = 0; i < _bufferSize - 1; i++)
                {
                    _buffer[i] = _buffer[i + 1];
                }

                _buffer[_bufferSize - 1] = 0;

                return item;
            }
        }
    }
    
}
